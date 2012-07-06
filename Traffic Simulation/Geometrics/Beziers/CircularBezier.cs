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
using System.Drawing.Drawing2D;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    public class CircularBezier : CubicBezier
    {
        private readonly PointD center;

        public CircularBezier(double startx, double starty, double centerx, double centery, double endx, double endy)
            : this(new PointD(startx, starty), new PointD(centerx, centery), new PointD(endx, endy))
        {
        }

        private CircularBezier(PointD start, PointD center, PointD end) //http://www.tinaja.com/glib/bezcirc2.pdf
        {
            double Θ = new VectorD(center, start).GetAngleWith(new VectorD(center, end));
            double φ = Θ/2;

            double x0 = Math.Cos(φ);
            double y0 = Math.Sin(φ);

            double x3 = x0;
            double y3 = -y0;

            double x1 = (4 - x0)/3;
            double y1 = ((1 - x0)*(3 - x0))/(3*y0);

            double x2 = x1;
            double y2 = -y1;

            var points = new PointF[] {new PointD(x0, y3), new PointD(x1, y2), new PointD(x2, y1), new PointD(x3, y0)};

            var matrix = new Matrix();
            matrix.Rotate(
                Convert.ToSingle(Geometrics.RadianToDegree(Math.Atan2(start.Y - center.Y, start.X - center.X) + φ)) +
                360);
            matrix.Scale(Convert.ToSingle(new LineD(start, center).Length),
                         Convert.ToSingle(new LineD(start, center).Length));
            matrix.TransformPoints(points);

            matrix = new Matrix();
            matrix.Translate(Convert.ToSingle(center.X), Convert.ToSingle(center.Y));
            matrix.TransformPoints(points);

            p0 = points[0];
            p1 = points[1];
            p2 = points[2];
            p3 = points[3];
            p0f = points[0];
            p1f = points[1];
            p2f = points[2];
            p3f = points[3];

            this.center = center;
            Length = getlength();
        }

        public override Bezier GetTranslatedBezier(bool reverse, double translation)
        {
            int reversefactor = Convert.ToInt32(!reverse) - Convert.ToInt32(reverse);

            PointD start = p0 + GetDirection(0).GetRightHandNormal()*translation*reversefactor;
            PointD end = p3 + GetDirection(1).GetRightHandNormal()*translation*reversefactor;

            return reverse ? new CircularBezier(end, center, start) : new CircularBezier(start, center, end);
        }
    }
}