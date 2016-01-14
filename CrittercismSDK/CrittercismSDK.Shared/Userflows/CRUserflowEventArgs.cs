using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    public class CRUserflowEventArgs : EventArgs
    {
        public string Name { get; internal set; }
        internal CRUserflowEventArgs(string name) {
            Name = name;
        }
    }
}
