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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class ParameterPanel : TableLayoutPanel, IParameterContainer
    {
        #region FieldsAndProperties

        private static Dictionary<Type, int> vehiclefractiondictionary;

        private static bool centervehicle;
        private static bool showchangecurve;
        private static bool showcurves;
        private static bool showdestination;
        private static bool showids;
        private static bool shownames;
        private static bool showpoints;

        private static double viewsize;
        private static double viewx;
        private static double viewy;

        private static int stepdelay;

        private static string vehicleset;
        private readonly MainForm mainform;

        private readonly int totalcontrolheight;
        private Dictionary<string, IParameterBox> iparameterboxdictionary;
        private List<IParameterBox> iparameterboxlist;

        public static Matrix Matrix { get; private set; }

        public static TerminationStatistic TerminationStatistic { get; private set; }
        public static TerminationType TerminationType { get; private set; }


        public static bool CenterVehicle
        {
            get { return centervehicle; }
            private set
            {
                bool changed = centervehicle != value;
                centervehicle = value;
                if (changed && CenterVehicleChanged != null)
                    CenterVehicleChanged(null, EventArgs.Empty);
            }
        }

        public static bool ShowChangeCurve
        {
            get { return showchangecurve; }
            private set
            {
                bool changed = showchangecurve != value;
                showchangecurve = value;
                if (changed && ShowChangeCurveChanged != null)
                    ShowChangeCurveChanged(null, EventArgs.Empty);
            }
        }

        public static bool ShowCurves
        {
            get { return showcurves; }
            private set
            {
                bool changed = showcurves != value;
                showcurves = value;
                if (changed && ShowCurvesChanged != null)
                    ShowCurvesChanged(null, EventArgs.Empty);
            }
        }

        public static bool ShowDestination
        {
            get { return showdestination; }
            private set
            {
                bool changed = showdestination != value;
                showdestination = value;
                if (changed && ShowDestinationChanged != null)
                    ShowDestinationChanged(null, EventArgs.Empty);
            }
        }

        public static bool ShowIds
        {
            get { return showids; }
            private set
            {
                bool changed = showids != value;
                showids = value;
                if (changed && ShowIdsChanged != null)
                    ShowIdsChanged(null, EventArgs.Empty);
            }
        }

        public static bool ShowNames
        {
            get { return shownames; }
            private set
            {
                bool changed = shownames != value;
                shownames = value;
                if (changed && ShowNamesChanged != null)
                    ShowNamesChanged(null, EventArgs.Empty);
            }
        }

        public static bool ShowPoints
        {
            get { return showpoints; }
            private set
            {
                bool changed = showpoints != value;
                showpoints = value;
                if (changed && ShowPointsChanged != null)
                    ShowPointsChanged(null, EventArgs.Empty);
            }
        }

        public static double GreenTime { get; private set; }
        public static double SpeedFactor { get; private set; }
        public static double TerminationValue { get; private set; }

        public static double ViewSize
        {
            get { return viewsize; }
            private set
            {
                bool changed = viewsize != value;
                viewsize = value;
                if (changed && ViewChanged != null)
                    ViewChanged(null, EventArgs.Empty);
            }
        }

        public static double ViewX
        {
            get { return viewx; }
            private set
            {
                bool changed = viewx != value;
                viewx = value;
                if (changed && ViewChanged != null)
                    ViewChanged(null, EventArgs.Empty);
            }
        }

        public static double ViewY
        {
            get { return viewy; }
            private set
            {
                bool changed = viewy != value;
                viewy = value;
                if (changed && ViewChanged != null)
                    ViewChanged(null, EventArgs.Empty);
            }
        }

        public static double YellowTime { get; private set; }
        public static double ZoomFactor { get; private set; }
        public static double μSpawn { get; private set; }
        public static double μSpeed { get; private set; }
        public static double σSpawn { get; private set; }
        public static double σSpeed { get; private set; }

        public static int NextStep { get; private set; }

        public static int StepDelay
        {
            get { return stepdelay; }
            private set
            {
                bool changed = stepdelay != value;
                stepdelay = value;
                if (changed && StepDelayChanged != null)
                    StepDelayChanged(null, EventArgs.Empty);
            }
        }

        public static int VehicleCap { get; private set; }

        public static object MatrixLock { get; private set; }

        public static string VehicleSet
        {
            get { return vehicleset; }
            set
            {
                bool changed = vehicleset != value;
                vehicleset = value;

                if (changed && VehicleSetChanged != null)
                    VehicleSetChanged(null, EventArgs.Empty);
            }
        }

        public static event EventHandler CenterVehicleChanged;
        public static event EventHandler ShowChangeCurveChanged;
        public static event EventHandler ShowCurvesChanged;
        public static event EventHandler ShowDestinationChanged;
        public static event EventHandler ShowIdsChanged;
        public static event EventHandler ShowNamesChanged;
        public static event EventHandler ShowPointsChanged;
        public static event EventHandler StepDelayChanged;
        public static event EventHandler VehicleFractionChanged;
        public static event EventHandler VehicleSetChanged;
        public static event EventHandler ViewChanged;

        public event EventHandler MatrixChanged;

        #endregion

        public ParameterPanel(MainForm mainform)
        {
            BackColor = SystemColors.ControlLight;
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            DoubleBuffered = true;
            this.mainform = mainform;
            Scroll += scroll;
            SizeChanged += sizechanged;

            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            ColumnCount = ColumnStyles.Count;

            RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            RowCount = RowStyles.Count;

            MatrixLock = new object();
            VehicleSetChanged += vehiclesetchanged;
            ViewChanged += UpdateMatrix;

            makeparametercontrols();

            totalcontrolheight = gettotalcontrolheight();
        }

        #region Parameters

        public void UpdateParameter(IParameterBox iparameterbox, object value)
        {
            // Parameter from Vehicle Fractions have a Type in their Tag
            // We use this to determine where the parameter needs to be written

            var type = iparameterbox.Tag as Type;

            if (type != null)
            {
                vehiclefractiondictionary[type] = (int) value;

                if (VehicleFractionChanged != null)
                    VehicleFractionChanged(null, new ParameterEventArgs(iparameterbox.Name, value));
            }
            else
            {
                PropertyInfo propertyinfo = typeof (ParameterPanel).GetProperty(iparameterbox.Name);

                if (propertyinfo != null)
                    propertyinfo.SetValue(null, value, null);
            }
        }

        public static int GetVehicleFraction(Type type)
        {
            return vehiclefractiondictionary[type];
        }

        public void SetParameters()
        {
            UpdateIParameterBox("CenterVehicle", false);
            UpdateIParameterBox("GreenTime", 9);
            UpdateIParameterBox("NextStep", 50);
            UpdateIParameterBox("ShowChangeCurve", true);
            UpdateIParameterBox("ShowCurves", false);
            UpdateIParameterBox("ShowDestination", true);
            UpdateIParameterBox("ShowIds", false);
            UpdateIParameterBox("ShowNames", true);
            UpdateIParameterBox("ShowPoints", false);
            UpdateIParameterBox("SpeedFactor", 1);
            UpdateIParameterBox("StepDelay", 0);
            UpdateIParameterBox("TerminationStatistic", TerminationStatistic.None);
            UpdateIParameterBox("TerminationType", TerminationType.Greater);
            UpdateIParameterBox("TerminationValue", 10);
            UpdateIParameterBox("VehicleCap", 1000);
            UpdateIParameterBox("VehicleSet", "Default");
            UpdateIParameterBox("ViewSize", 4000);
            UpdateIParameterBox("ViewX", 0);
            UpdateIParameterBox("ViewY", 0);
            UpdateIParameterBox("YellowTime", 3);
            UpdateIParameterBox("ZoomFactor", 2);
            UpdateIParameterBox("μSpawn", 10);
            UpdateIParameterBox("μSpeed", 1);
            UpdateIParameterBox("σSpeed", 0.1);
            UpdateIParameterBox("σSpawn", 3);

            List<IParameterBox> vehiclefractionlist =
                iparameterboxlist.Where(p => p.Tag is Type).ToList();

            foreach (IParameterBox t in vehiclefractionlist)
                UpdateIParameterBox(t.Name, 1);
        }

        #endregion

        #region Controls

        private void makeparametercontrols()
        {
            List<Type> vehicletypelist = Global.GetClassesInNameSpace("Traffic_Simulation.Vehicles");

            iparameterboxlist = new List<IParameterBox>();
            iparameterboxdictionary = new Dictionary<string, IParameterBox>();
            vehiclefractiondictionary = new Dictionary<Type, int>();

            makenamelabel();
            addlabelnumbertextboxcombo("Step Delay:", "StepDelay", typeof (IntTextBox), 0, 1000);
            addlabelnumbertextboxcombo("Speed Factor:", "SpeedFactor", typeof (DoubleTextBox), 0.1, 10);
            addlabelnumbertextboxcombo("Next Step:", "NextStep", typeof (IntTextBox), 1, 1000);
            addlabelnumbertextboxcombo("Zoom Factor:", "ZoomFactor", typeof (DoubleTextBox), 0.1, 10);
            addseperator(20);
            addlabelenumcomboboxcombo("Termination Statistic:", "TerminationStatistic", typeof (TerminationStatistic));
            addlabelenumcomboboxcombo("Termination Type:", "TerminationType", typeof (TerminationType));
            addlabelnumbertextboxcombo("Termination Value:", "TerminationValue", typeof (DoubleTextBox),
                                       double.NegativeInfinity, double.PositiveInfinity);
            addseperator(20);
            addlabelnumbertextboxcombo("X:", "ViewX", typeof (DoubleTextBox), double.NegativeInfinity,
                                       double.PositiveInfinity);
            addlabelnumbertextboxcombo("Y:", "ViewY", typeof (DoubleTextBox), double.NegativeInfinity,
                                       double.PositiveInfinity);
            addlabelnumbertextboxcombo("Size:", "ViewSize", typeof (DoubleTextBox), 1, 1000000);
            addseperator(20);
            addlabelnumbertextboxcombo("μ Spawn:", "μSpawn", typeof (DoubleTextBox), 0, 1000);
            addlabelnumbertextboxcombo("σ Spawn:", "σSpawn", typeof (DoubleTextBox), 0, 1000);
            addlabelnumbertextboxcombo("μ Speed:", "μSpeed", typeof (DoubleTextBox), 0, 10);
            addlabelnumbertextboxcombo("σ Speed:", "σSpeed", typeof (DoubleTextBox), 0, 10);
            addlabelnumbertextboxcombo("Vehicle Cap:", "VehicleCap", typeof (IntTextBox), 0, 10000);
            addlabelparametercomboboxcombo("Vehicle Set:", "VehicleSet", "Default", "Porsche 911 (993)");
            addseperator(20);
            addlabelnumbertextboxcombo("Green Time:", "GreenTime", typeof (DoubleTextBox), 0, 1000);
            addlabelnumbertextboxcombo("Yellow Time:", "YellowTime", typeof (DoubleTextBox), 0, 1000);
            addseperator(20);

            foreach (Type t in vehicletypelist)
            {
                NumberTextBox numbertextbox =
                    addlabelnumbertextboxcombo(string.Format("{0} Fraction:", t.Name),
                                               t.Name, typeof (IntTextBox), 0, 1000000);
                numbertextbox.Tag = t;
                // Add the type of this Vehicle Fraction NumberTextBox to NumberTextBox.
                vehiclefractiondictionary.Add(t, 0);
            }

            addseperator(20);
            addlabelboolcomboboxcombo("Center Vehicle:", "CenterVehicle");
            addlabelboolcomboboxcombo("Show Change Curve:", "ShowChangeCurve");
            addlabelboolcomboboxcombo("Show Curves:", "ShowCurves");
            addlabelboolcomboboxcombo("Show Destination:", "ShowDestination");
            addlabelboolcomboboxcombo("Show Ids:", "ShowIds");
            addlabelboolcomboboxcombo("Show Names:", "ShowNames");
            addlabelboolcomboboxcombo("Show Points:", "ShowPoints");
            addseperator(20);
        }

        private void addrow()
        {
            RowCount++;
            RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
            label.Text = "Parameters";
            label.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(label);
            SetCellPosition(label, new TableLayoutPanelCellPosition(0, 0));
            SetColumnSpan(label, 2);
        }

        private void addseperator(int size)
        {
            RowCount++;
            RowStyles.Add(new RowStyle(SizeType.Absolute, size));
        }

        public IParameterBox Next(IParameterBox parametertextbox)
        {
            // Our answer to GetNextControl of a normal Control. At the time of writing we didn't realise the method existed
            // and now we'll keep it here in case we'll ever be want to be able to exclude parametertextboxes

            int min = (iparameterboxlist.IndexOf(parametertextbox) + 1)%iparameterboxlist.Count;
            int max = min + iparameterboxlist.Count;

            for (int i = min; i < max; i++)
                if (iparameterboxlist[i%iparameterboxlist.Count].Enabled)
                    return iparameterboxlist[i%iparameterboxlist.Count];

            return parametertextbox;
        }

        #region ChangeIParameterBox

        public void SetIParameterBox(string name, bool enabled)
        {
            iparameterboxdictionary[name].Enabled = enabled;
        }

        public void UpdateIParameterBox(string name, object value)
        {
            iparameterboxdictionary[name].SetValue(value);
        }

        #endregion

        #region MakeIParameterBox

        private void addlabelboolcomboboxcombo(string labeltext, string comboboxname)
        {
            addrow();
            makelabel(labeltext);
            setiparameterbox(makeboolcombobox(comboboxname));
        }

        private static BoolComboBox makeboolcombobox(string name)
        {
            var boolcombobox = new BoolComboBox {Name = name};

            return boolcombobox;
        }

        private BoolTextBox addlabelbooltextboxcombo(string labelText, string textboxName)
        {
            addrow();
            makelabel(labelText);
            return setiparameterbox(makebooltextbox(textboxName)) as BoolTextBox;
        }

        private static BoolTextBox makebooltextbox(string name)
        {
            var boolcombobox = new BoolTextBox {Name = name};

            return boolcombobox;
        }

        private void addlabelenumcomboboxcombo(string labelText, string comboboxName, Type type)
        {
            addrow();
            makelabel(labelText);
            setiparameterbox(makeenumcombobox(comboboxName, type));
        }

        private static EnumComboBox makeenumcombobox(string name, Type type)
        {
            var enumcombobox = new EnumComboBox {Name = name};
            enumcombobox.SetEnum(type);

            return enumcombobox;
        }

        private NumberTextBox addlabelnumbertextboxcombo(string labelText, string textboxName, Type type, double min,
                                                         double max, double factor = 1)
        {
            addrow();
            makelabel(labelText);
            return setiparameterbox(makenumbertextbox(textboxName, type, min, max, factor)) as NumberTextBox;
        }

        private static NumberTextBox makenumbertextbox(string name, Type type, double min, double max, double factor = 1)
        {
            var numbertextbox = (NumberTextBox) Activator.CreateInstance(type);
            numbertextbox.Factor = factor;
            numbertextbox.Max = max;
            numbertextbox.Min = min;
            numbertextbox.Name = name;
            numbertextbox.TextAlign = HorizontalAlignment.Left;

            return numbertextbox;
        }

        private void addlabelparametercomboboxcombo(string labelText, string textboxName, params string[] items)
        {
            addrow();
            makelabel(labelText);
            setiparameterbox(makeparametercombobox(textboxName, items));
        }

        private static ParameterComboBox makeparametercombobox(string name, object[] items)
        {
            var parameterbox = new ParameterComboBox();
            parameterbox.Items.AddRange(items);
            parameterbox.Name = name;

            return parameterbox;
        }

        private IParameterBox setiparameterbox(IParameterBox iparameterbox)
        {
            // All IParameterBoxes have these properties, so we'll put it in an explicit method.

            iparameterbox.Dock = DockStyle.Fill;
            iparameterbox.SetIParameterContainer(this);

            iparameterbox.KeyPress += control_KeyPress;
            iparameterbox.StatusChanged += mainform.ExtendedStatusStrip.UpdateStatus;

            iparameterboxdictionary.Add(iparameterbox.Name, iparameterbox);
            iparameterboxlist.Add(iparameterbox);

            SetCellPosition((Control) iparameterbox, new TableLayoutPanelCellPosition(1, RowStyles.Count - 1));
            Controls.Add((Control) iparameterbox);

            return iparameterbox;
        }

        #endregion

        #endregion

        #region Matrix

        public void Translate(int x, int y)
        {
            int simulationwidth = mainform.SimulationPanel.Width;
            int simulationheight = mainform.SimulationPanel.Height;
            double factor = Math.Min(simulationwidth/ViewSize, simulationheight/ViewSize);

            UpdateIParameterBox("ViewX", ViewX + x/factor);
            UpdateIParameterBox("ViewY", ViewY + y/factor);
        }

        public void TranslateCenter(MouseEventArgs mea)
        {
            // Our methods calculate from the center coördinate while the drawing is done from the upperright corner.
            // Therefore:

            int simulationwidth = mainform.SimulationPanel.Width;
            int simulationheight = mainform.SimulationPanel.Height;

            // The ratio between the size of the drawingfield and the ViewSize. We want all that's within the ViewSize
            // to be drawn so we choose the minimum factor.

            double factor = Math.Min(simulationwidth/ViewSize, simulationheight/ViewSize);

            // Determine the coordinate of the map in the upper right corner.
            double xTranslation = ViewX - simulationwidth/(2*factor);
            double yTranslation = ViewY - simulationheight/(2*factor);

            // Put the new centrecoordinate on the place where the map was clicked.
            UpdateIParameterBox("ViewX", mea.X/factor + xTranslation);
            UpdateIParameterBox("ViewY", mea.Y/factor + yTranslation);
        }

        public void UpdateMatrix(object o, EventArgs ea)
        {
            // Sort of analoge to TranslateCenter
            int simulationwidth = mainform.SimulationPanel.Width;
            int simulationheight = mainform.SimulationPanel.Height;

            double factor = Math.Min(simulationwidth/ViewSize, simulationheight/ViewSize);
            double xTranslation = ViewX - simulationwidth/(2*factor);
            double yTranslation = ViewY - simulationheight/(2*factor);

            Monitor.Enter(MatrixLock);
            Matrix = new Matrix();
            Matrix.Translate((float) (-xTranslation*factor), (float) (-yTranslation*factor));
            Matrix.Scale((float) factor, (float) factor);
            Monitor.Exit(MatrixLock);

            if (MatrixChanged != null)
                MatrixChanged(o, ea);
        }

        public void Zoom(bool zoomOut)
        {
            double zoomfactor = ZoomFactor;

            if (zoomOut)
                zoomfactor = 1/zoomfactor;

            UpdateIParameterBox("ViewSize", ViewSize/zoomfactor);
        }

        #endregion

        #region EventHandlers

        private void control_KeyPress(object o, KeyPressEventArgs kpea)
        {
            if (kpea.KeyChar != 13) return;
            kpea.Handled = true;
            Next((IParameterBox) o).Focus();
        }

        private void scroll(object o, ScrollEventArgs sea)
        {
            VerticalScroll.Value = sea.NewValue;
        }

        private void sizechanged(object o, EventArgs ea)
        {
            VerticalScroll.Visible = totalcontrolheight > Height;
        }

        private void vehiclesetchanged(object o, EventArgs ea)
        {
            List<VehicleProductionItem> productionitemlist = VehicleSetData.GetVehicleSet(vehicleset);
            IEnumerable<IParameterBox> vehicleparameterboxes =
                iparameterboxlist.Where(iparameterbox => iparameterbox.Tag is Type);

            foreach (IParameterBox iparameterbox in vehicleparameterboxes)
            {
                iparameterbox.Enabled = productionitemlist.Count(
                    productionitem =>
                    productionitem.VehicleType == iparameterbox.Tag as Type) > 0;
            }
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