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
using System.Windows.Forms;
using Traffic_Simulation.Properties;

#endregion

namespace Traffic_Simulation
{
    public class MainForm : Form
    {
        private ScreenData screendata;
        private Simulation simulation;
        private int simulationid;

        public MainForm()
        {
            BackColor = SystemColors.ControlLightLight;
            DoubleBuffered = true;
            Icon = Resources.Traffic_Simulation_Logo;
            simulationid = 0;
            Size = new Size(Screen.PrimaryScreen.Bounds.Width/2, Screen.PrimaryScreen.Bounds.Height/2);
            ShowIcon = true;
            WindowState = FormWindowState.Maximized;

            FormClosing += formclosing;
            SimulationCreated += simulationcreated;
            SimulationNullified += simulationnullified;
            SizeChanged += sizechanged;

            Randomizer.Initialize();

            makemenus();
            makepanels();

            simulationnullified(null, EventArgs.Empty);
            sizechanged(null, EventArgs.Empty);

            ExtendedStatusStrip.UpdateStatus("Loading finished.");
        }

        #region MakeMethods

        private void makemenus()
        {
            ExtendedStatusStrip = new ExtendedStatusStrip();
            MainMenu = new MainMenu(this,
                                    new MenuData("File", "New", "Open", "Seperator", "Close", "Seperator", "Save",
                                                 "Save As", "Seperator", "Quit"),
                                    new MenuData("Edit", "Start", "Stop", "Reset", "Seperator", "Next Step"),
                                    new MenuData("Help", "About"));
            TaskMenu = new TaskMenu(this, "Start", "Stop", "Reset", "Seperator", "Next Step", "Seperator", "Zoom In",
                                    "Zoom Out", "Seperator", "Show Parameters", "Show Simulation", "Show Statistics",
                                    "Full Screen");

            (TaskMenu.GetToolStripItem("Show Parameters")).ForeColor = Color.Green;
            (TaskMenu.GetToolStripItem("Show Simulation")).ForeColor = Color.Green;
            (TaskMenu.GetToolStripItem("Show Statistics")).ForeColor = Color.Green;

            ExtendedStatusStrip.Set();
            MainMenuStrip = MainMenu;

            Controls.Add(ExtendedStatusStrip);
            Controls.Add(TaskMenu);
            Controls.Add(MainMenu);
        }

        private void makepanels()
        {
            ParameterPanel = new ParameterPanel(this);
            SimulationPanel = new SimulationPanel(this);
            StatisticsPanel = new StatisticsPanel(this);

            SimulationPanel.SetEventHandlers();
            ParameterPanel.SetParameters();
            StatisticsPanel.SetStatistics();

            Controls.Add(ParameterPanel);
            Controls.Add(SimulationPanel);
            Controls.Add(StatisticsPanel);
        }

        #endregion

        #region MenuMethods

        public void About_Click(object o, EventArgs ea)
        {
            new AboutBox().ShowDialog();
        }

        public void New_Click(object o, EventArgs ea)
        {
            if (simulationisnull() && new NewSimulationDialog().ShowDialog() == DialogResult.OK)
                Simulation = new Simulation(this, ++simulationid);
        }

        public void Open_Click(object o, EventArgs ea)
        {
            if (!simulationisnull()) return;
            var openfiledialog = new OpenFileDialog {Filter = "Simulation-file|*.sml", Title = "Open Simulation..."};

            if (openfiledialog.ShowDialog() == DialogResult.OK)
                open(openfiledialog.FileName);
        }

        public void Quit_Click(object o, EventArgs ea)
        {
            if (simulationisnull())
                Environment.Exit(0);
        }

        private void formclosing(object o, FormClosingEventArgs fcea)
        {
            Quit_Click(o, EventArgs.Empty);
            fcea.Cancel = true;
        }

        private void open(string filename)
        {
            if (Simulation != null)
                Simulation = null;
            Simulation = ((Simulation) Serializer.Deserialize(filename)).DeserializationRepopulation(this);
        }

        #endregion

        #region ScreenEventHandlers

        public void ShowParameters_Click(object o, EventArgs ea)
        {
            var button = o as ToolStripButton;

            ParameterPanel.Visible = !ParameterPanel.Visible;

            if (button != null) button.ForeColor = ParameterPanel.Visible ? Color.Green : Color.Black;

            sizechanged(o, ea);
        }

        public void ShowStatistics_Click(object o, EventArgs ea)
        {
            var button = o as ToolStripButton;

            StatisticsPanel.Visible = !StatisticsPanel.Visible;

            if (button != null) button.ForeColor = StatisticsPanel.Visible ? Color.Green : Color.Black;

            sizechanged(o, ea);
        }

        public void ShowSimulation_Click(object o, EventArgs ea)
        {
            var button = o as ToolStripButton;

            SimulationPanel.Draw = !SimulationPanel.Draw;

            if (button != null) button.ForeColor = SimulationPanel.Draw ? Color.Green : Color.Black;

            SimulationPanel.Invalidate();
        }

        public void FullScreen_Click(object o, EventArgs ea)
        {
            var button = o as ToolStripButton;
            screendata.FullScreen = !screendata.FullScreen;

            SuspendLayout();

            if (screendata.FullScreen)
            {
                screendata.FormWindowState = WindowState;
                screendata.Location = Location;
                screendata.Size = ClientSize;
                WindowState = FormWindowState.Normal;
                TopMost = true;
                Location = new Point(0, 0);
                FormBorderStyle = FormBorderStyle.None;
                Size = Screen.PrimaryScreen.Bounds.Size;

                if (button != null) button.ForeColor = Color.Green;
            }
            else
            {
                TopMost = false;
                Location = screendata.Location;
                ClientSize = screendata.Size;
                WindowState = screendata.FormWindowState;
                FormBorderStyle = FormBorderStyle.Sizable;

                if (button != null) button.ForeColor = Color.Black;
            }

            ResumeLayout();
        }

        #endregion

        #region SimulationMethods

        public void CloseSimulation()
        {
            Simulation = null;
        }

        private void simulationcreated(object o, EventArgs ea)
        {
            ToolStripManager.Merge(Simulation.TaskMenu, TaskMenu);
            ToolStripManager.Merge(Simulation.MainMenu, MainMenu);
            SimulationPanel.Text = Simulation.FileName;

            Text = string.Format("Traffic Simulation ({0}) - © Sije Harkema & Stefan Ottevanger 2011 - 2012",
                                 Simulation.FileName);
        }

        private bool simulationisnull()
        {
            if (simulation != null) // Bekijkt of een simulation null is
                simulation.Close_Click(null, new EventArgs()); // Zo nee, sluit hem
            return simulation == null;
        }

        private void simulationnullified(object o, EventArgs ea)
        {
            ToolStripManager.RevertMerge(TaskMenu);
            ToolStripManager.RevertMerge(MainMenu);

            ExtendedStatusStrip.Set();
            StatisticsPanel.SetStatistics();
            Text = "Traffic Simulation - © Sije Harkema & Stefan Ottevanger 2011 - 2012";
        }

        #endregion

        #region OtherEventHandlers

        private void sizechanged(object o, EventArgs ea)
        {
            ParameterPanel.Location = new Point(0, TaskMenu.Location.Y + TaskMenu.Height);

            if (ParameterPanel.Visible || !Visible)
                ParameterPanel.Size = new Size(250,
                                               ClientSize.Height - (TaskMenu.Location.Y + TaskMenu.Height) -
                                               ExtendedStatusStrip.Height);
            else
                ParameterPanel.Size = new Size(0, 0);

            if (StatisticsPanel.Visible || !Visible)
                StatisticsPanel.Size = new Size(250,
                                                ClientSize.Height - (TaskMenu.Location.Y + TaskMenu.Height) -
                                                ExtendedStatusStrip.Height);
            else
                StatisticsPanel.Size = new Size(0, 0);

            StatisticsPanel.Location = new Point(ClientSize.Width - StatisticsPanel.Width,
                                                 TaskMenu.Location.Y + TaskMenu.Height);

            SimulationPanel.Location = new Point(ParameterPanel.Width, TaskMenu.Location.Y + TaskMenu.Height);
            SimulationPanel.Size = new Size(ClientSize.Width - ParameterPanel.Width - StatisticsPanel.Width,
                                            ClientSize.Height - (TaskMenu.Location.Y + TaskMenu.Height) -
                                            ExtendedStatusStrip.Height);
        }

        #endregion

        public ExtendedStatusStrip ExtendedStatusStrip { get; private set; }
        private MainMenu MainMenu { get; set; }
        public ParameterPanel ParameterPanel { get; private set; }

        public Simulation Simulation
        {
            get { return simulation; }
            private set
            {
                bool changed = simulation != value;
                bool created = simulation == null && value != null;
                bool nullified = simulation != null && value == null;

                simulation = value;

                if (changed && SimulationChanged != null)
                    SimulationChanged(simulation, new EventArgs());
                if (created && SimulationCreated != null)
                    SimulationCreated(simulation, new EventArgs());
                if (nullified && SimulationNullified != null)
                    SimulationNullified(null, new EventArgs());
            }
        }

        public SimulationPanel SimulationPanel { get; private set; }
        public StatisticsPanel StatisticsPanel { get; private set; }
        private TaskMenu TaskMenu { get; set; }
        public event EventHandler SimulationChanged;
        public event EventHandler SimulationCreated;
        public event EventHandler SimulationNullified;
    }
}