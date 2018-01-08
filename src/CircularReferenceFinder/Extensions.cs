using System.Runtime.CompilerServices;

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
