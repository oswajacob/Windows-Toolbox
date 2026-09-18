using System.Collections.Generic;
using System.Management;

namespace Kfzt.SystemInfo
{
    /// <summary>
    /// Info de una sola ranura de RAM.
    /// </summary>
    public class RanuraRam
    {
        public string Fabricante { get; set; }
        public string CapacidadGB { get; set; }
        public string VelocidadMHz { get; set; }
        public string Tipo { get; set; } // ej. "DDR4"
    }

    /// <summary>
    /// Snapshot de hardware para la sección Info (equivalente a msinfo32,
    /// más algunos datos que ni siquiera trae msinfo32 como ranuras de RAM).
    /// Todo se lee vía WMI — es de solo lectura, no hay ejecución de comandos.
    /// </summary>
    public static class HardwareReader
    {
        public static string ObtenerCPU()
        {
            using (var buscador = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in buscador.Get())
                    return obj["Name"]?.ToString()?.Trim();
            }
            return "No detectado";
        }

        public static List<RanuraRam> ObtenerRanurasRam()
        {
            var ranuras = new List<RanuraRam>();

            using (var buscador = new ManagementObjectSearcher("SELECT Manufacturer, Capacity, Speed, SMBIOSMemoryType FROM Win32_PhysicalMemory"))
            {
                foreach (ManagementObject obj in buscador.Get())
                {
                    long capacidadBytes = (ulong)obj["Capacity"] > 0 ? (long)(ulong)obj["Capacity"] : 0;

                    ranuras.Add(new RanuraRam
                    {
                        Fabricante = obj["Manufacturer"]?.ToString()?.Trim(),
                        CapacidadGB = (capacidadBytes / (1024 * 1024 * 1024)) + " GB",
                        VelocidadMHz = obj["Speed"]?.ToString() + " MHz",
                        Tipo = InterpretarTipoRam(obj["SMBIOSMemoryType"])
                    });
                }
            }

            return ranuras;
        }

        public static string ObtenerDiscoResumen()
        {
            var resumen = new List<string>();

            using (var buscador = new ManagementObjectSearcher("SELECT Model, Size FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject obj in buscador.Get())
                {
                    long tamañoBytes = obj["Size"] != null ? (long)(ulong)obj["Size"] : 0;
                    double tamañoGB = tamañoBytes / (1024.0 * 1024 * 1024);
                    resumen.Add($"{obj["Model"]} — {tamañoGB:F0} GB");
                }
            }

            return string.Join("\n", resumen);
        }

        /// <summary>
        /// SMBIOSMemoryType es un código numérico del estándar SMBIOS.
        /// Solo cubrimos los tipos que de verdad vas a encontrar en
        /// equipos reales hoy en día (DDR3/DDR4/DDR5) — no todo el catálogo.
        /// </summary>
        private static string InterpretarTipoRam(object codigoRaw)
        {
            if (codigoRaw == null) return "Desconocido";

            switch ((ushort)codigoRaw)
            {
                case 24: return "DDR3";
                case 26: return "DDR4";
                case 34: return "DDR5";
                default: return "Desconocido";
            }
        }
    }
}