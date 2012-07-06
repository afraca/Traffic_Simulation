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
using System.Drawing;
using System.Runtime.Serialization;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (RoadData))]
    public class RoadData
    {
        public RoadData(RoadType roadtype, Dictionary<string, double> speeddictionary, Color roadcolor, Color linecolor,
                        int forwardlanecount, int backwardlanecount, bool forwardshoulder, bool backwardshoulder,
                        double lanewidth, double dashlength, double dashwidth, double gaplength)
        {
            BackwardLaneCount = backwardlanecount;
            BackwardShoulder = backwardshoulder;
            DashLength = dashlength;
            DashWidth = dashwidth;
            ForwardLaneCount = forwardlanecount;
            ForwardShoulder = forwardshoulder;
            GapLength = gaplength;
            LaneWidth = lanewidth;
            LineColor = linecolor;
            RoadColor = roadcolor;
            RoadType = roadtype;
            SpeedDictionary = speeddictionary;
        }

        public Dictionary<string, double> SpeedDictionary { get; private set; }

        public Color RoadColor { get; private set; }
        public Color LineColor { get; private set; }
        public RoadType RoadType { get; private set; }

        public bool BackwardShoulder { get; private set; }
        public bool ForwardShoulder { get; private set; }

        public double DashLength { get; private set; }
        public double DashWidth { get; private set; }
        public double GapLength { get; private set; }
        public double LaneWidth { get; private set; }

        public int BackwardLaneCount { get; private set; }
        public int ForwardLaneCount { get; private set; }

        public string Name { get; private set; }
    }
}