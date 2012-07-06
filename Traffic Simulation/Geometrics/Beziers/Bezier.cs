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
    [KnownType("GetKnownType")]
    public abstract class Bezier
    {
        // De drawpoints van de Bézier curve. Beziers rekenen met PointD voor Velocity en dergelijke.
        // The drawing points of the Bézier Curve. Beziers work with PointD for Velocity and such
        protected PointF p0f;
        protected PointF p1f;
        protected PointF p2f;
        protected PointF p3f;
        public double Length { get; protected set; }

        public abstract Bezier GetTranslatedBezier(bool reverse, double translation);
        public abstract PointD GetCoordinate(double t);
        public abstract VectorD GetVelocity(double t);
        public abstract VectorD GetAcceleration(double t);
        public abstract VectorD GetJerk(double t);

        protected virtual double getlength()
        {
            double length = 0;

            for (int i = 0; i < 10000; i++)
                length += new LineD(GetCoordinate((i + 1)/10000.0), GetCoordinate(i/10000.0)).Length;

            return length;
        }

        public PointF[] GetDrawPoints()
        {
            return new[] {p0f, p1f, p2f, p3f};
        }

        public VectorD GetDirection(double t)
        {
            return GetVelocity(t).GetResizedVector(1);
        }

        private static Type[] GetKnownType()
        {
            var types = new Type[5];
            types[0] = typeof (Bezier);
            types[1] = typeof (CircularBezier);
            types[2] = typeof (CubicBezier);
            types[3] = typeof (LinearBezier);
            types[4] = typeof (QuadraticBezier);
            return types;
        }

        public override string ToString()
        {
            return string.Format("P0: {0} P1: {1} P2: {2} P3: {3}", p0f, p1f, p2f, p3f);
        }
    }
}