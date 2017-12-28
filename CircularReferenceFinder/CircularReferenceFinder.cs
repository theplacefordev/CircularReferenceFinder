using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CircularReferenceFinder
{
    public static class CircularReferenceFinder 
    {
        private static readonly HashSet<Type> _IgnoredTypes = new HashSet<Type> { typeof(String), typeof(DateTime), typeof(TimeSpan), typeof(DateTimeOffset), typeof(Decimal), typeof(Guid) };

        public static IList<CircularReference> FindCycles(object root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            var cycles = new List<CircularReference>();
            var visited = new ConditionalWeakTable<object, ObjectState>();
            depthFirstSearch(root, new List<object>(), visited, cycles);
            return cycles;
        }

        private static void depthFirstSearch(object node, List<object> parents, ConditionalWeakTable<object, ObjectState> visited, List<CircularReference> cycles)
        {
            if (node == null || isTypeShouldBeIgnored(node.GetType())) return;
            var state = visited.GetStateOrDefault(node, VisitState.NotVisited);
            if (state == VisitState.Visited) return;
            if (state == VisitState.Visiting)
            {
                // Do not report nodes not included in the cycle.
                cycles.Add(new CircularReference(parents.Concat(new[] { node }).SkipWhile(parent => !referenceEquals(parent, node)).ToList()));
            }
            else
            {
                var stateObject = visited.GetOrCreateValue(node);
                stateObject.State = VisitState.Visiting;
                parents.Add(node);
                foreach (var child in getMembers(node))
                {
                    depthFirstSearch(child, parents, visited, cycles);
                }
                parents.RemoveAt(parents.Count - 1);
                stateObject.State = VisitState.Visited;
            }
        }

        private static IEnumerable getMembers(object obj)
        {
            var res = new List<object>();
            if (obj != null)
            {
                // if it's collection (array, list, dictionary etc.) - just return it members instead of properties
                var items = obj as IEnumerable;
                if (items != null)
                {
                    return items;
                }
                // scan public properties
                var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var propertyInfo in props)
                {
                    if (!isTypeShouldBeIgnored(propertyInfo.PropertyType) && propertyInfo.GetIndexParameters().Length == 0) // ignore indexer properties
                    {
                        res.Add(propertyInfo.GetValue(obj));
                    }
                }
            }
            return res;
        }

        private static bool isTypeShouldBeIgnored(Type type)
        {
            return type.IsPrimitive || type.IsEnum || _IgnoredTypes.Contains(type);
        }

        private static bool referenceEquals(object objA, object objB)
        {
            return (objA != null && objB != null) && ReferenceEquals(objA, objB);
        }


    }
}