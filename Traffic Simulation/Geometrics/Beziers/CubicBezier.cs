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
using System.Linq;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    public class CubicBezier : Bezier
    {
        protected PointD p0;
        protected PointD p1;
        protected PointD p2;
        protected PointD p3;

        public CubicBezier(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3)
            : this(new PointD(x0, y0), new PointD(x1, y1), new PointD(x2, y2), new PointD(x3, y3))
        {
        }

        public CubicBezier(PointD p0, PointD p1, PointD p2, PointD p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            p0f = p0;
            p1f = p1;
            p2f = p2;
            p3f = p3;

            Length = getlength();
        }

        protected CubicBezier()
        {
        }

        public override PointD GetCoordinate(double t)
        {
            double x = (1 - t)*(1 - t)*(1 - t)*p0.X + 3*(1 - t)*(1 - t)*t*p1.X + 3*(1 - t)*t*t*p2.X + t*t*t*p3.X;
            double y = (1 - t)*(1 - t)*(1 - t)*p0.Y + 3*(1 - t)*(1 - t)*t*p1.Y + 3*(1 - t)*t*t*p2.Y + t*t*t*p3.Y;

            return new PointD(x, y);
        }

        private static PointD getcoordinate(double t, PointD p0, PointD p1, PointD p2, PointD p3)
        {
            double x = (1 - t)*(1 - t)*(1 - t)*p0.X + 3*(1 - t)*(1 - t)*t*p1.X + 3*(1 - t)*t*t*p2.X + t*t*t*p3.X;
            double y = (1 - t)*(1 - t)*(1 - t)*p0.Y + 3*(1 - t)*(1 - t)*t*p1.Y + 3*(1 - t)*t*t*p2.Y + t*t*t*p3.Y;

            return new PointD(x, y);
        }

        public override VectorD GetVelocity(double t)
        {
            double x = -3*(1 - t)*(1 - t)*p0.X + 3*(1 - t)*(1 - t)*p1.X - 6*(1 - t)*t*p1.X + 6*(1 - t)*t*p2.X -
                       3*t*t*p2.X + 3*t*t*p3.X;
            double y = -3*(1 - t)*(1 - t)*p0.Y + 3*(1 - t)*(1 - t)*p1.Y - 6*(1 - t)*t*p1.Y + 6*(1 - t)*t*p2.Y -
                       3*t*t*p2.Y + 3*t*t*p3.Y;

            return new VectorD(x, y);
        }

        private VectorD getvelocity(double t, PointD p0, PointD p1, PointD p2, PointD p3)
        {
            double x = -3*(1 - t)*(1 - t)*p0.X + 3*(1 - t)*(1 - t)*p1.X - 6*(1 - t)*t*p1.X + 6*(1 - t)*t*p2.X -
                       3*t*t*p2.X + 3*t*t*p3.X;
            double y = -3*(1 - t)*(1 - t)*p0.Y + 3*(1 - t)*(1 - t)*p1.Y - 6*(1 - t)*t*p1.Y + 6*(1 - t)*t*p2.Y -
                       3*t*t*p2.Y + 3*t*t*p3.Y;

            return new VectorD(x, y);
        }

        public override VectorD GetAcceleration(double t)
        {
            double x = 6*(1 - t)*p0.X - 12*(1 - t)*p1.X + 6*t*p1.X + 6*(1 - t)*p2.X - 12*t*p2.X + 6*t*p3.X;
            double y = 6*(1 - t)*p0.Y - 12*(1 - t)*p1.Y + 6*t*p1.Y + 6*(1 - t)*p2.Y - 12*t*p2.Y + 6*t*p3.Y;

            return new VectorD(x, y);
        }

        public override VectorD GetJerk(double t)
        {
            double x = -6*p0.X + 18*p1.X - 18*p2.X + 6*p3.X;
            double y = -6*p0.Y + 18*p1.Y - 18*p2.Y + 6*p3.Y;

            return new VectorD(x, y);
        }

        public override Bezier GetTranslatedBezier(bool reverse, double translation)
        {
            var distancehashset = new HashSet<double>();

            PointD q0;
            PointD q1;
            PointD q2;
            PointD q3;

            PointD c25;
            PointD c50;
            PointD c75;

            VectorD direction0;
            VectorD direction1;

            double distance = 0;


            // Determine the controlpoints of the new Bézier curve and points of the current curve with which
            // you want to calibrate.
            if (reverse)
            {
                q0 = p3 + GetDirection(1).GetLeftHandNormal()*translation;
                q1 = p2 + GetDirection(1).GetLeftHandNormal()*translation;
                q2 = p1 + GetDirection(0).GetLeftHandNormal()*translation;
                q3 = p0 + GetDirection(0).GetLeftHandNormal()*translation;

                c25 = GetCoordinate(0.75) + translation*GetDirection(0.75).GetLeftHandNormal();
                c50 = GetCoordinate(0.50) + translation*GetDirection(0.50).GetLeftHandNormal();
                c75 = GetCoordinate(0.25) + translation*GetDirection(0.25).GetLeftHandNormal();

                direction0 = GetDirection(1);
                direction1 = GetDirection(0);
            }
            else
            {
                q0 = p0 + GetDirection(0).GetRightHandNormal()*translation;
                q1 = p1 + GetDirection(0).GetRightHandNormal()*translation;
                q2 = p2 + GetDirection(1).GetRightHandNormal()*translation;
                q3 = p3 + GetDirection(1).GetRightHandNormal()*translation;

                c25 = GetCoordinate(0.25) + translation*GetDirection(0.25).GetRightHandNormal();
                c50 = GetCoordinate(0.50) + translation*GetDirection(0.50).GetRightHandNormal();
                c75 = GetCoordinate(0.75) + translation*GetDirection(0.75).GetRightHandNormal();

                direction0 = GetDirection(0).GetInvertedVector();
                direction1 = GetDirection(1).GetInvertedVector();
            }

            // Determine the distance between the calibrationpoint for the new Bézier curve.
            distance += new LineD(getcoordinate(0.25, q0, q1, q2, q3), c25).Length;
            distance += new LineD(getcoordinate(0.50, q0, q1, q2, q3), c50).Length;
            distance += new LineD(getcoordinate(0.75, q0, q1, q2, q3), c75).Length;

            distancehashset.Add(distance);

            for (int i = 0; i < 1000; i++)
                // Try to improve the control points 1000 times as described in the Bézier curve pdf
            {
                PointD q1backup = q1;
                PointD q2backup = q2;

                var distancesnew = new List<double>(new double[4]);

                q1 = q1backup - 0.01*direction0;
                distancesnew[0] += new LineD(getcoordinate(0.25, q0, q1, q2, q3), c25).Length;
                distancesnew[0] += new LineD(getcoordinate(0.50, q0, q1, q2, q3), c50).Length;
                distancesnew[0] += new LineD(getcoordinate(0.75, q0, q1, q2, q3), c75).Length;

                q1 = q1backup + 0.01*direction0;
                distancesnew[1] += new LineD(getcoordinate(0.25, q0, q1, q2, q3), c25).Length;
                distancesnew[1] += new LineD(getcoordinate(0.50, q0, q1, q2, q3), c50).Length;
                distancesnew[1] += new LineD(getcoordinate(0.75, q0, q1, q2, q3), c75).Length;

                q2 = q2backup - 0.01*direction1;
                distancesnew[2] += new LineD(getcoordinate(0.25, q0, q1, q2, q3), c25).Length;
                distancesnew[2] += new LineD(getcoordinate(0.50, q0, q1, q2, q3), c50).Length;
                distancesnew[2] += new LineD(getcoordinate(0.75, q0, q1, q2, q3), c75).Length;

                q2 = q2backup + 0.01*direction1;
                distancesnew[3] += new LineD(getcoordinate(0.25, q0, q1, q2, q3), c25).Length;
                distancesnew[3] += new LineD(getcoordinate(0.50, q0, q1, q2, q3), c50).Length;
                distancesnew[3] += new LineD(getcoordinate(0.75, q0, q1, q2, q3), c75).Length;

                int minindex = distancesnew.IndexOf(distancesnew.Min());

                switch (minindex)
                {
                    case 0:
                        q1 = q1backup - 0.01*direction0;
                        break;
                    case 1:
                        q1 = q1backup + 0.01*direction0;
                        break;
                    case 2:
                        q2 = q2backup - 0.01*direction1;
                        break;
                    case 3:
                        q2 = q2backup + 0.01*direction1;
                        break;
                }

                distance = distancesnew[minindex];

                if (distancehashset.Contains(distance))
                    break;
                distancehashset.Add(distance);
            }

            return new CubicBezier(q0, q1, q2, q3);
        }
    }
}