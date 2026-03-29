using Unity.Netcode;

public struct ModifierUpgrade : INetworkSerializable, System.IEquatable<ModifierUpgrade>
{
    public Trait Trait;
    public ModifierType Type;
    public int Value;

    // This method is required by INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Enums must be cast to their underlying type (usually int)
        if (serializer.IsReader)
        {
            int traitInt = 0;
            int typeInt = 0;
            serializer.SerializeValue(ref traitInt);
            serializer.SerializeValue(ref typeInt);
            Trait = (Trait)traitInt;
            Type = (ModifierType)typeInt;
        }
        else
        {
            int traitInt = (int)Trait;
            int typeInt = (int)Type;
            serializer.SerializeValue(ref traitInt);
            serializer.SerializeValue(ref typeInt);
        }

        serializer.SerializeValue(ref Value);
    }

    // Recommended: Implement IEquatable so NetworkVariable can detect if the value actually changed
    public bool Equals(ModifierUpgrade other)
    {
        return Trait == other.Trait && Type == other.Type && Value == other.Value;
    }
}