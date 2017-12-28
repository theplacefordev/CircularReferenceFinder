using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CircularReferenceFinder
{
    public static class Extensions
    {
        public static VisitState GetStateOrDefault(this ConditionalWeakTable<object, ObjectState> dictionary, object key, VisitState defaultValue) 
        {
            ObjectState value;
            if (dictionary.TryGetValue(key, out value))
                return value.State;
            return defaultValue;
        }

    }
}
