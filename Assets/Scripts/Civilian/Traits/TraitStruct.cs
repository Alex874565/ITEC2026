using System;
using Unity.Netcode;

public struct TraitStruct : IEquatable<TraitStruct>, INetworkSerializable
{
    public Trait Trait;
    
    public TraitStruct(Trait trait)
    {
        this.Trait = trait;
    }
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Trait);
    }
    
    public bool Equals(TraitStruct other)
    {
        return this.Trait == other.Trait;
    }
}