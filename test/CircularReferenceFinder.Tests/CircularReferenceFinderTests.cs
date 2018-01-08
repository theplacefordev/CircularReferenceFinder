using System;
using System.Collections.Generic;
using CircularReferenceFinder.Tests.Models;
using NUnit.Framework;

namespace CircularReferenceFinder.Tests
{
    [TestFixture]
    public class CircularReferenceFinderTests
    {
        [Test]
        public void FindCycles_NullRoot_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => CircularReferenceFinder.FindCycles(null));
        }

        #region Negative tests

        [Test]
        public void FindCycles_SingleAnonObject_ReturnsEmptyList()
        {
            var cycles = CircularReferenceFinder.FindCycles(new { Test = 1 });
            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_SingleCustomObject_ReturnsEmptyList()
        {
            var cycles = CircularReferenceFinder.FindCycles(new Node());
            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_ListOfTheSameInternedString_ReturnsEmptyList()
        {
            var cycles = CircularReferenceFinder.FindCycles(new List<string> { String.Empty, String.Empty, String.Empty });
            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_SimpleDictWithNullValue_ReturnsEmptyList()
        {
            var obj = new object();
            var cycles = CircularReferenceFinder.FindCycles(new Dictionary<string, object> { { "1", obj }, { "2", obj }, { "3", null } });
            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_AnonObjWithPrimitiveProps_ReturnsEmptyList()
        {
            var cycles = CircularReferenceFinder.FindCycles(new
                {
                    a = 1,
                    b = true,
                    c = 'c',
                    d = DateTime.Now,
                    e = "string",
                    f = 3d,
                    g = Guid.NewGuid(),
                    h = 4L,
                    i = (decimal) 5,
                    j = TimeSpan.Zero,
                    k = new object(),
                    l = ConsoleColor.Black
                });
            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_AcyclicGraphUsingRefTypes_ReturnsEmptyList()
        {
            var a = new Node
            {
                Nodes = { new Node(), new Node() }
            };
            var cycles = CircularReferenceFinder.FindCycles(a);
            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_AcyclicGraphUsingValueTypes_ReturnsEmptyList()
        {
            var a = new StructNode();
            var b = new StructNode(new[] { a }); // 'a' struct is copied by value at this moment
            a.Nodes = new[] { b };

            var cycles = CircularReferenceFinder.FindCycles(a);

            Assert.IsEmpty(cycles);
        }

        [Test]
        public void FindCycles_SmallAcyclicGraphSpecialHierOfClasses_ReturnsEmptyList()
        {
            var a = new TestRoot
            {
                Name = "a",
                SubItem = new TestSub
                {
                    Child = new TestDeep()
                }
            };

            var cycles = CircularReferenceFinder.FindCycles(a);

            Assert.IsEmpty(cycles);
        }

        #endregion

        #region Positive tests
        [Test]
        public void FindCycles_SmallCyclicGraphOfNodes_ReturnsNonEmptyList()
        {
            var a = new Node();
            var b = new Node { Nodes = { a } };
            a.Nodes.Add(b);

            var cycles = CircularReferenceFinder.FindCycles(a);

            Assert.IsNotEmpty(cycles);
        }


        [Test]
        public void FindCycles_MediumCyclicGraphOfNodes_ReturnsNonEmptyList()
        {
            var a = new Node();
            var b = new Node { Nodes = { new Node { Nodes = { a } } } };
            a.Nodes.Add(new Node { Nodes = { b }});

            var cycles = CircularReferenceFinder.FindCycles(a);

            Assert.IsNotEmpty(cycles);
        }

        [Test]
        public void FindCycles_LargeCyclicGraphUsingListOfNodes_ReturnsNonEmptyList()
        {
            var roots = new List<Node>();
            for (int i = 0; i < 10; i++)
            {
                var node = new Node();
                for (int j = 0; j < 10; j++)
                {
                    node.Nodes.Add(new Node { Nodes = { node }});
                }
                roots.Add(node);
                node.Nodes.Add(roots[i]);
            }

            var cycles = CircularReferenceFinder.FindCycles(roots);

            Assert.IsNotEmpty(cycles);
        }

        [Test]
        public void FindCycles_SmallCyclicGraphOfDictAndNodes_ReturnsNonEmptyList()
        {
            var a = new Node();
            var b = new Node { Nodes = { a } };
            a.Nodes.Add(b);
            var dict = new Dictionary<string,Node> { { "a", a }, { "b", b } };

            var cycles = CircularReferenceFinder.FindCycles(dict);

            Assert.IsNotEmpty(cycles);
        }

        [Test]
        public void FindCycles_SmallCyclicGraphUsingSpecialHierOfClasses_ReturnsNonEmptyList()
        {
            var a = new TestRoot
            {
                Name = "test",
                SubItem = new TestSub
                {
                    Child = new TestDeep()
                }
            };
            a.SubItem.Parent = a;
            a.SubItem.Child.Root = a;

            var cycles = CircularReferenceFinder.FindCycles(a);

            Assert.IsNotEmpty(cycles);
        }

        #endregion
    }
}
