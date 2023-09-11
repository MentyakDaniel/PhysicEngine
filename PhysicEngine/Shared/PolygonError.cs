namespace PhysicEngine.Shared
{
    public enum PolygonError
    {
        NoError,
        InvalidAmountOfVertices,
        NotSimple,
        NotCounterClockWise,
        NotConvex,
        AreaTooSmall,
        SideTooSmall
    }
}
