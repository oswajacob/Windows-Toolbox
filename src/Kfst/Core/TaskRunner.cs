using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kfzt.Core
{
    /// <summary>
    /// Resultado final de una tarea al terminar de ejecutarse.
    /// </summary>
    public class TaskResult
    {
        public bool Exito { get; set; }
        public int CodigoSalida { get; set; }
        public string SalidaCompleta { get; set; }
        public string ErrorDetectado { get; set; } // ej. "Acceso denegado", null si no hubo
    }

    /// <summary>
    /// Motor central que ejecuta comandos del sistema (sfc, dism, chkdsk, etc.)
    /// sin mostrar consola, capturando su salida en tiempo real.
    /// </summary>
    public class TaskRunner
    {
        // ---- Estado compartido: solo una tarea corre a la vez en TODA la app ----
        private static readonly object _lock = new object();
        private static bool _hayTareaActiva = false;

        public static bool HayTareaActiva
        {
            get { lock (_lock) { return _hayTareaActiva; } }
        }

        // Cuánto tiempo sin output antes de disparar el aviso de "esto va lento"
        private static readonly TimeSpan TiempoDeAvisoPorInactividad = TimeSpan.FromMinutes(10);

        // ---- Eventos que la ventana de progreso puede escuchar ----
        public event Action<string> OnOutputLine;
        public event Action OnStalled;
        public event Action<TaskResult> OnFinished;

        /// <summary>
        /// Intenta iniciar un comando. Permite enviar una respuesta automática (ej. "S")
        /// para comandos interactivos como chkdsk.
        /// </summary>
        public bool TryStart(string archivo, string argumentos, string respuestaAutomatica = null)
        {
            lock (_lock)
            {
                if (_hayTareaActiva)
                    return false; // otra tarea ya está corriendo, se niega esta

                _hayTareaActiva = true;
            }

            _ = EjecutarAsync(archivo, argumentos, respuestaAutomatica); // fire-and-forget: corre en segundo plano
            return true;
        }

        private async Task EjecutarAsync(string archivo, string argumentos, string respuestaAutomatica)
        {
            var salidaCompleta = new StringBuilder();
            string errorDetectado = null;
            DateTime ultimoOutput = DateTime.Now;
            bool avisoYaDisparado = false;

            var psi = new ProcessStartInfo
            {
                FileName = archivo,
                Arguments = argumentos,
                UseShellExecute = false,       // necesario para poder redirigir la salida
                CreateNoWindow = true,         // esto es lo que evita que aparezca la consola
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,  // por si algún comando pide confirmación Y/N
                StandardOutputEncoding = Encoding.UTF8
            };

            using (var proceso = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                proceso.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null) return;
                    ultimoOutput = DateTime.Now;
                    salidaCompleta.AppendLine(e.Data);

                    if (e.Data.IndexOf("acceso denegado", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        e.Data.IndexOf("access is denied", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        errorDetectado = "Acceso denegado";
                    }

                    OnOutputLine?.Invoke(e.Data);
                };

                proceso.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null) return;
                    ultimoOutput = DateTime.Now;
                    salidaCompleta.AppendLine(e.Data);
                    OnOutputLine?.Invoke(e.Data);
                };

                proceso.Start();
                proceso.BeginOutputReadLine();
                proceso.BeginErrorReadLine();

                // Si nos dieron una respuesta automática (ej. "S" para confirmaciones), la enviamos
                if (!string.IsNullOrEmpty(respuestaAutomatica))
                {
                    proceso.StandardInput.WriteLine(respuestaAutomatica);
                    proceso.StandardInput.Flush();
                }

                // Vigilante de inactividad
                using (var vigilante = new CancellationTokenSource())
                {
                    var tareaVigilante = Task.Run(async () =>
                    {
                        while (!vigilante.IsCancellationRequested)
                        {
                            try
                            {
                                await Task.Delay(TimeSpan.FromSeconds(30), vigilante.Token);
                            }
                            catch (TaskCanceledException)
                            {
                                break;
                            }

                            if (!avisoYaDisparado &&
                                DateTime.Now - ultimoOutput > TiempoDeAvisoPorInactividad)
                            {
                                avisoYaDisparado = true;
                                OnStalled?.Invoke();
                            }
                        }
                    });

                    await Task.Run(() => proceso.WaitForExit());
                    vigilante.Cancel();
                }

                var resultado = new TaskResult
                {
                    Exito = proceso.ExitCode == 0 && errorDetectado == null,
                    CodigoSalida = proceso.ExitCode,
                    SalidaCompleta = salidaCompleta.ToString(),
                    ErrorDetectado = errorDetectado
                };

                lock (_lock) { _hayTareaActiva = false; }

                OnFinished?.Invoke(resultado);
            }
        }
    }
}