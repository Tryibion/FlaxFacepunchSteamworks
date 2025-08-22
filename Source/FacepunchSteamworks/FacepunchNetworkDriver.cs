using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;
using FlaxEngine.Networking;
using Steamworks;
using Steamworks.Data;
using Debug = FlaxEngine.Debug;

namespace FacepunchSteamworks;


/// <summary>
/// Facepunch Network Driver.
/// </summary>
public class FacepunchNetworkDriver : FlaxEngine.Object, INetworkDriver
{
    public class Client
    {
        public SteamId Id;
        public Connection SocketConnection;
    }

    private NetworkConfig _config;
    private SteamNetworkSocketManager _socketManager;
    private SteamNetworkConnectionManager _connectionManager;
    public Dictionary<ulong, Client> ConnectedClients;
    private NetworkPeer _networkPeer;
    Queue<NetworkEvent> _networkEventQueue = new();

    public ulong UserSteamId;
    public ulong TargetSteamId;

    public bool IsServer = false;
    public bool IsClient = false;

    public string DriverName()
    {
        return "FacepunchNetworkDriver";
    }

    public bool Initialize(NetworkPeer peer, NetworkConfig config)
    {
        _networkPeer = peer;
        _config = config;
        UserSteamId = SteamClient.SteamId;

        SteamNetworkingUtils.SendBufferSize = config.MessageSize;
        ConnectedClients = new Dictionary<ulong, Client>();

        SteamNetworkingUtils.InitRelayNetworkAccess();

        Scripting.Update += OnUpdate;

        return false;
    }

    private void OnUpdate()
    {
        try
        {
            _socketManager?.Receive();
            _connectionManager?.Receive();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error receiving data on socket/connection: {e}");
        }
    }

    public void Dispose()
    {
        Scripting.Update -= OnUpdate;

        _socketManager?.Close();
        _connectionManager?.Close();
        _connectionManager = null;
        _socketManager = null;
    }

    public bool Listen()
    {
        TargetSteamId = SteamClient.SteamId;
        Debug.Log($"[Steam Host] Initializing host (Steam id: {TargetSteamId})");

        _socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamNetworkSocketManager>();
        if (_socketManager == null)
        {
            Debug.LogError("[Steam Host] Failed to initialize socket manager");
            return false;
        }

        Debug.Log("[Steam Host] Created Steam socket manager.");

        _socketManager.Driver = this;
        _socketManager.NetworkEvent += OnNetworkEvent;

        IsServer = true;
        return true;
    }

    public bool Connect()
    {
        Debug.Log($"[Steam Client] Initializing client connection (host Steam id: {TargetSteamId})");

        _connectionManager = SteamNetworkingSockets.ConnectRelay<SteamNetworkConnectionManager>(TargetSteamId);
        if (_connectionManager == null)
        {
            Debug.LogError("[Steam Client] Failed to initialize connection manager");
            return false;
        }

        _connectionManager.Driver = this;
        _connectionManager.NetworkEvent += OnNetworkEvent;

        Debug.Log("[Steam Client] Created Steam connection manager.");

        IsClient = true;
        return true;
    }

    public void Disconnect()
    {
        if (_socketManager != null)
        {
            _socketManager.Close();
            _socketManager.NetworkEvent -= OnNetworkEvent;
        }

        if (_connectionManager != null)
        {
            _connectionManager.Connection.Close();
            _connectionManager.NetworkEvent -= OnNetworkEvent;
        }

        IsServer = false;
        IsClient = false;
    }

    public void Disconnect(NetworkConnection connection)
    {
        if (ConnectedClients.TryGetValue(connection.ConnectionId, out Client user))
        {
            // Flush messages
            user.SocketConnection.Flush();
            user.SocketConnection.Close();
            ConnectedClients.Remove(connection.ConnectionId);
        }
        else
        {
            foreach (var c in _socketManager.Connected)
            {
                if (c.Id == connection.ConnectionId)
                {
                    c.Flush();
                    c.Close();
                    ConnectedClients.Remove(c.Id);
                }
            }
        }
    }

    private void OnNetworkEvent(NetworkEventType eventType, ulong id, byte[] data)
    {
        if (eventType == NetworkEventType.Message && data != null)
        {
            NetworkEvent ev = new();
            ev.EventType = eventType;
            ev.Sender.ConnectionId = (uint)id;

            ev.Message = _networkPeer.CreateMessage();
            ev.Message.Length = (uint)data.Length;
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    ev.Message.Buffer = ptr;
                }
            }

            _networkEventQueue.Enqueue(ev);
        }
    }

    public bool PopEvent(out NetworkEvent networkEvent)
    {
        if (_networkEventQueue.Count == 0)
        {
            networkEvent = new();
            return false;
        }

        networkEvent = _networkEventQueue.Dequeue();
        return true;
    }

    public void SendMessage(NetworkChannelType channelType, NetworkMessage message)
    {
        if (IsServer)
            return;

        SendUsingConnection(_connectionManager.Connection, channelType, message);
    }

    public void SendMessage(NetworkChannelType channelType, NetworkMessage message, NetworkConnection target)
    {
        if (!IsServer)
            return;

        foreach (var c in _socketManager.Connected)
        {
            if (c.Id == target.ConnectionId)
            {
                SendUsingConnection(c, channelType, message);
                break;
            }
        }
    }

    public void SendMessage(NetworkChannelType channelType, NetworkMessage message, NetworkConnection[] targets)
    {
        if (!IsServer)
            return;

        foreach (var c in _socketManager.Connected)
        {
            if (targets.Any(t => c.Id == t.ConnectionId))
            {
                SendUsingConnection(c, channelType, message);
            }
        }
    }

    void SendUsingConnection(Connection connection, NetworkChannelType channelType, NetworkMessage message)
    {
        unsafe
        {
            var ptr = (IntPtr)message.Buffer;
            var length = (int)message.Length;
            connection.SendMessage(ptr, length, ConvertToSendType(channelType));
        }
    }

    private SendType ConvertToSendType(NetworkChannelType type)
    {
        var sendType = SendType.Unreliable;
        switch (type)
        {
            case NetworkChannelType.None:
            case NetworkChannelType.Unreliable:
            case NetworkChannelType.UnreliableOrdered:
                break;
            case NetworkChannelType.Reliable:
            case NetworkChannelType.ReliableOrdered:
                sendType = SendType.Reliable;
                break;
            default:
                break;
        }

        return sendType;
    }

    public NetworkDriverStats GetStats()
    {
        // TODO: need proper implementation
        return new NetworkDriverStats()
        {
            RTT = -1,
            TotalDataReceived = 0,
            TotalDataSent = 0,
        };
    }

    public NetworkDriverStats GetStats(NetworkConnection target)
    {
        // TODO: need proper implementation
        return new NetworkDriverStats()
        {
            RTT = -1,
            TotalDataReceived = 0,
            TotalDataSent = 0,
        };
    }
}
