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

using System.Collections.Generic;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public abstract class BaseMenu : MenuStrip
    {
        protected Dictionary<string, ToolStripItem> itemdictionary;

        protected BaseMenu()
        {
            CreateHandle();
            itemdictionary = new Dictionary<string, ToolStripItem>();
        }

        public ToolStripItem GetToolStripItem(string name)
        {
            return itemdictionary[name];
        }

        public void PerformClick(string name)
        {
            Invoke(new UpdateDelegate(itemdictionary[name].PerformClick));
        }
    }
}