using System;
using Xunit;

namespace Drgn.Rpc.Tests
{
    public class ServiceServerTest
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal("Hello", ServiceServer.SayHello());
        }
    }
}
