using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.json;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.integrationTests.datastructures {

    /// <summary>
    /// These tests make sure that libraries like the System.Collections.Immutable lib are included in the build correctly, 
    /// so the test is ment to be executed during runtime of the production application via the XunitTestRunner
    /// </summary>
    public class LinkXmlFileTests {

        public LinkXmlFileTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        // [Fact]
        // public void RegressionTestForFullyQualifiedNameOfImmutableLibrary() {
        //     // Ensures that when the DLL is modified in the future the link.xml file in the folder still has the correct assembly name
        //     AssertAssemblyName(typeof(ImmutableList), "System.Collections.Immutable");
        //     AssertAssemblyName(typeof(TypeConverter), "System"); // TODO FIXME for WebGL
        // }
        // private static void AssertAssemblyName(Type t, string n) {
        //     var path = new FileInfo(t.Assembly.Location);
        //     Assert.Equal(n, "" + path.GetNameWithoutExtension());
        // }

        [Fact]
        public void TestJsonDeserializeWithImmutableList1() {
            var r = TypedJsonHelper.NewTypedJsonReader();
            List<string> b1 = new List<string>() { "a", "b", "c" };
            var json = JsonWriter.GetWriter().Write(b1);
            Log.MethodEntered("TestJsonDeserializeWithImmutableList1", json);
            var b2 = r.Read<ImmutableList<string>>(json);
            Assert.Equal(b1.Count, b2.Count);
            Assert.Equal(b1, b2);
        }

        [Fact]
        public void TestJsonDeserializeWithImmutableList2() {
            var w = TypedJsonHelper.NewTypedJsonWriter();
            var r = TypedJsonHelper.NewTypedJsonReader();
            ServerOutbox b1 = new ServerOutbox();
            b1.serverActions = ImmutableList<ServerAction>.Empty.Add(new TestAction() { myString1 = "abc" });
            var json = w.Write(b1);
            Log.MethodEntered("TestJsonDeserializeWithImmutableList2", json);
            var b2 = r.Read<ServerOutbox>(json);
            Assert.IsType<TestAction>(b2.serverActions.First());
            Assert.Equal((b1.serverActions.First() as TestAction).myString1, (b2.serverActions.First() as TestAction).myString1);
        }

    }

    internal class TestAction : ServerAction {

        public string myString1;
        public Task<ServerActionResult> SendToServer() { return Task.FromResult(ServerActionResult.SUCCESS); }
        public Task RollbackLocalChanges(ServerActionResult reasonForRollback) { return Task.FromResult(true); }

    }
}