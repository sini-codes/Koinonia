using UnityEditor;

namespace Koinonia
{
    public static class TSAssetDatabase
    {
        public static void Refresh()
        {
            ThreadingUtils.WaitOnMainThread(AssetDatabase.Refresh);
        }
    }
}