using System.Numerics;

namespace MukiaEngine;

// TODO - Add trignometry

public static class MathT
{
    public const double Pi = 245850922d / 78256779d,
    Tau = 2 * Pi;

    public const double RadToDeg = 180d / Pi;
    public const double DegToRad = 1d / RadToDeg;

    public static bool IsBitSet<T>(T num, int pos) where T : INumber<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
    {
        return ((num >> pos) & T.One) != T.Zero;
    }

    public static T Min<T>(params T[] values) where T : INumber<T>
    {
        T min = T.Zero;

        foreach (T v in values)
        {
            if (min > v)
            {
                min = v;
            }
        }

        return min;
    }

    public static T Max<T>(params T[] values) where T : INumber<T>
    {
        T max = T.Zero;

        foreach (T v in values)
        {
            if (max < v)
            {
                max = v;
            }
        }

        return max;
    }

    public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
    {
        if (min >= max)
        {
            throw new Exception("Min greater than max");
        }

        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    public static T Lerp<T>(T a, T b, T t) where T : IFloatingPoint<T>
    {
        return a + (b - a) * t;
    }
}