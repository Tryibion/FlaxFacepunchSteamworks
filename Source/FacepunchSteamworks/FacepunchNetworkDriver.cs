using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FlaxEngine;
using FlaxEngine.Networking;
using Steamworks;
using Steamworks.Data;
using Debug = FlaxEngine.Debug;

namespace FacepunchSteamworks;

public class SteamNetworkSocketManager : SocketManager
{
    public FacepunchNetworkDriver Driver;

    public event Action<NetworkEventType, ulong, byte[]> NetworkEvent;
    
    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        if (Driver == null)
            return;

        var clients = Driver.ConnectedClients;
        if (!clients.ContainsKey(connection.Id))
        {
            clients.Add(connection.Id, new FacepunchNetworkDriver.Client()
            {
                Id = info.Identity.SteamId,
                SocketConnection = connection,
            });
            
            Debug.Write(LogType.Info, $"Connected with Steam user {info.Identity.SteamId}");
            NetworkEvent?.Invoke(NetworkEventType.Connected, connection.Id, default);
        }
    }

    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        Debug.Write(LogType.Info, $"Accepting connection from Steam user {info.Identity.SteamId}");
        connection.Accept();
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        if (Driver == null)
            return;
        Driver.ConnectedClients.Remove(connection.Id);
        Debug.Write(LogType.Info, $"Disconnected Steam user {info.Identity.SteamId}");
        NetworkEvent?.Invoke(NetworkEventType.Disconnected, connection.Id, default);
    }

    public override void OnConnectionChanged(Connection connection, ConnectionInfo info)
    {
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        byte[] bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);
        NetworkEvent?.Invoke(NetworkEventType.Message, connection.Id, bytes);
    }
}

public class SteamNetworkConnectionManager : ConnectionManager
{
    public event Action<NetworkEventType, ulong, byte[]> NetworkEvent;
    
    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        byte[] bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);
        NetworkEvent?.Invoke(NetworkEventType.Message, NetworkManager.ServerClientId, bytes);
    }
}

/// <summary>
/// Facepunch Network Driver.
/// </summary>
public class FacepunchNetworkDriver : FlaxEngine.Object, INetworkDriver
{
    public ulong UserSteamId;
    public ulong TargetSteamId;
    public Dictionary<ulong, Client> ConnectedClients;
    
    public class Client
    {
        public SteamId Id;
        public Connection SocketConnection;
    }
    
    private NetworkPeer _networkHost;
    private SteamNetworkSocketManager _socketManager;
    private NetworkConfig _config;
    private SteamNetworkConnectionManager _connectionManager;
    
    public string DriverName()
    {
        return "FacepunchNetworkDriver";
    }

    public bool Initialize(NetworkPeer host, NetworkConfig config)
    {
        _networkHost = host;
        _config = config;

        SteamNetworkingUtils.SendBufferSize = config.MessageSize;
        ConnectedClients = new Dictionary<ulong, Client>();

        SteamNetworkingUtils.InitRelayNetworkAccess();

        UserSteamId = SteamClient.SteamId;

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
        _socketManager?.Close();
        _connectionManager?.Close();
        _connectionManager = null;
        _socketManager = null;
        
        Scripting.Update -= OnUpdate;
    }

    public bool Listen()
    {
        _socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamNetworkSocketManager>();
        if (_socketManager == null)
        {
            Debug.Write(LogType.Error, "Failed to initialize socket manager");
            return false;
        }
        _socketManager.Driver = this;
        _socketManager.NetworkEvent += OnNetworkEvent;

        if (!NetworkManager.IsServer)
        {
            _connectionManager = SteamNetworkingSockets.ConnectRelay<SteamNetworkConnectionManager>(UserSteamId);
            _connectionManager.NetworkEvent += OnNetworkEvent;
        }
        
        Debug.Write(LogType.Info, "Created steam socket manager.");
        return true;
    }

    private NetworkEventType _networkEventType = NetworkEventType.Undefined;
    private ulong _eventId;
    private byte[]? _eventData;

    private void OnNetworkEvent(NetworkEventType eventType, ulong id, byte[] data)
    {
        _networkEventType = eventType;
        _eventId = id;
        if (data.Length > 0)
        {
            _eventData = data;
        }
    }

    public bool Connect()
    {
        //_connectionManager = SteamNetworkingSockets.ConnectRelay<SteamNetworkConnectionManager>(UserSteamId);
        _connectionManager = SteamNetworkingSockets.ConnectRelay<SteamNetworkConnectionManager>(TargetSteamId);
        _connectionManager.NetworkEvent += OnNetworkEvent;
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

    public unsafe bool PopEvent(NetworkEvent* eventPtr)
    {
        eventPtr->Sender.ConnectionId = (uint)_eventId;
        eventPtr->EventType = _networkEventType;
        if (_networkEventType == NetworkEventType.Message)
        {
            if (_eventData != null)
            {
                eventPtr->Message = _networkHost.CreateMessage();
                eventPtr->Message.Length = (uint)_eventData.Length;
                var i = (byte*)Unsafe.AsPointer(ref _eventData);
                eventPtr->Message.Buffer = i;
            }
        }

        // Reset data
        _networkEventType = NetworkEventType.Undefined;
        _eventId = 0;
        _eventData = null;

        return true;
    }

    public void SendMessage(NetworkChannelType channelType, NetworkMessage message)
    {
        if (NetworkManager.IsServer)
            return;
        
        unsafe
        {
            var ptr = (IntPtr)message.Buffer;
            var length = (int)message.Length;
            _connectionManager.Connection.SendMessage(ptr, length, ConvertToSendType(channelType));
        }
    }
    
    public void SendMessage(NetworkChannelType channelType, NetworkMessage message, NetworkConnection target)
    {
        if (!NetworkManager.IsServer)
            return;
        /*
        byte[] bytes = new byte[message.Length];
        message.ReadBytes(bytes, bytes.Length);
        _connectionManager.Connection.SendMessage(bytes, ConvertSendType(channelType));
        */

        foreach (var c in _socketManager.Connected)
        {
            if (c.Id == target.ConnectionId)
            {
                unsafe
                {
                    var ptr = (IntPtr)message.Buffer;
                    var length = (int)message.Length;
                    c.SendMessage(ptr, length, ConvertToSendType(channelType));
                }
                break;
            }
        }
    }

    public void SendMessage(NetworkChannelType channelType, NetworkMessage message, NetworkConnection[] targets)
    {
        if (!NetworkManager.IsServer)
            return;

        foreach (var c in _socketManager.Connected)
        {
            if (targets.Any(t => c.Id == t.ConnectionId))
            {
                unsafe
                {
                    var ptr = (IntPtr)message.Buffer;
                    var length = (int)message.Length;
                    c.SendMessage(ptr, length, ConvertToSendType(channelType));
                }
            }
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
        throw new NotImplementedException();
    }

    public NetworkDriverStats GetStats(NetworkConnection target)
    {
        throw new NotImplementedException();
    }
}
