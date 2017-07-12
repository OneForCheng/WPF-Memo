using System;

namespace Notes.Enum
{
    [Flags]
    public enum QueryRange
    {
        None = 0,
        NoFinished = 1,
        Finished = 2,
        Deleted = 4
    }
}