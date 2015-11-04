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
        private Transaction(string name) {
            // TODO: NIY
        }
        private Transaction(string name,int value) {
            // TODO: NIY
        }
        private Transaction(string name,long beginTime,long endTime) {
            // TODO: NIY
        }
        private void begin() {
            // TODO: NIY
        }
        private void end() {
            // TODO: NIY
        }
        private void fail() {
            // TODO: NIY
        }
        private void crash() {
            // TODO: NIY
        }

        // Metadata
        // An archaeological curiousity.  Original iOS/Android SDK
        // transaction design called for transactions to allow metadata.
        // Since then, Crittercism has not exposed API's in SDK's that
        // make it available to users.

        // JSON
        private static Object[] toArray() {
            // TODO: NIY
            return new Object[0];
        }

        // Persistence
        private static Transaction[] allTransactions() {
            // TODO: NIY
            return new Transaction[0];
        }
        private static Transaction transactionForId(int aTransactionId) {
            // TODO: NIY
            return null;
        }

        private static Transaction transactionForName(string name) {
            // TODO: NIY
            return null;
        }

        // void timerFired:(NSTimer *)timer;
    }
    }
