using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kfzt.Forms
{
    /// <summary>
    /// Ventana principal de Kfzt. Contiene la barra lateral de navegación
    /// y un panel de contenido que cambia según la sección seleccionada.
    /// No se puede redimensionar ni maximizar (ventana fija, minimalista).
    /// </summary>
    public class MainForm : Form
    {
        // Panel izquierdo: la barra lateral con los botones de navegación
        private Panel sidebarPanel;

        // Panel derecho: aquí se muestra el contenido de la sección activa
        private Panel contentPanel;

        // Botones de la barra lateral (uno por sección)
        private Button btnMenu;
        private Button btnHealth;
        private Button btnLimpieza;
        private Button btnInfo;
        private Button btnLogs;

        public MainForm()
        {
            ConfigurarVentana();
            ConstruirSidebar();
            ConstruirContentPanel();

            // Al abrir, mostramos la sección "Menu" por defecto
            MostrarSeccion(btnMenu);
        }

        /// <summary>
        /// Configuración base de la ventana: tamaño fijo, sin maximizar,
        /// sin poder estirar el borde. Esto cumple lo de "ventana pequeña
        /// y minimalista, no se puede hacer más grande/chica".
        /// </summary>
        private void ConfigurarVentana()
        {
            this.Text = "Kfzt";
            this.ClientSize = new Size(700, 450);               // tamaño fijo
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // no se puede estirar arrastrando el borde
            this.MaximizeBox = false;                            // desactiva el botón de maximizar
            this.MinimizeBox = true;                             // minimizar sí, eso no rompe el layout
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// Crea el panel lateral con los 5 botones de navegación.
        /// Cada botón, al hacer clic, llama a MostrarSeccion() para
        /// cambiar qué se ve en el panel de contenido.
        /// </summary>
        private void ConstruirSidebar()
        {
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = Color.FromArgb(30, 30, 30) // gris oscuro plano — v1, sin colores finales todavía
            };

            btnMenu     = CrearBotonSidebar("Menu", 0);
            btnHealth   = CrearBotonSidebar("Health", 1);
            btnLimpieza = CrearBotonSidebar("Limpieza", 2);
            btnInfo     = CrearBotonSidebar("Info", 3);
            btnLogs     = CrearBotonSidebar("Logs", 4);

            sidebarPanel.Controls.Add(btnMenu);
            sidebarPanel.Controls.Add(btnHealth);
            sidebarPanel.Controls.Add(btnLimpieza);
            sidebarPanel.Controls.Add(btnInfo);
            sidebarPanel.Controls.Add(btnLogs);

            this.Controls.Add(sidebarPanel);
        }

        /// <summary>
        /// Crea un botón plano para la barra lateral, apilado verticalmente
        /// según su índice (0 = hasta arriba).
        /// </summary>
        private Button CrearBotonSidebar(string texto, int indice)
        {
            var boton = new Button
            {
                Text = texto,
                Width = 150,
                Height = 45,
                Top = indice * 45,
                FlatStyle = FlatStyle.Flat,      // botón plano, sin relieve clásico de Windows
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            boton.FlatAppearance.BorderSize = 0; // sin borde — look minimalista
            boton.Click += (sender, e) => MostrarSeccion((Button)sender);
            return boton;
        }

        /// <summary>
        /// Panel donde se muestra el contenido de la sección activa.
        /// Por ahora vacío — cada vista real (MenuView, HealthView, etc.)
        /// se conecta aquí en los próximos archivos.
        /// </summary>
        private void ConstruirContentPanel()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            this.Controls.Add(contentPanel);
        }

        /// <summary>
        /// Cambia de sección. Por ahora solo pone una etiqueta de
        /// marcador de posición — cuando construyamos cada vista real
        /// esto las va a mostrar/ocultar en vez de texto genérico.
        /// </summary>
        private void MostrarSeccion(Button botonActivo)
        {
            contentPanel.Controls.Clear();

            var etiqueta = new Label
            {
                Text = $"Sección: {botonActivo.Text}",
                AutoSize = true,
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 12)
            };
            contentPanel.Controls.Add(etiqueta);
        }
    }
}