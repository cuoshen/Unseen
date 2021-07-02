using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomExtensions
{
    public static double NextDouble(
        this System.Random random,
        double minValue,
        double maxValue)
    {
        return random.NextDouble() * (maxValue - minValue) + minValue;
    }
}
