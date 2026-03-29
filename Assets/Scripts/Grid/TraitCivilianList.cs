using System;
using System.Collections.Generic;
using Unity.Netcode;

[Serializable]
public struct TraitCivilianList : INetworkSerializable
{
    public Trait Trait;
    public List<NetworkObjectReference> Civilians;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Trait);

        int count = Civilians != null ? Civilians.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            Civilians = new List<NetworkObjectReference>(count);
            for (int i = 0; i < count; i++)
            {
                NetworkObjectReference reference = default;
                serializer.SerializeValue(ref reference);
                Civilians.Add(reference);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                var reference = Civilians[i];
                serializer.SerializeValue(ref reference);
            }
        }
    }
}