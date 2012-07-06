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
using System.Runtime.Serialization;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (LineD))]
    public struct LineD
    {
        private PointD Point1;
        private PointD Point2;

        public LineD(PointD point1, PointD point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public LineD(PointD point, VectorD vector)
        {
            Point1 = point;
            Point2 = new PointD(point.X + vector.X, point.Y + vector.Y);
        }

        public VectorD Vector
        {
            get { return new VectorD(Point1, Point2); }
        }

        public double Length
        {
            get
            {
                return
                    Math.Sqrt((Point2.X - Point1.X)*(Point2.X - Point1.X) + (Point2.Y - Point1.Y)*(Point2.Y - Point1.Y));
            }
        }

        public PointD GetCoordinate(double t)
        {
            double x = Point1.X + (Point2.X - Point1.X)*t;
            double y = Point1.Y + (Point2.Y - Point1.Y)*t;

            return new PointD(x, y);
        }

        public PointD GetCrossingPoint(LineD line)
        {
            return GetCrossingPoint(this, line);
        }

        public double GetDistance(PointD point)
        {
            double x0 = Point1.X;
            double x1 = Point2.X;
            double x2 = point.X;

            double y0 = Point1.Y;
            double y1 = Point2.Y;
            double y2 = point.Y;

            return Math.Abs((x2 - x1)*(y1 - y0) - (x1 - x0)*(y2 - y1))/
                   Math.Sqrt((x2 - x1)*(x2 - x1) + (y2 - y1)*(y2 - y1));
        }

        public bool IsCoLinearWith(LineD line)
        {
            return new VectorD(Point1, Point2).IsCoLinearWith(new VectorD(line.Point1, line.Point2)) &&
                   GetDistance(line.Point1) < 0.001;
        }

        private static PointD GetCrossingPoint(LineD line1, LineD line2)
            // http://mathworld.wolfram.com/Line-LineIntersection.html
        {
            double x1 = line1.Point1.X;
            double x2 = line1.Point2.X;
            double x3 = line2.Point1.X;
            double x4 = line2.Point2.X;

            double y1 = line1.Point1.Y;
            double y2 = line1.Point2.Y;
            double y3 = line2.Point1.Y;
            double y4 = line2.Point2.Y;

            double x = ((x1*y2 - y1*x2)*(x3 - x4) - (x1 - x2)*(x3*y4 - y3*x4))/
                       ((x1 - x2)*(y3 - y4) - (x3 - x4)*(y1 - y2));
            double y = ((x1*y2 - y1*x2)*(y3 - y4) - (y1 - y2)*(x3*y4 - y3*x4))/
                       ((x1 - x2)*(y3 - y4) - (x3 - x4)*(y1 - y2));

            return new PointD(x, y);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Point1, Point2);
        }
    }
}