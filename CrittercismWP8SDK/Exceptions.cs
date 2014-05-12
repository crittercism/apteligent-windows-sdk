using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDK {
    class InvalidAppIdException : Exception {
        public InvalidAppIdException(string message) : base(message) { }
    }
}
