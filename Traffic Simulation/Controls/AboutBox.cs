#region COPYING

// Copyright 2011, 2012 Stefan Ottevanger & Sije Harkema
// 
// This file is part of Traffic Simulation.
// 
// Traffic Simulation is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Traffic Simulation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Traffic Simulation.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System.Drawing;
using System.Windows.Forms;
using Traffic_Simulation.Properties;

#endregion

namespace Traffic_Simulation
{
    public class AboutBox : Form
    {
        private readonly PictureBox logobox;
        private readonly Label textlabel;

        public AboutBox()
        {
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Size = new Size(375, 375);
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About Traffic Simulation";

            logobox = new PictureBox {Image = Resources.Traffic_Simulation_Logo.ToBitmap()};
            logobox.Location = new Point((Width - logobox.Image.Size.Width)/2, 20);
            logobox.Size = logobox.Image.Size;
            Controls.Add(logobox);

            textlabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Calibri", 10),
                    Text =
                        "Traffic Simulation\nBy Sije Harkema && Stefan Ottevanger\nCopyright © 2011 - 2012",
                    TextAlign = ContentAlignment.MiddleCenter
                };
            textlabel.Location = new Point((Width - TextRenderer.MeasureText(textlabel.Text, textlabel.Font).Width)/2,
                                           10 + logobox.Location.Y + logobox.Size.Width);
            Controls.Add(textlabel);
        }
    }
}