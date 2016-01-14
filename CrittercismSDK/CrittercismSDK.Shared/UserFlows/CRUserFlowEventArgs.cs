using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    public class CRUserFlowEventArgs : EventArgs
    {
        public string Name { get; internal set; }
        internal CRUserFlowEventArgs(string name) {
            Name = name;
        }
    }
}
