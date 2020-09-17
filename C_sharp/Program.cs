using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using LattePanda.Firmata;

namespace ROVServer
{
    
    public partial class Form1 : Form
    {

        //受信データ
        public string RcvData;
        //受信フラグ
        public bool Rcvflg;

        //リンクフラグ
        public bool Lnkflg;

        // ソケット・リスナー
        private TcpListener myListener;
        // クライアント送受信
        private ClientTcpIp[] myClient = new ClientTcpIp[4];    //System.NET の参照設定が必要！！

        private System.Windows.Forms.Label[] lbl;           //bottunステータス
        private System.Windows.Forms.TrackBar[] tBar;       //JoyStick Value

        //Arduino 定義
        private Arduino arduino; 

        private bool LedSts;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Arduino 使用開始
            arduino = new Arduino();

        /*    arduino.pinMode(13, Arduino.OUTPUT);
            LedSts = false;
         */   
            //Servo modeに
            arduino.pinMode(11, Arduino.SERVO);       //D5
            arduino.pinMode(5, Arduino.SERVO);       //D6
            arduino.pinMode(6, Arduino.SERVO);       //D9
            arduino.pinMode(9, Arduino.SERVO);      //D10
            arduino.pinMode(10, Arduino.SERVO);      //D11
            arduino.pinMode(13, Arduino.SERVO);      //D13

            arduino.servoWrite(11, 91);      //No.1
            arduino.servoWrite(5, 91);      //No.2
            arduino.servoWrite(6, 91);      //No.3
            arduino.servoWrite(9, 91);      //No.4
            arduino.servoWrite(10, 91);     //No.5
            arduino.servoWrite(13, 91);     //No.6

            // IPアドレス＆ポート番号設定
            int myPort = 2001;
            //IPAddress myIp = Dns.Resolve("localhost").AddressList[0]; // 旧バージョン
            string hostname = Dns.GetHostName();
            IPAddress[] mIP = Dns.GetHostAddresses(hostname);
            string strTmp;
            IPAddress myIp=mIP[0];
            foreach (IPAddress address in mIP)
            {
                strTmp = address.ToString();
                if (strTmp.Substring(0, 3) == "192")
                {
                    myIp = address;
                    break;
                }
            }
            Lnkflg = false;
            
            IPEndPoint myEndPoint = new IPEndPoint(myIp, myPort);

            //受信バッファー,フラグクリア
            RcvData = "";
            Rcvflg = false;

            // リスナー開始
            myListener = new TcpListener(myEndPoint);
            myListener.Start();

            // クライアント接続待ち開始
            Thread myServerThread = new Thread(new ThreadStart(ServerThread));
            myServerThread.Start();
            timer1.Enabled = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            arduino.Close();
            this.Close();
        }

        // フォームクローズ時のソケット切断処理
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // リスナー終了
            myListener.Stop();
            // クライアント切断
            for (int i = 0; i <= myClient.GetLength(0) - 1; i++)
            {
                if (myClient[i] != null)
                {
                    if (myClient[i].objSck.Connected == true)
                    {
                        // ソケットクローズ
                        myClient[i].objStm.Close();
                        myClient[i].objSck.Close();
                    }
                }
            }
        }
        // クライアント接続待ちスレッド
        private void ServerThread()
        {      
            try
            {
                int intNo;
                while (true)
                {
                    // ソケット接続待ち
                    TcpClient myTcpClient = myListener.AcceptTcpClient();
                    // クライアントから接続有り
                    for (intNo = 0; intNo <= myClient.GetLength(0) - 1; intNo++)
                    {
                        if (myClient[intNo] == null)
                        {
                            break;
                        }
                        else if (myClient[intNo].objSck.Connected == false)
                        {
                            break;
                        }
                        //else { 
                        //接続されていたら
                       // if (myClient[intNo] != null)
                      //  {
                       //     if (myClient[intNo].rcvflg == true)
                        //    {
                                //受信フラグクリア
                        //        myClient[intNo].rcvflg = false;
                        //        if (Rcvflg != true)
                        //        {
                        //            RcvData = myClient[intNo].rcvdat;
                        //            Rcvflg = true;
                         //       }

                        //    }

                        //}

                    }
                    if (intNo < myClient.GetLength(0))
                    {
                        // クライアント送受信オブジェクト生成
                        myClient[intNo] = new ClientTcpIp();
                        myClient[intNo].intNo = intNo + 1;
                        myClient[intNo].objSck = myTcpClient;
                        myClient[intNo].objStm = myTcpClient.GetStream();
                        // クライアントとの送受信開始
                        Thread myClientThread = new Thread(
                            new ThreadStart(myClient[intNo].ReadWrite));
                        myClientThread.Start();
                        Lnkflg = true;
                        myClient[intNo].Lnkflg = true;
                    }
                    else
                    {
                        // 接続拒否
                        myTcpClient.Close();
                    }
                }
            }
            catch { }
        }
        // クライアント送受信クラス
        public class ClientTcpIp
        {
            //受信データ
            private string RcvData;
            //受信フラグ
            private bool Rcvflag;

            //接続フラグ
            private bool LnkFlg;

            public int intNo;
            public TcpClient objSck;
            public NetworkStream objStm;

            //受信データ参照
            public string rcvdat
            {
                get
                {
                    return RcvData;
                }
                set
                {
                    RcvData = value;
                }
            }
            //受信フラグ参照
            public bool rcvflg
            {
                get
                {
                    return Rcvflag;
                }
                set
                {
                    Rcvflag = value;
                }

            }
            //リンクフラグ参照
            public bool Lnkflg
            {
                get
                {
                    return LnkFlg;
                }
                set
                {
                    LnkFlg = value;
                }

            }
            // クライアント送受信スレッド
            public void ReadWrite()
            {
                //送受信タイムアウト設定
                objStm.ReadTimeout = 5000;
                objStm.WriteTimeout = 5000;
               
                while (true)
                {
                    // ソケット受信
                    Byte[] rdat = new Byte[1024];
                    int ldat = 0;

                    //クライアントから送られたデータを受信する
                    System.Text.Encoding enc = System.Text.Encoding.UTF8;
                    //  bool disconnected = false;
                    //受信したデータはいったんバッファーへ保存
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    string resMsg;
                    try
                    {
                        do
                        {
                            //LF受信までデータ取得する
                            //データの一部を受信する
                            ldat = objStm.Read(rdat, 0, rdat.GetLength(0));
                            if (ldat == 0)   //あってはいけない？
                            {
                                ldat = 0;   //受信しないことにする
                                break;
                            }
                            //受信したデータを蓄積する
                            ms.Write(rdat, 0, ldat);
                            //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                            // 受信を続ける
                        } while (objStm.DataAvailable || rdat[ldat - 1] != '\n');

                        //受信したデータを文字列に変換
                         resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                        ms.Close();
                        //末尾の\nを削除
                        resMsg = resMsg.TrimEnd('\n');

                        //受信データを保持
                        rcvflg = true;
                        RcvData = resMsg;

                        if (ldat > 0)
                        {
                            // クライアントからの受信データ有り
                            // 送信データ作成
                            Byte[] sdat = new Byte[ldat];
                            Array.Copy(rdat, sdat, ldat);
                          //  String msg = "(" + intNo + ")" +
                            String msg = System.Text.Encoding.GetEncoding("SHIFT-JIS").GetString(sdat);
                            sdat = System.Text.Encoding.GetEncoding("SHIFT-JIS").GetBytes(msg);
                            // ソケット送信
                            objStm.Write(sdat, 0, sdat.GetLength(0));
                            
                        }
                        else
                        {
                            // ソケット切断有り
                            // ソケットクローズ
                            objStm.Close();
                            objSck.Close();
                            Lnkflg = false;
                            return;
                        }
                    }
                    catch {
                     

                    }
                }
               
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string rdat;
            int sVal = 0;
            int sVal2 = 0;
            int sVal3 = 0;
            int[] defVal0 = new int[7];
            int[] defVal1 = new int[7];
            int[] defVal2 = new int[7];
            int[] defVal3 = new int[7];

            /*  defVal[0] = 54;
              defVal[1] = 68;
              defVal[2] = 83;
              defVal[3] = 90;
              defVal[4] = 103;
              defVal[5] = 118;
              defVal[6] = 132;
  */
  //yoko
            defVal0[0] = 82;
            defVal0[1] = 85;
            defVal0[2] = 88;
            defVal0[3] = 91;
            defVal0[4] = 94;
            defVal0[5] = 97;
            defVal0[6] = 100;
//zenshin koutai
            defVal1[0] = 73;
            defVal1[1] = 79;
            defVal1[2] = 85;
            defVal1[3] = 91;
            defVal1[4] = 97;
            defVal1[5] = 113;
            defVal1[6] = 119;
//jouge
            defVal2[0] = 61;
            defVal2[1] = 71;
            defVal2[2] = 81;
            defVal2[3] = 91;
            defVal2[4] = 101;
            defVal2[5] = 111;
            defVal2[6] = 121;
//ROLL
            defVal3[0] = 82;
            defVal3[1] = 85;
            defVal3[2] = 88;
            defVal3[3] = 91;
            defVal3[4] = 94;
            defVal3[5] = 97;
            defVal3[6] = 100;

            string[] oldDat = new string[5];    //前のデータ保存用

        /*    if (LedSts == false)
            {
                arduino.digitalWrite(13, Arduino.HIGH);
                LedSts = true;
            }
            else
            {
                arduino.digitalWrite(13, Arduino.LOW);
                LedSts = false;
            }
*/

            //JoyStick Value
            this.tBar = new System.Windows.Forms.TrackBar[5];
            this.tBar[0] = trackBar1;
            this.tBar[1] = trackBar2;
            this.tBar[2] = trackBar3;
            this.tBar[3] = trackBar4;
            this.tBar[4] = trackBar5;
     
            //ラベルコントロール配列の作成
            this.lbl = new System.Windows.Forms.Label[12];
            //ラベルコントロールの配列にすでに作成されているインスタンスを代入
            this.lbl[0] = this.label6;
            this.lbl[1] = this.label7;
            this.lbl[2] = this.label8;
            this.lbl[3] = this.label9;
            this.lbl[4] = this.label10;
            this.lbl[5] = this.label11;
            this.lbl[6] = this.label12;
            this.lbl[7] = this.label13;
            this.lbl[8] = this.label14;
            this.lbl[9] = this.label15;
            this.lbl[10] = this.label16;
            this.lbl[11] = this.label17;

            for (int i = 0;i< 4; i++)
            {
                if (myClient[i] != null)
                {
                    //接続チェック
                    if (myClient[i].Lnkflg == true)
                    {
                        this.label1.Text = "接続中";
                    }
                    else
                    {
                        this.label1.Text = "接続待ち";
                    }
                    //接続中なら
                    if(this.label1.Text == "接続中")
                    {
                        //受信していたら
                        if (myClient[i].rcvflg == true)
                        {
                            myClient[i].rcvflg = false;
                            rdat = myClient[i].rcvdat;
                            label2.Text = rdat;
                            //受信データ分割
                            string[] rcvdat = rdat.Split(',');

                            //前のデータ保存
                            oldDat[0] = label18.Text;
                            oldDat[1] = label19.Text;
                            oldDat[2] = label20.Text;
                            oldDat[3] = label21.Text;
                            oldDat[4] = label22.Text;
                         
                            //データ代入
                            label18.Text = rcvdat[0];
                            label19.Text = rcvdat[1];
                            label20.Text = rcvdat[2];
                            label21.Text = rcvdat[3];
                            label22.Text = rcvdat[4];
                            if (oldDat[4] != rcvdat[4])
                            {
                                sVal = int.Parse(rcvdat[4]) - 1;
                                sVal2 = defVal3[sVal];
                                sVal3 = defVal3[sVal];

                                arduino.servoWrite(11, sVal2);      //No.1
                                arduino.servoWrite(5, sVal3);      //No.2
                            }
                            else { 
                                //横
                                if (oldDat[0] != rcvdat[0])
                                {
                                    sVal = int.Parse(rcvdat[0]) - 1;
                                    sVal2 = defVal0[6 - sVal];
                                    sVal3 = defVal0[sVal];

                                    arduino.servoWrite(11, sVal2);      //No.1
                                    arduino.servoWrite(5, sVal3);      //No.2

                                }

                                //前進

                                if (oldDat[1] != rcvdat[1])
                                {
                                    sVal = int.Parse(rcvdat[1]) - 1;
                                    sVal2 = defVal1[6 - sVal];
                                    sVal3 = defVal1[6 - sVal];

                                    arduino.servoWrite(10, sVal2);     //No.5
                                    arduino.servoWrite(13, sVal3);     //No.6

                                }

                            }

                            //上下
                            if(oldDat[2] != rcvdat[2])
                            {
                                sVal = int.Parse(rcvdat[2])-1;
                                sVal2 = defVal2[sVal];
                                sVal3 = defVal2[sVal];

                                arduino.servoWrite(6, sVal2);      //No.3
                                arduino.servoWrite(9, sVal3);      //No.4
                                

                            }
                            
                                
                            //スライダーに値代入
                            for (i = 0; i < 5; i++)
                            {
                                tBar[i].Value = Int16.Parse(rcvdat[i]);
                            }
                            //スイッチの状態
                            for (i = 0; i < 12; i++)
                            {
                                if (rcvdat[i + 5] == "1")
                                {
                                    lbl[i].BackColor = Color.YellowGreen;
                                }
                                else
                                {
                                    lbl[i].BackColor = Color.White;
                                }
                            }

                            
                        }

                    }
                }
            }
            
            
        }

        private void label19_Click(object sender, EventArgs e)
        {

        }
    }

    
}