using System;
using Kfzt.Core;

namespace Kfzt.Tasks
{
    /// <summary>
    /// Dos botones independientes: chkdsk /f y chkdsk /r. Sin detección
    /// automática de tipo de disco — la decisión es del usuario, la GUI
    /// solo muestra la nota de "recomendado para HDD" junto al de /r.
    /// Por defecto apunta al disco del sistema (C:).
    /// </summary>
    public class ChkdskTask
    {
        private readonly TaskRunner _runner = new TaskRunner();

        public event Action<string> OnOutputLine
        {
            add => _runner.OnOutputLine += value;
            remove => _runner.OnOutputLine -= value;
        }

        public event Action OnStalled
        {
            add => _runner.OnStalled += value;
            remove => _runner.OnStalled -= value;
        }

        public event Action<TaskResult> OnFinished
        {
            add => _runner.OnFinished += value;
            remove => _runner.OnFinished -= value;
        }

        /// <summary>chkdsk /f — corrige errores del sistema de archivos.</summary>
        public bool IniciarF(string unidad = "C:")
        {
            // "S" = respuesta automática al prompt de "¿programar para el
            // próximo reinicio? S/N" en Windows en español. El usuario ya
            // confirmó esto en el diálogo de la app antes de llegar aquí.
            return _runner.TryStart("chkdsk.exe", $"{unidad} /f", "S");
        }

        /// <summary>chkdsk /r — además localiza sectores dañados y recupera lo legible.</summary>
        public bool IniciarR(string unidad = "C:")
        {
            return _runner.TryStart("chkdsk.exe", $"{unidad} /r", "S");
        }
    }
}