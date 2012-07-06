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
    public static class Randomizer
    {
        [ThreadStatic] private static Random random;

        public static void Initialize()
        {
            random = new Random();
        }

        public static void Initialize(int seed)
        {
            random = new Random(seed);
        }

        public static int Next()
        {
            return random.Next();
        }

        public static int Next(int max)
        {
            return random.Next(max);
        }

        public static int Next(int min, int max)
        {
            return random.Next(min, max);
        }

        private static double NextDouble()
        {
            return random.NextDouble();
        }

        public static double NextNormal(double μ, double σ)
        {
            return μ + σ*Math.Sqrt(-2.0*Math.Log(NextDouble()))*Math.Sin(2.0*Math.PI*NextDouble());
        }
    }
}