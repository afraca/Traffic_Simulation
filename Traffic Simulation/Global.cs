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
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Traffic_Simulation
{
    public delegate void SetParameterDelegate(object o);

    public delegate void UpdateDelegate();

    public delegate void VehicleDelegate(Vehicle[] vehicles);

    public enum LaneDirection
    {
        Forward,
        Backward
    }

    public enum LaneType
    {
        Normal,
        Shoulder
    }

    public enum RoadType
    {
        Highway,
        Normal,
        CityStreet
    }

    public enum TerminationStatistic
    {
        DestinationReached,
        MeanMovingFraction,
        MovingFraction,
        Time,
        None,
        VehicleCount,
        TotalVehicleCount
    };

    public enum TerminationType
    {
        Less,
        EqualOrLess,
        EqualOrGreater,
        Greater
    }

    public enum TrafficLightState
    {
        Red,
        Yellow,
        Green
    }

    internal static class Global
    {
        static Global()
        {
            HomeFolder = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
            Directory.CreateDirectory(string.Format("{0}\\Maps", HomeFolder));
        }

        public static string HomeFolder { get; private set; }

        public static List<Type> GetClassesInNameSpace(string nameSpace)
        {
            var typelist =
                Assembly.GetExecutingAssembly().GetTypes().Where(
                    t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToList();
            typelist.Sort((t1, t2) => String.Compare(t1.Name, t2.Name, StringComparison.Ordinal));
            return typelist;
        }
    }
}