using System.Collections.Generic;

namespace CircularReferenceFinder
{
    public class CircularReference
    {
        public CircularReference(IEnumerable<object> vertices)
        {
            Vertices = vertices;
        }

        public IEnumerable<object> Vertices { get; }
    }
}