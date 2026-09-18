using System;
using Kfzt.Core;

namespace Kfzt.Tasks
{
    /// <summary>
    /// Envoltorio delgado sobre TaskRunner para correr DISM en modo
    /// de reparación (RestoreHealth) — el equivalente de "arregla la
    /// imagen de Windows", que es lo que la gente espera de un botón
    /// de "DISM" en un toolbox de recuperación.
    /// </summary>
    public class DismTask
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

        public bool Iniciar()
        {
            // /Online = actuar sobre la instalación de Windows activa (no una imagen offline)
            // /Cleanup-Image /RestoreHealth = reparar el almacén de componentes
            // usando Windows Update como fuente por defecto (necesita internet)
            return _runner.TryStart("dism.exe", "/Online /Cleanup-Image /RestoreHealth");
        }
    }
}