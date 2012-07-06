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
using System.Runtime.Serialization;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (PointD))]
    public struct PointD
    {
        public double X;
        public double Y;

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(PointD point)
        {
            return new Point(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
        }

        public static implicit operator PointD(Point point)
        {
            return new PointD(point.X, point.Y);
        }

        public static implicit operator PointD(PointF point)
        {
            return new PointD(point.X, point.Y);
        }

        public static implicit operator PointF(PointD point)
        {
            return new PointF(Convert.ToSingle(point.X), Convert.ToSingle(point.Y));
        }

        public static PointD operator +(PointD point1, PointD point2)
        {
            return new PointD(point1.X + point2.X, point1.Y + point2.Y);
        }

        public static PointD operator -(PointD point1, PointD point2)
        {
            return new PointD(point1.X - point2.X, point1.Y - point2.Y);
        }

        public static PointD operator *(double scalar, PointD point)
        {
            return new PointD(point.X*scalar, point.Y*scalar);
        }

        public static PointD operator *(PointD point, double scalar)
        {
            return new PointD(point.X*scalar, point.Y*scalar);
        }

        public static PointD operator /(PointD point, double scalar)
        {
            return new PointD(point.X/scalar, point.Y/scalar);
        }

        public double DistanceTo(PointD point)
        {
            return Math.Sqrt((X - point.X)*(X - point.X) + (Y - point.Y)*(Y - point.Y));
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }
    }
}