using TrProtocol.Attributes;

namespace Terraria.DataStructures;

public struct TrackedProjectileReference : IEquatable<TrackedProjectileReference>
{
    public ProjectileKey Key { get; set; }

    [SerializeAs(typeof(short))]
    public int ProjectileType { get; set; }

    public void Clear() => this = default;

    public readonly bool Equals(TrackedProjectileReference other) =>
        Key == other.Key && ProjectileType == other.ProjectileType;

    public readonly override bool Equals(object? obj) =>
        obj is TrackedProjectileReference other && Equals(other);

    public readonly override int GetHashCode() => Key.GetHashCode();

    public static bool operator ==(TrackedProjectileReference left, TrackedProjectileReference right) =>
        left.Equals(right);

    public static bool operator !=(TrackedProjectileReference left, TrackedProjectileReference right) =>
        !left.Equals(right);
}
