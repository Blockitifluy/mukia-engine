global using EVector3 = MukiaEngine.Vector3;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;

namespace MukiaEngine;

// Vector3

public struct Vector3(float x = 0, float y = 0, float z = 0)
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
    public float Z { get; set; } = z;

    #region Constants
    public static Vector3 Zero => zero;
    public static Vector3 One => one;
    public static Vector3 Right => right;
    public static Vector3 Up => up;
    public static Vector3 Forward => foward;

    private static readonly Vector3 zero = new(0, 0, 0);
    private static readonly Vector3 one = new(1, 1, 1);
    private static readonly Vector3 right = new(1, 0, 0);
    private static readonly Vector3 up = new(0, 1, 0);
    private static readonly Vector3 foward = new(0, 0, 1);
    #endregion

    #region Operators
    public static Vector3 operator /(Vector3 left, Vector3 right)
    {
        return new(
            left.X / right.X,
            left.Y / right.Y,
            left.Z / right.Z
        );
    }

    public static Vector3 operator /(Vector3 left, float right)
    {
        return new(
            left.X / right,
            left.Y / right,
            left.Z / right
        );
    }

    public static Vector3 operator *(Vector3 left, Vector3 right)
    {
        return new(
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z
        );
    }

    public static Vector3 operator *(Vector3 left, float right)
    {
        return new(
            left.X * right,
            left.Y * right,
            left.Z * right
        );
    }

    public static Vector3 operator *(float left, Vector3 right)
    {
        return new(
            left * right.X,
            left * right.Y,
            left * right.Z
        );
    }

    public static Vector3 operator +(Vector3 left, Vector3 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    public static Vector3 operator -(Vector3 left, Vector3 right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z
        );
    }

    public static Vector3 operator -(Vector3 vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z);
    }

    public static bool operator ==(Vector3 left, Vector3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3 left, Vector3 right)
    {
        return !(left == right);
    }

    public static explicit operator GLVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    public static explicit operator Vector3(GLVector3 vector) => new(vector.X, vector.Y, vector.Z);

    public static explicit operator Vector3Int(Vector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
    #endregion

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Vector3 other)
            throw new InvalidCastException($"Object has to be Vector3");
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static Vector3 RotateEuler(Vector3 point, Vector3 euler)
    {
        Vector3 dir = euler.Unit;
        float angle = float.DegreesToRadians(euler.Magnitude);

        return Rotate(point, dir, angle);
    }

    public static Vector3 Rotate(Vector3 point, Vector3 axis, float angle)
    {
        if (axis == Zero || angle == 0)
        {
            return point;
        }

        float radX = axis.X * angle,
        radY = axis.Y * angle,
        radZ = axis.Z * angle;

        float cosX = float.Cos(radX), sinX = float.Sin(radX),
        cosY = float.Cos(radY), sinY = float.Sin(radY),
        cosZ = float.Cos(radZ), sinZ = float.Sin(radZ);

        // Apply X rotation
        Vector3 v1 = new(
            point.X,
            point.Y * cosX - point.Z * sinX,
            point.Y * sinX + point.Z * cosX
        );

        // Apply Y rotation
        Vector3 v2 = new(
            v1.X * cosY + v1.Z * sinY,
            v1.Y,
            -v1.X * sinY + v1.Z * cosY
        );

        // Apply Z rotation
        Vector3 result = new(
            v2.X * cosZ - v2.Y * sinZ,
            v2.X * sinZ + v2.Y * cosZ,
            v2.Z
        );

        return result;
    }

    public static Vector3 Cross(Vector3 left, Vector3 right)
    {
        return new(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X)
        );
    }

    public static float Dot(Vector3 left, Vector3 right)
    {
        return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);
    }

    public float this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => 0,
            };
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
            }
        }
    }

    [JsonIgnore]
    public readonly float Magnitude
    {
        get { return float.Sqrt(X * X + Y * Y + Z * Z); }
    }

    [JsonIgnore]
    public readonly Vector3 Unit => this == Zero ? Zero : this / Magnitude;
}

public struct Vector3Int(int x = 0, int y = 0, int z = 0)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Z { get; set; } = z;

    #region Constants
    public static Vector3Int Zero => zero;
    public static Vector3Int One => one;
    public static Vector3Int Right => right;
    public static Vector3Int Up => up;
    public static Vector3Int Forward => foward;

    private static readonly Vector3Int zero = new(0, 0, 0);
    private static readonly Vector3Int one = new(1, 1, 1);
    private static readonly Vector3Int right = new(1, 0, 0);
    private static readonly Vector3Int up = new(0, 1, 0);
    private static readonly Vector3Int foward = new(0, 0, 1);
    #endregion

    #region Operators
    public static Vector3Int operator /(Vector3Int left, Vector3Int right)
    {
        return new(
            left.X / right.X,
            left.Y / right.Y,
            left.Z / right.Z
        );
    }

    public static Vector3Int operator /(Vector3Int left, int right)
    {
        return new(
            left.X / right,
            left.Y / right,
            left.Z / right
        );
    }

    public static Vector3Int operator *(Vector3Int left, Vector3Int right)
    {
        return new(
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z
        );
    }

    public static Vector3Int operator *(Vector3Int left, int right)
    {
        return new(
            left.X * right,
            left.Y * right,
            left.Z * right
        );
    }

    public static Vector3Int operator *(int left, Vector3Int right)
    {
        return new(
            left * right.X,
            left * right.Y,
            left * right.Z
        );
    }

    public static Vector3Int operator +(Vector3Int left, Vector3Int right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    public static Vector3Int operator -(Vector3Int left, Vector3Int right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z
        );
    }

    public static Vector3Int operator -(Vector3Int vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z);
    }

    public static bool operator ==(Vector3Int left, Vector3Int right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3Int left, Vector3Int right)
    {
        return !(left == right);
    }

    public static explicit operator Vector3i(Vector3Int vector) => new(vector.X, vector.Y, vector.Z);

    public static explicit operator Vector3Int(Vector3i vector) => new(vector.X, vector.Y, vector.Z);

    public static explicit operator Vector3(Vector3Int vector) => new(vector.X, vector.Y, vector.Z);
    #endregion

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Vector3Int other)
            throw new InvalidCastException($"Object has to be Vector3");
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override readonly string ToString()
    {
        return $"I({X}, {Y}, {Z})";
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static Vector3Int Cross(Vector3Int left, Vector3Int right)
    {
        return new(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X)
        );
    }

    public int this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => 0,
            };
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
            }
        }
    }

    [JsonIgnore]
    public readonly float Magnitude
    {
        get { return float.Sqrt(X * X + Y * Y + Z * Z); }
    }

    [JsonIgnore]
    public readonly Vector3 Unit => this == Zero ? Vector3.Zero : (Vector3)this / Magnitude;
}

// Vector2

public struct Vector2(float x = 0, float y = 0)
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;

    #region Constants
    public static Vector2 Zero => zero;
    public static Vector2 One => one;
    public static Vector2 Right => right;
    public static Vector2 Up => up;

    private static readonly Vector2 zero = new(0, 0);
    private static readonly Vector2 one = new(1, 1);
    private static readonly Vector2 right = new(1, 0);
    private static readonly Vector2 up = new(0, 1);
    #endregion

    #region Operators
    public static Vector2 operator /(Vector2 left, Vector2 right)
    {
        return new(
            left.X / right.X,
            left.Y / right.Y
        );
    }

    public static Vector2 operator /(Vector2 left, float right)
    {
        return new(
            left.X / right,
            left.Y / right
        );
    }

    public static Vector2 operator *(Vector2 left, Vector2 right)
    {
        return new(
            left.X * right.X,
            left.Y * right.Y
        );
    }

    public static Vector2 operator *(Vector2 left, float right)
    {
        return new(
            left.X * right,
            left.Y * right
        );
    }

    public static Vector2 operator *(float left, Vector2 right)
    {
        return new(
            left * right.X,
            left * right.Y
        );
    }

    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y
        );
    }

    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y
        );
    }

    public static Vector2 operator -(Vector2 vector)
    {
        return new(-vector.X, -vector.Y);
    }

    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !(left == right);
    }
    #endregion

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Vector2 other)
            throw new InvalidCastException($"Object has to be Vector2");
        return X == other.X && Y == other.Y;
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public float this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 1, nameof(index));

            return index switch
            {
                0 => X,
                1 => Y,
                _ => 0,
            };
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 1, nameof(index));

            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
            }
        }
    }

    public static explicit operator Vector2Int(Vector2 vector) => new((int)vector.X, (int)vector.Y);

    [JsonIgnore]
    public readonly float Magnitude
    {
        get { return float.Sqrt(X * X + Y * Y); }
    }

    [JsonIgnore]
    public readonly Vector2 Unit => this == Zero ? Zero : this / Magnitude;
}

public struct Vector2Int(int x = 0, int y = 0)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;

    #region Constants
    public static Vector2Int Zero => zero;
    public static Vector2Int One => one;
    public static Vector2Int Right => right;
    public static Vector2Int Up => up;

    private static readonly Vector2Int zero = new(0, 0);
    private static readonly Vector2Int one = new(1, 1);
    private static readonly Vector2Int right = new(1, 0);
    private static readonly Vector2Int up = new(0, 1);
    #endregion

    #region Operators
    public static Vector2Int operator /(Vector2Int left, Vector2Int right)
    {
        return new(
            left.X / right.X,
            left.Y / right.Y
        );
    }

    public static Vector2Int operator /(Vector2Int left, int right)
    {
        return new(
            left.X / right,
            left.Y / right
        );
    }

    public static Vector2Int operator *(Vector2Int left, Vector2Int right)
    {
        return new(
            left.X * right.X,
            left.Y * right.Y
        );
    }

    public static Vector2Int operator *(Vector2Int left, int right)
    {
        return new(
            left.X * right,
            left.Y * right
        );
    }

    public static Vector2Int operator *(int left, Vector2Int right)
    {
        return new(
            left * right.X,
            left * right.Y
        );
    }

    public static Vector2Int operator +(Vector2Int left, Vector2Int right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y
        );
    }

    public static Vector2Int operator -(Vector2Int left, Vector2Int right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y
        );
    }

    public static Vector2Int operator -(Vector2Int vector)
    {
        return new(-vector.X, -vector.Y);
    }

    public static bool operator ==(Vector2Int left, Vector2Int right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2Int left, Vector2Int right)
    {
        return !(left == right);
    }
    #endregion

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Vector2 other)
            throw new InvalidCastException($"Object has to be Vector2");
        return X == other.X && Y == other.Y;
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static explicit operator Vector2(Vector2Int vector) => new(vector.X, vector.Y);

    public int this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 1, nameof(index));

            return index switch
            {
                0 => X,
                1 => Y,
                _ => 0,
            };
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 1, nameof(index));

            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
            }
        }
    }

    [JsonIgnore]
    public readonly float Magnitude
    {
        get { return float.Sqrt(X * X + Y * Y); }
    }

    [JsonIgnore]
    public readonly Vector2 Unit => this == Zero ? Vector2.Zero : (Vector2)this / Magnitude;
}