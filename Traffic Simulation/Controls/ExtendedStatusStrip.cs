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
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class ExtendedStatusStrip : StatusStrip
    {
        private readonly ToolStripLabel bufferlabel;
        private readonly ToolStripLabel statuslabel;
        private readonly ToolStripLabel stepcounterlabel;
        private readonly ToolStripLabel stepdelaylabel;

        public ExtendedStatusStrip()
        {
            Dock = DockStyle.Bottom;

            statuslabel = addtoolstriplabel("Status: ");
            bufferlabel = addtoolstriplabel("");
            stepcounterlabel = addtoolstriplabel("Step Counter: ");
            stepdelaylabel = addtoolstriplabel("Step Delay: ");

            bufferlabel.AutoSize = false;

            SizeChanged += sizechanged;
            ParameterPanel.StepDelayChanged += stepdelaychanged;
        }

        public void Set()
        {
            UpdateStepCounter(0);
            stepdelaychanged(null, EventArgs.Empty);
        }

        public void UpdateStatus(string s)
        {
            statuslabel.Text = s;
        }

        public void UpdateStatus(string s, params object[] parameters)
        {
            UpdateStatus(string.Format(s, parameters));
        }

        public void UpdateStatus(object o, StatusEventArgs sea)
        {
            statuslabel.Text = sea.Status;
        }

        public void UpdateStepCounter(int stepcounter)
        {
            stepcounterlabel.Text = string.Format("Step Counter: {0}",
                                                  stepcounter.ToString(CultureInfo.InvariantCulture));
        }

        private ToolStripLabel addtoolstriplabel(string text)
        {
            var toolstriplabel = new ToolStripLabel(text) {BackColor = Color.Transparent};
            toolstriplabel.TextChanged += sizechanged;

            Items.Add(toolstriplabel);

            return toolstriplabel;
        }

        private void stepdelaychanged(object o, EventArgs ea)
        {
            stepdelaylabel.Text = string.Format("Step Delay: {0}",
                                                ParameterPanel.StepDelay.ToString(CultureInfo.InvariantCulture));
        }

        private void sizechanged(object o, EventArgs ea)
        {
            bufferlabel.Width = ClientSize.Width - statuslabel.Width - stepcounterlabel.Width - stepdelaylabel.Width -
                                15;
        }
    }
}