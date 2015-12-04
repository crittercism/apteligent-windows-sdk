using CrittercismSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTest {
    [TestClass]
    public class JsonTests {
        [TestMethod]
        public void JObjectParseTest() {
            string json = ("{"
                + "\n  CPU: 'Intel',"
                + "\n  Drives: ["
                + "\n    'DVD read/writer',"
                + "\n    '500 gigabyte hard drive'"
                + "\n  ]"
                + "\n}");
            JObject o = JObject.Parse(json);
            Assert.IsNotNull(o);
            Assert.IsTrue(o.Count == 2);
            {
                JToken cpu = o["CPU"];
                Assert.IsTrue(cpu.Type == JTokenType.String);
                Assert.AreEqual(cpu.Type,JTokenType.String);
                JValue cpuValue = cpu as JValue;
                string cpuString = cpuValue.Value as string;
                Assert.AreEqual(cpuString,"Intel");
            }
            {
                JToken drives = o["Drives"];
                Assert.IsTrue(drives.Type == JTokenType.Array);
                Assert.AreEqual(drives.Type,JTokenType.Array);
                JArray drivesArray = drives as JArray;
                {
                    JToken drive0 = drives[0];
                    Assert.IsTrue(drive0.Type == JTokenType.String);
                    Assert.AreEqual(drive0.Type,JTokenType.String);
                    JValue drive0Value = drive0 as JValue;
                    string drive0String = drive0Value.Value as string;
                    Assert.AreEqual(drive0String,"DVD read/writer");
                }
                {
                    JToken drive1 = drives[1];
                    Assert.IsTrue(drive1.Type == JTokenType.String);
                    Assert.AreEqual(drive1.Type,JTokenType.String);
                    JValue drive1Value = drive1 as JValue;
                    string drive1String = drive1Value.Value as string;
                    Assert.AreEqual(drive1String,"500 gigabyte hard drive");
                }
            }
        }
        [TestMethod]
        public void JArrayParseTest() {
            string json = ("["
                + "\n  'Small',"
                + "\n  'Medium',"
                + "\n  'Large'"
                + "\n]");
            JArray a = JArray.Parse(json);
            Assert.IsNotNull(a);
            Assert.IsTrue(a.Count == 3);
            {
                JToken a0 = a[0];
                Assert.IsTrue(a0.Type == JTokenType.String);
                Assert.AreEqual(a0.Type,JTokenType.String);
                JValue a0Value = a0 as JValue;
                string a0String = a0Value.Value as string;
                Assert.AreEqual(a0String,"Small");
            }
            {
                JToken a1 = a[1];
                Assert.IsTrue(a1.Type == JTokenType.String);
                Assert.AreEqual(a1.Type,JTokenType.String);
                JValue a0Value = a1 as JValue;
                string a0String = a0Value.Value as string;
                Assert.AreEqual(a0String,"Medium");
            }
            {
                JToken a2 = a[2];
                Assert.IsTrue(a2.Type == JTokenType.String);
                Assert.AreEqual(a2.Type,JTokenType.String);
                JValue a0Value = a2 as JValue;
                string a0String = a0Value.Value as string;
                Assert.AreEqual(a0String,"Large");
            }
        }
        [TestMethod]
        public void JValueParseStringTest() {
            // Parse a JSON string.
            string json = "'MrsCritter'";
            // JValue.Parse doesn't exist.  We have to use JToken.Parse .
            JToken token = JToken.Parse(json);
            Assert.IsTrue(token.Type == JTokenType.String);
            Assert.AreEqual(token.Type,JTokenType.String);
            {
                JValue tokenValue = token as JValue;
                Assert.IsNotNull(tokenValue);
                Assert.IsTrue(tokenValue.Type == JTokenType.String);
                Assert.AreEqual(tokenValue.Type,JTokenType.String);
                // Either "as String" or "as string" works here.
                string tokenString1 = tokenValue.Value as string;
                Assert.AreEqual(tokenString1,"MrsCritter");
                // ToObject<string>() Alternative.
                string tokenString2 = tokenValue.ToObject<string>();
                Assert.AreEqual(tokenString2,"MrsCritter");
            }
            {
                // More simply.
                Assert.AreEqual(token.ToObject<string>(),"MrsCritter");
            }
        }
        [TestMethod]
        public void JValueParseIntegerTest() {
            // Parse a JSON number.
            string json = "123456";
            // JValue.Parse doesn't exist.  We have to use JToken.Parse .
            // A JSON number will be JTokenType.Integer or JTokenType.Float .
            JToken token = JToken.Parse(json);
            Assert.IsTrue(token.Type == JTokenType.Integer);
            Assert.AreEqual(token.Type,JTokenType.Integer);
            {
                JValue tokenValue = token as JValue;
                Assert.IsNotNull(tokenValue);
                Assert.IsTrue(tokenValue.Type == JTokenType.Integer);
                Assert.AreEqual(tokenValue.Type,JTokenType.Integer);
                int tokenInt = tokenValue.ToObject<int>();
                Assert.AreEqual(tokenInt,123456);
                // This doesn't work: "tokenValue.Value as int;"
            }
            {
                // More simply.
                Assert.AreEqual(token.ToObject<int>(),123456);
            }
        }
        [TestMethod]
        public void JTokenParseTest() {
            {
                string json = "{'a':1,'b':2,'c':3}";
                JToken token = JToken.Parse(json);
                Assert.IsNotNull(token);
                Assert.IsTrue(token.Type == JTokenType.Object);
                JObject o = token as JObject;
                Assert.IsNotNull(o);
                Assert.IsTrue(o.Count == 3);
            }
            {
                string json = "[1,2,3]";
                JToken token = JToken.Parse(json);
                Assert.IsNotNull(token);
                Assert.IsTrue(token.Type == JTokenType.Array);
                JArray a = token as JArray;
                Assert.IsNotNull(a);
                Assert.IsTrue(a.Count == 3);
            }
            {
                string json = "'MrsCritter'";
                JToken token = JToken.Parse(json);
                Assert.IsNotNull(token);
                Assert.IsTrue(token.Type == JTokenType.String);
                JValue v = token as JValue;
                Assert.IsNotNull(v);
                Assert.IsTrue(v.ToObject<string>() == "MrsCritter");
                Assert.IsTrue(token.ToObject<string>() == "MrsCritter");
            }
        }
        [TestMethod]
        public void EndpointJsonTest() {
            string timestamp = DateUtils.ISO8601DateString(DateTime.UtcNow);
            Endpoint endpoint1 = new Endpoint(
                "POST",
                "http://www.mrscritter.com?doYouLoveCrittercism=YES",
                timestamp,
                433,
                3213,
                2478,
                HttpStatusCode.OK,
                WebExceptionStatus.Success);
            // Testing EndpointConverter WriteJson
            string json1 = JsonConvert.SerializeObject(endpoint1);
            // NOTE: VS editor syntax colors embedded URL, but the C# syntax is correct.
            Assert.IsTrue(json1.StartsWith("[\"POST\",\"http://www.mrscritter.com?doYouLoveCrittercism=YES\","));
            Assert.IsTrue(json1.EndsWith(",433,2,3213,2478,200,5,0]"));
            string json2 = JsonConvert.SerializeObject(endpoint1,Formatting.None,new EndpointConverter());
            Debug.WriteLine("json1 == " + json1);
            Debug.WriteLine("json2 == " + json2);
            Assert.AreEqual(json1,json2);
            // Testing EndpointConverter ReadJson
            Endpoint endpoint2 = JsonConvert.DeserializeObject(json1,typeof(Endpoint)) as Endpoint;
            Assert.IsNotNull(endpoint2);
            string json3 = JsonConvert.SerializeObject(endpoint2);
            Debug.WriteLine("json1 == " + json1);
            Debug.WriteLine("json3 == " + json3);
            Assert.AreEqual(json1,json3);
        }
        [TestMethod]
        public void TransactionJsonTest() {
            string timestamp = DateUtils.ISO8601DateString(DateTime.UtcNow);
            Transaction transaction1 = new Transaction(
                "Buy Critter Feed",
                TransactionState.ENDED,
                3600000,
                10000,//-2147483648,
                new Dictionary<string,string>(),
                635847638748307732L,
                635847638922943221L,
                174635489L);
            // Testing TransactionConverter WriteJson
            string json1 = JsonConvert.SerializeObject(transaction1);
            Debug.WriteLine(json1);
            Assert.AreEqual(json1,"[\"Buy Critter Feed\",2,360.0,10000,{},\"2015-12-03T18:24:34.830Z\",\"2015-12-03T18:24:52.294Z\",1.74635489]");
            string json2 = JsonConvert.SerializeObject(transaction1,Formatting.None,new TransactionConverter());
            Debug.WriteLine("json1 == " + json1);
            Debug.WriteLine("json2 == " + json2);
            Assert.AreEqual(json1,json2);
            // Testing TransactionConverter ReadJson
            Transaction transaction2 = JsonConvert.DeserializeObject(json1,typeof(Transaction)) as Transaction;
            Assert.IsNotNull(transaction2);
            string json3 = JsonConvert.SerializeObject(transaction2);
            Debug.WriteLine("json3 == " + json3);
            Assert.AreEqual(json1,json3);
        }
        [TestMethod]
        public void NetworkBreadcrumbJsonTest() {
            // Network,         // 2 - network breadcrumb    ; [verb,url,...,statusCode,errorCode]
            string timestamp = DateUtils.ISO8601DateString(DateTime.UtcNow);
            Endpoint endpoint1 = new Endpoint(
                "POST",
                "http://www.mrscritter.com?doYouLoveCrittercism=YES",
                timestamp,
                433,
                3213,
                2478,
                HttpStatusCode.OK,
                WebExceptionStatus.Success);
            Breadcrumb breadcrumb1 = new Breadcrumb(timestamp,BreadcrumbType.Network,endpoint1);
            // Testing BreadcrumbConverter WriteJson
            string json1 = JsonConvert.SerializeObject(breadcrumb1);
            // NOTE: VS editor syntax colors embedded URL, but the C# syntax is correct.
            Assert.IsTrue(json1.IndexOf(",2,[\"POST\",\"http://www.mrscritter.com?doYouLoveCrittercism=YES\"")>=0);
            Assert.IsTrue(json1.IndexOf(",433,2,3213,2478,200,5,0]]")>=0);
            string json2 = JsonConvert.SerializeObject(breadcrumb1,Formatting.None,new BreadcrumbConverter());
            Debug.WriteLine("json1 == " + json1);
            Debug.WriteLine("json2 == " + json2);
            Assert.AreEqual(json1,json2);
            // Testing BreadcrumbConverter ReadJson
            Breadcrumb breadcrumb2 = JsonConvert.DeserializeObject(json1,typeof(Breadcrumb)) as Breadcrumb;
            Assert.IsNotNull(breadcrumb2);
            string json3 = JsonConvert.SerializeObject(breadcrumb2);
            Debug.WriteLine("json3 == " + json3);
            Assert.AreEqual(json1,json3);
        }
        [TestMethod]
        public void UserBreadcrumbJsonTest() {
            // Text,            // 1 - user breadcrumb       ; {text:,level:}
            string timestamp = DateUtils.ISO8601DateString(DateTime.UtcNow);
            Dictionary<string,Object> data1 = new Dictionary<string,Object>();
            data1["text"] = "Critter Bowl is Empty!";
            data1["level"] = (int)BreadcrumbTextType.Urgent;
            Breadcrumb breadcrumb1 = new Breadcrumb(timestamp,BreadcrumbType.Text,data1);
            // Testing BreadcrumbConverter WriteJson
            string json1 = JsonConvert.SerializeObject(breadcrumb1);
            // We don't assume Dictionary key-value pairs appear in any particular order.
            Assert.IsTrue(json1.IndexOf("\"text\":\"Critter Bowl is Empty!\"") >= 0);
            Assert.IsTrue(json1.IndexOf("\"level\":1") >= 0);
            Assert.IsTrue(json1.IndexOf(",1,{") >= 0);
            Assert.IsTrue(json1.IndexOf("}]") >= 0);
            string json2 = JsonConvert.SerializeObject(breadcrumb1,Formatting.None,new BreadcrumbConverter());
            Debug.WriteLine("json1 == " + json1);
            Debug.WriteLine("json2 == " + json2);
            Assert.AreEqual(json1,json2);
            // Testing BreadcrumbConverter ReadJson
            Breadcrumb breadcrumb2 = JsonConvert.DeserializeObject(json1,typeof(Breadcrumb)) as Breadcrumb;
            Assert.IsNotNull(breadcrumb2);
            string json3 = JsonConvert.SerializeObject(breadcrumb2);
            Debug.WriteLine("json3 == " + json3);
            Assert.AreEqual(json1,json3);
        }
    }
}
