namespace Datatag
{
    using System.Collections.Generic;

    public class EncodeState
    {
        public enum NestingType
        {
            Object,
            Array
        }

        public List<NestingType> Nesting = new List<NestingType>();
    }
}