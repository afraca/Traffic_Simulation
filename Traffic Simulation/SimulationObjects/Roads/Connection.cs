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
    [KnownType(typeof (Connection))]
    public class Connection
    {
        public Connection(Bezier bezier, Lane startlane, Lane endlane, int startbezierindex, int endbezierindex,
                          double starttimeindex, double endtimeindex, double unspawnindex = double.NaN)
        {
            Bezier = bezier;
            StartLane = startlane;
            EndLane = endlane;
            StartBezierIndex = startbezierindex;
            EndBezierIndex = endbezierindex;
            StartTime = starttimeindex;
            EndTime = endtimeindex;
            UnSpawnTimeIndex = unspawnindex;
        }

        public Connection(Lane startlane, Lane endlane, PointD location, VectorD beginvelocity, int bezierindex,
                          double bezierfactor, double speed, double time)
        {
            StartLane = startlane;
            EndLane = endlane;

            StartBezierIndex = bezierindex;
            StartTime = time;
            EndBezierIndex = bezierindex;
            EndTime = time + 2*bezierfactor;
            UnSpawnTimeIndex = double.NaN;

            // This part is of importance when a lanechange occurs while the Vehicle changes Bezier.
            // This doesn't occur in the current build.
            if (EndTime > 1)
            {
                if (EndBezierIndex == endlane.Beziers.Length - 1)
                    UnSpawnTimeIndex = 0.5F;
                else
                {
                    EndTime -= 1;
                    EndTime /= bezierfactor;
                    EndTime *= speed/startlane.Beziers[EndBezierIndex].Length;
                    EndBezierIndex++;
                }
            }

            // Get velocity on the desired endpoint.
            VectorD endvelocity = endlane.Beziers[EndBezierIndex].GetVelocity(EndTime);

            // Set the Bézier curve.
            PointD p0 = location;
            PointD p1 = location + 2*speed*beginvelocity/(3*beginvelocity.Length);
            PointD p3 = endlane.Beziers[EndBezierIndex].GetCoordinate(EndTime);
            PointD p2 = p3 - 2*speed*endvelocity/(3*endvelocity.Length);

            Bezier = new CubicBezier(p0, p1, p2, p3);
        }

        public Bezier Bezier { get; private set; }
        public Lane StartLane { get; private set; }
        public Lane EndLane { get; private set; }

        public double StartTime { get; private set; }
        public double EndTime { get; private set; }
        private double UnSpawnTimeIndex { get; set; }

        private int StartBezierIndex { get; set; }
        public int EndBezierIndex { get; private set; }
    }
}