using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TestPhoneApp
{
    public enum TestStatus
    {
        NotRun,
        Running, 
        Success,
        Fail
    }

    public class TestInfo : INotifyPropertyChanged
    {
        private TestStatus testStatus;
        public event PropertyChangedEventHandler PropertyChanged;
        public string TestName { get; set; }
        public Exception TestException { get; set; }
        public TestStatus Status 
        { 
            get 
            {
                return testStatus;
            }
            set 
            {
               testStatus = value;
               this.NotifyPropertyChanged("TestSuccess");
               this.NotifyPropertyChanged("TestStatusBrush");
               this.NotifyPropertyChanged("TestStatusLabel");
               this.NotifyPropertyChanged("TestExceptionType");
               this.NotifyPropertyChanged("TestExecutionTimeString");
            }
        }
        public MethodInfo TestMethod { get; set; }
        public MethodInfo TestCleanUpMethod { get; set; }
        public TimeSpan ExcecutionTime { get; set; }

        private void NotifyPropertyChanged(string p)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        public Brush TestStatusBrush
        {
            get
            {
                switch (this.testStatus)
                {
                    case TestStatus.NotRun:
                        return new SolidColorBrush(Colors.LightGray);
                    case TestStatus.Running:
                        return new SolidColorBrush(Colors.Yellow);
                    case TestStatus.Success:
                        return new SolidColorBrush(Colors.Green);

                    case TestStatus.Fail:
                        return new SolidColorBrush(Colors.Red);
                }

                return new SolidColorBrush(Colors.LightGray);
            }
        }

        public string TestStatusLabel
        {
            get
            {
                return Enum.GetName(typeof(TestStatus), this.testStatus);
            }
        }

        public string TestExceptionType
        {
            get {
                if (this.TestException != null)
                {
                    return this.TestException.GetType().FullName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string TestExecutionTimeString
        {
            get
            {
                if (this.ExcecutionTime != null)
                {
                    return this.ExcecutionTime.ToString("mm':'ss'.'ffff");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string methodName = string.Empty;

        public TestInfo()
        {
        }

        public TestInfo(MethodInfo testMethod, MethodInfo cleanupMethod)
        {
            this.TestName = testMethod.Name;
            this.TestMethod = testMethod;
            this.TestCleanUpMethod = cleanupMethod;
            this.Status = TestStatus.NotRun;
        }

        public bool Run()
        {
            var instance = Activator.CreateInstance(TestMethod.DeclaringType);
            DateTime startTime = DateTime.UtcNow;
            try
            {
                this.TestMethod.Invoke(instance, null);
                this.ExcecutionTime = DateTime.UtcNow.Subtract(startTime);
                return true;
            }
            catch (Exception ex)
            {
                this.TestException = ex.GetBaseException();
                if (this.TestException != null)
                {
                    this.TestException = ex;
                }
                this.ExcecutionTime = DateTime.UtcNow.Subtract(startTime);
                return false;
            }
            finally
            {
                if (this.TestCleanUpMethod != null)
                {
                    this.TestCleanUpMethod.Invoke(instance, null);
                }
            }
        }

        
    }
}
