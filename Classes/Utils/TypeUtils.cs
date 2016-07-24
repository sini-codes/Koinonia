using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Koinonia
{
    public static class TypeUtils
    {


        public static List<Type> FindImplementations(this Type parent)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => parent.IsAssignableFrom(t) && t != parent)
                .ToList();
        }

    }
}