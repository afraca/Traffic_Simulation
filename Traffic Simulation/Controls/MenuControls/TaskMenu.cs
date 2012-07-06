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
using System.Reflection;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class TaskMenu : BaseMenu
    {
        public TaskMenu(object classobject, params string[] items)
        {
            foreach (string item in items)
                addtoolstripitem(item, classobject);
        }

        private void addtoolstripitem(string name, object classobject)
        {
            if (name == "Seperator")
                Items.Add(new ToolStripSeparator());
            else
            {
                bool merge = typeof (MainForm) != classobject.GetType();

                var toolstripbutton = new ToolStripButton(name) {Name = name};
                EventInfo eventinfo = toolstripbutton.GetType().GetEvent("Click");
                // Zoek de string.Format("{0}_Click", name.Replace(" ", "")) methode van classobject
                MethodInfo methodinfo =
                    classobject.GetType().GetMethod(string.Format("{0}_Click", name.Replace(" ", "")));

                if (methodinfo == null) // Als die niet bestaat.
                    toolstripbutton.Enabled = false;
                else // Anders, voeg een EventHandler toe van het Click event naar die methode.
                    eventinfo.AddEventHandler(toolstripbutton,
                                              Delegate.CreateDelegate(eventinfo.EventHandlerType, classobject,
                                                                      methodinfo));

                if (merge)
                    toolstripbutton.MergeAction = MergeAction.Replace;

                Items.Add(toolstripbutton);
                itemdictionary.Add(name, toolstripbutton);
            }
        }
    }
}