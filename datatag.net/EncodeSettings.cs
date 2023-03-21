namespace Datatag;

public struct EncodeSettings
{
    public int Indent;

    public bool ExpandArrays;

    public bool ExpandObjects;

    public static readonly EncodeSettings Compact = new EncodeSettings { Indent = 0, ExpandArrays = false, ExpandObjects = false };

    public static readonly EncodeSettings Pretty = new EncodeSettings { Indent = 2, ExpandArrays = false, ExpandObjects = true };
}
