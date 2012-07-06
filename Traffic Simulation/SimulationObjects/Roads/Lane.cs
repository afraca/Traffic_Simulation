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
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (Lane))]
    public abstract class Lane : SimulationObject
    {
        private static int laneid;
        [OptionalField] protected Bezier[] beziers;
        [OptionalField] protected Color curvecolor;
        [NonSerialized] protected Pen curvepen;
        [OptionalField] protected PointF[][] drawpoints;
        [OptionalField] private ILaneContainer ilanecontainer;
        [OptionalField] private LaneDirection lanedirection;
        [OptionalField] private int laneindex;
        [OptionalField] private LaneType lanetype;
        [OptionalField] private double length;
        [OptionalField] protected List<Lane> nextlanes;
        [NonSerialized] protected Pen pen;
        [OptionalField] protected float pointsize;
        [OptionalField] protected List<Lane> previouslanes;
        [OptionalField] protected List<Vehicle> vehiclelist;
        [OptionalField] private double width;

        protected Lane()
        {
            nextlanes = new List<Lane>();
            previouslanes = new List<Lane>();
            id = ++laneid;
        }

        public Bezier[] Beziers
        {
            get { return beziers; }
        }

        protected ILaneContainer ILaneContainer
        {
            get { return ilanecontainer; }
            set { ilanecontainer = value; }
        }

        public LaneDirection LaneDirection
        {
            get { return lanedirection; }
            protected set { lanedirection = value; }
        }

        public LaneType LaneType
        {
            get { return lanetype; }
            protected set { lanetype = value; }
        }

        public double Length
        {
            get { return length; }
            protected set { length = value; }
        }

        public double Width
        {
            get { return width; }
            protected set { width = value; }
        }

        public int LaneIndex
        {
            get { return laneindex; }
            protected set { laneindex = value; }
        }

        public int NextLaneCount
        {
            get { return nextlanes.Count; }
        }

        public int PreviousLaneCount
        {
            get { return previouslanes.Count; }
        }

        public bool BelongsToSameILaneContainer(Lane lane)
        {
            return ILaneContainer == lane.ILaneContainer;
        }

        public Vehicle[] GetVehiclesBetween(double min, double max)
        {
            return
                vehiclelist.Where(vehicle => vehicle.FrontTimeIndex > min && vehicle.BackTimeIndex < max).
                    ToArray();
        }

        public int GetVehicleCountBetween(double min, double max)
        {
            return vehiclelist.Count(vehicle => vehicle.FrontTimeIndex > min && vehicle.BackTimeIndex < max);
        }

        public Vehicle GetNextVehicle(Vehicle vehicle)
        {
            int index = vehiclelist.IndexOf(vehicle);

            if (index == vehiclelist.Count - 1)
                return nextlanes.Count > 0 ? nextlanes[0].GetFirstVehicle() : null;
            return vehiclelist[index + 1];
        }

        private Vehicle GetFirstVehicle()
        {
            return vehiclelist.Count > 0 ? vehiclelist[0] : null;
        }

        public PointD GetFirstCoordinate()
        {
            return beziers[0].GetCoordinate(0);
        }

        public VectorD GetFirstVelocity()
        {
            return beziers[0].GetVelocity(0);
        }

        public PointD GetLastCoordinate()
        {
            return beziers[beziers.Length - 1].GetCoordinate(1);
        }

        public VectorD GetLastVelocity()
        {
            return beziers[beziers.Length - 1].GetVelocity(1);
        }

        public bool IsToward(PointD point)
        {
            return GetFirstCoordinate().DistanceTo(point) > GetLastCoordinate().DistanceTo(point);
        }

        public PointD GetTowardCoordinate(Point point)
        {
            if ((IsToward(point) && LaneDirection == LaneDirection.Forward) ||
                (!IsToward(point) && LaneDirection == LaneDirection.Backward))
                return GetLastCoordinate();
            return GetFirstCoordinate();
        }

        public void AddNextLane(Lane nextlane)
        {
            nextlanes.Add(nextlane);
        }

        public void AddPreviousLane(Lane nextlane)
        {
            previouslanes.Add(nextlane);
        }

        public virtual void EnterLane(Vehicle vehicle)
        {
            if (vehiclelist.Contains(vehicle))
                return;

            for (int i = 0; i < vehiclelist.Count; i++)
            {
                if (!(vehiclelist[i].TimeIndex > vehicle.TimeIndex)) continue;
                vehiclelist.Insert(i, vehicle);
                return;
            }
            vehiclelist.Add(vehicle);
        }

        public virtual void LeaveLane(Vehicle vehicle, bool changeilanecontainer)
        {
            vehiclelist.Remove(vehicle);
        }

        public void ClearVehicleList()
        {
            vehiclelist.Clear();
        }

        public Lane GetNextLane()
        {
            return nextlanes.Count > 0 ? nextlanes[Randomizer.Next(0, nextlanes.Count)] : null;
        }

        public Lane GetNextLaneTo(PointD point)
        {
            foreach (
                CrossingLane crossinglane in
                    nextlanes.OfType<CrossingLane>().Where(
                        crossinglane => crossinglane.NextLane.GetLaneIndexTo(point) == crossinglane.LaneIndex))
            {
                return crossinglane;
            }

            return GetNextLane();
        }

        public override void Draw(PaintEventArgs pea)
        {
            foreach (var points in drawpoints)
                pea.Graphics.DrawBezier(pen, points[0], points[1], points[2], points[3]);
        }

        public void DrawCurve(PaintEventArgs pea)
        {
            foreach (var points in drawpoints)
                pea.Graphics.DrawBezier(curvepen, points[0], points[1], points[2], points[3]);
        }

        public void DrawPoints(PaintEventArgs pea)
        {
            foreach (PointF point in drawpoints.SelectMany(points => points))
                pea.Graphics.FillRectangle(new SolidBrush(curvecolor), point.X - pointsize, point.Y - pointsize,
                                           2*pointsize, 2*pointsize);
        }

        public override string ToString()
        {
            string s = string.Format("{0}: {1}", GetType().Name, id);

            if (!string.IsNullOrEmpty(name))
                s += string.Format(" - {0}", name);

            s += string.Format(" - {0} - {1}", LaneDirection, LaneIndex);

            return s;
        }
    }
}