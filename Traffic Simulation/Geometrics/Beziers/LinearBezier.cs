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

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    public class LinearBezier : Bezier
    {
        private PointD p0;
        private PointD p1;

        public LinearBezier(double x0, double y0, double x1, double y1)
            : this(new PointD(x0, y0), new PointD(x1, y1))
        {
        }

        private LinearBezier(PointD p0, PointD p1)
        {
            this.p0 = p0;
            this.p1 = p1;

            p0f = p0;
            p1f = GetCoordinate(1.0/3.0);
            p2f = GetCoordinate(2.0/3.0);
            p3f = p1;

            Length = getlength();
        }

        private LinearBezier()
        {
        }

        protected override double getlength()
        {
            return Math.Sqrt((p1.X - p0.X)*(p1.X - p0.X) + (p1.Y - p0.Y)*(p1.Y - p0.Y));
        }

        public override PointD GetCoordinate(double t)
        {
            double x = p0.X + (p1.X - p0.X)*t;
            double y = p0.Y + (p1.Y - p0.Y)*t;

            return new PointD(x, y);
        }

        public override VectorD GetVelocity(double t)
        {
            double x = p1.X - p0.X;
            double y = p1.Y - p0.Y;

            return new VectorD(x, y);
        }

        public override VectorD GetAcceleration(double t)
        {
            return new VectorD(0, 0);
        }

        public override VectorD GetJerk(double t)
        {
            return new VectorD(0, 0);
        }

        public override Bezier GetTranslatedBezier(bool reverse, double translation)
        {
            if (reverse)
            {
                VectorD normal = GetDirection(0).GetLeftHandNormal();
                return new LinearBezier(p1 + normal*translation, p0 + normal*translation);
            }
            else
            {
                VectorD normal = GetDirection(0).GetRightHandNormal();
                return new LinearBezier(p0 + normal*translation, p1 + normal*translation);
            }
        }
    }
}