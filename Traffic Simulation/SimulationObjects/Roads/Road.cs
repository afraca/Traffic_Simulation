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
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (Road))]
    public class Road : SimulationObject, ILaneContainer
    {
        [OptionalField] private readonly Bezier[] beziers;
        [OptionalField] private readonly List<RoadLane> lanelist;
        [OptionalField] private readonly Color roadcolor;
        [OptionalField] private readonly RoadData roaddata;
        [OptionalField] private readonly List<Vehicle> vehiclelist;
        [OptionalField] private PointF[][] drawpoints;

        [OptionalField] private Color linecolor;
        [NonSerialized] private Pen linepen;
        [OptionalField] private ILaneContainer next;
        [OptionalField] private float pointsize;
        [OptionalField] private ILaneContainer previous;
        [NonSerialized] private Pen roadpen;
        [OptionalField] private PointD sortpoint;

        public Road(RoadData roaddata, Bezier[] beziers)
        {
            this.beziers = beziers;
            linecolor = roaddata.LineColor;
            lanelist = new List<RoadLane>();
            roadcolor = roaddata.RoadColor;
            this.roaddata = roaddata;
            vehiclelist = new List<Vehicle>();

            for (int i = 0; i < roaddata.ForwardLaneCount; i++)
                if (i == roaddata.ForwardLaneCount - 1 && roaddata.ForwardShoulder)
                    addlane(LaneDirection.Forward, LaneType.Shoulder);
                else
                    addlane(LaneDirection.Forward, LaneType.Normal);

            for (int i = 0; i < roaddata.BackwardLaneCount; i++)
                if (i == roaddata.BackwardLaneCount - 1 && roaddata.BackwardShoulder)
                    addlane(LaneDirection.Backward, LaneType.Shoulder);
                else
                    addlane(LaneDirection.Backward, LaneType.Normal);
        }

        public Bezier[] Beziers
        {
            get { return beziers; }
        }

        public Color RoadColor
        {
            get { return roaddata.RoadColor; }
        }

        public Color LineColor
        {
            get { return roaddata.LineColor; }
        }

        public ILaneContainer Next
        {
            get { return next; }
            private set { next = value; }
        }

        public ILaneContainer Previous
        {
            get { return previous; }
            private set { previous = value; }
        }

        public PointD SortPoint
        {
            get { return sortpoint; }
            set { sortpoint = value; }
        }

        public double DashLength
        {
            get { return roaddata.DashLength; }
        }

        public double DashWidth
        {
            get { return roaddata.DashWidth; }
        }

        public double GapLength
        {
            get { return roaddata.GapLength; }
        }

        public double Length
        {
            get { return beziers.Sum(bezier => bezier.Length); }
        }

        public double TotalLaneLength
        {
            get { return lanelist.Sum((Lane lane) => lane.Length); }
        }

        public double Width
        {
            get { return lanelist.Sum((Lane lane) => lane.Width); }
        }

        public int LaneCount
        {
            get { return lanelist.Count; }
        }

        public int VehicleCount
        {
            get { return vehiclelist.Count; }
        }

        public bool IsToward(PointD point)
        {
            return GetFirstCoordinate().DistanceTo(point) > GetLastCoordinate().DistanceTo(point);
        }

        public void EnterRoad(Vehicle vehicle)
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

        public void LeaveRoad(Vehicle vehicle)
        {
            vehiclelist.Remove(vehicle);
        }

        public void ClearVehicleList()
        {
            vehiclelist.Clear();

            foreach (RoadLane lane in lanelist)
                lane.ClearVehicleList();
        }

        public void SetNext(ILaneContainer ilanecontainer)
        {
            Next = ilanecontainer;
        }

        public void SetPrevious(ILaneContainer ilanecontainer)
        {
            Previous = ilanecontainer;
        }

        public IEnumerable<RoadLane> GetSpawnLanes()
        {
            var spawnlanelist = new List<RoadLane>();

            if (Previous == null)
                spawnlanelist.AddRange(GetForwardLanes());
            if (Next == null)
                spawnlanelist.AddRange(GetBackwardLanes());

            return spawnlanelist.Where(lane => lane.LaneType == LaneType.Normal).ToList();
        }

        public IEnumerable<RoadLane> GetUnSpawnLanes()
        {
            var unspawnlanelist = new List<RoadLane>();

            if (Previous == null)
                unspawnlanelist.AddRange(GetBackwardLanes());
            if (Next == null)
                unspawnlanelist.AddRange(GetForwardLanes());

            return unspawnlanelist.Where(lane => lane.LaneType == LaneType.Normal).ToList();
        }

        public PointD GetFirstCoordinate()
        {
            return beziers[0].GetCoordinate(0);
        }

        public PointD GetLastCoordinate()
        {
            return beziers[beziers.Length - 1].GetCoordinate(1);
        }

        public VectorD GetFirstDirection()
        {
            return beziers[0].GetDirection(0);
        }

        public VectorD GetLastDirection()
        {
            return beziers[beziers.Length - 1].GetDirection(1);
        }

        public List<RoadLane> GetTowardLanes(Point point)
        {
            return IsToward(point) ? GetForwardLanes() : GetBackwardLanes();
        }

        public List<RoadLane> GetOffwardLanes(Point point)
        {
            return IsToward(point) ? GetBackwardLanes() : GetForwardLanes();
        }

        public PointD GetTowardCoordinate(Point point)
        {
            return IsToward(point) ? GetLastCoordinate() : GetFirstCoordinate();
        }

        public PointD GetOffwardCoordinate(Point point)
        {
            return IsToward(point) ? GetFirstCoordinate() : GetLastCoordinate();
        }

        private List<RoadLane> GetForwardLanes()
        {
            List<RoadLane> sublanelist =
                lanelist.Where(lane => lane.LaneDirection == LaneDirection.Forward).ToList();
            sublanelist.Sort(
                (lane1, lane2) => lane1.LaneIndex.CompareTo(lane2.LaneIndex));
            return sublanelist;
        }

        private List<RoadLane> GetBackwardLanes()
        {
            List<RoadLane> sublanelist =
                lanelist.Where(lane => lane.LaneDirection == LaneDirection.Backward).ToList();
            sublanelist.Sort(
                (lane1, lane2) => lane1.LaneIndex.CompareTo(lane2.LaneIndex));
            return sublanelist;
        }

        public List<RoadLane> GetLanes()
        {
            return new List<RoadLane>(lanelist);
        }

        public int GetLaneIndexTo(RoadLane currentlane, PointD point)
        {
            Crossing crossing;
            if (currentlane.LaneDirection == LaneDirection.Forward)
                crossing = Next as Crossing;
            else
                crossing = Previous as Crossing;

            return crossing == null ? getlaneindexto(point) : crossing.GetLaneIndexTo(this, point);
        }

        private int getlaneindexto(PointD point)
        {
            double min = lanelist.Min(roadlane => roadlane.GetLastCoordinate().DistanceTo(point));
            return
                lanelist.First(roadlane => roadlane.GetLastCoordinate().DistanceTo(point) == min).LaneIndex;
        }

        public int GetDashCount(int bezierindex)
        {
            return Convert.ToInt32(beziers[bezierindex].Length/(roaddata.DashLength + roaddata.GapLength));
        }

        public int GetLaneCount(LaneDirection lanedirection)
        {
            return lanelist.Count(lane => lane.LaneDirection == lanedirection);
        }

        public double GetMaximumSpeed(string type)
        {
            return roaddata.SpeedDictionary[type];
        }

        public int GetVehiclesAtLastOfRoadToward(PointD point, double length)
        {
            List<RoadLane> lanes = IsToward(point) ? GetForwardLanes() : GetBackwardLanes();

            return lanes.Sum(roadlane => roadlane.GetVehiclesAtLast(length));
        }

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;
            drawpoints = new PointF[beziers.Length][];
            linepen = new Pen(Brushes.White, (float) DashWidth*matrix.Elements[0]);
            pointsize = (float) Width*matrix.Elements[0]/4;
            roadpen = new Pen(roadcolor, (float) Width*matrix.Elements[0]);

            for (int i = 0; i < beziers.Length; i++)
            {
                drawpoints[i] = beziers[i].GetDrawPoints();
                matrix.TransformPoints(drawpoints[i]);
            }
        }

        public void CalculateLaneDrawPoints()
        {
            foreach (RoadLane lane in lanelist)
                lane.CalculateDrawPoints();
        }

        public override void Draw(PaintEventArgs pea)
        {
            foreach (var points in drawpoints)
                pea.Graphics.DrawBezier(roadpen, points[0], points[1], points[2], points[3]);
        }

        public void DrawLine(PaintEventArgs pea)
        {
            foreach (var points in drawpoints)
                pea.Graphics.DrawBezier(linepen, points[0], points[1], points[2], points[3]);
        }

        public void DrawPoints(PaintEventArgs pea)
        {
            foreach (PointF point in drawpoints.SelectMany(points => points))
                pea.Graphics.FillRectangle(Brushes.Blue, point.X - pointsize, point.Y - pointsize, 2*pointsize,
                                           2*pointsize);
        }

        public void DrawLanes(PaintEventArgs pea)
        {
            foreach (RoadLane lane in lanelist)
                lane.Draw(pea);

            foreach (RoadLane lane in lanelist)
                lane.DrawLine(pea);
        }

        public void DrawLaneCurves(PaintEventArgs pea)
        {
            foreach (RoadLane lane in lanelist)
                lane.DrawCurve(pea);
        }

        public void DrawLanePoints(PaintEventArgs pea)
        {
            foreach (RoadLane lane in lanelist)
                lane.DrawPoints(pea);
        }

        private void addlane(LaneDirection lanedirection, LaneType lanetype)
        {
            List<RoadLane> sublanelist = lanelist.Where(l => l.LaneDirection == lanedirection).ToList();
            sublanelist.Sort((l1, l2) => l1.LaneIndex.CompareTo(l2.LaneIndex));

            double translation = sublanelist.Sum(l => l.Width);
            translation += roaddata.LaneWidth/2;

            var lane = new RoadLane(this, lanedirection, lanetype, lanelist.Count(l => l.LaneDirection == lanedirection),
                                    roaddata.LaneWidth, translation);

            if (sublanelist.Count > 0 && lane.LaneType != LaneType.Shoulder)
            {
                lane.LeftLane = sublanelist[sublanelist.Count - 1];
                sublanelist[sublanelist.Count - 1].RightLane = lane;
            }

            lanelist.Add(lane);
        }
    }
}