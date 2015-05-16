using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrittercismSDK;

namespace ConsoleApp {
    class Program {
        static void Main(string[] args) {
            {
                Console.WriteLine("Press <Enter> to Init Crittercism.");
                Console.ReadLine();
                Crittercism.Init("537a4e738039805d82000002");
            }
            {
                Console.WriteLine("Press <Enter> to SetUsername.");
                Console.ReadLine();
                Crittercism.SetUsername("MrsCritter");
            }
            {
                Console.WriteLine("Press <Enter> to LeaveBreadcrumb.");
                Console.ReadLine();
                Crittercism.LeaveBreadcrumb("ConsoleApp Breadcrumb");
            }
            {
                Console.WriteLine("Press <Enter> to LogHandledException.");
                Console.ReadLine();
                try {
                    ThrowException();
                } catch (Exception e) {
                    Crittercism.LogHandledException(e);
                }
            }
            {
                Console.WriteLine("Press <Enter> to Shutdown Crittercism.");
                Console.ReadLine();
                Crittercism.Shutdown();
            }
            {
                Console.WriteLine("Press <Enter> to exit.");
                Console.ReadLine();
            }
        }

        private static void DeepError(int n) {
            if (n==0) {
                throw new Exception("Deep Inner Exception");
            } else {
                DeepError(n-1);
            }
        }

        private static void ThrowException() {
            try {
                DeepError(4);
            } catch (Exception ie) {
                throw new Exception("Outer Exception",ie);
            }
        }
    }
}
