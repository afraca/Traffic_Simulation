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

#endregion

namespace Traffic_Simulation
{
    public static class Colors
    {
        public static Color GetSaturatedColor(int i) // http://en.wikipedia.org/wiki/HSL_and_HSV
        {
            double H = (101*i)%360;

            while (H < 0)
                H += 360;

            const double S = 1;
            const double V = 1;

            return HSV2Color(H, S, V);
        }

        private static Color HSV2Color(double H, double S, double V)
        {
            double Ri;
            double Gi;
            double Bi;

            double C = V*S;
            double Hi = H/60;
            double X = C*(1 - Math.Abs(Hi%2 - 1));
            double m = V - C;

            switch ((int) Math.Floor(Hi))
            {
                default:
                    Ri = C;
                    Gi = X;
                    Bi = 0;
                    break;
                case 1:
                    Ri = X;
                    Gi = C;
                    Bi = 0;
                    break;
                case 2:
                    Ri = 0;
                    Gi = C;
                    Bi = X;
                    break;
                case 3:
                    Ri = 0;
                    Gi = X;
                    Bi = C;
                    break;
                case 4:
                    Ri = X;
                    Gi = 0;
                    Bi = C;
                    break;
                case 5:
                    Ri = C;
                    Gi = 0;
                    Bi = X;
                    break;
            }

            var R = Math.Min(255, Convert.ToInt32(256*(Ri + m)));
            var G = Math.Min(255, Convert.ToInt32(256*(Gi + m)));
            var B = Math.Min(255, Convert.ToInt32(256*(Bi + m)));

            return Color.FromArgb(R, G, B);
        }
    }
}