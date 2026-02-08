using System;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static float Wrap(float v, float min, float max)
    {
        // ensure min <= max by swapping if needed
        if (min > max)
        {
            float temp = min;
            min = max;
            max = temp;
        }

        // handle case where range is zero
        if (min == max) return min;

        // calculate offset from min using modulo for wrapping
        float range = max - min;
        float offset = (v - min) % range;

        // handle negative offsets (when v < min)
        if (offset < 0) offset += range;

        return min + offset;
    }

    public static Vector3 Wrap(Vector3 v, Vector3 min, Vector3 max)
    {
        v.x = Wrap(v.x, min.x, max.x);
        v.y = Wrap(v.y, min.y, max.y);
        v.z = Wrap(v.z, min.z, max.z);

        return v;
    }

    public static Vector3 WrapXZ(Vector3 v, Vector3 min, Vector3 max)
    {
        v.x = Wrap(v.x, min.x, max.x);
        v.z = Wrap(v.z, min.z, max.z);

        return v;
    }

    public static Vector3[] GetDirectionsInCircle(int num, float angle)
    {
        if (num <= 0) return Array.Empty<Vector3>();
        if (num == 1) return new Vector3[] { Vector3.forward };

        // create array of vector3
        Vector3[] result = new Vector3[num];
        int index = 0;

        // set forward direction if odd number
        if (num % 2 == 1)
	{
            result[index++] = Vector3.forward;
            num--;
        }

        // compute angle between rays (angle * 2 / num rays - 1)
        float angleOffset = (angle * 2) / (num);

        // add directions symmetrically around the circle
        for (int i = 1; i <= num / 2; i++)
        {
            result[index++] = Quaternion.AngleAxis(+angleOffset * i, Vector3.up) * Vector3.forward;
            result[index++] = Quaternion.AngleAxis(-angleOffset * i, Vector3.up) * Vector3.forward;
        }

        return result;
    }
}
