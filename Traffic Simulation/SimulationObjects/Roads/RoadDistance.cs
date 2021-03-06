﻿#region COPYING

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
    [KnownType(typeof (RoadDistance))]
    public struct RoadDistance
    {
        public RoadDistance(Road road, double distance, bool endofmap) : this()
        {
            Distance = distance;
            EndOfMap = endofmap;
            Road = road;
        }

        public Road Road { get; private set; }

        public bool EndOfMap { get; private set; }
        public double Distance { get; private set; }
    }
}