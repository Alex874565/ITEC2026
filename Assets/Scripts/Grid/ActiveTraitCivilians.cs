using System;
using System.Collections.Generic;
using Unity.Netcode;

[Serializable]
public struct ActiveTraitCivilians : INetworkSerializable
{
    public List<TraitCivilianList> TraitLists;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int count = TraitLists != null ? TraitLists.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            TraitLists = new List<TraitCivilianList>(count);

            for (int i = 0; i < count; i++)
            {
                TraitCivilianList entry = default;
                entry.NetworkSerialize(serializer);
                TraitLists.Add(entry);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                TraitCivilianList entry = TraitLists[i];
                entry.NetworkSerialize(serializer);
            }
        }
    }
}