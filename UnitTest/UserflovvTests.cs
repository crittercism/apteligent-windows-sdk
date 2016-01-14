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
    public class UserflowTests {
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

        #region Example Userflow
        internal Userflow ExampleUserflow() {
            Userflow answer = new Userflow("Purchase Crittercism SDK",100000);
            answer.SetTimeout(300 * MSEC_PER_SEC);
            return answer;
        }
        #endregion

        [TestInitialize()]
        public void TestInitialize() {
            // Use TestInitialize to run code before running each test 
            UserflowReporter.Init();
        }

        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            UserflowReporter.Shutdown();
        }

        #region Userflow state property
        [TestMethod]
        public void TestStateProperty() {
            // Check state is correct UserflowState after
            // various Userflow method calls.
            {
                Userflow example1 = ExampleUserflow();
                Assert.IsTrue(example1.State() == UserflowState.CREATED,
                             "Expecting UserflowState.CREATED .");
                {
                    example1.Begin(); ;
                    Assert.IsTrue(example1.State() == UserflowState.BEGUN,
                                 "Expecting UserflowState.BEGUN .");
                }
                Thread.Sleep(100);
                {
                    example1.End(); ;
                    Assert.IsTrue(example1.State() == UserflowState.ENDED,
                                 "Expecting UserflowState.ENDED .");
                }
            }
            {
                Userflow example2 = ExampleUserflow();
                Assert.IsTrue(example2.State() == UserflowState.CREATED,
                             "Expecting UserflowState.CREATED .");
                {
                    example2.Begin(); ;
                    Assert.IsTrue(example2.State() == UserflowState.BEGUN,
                                 "Expecting UserflowState.BEGUN .");
                }
                Thread.Sleep(100);
                {
                    example2.Fail();
                    Assert.IsTrue(example2.State() == UserflowState.FAILED,
                                 "Expecting UserflowState.FAILED .");
                }
            }
            {
                Userflow example3 = ExampleUserflow();
                Assert.IsTrue(example3.State() == UserflowState.CREATED,
                             "Expecting UserflowState.CREATED .");
                const int timeout = 100;
                example3.SetTimeout(timeout);
                {
                    example3.Begin();
                    Assert.IsTrue(example3.State() == UserflowState.BEGUN,
                                 "Expecting UserflowState.BEGUN .");
                }
                // Yield here so CLR can operate example3's timer .
                Thread.Sleep(2 * timeout);
                Assert.IsTrue(example3.State() == UserflowState.TIMEOUT,
                             "Expecting UserflowState.TIMEOUT .");
            }
        }
        #endregion

        #region Userflow timeout property
        [TestMethod]
        public void TestTimeoutProperty() {
            // Userflow should TIMEOUT if run in excess of it's timeout.
            Userflow example = ExampleUserflow();
            Assert.IsTrue(example.State() == UserflowState.CREATED,
                         "Expecting UserflowState.CREATED .");
            const int timeout = 100;
            example.SetTimeout(timeout);
            {
                example.Begin();
                Assert.IsTrue(example.State() == UserflowState.BEGUN,
                             "Expecting UserflowState.BEGUN .");
            }
            // Yield here so CLR can operate example3's timer .
            Thread.Sleep(2 * timeout);
            Assert.IsTrue(example.State() == UserflowState.TIMEOUT,
                         "Expecting UserflowState.TIMEOUT .");
        }
        #endregion

        #region Userflow value property
        [TestMethod]
        public void TestValueProperty() {
            // Confirm value property is working.
            const int value1 = 1234; // $12.34
            const int value2 = 9999; // $99.99
            Userflow example = ExampleUserflow();
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

        #region Userflow times properties
        [TestMethod]
        public void TestTimeProperties() {
            // Testing beginTime, endTime, eyeTime
            Userflow example = ExampleUserflow();
            Assert.IsTrue(example.State() == UserflowState.CREATED,
                         "Expecting UserflowState.CREATED .");
            example.SetTimeout(Int32.MaxValue);
            {
                example.Begin();
                Assert.IsTrue(example.State() == UserflowState.BEGUN,
                             "Expecting UserflowState.BEGUN .");
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
                Assert.IsTrue(example.State() == UserflowState.ENDED,
                             "Expecting UserflowState.ENDED .");
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

        #region Userflow max userflow limit
        [TestMethod]
        public void TestMaxUserflowCount() {
            for (int i = 0; i < 2 * UserflowReporter.MAX_USERFLOW_COUNT; i++) {
                Crittercism.BeginUserflow(i.ToString());
            }
            Object[] loggedUserflows = UserflowReporter.AllUserflows();
            Assert.AreEqual(UserflowReporter.UserflowCount(),UserflowReporter.MAX_USERFLOW_COUNT,
                            String.Format("Expecting userflowCount : {0} to be the maximum count : {1} .",
                                          UserflowReporter.UserflowCount(),
                                          UserflowReporter.MAX_USERFLOW_COUNT));
            Assert.AreEqual(UserflowReporter.UserflowCount(),loggedUserflows.Length,
                            String.Format("Expecting userflowCount : {0} to be the # of userflows : {1} .",
                                          UserflowReporter.UserflowCount(),
                                          loggedUserflows.Length));
            foreach (Userflow userflow in loggedUserflows) {
                int index = Int32.Parse(userflow.Name());
                Assert.IsTrue(((index >= 0) && (index < UserflowReporter.MAX_USERFLOW_COUNT)),
                              String.Format("Expecting userflow name to be an expected value between 0 and"
                                            + "MAX_USERFLOW_COUNT ({0}), name was {1}",
                                            UserflowReporter.MAX_USERFLOW_COUNT,
                                            index));
            }
        }
        #endregion

        #region Userflow(name) constructor
        [TestMethod]
        public void TestInit() {
            string name1 = "";
            Userflow example = new Userflow(name1);
            Assert.IsTrue(example.Name() == name1,
                         "Confirm constructor sets name property.");
        }

        [TestMethod]
        public void TestInitLongString() {
            Userflow example = new Userflow(ExampleLongString);
            Trace.WriteLine("example.Name() == " + example.Name());
            Assert.IsTrue(example.Name() == ExampleTruncatedString,
                         "Confirm constructor sets name property.");
        }
        #endregion

        #region Userflow(name,value) constructor
        [TestMethod]
        public void TestInitWithValue() {
            string name1 = "";
            int value1 = 2000;
            Userflow example = new Userflow(name1,value1);
            Assert.IsTrue(example.Name() == name1,
                         "Confirm constructor sets name property.");
            Assert.IsTrue(example.Value() == value1,
                         "Confirm constructor sets value property.");
        }

        [TestMethod]
        public void TestInitLongStringWithValue() {
            int value1 = 2000;
            Userflow example = new Userflow(ExampleLongString,value1);
            Assert.IsTrue(example.Name() == ExampleTruncatedString,
                         "Confirm constructor sets name property.");
            Assert.IsTrue(example.Value() == value1,
                         "Confirm constructor sets value property.");
        }
        #endregion

        #region Userflow Begin() method
        [TestMethod]
        public void TestBegin() {
            // Test "Begin"'s resulting state is correct.
            Userflow example = ExampleUserflow();
            example.Begin();
            Assert.IsTrue(example.State() == UserflowState.BEGUN,
                         "Confirm Begin changes state to UserflowState.BEGUN");
        }
        #endregion

        #region Userflow End() method
        [TestMethod]
        public void TestSuccess() {
            // Test "End"'s resulting state is correct.  It's required to "Begin" first.
            Userflow example = ExampleUserflow();
            example.Begin();
            Assert.IsTrue(example.State() == UserflowState.BEGUN,
                         "Confirm Begin changes state to UserflowState.BEGUN");
            example.End();
            Assert.IsTrue(example.State() == UserflowState.ENDED,
                         "Confirm End changes state to UserflowState.ENDED");
        }
        #endregion

        #region Userflow Fail method
        [TestMethod]
        public void TestFail() {
            // Test "Fail"'s resulting state is correct.  It's required to "begin" first.
            Userflow example = ExampleUserflow();
            example.Begin();
            Assert.IsTrue(example.State() == UserflowState.BEGUN,
                         "Confirm Begin changes state to UserflowState.BEGUN");
            example.Fail();
            Assert.IsTrue(example.State() == UserflowState.FAILED,
                         "Confirm Fail changes state to UserflowState.FAILED");
        }
        #endregion

        #region Userflow Transition method
        [TestMethod]
        public void TestTransition() {
            {
                // Resulting state are correct.
                UserflowState state1 = UserflowState.BEGUN;
                Userflow example = ExampleUserflow();
                example.Transition(state1);
                Assert.IsTrue(example.State() == state1,
                             "Confirm Transition changes state to given state");
            }
        }
        #endregion

        #region Userflow ToArray method
        [TestMethod]
        public void TestUserflowToJArray() {
            Userflow example1 = ExampleUserflow();
            JArray json = example1.ToJArray();
            Assert.IsTrue(UserflowConverter.IsUserflowJson(json));
        }
        #endregion

        #region Userflow toJSONString method
        public void CheckJSONString(string jsonString) {
            // Confirm this jsonString looks like proper JSON for a Userflow .
            // NOTE: userflow represented as JSON to server
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
                         "Expecting userflow JSON string representing JSON array.");
            Trace.WriteLine(String.Format("json == " + json));
            // TODO: Following commented out test statement is still broken.
            // JsonConvert.DeserializeObject is doing some unwanted weird stuff.
            //Assert.IsTrue(UserflowConverter.IsUserflowJson(json));
        }

        [TestMethod]
        public void TestUserflowToString() {
            // Confirm ToString's return value parses as plausible JSON .
            Userflow example1 = ExampleUserflow();
            CheckJSONString(example1.ToString());
        }
        #endregion

        #region Userflow Persistence
        [TestMethod]
        public void TestSaveLoad() {
            // Load saved userflow.  Does it look the same?
            // Extract fields from userflow before saving.
            Userflow example1 = ExampleUserflow();
            string firstName = example1.Name();
            UserflowState firstState = example1.State();
            long firstTimeout = example1.Timeout();
            int firstValue = example1.Value();
            long firstEyeTime = example1.EyeTime();
            string firstBeginTime = example1.BeginTimeString();
            string firstEndTime = example1.EndTimeString();
            // Save followed by load.
            UserflowReporter.Background();
            UserflowReporter.Foreground();
            example1 = Userflow.UserflowForName(firstName);
            Assert.IsNotNull(example1,
                           "Expecting to find example1 again");
            // Extract fields from loaded userflow.
            string secondName = example1.Name();
            UserflowState secondState = example1.State();
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
            //UserflowReporter.Background();
            Trace.WriteLine("testSaveLoad EXITING");
            Trace.WriteLine("");
        }

        [TestMethod]
        public void TestPersistence() {
            // Test Persistence, UserflowForName
            // Intially, example1 = new Userflow("Purchase Crittercism SDK", 100000);
            Userflow example1 = ExampleUserflow();
            string firstName = example1.Name();
            int firstTimeout = example1.Timeout();
            int firstValue = example1.Value();
            // Test userflowForName
            Assert.IsTrue(Userflow.UserflowForName(firstName) == example1,
                          "Expecting Userflow.UserflowForName(firstName)==example1");
            Assert.IsTrue(Userflow.UserflowForName(firstName).Name() == firstName,
                          "Expecting Userflow.UserflowForName(firstName).Name()==firstName");
            // And example2 is example1's identical twin
            Userflow example2 = ExampleUserflow();
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

        #region Userflow Static API
        [TestMethod]
        public void TestStaticAPIBeginUserflow() {
            // Test "Static API" "BeginUserflow(name)" method
            string exampleName = "Purchase Crittercism SDK";
            // Resulting value of "BeginUserflow(name)" is correct.
            Crittercism.BeginUserflow(exampleName);
            Assert.IsNotNull(Userflow.UserflowForName(exampleName),
                             "Confirm begun userflow is accessible");
            Assert.IsTrue(Userflow.UserflowForName(exampleName).State() == UserflowState.BEGUN,
                          "Confirm BeginUserflow changes state to UserflowState.BEGUN");
            Crittercism.EndUserflow(exampleName);
            Assert.IsNull(Userflow.UserflowForName(exampleName),
                          "Confirm finished userflow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIBeginUserflowWithValue() {
            // Test "Static API" "BeginUserflow(name,value)" method
            string exampleName = "Purchase Crittercism SDK";
            int exampleValue = 12345678;
            // Resulting value of "BeginUserflow(name,value)" is correct.
            Crittercism.BeginUserflow(exampleName,exampleValue);
            Assert.IsTrue(Crittercism.GetUserflowValue(exampleName) == exampleValue,
                          String.Format("Confirm BeginUserflow changes value to {0} #2",
                                        exampleValue));
            Crittercism.EndUserflow(exampleName);
            Assert.IsNull(Userflow.UserflowForName(exampleName),
                          "Confirm finished userflow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIEndUserflow() {
            // Test "Static API" "EndUserflow"
            string exampleName = "Purchase Crittercism SDK";
            // Resulting state of "EndUserflow" is correct.  It's required to "Begin" first.
            Crittercism.BeginUserflow(exampleName);
            Assert.IsTrue(Userflow.UserflowForName(exampleName).State() == UserflowState.BEGUN,
                         "Confirm BeginUserflow changes state to UserflowState.BEGUN");
            Crittercism.EndUserflow(exampleName);
            Assert.IsNull(Userflow.UserflowForName(exampleName),
                        "Confirm finished userflow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIFailUserflow() {
            // Test "Static API" "FailUserflow"
            string exampleName = "Purchase Crittercism SDK";
            // Resulting state of "FailUserflow" is correct.  It's required to "Begin" first.
            Crittercism.BeginUserflow(exampleName);
            Assert.IsTrue(Userflow.UserflowForName(exampleName).State() == UserflowState.BEGUN,
                         "Confirm BeginUserflow changes state to UserflowState.BEGUN");
            Crittercism.FailUserflow(exampleName);
            Assert.IsNull(Userflow.UserflowForName(exampleName),
                        "Confirm finished userflow no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIUserflowValueProperty() {
            // Test "Static API" "GetUserflowValue" and "SetUserflowValue"
            string exampleName = "Purchase Crittercism SDK";
            const int exampleValue = 12345678; // $123456.78
            const int value1 = 1234; // $12.34
            const int value2 = 9999; // $99.99
            Crittercism.BeginUserflow(exampleName,exampleValue);
            // Set and get the property value.
            Crittercism.SetUserflowValue(exampleName,value1);
            Assert.IsTrue(Crittercism.GetUserflowValue(exampleName) == value1,
                         "Expecting Crittercism.GetUserflowValue(exampleName) == {0} #1",value1);
            // Set and get the property value.
            Crittercism.SetUserflowValue(exampleName,value2);
            Assert.IsTrue(Crittercism.GetUserflowValue(exampleName) == value2,
                         "Expecting Crittercism.GetUserflowValue(exampleName) == {0} #2",value2);
        }

        [TestMethod]
        public void TestStaticAPIInterrupt() {
            // Test "Static API" "Interrupt" works as expected.
            string exampleName = "Purchase Crittercism SDK";
            // First "Static API" call.
            Crittercism.BeginUserflow(exampleName);
            Userflow firstUserflow = Userflow.UserflowForName(exampleName);
            // Second "Static API" call.
            Crittercism.BeginUserflow(exampleName);
            Userflow secondUserflow = Userflow.UserflowForName(exampleName);
            // Confirm firstUserflow has been "Interrupt"ed.
            Assert.IsTrue(firstUserflow.State() == UserflowState.CANCELLED,
                         "Confirm BeginUserflow changes state to UserflowState.CANCELLED");
            // Confirm secondUserflow has begun
            Assert.IsTrue(secondUserflow.State() == UserflowState.BEGUN,
                         "Confirm BeginUserflow changes state to UserflowState.BEGUN");
            // Finish up.
            Crittercism.EndUserflow(exampleName);
            Assert.IsNull(Userflow.UserflowForName(exampleName),
                        "Confirm finished userflow no longer accessible");
        }
        #endregion
    }
}
