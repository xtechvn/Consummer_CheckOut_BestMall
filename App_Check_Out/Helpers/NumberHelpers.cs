using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class NumberHelpers
    {
        public static double RoundUpToHundredsDouble(double amount)
        {
            return Math.Floor(amount / 500) * 500;

        }
    }
}
