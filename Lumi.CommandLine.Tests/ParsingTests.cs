using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lumi.CommandLine.Tests
{
    [TestClass]
    public class ParsingTests
    {
        [TestMethod]
        public void ConstructorInvokationTest()
        {
            var ( _, defaultConstruction ) =
                CommandLineParser.ParseArguments<EmptyApplication>( Array.Empty<string>() );
            var ( _, providedValue ) = CommandLineParser.ParseArguments<EmptyApplication>( Array.Empty<string>(), 10 );

            Assert.AreEqual( 5, defaultConstruction.TestVariable );
            Assert.AreEqual( 10, providedValue.TestVariable );
        }

        [TestMethod]
        public void MemberArgumentsTest()
        {
            var ( _, parsed ) =
                CommandLineParser.ParseArguments<Person>( new[] { "John", "-a", "35", "--surname", "Smith" } );

            Assert.AreEqual( "John", parsed.FirstName );
            Assert.AreEqual( "Smith", parsed.LastName );
            Assert.AreEqual( 35, parsed.Age );
        }

        [TestMethod]
        public void OptionalArgumentTest()
        {
            var ( _, parsed1 ) = CommandLineParser.ParseArguments<TestApplication1>( new[] { "-r", "Test" } );
            var ( _, parsed2 ) = CommandLineParser.ParseArguments<TestApplication1>(
                new[] { "-r", "Test", "-o", "50" }
            );

            Assert.AreEqual( "Test", parsed1.RequiredValue );
            Assert.AreEqual( 5, parsed1.OptionalValue );

            Assert.AreEqual( "Test", parsed2.RequiredValue );
            Assert.AreEqual( 50, parsed2.OptionalValue );
        }

        [TestMethod]
        public void MissingRequiredArgumentTest()
        {
            Assert.ThrowsException<CommandLineException>(
                () => CommandLineParser.ParseArguments<TestApplication1>( Array.Empty<string>() )
            );
        }

        [TestMethod]
        public void CommandTest()
        {
            var (code, _) = CommandLineParser.ParseArguments<TestApplication2>( new[] { "with-code", "10" } );
            Assert.AreEqual( 10, code );

            var (_, parsed) =
                CommandLineParser.ParseArguments<TestApplication2>( new[] { "with-required", "-r", "Test" } );
            Assert.AreEqual( "Test", parsed.RequiredParameter );

            ( code, parsed ) = CommandLineParser.ParseArguments<TestApplication2>( Array.Empty<string>() );

            Assert.AreEqual( 100, code );
            Assert.AreEqual( "Hello, World", parsed.DefaultParameter );

            Assert.ThrowsException<CommandLineException>(
                () => CommandLineParser.ParseArguments<TestApplication2>( new[] { "with-required" } )
            );
        }
    }
}
