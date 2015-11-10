using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using CrittercismSDK;

namespace UnitTest {
    /// <summary>
    /// Summary description for TransactionTests
    /// </summary>
    [TestClass]
    public class TransactionTests {
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

        #region Example Transaction
        internal Transaction ExampleTransaction() {
            Transaction answer = new Transaction("Purchase Crittercism SDK",100000);
            answer.SetTimeout(300 * MSEC_PER_SEC);
            return answer;
        }
        #endregion

        #region Transaction state property
        [TestMethod]
        public void TestStateProperty() {
            // Check "state" is correct TransactionState after
            // various Transaction method calls.
            {
                Transaction example1 = ExampleTransaction();
                Assert.IsTrue(example1.State() == TransactionState.CREATED,
                             "Expecting TransactionState.CREATED .");
                {
                    example1.Begin(); ;
                    Assert.IsTrue(example1.State() == TransactionState.BEGUN,
                                 "Expecting TransactionState.BEGUN .");
                }
                Thread.Sleep(100);
                {
                    example1.End(); ;
                    Assert.IsTrue(example1.State() == TransactionState.ENDED,
                                 "Expecting TransactionState.ENDED .");
                }
            }
            {
                Transaction example2 = ExampleTransaction();
                Assert.IsTrue(example2.State() == TransactionState.CREATED,
                             "Expecting TransactionState.CREATED .");
                {
                    example2.Begin(); ;
                    Assert.IsTrue(example2.State() == TransactionState.BEGUN,
                                 "Expecting TransactionState.BEGUN .");
                }
                Thread.Sleep(100);
                {
                    example2.Fail();
                    Assert.IsTrue(example2.State() == TransactionState.FAILED,
                                 "Expecting TransactionState.FAILED .");
                }
            }
            {
                Transaction example3 = ExampleTransaction();
                Assert.IsTrue(example3.State() == TransactionState.CREATED,
                             "Expecting TransactionState.CREATED .");
                const int timeout = 100;
                example3.SetTimeout(timeout);
                {
                    example3.Begin();
                    Assert.IsTrue(example3.State() == TransactionState.BEGUN,
                                 "Expecting TransactionState.BEGUN .");
                }
                // Yield here so CLR can operate example3's timer .
                Thread.Sleep(2 * timeout);
                Assert.IsTrue(example3.State() == TransactionState.TIMEOUT,
                             "Expecting TransactionState.TIMEOUT .");
            }
        }
        #endregion

        #region Transaction timeout property
        [TestMethod]
        public void TestTimeoutProperty() {
            // Transaction should TIMEOUT if run in excess of it's timeout.
            Transaction example = ExampleTransaction();
            Assert.IsTrue(example.State() == TransactionState.CREATED,
                         "Expecting TransactionState.CREATED .");
            const int timeout = 100;
            example.SetTimeout(timeout);
            {
                example.Begin();
                Assert.IsTrue(example.State() == TransactionState.BEGUN,
                             "Expecting TransactionState.BEGUN .");
            }
            // Yield here so CLR can operate example3's timer .
            Thread.Sleep(2 * timeout);
            Assert.IsTrue(example.State() == TransactionState.TIMEOUT,
                         "Expecting TransactionState.TIMEOUT .");
        }
        #endregion

        #region Transaction value property
        [TestMethod]
        public void TestValueProperty() {
            // Confirm value property is working.
            const int value1 = 1234; // $12.34
            const int value2 = 9999; // $99.99
            Transaction example = ExampleTransaction();
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

        #region Transaction times properties
        [TestMethod]
        public void TestTimeProperties() {
            // Testing beginTime, endTime, eyeTime
            Transaction example = ExampleTransaction();
            Assert.IsTrue(example.State() == TransactionState.CREATED,
                         "Expecting TransactionState.CREATED .");
            example.SetTimeout(Int32.MaxValue);
            {
                example.Begin();
                Assert.IsTrue(example.State() == TransactionState.BEGUN,
                             "Expecting TransactionState.BEGUN .");
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
                Assert.IsTrue(example.State() == TransactionState.ENDED,
                             "Expecting TransactionState.ENDED .");
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

        #region Transaction max transaction limit
        [TestMethod]
        public void TestMaxTransactionCount() {
            for (int i = 0; i < 2 * TransactionReporter.MAX_TRANSACTION_COUNT; i++) {
                Crittercism.BeginTransaction(i.ToString());
            }
            Object[] loggedTransactions = TransactionReporter.AllTransactions();
            Assert.AreEqual(TransactionReporter.TransactionCount(),TransactionReporter.MAX_TRANSACTION_COUNT,
                            String.Format("Expecting transactionCount : {0} to be the maximum count : {1} .",
                                          TransactionReporter.TransactionCount(),
                                          TransactionReporter.MAX_TRANSACTION_COUNT));
            Assert.AreEqual(TransactionReporter.TransactionCount(),loggedTransactions.Length,
                            String.Format("Expecting transactionCount : {0} to be the # of transactions : {1} .",
                                          TransactionReporter.TransactionCount(),
                                          loggedTransactions.Length));
            foreach (Transaction transaction in loggedTransactions) {
                int index = Int32.Parse(transaction.Name());
                Assert.IsTrue(((index >= 0) && (index < TransactionReporter.MAX_TRANSACTION_COUNT)),
                              String.Format("Expecting transaction name to be an expected value between 0 and"
                                            + "MAX_TRANSACTION_COUNT ({0}), name was {1}",
                                            TransactionReporter.MAX_TRANSACTION_COUNT,
                                            index));
            }
        }
        #endregion

        #region Transaction init: method
        [TestMethod]
        public void TestInit() {
            string name1 = "";
            Transaction example = new Transaction(name1);
            Assert.IsTrue(example.Name() == name1,
                         "Confirm constructor sets name property.");
        }

        [TestMethod]
        public void TestInitLongString() {
            Transaction example = new Transaction(ExampleLongString);
            Debug.WriteLine("example.Name() == {0}",example.Name());
            Assert.IsTrue(example.Name() == ExampleTruncatedString,
                         "Confirm constructor sets name property.");
        }
        #endregion

        #region Transaction init:withValue: method
        [TestMethod]
        public void TestInitWithValue() {
            string name1 = "";
            int value1 = 2000;
            Transaction example = new Transaction(name1,value1);
            Assert.IsTrue(example.Name() == name1,
                         "Confirm constructor sets name property.");
            Assert.IsTrue(example.Value() == value1,
                         "Confirm constructor sets value property.");
        }

        [TestMethod]
        public void TestInitLongStringWithValue() {
            int value1 = 2000;
            Transaction example = new Transaction(ExampleLongString,value1);
            Assert.IsTrue(example.Name() == ExampleTruncatedString,
                         "Confirm constructor sets name property.");
            Assert.IsTrue(example.Value() == value1,
                         "Confirm constructor sets value property.");
        }
        #endregion

        #region Transaction begin: method
        [TestMethod]
        public void TestBegin() {
            // Test "begin"'s resulting state is correct.
            Transaction example = ExampleTransaction();
            example.Begin();
            Assert.IsTrue(example.State() == TransactionState.BEGUN,
                         "Confirm begin: changes state to TransactionState.BEGUN");
        }
        #endregion

        #region Transaction end: method
        [TestMethod]
        public void TestSuccess() {
            // Test "end"'s resulting state is correct.  It's required to "begin" first.
            Transaction example = ExampleTransaction();
            example.Begin();
            Assert.IsTrue(example.State() == TransactionState.BEGUN,
                         "Confirm begin: changes state to TransactionState.BEGUN");
            example.End();
            Assert.IsTrue(example.State() == TransactionState.ENDED,
                         "Confirm end: changes state to TransactionState.ENDED");
        }
        #endregion

        #region Transaction fail: method
        [TestMethod]
        public void TestFail() {
            // Test "fail"'s resulting state is correct.  It's required to "begin" first.
            Transaction example = ExampleTransaction();
            example.Begin();
            Assert.IsTrue(example.State() == TransactionState.BEGUN,
                         "Confirm begin: changes state to TransactionState.BEGUN");
            example.Fail();
            Assert.IsTrue(example.State() == TransactionState.FAILED,
                         "Confirm fail: changes state to TransactionState.FAILED");
        }
        #endregion

        #region Transaction transition: method
        [TestMethod]
        public void TestTransition() {
            {
                // Resulting state are correct.
                TransactionState state1 = TransactionState.BEGUN;
                Transaction example = ExampleTransaction();
                example.Transition(state1);
                Assert.IsTrue(example.State() == state1,
                             "Confirm transition: changes state to given state");
            }
        }
        #endregion

        #region Transaction ToArray method
        public void CheckJSONArray(Object json) {
            Object[] jsonArray = json as Object[];
            Assert.IsNotNull(jsonArray,"Expecting transaction jsonArray");
            if (jsonArray != null) {
                Debug.WriteLine("jsonArray.Count=={0}",jsonArray.Length);
                Assert.IsTrue(jsonArray.Length == (int)TransactionIndex.COUNT,
                             "Expecting transaction jsonArray with correct count.");
                Assert.IsTrue(jsonArray[(int)TransactionIndex.Name] is String,
                              String.Format("Expecting objectAtIndex:{0} to be an String",
                                            TransactionIndex.Name));
                Assert.IsTrue(JsonUtils.IsNumber(jsonArray[(int)TransactionIndex.State]),
                              String.Format("Expecting objectAtIndex:{0} to be a Number",
                                            TransactionIndex.State));
                // NOTE: Extended reals is the set of reals with +/-infinity included.  These are
                // represented as NSNumber's whether finite or INFINITY .  However, JSON numbers are
                // finite.  We indicate INFINITY to platform via JSON string "INFINITY".
                // See ExtendedRealToJSONString and JsonStringToExtendedReal in JsonUtils.cs .
                Assert.IsTrue((JsonUtils.IsNumber(jsonArray[(int)TransactionIndex.Timeout])
                              || (jsonArray[(int)TransactionIndex.Timeout] is String)),
                              String.Format("Expecting objectAtIndex:{0} to be an NSNumber or NSString",
                                            TransactionIndex.Timeout));
                Assert.IsTrue(JsonUtils.IsNumber(jsonArray[(int)TransactionIndex.Value]),
                              String.Format("Expecting objectAtIndex:{0} to be an NSNumber",
                                            TransactionIndex.Value));
                Assert.IsTrue(jsonArray[(int)TransactionIndex.Metadata] is Dictionary<string,string>,
                              String.Format("Expecting objectAtIndex:{0} to be an NSDictionary",
                                            TransactionIndex.Metadata));
                Assert.IsTrue(jsonArray[(int)TransactionIndex.BeginTime] is String,
                              String.Format("Expecting objectAtIndex:{0} to be an NSString",
                                            TransactionIndex.BeginTime));
                Assert.IsTrue(jsonArray[(int)TransactionIndex.EndTime] is String,
                              String.Format("Expecting objectAtIndex:{0} to be an NSString",
                                            TransactionIndex.EndTime));
                Assert.IsTrue(JsonUtils.IsNumber(jsonArray[(int)TransactionIndex.EyeTime]),
                              String.Format("Expecting objectAtIndex:{0} to be an NSNumber",
                                            TransactionIndex.EyeTime));
                Assert.IsTrue(jsonArray[(int)TransactionIndex.ForegroundTime] is String,
                              String.Format("Expecting objectAtIndex:{0} to be an NSString",
                                            TransactionIndex.ForegroundTime));
                Assert.IsTrue(JsonUtils.IsNumber(jsonArray[(int)TransactionIndex.IsForegrounded]),
                              String.Format("Expecting objectAtIndex:{0} to be an NSNumber",
                                            TransactionIndex.IsForegrounded));
                Debug.WriteLine("");
            };
        }

        [TestMethod]
        public void TestToArray() {
            Transaction example1 = ExampleTransaction();
            Object[] jsonArray = example1.ToArray();
            CheckJSONArray(jsonArray);
        }
        #endregion

        #region Transaction toJSONString method
        public void CheckJSONString(string jsonString) {
            // Confirm this jsonString looks like proper JSON for a Transaction .
            // NOTE: transaction represented as JSON to server
            //     == [hash,id,name,state,timeout,value,
            //         metadata,beginTime,endTime,eyeTime]
            Object json = null;
            try {
                json = JsonConvert.DeserializeObject(jsonString);
            } catch (JsonException e) {
                // There should be no "error" .
                Assert.IsNull(e,
                            "Expection a legal JSON string that parses correctly #1.");
                if (e!=null) {
                    Debug.WriteLine("error == {0}",e.Message);
                }
            }
            // The "json" should be an NSArray .
            Assert.IsNotNull(json,
                           "Expection a legal JSON string that parses correctly #2.");
            Debug.WriteLine("Converted jsonString's class == \"{0}\"",json.GetType().FullName);
            Assert.IsTrue(json is Object[],
                         "Expecting transaction JSON string representing JSON array.");
            Debug.WriteLine("json == {0}",json);
            CheckJSONArray(json);
        }

        [TestMethod]
        public void TestToJSONString() {
            // Confirm ToJSONString's return value parses as plausible JSON .
            Transaction example1 = ExampleTransaction();
            CheckJSONString(example1.ToJSONString());
        }
        #endregion

        #region Transaction Persistence
        [TestMethod]
        public void TestSaveLoad() {
            // Load saved transaction.  Does it look the same?
            // Extract fields from transaction before saving.
            Transaction example1 = ExampleTransaction();
            string firstName = example1.Name();
            TransactionState firstState = example1.State();
            long firstTimeout = example1.Timeout();
            int firstValue = example1.Value();
            long firstEyeTime = example1.EyeTime();
            string firstBeginTime = example1.BeginTimeString();
            string firstEndTime = example1.EndTimeString();
            // Save followed by load.
            TransactionReporter.Background();
            TransactionReporter.Resume();
            example1 = Transaction.TransactionForName(firstName);
            Assert.IsNotNull(example1,
                           "Expecting to find example1 again");
            // Extract fields from loaded transaction.
            string secondName = example1.Name();
            TransactionState secondState = example1.State();
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
            //TransactionReporter.Background();
            Debug.WriteLine("testSaveLoad EXITING");
            Debug.WriteLine("");
        }

        [TestMethod]
        public void TestPersistence() {
            // Test Persistence, TransactionForName
            // Intially, example1 = new Transaction("Purchase Crittercism SDK", 100000);
            Transaction example1 = ExampleTransaction();
            string firstName = example1.Name();
            int firstTimeout = example1.Timeout();
            int firstValue = example1.Value();
            // Test transactionForName
            Assert.IsTrue(Transaction.TransactionForName(firstName) == example1,
                          "Expecting Transaction.TransactionForName(firstName)==example1");
            Assert.IsTrue(Transaction.TransactionForName(firstName).Name() == firstName,
                          "Expecting Transaction.TransactionForName(firstName).Name()==firstName");
            // And example2 is example1's identical twin
            Transaction example2 = ExampleTransaction();
            Debug.WriteLine("INITIALLY EQUAL");
            Debug.WriteLine("example1 == {0}",example1.ToArray());
            Debug.WriteLine("example2 == {0}",example2.ToArray());
            // Change example1
            example1.SetTimeout(example1.Timeout() + 100);
            example1.SetValue(example1.Value() + 10000);
            // Confirm members of example1 have been changed.
            Assert.IsFalse(example1.Timeout() == firstTimeout,
                           "Not expecting example1.Timeout()==firstTimeout");
            Assert.IsFalse(example1.Value() == firstValue,
                           "Not expecting example1.Value()==firstValue");
            Debug.WriteLine("NO LONGER EQUAL");
            Debug.WriteLine("example1 == {0}",example1.ToArray());
            Debug.WriteLine("example2 == {0}",example2.ToArray());
            // Change example1 back
            example1.SetTimeout(firstTimeout);
            example1.SetValue(firstValue);
            Debug.WriteLine("SHOULD BE EQUAL AGAIN");
            Debug.WriteLine("example1 == {0}",example1.ToArray());
            Debug.WriteLine("example2 == {0}",example2.ToArray());
            // Confirm members of example1 have been restored.
            Assert.IsTrue(example1.Name() == firstName,
                          "Expecting example1.Name()==firstName");
            Assert.IsTrue(example1.Timeout() == firstTimeout,
                          "Expecting example1.Timeout()==firstTimeout");
            Assert.IsTrue(example1.Value() == firstValue,
                          "Expecting example1.Value()==firstValue");
        }
        #endregion

        #region Transaction Static API
        [TestMethod]
        public void TestStaticAPIBeginTransaction() {
            // Test "Static API" "beginTransaction:" method
            string exampleName = "Purchase Crittercism SDK";
            // Resulting value of "beginTransaction:" is correct.
            Crittercism.BeginTransaction(exampleName);
            Assert.IsNotNull(Transaction.TransactionForName(exampleName),
                             "Confirm begun transaction is accessible");
            Assert.IsTrue(Transaction.TransactionForName(exampleName).State() == TransactionState.BEGUN,
                          "Confirm begin: changes state to TransactionState.BEGUN");
            Crittercism.EndTransaction(exampleName);
            Assert.IsNull(Transaction.TransactionForName(exampleName),
                          "Confirm finished transaction no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIBeginTransactionWithValue() {
            // Test "Static API" "beginTransaction:withValue:" method
            string exampleName = "Purchase Crittercism SDK";
            int exampleValue = 12345678;
            // Resulting value of "begin:withValue:" is correct.
            Crittercism.BeginTransaction(exampleName,exampleValue);
            Assert.IsTrue(Crittercism.GetTransactionValue(exampleName) == exampleValue,
                          String.Format("Confirm begin:withValue: changes value to{0} #2",
                                        exampleValue));
            Crittercism.EndTransaction(exampleName);
            Assert.IsNull(Transaction.TransactionForName(exampleName),
                          "Confirm finished transaction no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIEndTransaction() {
            // Test "Static API" "endTransaction:"
            string exampleName = "Purchase Crittercism SDK";
            // Resulting state of "endTransaction:" is correct.  It's required to "begin" first.
            Crittercism.BeginTransaction(exampleName);
            Assert.IsTrue(Transaction.TransactionForName(exampleName).State() == TransactionState.BEGUN,
                         "Confirm begin: changes state to TransactionState.BEGUN");
            Crittercism.EndTransaction(exampleName);
            Assert.IsNull(Transaction.TransactionForName(exampleName),
                        "Confirm finished transaction no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPIFailTransaction() {
            // Test "Static API" "failTransaction:"
            string exampleName = "Purchase Crittercism SDK";
            // Resulting state of "failTransaction:" is correct.  It's required to "begin" first.
            Crittercism.BeginTransaction(exampleName);
            Assert.IsTrue(Transaction.TransactionForName(exampleName).State() == TransactionState.BEGUN,
                         "Confirm begin: changes state to TransactionState.BEGUN");
            Crittercism.FailTransaction(exampleName);
            Assert.IsNull(Transaction.TransactionForName(exampleName),
                        "Confirm finished transaction no longer accessible");
        }

        [TestMethod]
        public void TestStaticAPITransactionValueProperty() {
            // Test "Static API" "valueForTransaction:" and "setValue:forTransaction:"
            string exampleName = "Purchase Crittercism SDK";
            const int exampleValue = 12345678; // $123456.78
            const int value1 = 1234; // $12.34
            const int value2 = 9999; // $99.99
            Crittercism.BeginTransaction(exampleName,exampleValue);
            // Set and get the property value.
            Crittercism.SetTransactionValue(exampleName,value1);
            Assert.IsTrue(Crittercism.GetTransactionValue(exampleName) == value1,
                         "Expecting Crittercism.GetTransactionValue(exampleName) == {0} #1",value1);
            // Set and get the property value.
            Crittercism.SetTransactionValue(exampleName,value2);
            Assert.IsTrue(Crittercism.GetTransactionValue(exampleName) == value2,
                         "Expecting Crittercism.GetTransactionValue(exampleName) == {0} #2",value2);
        }

        [TestMethod]
        public void TestStaticAPIInterrupt() {
            // Test "Static API" "Interrupt" works as expected.
            string exampleName = "Purchase Crittercism SDK";
            // First "Static API" call.
            Crittercism.BeginTransaction(exampleName);
            Transaction firstTransaction = Transaction.TransactionForName(exampleName);
            // Second "Static API" call.
            Crittercism.BeginTransaction(exampleName);
            Transaction secondTransaction = Transaction.TransactionForName(exampleName);
            // Confirm firstTransaction has been "Interrupt"ed.
            Assert.IsTrue(firstTransaction.State() == TransactionState.INTERRUPTED,
                         "Confirm begin: changes state to TransactionState.INTERRUPTED");
            // Confirm secondTransaction has begun
            Assert.IsTrue(secondTransaction.State() == TransactionState.BEGUN,
                         "Confirm begin: changes state to TransactionState.BEGUN");
            // Finish up.
            Crittercism.EndTransaction(exampleName);
            Assert.IsNull(Transaction.TransactionForName(exampleName),
                        "Confirm finished transaction no longer accessible");
        }
        #endregion
    }
}
