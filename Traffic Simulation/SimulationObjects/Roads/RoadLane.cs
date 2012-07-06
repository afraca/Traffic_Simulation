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
    [KnownType(typeof (RoadLane))]
    //[DataContract(IsReference=true)]
    public class RoadLane : Lane
    {
        [OptionalField] private readonly Road road;

        [OptionalField] private readonly double spawndistance;
        [OptionalField] private readonly double translation;
        [OptionalField] private double[] gaplengths;
        [OptionalField] private RoadLane leftlane;
        [OptionalField] private Bezier[] linebeziers;

        [OptionalField] private Color linecolor;
        [OptionalField] private PointF[][] linedrawpoints;
        [NonSerialized] private Pen linepen;
        [OptionalField] private RoadLane rightlane;
        [OptionalField] private Color roadcolor;
        [OptionalField] private TrafficLight trafficlight;

        public RoadLane(Road road, LaneDirection lanedirection, LaneType lanetype, int laneindex, double width,
                        double translation)
        {
            int colorindex;

            if (lanedirection == LaneDirection.Backward)
                colorindex = laneindex;
            else
                colorindex = -laneindex - 1;

            curvecolor = Colors.GetSaturatedColor(colorindex);
            ILaneContainer = road;
            LaneDirection = lanedirection;
            LaneIndex = laneindex;
            LaneType = lanetype;
            linecolor = road.LineColor;
            this.road = road;
            roadcolor = road.RoadColor;
            this.translation = translation;
            vehiclelist = new List<Vehicle>();
            Width = width;

            setbeziers();
            setgaplengths();

            Length = beziers.Sum(bezier => bezier.Length);
            spawndistance = 2*road.GetMaximumSpeed("Vehicle")/beziers[0].Length;
        }

        public Road Road
        {
            get { return road; }
        }

        public RoadLane LeftLane
        {
            get { return leftlane; }
            set { leftlane = value; }
        }

        public RoadLane RightLane
        {
            get { return rightlane; }
            set { rightlane = value; }
        }

        public TrafficLight TrafficLight
        {
            get { return trafficlight; }
            set { trafficlight = value; }
        }

        public bool CanSpawn
        {
            get
            {
                if (vehiclelist.Count == 0)
                    return true;
                return vehiclelist.Count(vehicle => vehicle.BackTimeIndex < spawndistance) == 0;
            }
        }

        public override void EnterLane(Vehicle vehicle)
        {
            base.EnterLane(vehicle);
            Road.EnterRoad(vehicle);
        }

        public override void LeaveLane(Vehicle vehicle, bool changeilanecontainer)
        {
            base.LeaveLane(vehicle, changeilanecontainer);
            if (changeilanecontainer)
                Road.LeaveRoad(vehicle);
        }

        public int GetVehiclesAtLast(double length)
        {
            double mintimeindex = beziers.Length;
            int bezierindex = beziers.Length - 1;

            while (length > 0 && bezierindex >= 0)
            {
                if (length < beziers[bezierindex].Length)
                {
                    mintimeindex -= length/beziers[bezierindex].Length;
                    length = 0;
                }
                else
                {
                    mintimeindex -= 1;
                    length -= beziers[bezierindex].Length;
                }
            }
            return vehiclelist.Count(vehicle => vehicle.TimeIndex > mintimeindex);
        }

        public int GetLaneIndexTo(PointD point)
        {
            return road.GetLaneIndexTo(this, point);
        }

        private void setbeziers()
        {
            Bezier[] roadbeziers = Road.Beziers;
            bool reverse = LaneDirection == LaneDirection.Backward;

            beziers = new Bezier[roadbeziers.Length];
            linebeziers = new Bezier[roadbeziers.Length];

            for (int i = 0; i < beziers.Length; i++)
            {
                beziers[i] = roadbeziers[i].GetTranslatedBezier(reverse, translation);
                linebeziers[i] = roadbeziers[i].GetTranslatedBezier(reverse, translation + Width/2);
            }

            if (!reverse) return;
            beziers = beziers.Reverse().ToArray();
            linebeziers = linebeziers.Reverse().ToArray();
        }

        private void setgaplengths()
        {
            gaplengths = new double[beziers.Length];

            for (int i = 0; i < gaplengths.Length; i++)
            {
                int dashcount = LaneDirection == LaneDirection.Forward
                                    ? Road.GetDashCount(i)
                                    : Road.GetDashCount(gaplengths.Length - 1 - i);
                double length = beziers[i].Length - dashcount*Road.DashLength;
                gaplengths[i] = length/dashcount;
            }
        }

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;

            curvepen = new Pen(new SolidBrush(curvecolor), 0.5F*matrix.Elements[0]) {EndCap = LineCap.ArrowAnchor};
            drawpoints = new PointF[beziers.Length][];
            linepen = new Pen(Brushes.White, (float) Road.DashWidth*matrix.Elements[0]);
            linedrawpoints = new PointF[beziers.Length][];
            pen = new Pen(Brushes.Gray, (float) Width*matrix.Elements[0]);
            pointsize = (float) Width*matrix.Elements[0]/2;

            for (int i = 0; i < beziers.Length; i++)
            {
                drawpoints[i] = beziers[i].GetDrawPoints();
                matrix.TransformPoints(drawpoints[i]);

                linedrawpoints[i] = linebeziers[i].GetDrawPoints();
                matrix.TransformPoints(linedrawpoints[i]);
            }
        }

        public void DrawDestinationLine(PaintEventArgs pea, SolidBrush solidbrush)
        {
            var pen = new Pen(solidbrush, this.pen.Width/2) {EndCap = LineCap.ArrowAnchor};
            PointF[] points = drawpoints[drawpoints.Length - 1];
            pea.Graphics.DrawBezier(pen, points[0], points[1], points[2], points[3]);
        }

        public void DrawLine(PaintEventArgs pea)
        {
            for (int i = 0; i < linedrawpoints.Length; i++)
            {
                if (Road != null && LaneIndex < Road.GetLaneCount(LaneDirection) - 2)
                {
                    if (linepen.Width < 1)
                        linepen.DashPattern = new[]
                            {
                                (float) (Road.DashLength/Road.DashWidth*linepen.Width),
                                (float) (gaplengths[i]/Road.DashWidth*linepen.Width)
                            };
                    else
                        linepen.DashPattern = new[]
                            {
                                (float) (Road.DashLength/Road.DashWidth),
                                (float) (gaplengths[i]/Road.DashWidth)
                            };
                }

                PointF[] points = linedrawpoints[i];
                pea.Graphics.DrawBezier(linepen, points[0], points[1], points[2], points[3]);
            }
        }
    }
}