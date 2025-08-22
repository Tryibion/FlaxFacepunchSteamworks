using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FlaxEngine.Networking;
using Steamworks;
using Steamworks.Data;

namespace FacepunchSteamworks;

public class SteamNetworkConnectionManager : ConnectionManager
{
    public FacepunchNetworkDriver Driver;

    public event Action<NetworkEventType, ulong, byte[]> NetworkEvent;

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        byte[] bytes = new byte[size];

        unsafe
        {
            fixed (void* p = bytes)
            {
                Buffer.MemoryCopy((void*)data, p, size, size);
            }
        }

        NetworkEvent?.Invoke(NetworkEventType.Message, Driver.TargetSteamId, bytes);
    }
}
