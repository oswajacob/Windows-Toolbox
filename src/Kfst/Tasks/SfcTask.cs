using System;
using Kfzt.Core;

namespace Kfzt.Tasks
{
    /// <summary>
    /// Envoltorio delgado sobre TaskRunner para correr "sfc /scannow".
    /// No tiene lógica propia de ejecución — solo define el comando
    /// y reexpone los eventos para que la ventana de progreso los escuche.
    /// </summary>
    public class SfcTask
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

        /// <summary>
        /// Arranca sfc /scannow. Devuelve false si ya hay otra tarea corriendo
        /// (regla de "una a la vez" heredada de TaskRunner).
        /// </summary>
        public bool Iniciar()
        {
            // sfc.exe vive en System32 y se puede llamar directo,
            // no hace falta pasar por cmd.exe
            return _runner.TryStart("sfc.exe", "/scannow");
        }
    }
}