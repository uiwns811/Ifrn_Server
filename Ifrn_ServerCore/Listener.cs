using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;
            // onAcceptHandler = 핸들러에 등록할 함수. 현재는 Accept되면 호출할 함수를 인자로 받는다

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기 수 (웨이팅 몇 명까지 받을건지) : 초과 시 Fail
            _listenSocket.Listen(10);

            // 동접 극도로 높이고 싶다면 아래 부분을 루프돌려도 괜찮음
            for (int i = 0; i < 1; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();           // 한 번 만들면 재사용 가능
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                // RegisterAccept에서 pending == true라면
                // 완료됐을 때 args가 자동으로 OnAcceptCompletetd를 호출해줌
                // - Completed라는 델리게이트에 OnAcceptCompleted라는 함수를 등록해준다.

                // 최초 한 번은 등록 (초기화)
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // args 재사용의 단점 : 기존 args의 내용을 지우고 넣어줘야 한다. 
            // - OnAcceptCompleted가 완료됐으면 args의 AcceptSocket = 대리인의 socket으로 만들어짐
            // - 안지우고 넣어주면 AcceptSocket != null
            // - 이는 args가 초기화된 값이 아니라는 의미임. 
            // - 이벤트 재사용 시 기존값을 없애고 다시 시작해야 한다.

            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);         // 비동기방식으로 예약만 한다.
            if (pending == false)
                OnAcceptCompleted(null, args); 
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success) 
            {
                // 실제 유저 Accept했을 때 해야 할 일
                
                _onAcceptHandler.Invoke(args.AcceptSocket);         // args = 일꾼
                // 등록한 핸들러의 함수 실행
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args);   
            // 완료됐으니까 다음 턴을 위해 다시 등록
        }

        // 콜백함수는 호출되는 순간 쓰레드 풀에서 별도의 쓰레드를 꺼내와 실행한다.
    }
}
