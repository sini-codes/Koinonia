using System.Collections.Generic;

namespace Koinonia
{
    public static class Ko
    {
        public static bool IsNull<T>(T obj)
        {
            return EqualityComparer<T>.Default.Equals(obj, default(T));
        }
    }
}