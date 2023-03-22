using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MathUtils : MonoBehaviour
{
    public static float DistanceFrom(Vector3 src, Vector3 dest)
    {
        return Mathf.Sqrt(Mathf.Pow(dest.x - src.x, 2) +
                          Mathf.Pow(dest.y - src.y, 2) +
                          Mathf.Pow(dest.z - src.z, 2));
    }

    public static long NanoTime()
    {
        long nano = 10000L * Stopwatch.GetTimestamp();
        nano /= TimeSpan.TicksPerMillisecond;
        nano *= 100L;
        return nano;
    }

    public static float Interpolate(float a0, float a1, float w)
    {
        return (a1 - a0) * w + a0;
    }
}
