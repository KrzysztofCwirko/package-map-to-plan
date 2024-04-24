using UnityEngine;

namespace MapToPlan.Scripts.Core
{
    public static class Extensions
    {
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            if (obj == null)
                return;

            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        public static Vector3 With(this Vector3 source, float x, float y, float z)
        {
            return source + new Vector3(x, y, z);
        }
    }
}