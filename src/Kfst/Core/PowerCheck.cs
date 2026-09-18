using System.Windows.Forms;

namespace Kfzt.Core
{
    /// <summary>
    /// Foto del estado de energía del equipo en el momento de la consulta.
    /// No es monitoreo en tiempo real — se toma justo antes de arrancar
    /// una tarea riesgosa como chkdsk.
    /// </summary>
    public class EstadoEnergia
    {
        /// <summary>Porcentaje de batería (0.0 a 1.0). En equipos de escritorio siempre es 1.0.</summary>
        public float PorcentajeBateria { get; set; }

        /// <summary>True si el equipo tiene batería física (es laptop). False = equipo de escritorio.</summary>
        public bool TieneBateria { get; set; }

        /// <summary>True si está conectado a corriente. Un equipo de escritorio siempre cuenta como "conectado".</summary>
        public bool EstaConectado { get; set; }

        /// <summary>
        /// True solo cuando de verdad hay riesgo: es laptop, corre con batería
        /// (no conectada) y el nivel está por debajo del 40%. Un equipo de
        /// escritorio jamás es "riesgoso" — no tiene de qué quedarse sin luz.
        /// </summary>
        public bool EsRiesgoso => TieneBateria && !EstaConectado && PorcentajeBateria < 0.40f;
    }

    /// <summary>
    /// Lee el estado de energía usando la info que Windows ya expone
    /// (no hace falta WMI para esto, System.Windows.Forms ya lo trae).
    /// </summary>
    public static class PowerCheck
    {
        public static EstadoEnergia ObtenerEstado()
        {
            var powerStatus = SystemInformation.PowerStatus;

            // Si el sistema reporta "NoSystemBattery", es un equipo de escritorio
            bool tieneBateria = powerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery;

            bool estaConectado = !tieneBateria
                || powerStatus.PowerLineStatus == PowerLineStatus.Online;

            return new EstadoEnergia
            {
                TieneBateria = tieneBateria,
                EstaConectado = estaConectado,
                PorcentajeBateria = tieneBateria ? powerStatus.BatteryLifePercent : 1.0f
            };
        }
    }
}