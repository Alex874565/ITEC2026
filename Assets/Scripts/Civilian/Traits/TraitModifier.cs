using System;
using Unity.Netcode;

public struct TraitModifier : INetworkSerializable, IEquatable<TraitModifier>
{
    public Trait Trait;
    public int Positive;
    public int Negative;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Trait);
        serializer.SerializeValue(ref Positive);
        serializer.SerializeValue(ref Negative);
    }

    public bool Equals(TraitModifier other)
    {
        return Trait == other.Trait &&
               Positive == other.Positive &&
               Negative == other.Negative;
    }

    public override bool Equals(object obj)
    {
        return obj is TraitModifier other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Trait, Positive, Negative);
    }
}