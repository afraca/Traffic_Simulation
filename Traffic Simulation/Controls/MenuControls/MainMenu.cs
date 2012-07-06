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
using System.Reflection;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class MainMenu : BaseMenu
    {
        public MainMenu(object classobject, params MenuData[] menudata)
        {
            foreach (MenuData menu in menudata)
                addmenu(menu.Name, menu.Items, classobject);
        }

        private void addmenu(string name, IEnumerable<string> itemarray, object classobject)
            // http://stackoverflow.com/questions/1121441/c-sharp-addeventhandler-using-reflection
        {
            var menu = new ToolStripMenuItem(name);

            bool merge = typeof (MainForm) != classobject.GetType();

            if (merge)
                menu.MergeAction = MergeAction.MatchOnly;

            foreach (string itemtext in itemarray)
            {
                if (itemtext == "Seperator")
                    menu.DropDownItems.Add(new ToolStripSeparator());
                else
                {
                    var item = new ToolStripMenuItem {Text = itemtext};
                    EventInfo eventinfo = item.GetType().GetEvent("Click");
                    // Zoek de string.Format("{0}_Click", itemtext.Replace(" ", "") methode van classobject
                    MethodInfo methodinfo =
                        classobject.GetType().GetMethod(string.Format("{0}_Click", itemtext.Replace(" ", "")));

                    if (methodinfo == null) // Als die niet bestaat.
                        item.Enabled = false;
                    else // Anders, voeg een EventHandler toe van het Click event naar die methode.
                        eventinfo.AddEventHandler(item,
                                                  Delegate.CreateDelegate(eventinfo.EventHandlerType, classobject,
                                                                          methodinfo));

                    if (merge)
                        item.MergeAction = MergeAction.Replace;

                    menu.DropDownItems.Add(item);
                    itemdictionary.Add(item.Text, item);
                }
            }

            Items.Add(menu);
        }
    }
}