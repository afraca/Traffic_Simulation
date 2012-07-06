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
using System.Linq;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    public class Simulation
    {
        #region FieldsAndProperties

        private readonly bool buggedmap;
        private readonly List<Building> buildinglist;
        private readonly List<Crossing> crossinglist;
        private readonly List<Road> roadlist;
        private readonly List<RoadLane> spawnlanelist;
        private readonly List<RoadLane> unspawnlanelist;
        private readonly VehicleFactory vehiclefactory;
        private int destinationmissed;
        private int destinationreached;

        [NonSerialized] private Font font;
        private double framerate;
        [NonSerialized] private MainForm mainform;
        [NonSerialized] private MainMenu mainmenu;
        [NonSerialized] private ManualResetEvent manualresetevent;
        private double movingfractionsum;
        [NonSerialized] private Thread simulationthread;
        private int stepcount;
        [NonSerialized] private TaskMenu taskmenu;
        [NonSerialized] private TimeWatch timewatch;
        private double totaldistance;
        private double totalseconds;
        private List<Vehicle> vehiclelist;

        public MainMenu MainMenu
        {
            get { return mainmenu; }
            private set { mainmenu = value; }
        }

        public TaskMenu TaskMenu
        {
            get { return taskmenu; }
            private set { taskmenu = value; }
        }

        private bool FileNameEntered
        {
            get { return FileName.Contains("\\"); }
        }

        public double XMin { get; private set; }
        public double XMax { get; private set; }
        public double YMin { get; private set; }
        public double YMax { get; private set; }
        private int Id { get; set; }
        public string FileName { get; private set; }

        private double lanelength
        {
            get { return roadlist.Sum(road => road.TotalLaneLength); }
        }

        private double meanspeed
        {
            get { return vehiclelist.Sum(vehicle => vehicle.Speed)/vehiclelist.Count; }
        }

        private double movingfraction
        {
            get
            {
                if (vehiclelist.Count > 0)
                    return 1 - 1.0*vehiclelist.Count(vehicle => vehicle.Speed == 0)/vehiclelist.Count;
                return 1;
            }
        }

        private double roadlength
        {
            get { return roadlist.Sum(road => road.Length); }
        }

        private int lanecount
        {
            get { return roadlist.Sum(road => road.LaneCount); }
        }

        #endregion

        public Simulation(MainForm mainform, int id)
        {
            buildinglist = new List<Building>();
            crossinglist = new List<Crossing>();
            FileName = string.Format("Simulation {0}", id);
            Id = id;
            this.mainform = mainform;
            manualresetevent = new ManualResetEvent(false);
            roadlist = new List<Road>();
            simulationthread = new Thread(simulate);
            spawnlanelist = new List<RoadLane>();
            timewatch = new TimeWatch();
            unspawnlanelist = new List<RoadLane>();
            vehiclefactory = new VehicleFactory();
            vehiclelist = new List<Vehicle>();
            buggedmap = false;

            makemenus();
            makemap();
            calculateborderpoints();

            foreach (Road road in roadlist)
            {
                spawnlanelist.AddRange(road.GetSpawnLanes());
                unspawnlanelist.AddRange(road.GetUnSpawnLanes());
            }

            matrixchanged(null, new EventArgs());
            subscribeeventhandlers();
            resetsimulation();

            simulationthread.Start();
            mainform.ExtendedStatusStrip.UpdateStatus("Simulation loaded.");
        }

        #region Other

        private void calculateborderpoints()
        {
            XMax =
                Math.Max(
                    Math.Max(roadlist.Max(road => road.GetFirstCoordinate().X),
                             roadlist.Max(road => road.GetLastCoordinate().X)),
                    crossinglist.Max(crossing => crossing.Location.X));
            XMin =
                Math.Min(
                    Math.Min(roadlist.Min(road => road.GetFirstCoordinate().X),
                             roadlist.Min(road => road.GetLastCoordinate().X)),
                    crossinglist.Min(crossing => crossing.Location.X));
            YMax =
                Math.Max(
                    Math.Max(roadlist.Max(road => road.GetFirstCoordinate().Y),
                             roadlist.Max(road => road.GetLastCoordinate().Y)),
                    crossinglist.Max(crossing => crossing.Location.Y));
            YMin =
                Math.Min(
                    Math.Min(roadlist.Min(road => road.GetFirstCoordinate().Y),
                             roadlist.Min(road => road.GetLastCoordinate().Y)),
                    crossinglist.Min(crossing => crossing.Location.Y));
        }

        private void makemenus()
        {
            MainMenu = new MainMenu(this, new MenuData("File", "Close", "Save", "Save As"),
                                    new MenuData("Edit", "Start", "Stop", "Reset", "Next Step"));
            TaskMenu = new TaskMenu(this, "Start", "Stop", "Reset", "Next Step", "Zoom In", "Zoom Out");

            MainMenu.GetToolStripItem("Stop").Enabled = false;
            TaskMenu.GetToolStripItem("Stop").Enabled = false;
        }

        private void changeiparameterboxenabled(bool enabled)
        {
            mainform.ParameterPanel.SetIParameterBox("VehicleSet", enabled);
        }

        public Simulation DeserializationRepopulation(MainForm mainform)
        {
            this.mainform = mainform;
            timewatch = new TimeWatch();
            font = new Font("Calibri", 2*ParameterPanel.Matrix.Elements[0]);
            manualresetevent = new ManualResetEvent(false);
            makemenus();
            calculateborderpoints();
            matrixchanged(null, new EventArgs());
            subscribeeventhandlers();
            resetsimulation();
            simulationthread = new Thread(simulate);
            simulationthread.Start();
            mainform.ExtendedStatusStrip.UpdateStatus("Simulation loaded.");
            return this;
        }

        #endregion

        #region EventHandlerSubscription

        private void subscribeeventhandlers()
        {
            // EventHandlers die we willen hebben als de Simulation wordt gemaakt

            ParameterPanel.CenterVehicleChanged += centervehicle;
            ParameterPanel.ShowChangeCurveChanged += redraw;
            ParameterPanel.ShowCurvesChanged += redraw;
            ParameterPanel.ShowDestinationChanged += redraw;
            ParameterPanel.ShowIdsChanged += redraw;
            ParameterPanel.ShowNamesChanged += redraw;
            ParameterPanel.ShowPointsChanged += redraw;
            mainform.ParameterPanel.MatrixChanged += matrixchanged;

            MainMenu.GetToolStripItem("Start").Click += simulationrunchanged;
            TaskMenu.GetToolStripItem("Start").Click += simulationrunchanged;
            MainMenu.GetToolStripItem("Stop").Click += simulationrunchanged;
            TaskMenu.GetToolStripItem("Stop").Click += simulationrunchanged;
        }

        private void unsubscribeeventhandlers()
        {
            // EventHandlers die we moeten unsubscriben als de Simulation wordt gesloten

            ParameterPanel.ShowChangeCurveChanged -= redraw;
            ParameterPanel.ShowCurvesChanged -= redraw;
            ParameterPanel.ShowDestinationChanged -= redraw;
            ParameterPanel.ShowIdsChanged -= redraw;
            ParameterPanel.ShowNamesChanged -= redraw;
            ParameterPanel.ShowPointsChanged -= redraw;
            mainform.ParameterPanel.MatrixChanged -= matrixchanged;

            MainMenu.GetToolStripItem("Start").Click -= simulationrunchanged;
            TaskMenu.GetToolStripItem("Start").Click -= simulationrunchanged;
            MainMenu.GetToolStripItem("Stop").Click -= simulationrunchanged;
            TaskMenu.GetToolStripItem("Stop").Click -= simulationrunchanged;
        }

        #endregion

        #region SimulationEventHandlers

        public void NextStep_Click(object o, EventArgs ea)
        {
            changeiparameterboxenabled(false);

            timewatch.Reset();
            timewatch.SetTime(ParameterPanel.NextStep);

            manualresetevent.Set();
            Thread.Sleep(1);
            manualresetevent.Reset();
        }

        public void Reset_Click(object o, EventArgs ea)
        {
            if (
                MessageBox.Show(
                    "You are about to reset the Simulation. All progress will be lost.\nDo you want to continue?",
                    "Reset Simulation", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1) == DialogResult.OK)
                resetsimulation();
        }

        private void resetsimulation()
        {
            destinationmissed = 0;
            destinationreached = 0;
            movingfractionsum = 0;
            stepcount = 0;
            totalseconds = 0;

            foreach (Road road in roadlist)
                road.ClearVehicleList();

            mainform.StatisticsPanel.RemoveVehicles(vehiclelist);
            mainform.StatisticsPanel.SetStatistics();

            mainform.StatisticsPanel.LaneCount = lanecount;
            mainform.StatisticsPanel.LaneLength = lanelength;
            mainform.StatisticsPanel.RoadCount = roadlist.Count;
            mainform.StatisticsPanel.RoadLength = roadlength;

            vehiclelist = new List<Vehicle>();

            changeiparameterboxenabled(true);

            timewatch.Reset();
            vehiclefactory.Reset();

            matrixchanged(null, new EventArgs());

            mainform.ExtendedStatusStrip.UpdateStatus("Simulation reset.");
        }

        private void simulationrunchanged(object o, EventArgs ea)
        {
            bool enable = ((ToolStripItem) o).Text == "Start";

            MainMenu.GetToolStripItem("Next Step").Enabled = !enable;
            MainMenu.GetToolStripItem("Reset").Enabled = !enable;
            MainMenu.GetToolStripItem("Start").Enabled = !enable;
            MainMenu.GetToolStripItem("Stop").Enabled = enable;

            TaskMenu.GetToolStripItem("Next Step").Enabled = !enable;
            TaskMenu.GetToolStripItem("Reset").Enabled = !enable;
            TaskMenu.GetToolStripItem("Start").Enabled = !enable;
            TaskMenu.GetToolStripItem("Stop").Enabled = enable;
        }

        public void Start_Click(object o, EventArgs ea)
        {
            mainform.ExtendedStatusStrip.UpdateStatus("Simulation started");

            changeiparameterboxenabled(false);

            timewatch.Start();
            manualresetevent.Set();
        }

        public void Stop_Click(object o, EventArgs ea)
        {
            mainform.ExtendedStatusStrip.UpdateStatus("Simulation stopped");
            manualresetevent.Reset();
        }

        public void ZoomIn_Click(object o, EventArgs ea)
        {
            mainform.ParameterPanel.Zoom(false);
        }

        public void ZoomOut_Click(object o, EventArgs ea)
        {
            mainform.ParameterPanel.Zoom(true);
        }

        private void centervehicle(object o, EventArgs ea)
        {
            if (mainform.StatisticsPanel.SelectedVehicle == null)
                return;

            mainform.ParameterPanel.UpdateIParameterBox("ViewX", mainform.StatisticsPanel.SelectedVehicle.Location.X);
            mainform.ParameterPanel.UpdateIParameterBox("ViewY", mainform.StatisticsPanel.SelectedVehicle.Location.Y);
        }

        private void matrixchanged(object o, EventArgs ea)
        {
            Monitor.Enter(ParameterPanel.MatrixLock);

            foreach (Building building in buildinglist)
                building.CalculateDrawPoints();

            foreach (Road road in roadlist)
            {
                road.CalculateDrawPoints();
                road.CalculateLaneDrawPoints();
            }

            foreach (Crossing crossing in crossinglist)
            {
                crossing.CalculateDrawPoints();
                crossing.CalculateLaneDrawPoints();
                crossing.CalculateTrafficLightDrawPoints();
            }

            if (!manualresetevent.WaitOne(0)) // Als de Simulation niet draait moeten we ook de Vehicles herberekenen
                foreach (Vehicle vehicle in vehiclelist)
                    vehicle.CalculateDrawPoints();

            font = new Font("Calibri", 2*ParameterPanel.Matrix.Elements[0]);
            Monitor.Exit(ParameterPanel.MatrixLock);

            mainform.SimulationPanel.Invalidate();
        }

        private void redraw(object o, EventArgs ea)
        {
            mainform.SimulationPanel.Invalidate();
        }

        #endregion

        #region MenuMethods

        public void Close_Click(object o, EventArgs ea)
        {
            Stop_Click(o, ea);

            if (!FileNameEntered)
            {
                string name = FileName.Substring(FileName.IndexOf("\\", StringComparison.Ordinal) + 1);

                if (name.Contains("."))
                    name.Substring(0, name.LastIndexOf(".", StringComparison.Ordinal));

                DialogResult dialogresult = MessageBox.Show(string.Format("Save changes in {0}?", name), "Save",
                                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (dialogresult == DialogResult.Cancel)
                    return;
            }

            simulationthread.Abort();
            unsubscribeeventhandlers();
            mainform.CloseSimulation();
        }

        public void Save_Click(object o, EventArgs ea)
        {
            if (FileNameEntered)
                save(FileName);
            else
                SaveAs_Click(o, ea);
        }

        private void SaveAs_Click(object o, EventArgs ea)
        {
            var savefiledialog = new SaveFileDialog {Filter = "Simulation-file|*.sml", Title = "Save Simulation As..."};

            if (savefiledialog.ShowDialog() != DialogResult.OK) return;
            FileName = savefiledialog.FileName;
            save(FileName);
        }

        private void save(string filename)
        {
            Serializer.Serialize(filename, this);
        }

        #endregion

        #region SimulationMethods

        public void Draw(PaintEventArgs pea)
        {
            // Teken in precies deze volgorde zodat alles op de juiste manier van onder naar boven te zien is.

            foreach (Building building in buildinglist)
                building.Draw(pea);

            foreach (Road road in roadlist)
                road.Draw(pea);

            foreach (Road road in roadlist)
                road.DrawLanes(pea);

            if (ParameterPanel.ShowCurves)
                foreach (Road road in roadlist)
                    road.DrawLaneCurves(pea);

            foreach (Road road in roadlist)
                road.DrawLine(pea);

            foreach (Crossing crossing in crossinglist)
                crossing.Draw(pea);

            if (ParameterPanel.ShowCurves)
                foreach (Crossing crossing in crossinglist)
                    crossing.DrawLaneCurves(pea);

            if (ParameterPanel.ShowPoints)
                foreach (Road road in roadlist)
                    road.DrawPoints(pea);

            if (ParameterPanel.ShowPoints)
                foreach (Road road in roadlist)
                    road.DrawLanePoints(pea);

            if (ParameterPanel.ShowPoints)
                foreach (Crossing crossing in crossinglist)
                    crossing.DrawLanePoints(pea);

            Monitor.Enter(vehiclelist);
            var drawvehiclelist = new List<Vehicle>(vehiclelist);
            Monitor.Exit(vehiclelist);

            foreach (Vehicle vehicle in drawvehiclelist)
                vehicle.Draw(pea);

            if (ParameterPanel.ShowIds || ParameterPanel.ShowNames)
                foreach (Vehicle vehicle in drawvehiclelist)
                    vehicle.DrawText(pea, font);

            if (ParameterPanel.ShowChangeCurve)
                foreach (Vehicle vehicle in drawvehiclelist)
                    vehicle.DrawChangeCurve(pea);

            foreach (Crossing crossing in crossinglist)
                crossing.DrawTrafficLights(pea);

            if (!ParameterPanel.ShowDestination) return;
            if (mainform.StatisticsPanel.SelectedVehicle != null)
                mainform.StatisticsPanel.SelectedVehicle.DrawDestination(pea);
        }

        private void next()
        {
            #region TimeUpdate

            double seconds = timewatch.CorrectedElapsed.TotalSeconds;
            timewatch.ResetTime();
            timewatch.Restart();
            framerate = 1/seconds;
            seconds *= ParameterPanel.SpeedFactor;
            totalseconds += seconds;

            #endregion

            #region SpawnVehicles

            var removevehiclehashset = new HashSet<Vehicle>();
            List<RoadLane> currentspawnlanelist = spawnlanelist.Where(roadlane => roadlane.CanSpawn).ToList();
            Vehicle[] vehicles = vehiclefactory.OrderVehicles(seconds, vehiclelist.Count, currentspawnlanelist.Count);

            foreach (Vehicle vehicle in vehicles) // Zet de nieuwe Vehicles op een Lane
            {
                RoadLane spawnlane = currentspawnlanelist[Randomizer.Next(0, currentspawnlanelist.Count)];
                RoadLane destinationlane = unspawnlanelist[Randomizer.Next(0, unspawnlanelist.Count)];
                vehicle.EnterLane(spawnlane, destinationlane);
                currentspawnlanelist.Remove(spawnlane);
            }

            // Voeg de nieuwe Vehicles toe aan de vehiclelist
            Monitor.Enter(vehiclelist);
            vehiclelist.AddRange(vehicles);
            Monitor.Exit(vehiclelist);

            mainform.StatisticsPanel.AddVehicles(vehicles);

            #endregion

            #region TrafficLightNext

            foreach (Crossing crossing in crossinglist)
                crossing.TrafficLightNext(seconds);

            #endregion

            #region VehicleNext

            Monitor.Enter(ParameterPanel.MatrixLock);
            foreach (Vehicle vehicle in vehiclelist)
            {
                VehicleNext vehiclenext = vehicle.Next(seconds);
                totaldistance += vehiclenext.Distance;

                if (!vehiclenext.Remove) continue;
                if (vehiclenext.DestinationReached)
                    destinationreached++;
                else
                    destinationmissed++;
                removevehiclehashset.Add(vehicle);
            }
            Monitor.Exit(ParameterPanel.MatrixLock);

            Monitor.Enter(vehiclelist);
            vehiclelist.RemoveAll(removevehiclehashset.Contains);
            Monitor.Exit(vehiclelist);

            mainform.StatisticsPanel.RemoveVehicles(removevehiclehashset);

            #endregion

            #region Statistics

            double movefrac = movingfraction;
            movingfractionsum += movefrac*seconds;
            mainform.StatisticsPanel.DestinationMissed = destinationmissed;
            mainform.StatisticsPanel.DestinationReached = destinationreached;
            mainform.StatisticsPanel.MeanMovingFraction = movingfractionsum/totalseconds;
            mainform.StatisticsPanel.MeanSpeed = 3.6*meanspeed;
            mainform.StatisticsPanel.MovingFraction = movefrac;
            mainform.StatisticsPanel.StepCount = ++stepcount;
            mainform.ExtendedStatusStrip.UpdateStepCounter(stepcount);
            mainform.StatisticsPanel.StepRate = framerate;
            mainform.StatisticsPanel.Time = totalseconds;
            mainform.StatisticsPanel.TotalDistance = totaldistance;
            mainform.StatisticsPanel.TotalVehicleCount = vehiclefactory.ProductionCount;
            mainform.StatisticsPanel.VehicleCount = vehiclelist.Count;
            mainform.StatisticsPanel.UpdateVehicleStatistics();

            #endregion

            #region TerminationCriterium

            double terminationvalue = mainform.StatisticsPanel.GetTerminationValue();

            if (!double.IsNaN(terminationvalue) &&
                ((ParameterPanel.TerminationType == TerminationType.Less &&
                  terminationvalue < ParameterPanel.TerminationValue) ||
                 (ParameterPanel.TerminationType == TerminationType.Greater &&
                  terminationvalue > ParameterPanel.TerminationValue) ||
                 (ParameterPanel.TerminationType == TerminationType.EqualOrLess &&
                  terminationvalue <= ParameterPanel.TerminationValue) ||
                 (ParameterPanel.TerminationType == TerminationType.EqualOrGreater &&
                  terminationvalue >= ParameterPanel.TerminationValue)))
            {
                TaskMenu.PerformClick("Stop");
                mainform.ExtendedStatusStrip.UpdateStatus(
                    "Termination Criterium reachted: {0} is {1} than {2}. Current value of {0} is {3}.",
                    ParameterPanel.TerminationStatistic, ParameterPanel.TerminationType, ParameterPanel.TerminationValue,
                    terminationvalue);
            }

            #endregion

            if (ParameterPanel.CenterVehicle)
                centervehicle(null, EventArgs.Empty);

            mainform.SimulationPanel.Invalidate();
        }

        private void simulate()
        {
            Randomizer.Initialize();

            while (true)
            {
                manualresetevent.WaitOne(); // Wacht tot je een stap mag doen
                next();

                if (ParameterPanel.StepDelay > 10) // Voer de stepdelay uit
                    Thread.Sleep(ParameterPanel.StepDelay);

                // Stop met het meten van de tijd die sinds het begin van de net uitgevoerde next is verstreken.
                timewatch.Stop();
            }
        }

        #endregion

        #region Map

        private void makemap()
        {
            var speeddictionary = new Dictionary<string, double>();

            var crossingdata = new CrossingData(0.15);

            // Snelheden van Vehicle typen
            speeddictionary.Add("Bus", 80/3.6);
            speeddictionary.Add("Car", 120/3.6);
            speeddictionary.Add("Motorcycle", 120/3.6);
            speeddictionary.Add("Truck", 80/3.6);
            speeddictionary.Add("Vehicle", 120/3.6);
            var roaddata = new RoadData(RoadType.Highway, speeddictionary, Color.Gray, Color.White, 4, 4, true, true,
                                        3.5, 3, 0.15, 9);

            // Bepaal via welke Lane je waarheen mag zoals beschreven in de PDF voor symmetrische Crossings

            var connectiondictionary1 = new Dictionary<int, HashSet<RoadLaneIndex>>();

            var roadlaneindexhashset = new HashSet<RoadLaneIndex>
                {new RoadLaneIndex(3, 0), new RoadLaneIndex(3, 1), new RoadLaneIndex(3, 2)};
            connectiondictionary1.Add(0, roadlaneindexhashset);

            roadlaneindexhashset = new HashSet<RoadLaneIndex>
                {new RoadLaneIndex(2, 0), new RoadLaneIndex(2, 1), new RoadLaneIndex(2, 2)};
            connectiondictionary1.Add(1, roadlaneindexhashset);

            roadlaneindexhashset = new HashSet<RoadLaneIndex>
                {new RoadLaneIndex(1, 0), new RoadLaneIndex(1, 1), new RoadLaneIndex(1, 2)};
            connectiondictionary1.Add(2, roadlaneindexhashset);

            roadlaneindexhashset = new HashSet<RoadLaneIndex> {new RoadLaneIndex(1, 3)};
            connectiondictionary1.Add(3, roadlaneindexhashset);

            // Leg de Roads neer. 

            if (!buggedmap)
            {
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-1450, -500, -550, -500)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-450, -500, 450, -500)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(550, -500, 1500, -500)}));

                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-1450, 500, -550, 500)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-450, 500, 450, 500)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(550, 500, 1500, 500)}));

                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-500, -1500, -500, -550)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-500, -450, -500, 450)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-500, 550, -500, 1500)}));

                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(500, -1500, 500, -550)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(500, -450, 500, 450)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(500, 550, 500, 1500)}));

                roadlist.Add(new Road(roaddata,
                                      new Bezier[]
                                          {
                                              new CubicBezier(-3500, -2500, -3000, -2500, -2500, -2000, -2000, -2000),
                                              new CircularBezier(-2000, -2000, -2000, -1500, -1500, -1500),
                                              new LinearBezier(-1500, -1500, -1500, -550)
                                          })); // 12
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-1500, -450, -1500, 450)}));
                roadlist.Add(new Road(roaddata,
                                      new Bezier[]
                                          {
                                              new LinearBezier(-1500, 550, -1500, 1500),
                                              new CircularBezier(-1500, 1500, -2000, 1500, -2000, 2000),
                                              new QuadraticBezier(-2000, 2000, -2500, 2000, -2500, 1500)
                                          }));

                // Voeg de Crossings toe.

                crossinglist.Add(new Crossing(-500, -500, crossingdata,
                                              new RoadConnection(roadlist[0], connectiondictionary1),
                                              new RoadConnection(roadlist[1], connectiondictionary1),
                                              new RoadConnection(roadlist[6], connectiondictionary1),
                                              new RoadConnection(roadlist[7], connectiondictionary1)));
                crossinglist.Add(new Crossing(500, -500, crossingdata,
                                              new RoadConnection(roadlist[1], connectiondictionary1),
                                              new RoadConnection(roadlist[2], connectiondictionary1),
                                              new RoadConnection(roadlist[9], connectiondictionary1),
                                              new RoadConnection(roadlist[10], connectiondictionary1)));
                crossinglist.Add(new Crossing(-500, 500, crossingdata,
                                              new RoadConnection(roadlist[3], connectiondictionary1),
                                              new RoadConnection(roadlist[4], connectiondictionary1),
                                              new RoadConnection(roadlist[7], connectiondictionary1),
                                              new RoadConnection(roadlist[8], connectiondictionary1)));
                crossinglist.Add(new Crossing(500, 500, crossingdata,
                                              new RoadConnection(roadlist[4], connectiondictionary1),
                                              new RoadConnection(roadlist[5], connectiondictionary1),
                                              new RoadConnection(roadlist[10], connectiondictionary1),
                                              new RoadConnection(roadlist[11], connectiondictionary1)));
            }

            else
            {
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-600, -500, -600, -50)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-600, 50, -600, 250)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-950, 0, -650, 0)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(-550, 0, -50, 0)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(50, 0, 350, 0)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(0, -250, 0, -50)}));
                roadlist.Add(new Road(roaddata, new Bezier[] {new LinearBezier(0, 50, 0, 600)}));

                crossinglist.Add(new Crossing(-600, 0, crossingdata,
                                              new RoadConnection(roadlist[0], connectiondictionary1),
                                              new RoadConnection(roadlist[1], connectiondictionary1),
                                              new RoadConnection(roadlist[2], connectiondictionary1),
                                              new RoadConnection(roadlist[3], connectiondictionary1)));
                crossinglist.Add(new Crossing(0, 0, crossingdata, new RoadConnection(roadlist[3], connectiondictionary1),
                                              new RoadConnection(roadlist[4], connectiondictionary1),
                                              new RoadConnection(roadlist[5], connectiondictionary1),
                                              new RoadConnection(roadlist[6], connectiondictionary1)));
            }

            if (!buggedmap)
            {
                //Bepaal via welke Lane je waarheen mag zoals beschreven in de PDF voor T Crossings.
                connectiondictionary1 = new Dictionary<int, HashSet<RoadLaneIndex>>();
                var connectiondictionary2 = new Dictionary<int, HashSet<RoadLaneIndex>>();
                var connectiondictionary3 = new Dictionary<int, HashSet<RoadLaneIndex>>();

                roadlaneindexhashset = new HashSet<RoadLaneIndex>
                    {new RoadLaneIndex(2, 0), new RoadLaneIndex(2, 1), new RoadLaneIndex(2, 2)};
                connectiondictionary1.Add(0, roadlaneindexhashset);
                connectiondictionary2.Add(0, roadlaneindexhashset);

                roadlaneindexhashset = new HashSet<RoadLaneIndex> {new RoadLaneIndex(1, 0), new RoadLaneIndex(1, 1)};
                connectiondictionary1.Add(1, roadlaneindexhashset);
                connectiondictionary2.Add(1, roadlaneindexhashset);

                roadlaneindexhashset = new HashSet<RoadLaneIndex> {new RoadLaneIndex(1, 2)};
                connectiondictionary1.Add(2, roadlaneindexhashset);
                connectiondictionary2.Add(2, roadlaneindexhashset);

                roadlaneindexhashset = new HashSet<RoadLaneIndex> {new RoadLaneIndex(1, 3)};
                connectiondictionary1.Add(3, roadlaneindexhashset);
                connectiondictionary2.Add(3, roadlaneindexhashset);
                connectiondictionary3.Add(3, roadlaneindexhashset);

                roadlaneindexhashset = new HashSet<RoadLaneIndex> {new RoadLaneIndex(2, 0)};
                connectiondictionary3.Add(0, roadlaneindexhashset);

                roadlaneindexhashset = new HashSet<RoadLaneIndex> {new RoadLaneIndex(2, 1), new RoadLaneIndex(2, 2)};
                connectiondictionary3.Add(1, roadlaneindexhashset);

                roadlaneindexhashset = new HashSet<RoadLaneIndex>
                    {new RoadLaneIndex(1, 0), new RoadLaneIndex(1, 1), new RoadLaneIndex(1, 2)};
                connectiondictionary3.Add(2, roadlaneindexhashset);


                crossinglist.Add(new Crossing(-1500, -500, crossingdata,
                                              new RoadConnection(roadlist[12], connectiondictionary1),
                                              new RoadConnection(roadlist[0], connectiondictionary2),
                                              new RoadConnection(roadlist[13], connectiondictionary3)));
                crossinglist.Add(new Crossing(-1500, 500, crossingdata,
                                              new RoadConnection(roadlist[13], connectiondictionary1),
                                              new RoadConnection(roadlist[3], connectiondictionary2),
                                              new RoadConnection(roadlist[14], connectiondictionary3)));
            }
            //bool dreamscametrue = false;
            //if (dreamscametrue)
            //{
            //    roadlist = (List<Road>)Serializer.Deserialize(Global.HomeFolder + "\\Maps\\Default_roadlist.xml", false);
            //    crossinglist = new List<Crossing>();
            //    foreach (Crossing crossing in (List<Crossing>)Serializer.Deserialize(Global.HomeFolder + "\\Maps\\Default_crossinglist.xml", false))
            //    {
            //        crossing.DeserializationRepopulation();
            //        crossinglist.Add(crossing);
            //    }
            //}


            // Voeg Buildings toe.

            #region LegacyBuildingConstruction

            /*
             * LEGACY
             * 
            buildinglist.Add(new Building(Color.Beige, -450, -300, -400, -300, -425, -100, -450, -100));
            buildinglist.Add(new Building(Color.Chocolate, -450, -50, -400, -50, -400, 0, -425, 0, -425, -25, -450, -25));\
             * 
             */

            #endregion

            foreach (
                Building building in
                    (List<Building>)
                    Serializer.Deserialize(Global.HomeFolder + "\\Maps\\Default_buildinglist.xml", false))
            {
                building.DeserializationRepopulation();
                buildinglist.Add(building);
            }
        }

        #endregion
    }
}