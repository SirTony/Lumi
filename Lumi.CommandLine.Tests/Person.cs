using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumi.CommandLine.Tests
{
    internal sealed class Person
    {
        [Positional( 0 ), Required]
        public string FirstName { get; private set; }

        [Named( 's', "surname" )]
        public string LastName { get; private set; }

        [Named( 'a', "age" )]
        public int Age { get; private set; }
    }
}
