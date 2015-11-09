using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class Transaction
    {
        // Use Int32.MinValue pennies to represent Wire+Protocol doc's "null"
        // transaction value.  (We would prefer to not have "null" at all.)
        // It's conceivable some apps might want modest negative values for
        // debits, so we use Int32.MinValue == -2^31 == -2147483648 == -$21,474,836.48
        // here.
        const int NULL_VALUE = Int32.MinValue;

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
        internal string Name() {
            // The "name" is immutable.
            return name;
        }
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
            isForegrounded = TransactionReporter.IsForegrounded();
            switch (state) {
                case TransactionState.BEGUN:
                    SetBeginTime(nowTime);
                    if (isForegrounded) {
                        SetForegroundTime(nowTime);
                        // TODO: Create expiration timer
                    }
                    break;
                default:
                    // Final state
                    SetEndTime(nowTime);
                    // TODO: Remove expiration timer
                    if (isForegrounded) {
                        // Entering final state is effectively closing early ahead of
                        // the time when app may be backgrounded later.  The persisted
                        // record gets the correct additional "eye time".
                        eyeTime += nowTime - foregroundTime;
                        isForegrounded = false;
                    }
                    break;
            }
            TransactionReporter.Save(this);
        }
        private long ClampTimeout(long newTimeout) {
            long answer = newTimeout;
            // TODO: NIY (It's not this simple)
            answer = Math.Min(answer,TransactionReporter.DefaultTimeout());
            return answer;
        }
        internal void SetTimeout(long newTimeout) {
            lock (this) {
                if (IsFinal()) {
                    // Complain
                    Crittercism.LOG_ERROR("Changing final state transaction is forbidden.");
                } else {
                    timeout = ClampTimeout(newTimeout);
                    // TODO: eye time management
                }
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
        internal Dictionary<string,string> Metadata() {
            Dictionary<string,string> answer;
            lock (this) {
                answer = metadata;
            }
            return answer;
        }
        internal long BeginTime() {
            long answer;
            lock (this) {
                answer = beginTime;
            }
            return answer;
        }
        internal void SetBeginTime(long newBeginTime) {
            DateTime begin_date = (new DateTime(newBeginTime)).ToUniversalTime();
            beginTimeString = DateUtils.ISO8601DateString(begin_date);
            beginTime = newBeginTime;
        }
        internal long EndTime() {
            long answer;
            lock (this) {
                answer = endTime;
            }
            return answer;
        }
        internal void SetEndTime(long newEndTime) {
            DateTime end_date = (new DateTime(newEndTime)).ToUniversalTime();
            endTimeString = DateUtils.ISO8601DateString(end_date);
            endTime = newEndTime;
        }
        internal long ForegroundTime() {
            // "Foreground time" == the latest Crittercism Init
            // time or foreground time, whichever is later.
            long answer;
            lock (this) {
                answer = foregroundTime;
            }
            return answer;
        }
        internal void SetForegroundTime(long newForegroundTime) {
            // "Foreground time" == the latest Crittercism Init
            // time or foreground time, whichever is later.
            DateTime foreground_date = (new DateTime(newForegroundTime)).ToUniversalTime();
            foregroundTimeString = DateUtils.ISO8601DateString(foreground_date);
            foregroundTime = newForegroundTime;
        }
        #endregion

        #region Instance Life Cycle
        private Transaction() {
            name = "";
            state = TransactionState.CREATED;
            value = NULL_VALUE;
            metadata = new Dictionary<string,string>();
            // 0 corresponds to reference date of 
            // 12:00:00 midnight, January 1, 0001
            // (0:00:00 UTC on January 1, 0001, in the Gregorian calendar)
            // https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
            // And we are calling SetBeginTime and SetEndTime: here
            // so the strings beginTimeString and endTimeString
            // will get computed.
            SetBeginTime(0);
            SetEndTime(0);
            eyeTime = 0;
            SetForegroundTime(0);
            timeout = ClampTimeout(Int64.MaxValue);
        }
        internal Transaction(string name) : this() {
            this.name = Crittercism.TruncatedString(name);
            TransactionReporter.Save(this);
        }
        internal Transaction(string name,int value) : this(name) {
            this.value = value;
        }
        internal Transaction(string name,long beginTime,long endTime) : this(name) {
            ////////////////////////////////////////////////////////////////
            // NOTE: Automatic transactions ("App Load", "App Foreground", "App Background")
            ////////////////////////////////////////////////////////////////
            state = TransactionState.ENDED;
            value = 0;
            SetBeginTime(beginTime);
            SetEndTime(endTime);
            eyeTime = endTime - beginTime;
            SetForegroundTime(beginTime);
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

        #region JSON
        internal Object[] ToArray() {
            Object[] answer = new Object[] {
                name,
                state,
                timeout,
                ((value == NULL_VALUE) ? null : (Object)value),
                metadata,
                beginTimeString,
                endTimeString,
                eyeTime,
                foregroundTimeString
            };
            return answer;
        }
        #endregion

        // #region Metadata
        // An archaeological curiousity.  Original iOS/Android SDK
        // transaction design called for transactions to allow metadata.
        // Since then, Crittercism has not exposed API's in SDK's that
        // make it available to users.
        // #endregion

        #region Persistence
        internal static Transaction[] AllTransactions() {
            return TransactionReporter.AllTransactions();
        }

        internal static Transaction TransactionForName(string name) {
            return TransactionReporter.TransactionForName(name);
        }
        #endregion

        #region Timing
        // void timerFired:(NSTimer *)timer;
        #endregion

    }
}
