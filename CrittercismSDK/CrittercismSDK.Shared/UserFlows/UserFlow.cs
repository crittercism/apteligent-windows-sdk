using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if NETFX_CORE || WINDOWS_PHONE
using Windows.System.Threading;
#else
using System.Timers;
#endif // NETFX_CORE

namespace CrittercismSDK {
    [JsonConverter(typeof(UserFlowConverter))]
    internal class UserFlow {
        #region Constants
        internal const int MAX_NAME_LENGTH = 255;
        // Use Int32.MinValue pennies to represent Wire+Protocol doc's "null"
        // userFlow value.  (We would prefer to not have "null" at all.)
        // It's conceivable some apps might want modest negative values for
        // debits, so we use Int32.MinValue == -2^31 == -2147483648 == -$21,474,836.48
        // here.
        internal const int NULL_VALUE = Int32.MinValue;
        #endregion

        #region Properties
        ////////////////////////////////////////////////////////////////
        // NOTE: Microsoft Time Measurements
        // Not living in Steve Jobs' ideal world, we must contend with
        // several different inconsistent ways that Microsoft prefers
        // to measure time:
        // * Precise moments are measured in ticks (tick == 10^-7 seconds)
        // 0 ticks corresponds to reference date of
        // 12:00:00 midnight, January 1, 0001
        // (0:00:00 UTC on January 1, 0001, in the Gregorian calendar)
        // https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
        // * Time outs are measured in milliseconds.  Applicable
        // to timers and Thread.Sleep .
        ////////////////////////////////////////////////////////////////
        private string name;
        private UserFlowState state;
        private int timeout; // milliseconds
        private int value; // pennies (http://www.usmint.gov/mint_programs/circulatingCoins/?action=circPenny)
        private Dictionary<string,string> metadata;
        private long beginTime; // ticks (absolute)
        private long endTime; // ticks (absolute)
        private long eyeTime; // ticks (relative interval)
        private string beginTimeString; // ISO8601DateString
        private string endTimeString; // ISO8601DateString
        private long foregroundTime; // ticks (absolute)
        private string foregroundTimeString; // ISO8601DateString
        private bool isForegrounded;

        internal string Name() {
            // The "name" is immutable.
            return name;
        }
        internal UserFlowState State() {
            UserFlowState answer;
            lock (this) {
                answer = state;
            }
            return answer;
        }
        private void SetState(UserFlowState newState,long nowTime) {
            // Establishes newState for userFlow at nowTime .
            state = newState;
            isForegrounded = UserFlowReporter.isForegrounded;
            switch (state) {
                case UserFlowState.CANCELLED:
                    SetEndTime(nowTime);
                    RemoveTimer();
                    break;
                case UserFlowState.BEGUN:
                    SetBeginTime(nowTime);
                    if (isForegrounded) {
                        SetForegroundTime(nowTime);
                        CreateTimer();
                    }
                    break;
                default:
                    // Final state
                    SetEndTime(nowTime);
                    RemoveTimer();
                    if (isForegrounded) {
                        // Entering final state is effectively closing early ahead of
                        // the time when app may be backgrounded later.  The persisted
                        // record gets the correct additional "eye time".
                        eyeTime += nowTime - foregroundTime;
                        isForegrounded = false;
                    }
                    if (newState == UserFlowState.TIMEOUT) {
                        Crittercism.OnUserFlowTimeOut(new CRUserFlowEventArgs(name));
                    }
                    break;
            }
            UserFlowReporter.Save(this);
        }
        private int ClampTimeout(int newTimeout) {
            int answer = UserFlowReporter.ClampTimeout(name,newTimeout);
            return answer;
        }
        internal int Timeout() {
            // UserFlow timeout in milliseconds
            int answer;
            lock (this) {
                answer = timeout;
            }
            return answer;
        }
        internal void SetTimeout(int newTimeout) {
            // Set new userFlow timeout in milliseconds.
            lock (this) {
                if (IsFinal()) {
                    // Complain
                    DebugUtils.LOG_ERROR("Changing final state userFlow is forbidden.");
                } else {
                    timeout = ClampTimeout(newTimeout);
                    if (isForegrounded) {
                        CreateTimer();
                    }
                }
            }
        }
        private int DefaultValue() {
            int answer = UserFlowReporter.DefaultValue(name);
            return answer;
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
                    DebugUtils.LOG_ERROR("Changing final state userFlow is forbidden.");
                } else if (newValue < 0) {
                    // DESIGN: Decision by product team.
                    DebugUtils.LOG_ERROR("Cannot assign userFlow a negative value");
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
            // Begin time of userFlow in ticks
            long answer;
            lock (this) {
                answer = beginTime;
            }
            return answer;
        }
        private void SetBeginTime(long newBeginTime) {
            // Set begin time of userFlow in ticks.
            DateTime begin_date = (new DateTime(newBeginTime,DateTimeKind.Utc));
            beginTimeString = TimeUtils.ISO8601DateString(begin_date);
            beginTime = newBeginTime;
        }
        internal string BeginTimeString() {
            string answer;
            lock (this) {
                answer = beginTimeString;
            }
            return answer;
        }
        internal long EndTime() {
            // End time of userFlow in ticks.
            long answer;
            lock (this) {
                answer = endTime;
            }
            return answer;
        }
        private void SetEndTime(long newEndTime) {
            // Set end time of userFlow in ticks.
            DateTime end_date = (new DateTime(newEndTime,DateTimeKind.Utc));
            endTimeString = TimeUtils.ISO8601DateString(end_date);
            endTime = newEndTime;
        }
        internal string EndTimeString() {
            string answer;
            lock (this) {
                answer = endTimeString;
            }
            return answer;
        }
        internal long EyeTime() {
            // The "eyeTime" of a userFlow is the sum of the lengths of the
            // [F B] intervals that appeared in the userFlow's lifetime, in ticks.
            // F = foreground time, B = background time .
            long answer;
            lock (this) {
                answer = eyeTime;
            }
            return answer;
        }
        internal long ForegroundTime() {
            // "Foreground time" == the latest Crittercism Init
            // time or foreground time, whichever is later, in ticks.
            long answer;
            lock (this) {
                answer = foregroundTime;
            }
            return answer;
        }
        private void SetForegroundTime(long newForegroundTime) {
            // "Foreground time" == the latest Crittercism Init
            // time or foreground time, whichever is later, in ticks.
            DateTime foreground_date = (new DateTime(newForegroundTime,DateTimeKind.Utc));
            foregroundTimeString = TimeUtils.ISO8601DateString(foreground_date);
            foregroundTime = newForegroundTime;
        }
        internal string ForegroundTimeString() {
            string answer;
            lock (this) {
                answer = foregroundTimeString;
            }
            return answer;
        }
        #endregion

        #region Instance Life Cycle
        private UserFlow() {
            name = "";
            state = UserFlowState.CREATED;
            value = NULL_VALUE;
            metadata = new Dictionary<string,string>();
            // 0 ticks corresponds to reference date of
            // 12:00:00 midnight, January 1, 0001
            // (0:00:00 UTC on January 1, 0001, in the Gregorian calendar)
            // https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
            // And we are calling SetBeginTime and SetEndTime here so the strings
            // beginTimeString and endTimeString will get computed.
            const long referenceTime = 0;
            SetBeginTime(referenceTime);
            SetEndTime(referenceTime);
            eyeTime = 0;
            SetForegroundTime(referenceTime);
        }
        internal UserFlow(string name,int value) : this() {
            this.name = StringUtils.TruncateString(name,MAX_NAME_LENGTH);
            // timeout in milliseconds (Applying "userFlow specific configuration"
            // can only be done after we know "name" of this userFlow.)
            timeout = ClampTimeout(Int32.MaxValue);
            if (value == NULL_VALUE) {
                value = DefaultValue();
            }
            this.value = value;
            UserFlowReporter.Save(this);
        }
        internal UserFlow(string name) : this(name,NULL_VALUE) {
        }
        internal UserFlow(string name,long beginTime,long endTime) : this(name,0) {
            ////////////////////////////////////////////////////////////////
            // Input:
            //    name = userFlow name
            //    beginTime = userFlow begin time in ticks
            //    endTime = userFlow end time in ticks
            // NOTE: Automatic userFlows ("App Load", "App Foreground", "App Background")
            ////////////////////////////////////////////////////////////////
            state = UserFlowState.ENDED;
            SetBeginTime(beginTime);
            SetEndTime(endTime);
            eyeTime = endTime - beginTime;
            SetForegroundTime(beginTime);
            // This "Save" needs to occur after the "state" assigned above is known.
            UserFlowReporter.Save(this);
        }
        internal UserFlow(
            string name,
            UserFlowState state,
            int timeout,
            int value,
            Dictionary<string,string> metadata,
            long beginTime,
            long endTime,
            long eyeTime) {
            // This constructor only used by UserFlowConvert ReadJson for finished
            // UserFlow's appearing in either a UserFlowReport or a Crash report.
            this.name = name;
            this.state = state;
            this.timeout = timeout; // milliseconds
            this.value = value;
            this.metadata = metadata;
            SetBeginTime(beginTime); // ticks
            SetEndTime(endTime); // ticks
            this.eyeTime = eyeTime; // ticks
        }
        #endregion

        #region State Transitions
        internal void Begin() {
            lock (this) {
                Transition(UserFlowState.BEGUN);
            }
        }
        internal void Cancel() {
            lock (this) {
                Transition(UserFlowState.CANCELLED);
            }
        }
        internal void End() {
            lock (this) {
                Transition(UserFlowState.ENDED);
            }
        }
        internal void Fail() {
            lock (this) {
                Transition(UserFlowState.FAILED);
            }
        }
        internal void Crash() {
            lock (this) {
                Transition(UserFlowState.CRASHED);
            }
        }
        private bool IsFinal() {
            // UserFlow is in final state?
            bool answer = ((state != UserFlowState.CREATED) && (state != UserFlowState.BEGUN));
            return answer;
        }
        internal void Transition(UserFlowState newState) {
            // Transition a userFlow from current state to newState .
            if (newState == UserFlowState.CANCELLED) {
                SetState(newState,DateTime.UtcNow.Ticks);
            } else {
                switch (state) {
                    case UserFlowState.CREATED:
                        if (newState == UserFlowState.BEGUN) {
                            SetState(newState,DateTime.UtcNow.Ticks);
                        } else if (newState == UserFlowState.CRASHED) {
                            // NOP. Leave userFlow in CREATED state.
                        } else {
                            // UserFlow being begun for the first time after create.
                            // Crittercism spec says newState has to be
                            // UserFlowState.BEGUN in this case unless there is change
                            // of opinion about immediately failing a userFlow possibility.
                            DebugUtils.LOG_ERROR("Ending userFlow that hasn't begun is forbidden.");
                        }
                        break;
                    case UserFlowState.BEGUN:
                        if (newState != UserFlowState.BEGUN) {
                            SetState(newState,DateTime.UtcNow.Ticks);
                        } else {
                            // Complain. Crittercism spec says you shouldn't begin userFlow
                            // more than once.
                            DebugUtils.LOG_ERROR("Beginning userFlow more than once is forbidden.");
                        }
                        break;
                    default:
                        if (newState != UserFlowState.TIMEOUT) {
                            // Already in final state.  We are only checking for TIMEOUT to prevent
                            // printing this message (the UserFlow must have entered some final
                            // state in the nick of time).
                            DebugUtils.LOG_ERROR("Ending userFlow more than once is forbidden.");
                        }
                        break;
                }
            }
        }
        #endregion

        #region JSON
        internal JArray ToJArray() {
            // Per "UserFlows Wire Protocol - v1", timeout and eyeTime are returned in seconds.
            List<JToken> list = new List<JToken>();
            list.Add(name);
            list.Add((int)state);
            list.Add(timeout / (double)TimeUtils.MSEC_PER_SEC); // seconds
            if (value == NULL_VALUE) {
                list.Add(null);
            } else {
                list.Add(value);
            };
            list.Add(new JObject());
            list.Add(beginTimeString);
            list.Add(endTimeString);
            list.Add(eyeTime / (double)TimeUtils.TICKS_PER_SEC); // seconds
            JArray answer = new JArray(list);
            return answer;
        }
        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
        #endregion

        // #region Metadata
        // An archaeological curiousity.  Original iOS/Android SDK
        // userFlow design called for userFlows to allow metadata.
        // Since then, Crittercism has not exposed API's in SDK's that
        // make it available to users.
        // #endregion

        #region Notifications
        internal void Foreground(long foregroundTime) {
            // Called by UserFlowReporter's "Foreground" method when app foregrounds.
            lock (this) {
                if (state == UserFlowState.BEGUN) {
                    SetForegroundTime(foregroundTime);
                    isForegrounded = true;
                    CreateTimer();
                }
            }
        }
        internal void Background(long backgroundTime) {
            // Called by UserFlowReporter's "Background" method when app backgrounds.
            lock (this) {
                if (state == UserFlowState.BEGUN) {
                    RemoveTimer();
                    eyeTime = (eyeTime + backgroundTime - foregroundTime);
                    isForegrounded = false;
                }
            }
        }
        #endregion

        #region Persistence
        internal static UserFlow[] AllUserFlows() {
            return UserFlowReporter.AllUserFlows();
        }

        internal static UserFlow UserFlowForName(string name) {
            return UserFlowReporter.UserFlowForName(name);
        }
        #endregion

        #region Timing
#if NETFX_CORE || WINDOWS_PHONE
        private static ThreadPoolTimer timer = null;
#else
        private static Timer timer=null;
#endif // NETFX_CORE

        internal void CreateTimer() {
            // Called every assignment to "timeout" property.    Microsoft
            // timers fire on thread pool threads (better than Objective-C!).
            // Kill any existing timer
            lock (this) {
                RemoveTimer();
                if (timeout == Int32.MaxValue) {
                    // If the timeout is +infinity, don't create a new timer.
                } else {
                    // Create new timer based on "timeout" property and when we began
                    // and now.
                    int milliseconds = timeout - (int)(eyeTime / TimeUtils.TICKS_PER_MSEC);
                    if (milliseconds <= 0) {
                        // If remaining time is nonpositive, just timeout here
                        Transition(UserFlowState.TIMEOUT);
                    } else {
                        // Otherwise
                        CreateTimerMilliseconds(milliseconds);
                    }
                }
            }
        }

        private void CreateTimerMilliseconds(int milliseconds) {
            // Create timer
#if NETFX_CORE || WINDOWS_PHONE
            if (timer == null) {
                // Creates a single-use timer.
                // https://msdn.microsoft.com/en-US/library/windows/apps/windows.system.threading.threadpooltimer.aspx
                timer = ThreadPoolTimer.CreateTimer(
                    OnTimerElapsed,
                    TimeSpan.FromMilliseconds(milliseconds));
            }
#else
            if (timer==null) {
                // Generates an event after a set interval
                // https://msdn.microsoft.com/en-us/library/system.timers.timer(v=vs.110).aspx
                timer = new Timer(milliseconds);
                timer.Elapsed += OnTimerElapsed;
                // the Timer should raise the Elapsed event only once (false)
                timer.AutoReset = false;        // fire once
                timer.Enabled = true;           // Start the timer
            }
#endif // NETFX_CORE
        }

#if NETFX_CORE || WINDOWS_PHONE
        private void OnTimerElapsed(ThreadPoolTimer timer) {
            // The userFlow has timed out.
            lock (this) {
                Transition(UserFlowState.TIMEOUT);
            }
        }
#else
        private void OnTimerElapsed(Object source, ElapsedEventArgs e) {
            // The userFlow has timed out.
            lock (this) {
                Transition(UserFlowState.TIMEOUT);
            }
        }
#endif // NETFX_CORE
        private void RemoveTimer() {
            // Call if we don't need the timer anymore.
#if NETFX_CORE || WINDOWS_PHONE
            if (timer != null) {
                timer.Cancel();
                timer = null;
            }
#else
            if (timer!=null) {
                timer.Stop();
                timer = null;
            }
#endif // NETFX_CORE
        }
        #endregion
    }
}
