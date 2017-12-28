using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircularReferenceFinder.Tests.Models
{
    public struct StructNode
    {
        public StructNode(StructNode[] nodes)
        {
            Nodes = nodes;
        }

        public StructNode[] Nodes { get; set; }
    }
}
