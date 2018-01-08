using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircularReferenceFinder.Tests.Models
{
    public class Node
    {
        public Node()
        {
            Nodes = new List<Node>();
        }

        public IList<Node> Nodes { get; private set; }
    }
}
