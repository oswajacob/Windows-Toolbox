using System;
using System.Drawing;
using System.Windows.Forms;
using Kfzt.Core;

namespace Kfzt.Forms
{
    /// <summary>
    /// Ventana de progreso independiente para tareas que corren en vivo
    /// (sfc, dism). Modal, SIN botón de cerrar — por diseño, una vez
    /// confirmada la tarea corre hasta terminar, no se cancela. No
    /// muestra % real: sfc/dism no lo reportan de forma confiable al
    /// redirigir su salida, así que usa un indicador indeterminado.
    /// </summary>
    public class ProgressForm : Form
    {
        public TaskResult Resultado { get; private set; }

        private readonly Label lblEstado;
        private readonly TextBox txtSalida;
        private bool _tareaTerminada = false;

        public ProgressForm(string nombreTarea)
        {
            this.Text = nombreTarea;
            this.ClientSize = new Size(420, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false; // sin X -- no se cancela una vez confirmada
            this.StartPosition = FormStartPosition.CenterParent;

            lblEstado = new Label
            {
                Text = $"Ejecutando {nombreTarea}...",
                Location = new Point(15, 15),
                Size = new Size(390, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var barra = new ProgressBar
            {
                Location = new Point(15, 60),
                Size = new Size(390, 20),
                Style = ProgressBarStyle.Marquee, // indeterminado, no hay % real confiable
                MarqueeAnimationSpeed = 30
            };

            txtSalida = new TextBox
            {
                Location = new Point(15, 90),
                Size = new Size(390, 135),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8)
            };

            this.Controls.Add(lblEstado);
            this.Controls.Add(barra);
            this.Controls.Add(txtSalida);

            // No se deja cerrar la ventana mientras la tarea sigue corriendo
            this.FormClosing += (s, e) =>
            {
                if (!_tareaTerminada) e.Cancel = true;
            };
        }

        /// <summary>Se llama por cada línea nueva que reporta la tarea.</summary>
        public void AgregarLinea(string linea)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => AgregarLinea(linea))); return; }
            txtSalida.AppendText(linea + Environment.NewLine);
        }

        /// <summary>Se llama cuando el vigilante detecta demasiado tiempo sin output.</summary>
        public void MostrarAvisoDeLentitud()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(MostrarAvisoDeLentitud)); return; }
            lblEstado.Text = "Esto está tardando más de lo normal — es normal en discos grandes, no cierres la aplicación.";
        }

        /// <summary>Se llama cuando la tarea termina. Cierra la ventana.</summary>
        public void Finalizar(TaskResult resultado)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => Finalizar(resultado))); return; }
            Resultado = resultado;
            _tareaTerminada = true;
            this.Close();
        }
    }
}