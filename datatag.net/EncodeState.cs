namespace Datatag;

public struct EncodeState
{
    public enum NestingType
    {
        Object,
        Array
    }

    public List<NestingType> Nesting;

    public EncodeState()
    {
        Nesting = new();
    }
}