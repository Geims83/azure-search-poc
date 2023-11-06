using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzSearch.Api
{
    internal class Models
    {
        public class PullInput
        {
            public List<ValueInput> values { get; set; }
        }

        public class ValueInput
        {
            public string recordId { get; set; }
            public DataInput data { get; set; }
        }

        public class DataInput
        {
            public string metadata_storage_name { get; set; }

        }

        public class PullOutput
        {
            public List<ValueOutput> values { get; set; }
        }

        public class ValueOutput
        {
            public string recordId { get; set; }
            public DataOutput data { get; set; }
            public List<string> errors { get; set; }
            public List<string> warnings { get; set; }

        }

        public class DataOutput
        {
            public string db_id { get; set; }
            public string titolo { get; set; }
            public string argomento { get; set; }
            public DateTimeOffset data_creazione { get; set; }
            public int anno { get; set; }
            public string guid { get; set; }
            public string thumbnail_link { get; set; }
        }
    }
}
