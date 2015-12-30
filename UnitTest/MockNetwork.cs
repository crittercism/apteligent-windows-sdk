using CrittercismSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest {
    internal class MockNetwork : IMockNetwork {
        internal static SynchronizedQueue<MessageReport> MessageQueue = new SynchronizedQueue<MessageReport>(new Queue<MessageReport>());

        #region SendRequest
        // Example AppLoad response platform might send.  AppLoadTests expect to see this.
        private string ExampleAppLoadResponse = (
            "{\"txnConfig\":{\"defaultTimeout\":3600000,\n"
            + "              \"interval\":10,\n"
            + "              \"enabled\":true,\n"
            + "              \"transactions\":{\"Buy Critter Feed\":{\"timeout\":60000,\"slowness\":3600000,\"value\":1299},\n"
            + "                              \"Sing Critter Song\":{\"timeout\":90000,\"slowness\":3600000,\"value\":1500},\n"
            + "                              \"Write Critter Poem\":{\"timeout\":60000,\"slowness\":3600000,\"value\":2000}}},\n"
            + " \"apm\":{\"net\":{\"enabled\":true,\n"
            + "               \"persist\":false,\n"
            + "               \"interval\":10}},\n"
            + " \"needPkg\":1,\n"
            + " \"internalExceptionReporting\":true}"
        );

        public bool SendRequest(MessageReport messageReport) {
            // Simulate sending messageReport to platform and receiving response.
            lock (this) {
                if (messageReport is AppLoad) {
                    // Currently, the UnitTest is only testing AppLoad response details.
                    messageReport.DidReceiveResponse(ExampleAppLoadResponse);
                }
                // The messageReport is stored on "platform" so DequeueMessageType can find it.
                MessageQueue.Enqueue(messageReport);
            }
            return true;
        }
        #endregion

        #region MessageReport's Received by Mock Platform
        internal MessageReport DequeueMessageType(Type type) {
            // Simulate platform receiving messageReport of a given type .
            MessageReport answer = null;
            for (int i = 1; i < 10; i++) {
                lock (this) {
                    while (MessageQueue.Count > 0) {
                        // Toss messages until we get message of the specified type.
                        MessageReport messageReport = MessageQueue.Dequeue();
                        messageReport.Delete();
                        if ((messageReport.GetType() == type)
                            || (messageReport.GetType().IsSubclassOf(type))) {
                            answer = messageReport;
                            break;
                        }
                    }
                };
                if (answer != null) {
                    break;
                } else {
                    // Wait a little longer.
                    Thread.Sleep(100);
                }
            }
            // Returning MessageReport (or null) of the specified type .
            return answer;
        }

        internal void Cleanup() {
            lock (this) {
                MessageQueue.Clear();
            }
        }
        #endregion
    }
}
