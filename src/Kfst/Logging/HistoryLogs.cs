using System;
using System.IO;
using System.Text.Json;

namespace Kfzt.Logging
{
    /// <summary>
    /// Cada entrada del historial. Una tarea genera DOS entradas: una con
    /// Estado = "Iniciado" al arrancar, y otra con el resultado final al
    /// terminar. Si una tarea nunca llega a la segunda entrada, esa
    /// entrada huérfana es tu señal de que algo no se procesó completo.
    /// </summary>
    public class EntradaHistorial
    {
        public string Id { get; set; }
        public string Tarea { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
    }

    /// <summary>
    /// Escribe y lee el historial de tareas. Usa formato "JSON Lines"
    /// (un objeto JSON por línea) en vez de un solo arreglo JSON grande:
    /// cada entrada se agrega con un simple append al archivo, sin leer
    /// y reescribir todo el historial cada vez — si la app truena a
    /// medio proceso, no se pierde nada de lo que ya estaba escrito.
    /// </summary>
    public static class HistoryLog
    {
        private static readonly string CarpetaDatos =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kfzt");

        private static readonly string RutaArchivo =
            Path.Combine(CarpetaDatos, "historial.jsonl");

        /// <summary>Ruta de la carpeta — la usa el botón de "abrir carpeta" en la sección Logs.</summary>
        public static string ObtenerRutaCarpeta() => CarpetaDatos;

        /// <summary>
        /// Registra el inicio de una tarea. Devuelve el Id generado —
        /// guárdalo para pasarlo a RegistrarCierre() cuando termine.
        /// </summary>
        public static string RegistrarInicio(string nombreTarea)
        {
            string id = Guid.NewGuid().ToString("N");
            Escribir(new EntradaHistorial
            {
                Id = id,
                Tarea = nombreTarea,
                Estado = "Iniciado",
                Fecha = DateTime.Now
            });
            return id;
        }

        /// <summary>Registra el resultado final de una tarea ya iniciada.</summary>
        public static void RegistrarCierre(string id, string nombreTarea, string resultado)
        {
            Escribir(new EntradaHistorial
            {
                Id = id,
                Tarea = nombreTarea,
                Estado = resultado,
                Fecha = DateTime.Now
            });
        }

        private static void Escribir(EntradaHistorial entrada)
        {
            Directory.CreateDirectory(CarpetaDatos); // no falla si ya existe
            string linea = JsonSerializer.Serialize(entrada);
            File.AppendAllText(RutaArchivo, linea + Environment.NewLine);
        }
    }
}