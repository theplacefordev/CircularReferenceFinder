namespace CircularReferenceFinder
{
    public class ObjectState
    {
        public ObjectState()
        {
        }

        public ObjectState(VisitState state)
        {
            State = state;
        }

        public VisitState State { get; set; }
    }

}