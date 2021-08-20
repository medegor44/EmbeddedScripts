using System;
using System.Collections.Generic;
using EmbeddedScripts.CSharp.Shared.CodeGeneration;
using EmbeddedScripts.Shared;
using Xunit;

namespace EmbeddedScripts.CSharp.Shared.Tests
{
    public class ResolvingCodeGeneratorTests
    {
        [Fact]
        public void GenerateVariablesDeclaration_GeneratesCorrectly()
        {
            var container = new Container();

            container.Register(new int(), "x");
            container.Register("abc", "y");

            var actual = new ResolvingCodeGenerator().GenerateVariablesDeclaration(container);
            var expected =
                "System.Int32 x;" +
                Environment.NewLine +
                "System.String y;" +
                Environment.NewLine;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GenerateVariablesInitialization_GeneratesCorrectly()
        {
            var container = new Container();

            container.Register(new int(), "x");
            container.Register("abc", "y");

            var actual = new ResolvingCodeGenerator().GenerateVariablesInitialization(container, "w");
            var expected =
                "x = w.Resolve<System.Int32>(\"x\");" +
                Environment.NewLine +
                "y = w.Resolve<System.String>(\"y\");" +
                Environment.NewLine;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GenerateGenericVariableDeclaration_GeneratesCorrectly()
        {
            var container = new Container();
            container.Register(new List<int>(), "a");

            var actual = new ResolvingCodeGenerator().GenerateVariablesDeclaration(container);
            var expected = "System.Collections.Generic.List<System.Int32> a;" + Environment.NewLine;
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void GenerateNestedGenericVariableDeclaration_GeneratesCorrectly()
        {
            var container = new Container();
            container.Register(new List<Dictionary<List<int>, string>>(), "a");

            var actual = new ResolvingCodeGenerator().GenerateVariablesDeclaration(container);
            var expected = 
                "System.Collections.Generic.List<" +
                    "System.Collections.Generic.Dictionary<" + 
                        "System.Collections.Generic.List<System.Int32>," + 
                        "System.String>" +
                "> a;" + Environment.NewLine;
            
            Assert.Equal(expected, actual);
        }
    }
}