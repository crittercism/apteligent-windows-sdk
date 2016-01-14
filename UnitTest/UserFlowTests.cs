using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CrittercismSDK;

namespace UnitTest {
    [TestClass]
    public class UserFlowTests {
        const string ExampleLongString =
        ("AAAAAAAA11111111222222223333333344444444555555556666666677777777"
        + "BBBBBBBB11111111222222223333333344444444555555556666666677777777"
        + "CCCCCCCC11111111222222223333333344444444555555556666666677777777"
        + "DDDDDDDD11111111222222223333333344444444555555556666666677777777"
        + "EEEEEEEE11111111222222223333333344444444555555556666666677777777"
        + "FFFFFFFF11111111222222223333333344444444555555556666666677777777"
        + "GGGGGGGG11111111222222223333333344444444555555556666666677777777"
        + "HHHHHHHH11111111222222223333333344444444555555556666666677777777");

        const string ExampleTruncatedString =
        ("AAAAAAAA11111111222222223333333344444444555555556666666677777777"
        + "BBBBBBBB11111111222222223333333344444444555555556666666677777777"
        + "CCCCCCCC11111111222222223333333344444444555555556666666677777777"
        + "DDDDDDDD1111111122222222333333334444444455555555666666667777777");

        const string ExampleLongString2 =
        ("AAAAAAAA11111111222222223333333344444444555555556666666677777777"
        + "BBBBBBBB11111111222222223333333344444444555555556666666677777777"
        + "CCCCCCCC11111111222222223333333344444444555555556666666677777777"
        + "DDDDDDDD11111111222222223333333344444444555555556666666677777777"
        + "IIIIIIII11111111222222223333333344444444555555556666666677777777"
        + "JJJJJJJJ11111111222222223333333344444444555555556666666677777777"
        + "KKKKKKKK11111111222222223333333344444444555555556666666677777777"
        + "LLLLLLLL11111111222222223333333344444444555555556666666677777777");

        const int MSEC_PER_SEC = 1000;

        #region Example UserFlow
        internal UserFlow ExampleUserFlow() {
            UserFlow answer = new UserFlow("Purchase Crittercism SDK",100000);
            answer.SetTimeout(300 * MSEC_PER_SEC);
            return answer;
        }
        #endregion

        [TestInitialize()]
        public void TestInitialize() {
            // Use TestInitialize to run code before running each test 
            UserFlowReporter.Init();
        }

        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            UserFlowReporter.Shutdown();
        }

        #region UserFlow state property
        [TestMethod]
        public void TestStateProperty() {
            // Check state is correct UserFlowState after
            // various UserFlow method calls.
            {
                UserFlow example1 = ExampleUserFlow();
                Assert.IsTrue(example1.State() == UserFlowState.CREATED,
                             "Expecting UserFlowState.CREATED .");
                {
                    example1.Begin(); ;
                    Assert.IsTrue(example1.State() == UserFlowState.BEGUN,
                                 "Expecting UserFlowState.BEGUN .");
                }
                Thread.Sleep(100);
                {
                    example1.End(); ;
                    Assert.IsTrue(example1.State() == UserFlowState.ENDED,
                                 "Expecting UserFlowState.ENDED .");
                }
            }
            {
                UserFlow example2 = ExampleUserFlow();
                Assert.IsTrue(example2.State() == UserFlowState.CREATED,
                             "Expecting UserFlowState.CREATED .");
                {
                    example2.Begin(); ;
                    Assert.IsTrue(example2.State() == UserFlowState.BEGUN,
                                 "Expecting UserFlowState.BEGUN .");
                }
                Thread.Sleep(100);
                {
                    example2.Fail();
                    Assert.IsTrue(example2.State() == UserFlowState.FAILED,
                                 "Expecting UserFlowState.FAILED .");
                }
            }
            {
                UserFlow example3 = ExampleUserFlow();
                Assert.IsTrue(example3.State() == UserFlowState.CREATED,
                             "Expecting UserFlowState.CREATED .");
                const int timeout = 100;
                example3.SetTimeout(timeout);
                {
                    example3.Begin();
                    Assert.IsTrue(example3.State() == UserFlowState.BEGUN,
                                 "Expecting UserFlowState.BEGUN .");
                }
                // Yield here so CLR can operate example3's timer .
                Thread.Sleep(2 * timeout);
                Assert.IsTrue(example3.State() == UserFlowState.TIMEOUT,
                             "Expecting UserFlowState.TIMEOUT .");
            }
        }
        #endregion

        #region UserFlow timeout property
        [TestMethod]
        public void TestTimeoutProperty() {
            // UserFlow should TIMEOUT if run in excess of it's timeout.
            UserFlow example = ExampleUserFlow();
            Assert.IsTrue(example.State() == UserFlowState.CREATED,
                         "Expecting UserFlowState.CREATED .");
            const int timeout = 100;
            example.SetTimeout(timeout);
            {
                example.Begin();
                Assert.IsTrue(example.State() == UserFlowState.BEGUN,
                             "Expecting UserFlowState.BEGUN .");
            }
            // Yield here so CLR can operate example3's timer .
            Thread.Sleep(2 * timeout);
            Assert.IsTrue(example.State() == UserFlowState.TIMEOUT,
                         "Expecting UserFlowState.TIMEOUT .");
        }
        #endregion

        #region UserFlow value property
        [TestMethod]
        public void TestValueProperty() {
            // Confirm value property is working.
            const int value1 = 1234; // $12.34
            const int value2 = 9999; // $99.99
            UserFlow example = ExampleUserFlow();
            // Set and get the property value via dot syntax.
            example.SetValue(value1);
            Assert.IsTrue(example.Value() == value1,
                         "Expecting example.Value() == {0}",value1);
            // Set and get the property value via getter and setter methods.
            example.SetValue(value2);
            Assert.IsTrue(example.Value() == value2,
                         "Expecting example.Value() == {0}",value2);
            // Mixed syntax #1
            example.SetValue(value1);
            Assert.IsTrue(example.Value() == value1,
                         "Expecting example.Value() == {0}",value1);
            // Mixed syntax #2
            example.SetValue(value2);
            Assert.IsTrue(example.Value() == value2,
                         "Expecting example.Value() == {0}",value2);
        }
        #endregion

        #region UserFlow times properties
        [TestMethod]
        public void TestTimeProperties() {
            // Testing beginTime, endTime, eyeTime
            UserFlow example = ExampleUserFlow();
            Assert.IsTrue(example.State() == UserFlowState.CREATED,
                         "Expecting UserFlowState.CREATED .");
            example.SetTimeout(Int32.MaxValue);
            {
                example.Begin();
                Assert.IsTrue(example.State() == UserFlowState.BEGUN,
                             "Expecting UserFlowState.BEGUN .");
            }
            // Call "yield" here so CLR can operate example's timer .
            const int positiveTime = 100;
            long yieldTime;
            {
                long yieldBeginTime = DateTime.UtcNow.Ticks;
                Thread.Sleep(positiveTime);
                long yieldEndTime = DateTime.UtcNow.Ticks;
                yieldTime = yieldEndTime - yieldBeginTime;
            }
            {
                example.End();
                Assert.IsTrue(example.State() == UserFlowState.ENDED,
                             "Expecting UserFlowState.ENDED .");
            }
            // All the above is mainly just to get some interesting
            // beginTime, endTime, eyeTime play around with.  Now,
            // it is play time.
            {
                long beginTime = example.BeginTime();
                long endTime = example.EndTime();
                long eyeTime = example.EyeTime();
                // Test beginTime, endTime, eyeTime wrt the actual "yieldTime"
                Assert.IsTrue(beginTime < endTime,
                             "Expecting beginTime < endTime .");
                // The test is run entirely in foreground, so
                // endTime - beginTime should be close to eyeTime
                // allowing a little bit of deviation due to use
                // of "yield".
                Assert.IsTrue((0.8 * yieldTime) < eyeTime,
                              String.Format("Expecting eyeTime == {0} to be close to {1} .",
                                            eyeTime,
                                            yieldTime));
                Assert.IsTrue(eyeTime < (1.2 * yieldTime),
                              String.Format("Expecting eyeTime == {0} to be close to {1} .",
                                            eyeTime,
                                            yieldTime));
                Assert.IsTrue((0.8 * yieldTime) < (endTime - beginTime),
                              String.Format("Expecting (endTime - beginTime) == {0} to be close to {1} .",
                                            endTime - beginTime,
                                            yieldTime));
                Assert.IsTrue((endTime - beginTime) < (1.2 * yieldTime),
                              String.Format("Expecting (endTime - beginTime) == {0} to be close to {1} .",
                                            endTime - beginTime,
                                            yieldTime));
            }
        }
        #endregion

        #region UserFlow max userFlow limit
        [TestMethod]
        public void TestMaxUserFlowCount() {
            for (int i = 0; i < 2 * UserFlowReporter.MAX_USERFLOW_COUNT; i++) {
                Crittercism.BeginUserFlow(i.ToString());
            }
            Object[] loggedUserFlows = UserFlowReporter.AllUserFlows();
            Assert.AreEqual(UserFlowReporter.UserFlowCount(),UserFlowReporter.MAX_USERFLOW_COUNT,
                            String.Format("Expecting userFlowCount : {0} to be the maximum count : {1} .",
                                          UserFlowReporter.UserFlowCount(),
                                          UserFlowReporter.MAX_USERFLOW_COUNT));
            Assert.AreEqual(UserFlowReporter.UserFlowCount(),loggedUserFlows.Length,
                            String.Format("Expecting userFlowCount : {0} to be the # of userFlows : {1} .",
                                          UserFlowReporter.UserFlowCount(),
                                          loggedUserFlows.Length));
            foreach (UserFlow userFlow in loggedUserFlows) {
                int index = Int32.Parse(userFlow.Name());
                Assert.IsTrue(((index >= 0) && (index < UserFlowReporter.MAX_USERFLOW_COUNT)),
                              String.Format("Expecting userFlow name to be an expected value between 0 and"
                                            + "MAX_USERFLOW_COUNT ({0}), name was {1}",
                                            UserFlowReporter.MAX_USERFLOW_COUNT,
                                            index));
            }
        }
        #endregion

        #region UserFlow(name) constructor
        [TestMethod]
        public void TestInit() {
            string name1 = "";
            UserFlow example = new UserFlow(name1);
            Assert.IsTrue(example.Name() == name1,
                         "Confirm constructor sets name property.");
        }

        [TestMethod]
        public void TestInitLongString() {
            UserFlow example = new UserFlow(ExampleLongString);
            Trace.WriteLine("example.Name() == " + example.Name());
            Assert.IsTrue(example.Name() == ExampleTruncatedString,
                         "Confirm constructor sets name property.");
        }
        #endregion

        #region UserFlow(name,value) constructor
        [TestMethod]
        public void TestInitWithValue() {
            string name1 = "";
            int value1 = 2000;
            UserFlow example = new UserFlow(name1,value1);
            Assert.IsTrue(example.Name() == name1,
                         "Confirm constructor sets name property.");
            Assert.IsTrue(example.Value() == value1,
                         "Confirm constructor sets value property.");
        }

        [TestMethod]
        public void TestInitLongStringWithValue() {
            int value1 = 2000;
            UserFlow example = new UserFlow(ExampleLongString,value1);
            Assert.IsTrue(example.Name() == ExampleTruncatedString,
                         "Confirm constructor sets name property.");
            Assert.IsTrue(example.Value() == value1,
                         "Confirm constructor sets value property.");
        }
        #endregion

        #region UserFlow Begin() method
        [TestMethod]
        public void TestBegin() {
            // Test "Begin"'s resulting state is correct.
            UserFlow example = ExampleUserFlow();
            example.Begin();
            Assert.IsTrue(example.State() == UserFlowState.BEGUN,
                         "Confirm Begin changes state to UserFlowState.BEGUN");
        }
        #endregion

        #region UserFlow End() method
        [TestMethod]
        public void TestSuccess() {
            // Test "End"'s resulting state is correct.  It's required to "Begin" first.
            UserFlow example = ExampleUserFlow();
            example.Begin();
            Assert.IsTrue(example.State() == UserFlowState.BEGUN,
                         "Confirm Begin changes state to UserFlowState.BEGUN");
            example.End();
            Assert.IsTrue(example.State() == UserFlowState.ENDED,
                         "Confirm End changes state to UserFlowState.ENDED");
        }
        #endregion

        #region UserFlow Fail method
        [TestMethod]
        public void TestFail() {
            // Test "Fail"'s resulting state is correct.  It's required to "begin" first.
            UserFlow example = ExampleUserFlow();
            example.Begin();
            Assert.IsTrue(example.State() == UserFlowState.BEGUN,
                         "Confirm Begin changes state to UserFlowState.BEGUN");
            example.Fail();
            Assert.IsTrue(example.State() == UserFlowState.FAILED,
                         "Confirm Fail changes state to UserFlowState.FAILED");
        }
        #endregion

        #region UserFlow Transition method
        [TestMethod]
        public void TestTransition() {
            {
                // Resulting state are correct.
                UserFlowState state1 = UserFlowState.BEGUN;
                UserFlow example = ExampleUserFlow();
                example.Transition(state1);
                Assert.IsTrue(example.State() == state1,
                             "Confirm Transition changes state to given state");
            }
        }
        #endregion

        #region UserFlow ToArray method
        [TestMethod]
        public void TestUserFlowToJArray() {
            UserFlow example1 = ExampleUserFlow();
            JArray json = example1.ToJArray();
            Assert.IsTrue(UserFlowConverter.IsUserFlowJson(json));
        }
        #endregion

        #region UserFlow toJSONString method
        public void CheckJSONString(string jsonString) {
            // Confirm this jsonString looks like proper JSON for a UserFlow .
            // NOTE: userFlow represented as JSON to server
            //     == [hash,id,name,state,timeout,value,
            //         metadata,beginTime,endTime,eyeTime]
            Object json = null;
            try {
                json = JsonConvert.DeserializeObject(jsonString,typeof(Object[]));
            } catch (JsonException e) {
                // There should be no "error" .
                Assert.IsNull(e,
                            "Expection a legal JSON string that parses correctly #1.");
                if (e != null) {
                    Trace.WriteLine(String.Format("error == " + e.Message));
                }
            }
            // The "json" should be an NSArray .
            Assert.IsNotNull(json,
                           "Expection a legal JSON string that parses correctly #2.");
            Trace.WriteLine(String.Format("Converted jsonString's class == "
                                          + json.GetType().FullName));
            Assert.IsTrue(json is Object[],
                         "Expecting userFlow JSON string representing JSON array.");
            Trace.WriteLine(String.Format("json == " + json));
            // TODO: Following commented out test statement is still broken.
            // JsonConvert.DeserializeObject is doing some unwanted weird stuff.
            //Assert.IsTrue(UserFlowConverter.IsUserFlowJson(json));
        }

        [TestMethod]
        public void TestUserFlowToString() {
            // Confirm ToString's return value parses as plausible JSON .
            UserFlow example1 = ExampleUserFlow();
            CheckJSONString(example1.ToString());
        }
        #endregion

        #region UserFlow Persistence
        [TestMethod]
        public void TestSaveLoad() {
            // Load saved userFlow.  Does it look the same?
            // Extract fields from userFlow before saving.
            UserFlow example1 = ExampleUserFlow();
            string firstName = example1.Name();
            UserFlowState firstState = example1.State();
            long firstTimeout = example1.Timeout();
            int firstValue = example1.Value();
            long firstEyeTime = example1.EyeTime();
            string firstBeginTime = example1.BeginTimeString();
            string firstEndTime = example1.EndTimeString();
            // Save followed by load.
            UserFlowReporter.Background();
            UserFlowReporter.Foreground();
            example1 = UserFlow.UserFlowForName(firstName);
            Assert.IsNotNull(example1,
                           "Expecting to find example1 again");
            // Extract fields from loaded userFlow.
            string secondName = example1.Name();
            UserFlowState secondState = example1.State();
            long secondTimeout = example1.Timeout();
            int secondValue = example1.Value();
            long secondEyeTime = example1.EyeTime();
            string secondBeginTime = example1.BeginTimeString();
            string secondEndTime = example1.EndTimeString();
            // Everything is supposed to match now (within limits of persisting doubles).
            Assert.IsTrue(firstState == secondState,
                         "Expecting firstState==secondState");
            Assert.IsTrue(firstName == secondName,
                         "Expecting firstName==secondName");
            Assert.IsTrue(firstTimeout == secondTimeout,
                         "Expecting firstTimeout==secondTimeout");
            Assert.IsTrue(firstValue == secondValue,
                         "Expecting firstValue==secondValue");
            Assert.IsTrue(firstEyeTime == secondEyeTime,
                         "Expecting firstEyeTime==secondEyeTime");
            Assert.IsTrue(firstBeginTime == secondBeginTime,
                         "Expecting firstBeginTime==secondBeginTime");
            Assert.IsTrue(firstEndTime == secondEndTime,
                         "Expecting firstEndTime==secondEndTime");
            //UserFlowReporter.Background();
            Trace.WriteLine("testSaveLoad EXITING");
            Trace.WriteLine("");
        }

        [TestMethod]
        public void TestPersistence() {
            // Test Persistence, UserFlowForName
            // Intially, example1 = new UserFlow("Purchase Crittercism SDK", 100000);
            UserFlow example1 = ExampleUserFlow();
            string firstName = example1.Name();
            int firstTimeout = example1.Timeout();
            int firstValue = example1.Value();
            // Test userFlowForName
            Assert.IsTrue(UserFlow.UserFlowForName(firstName) == example1,
                          "Expecting UserFlow.UserFlowForName(firstName)==example1");
            Assert.IsTrue(UserFlow.UserFlowForName(firstName).Name() == firstName,
                          "Expecting UserFlow.UserFlowForName(firstName).Name()==firstName");
            // And example2 is example1's identical twin
            UserFlow example2 = ExampleUserFlow();
            Trace.WriteLine("INITIALLY EQUAL");
            //JsonConvert.SerializeObject(example1)
            Trace.WriteLine("example1 == " + example1);
            Trace.WriteLine("example2 == " + example2);
            // Change example1
            example1.SetTimeout(example1.Timeout() + 100);
            example1.SetValue(example1.Value() + 10000);
            // Confirm members of example1 have been changed.
            Assert.IsFalse(example1.Timeout() == firstTimeout,
                           "Not expecting example1.Timeout()==firstTimeout");
            Assert.IsFalse(example1.Value() == firstValue,
                           "Not expecting example1.Value()==firstValue");
            Trace.WriteLine("NO LONGER EQUAL");
            Trace.WriteLine("example1 == " + example1);
            Trace.WriteLine("example2 == " + example2);
            // Change example1 back
            example1.SetTimeout(firstTimeout);
            example1.SetValue(firstValue);
            Trace.WriteLine("SHOULD BE EQUAL AGAIN");
            Trace.WriteLine("example1 == " + example1);
            Trace.WriteLine("example2 == " + example2);
            // Confirm members of example1 have been restored.
            Assert.IsTrue(example1.Name() == firstName,
                          "Expecting example1.Name()==firstName");
            Assert.IsTrue(example1.Timeout() == firstTimeout,
                          "Expecting example1.Timeout()==firstTimeout");
            Assert.IsTrue(example1.Value() == firstValue,
                          "Expecting example1.Value()==firstValue");
        }
        #endregion

        #region UserFlow Static API
        [TestMethod]
        public void TestStaticAPIBeginUserFlow() {
            // Test "Static API" "BeginUserFlow(name)" method
            string exampleName = "Purchase Crittercism SDK";
            // Resulting value of "BeginUserFlow(name)" is correct.
            Crittercism.BeginUserFlow(exampleName);
            Assert.IsNotNull(UserFlow.UserFlowForName(exampleName),
                             "Confirm begun userFlow is accessible");
            Assert.IsTrue(UserFlow.UserFlowForName(exampleName).State() == UserFlowState.BEGUN,
                          "Confirm BeginUserFlow changes state to UserFlowState.BEGUN");
            Crittercism.EndUserFlow(exampleName);
            Assert.IsNull(UserFlow.UserFlowForName(exampleName),
                          "Confirm finished userFlow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIBeginUserFlowWithValue() {
            // Test "Static API" "BeginUserFlow(name,value)" method
            string exampleName = "Purchase Crittercism SDK";
            int exampleValue = 12345678;
            // Resulting value of "BeginUserFlow(name,value)" is correct.
            Crittercism.BeginUserFlow(exampleName,exampleValue);
            Assert.IsTrue(Crittercism.GetUserFlowValue(exampleName) == exampleValue,
                          String.Format("Confirm BeginUserFlow changes value to {0} #2",
                                        exampleValue));
            Crittercism.EndUserFlow(exampleName);
            Assert.IsNull(UserFlow.UserFlowForName(exampleName),
                          "Confirm finished userFlow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIEndUserFlow() {
            // Test "Static API" "EndUserFlow"
            string exampleName = "Purchase Crittercism SDK";
            // Resulting state of "EndUserFlow" is correct.  It's required to "Begin" first.
            Crittercism.BeginUserFlow(exampleName);
            Assert.IsTrue(UserFlow.UserFlowForName(exampleName).State() == UserFlowState.BEGUN,
                         "Confirm BeginUserFlow changes state to UserFlowState.BEGUN");
            Crittercism.EndUserFlow(exampleName);
            Assert.IsNull(UserFlow.UserFlowForName(exampleName),
                        "Confirm finished userFlow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIFailUserFlow() {
            // Test "Static API" "FailUserFlow"
            string exampleName = "Purchase Crittercism SDK";
            // Resulting state of "FailUserFlow" is correct.  It's required to "Begin" first.
            Crittercism.BeginUserFlow(exampleName);
            Assert.IsTrue(UserFlow.UserFlowForName(exampleName).State() == UserFlowState.BEGUN,
                         "Confirm BeginUserFlow changes state to UserFlowState.BEGUN");
            Crittercism.FailUserFlow(exampleName);
            Assert.IsNull(UserFlow.UserFlowForName(exampleName),
                        "Confirm finished userFlow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIUserFlowValueProperty() {
            // Test "Static API" "GetUserFlowValue" and "SetUserFlowValue"
            string exampleName = "Purchase Crittercism SDK";
            const int exampleValue = 12345678; // $123456.78
            const int value1 = 1234; // $12.34
            const int value2 = 9999; // $99.99
            Crittercism.BeginUserFlow(exampleName,exampleValue);
            // Set and get the property value.
            Crittercism.SetUserFlowValue(exampleName,value1);
            Assert.IsTrue(Crittercism.GetUserFlowValue(exampleName) == value1,
                         "Expecting Crittercism.GetUserFlowValue(exampleName) == {0} #1",value1);
            // Set and get the property value.
            Crittercism.SetUserFlowValue(exampleName,value2);
            Assert.IsTrue(Crittercism.GetUserFlowValue(exampleName) == value2,
                         "Expecting Crittercism.GetUserFlowValue(exampleName) == {0} #2",value2);
        }

        [TestMethod]
        public void TestStaticAPIInterrupt() {
            // Test "Static API" "Interrupt" works as expected.
            string exampleName = "Purchase Crittercism SDK";
            // First "Static API" call.
            Crittercism.BeginUserFlow(exampleName);
            UserFlow firstUserFlow = UserFlow.UserFlowForName(exampleName);
            // Second "Static API" call.
            Crittercism.BeginUserFlow(exampleName);
            UserFlow secondUserFlow = UserFlow.UserFlowForName(exampleName);
            // Confirm firstUserFlow has been "Interrupt"ed.
            Assert.IsTrue(firstUserFlow.State() == UserFlowState.CANCELLED,
                         "Confirm BeginUserFlow changes state to UserFlowState.CANCELLED");
            // Confirm secondUserFlow has begun
            Assert.IsTrue(secondUserFlow.State() == UserFlowState.BEGUN,
                         "Confirm BeginUserFlow changes state to UserFlowState.BEGUN");
            // Finish up.
            Crittercism.EndUserFlow(exampleName);
            Assert.IsNull(UserFlow.UserFlowForName(exampleName),
                        "Confirm finished userFlow no longer accessible");
        }
        #endregion
    }
}
