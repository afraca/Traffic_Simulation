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

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (CrossingLane))]
    public class CrossingLane : Lane
    {
        [OptionalField] private Crossing crossing;
        //[OptionalField] private RoadLane nextlane;
        //[OptionalField] private RoadLane previouslane;

        public CrossingLane(Crossing crossing, Lane previouslane, Lane nextlane, double width, List<Vehicle> vehiclelist)
        {
            int colorindex;

            if (previouslane.LaneDirection == LaneDirection.Forward)
                colorindex = previouslane.LaneIndex;
            else
                colorindex = -previouslane.LaneIndex - 1;

            Crossing = crossing;
            curvecolor = Colors.GetSaturatedColor(colorindex);
            ILaneContainer = crossing;
            LaneIndex = nextlane.LaneIndex;
            Width = width;
            this.vehiclelist = vehiclelist;

            nextlanes.Add(nextlane);
            nextlane.AddPreviousLane(this);
            previouslanes.Add(previouslane);
            previouslane.AddNextLane(this);

            PointD point1 = previouslane.GetLastCoordinate();
            PointD point2 = nextlane.GetFirstCoordinate();

            VectorD vector1 = previouslane.GetLastVelocity();
            VectorD vector2 = nextlane.GetFirstVelocity();
            VectorD vector3 = vector1.GetUnitVector()*new VectorD(point1, point2).Length/3;

            if (vector1.IsCoLinearWith(vector2))
                // Als de in en uitgaande RoadLane van de die deze CrossingLane verbindt dezelfde richting hebben
                // If the incoming and outgoing RoadLane that this crossinglane connects have the same direction
                // we need a third order curve
                beziers = new Bezier[] {new CubicBezier(point1, point1 + vector3, point2 - vector3, point2)};
            else // else a second order
                beziers = new Bezier[]
                    {
                        new QuadraticBezier(point1,
                                            new LineD(point1, vector1).GetCrossingPoint(new LineD(point2,
                                                                                                  vector2)),
                                            point2)
                    };

            Length = beziers.Sum(bezier => bezier.Length);
        }

        private Crossing Crossing
        {
            get { return crossing; }
            set { crossing = value; }
        }

        public RoadLane NextLane
        {
            get { return nextlanes[0] as RoadLane; }
        }

        public RoadLane PreviousLane
        {
            get { return previouslanes[0] as RoadLane; }
        }

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;
            drawpoints = new PointF[beziers.Length][];

            pen = new Pen(Brushes.Gray, (float) Width*matrix.Elements[0]);
            curvepen = new Pen(new SolidBrush(curvecolor), 0.5F*matrix.Elements[0]);
            pointsize = (float) Width*matrix.Elements[0]/2;

            curvepen.EndCap = LineCap.ArrowAnchor;

            for (int i = 0; i < beziers.Length; i++)
            {
                drawpoints[i] = beziers[i].GetDrawPoints();
                matrix.TransformPoints(drawpoints[i]);
            }
        }
    }
}