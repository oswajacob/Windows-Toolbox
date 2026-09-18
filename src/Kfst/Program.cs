using System;
using System.Windows.Forms;

namespace Kfzt
{
    /// <summary>
    /// Punto de entrada de toda la app. Este archivo NO debe crecer con
    /// lógica de negocio (nada de sfc, dism, etc. aquí) -- su único trabajo
    /// es arrancar Windows Forms y abrir la ventana principal.
    /// </summary>
    internal static class Program
    {
        // [STAThread] es obligatorio para cualquier app de Windows Forms:
        // le dice a .NET que la interfaz gráfica corre en un solo hilo
        // (Single-Threaded Apartment). Sin esto, cosas como los diálogos
        // de archivos o el portapapeles pueden fallar en silencio.
        [STAThread]
        static void Main()
        {
            // Configuración visual estándar que trae el molde de .NET 8
            // para Windows Forms (fuentes, escalado en pantallas de alta
            // resolución, etc.). No hay que tocarla.
            ApplicationConfiguration.Initialize();

            // MainForm todavía NO existe -- es la siguiente pieza que
            // vamos a construir juntos. En cuanto exista, esta línea es
            // la que abre la ventana con el sidebar (Menu/Health/Limpieza/Info/Logs).
            Application.Run(new MainForm());
        }
    }
}