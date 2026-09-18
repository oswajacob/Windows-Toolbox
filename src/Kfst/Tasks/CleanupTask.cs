using System;
using System.IO;
using System.Threading.Tasks;
using Kfzt.Core;

namespace Kfzt.Tasks
{
    /// <summary>Resultado de una limpieza.</summary>
    public class ResultadoLimpieza
    {
        public long BytesLiberados { get; set; }
        public int ArchivosOmitidos { get; set; }

        public string BytesLiberadosLegible =>
            (BytesLiberados / (1024.0 * 1024)).ToString("F1") + " MB";
    }

    /// <summary>
    /// Limpia %temp% del usuario y la caché de descargas de Windows Update.
    /// NO toca la papelera — esa guarda cosas que el usuario puso ahí a
    /// propósito; temp y la caché de Update las regenera Windows solo.
    ///
    /// No pasa por TaskRunner porque no ejecuta ningún proceso externo
    /// (es borrado directo de archivos), pero sí respeta el mismo candado
    /// de "una tarea a la vez" vía TaskRunner.TryReservar().
    /// </summary>
    public static class CleanupTask
    {
        private static readonly string CarpetaTempUsuario = Path.GetTempPath();
        private static readonly string CarpetaCacheWindowsUpdate =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");

        public static async Task<ResultadoLimpieza> EjecutarAsync()
        {
            if (!TaskRunner.TryReservar())
                return null; // ya hay otra tarea corriendo, se niega igual que las demás

            try
            {
                return await Task.Run(() =>
                {
                    var resultado = new ResultadoLimpieza();
                    LimpiarCarpeta(CarpetaTempUsuario, resultado);
                    LimpiarCarpeta(CarpetaCacheWindowsUpdate, resultado);
                    return resultado;
                });
            }
            finally
            {
                TaskRunner.Liberar();
            }
        }

        /// <summary>
        /// Borra archivo por archivo. Si uno está bloqueado/en uso, lo
        /// salta y sigue — no se detiene toda la limpieza por uno solo.
        /// </summary>
        private static void LimpiarCarpeta(string ruta, ResultadoLimpieza resultado)
        {
            if (!Directory.Exists(ruta)) return;

            foreach (var archivo in Directory.EnumerateFiles(ruta, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var info = new FileInfo(archivo);
                    long tamaño = info.Length;
                    info.Delete();
                    resultado.BytesLiberados += tamaño;
                }
                catch
                {
                    resultado.ArchivosOmitidos++; // en uso o sin permiso — se salta
                }
            }
        }
    }
}