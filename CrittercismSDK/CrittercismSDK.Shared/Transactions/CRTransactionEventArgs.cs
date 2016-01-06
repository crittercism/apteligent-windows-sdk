using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    public class CRTransactionEventArgs : EventArgs
    {
        public string Name { get; internal set; }
        internal CRTransactionEventArgs(string name) {
            Name = name;
        }
    }
}
