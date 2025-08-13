using System.Diagnostics.CodeAnalysis;

namespace MukiaEngine;

/// <summary>
/// Stores the red, green and blue components of a color.
/// </summary>
/// <param name="red"><inheritdoc cref="R" path="/summary"/></param>
/// <param name="green"><inheritdoc cref="G" path="/summary"/></param>
/// <param name="blue"><inheritdoc cref="B" path="/summary"/></param>
public struct Color(float red, float green, float blue)
{
    private float _R = red;
    private float _G = green;
    private float _B = blue;

    /// <summary>
    /// The red component, between 0 to 1.
    /// </summary>
    public float R
    {
        readonly get => _R;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(value));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1, nameof(value));
            _R = value;
        }
    }
    /// <summary>
    /// The green component, between 0 to 1.
    /// </summary>
    public float G
    {
        readonly get => _G;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(value));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1, nameof(value));
            _G = value;
        }
    }
    /// <summary>
    /// The blue component, between 0 to 1.
    /// </summary>
    public float B
    {
        readonly get => _B;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(value));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1, nameof(value));
            _B = value;
        }
    }

    public override readonly int GetHashCode()
    {
        return (int)ToHex();
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Color color)
        {
            throw new InvalidCastException(nameof(obj));
        }

        return _R == color._R && _G == color._G && _B == color._B;
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }

    #region Convert

    public override readonly string ToString()
    {
        var (r, g, b) = ToRGB();
        return $"({r}, {g}, {b})";
    }

    /// <summary>
    /// Converts to RGB.
    /// </summary>
    /// <returns>In order of RGB, ranging between 0 to 255</returns>
    public readonly (byte, byte, byte) ToRGB()
    {
        return ((byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
    }

    /// <summary>
    /// Converts to hue, satruation and value.
    /// </summary>
    /// <returns>In order in HSV, hue in range of 0 to 360 and SV from 0 to 1.</returns>
    public readonly (int, float, float) ToHSV()
    {
        float cMax = float.Max(float.Max(R, G), B),
        cMin = float.Min(float.Min(R, G), B);

        float delta = cMax - cMin;

        float value = cMax;

        float saturation;
        if (cMax == 0)
        {
            saturation = 0;
        }
        else
        {
            saturation = delta / cMax;
        }

        int hue;
        if (delta == 0)
        {
            hue = 0;
        }
        else if (cMax == R)
        {
            hue = (int)(60 * ((G - B) / delta % 6));
        }
        else if (cMax == G)
        {
            hue = (int)(60 * ((B - R) / delta + 2));
        }
        else
        {
            hue = (int)(60 * ((R - G) / delta + 4));
        }

        return (hue, saturation, value);
    }

    /// <summary>
    /// Converts into a hex.
    /// </summary>
    /// <returns>A hex in this order: <c>0x00RRGGBB</c></returns>
    public readonly uint ToHex()
    {
        var (r, g, b) = ToRGB();
        uint hex = (uint)((r << 16) | (g << 8) | b);
        return hex;
    }

    /// <summary>
    /// Converts into a hexcode.
    /// </summary>
    /// <returns>A hexcode: <c>#RRGGBB</c></returns>
    public readonly string ToHexcode()
    {
        uint hex = ToHex();
        return '#' + Convert.ToString(hex, 16).ToUpper();
    }

    #endregion

    #region Custom Constructors

    /// <summary>
    /// RGB to Color.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    /// <returns>A Color</returns>
    public static Color FromRGB(byte red, byte green, byte blue)
    {
        return new(
            (float)red / 255,
            (float)green / 255,
            (float)blue / 255
        );
    }

    /// <summary>
    /// HSV to Color.
    /// </summary>
    /// <param name="hue">The hue between 0 to 360.</param>
    /// <param name="saturation">The saturation between 0 to 1.</param>
    /// <param name="value">The value between 0 to 1.</param>
    /// <returns>A Color</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Color FromHSV(int hue, float saturation, float value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(hue, 0, nameof(hue));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(hue, 360, nameof(hue));

        ArgumentOutOfRangeException.ThrowIfLessThan(saturation, 0, nameof(saturation));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(saturation, 1, nameof(saturation));

        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(value));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1, nameof(value));

        float c = value * saturation,
        x = c * (1 - float.Abs((hue / 60.0f % 2) - 1)),
        m = value - c;

        int r = hue / 60;

        float rQ, gQ, bQ;
        switch (r)
        {
            case 0:
                rQ = c;
                gQ = x;
                bQ = 0;
                break;
            case 1:
                rQ = x;
                gQ = c;
                bQ = 0;
                break;
            case 2:
                rQ = 0;
                gQ = c;
                bQ = x;
                break;
            case 3:
                rQ = 0;
                gQ = x;
                bQ = c;
                break;
            case 4:
                rQ = x;
                gQ = 0;
                bQ = c;
                break;
            case 5:
                rQ = c;
                gQ = 0;
                bQ = x;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(hue));
        }

        return new(rQ + m, gQ + m, bQ + m);
    }

    /// <summary>
    /// Hex to Color.
    /// </summary>
    /// <param name="hex">Hex in the form of <c>0x00RRGGBB</c></param>
    /// <returns></returns>
    public static Color FromHex(uint hex)
    {
        byte rHex = (byte)((hex >> 16) & 0xFF),
        gHex = (byte)((hex >> 8) & 0xFF),
        bHex = (byte)(hex & 0xFF);

        return FromRGB(rHex, gHex, bHex);
    }

    /// <summary>
    /// Hexcode to Color.
    /// </summary>
    /// <param name="hexcode">Hexcode in the form of <c>#RRGGBB</c></param>
    /// <returns>A color</returns>
    public static Color FromHexcode(string hexcode)
    {
        string sub = hexcode[0] == '#' ? hexcode[1..] : hexcode;

        uint hex = Convert.ToUInt32(sub, 16);

        return FromHex(hex);
    }

    #endregion
}