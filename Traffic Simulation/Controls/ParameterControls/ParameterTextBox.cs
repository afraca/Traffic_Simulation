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
using System.Media;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public abstract class ParameterTextBox : TextBox, IParameterBox
    {
        private IParameterContainer iparametercontainer;
        protected string text;

        protected ParameterTextBox()
        {
            GotFocus += gotfocus;
            LostFocus += lostfocus;
            TextChanged += textchanged;
        }

        public Type ValueType { get; protected set; }

        public dynamic ValueDynamic { get; protected set; }

        #region IParameterBox Members

        public event EventHandler<StatusEventArgs> StatusChanged;

        public void SetValue(object o)
        {
            if (InvokeRequired)
                Invoke(new SetParameterDelegate(setvalue), o);
            else
                setvalue(o);
        }

        public void SetIParameterContainer(IParameterContainer iparametercontainer)
        {
            this.iparametercontainer = iparametercontainer;
        }

        #endregion

        protected bool typeerror()
        {
            SystemSounds.Beep.Play();
            onstatuschanged(
                new StatusEventArgs(
                    string.Format("The value entered at {0} isn't a valid {1}. Value changed back to {2}.", Name,
                                  ValueType.Name, text)));
            return false;
        }

        protected void onstatuschanged(StatusEventArgs sea)
        {
            if (StatusChanged != null)
                StatusChanged(this, sea);
        }

        private void gotfocus(object o, EventArgs ea)
        {
            TextChanged -= textchanged;
            text = Text;
        }

        private void lostfocus(object o, EventArgs ea)
        {
            checktext();
            TextChanged += textchanged;
        }

        private void textchanged(object o, EventArgs ea)
        {
            checktext();
        }

        private void checktext()
        {
            if (valid())
            {
                text = Text;
                // If the Text is valid, the old value can be updated with the new. The Text-Update won't have to be rollbacked.
                iparametercontainer.UpdateParameter(this, ValueDynamic);
                onstatuschanged(new StatusEventArgs(string.Format("{0} updated to: '{1}'.", Name, Text)));
            }
            else
            {
                reset();
                Text = text;
            }
        }

        private void setvalue(object o)
        {
            Text = o.ToString();
        }

        public abstract T GetValue<T>();

        protected abstract bool valid();

        protected abstract void reset();
    }
}