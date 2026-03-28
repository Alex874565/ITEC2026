using System;
using System.Collections.Generic;
using Unity.Netcode;

[Serializable]
public struct ActiveTraitCivilians : INetworkSerializable
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
                NetworkObjectReference civilian = default;
                serializer.SerializeValue(ref civilian);
                Civilians.Add(civilian);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                NetworkObjectReference civilian = Civilians[i];
                serializer.SerializeValue(ref civilian);
            }
        }
    }
}