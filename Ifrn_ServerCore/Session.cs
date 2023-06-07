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

        object _lock = new object();   
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;        // 한 번이라도 RegisterSend했으면 true. 작업 완료되면 false.
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();


        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);        // 버퍼 0부터 시작하겠다. 버퍼의 사이즈느 1024이다
                                                                // 경우에 따라 시작 offset이 0이 아닌 경우가 있음

            // recvArgs.UserToken = 1;     // 숫자, this 등 넣어줄 수 있음

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);


            RegisterRecv(recvArgs);    
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false)
                    RegisterSend();

                // _pending : 다른 쓰레드가 send를 예약한 상태인지 판별
                // _pending == false : 아무도 send 안함 -> 내가 함
                // _pending == true : 누가 RegisterSend했고, 아직 Send가 완료되지 않음 (대기)
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
            _pending = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);  

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
                        // 내가 RegisterSend 호출했다고 쳐도, pending == true여서 좀이따 OnSendCompleted가 호출된 경우
                        // 다른 쓰레드가 Send하면 Enqueue만 함 -> 나중에라도 누가 SendQueue를 해주면 됨
                        // -> SendQueue 처리
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                        else
                            _pending = false;
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

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);
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

                    RegisterRecv(args);
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
