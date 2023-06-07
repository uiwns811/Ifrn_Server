using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    internal class Session
    {
        Socket _socket;
        int _disconnected = 0;

        // sendArgs 재사용
        object _lock = new object();   
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);        // 버퍼 0부터 시작하겠다. 버퍼의 사이즈느 1024이다

            // recvArgs.UserToken = 1;     // 숫자, this 등 넣어줄 수 있음

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);


            RegisterRecv();    
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            // Send에서 lock하고 있으니 여기선 별도로 lock 안해줘도 됨
         
            // 보낼 정보들을 list로 연결된다. -> BufferList로 한 번에 넣자
            // BufferList != null 인데 SetBuffer 하면 Error!!

            while(_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
                // ArraySegment : Array의 일부. 

                // C++ :  a[10]에서 4번째 원소를 표현하고 싶으면 걍 포인터로 넘겨주면 됨.
                // C#  : 포인터가 없으므로, 시작 인덱스와 범위를 넣어줘야 한다.
                // - 그래서 대부분 범위를 표현할 때는 (버퍼, 시작 인덱스, 크기)를 세트로 넘겨줘야 한다.
                // -
            }
            _sendArgs.BufferList = _pendingList;
            // BufferList.Add 안되고, 미리 만들어놓은 list로 대입만 가능 (문법이 그럼)

            // 현재 _sendArgs.Buffer = null

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // lock이 필요한 이유 : RegisterSend에서 호출된게 아니더라도 다른 쓰레드에서 Callback으로 호출된 것일 수도 있음
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        Console.WriteLine($"Transfferd Bytes : {_sendArgs.BytesTransferred}");

                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // recv한 byte == 0 -> 연결 끊김
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)       // BytesTransferred = 받은 바이트 수 
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {ex}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
