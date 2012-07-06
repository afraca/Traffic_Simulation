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
    public class QuadraticBezier : Bezier
    {
        private PointD p0;
        private PointD p1;
        private PointD p2;

        public QuadraticBezier(double x0, double y0, double x1, double y1, double x2, double y2)
            : this(new PointD(x0, y0), new PointD(x1, y1), new PointD(x2, y2))
        {
        }

        public QuadraticBezier(PointD p0, PointD p1, PointD p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            p0f = p0;
            p1f = p0/3 + 2*p1/3;
            p2f = 2*p1/3 + p2/3;
            p3f = p2;

            Length = getlength();
        }

        private QuadraticBezier()
        {
        }

        public override PointD GetCoordinate(double t)
        {
            double x = (1 - t)*(1 - t)*p0.X + 2*(1 - t)*t*p1.X + t*t*p2.X;
            double y = (1 - t)*(1 - t)*p0.Y + 2*(1 - t)*t*p1.Y + t*t*p2.Y;

            return new PointD(x, y);
        }

        public override VectorD GetVelocity(double t)
        {
            double x = -2*(1 - t)*p0.X + 2*(1 - t)*p1.X - 2*t*p1.X + 2*t*p2.X;
            double y = -2*(1 - t)*p0.Y + 2*(1 - t)*p1.Y - 2*t*p1.Y + 2*t*p2.Y;

            return new VectorD(x, y);
        }

        public override VectorD GetAcceleration(double t)
        {
            double x = 2*p0.X - 4*p1.X + 2*p2.X;
            double y = 2*p0.Y - 4*p1.Y + 2*p2.Y;

            return new VectorD(x, y);
        }

        public override VectorD GetJerk(double t)
        {
            return new VectorD(0, 0);
        }

        public override Bezier GetTranslatedBezier(bool reverse, double translation)
        {
            PointD q0;
            PointD q1;
            PointD q2;

            if (reverse)
            {
                q0 = p2 + GetDirection(1).GetLeftHandNormal()*translation;
                q2 = p0 + GetDirection(0).GetLeftHandNormal()*translation;
                q1 = new LineD(q0, GetVelocity(1)).GetCrossingPoint(new LineD(q2, GetVelocity(0)));
            }
            else
            {
                q0 = p0 + GetDirection(0).GetRightHandNormal()*translation;
                q2 = p2 + GetDirection(1).GetRightHandNormal()*translation;
                q1 = new LineD(q0, GetVelocity(0)).GetCrossingPoint(new LineD(q2, GetVelocity(1)));
            }

            return new QuadraticBezier(q0, q1, q2);
        }
    }
}