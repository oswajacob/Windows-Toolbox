using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace Kfzt.Core
{
    public enum TipoDisco { HDD, SSD, Desconocido }

    public class InfoDiscoFisico
    {
        public string Modelo { get; set; }
        public TipoDisco Tipo { get; set; }
    }

    public class InfoVolumen
    {
        public string Unidad { get; set; }          // ej. "C:\"
        public long EspacioTotalGB { get; set; }
        public long EspacioLibreGB { get; set; }
    }

    /// <summary>
    /// Tipo de disco (HDD/SSD) y espacio por unidad — solo para mostrar
    /// en la sección Info. NO se usa para decidir automáticamente entre
    /// chkdsk /f y /r, esa decisión sigue siendo del usuario.
    /// </summary>
    public static class DiskInfo
    {
        public static List<InfoDiscoFisico> ObtenerDiscosFisicos()
        {
            var discos = new List<InfoDiscoFisico>();

            // OJO: MSFT_PhysicalDisk vive en un namespace WMI distinto
            // al que usa HardwareReader (root\cimv2) — si lo consultas
            // en el namespace normal, no vas a encontrar nada.
            using (var buscador = new ManagementObjectSearcher(
                @"root\Microsoft\Windows\Storage",
                "SELECT FriendlyName, MediaType FROM MSFT_PhysicalDisk"))
            {
                foreach (ManagementObject obj in buscador.Get())
                {
                    ushort mediaType = (ushort)obj["MediaType"];
                    discos.Add(new InfoDiscoFisico
                    {
                        Modelo = obj["FriendlyName"]?.ToString(),
                        Tipo = mediaType switch
                        {
                            3 => TipoDisco.HDD,
                            4 => TipoDisco.SSD,
                            _ => TipoDisco.Desconocido
                        }
                    });
                }
            }

            return discos;
        }

        public static List<InfoVolumen> ObtenerVolumenes()
        {
            var volumenes = new List<InfoVolumen>();

            // Para espacio libre no hace falta WMI -- .NET ya trae
            // DriveInfo, que es más simple y directo para esto.
            foreach (var unidad in DriveInfo.GetDrives())
            {
                if (!unidad.IsReady) continue;

                volumenes.Add(new InfoVolumen
                {
                    Unidad = unidad.Name,
                    EspacioTotalGB = unidad.TotalSize / (1024 * 1024 * 1024),
                    EspacioLibreGB = unidad.AvailableFreeSpace / (1024 * 1024 * 1024)
                });
            }

            return volumenes;
        }
    }
}