using System;
using System.Runtime.InteropServices;
using FlaxEngine;
using FlaxEngine.Networking;
using Steamworks;
using Steamworks.Data;

namespace FacepunchSteamworks;

public class SteamNetworkSocketManager : SocketManager
{
    public FacepunchNetworkDriver Driver;

    public event Action<NetworkEventType, ulong, byte[]> NetworkEvent;

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);

        if (Driver == null)
            return;

        if (!Driver.ConnectedClients.ContainsKey(connection.Id))
        {
            Driver.ConnectedClients.Add(connection.Id, new FacepunchNetworkDriver.Client
            {
                Id = info.Identity.SteamId,
                SocketConnection = connection,
            });

            Debug.Log($"Connected with Steam user {info.Identity.SteamId}");
            NetworkEvent?.Invoke(NetworkEventType.Connected, connection.Id, default);
        }
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);

        if (Driver == null)
            return;
        Driver.ConnectedClients.Remove(connection.Id);
        Debug.Log($"Disconnected Steam user {info.Identity.SteamId}");
        NetworkEvent?.Invoke(NetworkEventType.Disconnected, connection.Id, default);
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        byte[] bytes = new byte[size];

        unsafe
        {
            fixed (void* p = bytes)
            {
                Buffer.MemoryCopy((void*)data, p, size, size);
            }
        }

        NetworkEvent?.Invoke(NetworkEventType.Message, connection.Id, bytes);
    }
}
