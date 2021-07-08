using System;
using System.Linq;
using Xunit;

namespace EmbeddedScripts.Shared.Tests
{
    public class ContainerTests
    {
        [Fact]
        public void RegisteringDependency_Succeed()
        {
            var x = 1;
            var container = new Container();
            container.Register(x, "x");
            Assert.Equal(1, container.Resolve<int>("x"));
        }

        [Fact]
        public void RegisteringCustomObject_Succeed()
        {
            var obj = new HelperObjects.HelperObject();
            var container = new Container();
            container.Register(obj, "obj");

            Assert.Equal(obj, container.Resolve<HelperObjects.HelperObject>("obj"));
        }

        [Fact]
        public void ResolveUnregisteredDependency_ThrowsException()
        {
            var container = new Container();
            Assert.Throws<ArgumentException>(() => container.Resolve<int>("x"));
        }

        [Fact]
        public void RevealAllAliases()
        {
            var container = new Container();

            var x = 1;
            var y = "2";

            container.Register(x, "x");
            container.Register(y, "y");

            var actualCount = container.VariableAliases.Count();

            Assert.Equal(2, actualCount);
        }

        [Fact]
        public void GetTypeOfVariable_ReturnsCorrectType()
        {
            var container = new Container();

            var x = 1;
            container.Register(x, "x");

            Assert.Equal(x.GetType(), container.GetTypeByAlias("x"));
        }
    }
}