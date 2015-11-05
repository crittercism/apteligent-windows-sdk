using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class Transaction
    {
        private int transactionId;
        private string name;
        private TransactionState state;
        private long timeout;
        private int value;
        private Dictionary<string,string> metadata;
        private long beginTime;
        private long endTime;
        private long eyeTime;

        private string beginTimeString;
        private string endTimeString;
        private long foregroundTime;
        private string foregroundTimeString;
        private bool isForegrounded;
        //private NSTimer timer;
        internal Transaction(string name) {
            // TODO: NIY
        }
        internal Transaction(string name,int value) {
            // TODO: NIY
        }
        internal Transaction(string name,long beginTime,long endTime) {
            // TODO: NIY
        }
        internal int Value() {
            // TODO: NIY
            return value;
        }
        internal void SetValue(int newValue) {
            // TODO: NIY
            value = newValue;
        }
        internal void Begin() {
            // TODO: NIY
        }
        internal void End() {
            // TODO: NIY
        }
        internal void Fail() {
            // TODO: NIY
        }
        internal void Crash() {
            // TODO: NIY
        }

        // Metadata
        // An archaeological curiousity.  Original iOS/Android SDK
        // transaction design called for transactions to allow metadata.
        // Since then, Crittercism has not exposed API's in SDK's that
        // make it available to users.

        // JSON
        internal static Object[] ToArray() {
            // TODO: NIY
            return new Object[0];
        }

        // Persistence
        internal static Transaction[] AllTransactions() {
            // TODO: NIY
            return new Transaction[0];
        }
        internal static Transaction TransactionForId(int aTransactionId) {
            // TODO: NIY
            return null;
        }

        internal static Transaction TransactionForName(string name) {
            // TODO: NIY
            return null;
        }

        // void timerFired:(NSTimer *)timer;

        internal void Interrupt() {
            // NIY
        }
    }
}
