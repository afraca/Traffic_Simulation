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
using System.IO;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    internal class NewSimulationDialog : Form
    {
        private readonly ListBox simulationlistbox;

        public NewSimulationDialog()
        {
            MaximumSize = new Size(640, 480);
            MinimumSize = new Size(640, 480);
            Size = new Size(640, 480);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Simulation...";

            makeButton("OK", new Point(460, 415), DialogResult.OK);
            makeButton("Cancel", new Point(545, 415), DialogResult.Cancel);

            string[] files = Directory.GetFiles(string.Format("{0}\\Maps", Global.HomeFolder));

            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileNameWithoutExtension(files[i]);

            simulationlistbox = makeListBox(new Point(5, 5), new Size(610, 405), files);
        }

        public string SelctedSimulationItem
        {
            get { return simulationlistbox.SelectedItem.ToString(); }
        }

        private void makeButton(string text, Point location, DialogResult dialogresult = DialogResult.None)
        {
            var button = new Button {DialogResult = dialogresult, Location = location, Text = text};

            switch (dialogresult)
            {
                case DialogResult.OK:
                    AcceptButton = button;
                    break;
                case DialogResult.Cancel:
                    CancelButton = button;
                    break;
            }

            Controls.Add(button);
        }

        private ListBox makeListBox(Point location, Size size, object[] items)
        {
            var listbox = new ListBox();
            listbox.Items.AddRange(items);
            listbox.Location = location;
            listbox.Size = size;

            if (items.Length > 0)
                listbox.SelectedIndex = 0;

            Controls.Add(listbox);

            return listbox;
        }
    }
}