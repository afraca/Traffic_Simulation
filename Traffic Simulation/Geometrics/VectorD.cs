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
    [KnownType(typeof (VectorD))]
    public struct VectorD
    {
        public double X;
        public double Y;

        public VectorD(PointD p0, PointD p1)
        {
            X = p1.X - p0.X;
            Y = p1.Y - p0.Y;
        }

        public VectorD(PointD p0, PointD p1, double length)
        {
            X = p1.X - p0.X;
            Y = p1.Y - p0.Y;

            Scale(length);
        }

        public VectorD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public VectorD(double x, double y, double length)
        {
            X = x;
            Y = y;

            Resize(length);
        }

        public double Angle
        {
            get { return Math.Atan2(Y, X); }
        }

        public double Length
        {
            get { return Math.Sqrt(X*X + Y*Y); }
        }

        public static PointD operator +(VectorD vector, PointD point)
        {
            return new PointD(vector.X + point.X, vector.Y + point.Y);
        }

        public static PointD operator +(PointD point, VectorD vector)
        {
            return new PointD(vector.X + point.X, vector.Y + point.Y);
        }

        public static VectorD operator +(VectorD vector1, VectorD vector2)
        {
            return new VectorD(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }

        public static PointD operator -(PointD point, VectorD vector)
        {
            return new PointD(point.X - vector.X, point.Y - vector.Y);
        }

        public static VectorD operator -(VectorD vector1, VectorD vector2)
        {
            return new VectorD(vector1.X - vector2.X, vector1.Y - vector2.Y);
        }

        public static VectorD operator *(VectorD vector, double scalar)
        {
            return new VectorD(vector.X*scalar, vector.Y*scalar);
        }

        public static VectorD operator *(double scalar, VectorD vector)
        {
            return new VectorD(vector.X*scalar, vector.Y*scalar);
        }

        public static double operator *(VectorD vector1, VectorD vector2)
        {
            return vector1.X*vector2.X + vector1.Y*vector2.Y;
        }

        public static VectorD operator /(VectorD vector, double scalar)
        {
            return new VectorD(vector.X/scalar, vector.Y/scalar);
        }

        private void Resize(double length)
        {
            double factor = Math.Sqrt(X*X + Y*Y)/length;

            X = X/factor;
            Y = Y/factor;
        }

        private void Scale(double factor)
        {
            X *= factor;
            Y *= factor;
        }

        public void ToLeftHandNormal()
        {
            X -= Y;
            Y += X;
            X = Y - X;
        }

        public void ToRightHandNormal()
        {
            Y -= X;
            X += Y;
            Y = X - Y;
        }


        public VectorD GetInvertedVector()
        {
            return new VectorD(-X, -Y);
        }

        public VectorD GetResizedVector(double length)
        {
            double factor = Math.Sqrt(X*X + Y*Y)/length;
            return new VectorD(X/factor, Y/factor);
        }

        public VectorD GetScaledVector(double factor)
        {
            return new VectorD(X*factor, Y*factor);
        }

        public VectorD GetUnitVector()
        {
            return new VectorD(X/Length, Y/Length);
        }

        public VectorD GetLeftHandNormal()
        {
            return new VectorD(Y, -X);
        }

        public VectorD GetRightHandNormal()
        {
            return new VectorD(-Y, X);
        }

        public double GetAngleWith(VectorD vector)
        {
            double angle = Math.Acos((this*vector)/(Length*vector.Length));

            if (X*vector.Y < Y - vector.X)
                angle *= -1;

            return angle;
        }

        public double GetDotProductWith(VectorD vector)
        {
            return X*vector.X + Y*vector.Y;
        }

        public bool IsCoLinearWith(VectorD vector)
        {
            if (X == 0 && vector.X == 0 || Y == 0 && vector.Y == 0)
                return true;

            double quotientX = X/vector.X;
            double quotientY = Y/vector.Y;

            return Math.Abs(quotientX - quotientY) < 0.001;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }
    }
}