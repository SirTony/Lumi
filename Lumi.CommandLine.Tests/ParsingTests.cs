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
            var defaultConstruction = CommandLineParser.ParseArguments<EmptyApplication>( Array.Empty<string>() );
            var providedValue = CommandLineParser.ParseArguments<EmptyApplication>( Array.Empty<string>(), 10 );

            Assert.AreEqual( 5, defaultConstruction.TestVariable );
            Assert.AreEqual( 10, providedValue.TestVariable );
        }

        [TestMethod]
        public void MemberArgumentsTest()
        {
            var parsed = CommandLineParser.ParseArguments<Person>( new[] { "John", "-a", "35", "--surname", "Smith" } );

            Assert.AreEqual( "John", parsed.FirstName );
            Assert.AreEqual( "Smith", parsed.LastName );
            Assert.AreEqual( 35, parsed.Age );
        }
    }
}
