﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace com.csutil.testing {

    public class XunitTestRunner {

        public class Test {

            public MethodInfo methodToTest;
            public string name;
            public Task testTask;
            public Action StartTest;
            public object invokeResult;
            public ExceptionDispatchInfo reportedError;

            public Test(MethodInfo methodToTest) {
                this.methodToTest = methodToTest;
                this.name = methodToTest.ToStringV2();
            }

            public override string ToString() {
                var res = methodToTest.ToStringV2() + ": ";
                res = (invokeResult != null) ? res + invokeResult : res;
                res = (reportedError != null) ? res + reportedError.SourceException.Message : res;
                return res;
            }

            public Task RunTestOnMethod(object classInstance) {
                try {
                    invokeResult = methodToTest.Invoke(classInstance, null);
                    // Its an async test so the run task should wait for the test to finish
                    if (invokeResult is Task t) { return t; }
                    return Task.FromResult(invokeResult);
                }
                catch (Exception e) {
                    reportedError = ExceptionDispatchInfo.Capture(e);
                    return Task.FromException(e);
                }
            }

        }

        public static List<Test> CollectAllTests(IEnumerable<Type> classesToTest, Action<Test> onTestStarted) {
            var allTests = new List<Test>();
            foreach (var classToTest in classesToTest) {
                allTests.AddRange(GetIteratorOverAllTests(classToTest, onTestStarted));
            }
            return allTests;
        }

        public static IEnumerable<Test> GetIteratorOverAllTests(Type classToTest, Action<Test> onTestStarted) {
            return GetIteratorOverTests(classToTest, GetMethodsToTest(classToTest), onTestStarted);
        }

        public static IEnumerable<Test> GetIteratorOverTests(Type classToTest, IEnumerable<MethodInfo> methodsToTest, Action<Test> onTestStarted) {
            return methodsToTest.Map((methodToTest) => {
                var test = new Test(methodToTest);
                test.StartTest = () => {
                    ResetStaticInstances();
                    onTestStarted(test);
                    test.testTask = test.RunTestOnMethod(CreateInstance(classToTest));
                };
                return test;
            });
        }

        private static void ResetStaticInstances() {
            AssertV2.throwExeptionIfAssertionFails = false;
            AssertV3.throwExeptionIfAssertionFails = false;
            DisposeAllInjectors();
            EventBus.instance = new EventBus();
            IoC.inject = new injection.Injector();
            UnitySetup.ResetAllStaticObjects();
        }

        private static void DisposeAllInjectors() {
            IEnumerable<object> instances = IoC.inject.GetAllInjectorsMap(null).SelectMany(x => x.Value);
            foreach (var d in instances.OfType<IDisposable>()) { try { d.Dispose(); } catch (Exception e) { Log.e(e); } }
            foreach (var d in instances.OfType<UnityEngine.Object>()) { d.Destroy(); }
        }

        public static IEnumerable<MethodInfo> GetMethodsToTest(Type classToTest) {
            var f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var testMethods = classToTest.GetMethods(f).Filter(m => { return m.HasAttribute<Fact>(true); });
            return testMethods;
        }

        private static object CreateInstance(Type classToTest) {
            var c1 = classToTest.GetConstructors().FirstOrDefault(c => c.GetParameters().IsNullOrEmpty());
            if (c1 != null) { return c1.Invoke(null); }
            var c2 = classToTest.GetConstructors().FirstOrDefault(c => c.GetParameters().Single().ParameterType.IsCastableTo<ITestOutputHelper>());
            if (c2 != null) { return c2.Invoke(new object[1] { new ITestOutputHelper() }); }
            throw new ArgumentException("No construction found with no or a single ITestOutputHelper parameter");
        }

    }

}
