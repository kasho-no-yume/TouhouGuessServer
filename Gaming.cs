using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouGuessServer
{
    enum State
    {
        guessing,       //正常的可猜测阶段。这个时候可以接收回答。
        revealing,      //接收到回答的揭示阶段。这个时候接收的回答要丢弃。接收到回答全转发，然后两秒后转成waiting。客户端也应等两秒显示答案。给个两秒防止正常的延迟
        waiting         //揭示完成，等待主机发送下一题的数据。这个时候接收的回答也丢弃。刚开始游戏也这个状态。此外，如果游戏结束，也从这个状态退出。
    }
    internal class Gaming
    {
        private State state;
        private int leftRound;
        public delegate void boardcast(Message message);
        private boardcast Boardcast;
        public delegate void gameEnd();
        private gameEnd GameEnd;
        public Gaming(int round,boardcast board, gameEnd gameEnd) 
        {
            this.state = State.waiting;
            this.leftRound = round;
            this.Boardcast = board;
            this.GameEnd = gameEnd;
        }

        public async void SomeoneAnswer(Message msg)
        {
            if(state == State.guessing)
            {
                state = State.revealing;
                Boardcast(new Message(STCME.SomeoneGuess,msg.data));
                this.leftRound--;
                if(leftRound == 0)
                {
                    await Task.Delay(4000);
                    GameEnd();
                    return;
                }
                await Task.Delay(2000);
                state = State.waiting;
            }
        }

        public void GetQuestion(Message message)
        {
            if(state == State.waiting)
            {
                state = State.guessing;
                Boardcast(new Message(STCME.NewQuesData,message.data));
            }
        }

        public void SomeoneWaive(Message msg)
        {
            if(state == State.guessing)
            {
                Boardcast(new Message(STCME.SomeoneWaive,msg.data));
            }
        }
    }
}
