using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // ushort : packetId
    // action : 할 행동
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
_handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
;
_onRecv.Add((ushort)PacketID.Test, MakePacket<Test>);
_handler.Add((ushort)PacketID.Test, PacketHandler.TestHandler);
;

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);                         // - MakePacket 호출
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T p = new T();
        p.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(p.Protocol, out action))
            action.Invoke(session, p);                           // - PacketHandler에 있는 함수 호출
    }
}
