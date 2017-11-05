using MagicMirror.Services.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services
{
    public class TestResult
    {
        public string TestString { get; set; } = "TestStringValue";
        public int TestInt { get; set; } = 9999;
        public string DependencyInfo { get; set; } = "None";
    }

    public interface ITestDependency
    {
        string DependencyInfo { get; }
    }

    public class TestDependecyImplementation0 : ITestDependency
    {
        public string DependencyInfo => "Implemetation0";
    }
    public class TestDependecyImplementation1 : ITestDependency
    {
        public string DependencyInfo => "Implemetation1";
    }

    [CloudServiceInstance(CloudServiceInstanceType.SingletonStrict)]
    public class TestApiController : CloudService
    {
        private readonly ITestDependency _testDependency;
        public TestApiController(ITestDependency testDependency)
        {
            _testDependency = testDependency;
        }

        [CloudCall("TestApi/TestAsyncVoid")]
        public async Task<TestResult> TestAsyncVoid()
        {
            await Task.Delay(500);

            return new TestResult() { DependencyInfo = _testDependency.DependencyInfo };
        }

        [CloudCall("TestApi/TestVoid")]
        public TestResult TestVoid()
        {
            return new TestResult() { DependencyInfo = _testDependency.DependencyInfo };
        }
    }
}
