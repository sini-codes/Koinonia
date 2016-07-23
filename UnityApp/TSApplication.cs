using UnityEngine;

namespace Koinonia
{
    public static class TSApplication
    {
        public static string DataPath
        {
            get { return ThreadingUtils.GetOnMainThread(() => Application.dataPath); }
        }
    }
}