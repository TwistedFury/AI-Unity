using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static float Wrap(float v, float min, float max)
    {
        return (v > max) ? Wrap(min + (v - max), min, max) : (v < min) ? Wrap(max + (v - min), min, max) : v;
    }

    public static Vector3 Wrap(Vector3 v, Vector3 min, Vector3 max)
    {
        v.x = Wrap(v.x, min.x, max.x);
        v.y = Wrap(v.y, min.y, max.y);
        v.z = Wrap(v.z, min.z, max.z);

        return v;
    }
}
