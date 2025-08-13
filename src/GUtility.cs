using System.Globalization;
using System.Numerics;

namespace MukiaEngine;

public static class GUtility
{
    public static bool IsBitSet(dynamic b, int pos)
    {
        return ((b >> pos) & 1) != 0;
    }
}