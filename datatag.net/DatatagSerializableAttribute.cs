namespace Datatag
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class DatatagSerializableAttribute : Attribute
    {

    }
}