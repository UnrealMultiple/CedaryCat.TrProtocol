using System.Runtime.InteropServices;
using TrProtocol.Interfaces;

namespace Terraria.DataStructures;

[StructLayout(LayoutKind.Explicit)]
public struct ProjectileKey : IPackedSerializable, IEquatable<ProjectileKey>
{
    [FieldOffset(0)]
    public readonly uint bits;

    [FieldOffset(0)]
    public readonly float floatBits;

    public readonly int Spawner => (int)(bits & 0xFF);

    public readonly int Index => (int)((bits >> 8) & 0x3FF);

    public readonly int Generation => (int)((bits >> 18) & 0x3FFF);

    public ProjectileKey(uint bits)
    {
        this = default;
        this.bits = bits;
    }

    public ProjectileKey(float bits)
    {
        this = default;
        floatBits = bits;
    }

    public ProjectileKey(int spawner, int index, int generation)
        : this(Pack(spawner, index, generation))
    {
    }

    public static uint Pack(int spawner, int index, int generation)
    {
        if ((uint)spawner > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(spawner));
        if ((uint)index > 1000)
            throw new ArgumentOutOfRangeException(nameof(index));

        return (uint)((spawner & 0xFF) | ((index & 0x3FF) << 8) | ((generation & 0x3FFF) << 18));
    }

    public readonly bool Equals(ProjectileKey other) => (int)this == (int)other;

    public readonly override bool Equals(object? obj) => obj is ProjectileKey other && Equals(other);

    public readonly override int GetHashCode() => bits.GetHashCode();

    public readonly override string ToString() => $"spawner:{Spawner}, index:{Index}, gen:{Generation}";

    public static bool operator ==(ProjectileKey left, ProjectileKey right) => left.Equals(right);

    public static bool operator !=(ProjectileKey left, ProjectileKey right) => !left.Equals(right);

    public static implicit operator uint(ProjectileKey key) => key.bits;

    public static implicit operator int(ProjectileKey key) => (int)key.bits;

    public static implicit operator float(ProjectileKey key) => key.floatBits;

    public static explicit operator ProjectileKey(uint value) => new(value);

    public static explicit operator ProjectileKey(int value) => new((uint)value);

    public static explicit operator ProjectileKey(float value) => new(value);
}
