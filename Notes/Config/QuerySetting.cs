using System;
using System.Collections.Generic;
using Notes.Enum;

namespace Notes.Config
{
    [Serializable]
    public class QuerySetting
    {
        public QuerySetting()
        {
            RegexPattren = false;
            QueryRange = QueryRange.NoFinished;
            AlwaysShowTransparent = false;
            Transparency = 1.0;
            QueryHistory = new List<String>();
        }

        public bool RegexPattren { get; set; }

        public QueryRange QueryRange { get; set; }

        public bool AlwaysShowTransparent { get; set; }

        public double Transparency { get; set; }
        
        public List<String> QueryHistory { get; set; }
    }
}