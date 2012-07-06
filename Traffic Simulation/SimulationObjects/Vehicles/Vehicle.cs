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
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    public abstract class Vehicle : SimulationObject
    {
        #region FieldsAndProperties

        [OptionalField] protected double acceleration;
        [OptionalField] protected double bezierfactor;
        [OptionalField] protected int bezierindex;
        [OptionalField] protected PointF[] carpoints;
        [OptionalField] protected PointF[] centerpoint;
        [OptionalField] protected Connection connection;
        [OptionalField] protected CrossingLane crossinglane;
        [OptionalField] protected PointF[] curvepoints;
        [OptionalField] protected double deceleration;
        [OptionalField] protected RoadLane destinationlane;
        [OptionalField] protected bool destinationreached;
        [OptionalField] protected double distancedriven;
        [OptionalField] protected object drawlock;
        [OptionalField] protected Lane lane;
        [OptionalField] protected PointD location;
        [OptionalField] protected VehicleModelData modeldata;
        [OptionalField] protected RoadLane roadlane;
        [OptionalField] protected SizeD size;
        [NonSerialized] protected SolidBrush solidbrush;
        [OptionalField] protected double speed;
        [OptionalField] protected double speedfactor;
        [OptionalField] protected int targetlaneindex;
        [OptionalField] protected double targetspeed;
        [OptionalField] protected double time;
        [OptionalField] protected double totalmovingseconds;
        [OptionalField] protected double totalseconds;
        [OptionalField] protected TrafficLight trafficlight;
        [OptionalField] protected string type;
        [OptionalField] protected VectorD velocity;

        public PointD Location
        {
            get { return location; }
        }

        public double BackTimeIndex
        {
            get { return BackTime + bezierindex; }
        }

        public double DistanceDriven
        {
            get { return distancedriven; }
        }

        public double FrontTimeIndex
        {
            get { return FrontTime + bezierindex; }
        }

        public double Speed
        {
            get { return speed; }
        }

        public double TimeIndex
        {
            get { return Time + bezierindex; }
        }

        public double TotalMovingSeconds
        {
            get { return totalmovingseconds; }
        }

        public double TotalSeconds
        {
            get { return totalseconds; }
        }

        private double BackTime
        {
            get { return Time - size.Length/(2*lane.Beziers[bezierindex].Length); }
        }

        private double FrontTime
        {
            get { return Time + size.Length/(2*lane.Beziers[bezierindex].Length); }
        }

        private double Time
        {
            get
            {
                if (connection == null)
                    return time;
                return connection.StartTime + (connection.EndTime - connection.StartTime)*time;
            }
        }

        #endregion

        protected Vehicle(int id, VehicleModelData modeldata)
        {
            acceleration = modeldata.Acceleration;
            deceleration = modeldata.Deceleration;
            drawlock = new object();
            this.id = id;
            this.modeldata = modeldata;
            name = modeldata.Name;
            size = modeldata.Size;
            solidbrush =
                new SolidBrush(Color.FromArgb(Randomizer.Next(0, 256), Randomizer.Next(0, 256), Randomizer.Next(0, 256)));
            type = GetType().Name;
        }

        public void EnterLane(RoadLane lane, RoadLane destinationlane, int bezierindex = 0, double time = 0)
        {
            // Location
            this.bezierindex = bezierindex;
            this.time = time;
            setlane(lane, true);
            location = this.lane.Beziers[bezierindex].GetCoordinate(time);
            velocity = this.lane.Beziers[bezierindex].GetVelocity(time);

            // Destination
            this.destinationlane = destinationlane;
            targetlaneindex = roadlane.GetLaneIndexTo(destinationlane.GetLastCoordinate());

            // Speed
            speedfactor = Math.Abs(Randomizer.NextNormal(ParameterPanel.μSpeed, ParameterPanel.σSpeed));
            targetspeed = Math.Min(speedfactor*roadlane.Road.GetMaximumSpeed(type), modeldata.TopSpeed);
            speed = targetspeed;
            bezierfactor = speed/this.lane.Beziers[bezierindex].Length;
        }

        protected void setlane(Lane lane, bool changeilanecontainer)
        {
            // This methods sets a lane. This isn't accomplished by property because of intended side-effects:
            // Crossinglane and roadlane also get changed. We save these two to not always have to cast in case
            // we want to know if Vehicle is on a crossinglane or roadlane.
            
            if (this.lane == lane)
                return;

            // Remove the Vehicle from the Vehiclelist of Lane. If ChangeIlaneContainer, then also remove Vehicle from the
            // IlaneContainer of the Lane.
            if (this.lane != null)
                this.lane.LeaveLane(this, changeilanecontainer);

            crossinglane = lane as CrossingLane;
            this.lane = lane;
            roadlane = lane as RoadLane;

            trafficlight = roadlane == null ? null : roadlane.TrafficLight;

            if (this.lane != null)
                this.lane.EnterLane(this);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", id, type);
        }

        #region Next

        protected virtual double getspeedchange(double seconds, Vehicle nextvehicle)
        {
            double trafficlightdistance = 0;
            double vehicledistance = 0;

            if (nextvehicle != null) // Er is een voorganger. Bepaal de afstand tot hem.
            {
                if (lane.BelongsToSameILaneContainer(nextvehicle.lane)) // Ze rijden op dezelfde ILaneContainer.
                {
                    if (bezierindex == nextvehicle.bezierindex)
                        // Ze rijden op dezelfde bezier (of op beziers die naast elkaar liggen)
                        vehicledistance = nextvehicle.BackTime - FrontTime;
                    else if (bezierindex == nextvehicle.bezierindex - 1)
                        // NextVehicle rijdt op de volgende bezier (of op een bezier die naast de volgende bezier ligt)
                        vehicledistance = nextvehicle.BackTime*
                                          nextvehicle.lane.Beziers[nextvehicle.bezierindex].Length/
                                          lane.Beziers[bezierindex].Length + (1 - FrontTime);
                    else // NextVehicle rijdt de ver voor ons, zodat we 'm net zo goed kunnen negeren.
                        nextvehicle = null;
                }
                else // NextVehicle rijdt op de volgende Lane
                {
                    if (bezierindex == lane.Beziers.Length - 1 && nextvehicle.bezierindex == 0)
                        // Wij rijden op de laatste Bezier van onze Lane en NextVehicle rijdt op de eerste Bezier van zijn Lane
                        vehicledistance = nextvehicle.BackTime*nextvehicle.lane.Beziers[0].Length/
                                          lane.Beziers[bezierindex].Length + (1 - FrontTime);
                    else // NextVehicle rijdt de ver voor ons, zodat we 'm net zo goed kunnen negeren.
                        nextvehicle = null;
                }
            }

            if (trafficlight != null)
                trafficlightdistance = lane.Beziers.Length - FrontTimeIndex;

            if (nextvehicle == null) // Er is geen voorganger. Bepaal de snelheidsverandering.
            {
                if (trafficlight == null) // Er is geen verkeerslicht. Bepaal de snelheidsverandering.
                    return getnextspeedwithoutnextvehicle(seconds);
                if (trafficlight.TrafficLightState != TrafficLightState.Green &&
                    (trafficlightdistance < 5*speed/lane.Beziers[bezierindex].Length ||
                     trafficlightdistance < 5/lane.Beziers[bezierindex].Length))
                {
                    // We zijn vlakbij het verkeerslicht.

                    if (trafficlightdistance < speed*speed/lane.Beziers[bezierindex].Length/deceleration)
                        // We zijn vlakbij en kunnen niet meer redelijkerwijs stoppen.
                        return getnextspeedwithoutnextvehicle(seconds); // Rij door.
                    if (trafficlightdistance < 5/lane.Beziers[bezierindex].Length)
                        // We zijn op minder dan 5 meter afstand.
                        return -speed; // Ga stil staan.
                    return -Math.Min(speed, deceleration*seconds); // Rem
                }
                return getnextspeedwithoutnextvehicle(seconds); // Rij door
            }
            if (trafficlight == null) // Er is geen verkeerslicht. Bepaal de snelheidsverandering.
                return getspeedchangewithnextvehiclecloseby(seconds, vehicledistance, nextvehicle.Speed);
            if (trafficlight.TrafficLightState != TrafficLightState.Green &&
                (trafficlightdistance < 5*speed/lane.Beziers[bezierindex].Length ||
                 trafficlightdistance < 5/lane.Beziers[bezierindex].Length))
            {
                // We zijn vlakbij het verkeerslicht.

                if (trafficlightdistance < speed*speed/lane.Beziers[bezierindex].Length/deceleration)
                    // We zijn vlakbij en kunnen niet meer redelijkerwijs stoppen.
                    return getspeedchangewithnextvehiclecloseby(seconds, vehicledistance, nextvehicle.Speed);
                // Rij door.
                if (trafficlightdistance < 5/lane.Beziers[bezierindex].Length)
                    // We zijn op minder dan 5 meter afstand.
                    return -speed; // Ga stil staan.
                return -Math.Min(speed, deceleration*seconds); // Rem
            }
            return getspeedchangewithnextvehiclecloseby(seconds, vehicledistance, nextvehicle.Speed);
            // Rij door
        }

        protected virtual double getnextspeedwithoutnextvehicle(double seconds)
        {
            return speed < targetspeed ? Math.Min(targetspeed - speed, acceleration*seconds) : 0;
        }

        protected virtual double getspeedchangewithnextvehiclecloseby(double seconds, double vehicledistance,
                                                                      double nextvehiclespeed)
        {
            if (vehicledistance < 3/lane.Beziers[bezierindex].Length ||
                (vehicledistance < speed/lane.Beziers[bezierindex].Length && speed >= 0.95*nextvehiclespeed))
                return -Math.Min(deceleration*seconds, speed - 0.95*nextvehiclespeed);
            // Als we heel dichtbij zijn remmen we hard.
            if (vehicledistance < 2*speed/lane.Beziers[bezierindex].Length && speed >= nextvehiclespeed)
                return -Math.Min(deceleration*seconds, speed - 0.98*nextvehiclespeed);
            // Als we iets minder dichtbij zijn remmen we wat minder hard.
            return speed < targetspeed ? Math.Min(targetspeed - speed, acceleration*seconds) : 0;
        }

        protected virtual void changelane() // Wissel van baan
        {
            // Bepaal de Lane waar we heen willen wisselen.
            RoadLane nextlane = targetlaneindex < lane.LaneIndex ? roadlane.LeftLane : roadlane.RightLane;

            if (nextlane == null)
                return;

            double distance = roadlane.Road.GetMaximumSpeed("Vehicle")/lane.Beziers[bezierindex].Length/2;
            Vehicle[] vehicles = nextlane.GetVehiclesBetween(BackTimeIndex - distance, FrontTimeIndex + distance);
            // Zoek het overige verkeer.

            // Als we een gevaar voor het overige verkeer zouden zijn doen we het niet.
            if (!(vehicles.Length == 0 ||
                  (vehicles.Length == 1 &&
                   ((vehicles[0].FrontTimeIndex < BackTimeIndex && 0.98*speed > vehicles[0].Speed) ||
                    (vehicles[0].BackTimeIndex > FrontTimeIndex && speed < 0.98*vehicles[0].Speed)))))
                return;

            connection = new Connection(roadlane, nextlane, location, velocity, bezierindex, bezierfactor, speed, time);

            // Als we over verschillende Lanen moeten rijden of de dicht bij een Crossing zijn doen we het niet.
            if ((connection.EndBezierIndex == roadlane.Beziers.Length && roadlane.NextLaneCount > 0) ||
                (connection.EndBezierIndex == roadlane.Beziers.Length - 1 && roadlane.NextLaneCount > 0 &&
                 connection.EndTime > 1 - 50/connection.EndLane.Beziers[connection.EndBezierIndex].Length))
            {
                connection = null;
                return;
            }

            connection.EndLane.EnterLane(this);
            time = 0;
        }

        private void changelanewindup()
        {
            time -= 1;
            time /= bezierfactor;
            bezierindex = connection.EndBezierIndex;
            bezierfactor = speed/lane.Beziers[bezierindex].Length;
            time = connection.EndTime + time*bezierfactor;
            setlane(connection.EndLane, false);
            connection = null;
        }

        private bool nextbezier()
        {
            if (bezierindex == lane.Beziers.Length - 1)
            {
                Lane nextlane = lane.GetNextLane();

                if (nextlane == null)
                {
                    destinationreached = lane == destinationlane;
                    setlane(null, true);
                    return true;
                }

                // setlane na deze statements anders werkt het sorteren niet.

                time -= 1;
                time /= bezierfactor;
                bezierindex = 0;
                bezierfactor = speed/nextlane.Beziers[bezierindex].Length;
                time *= bezierfactor;

                setlane(nextlane, true);

                if (roadlane != null)
                {
                    targetlaneindex = roadlane.GetLaneIndexTo(destinationlane.GetLastCoordinate());
                    targetspeed = Math.Min(speedfactor*roadlane.Road.GetMaximumSpeed(type), modeldata.TopSpeed);
                }
                else
                    targetlaneindex = lane.LaneIndex;
            }
            else
            {
                time -= 1;
                time /= bezierfactor;
                bezierindex++;
                bezierfactor = speed/lane.Beziers[bezierindex].Length;
                time *= bezierfactor;
            }

            return false;
        }

        public virtual VehicleNext Next(double seconds)
        {
            totalseconds += seconds;

            if (speed > 0)
            {
                totalmovingseconds += seconds;

                if (connection == null && targetlaneindex != lane.LaneIndex)
                    changelane();
            }

            if (connection == null || time >= 0.5)
                speed += getspeedchange(seconds, lane.GetNextVehicle(this));
            else
            {
                if (time < 0.5)
                {
                    double endlanechange = getspeedchange(seconds, connection.EndLane.GetNextVehicle(this));
                    double startlanechange = getspeedchange(seconds, connection.StartLane.GetNextVehicle(this));
                    speed += Math.Min(startlanechange, endlanechange);
                }
                else
                {
                    setlane(connection.EndLane, false);
                    speed += getspeedchange(seconds, connection.EndLane.GetNextVehicle(this));
                }
            }

            distancedriven += seconds*speed;

            if (connection == null)
            {
                bezierfactor = speed/lane.Beziers[bezierindex].Length;
                time += bezierfactor*seconds;

                if (time > 1 && nextbezier())
                    return new VehicleNext(true, seconds*speed, destinationreached);
            }
            else
            {
                bezierfactor = speed/connection.Bezier.Length;
                time += bezierfactor*seconds;

                if (time > 1)
                    changelanewindup();
            }

            if (connection == null)
            {
                location = lane.Beziers[bezierindex].GetCoordinate(time);
                velocity = lane.Beziers[bezierindex].GetVelocity(time);
            }
            else
            {
                location = connection.Bezier.GetCoordinate(time);
                velocity = connection.Bezier.GetVelocity(time);
            }

            Monitor.Enter(drawlock);
            CalculateDrawPoints();
            Monitor.Exit(drawlock);

            return new VehicleNext(false, seconds*speed, false);
        }

        #endregion

        #region Drawing

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;
            var rotate = new Matrix();
            float factor = matrix.Elements[0];

            centerpoint = new PointF[] {location};
            matrix.TransformPoints(centerpoint);

            rotate.Rotate((float) Geometrics.RadianToDegree(Math.Atan2(velocity.Y, velocity.X)) + 90);
            carpoints = new PointF[]
                {
                    new PointD(-size.Width*factor/2, -size.Length*factor/2),
                    new PointD(size.Width*factor/2, -size.Length*factor/2),
                    new PointD(size.Width*factor/2, size.Length*factor/2),
                    new PointD(-size.Width*factor/2, size.Length*factor/2)
                };
            rotate.TransformPoints(carpoints);

            for (int i = 0; i < 4; i++)
                carpoints[i] = new PointF(carpoints[i].X + centerpoint[0].X, carpoints[i].Y + centerpoint[0].Y);

            if (connection == null) return;
            curvepoints = connection.Bezier.GetDrawPoints();
            matrix.TransformPoints(curvepoints);
        }

        public override void Draw(PaintEventArgs pea)
        {
            Monitor.Enter(drawlock);
            if (carpoints != null)
                pea.Graphics.FillPolygon(solidbrush, carpoints);
            Monitor.Exit(drawlock);
        }

        public void DrawChangeCurve(PaintEventArgs pea)
        {
            Monitor.Enter(drawlock);
            if (connection != null && curvepoints != null)
                pea.Graphics.DrawBezier(Pens.Black, curvepoints[0], curvepoints[1], curvepoints[2], curvepoints[3]);
            Monitor.Exit(drawlock);
        }

        public void DrawDestination(PaintEventArgs pea)
        {
            destinationlane.DrawDestinationLine(pea, solidbrush);
        }

        public void DrawText(PaintEventArgs pea, Font font)
        {
            Monitor.Enter(drawlock);
            if (centerpoint != null)
            {
                string text = string.Empty;

                if (ParameterPanel.ShowIds)
                    text = string.Format("{0} ", id);
                if (ParameterPanel.ShowNames)
                    text = string.Format("{0}{1}", text, name);

                pea.Graphics.DrawString(text, font, Brushes.Black, centerpoint[0]);
            }
            Monitor.Exit(drawlock);
        }

        #endregion
    }
}