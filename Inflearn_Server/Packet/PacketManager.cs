﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // OnRecvPacket에서 처리하는 부분을 자동화
    // PacketHandler를 등록한 뒤에는 수정할 일이 없음 -> 싱글톤으로 관리
    internal class PacketManager
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
}
