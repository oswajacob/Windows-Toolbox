using System;
using System.Drawing;
using System.Windows.Forms;
using Kfzt.Core;

namespace Kfzt.Forms
{
    /// <summary>
    /// Diálogo de confirmación para chkdsk (/f o /r). SIEMPRE muestra el
    /// texto de advertencia de batería/corriente, sin importar el nivel
    /// actual. Si PowerCheck detecta el caso realmente riesgoso (laptop,
    /// desconectada, menos de 40%), además bloquea el botón de confirmar.
    /// </summary>
    public class ConfirmDialog : Form
    {
        public bool Confirmado { get; private set; } = false;

        public ConfirmDialog(string nombreComando, string notaAdicional = null)
        {
            ConfigurarVentana();
            ConstruirContenido(nombreComando, notaAdicional);
        }

        private void ConfigurarVentana()
        {
            this.Text = "Confirmar";
            this.ClientSize = new Size(380, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void ConstruirContenido(string nombreComando, string notaAdicional)
        {
            var estadoEnergia = PowerCheck.ObtenerEstado();

            string textoAdvertencia =
                "Asegúrese de estar conectado a una fuente de luz y tener más " +
                "del 40% de batería. El escaneo consume energía y es peligroso " +
                "si se descarga en el proceso.";

            if (!string.IsNullOrEmpty(notaAdicional))
                textoAdvertencia += "\n\n" + notaAdicional;

            var lblAdvertencia = new Label
            {
                Text = $"Vas a ejecutar: {nombreComando}\n\n{textoAdvertencia}",
                Location = new Point(15, 15),
                Size = new Size(350, 130),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblAdvertencia);

            var btnSi = new Button
            {
                Text = "Sí, continuar",
                Location = new Point(120, 160),
                Size = new Size(110, 32),
                Enabled = !estadoEnergia.EsRiesgoso // bloqueado si de verdad es peligroso
            };
            btnSi.Click += (s, e) => { Confirmado = true; this.Close(); };

            var btnNo = new Button
            {
                Text = "Cancelar",
                Location = new Point(240, 160),
                Size = new Size(110, 32)
            };
            btnNo.Click += (s, e) => { Confirmado = false; this.Close(); };

            this.Controls.Add(btnSi);
            this.Controls.Add(btnNo);

            if (estadoEnergia.EsRiesgoso)
            {
                var lblBloqueo = new Label
                {
                    Text = "Conecta el cargador para continuar.",
                    ForeColor = Color.DarkRed,
                    Location = new Point(15, 145),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8, FontStyle.Italic)
                };
                this.Controls.Add(lblBloqueo);
            }
        }
    }
}