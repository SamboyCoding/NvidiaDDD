using System;
using System.Collections.Generic;
using System.Linq;
using NvidiaDriverThing.Models;

namespace NvidiaDriverThing
{
    public static class Extensions
    {
        public static int GetOptionId(this IEnumerable<MenuItem> list, string optionText) => list.FirstOrDefault(i => string.Equals(i.menutext, optionText, StringComparison.OrdinalIgnoreCase))?.id ?? -1;
    }
}