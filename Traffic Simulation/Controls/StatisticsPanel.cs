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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class StatisticsPanel : TableLayoutPanel
    {
        #region FieldsAndProperties

        private readonly Dictionary<string, TextBox> textboxdictionary;
        private readonly int totalcontrolheight;
        private int destinationmissed;
        private int destinationreached;
        private int lanecount;
        private double lanelength;
        private MainForm mainform;
        private double meanmovingfraction;
        private double meanspeed;
        private double movingfraction;
        private int roadcount;
        private double roadlength;
        private int stepcount;
        private double steprate;
        private double time;
        private double totaldistance;
        private int totalvehiclecount;
        private ComboBox vehiclecombobox;
        private int vehiclecount;

        public Vehicle SelectedVehicle { get; private set; }

        // Invokes zijn nodig voor Thread safety
        public double LaneLength
        {
            get { return lanelength; }
            set
            {
                Invoke(new UpdateDelegate(delegate
                    {
                        textboxdictionary["LaneLength"].Text =
                            Math.Round(value/1000, 3).ToString(CultureInfo.InvariantCulture);
                        lanelength = value;
                    }));
            }
        }

        public double MeanMovingFraction
        {
            get { return meanmovingfraction; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["MeanMovingFraction"].Text =
                                    Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);
                            }));
                meanmovingfraction = value;
            }
        }

        public double MeanSpeed
        {
            get { return meanspeed; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["MeanSpeed"].Text =
                                    Math.Round(value, 1).ToString(CultureInfo.InvariantCulture);
                            }));
                meanspeed = value;
            }
        }

        public double MovingFraction
        {
            get { return movingfraction; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["MovingFraction"].Text =
                                    Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);
                            }));
                movingfraction = value;
            }
        }

        public double RoadLength
        {
            get { return roadlength; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["RoadLength"].Text =
                                    Math.Round(value/1000, 3).ToString(CultureInfo.InvariantCulture);
                            }));
                roadlength = value;
            }
        }

        public double StepRate
        {
            get { return steprate; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["StepRate"].Text =
                                    Math.Round(value, 1).ToString(CultureInfo.InvariantCulture);
                            }));
                steprate = value;
            }
        }

        public double Time
        {
            get { return time; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["Time"].Text =
                                    Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);
                            }));
                time = value;
            }
        }

        public double TotalDistance
        {
            get { return totaldistance; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["TotalDistance"].Text =
                                    Math.Round(value/1000, 3).ToString(CultureInfo.InvariantCulture);
                            }));
                totaldistance = value;
            }
        }

        public int DestinationMissed
        {
            get { return destinationmissed; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["DestinationMissed"].Text =
                                    value.ToString(CultureInfo.InvariantCulture);
                            }));
                destinationmissed = value;
            }
        }

        public int DestinationReached
        {
            get { return destinationreached; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate
                            {
                                textboxdictionary["DestinationReached"].Text =
                                    value.ToString(CultureInfo.InvariantCulture);
                            }));
                destinationreached = value;
            }
        }

        public int LaneCount
        {
            get { return lanecount; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate { textboxdictionary["LaneCount"].Text = value.ToString(CultureInfo.InvariantCulture); }));
                lanecount = value;
            }
        }

        public int RoadCount
        {
            get { return roadcount; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate { textboxdictionary["RoadCount"].Text = value.ToString(CultureInfo.InvariantCulture); }));
                roadcount = value;
            }
        }

        public int StepCount
        {
            get { return stepcount; }
            set
            {
                Invoke(
                    new UpdateDelegate(
                        delegate { textboxdictionary["StepCount"].Text = value.ToString(CultureInfo.InvariantCulture); }));
                stepcount = value;
            }
        }

        public int TotalVehicleCount
        {
            get { return totalvehiclecount; }
            set
            {
                Invoke(new UpdateDelegate(delegate
                    {
                        textboxdictionary["TotalVehicleCount"].Text =
                            value.ToString(CultureInfo.InvariantCulture);
                        totalvehiclecount = value;
                    }));
            }
        }

        public int VehicleCount
        {
            get { return vehiclecount; }
            set
            {
                Invoke(new UpdateDelegate(delegate
                    {
                        textboxdictionary["VehicleCount"].Text =
                            value.ToString(CultureInfo.InvariantCulture);
                        vehiclecount = value;
                    }));
            }
        }

        #endregion

        public StatisticsPanel(MainForm mainform)
        {
            BackColor = SystemColors.ControlLight;
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            CreateHandle();
            DoubleBuffered = true;
            this.mainform = mainform;
            Scroll += scroll;
            SizeChanged += sizechanged;
            textboxdictionary = new Dictionary<string, TextBox>();

            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            ColumnCount = ColumnStyles.Count;
            RowCount = RowStyles.Count;

            makestatisticcontrols();
            totalcontrolheight = gettotalcontrolheight();
            vehiclecombobox.SelectedIndexChanged += selectedindexchanged;
        }

        public void SetStatistics()
        {
            DestinationMissed = 0;
            DestinationReached = 0;
            LaneCount = 0;
            LaneLength = 0;
            MeanMovingFraction = 0;
            MeanSpeed = 0;
            MovingFraction = 0;
            RoadCount = 0;
            RoadLength = 0;
            StepCount = 0;
            StepRate = 0;
            Time = 0;
            TotalDistance = 0;
            TotalVehicleCount = 0;
            VehicleCount = 0;

            resetvehiclestatistics();

            vehiclecombobox.Items.Clear();
            selectedindexchanged(null, EventArgs.Empty);
        }

        #region Controls

        private void makestatisticcontrols()
        {
            makenamelabel();
            addlabeltextboxcombo("Time:", "Time");
            addlabeltextboxcombo("Step Count:", "StepCount");
            addlabeltextboxcombo("Steprate:", "StepRate");
            addseperator(20);
            addlabeltextboxcombo("Road Count:", "RoadCount");
            addlabeltextboxcombo("Road Length:", "RoadLength");
            addlabeltextboxcombo("Lane Count:", "LaneCount");
            addlabeltextboxcombo("Lane Length:", "LaneLength");
            addseperator(20);
            addlabeltextboxcombo("Vehicle Count:", "VehicleCount");
            addlabeltextboxcombo("Total Vehicle Count:", "TotalVehicleCount");
            addlabeltextboxcombo("Total Distance:", "TotalDistance");
            addseperator(20);
            addlabeltextboxcombo("Destination Reached:", "DestinationReached");
            addlabeltextboxcombo("Destination Missed:", "DestinationMissed");
            addseperator(20);
            addlabeltextboxcombo("Mean Speed:", "MeanSpeed");
            addlabeltextboxcombo("Moving Fraction:", "MovingFraction");
            addlabeltextboxcombo("Mean Moving Fraction:", "MeanMovingFraction");
            addseperator(20);
            vehiclecombobox = addlabelcomboboxcombo("Vehicle:");
            addlabeltextboxcombo("Type:", "Type");
            addlabeltextboxcombo("X:", "X");
            addlabeltextboxcombo("Y:", "Y");
            addlabeltextboxcombo("Speed:", "Speed");
            addlabeltextboxcombo("Distance Driven:", "DistanceDriven");
            addseperator(20);
            addlabeltextboxcombo("Time Spent Moving:", "TimeSpentMoving");
            addlabeltextboxcombo("Time in City:", "TimeInCity");
            addlabeltextboxcombo("Moving Fraction:", "VehicleMovingFraction");
            addseperator(20);
        }

        private ComboBox addlabelcomboboxcombo(string labeltext)
        {
            addrow();
            makelabel(labeltext);
            return makecombobox();
        }

        private ComboBox makecombobox()
        {
            var combobox = new ComboBox {Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList};

            Controls.Add(combobox);
            SetCellPosition(combobox, new TableLayoutPanelCellPosition(1, RowStyles.Count - 1));
            return combobox;
        }

        private void addlabeltextboxcombo(string labeltext, string name)
        {
            addrow();
            makelabel(labeltext);
            addtextbox(name);
        }

        private void addtextbox(string name)
        {
            var textbox = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Enabled = false,
                    Name = name,
                    TextAlign = HorizontalAlignment.Left
                };

            textboxdictionary.Add(name, textbox);
            SetCellPosition(textbox, new TableLayoutPanelCellPosition(1, RowStyles.Count - 1));
            Controls.Add(textbox);
        }

        private void makelabel(string text)
        {
            var label = new Label
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Fill,
                    Text = text,
                    TextAlign = ContentAlignment.MiddleLeft
                };

            Controls.Add(label);
            SetCellPosition(label, new TableLayoutPanelCellPosition(0, RowStyles.Count - 1));
        }

        private void makenamelabel()
        {
            var label = new Label {AutoSize = true, Dock = DockStyle.Fill};
            label.Font = new Font(label.Font.Name, 18, FontStyle.Bold);
            label.Text = "Statistics";
            label.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(label);
            SetCellPosition(label, new TableLayoutPanelCellPosition(0, 0));
            SetColumnSpan(label, 2);
        }

        private void addrow()
        {
            RowCount++;
            RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        private void addseperator(int size)
        {
            RowCount++;
            RowStyles.Add(new RowStyle(SizeType.Absolute, size));
        }

        #endregion

        #region Statistics

        public void UpdateVehicleStatistics()
        {
            Invoke(new UpdateDelegate(updatevehiclestatistics));
        }

        private void updatevehiclestatistics()
        {
            var vehicle = vehiclecombobox.SelectedItem as Vehicle;

            if (vehicle == null) return;
            textboxdictionary["DistanceDriven"].Text =
                Math.Round(vehicle.DistanceDriven/1000, 3).ToString(CultureInfo.InvariantCulture);
            textboxdictionary["Speed"].Text = (3.6*vehicle.Speed).ToString(CultureInfo.InvariantCulture);
            textboxdictionary["TimeInCity"].Text =
                Math.Round(vehicle.TotalSeconds, 3).ToString(CultureInfo.InvariantCulture);
            textboxdictionary["TimeSpentMoving"].Text =
                Math.Round(vehicle.TotalMovingSeconds, 3).ToString(CultureInfo.InvariantCulture);
            textboxdictionary["VehicleMovingFraction"].Text =
                Math.Round((vehicle.TotalMovingSeconds/vehicle.TotalSeconds), 3).ToString(CultureInfo.InvariantCulture);
            textboxdictionary["X"].Text = vehicle.Location.X.ToString(CultureInfo.InvariantCulture);
            textboxdictionary["Y"].Text = vehicle.Location.Y.ToString(CultureInfo.InvariantCulture);
        }

        private void resetvehiclestatistics()
        {
            textboxdictionary["DistanceDriven"].Text = "0";
            textboxdictionary["Speed"].Text = "0";
            textboxdictionary["TimeInCity"].Text = "0";
            textboxdictionary["TimeSpentMoving"].Text = "0";
            textboxdictionary["Type"].Text = "";
            textboxdictionary["VehicleMovingFraction"].Text = "0";
            textboxdictionary["X"].Text = "0";
            textboxdictionary["Y"].Text = "0";
        }

        public double GetTerminationValue()
        {
            PropertyInfo propertyinfo =
                typeof (StatisticsPanel).GetProperty(ParameterPanel.TerminationStatistic.ToString());

            if (propertyinfo == null)
                return double.NaN;
            return (double) propertyinfo.GetValue(this, null);
        }

        #endregion

        #region VehicleComboBoxUpdateMethods

        public void AddVehicles(IEnumerable<Vehicle> vehicles)
        {
            Invoke(new VehicleDelegate(addvehicles), new object[] {vehicles.ToArray()});
        }

        private void addvehicles(IEnumerable<Vehicle> vehicles)
        {
            foreach (Vehicle vehicle in vehicles)
                vehiclecombobox.Items.Add(vehicle);

            if (vehiclecombobox.SelectedItem == null && vehiclecombobox.Items.Count > 0)
                vehiclecombobox.SelectedItem = vehiclecombobox.Items[0];
        }

        public void RemoveVehicles(IEnumerable<Vehicle> vehicles)
        {
            Invoke(new VehicleDelegate(removevehicles), new object[] {vehicles.ToArray()});
        }

        private void removevehicles(IEnumerable<Vehicle> vehicles)
        {
            foreach (Vehicle vehicle in vehicles)
                vehiclecombobox.Items.Remove(vehicle);

            if (vehiclecombobox.SelectedItem == null && vehiclecombobox.Items.Count > 0)
                vehiclecombobox.SelectedItem = vehiclecombobox.Items[0];
        }

        #endregion

        #region EventHandlers

        private void scroll(object o, ScrollEventArgs sea)
        {
            VerticalScroll.Value = sea.NewValue;
        }

        private void selectedindexchanged(object o, EventArgs ea)
        {
            SelectedVehicle = vehiclecombobox.SelectedItem as Vehicle;

            if (SelectedVehicle != null)
            {
                textboxdictionary["Type"].Text = SelectedVehicle.Name;
                updatevehiclestatistics();
            }
            else
                resetvehiclestatistics();
        }

        private void sizechanged(object o, EventArgs ea)
        {
            VerticalScroll.Visible = totalcontrolheight > Height;
        }

        #endregion

        #region Other

        private int gettotalcontrolheight()
        {
            return (from Control control in Controls select control.Location.Y + control.Height).Concat(new[] {0}).Max();
        }

        #endregion
    }
}