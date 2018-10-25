using System;
using System.Collections.Generic;

namespace Framework.Assertions
{
    public class DateTimeEqualityComparer : IEqualityComparer<DateTime>
    {
        private readonly int _range;

        public DateTimeEqualityComparer(int range = 100_000)
        {
            _range = range;
        }

        public bool Equals(DateTime x, DateTime y) => Math.Abs(x.Ticks - y.Ticks) <= _range;

        public int GetHashCode(DateTime obj) => obj.GetHashCode();
    }
}
