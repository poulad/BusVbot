using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Framework
{
    public class OrderedFactAttribute : FactAttribute
    {
        public int LineNumber { get; }

        public OrderedFactAttribute(
            [CallerLineNumber] int line = default
        )
        {
            if (line < 1)
                throw new ArgumentOutOfRangeException(nameof(line));

            LineNumber = line;
        }
    }
}
