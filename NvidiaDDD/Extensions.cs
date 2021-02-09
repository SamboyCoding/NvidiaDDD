using System.Collections.Generic;
using System.Linq;
using NvidiaDriverThing.Models;

namespace NvidiaDriverThing
{
    public static class Extensions
    {
        public static int GetOptionId(this IEnumerable<MenuItem> list, string optionText) => list.First(i => i.menutext == optionText).id;
    }
}