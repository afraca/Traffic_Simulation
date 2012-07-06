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
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (SimulationObject))]
    [DataContract(IsReference = true)]
    public abstract class SimulationObject
    {
        protected int id;
        protected string name;

        public int Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public override string ToString()
        {
            string s = string.Format("{0}: {1}", GetType().Name, id);

            if (!string.IsNullOrEmpty(name))
                s += string.Format(" - {0}", name);

            return s;
        }

        public abstract void Draw(PaintEventArgs pea);

        public abstract void CalculateDrawPoints();
    }
}