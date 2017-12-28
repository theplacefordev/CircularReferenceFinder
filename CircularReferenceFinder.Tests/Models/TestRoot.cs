using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircularReferenceFinder.Tests.Models
{
    public class TestRoot
    {
        public string Name { get; set; }

        public TestSub SubItem { get; set; }
    }

    public class TestSub
    {
        public bool DummyProp { get; set; }

        public TestDeep Child { get; set; }

        public TestRoot Parent { get; set; }
    }

    public class TestDeep
    {
        public TestRoot Root { get; set; }

        public ConsoleColor Color { get; set; }
    }
}
