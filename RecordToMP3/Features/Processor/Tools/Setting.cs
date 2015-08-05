using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class Setting<T>
    {
        public Setting(T defaultValue, T minimum, T maximum, T increment, string description)
        {
            Default = defaultValue;
            Min = minimum;
            Max = maximum;
            Increment = increment;
            Value = defaultValue;
            Description = description;
        }

        public T Min { get; set; }

        public T Max { get; set; }

        public T Default { get; set; }

        public T Increment { get; set; }

        public T Value { get; set; }

        public string Description { get; set; }
    }
}
