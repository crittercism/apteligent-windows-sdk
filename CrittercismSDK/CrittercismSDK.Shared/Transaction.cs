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

        #region Properties
        internal TransactionState State() {
            TransactionState answer;
            lock (this) {
                answer = state;
            }
            return answer;
        }

        internal void SetState(TransactionState newState,long nowTime) {
            // Establishes newState for transaction at nowTime .
            state = newState;
            switch (state) {
                case TransactionState.BEGUN:
                    // TODO: eye time management
                    break;
                default:
                    // Final state
                    // TODO: eye time management
                    break;
            }
        }
        internal int Value() {
            int answer;
            lock (this) {
                answer = value;
            }
            return answer;
        }
        internal void SetValue(int newValue) {
            lock (this) {
                if (IsFinal()) {
                    // Complain
                    Crittercism.LOG_ERROR("Changing final state transaction is forbidden.");
                } else if (newValue < 0) {
                    // DESIGN: Decision by product team.
                    Crittercism.LOG_ERROR("Cannot assign transaction a negative value");
                } else {
                    value = newValue;
                }
            }
        }
        #endregion

        #region Instance Life Cycle
        internal Transaction(string name) {
            // TODO: NIY
        }
        internal Transaction(string name,int value) {
            // TODO: NIY
        }
        internal Transaction(string name,long beginTime,long endTime) {
            // TODO: NIY
        }
        #endregion

        #region State Transitions
        internal void Begin() {
            lock (this) {
                Transition(TransactionState.BEGUN);
            }
        }
        internal void End() {
            lock (this) {
                Transition(TransactionState.ENDED);
            }
        }
        internal void Fail() {
            lock (this) {
                Transition(TransactionState.FAILED);
            }
        }
        internal void Crash() {
            lock (this) {
                Transition(TransactionState.CRASHED);
            }
        }
        internal void Abort() {
            // DESIGN: Do we really need or want this?
            lock (this) {
                Transition(TransactionState.ABORTED);
            }
        }
        internal void Interrupt() {
            // DESIGN: Do we really need or want this?
            lock (this) {
                Transition(TransactionState.INTERRUPTED);
            }
        }
        private bool IsFinal() {
            // Transaction is in final state?
            bool answer= ((state != TransactionState.CREATED) && (state != TransactionState.BEGUN));
            return answer;
        }

        private void Transition(TransactionState newState) {
            // Transition a transaction from current state to newState .
            switch (state) {
                case TransactionState.CREATED:
                    if (newState == TransactionState.BEGUN) {
                        SetState(newState,DateTime.UtcNow.Ticks);
                    } else {
                        // Transaction being begun for the first time after create.
                        // Crittercism spec says newState has to be
                        // TransactionState.BEGUN in this case unless there is change
                        // of opinion about immediately failing a transaction possibility.
                        Crittercism.LOG_ERROR("Ending transaction that hasn't begun is forbidden.");
                    }
                    break;
                case TransactionState.BEGUN:
                    if (newState != TransactionState.BEGUN) {
                        SetState(newState,DateTime.UtcNow.Ticks);
                    } else {
                        // Complain. Crittercism spec says you shouldn't begin transaction
                        // more than once.
                        Crittercism.LOG_ERROR("Beginning transaction more than once is forbidden.");
                    }
                    break;
                default:
                    if (newState != TransactionState.TIMEOUT) {
                        // Already in final state
                        Crittercism.LOG_ERROR("Ending transaction more than once is forbidden.");
                    }
                    break;
            }
        }
        #endregion

        // Metadata
        // An archaeological curiousity.  Original iOS/Android SDK
        // transaction design called for transactions to allow metadata.
        // Since then, Crittercism has not exposed API's in SDK's that
        // make it available to users.

        // JSON
        internal Object[] ToArray() {
            Object[] answer = new Object[] {
                name,
                state,
                timeout,
                value,
                metadata,
                beginTimeString,
                endTimeString,
                eyeTime,
                foregroundTimeString
            };
            return answer;
        }

        // Persistence
        internal static Transaction[] AllTransactions() {
            return TransactionReporter.AllTransactions();
        }

        internal static Transaction TransactionForName(string name) {
            return TransactionReporter.TransactionForName(name);
        }

        // void timerFired:(NSTimer *)timer;


    }
}
