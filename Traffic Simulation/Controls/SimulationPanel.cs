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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class SimulationPanel : Panel
    {
        private readonly HScrollBar hscrollbar;
        private readonly MainForm mainform;
        private readonly VScrollBar vscrollbar;

        private int height;
        private int mouseX;
        private int mouseY;
        private bool moved;
        private int width;

        public SimulationPanel(MainForm mainform)
        {
            DoubleBuffered = true;
            Draw = true;
            this.mainform = mainform;

            hscrollbar = new HScrollBar {Dock = DockStyle.Bottom, Enabled = false};
            hscrollbar.Scroll += hscroll;
            hscrollbar.Visible = true;

            vscrollbar = new VScrollBar {Dock = DockStyle.Right, Enabled = false};
            vscrollbar.Scroll += vscroll;
            vscrollbar.Visible = true;

            Controls.Add(hscrollbar);
            Controls.Add(vscrollbar);
        }

        public bool Draw { get; set; }

        public void SetEventHandlers()
        {
            mainform.SimulationChanged += simulationchanged;
            mainform.SimulationCreated += simulationcreated;
            mainform.SimulationNullified += simulationnullified;

            Paint += paint;
            SizeChanged += mainform.ParameterPanel.UpdateMatrix;
        }

        #region SimulationEventHandlers

        private void simulationchanged(object o, EventArgs ea)
        {
            Invalidate();
        }

        private void simulationcreated(object o, EventArgs ea)
        {
            MouseClick += mouseclick;
            MouseDown += mousedown;
            MouseMove += mousemove;
            MouseUp += mouseup;

            hscrollbar.Enabled = true;
            vscrollbar.Enabled = true;
            mainform.ParameterPanel.MatrixChanged += matrixchanged;

            matrixchanged(o, ea);
        }

        private void simulationnullified(object o, EventArgs ea)
        {
            MouseClick -= mouseclick;
            MouseDown -= mousedown;
            MouseMove -= mousemove;
            MouseUp -= mouseup;

            hscrollbar.Enabled = false;
            vscrollbar.Enabled = false;
            mainform.ParameterPanel.MatrixChanged -= matrixchanged;
        }

        #endregion

        #region MouseEventHandlers

        private void mouseclick(object o, MouseEventArgs mea)
        {
            // Als ergens op het SimulationPanel wordt geklikt, zorg er dan voor dat de map het centrum verplaatst naar waar geklikt is
            // En zoom zoals is gedefinieerd.

            if (moved) return;
            mainform.ParameterPanel.TranslateCenter(mea);
            mainform.ParameterPanel.Zoom(mea.Button == MouseButtons.Right);
        }

        private void mousedown(object o, MouseEventArgs mea)
        {
            mouseX = mea.X;
            mouseY = mea.Y;
        }

        private void mousemove(object o, MouseEventArgs mea)
        {
            if (mea.Button != MouseButtons.Left) return;
            if ((mea.X - mouseX)*(mea.X - mouseX) > 5 || (mea.Y - mouseY)*(mea.Y - mouseY) > 5)
                moved = true;

            mainform.ParameterPanel.Translate(mouseX - mea.X, mouseY - mea.Y);
            mouseX = mea.X;
            mouseY = mea.Y;
        }

        private void mouseup(object o, EventArgs mea)
        {
            moved = false;
        }

        #endregion

        #region ScrollBarEventHandlers

        private void hscroll(object o, ScrollEventArgs sea)
        {
            mainform.ParameterPanel.UpdateIParameterBox("ViewX", sea.NewValue - width/2);
        }

        private void vscroll(object o, ScrollEventArgs sea)
        {
            mainform.ParameterPanel.UpdateIParameterBox("ViewY", sea.NewValue - height/2);
        }

        #endregion

        #region OtherEventHandlers

        private void matrixchanged(object o, EventArgs ea)
        {
            width = Convert.ToInt32(2*Math.Max(Math.Abs(mainform.Simulation.XMin), Math.Abs(mainform.Simulation.XMax)));
            height = Convert.ToInt32(2*Math.Max(Math.Abs(mainform.Simulation.YMin), Math.Abs(mainform.Simulation.YMax)));

            hscrollbar.Maximum = width;
            hscrollbar.Minimum = 0;

            vscrollbar.Maximum = height;
            vscrollbar.Minimum = 0;

            hscrollbar.Value = Math.Max(0, Math.Min(width, Convert.ToInt32(ParameterPanel.ViewX + width/2)));
            vscrollbar.Value = Math.Max(0, Math.Min(height, Convert.ToInt32(ParameterPanel.ViewY + height/2)));
        }

        private void paint(object o, PaintEventArgs pea)
        {
            pea.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pea.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            pea.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            if (mainform.Simulation == null || !Draw)
                pea.Graphics.FillRectangle(new SolidBrush(SystemColors.ControlDarkDark), 0, 0, Width, Height);
            else
            {
                pea.Graphics.FillRectangle(new SolidBrush(SystemColors.ControlLight), 0, 0, Width, Height);
                mainform.Simulation.Draw(pea);
            }
        }

        #endregion
    }
}