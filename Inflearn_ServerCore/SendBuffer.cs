using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        // 쓰레드마다 고유하게 가지는 SendBuffer (TLS)
        // - 다른 쓰레드에서 Open/Close해도 내꺼는 접근 불가
        // 결국 _buffer에 Write하는 부분은 Open/Close만 있는데 이 때는 TLS에 있으니 멀티쓰레드에 안전
        // 나머지는 _buffer를 읽기만 하니까 안전하다

        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(()=> { return null; });

        // 쓰레드마다 Chunk가 있고, 이를 할당해서 쓴다.
        public static int ChunkSize { get; set; } = 4096;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // 버퍼의 잔여 공간보다 요청한 크기가 더 큼 -> 새로운 버퍼 필요
            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        // [][][]][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0;          // recvbuff의 writePos
        
        // sendBuffer : 일회용

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }
        
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return new ArraySegment<byte>();

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }       // 너가 사용한 범위는 여기까지였다.
    }
}

// sendBuffer를 Session의 멤버변수로 두지 않는 이유
// - send는 굉장히 많이, 빈번히 일어난다.
// 각각의 sendbuff를 가지고 있다면 매 번 모든 session의 sendbuff에 복사해줘야 한다.

// send해야할 때 sendbuff를 한 번 만들어주고,
// send할 때 루프돎녀서 싹 send해버리면 복사 횟수가 줄어든다.
// - 불필요한 복사 줄여진다.