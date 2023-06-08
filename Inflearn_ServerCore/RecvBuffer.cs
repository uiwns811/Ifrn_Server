using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflearn_ServerCore
{
    public class RecvBuffer
    {
        // [r][][][][w][][][][][]
        ArraySegment<byte> _buffer;
        int _readPos;       // 읽고 있는 커서
        int _writePos;      // 커서
        // 5byte recv 했으면 _writePos를 5byte 미뤄줌
        // 처리할 수 있는 패킷 크기만큼 왔으면 _readPos를 _writePos로 옮겨줌.
        // - 아직 덜 왔으면 _readPos는 그 자리에서 기다림
        // - 성공적인 처리가 되면 r = w

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }        // 버퍼 내 아직 처리되지 않은 사이즈
        public int FreeSize { get { return _buffer.Count - _writePos; } }   // 버퍼 내 빈공간

        public ArraySegment<byte> ReadSegment { get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); } }      // 어디서부터 읽으면 되니? (현재까지 받은 데이터의 유효범위)
        public ArraySegment<byte> WriteSegment { get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); } }      // 다음 Recv할 때의 유효범위 

        public void Clean()
        {
            // r, w를 앞으로 밀어주는 함수 (안하면 버퍼가 끝까지 밀려남)
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 클라에서 보내준 모든 데이터를 처리한 상태
                // - 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 찌끄레기가 있으면 시작 위치로 복사
                // 즉, DataSize 내부의 데이터를 시작 위치로 복사시키고, r을 0으로 옮기고 w도 옮겨서 앞으로 쭉 ~ 밀어줌
                Array.Copy(_buffer.Array,  _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize) 
                return false;    
            _writePos += numOfBytes;
            return true;
        }
    }
}
