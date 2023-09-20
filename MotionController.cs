using System;
using System.Collections;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EposCmd.Net;
using EposCmd.Net.DeviceCmdSet.Operation;


namespace BinderJetMotionControllerVer._1
{
    public partial class MotionController : Form
    {
        public string[] NMCDesc = {
                                "NMC2_220S"
                                ,"NMC2_420S"
                                ,"NMC2_620S"
                                ,"NMC2_820S"
                                ,"NMC2_220_DIO32"
                                ,"NMC2_220_DIO64"
                                ,"NMC2_420_DIO32"
                                ,"NMC2_420_DIO64"
                                ,"NMC2_820_DIO32"
                                ,"NMC2_820_DIO64"
                                ,"NMC2_DIO32"
                                ,"NMC2_DIO64"
                                ,"NMC2_DIO96"
                                ,"NMC2_DIO128"
                                ,"NMC2_220"
                                ,"NMC2_420"
                                ,"NMC2_620"
                                ,"NMC2_820"
                                ,"NMC2_620_DIO32"
                                ,"NMC2_620_DIO64"
                                ,null
                                };
        PaixMotion PaixMotion;
        Thread TdWatchSensor;
        Thread UpdateLogger;
        Paix_MotionController.NMC2.NMCAXESEXPR NmcData = new Paix_MotionController.NMC2.NMCAXESEXPR();
        Paix_MotionController.NMC2.NMCAXESEXPR NmcData2 = new Paix_MotionController.NMC2.NMCAXESEXPR();
        private SerialPort Serial_RS485 = new SerialPort();
        public ArrayList arrSerialbuff = new ArrayList();
        XAARWinform.PrintheadCon Printhead = new XAARWinform.PrintheadCon();
        


        // private int Ping = 0;
        // private int Position = 1;
        // private int Speed = 2;
        // private int Control_ON_OFF = 3;

        public uint ID_485;
        public string State_485;
        public string Setting_485;


        private const short Xaxis = 0;
        private const short X2axis = 1;
        private const short Yaxis = 3;
        private const short Zaxis = 2;
        private const short Raxis = 0;
        private const short R2axis = 1;
        private const short ON = 1;
        private const short OFF = 0;
        private const short NC = 0;
        private const short NO = 1;
        private const short HomeMode = 2;       //Home 방향 +Limit(0)인지 -Limit(1)인지 +Near(2)인지 -Near(3)인지 설정. +Near면 -Limit 방향으로 Near 검색 이동함.


        //Nmc2.nDir 0: CW, 1:CCW
        private const short CW = 0;
        private const short CCW = 1;

        //Set Speed
        private const short iStartSpeed = 10;
        private const short iAccelSpeed = 100;
        private const short iDeccelSpeed = 100;
        private const short iDriveSpeed = 50;
        private const short zStartSpeed = 1;
        private const short zAccelSpeed = 1;
        private const short zDeccelSpeed = 1;
        private const short zDriveSpeed = 5;
        private const double PulseUnit = 0.001;
        private const double zPulseUnit = 0.0005;
        private const double SpeedLimit = 800;

        private const short rStartSpeed = 100;
        private const double rAccelSpeed = 1000;
        private const double rDeccelSpeed = 1000;
        private const double rDriveSpeed = 100;
        private const double rPulseUnit = 0.1;
        private const double rSpeedLimit = 5000;

        private const double HomeSpeed0 = 15;
        private const double HomeSpeed1 = 15;
        private const double HomeSpeed2 = 15;
        private const double zHomeSpeed0 = 3;
        private const double zHomeSpeed1 = 3;
        private const double zHomeSpeed2 = 3;

        private const double xPosMaxLimit = 845;
        private const double xPosMinLimit = 0;
        private const double x2PosMaxLimit = 820;
        private const double x2PosMinLimit = 0;
        private const double yPosMaxLimit = 300;
        private const double yPosMinLimit = -41;
        private const double zPosMaxLimit = 99.5;
        private const double zPosMinLimit = -29.5;

        public double xPos = 0;
        public double x2Pos = 0;
        public double yPos = 0;
        public double zPos = 0;

        private short devID;
        private short devID2;
        private bool ndevOpen = false;

        private int Currentlayer;
        private int JobLayer;
        private short ImageNum = 0;

        private const short pZ_Down = 0;
        private const short pHeater_ON = 1;
        private const short pRecoater_MoveToMax = 2;
        private const short pZ_Up = 3;
        private const short pPowder_Feed_Ready = 4;
        private const short pRecoater_Recoating = 5;
        private const short pImageProcess = 6;
        private const short pPrint = 7;
        private const short pPrinthead_MoveToMin = 8;
        private const short pAfter_Powder = 17;

        //Densification 용 선언
        private const short rZ_Down = 1;
        private const short rF_Forward = 2;
        private const short rZ_up = 3;
        private const short rF_Backward = 4;
        private const short r_End = 5;
        private bool PrintEnable = false;
        private short PowderIO = 1;

        private short TurnON = 1;
        private short TurnOFF = 0;

        private short small = 0; //위치 관련하여 대소를 판단하는 변수 (비동기 동작에 필요함)
        private short big = 1; //위치 관련하여 대소를 판단하는 변수 (비동기 동작에 필요함)
        double temp = 500; //기본 온도를 500도로 가정 (임의의 값)


        //Asyncind
        internal class ind_powder_supply { }
        internal class ind_head_cleaning { }

        bool logStart = false;
        private string logPath;
        private string logButtonPath;
        private string logtempPath;
        static string dirPath = Environment.CurrentDirectory + @"\log\";
        static DirectoryInfo di = new DirectoryInfo(dirPath);

        private List<DateTime> layertimes = new List<DateTime>();

        private bool rpowder; //리코팅 동작에서 파우더를 뿌릴것인지 체크하는 변수
        private bool rroller; //리코팅 동작에서 롤러를 돌릴것인지 체크하는 변수
        private bool rzmove; //리코팅 동작에서 Z축 동작을 할것인지 체크하는 변수
        private bool head_onv=true; //헤드 클리닝 동작 상태를 표시하는 변수

        //Maxon Motor related
        private DeviceManager _connector;
        private Device _epos;
        StateMachine sm;
        Maxon MaxBLDC = new Maxon();

        public MotionController()
        {
            InitializeComponent();
            PaixMotion = new PaixMotion();
            TdWatchSensor = new Thread(new ThreadStart(watchSensor));
            UpdateLogger = new Thread(new ThreadStart(watchLogger));
            if (!di.Exists) Directory.CreateDirectory(dirPath);
            
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            devID = Convert.ToInt16(txtIPRead.Text);
            devID2 = Convert.ToInt16(txtIPRead2.Text);

            if (btnOpen.Text == "Open" && PaixMotion.Open2(devID2) && PaixMotion.Open(devID))
            {
                ndevOpen = true;
                btnOpen.Enabled = false;
                switch (TdWatchSensor.ThreadState)
                {
                    case ThreadState.Stopped:
                        TdWatchSensor = new Thread(new ThreadStart(watchSensor));
                        UpdateLogger = new Thread(new ThreadStart(watchLogger));
                        break;
                    case ThreadState.Unstarted:
                        break;
                    default:
                        TdWatchSensor.Join();
                        UpdateLogger.Join();
                        Console.WriteLine(TdWatchSensor.ThreadState);
                        btnOpen.Text = "Close";
                        while (TdWatchSensor.ThreadState != ThreadState.Stopped)
                        {
                            if (TdWatchSensor.ThreadState == ThreadState.Running) btnOpen.Enabled = false;
                            else btnOpen.Enabled = true;
                        }
                        break;
                }
                string dtnow = DateTime.Now.ToString("yyyy-MM-ddtthhmmss");
                Console.WriteLine(dtnow);
                logPath = dirPath + dtnow + ".txt";
                logButtonPath = dirPath + dtnow + "_Button.txt";
                logtempPath = dirPath + dtnow + "_temp.txt";
                TdWatchSensor.Start();
                UpdateLogger.Start();
                btnOpen.Text = "Close";
                Console.WriteLine(TdWatchSensor.ThreadState);
                pnlConnection.BackColor = Color.LimeGreen;
                btnOpen.Enabled = true;

                MaxBLDC.Connect();
                MaxBLDC.Enable();

                Logger.WriteButtonLog("Motion Controller Opened", logButtonPath);
                MessageBox.Show("Motion Connected..");

            }
            else if (btnOpen.Text == "Close" && PaixMotion.Close())
            {

                ndevOpen = false;
                //TdWatchSensor.Abort();
                TdWatchSensor.Join();
                UpdateLogger.Join();
                Console.WriteLine(TdWatchSensor.ThreadState);
                while (TdWatchSensor.ThreadState != ThreadState.Stopped)
                {
                    if (TdWatchSensor.ThreadState == ThreadState.Running) btnOpen.Enabled = false;
                    else btnOpen.Enabled = true;
                }
                pnlConnection.BackColor = Color.Red;
                btnOpen.Text = "Open";
                btnOpen.Enabled = true;
                logStart = false;
                Logger.WriteButtonLog("Motion Controller Closed", logButtonPath);
                MessageBox.Show("Motion Disconnetced..");
            }

        }

        private void btnInitialize_Click(object sender, EventArgs e)
        {
            if (PaixMotion.GetNmcStatus(ref NmcData) == false) return;
            //공통설정
            PaixMotion.SetEmerLogic(NC);    //비상 스위치 로직이 NC/NO 인지 
            PaixMotion.SetEmgEnable(OFF);
            Paix_MotionController.NMC2.nmc_SetHomeDelay(devID, 0x12C);
            short[] IO_OFF = { OFF, OFF, OFF, OFF, OFF, OFF, OFF, OFF };
            Paix_MotionController.NMC2.nmc_SetMDIOOutput(devID, IO_OFF);

            //공압 초기화
            //Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 4, 1);  //클램프 열기
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 3, 0);  //  Paix_MotionController.NMC2.nmc_GetMDIOInPin(devID, 4, ); 
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 7, 1);  //


            //X축 설정

            PaixMotion.SetUnitPulse(Xaxis, PulseUnit);  //단위 펄스값
            PaixMotion.SetCmd(Xaxis, 0);    //초기 커맨드 세팅 값   0
            PaixMotion.SetEnc(Xaxis, 0);    //초기 엔코더 세팅 값   0
            PaixMotion.SetPulseLogic(Xaxis, 0); //펄스 모드 설정 값--nLogic = 0 이면, 2Pulse/Active LOW/CW-CCW 세팅.
            PaixMotion.SetEncCountMode(Xaxis, 0);       //엔코더 체배 설정 값--nLogic = 0 이면, 4체배를 의미.
            PaixMotion.SetEncInputMode(Xaxis, 0);       //세팅 값 중 엔코더 input이라 나오는 값. 엔코더 카운트 방향 설정 값--nLogic = 0이면, A|B(+)를 의미.
            PaixMotion.SetNearLogic(Xaxis, NC);         //Home 센서가 -Limit 쪽 방향인지 +Limit 쪽 방향인지 선택하는 설정 값--nLogic = NC이면, -Limit 방향.
            PaixMotion.SetMinusLimitLogic(Xaxis, NC);   //-Limit 센서 로직 설정 값
            PaixMotion.SetPlusLimitLogic(Xaxis, NC);    //+Limit 센서 로직 설정 값
            PaixMotion.SetAlarmLogic(Xaxis, NO);         //알람 입력 로직 설정 값
            PaixMotion.SetZLogic(Xaxis, NO);            //서보모터 엔코더 Z상 입력 로직 설정 값
            PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, iDriveSpeed);
            PaixMotion.SetHomeSpeed(Xaxis, HomeSpeed0, HomeSpeed1, HomeSpeed2);
            PaixMotion.SetSWLimitLogic(Xaxis, ON, xPosMinLimit, xPosMaxLimit);
            xPos = 0;

            //X2축 설정
            //PaixMotion.SetSWLimitLogic(X2axis, ON, 0, 500);
            PaixMotion.SetUnitPulse(X2axis, PulseUnit);
            PaixMotion.SetCmd(X2axis, 0);
            PaixMotion.SetEnc(X2axis, 0);
            PaixMotion.SetPulseLogic(X2axis, 2);
            PaixMotion.SetEncCountMode(X2axis, 0);
            PaixMotion.SetEncInputMode(X2axis, 1);
            PaixMotion.SetNearLogic(X2axis, NC);
            PaixMotion.SetMinusLimitLogic(X2axis, NC);
            PaixMotion.SetPlusLimitLogic(X2axis, NC);
            PaixMotion.SetAlarmLogic(X2axis, NO);
            PaixMotion.SetZLogic(X2axis, NO);
            PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, iAccelSpeed, iDeccelSpeed, iDriveSpeed);
            PaixMotion.SetHomeSpeed(X2axis, HomeSpeed0, HomeSpeed1, HomeSpeed2);
            PaixMotion.SetSWLimitLogic(X2axis, ON, x2PosMinLimit, x2PosMaxLimit);
            x2Pos = 0;

            //Y축 설정
            //PaixMotion.SetSWLimitLogic(Yaxis, ON, 0, 500);
            PaixMotion.SetUnitPulse(Yaxis, PulseUnit);
            PaixMotion.SetCmd(Yaxis, 0);
            PaixMotion.SetEnc(Yaxis, 0);
            PaixMotion.SetPulseLogic(Yaxis, 0);
            PaixMotion.SetEncCountMode(Yaxis, 0);
            PaixMotion.SetEncInputMode(Yaxis, 0);
            PaixMotion.SetNearLogic(Yaxis, NC);
            PaixMotion.SetMinusLimitLogic(Yaxis, NC);
            PaixMotion.SetPlusLimitLogic(Yaxis, NC);
            PaixMotion.SetAlarmLogic(Yaxis, NO);
            PaixMotion.SetZLogic(Yaxis, NO);
            PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, iDriveSpeed);
            PaixMotion.SetHomeSpeed(Yaxis, HomeSpeed0, HomeSpeed1, HomeSpeed2);
            PaixMotion.SetSWLimitLogic(Yaxis, ON, yPosMinLimit, yPosMaxLimit);
            yPos = 0;

            //Z축 설정
            //PaixMotion.SetSWLimitLogic(Zaxis, ON, 0, 500);
            PaixMotion.SetUnitPulse(Zaxis, zPulseUnit);
            PaixMotion.SetCmd(Zaxis, 0);
            PaixMotion.SetEnc(Zaxis, 0);
            PaixMotion.SetPulseLogic(Zaxis, 0);
            PaixMotion.SetEncCountMode(Zaxis, 0);
            PaixMotion.SetEncInputMode(Zaxis, 0);
            PaixMotion.SetNearLogic(Zaxis, NC);
            PaixMotion.SetMinusLimitLogic(Zaxis, NC);
            PaixMotion.SetPlusLimitLogic(Zaxis, NC);
            PaixMotion.SetAlarmLogic(Zaxis, NO);
            PaixMotion.SetZLogic(Zaxis, NO);
            PaixMotion.SetSCurveSpeed(Zaxis, zStartSpeed, zAccelSpeed, zDeccelSpeed, zDriveSpeed);
            PaixMotion.SetHomeSpeed(Zaxis, zHomeSpeed0, zHomeSpeed1, zHomeSpeed2);
            PaixMotion.SetSWLimitLogic(Zaxis, ON, zPosMinLimit, zPosMaxLimit);
            zPos = 0;

            //R축 설정
            Paix_MotionController.NMC2.nmc_SetEmgLogic(devID2, Raxis, NC);
            Paix_MotionController.NMC2.nmc_SetUnitPerPulse(devID2, Raxis, rPulseUnit);
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, Raxis, rStartSpeed, rAccelSpeed, rDeccelSpeed, rDriveSpeed);
            Paix_MotionController.NMC2.nmc_SetPulseLogic(devID2, Raxis, 0);
            Paix_MotionController.NMC2.nmc_SetEncoderCount(devID2, Raxis, 0);
            Paix_MotionController.NMC2.nmc_SetEncoderDir(devID2, Raxis, 0);
            Paix_MotionController.NMC2.nmc_SetNearLogic(devID2, Raxis, NC);
            Paix_MotionController.NMC2.nmc_SetPlusLimitLogic(devID2, Raxis, NC);
            Paix_MotionController.NMC2.nmc_SetMinusLimitLogic(devID2, Raxis, NC);
            Paix_MotionController.NMC2.nmc_SetAlarmLogic(devID2, Raxis, NC);

            Paix_MotionController.NMC2.nmc_SetEmgLogic(devID2, R2axis, NC);
            Paix_MotionController.NMC2.nmc_SetUnitPerPulse(devID2, R2axis, rPulseUnit);
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, R2axis, rStartSpeed, rAccelSpeed, rDeccelSpeed, rDriveSpeed);
            Paix_MotionController.NMC2.nmc_SetPulseLogic(devID2, R2axis, 0);
            Paix_MotionController.NMC2.nmc_SetEncoderCount(devID2, R2axis, 0);
            Paix_MotionController.NMC2.nmc_SetEncoderDir(devID2, R2axis, 0);
            Paix_MotionController.NMC2.nmc_SetNearLogic(devID2, R2axis, NC);
            Paix_MotionController.NMC2.nmc_SetPlusLimitLogic(devID2, R2axis, NC);
            Paix_MotionController.NMC2.nmc_SetMinusLimitLogic(devID2, R2axis, NC);
            Paix_MotionController.NMC2.nmc_SetAlarmLogic(devID2, R2axis, NC);

            Logger.WriteButtonLog("Intialized", logButtonPath);

            btn485OPEN();
            picinitialize();//리코터 체크 항목 관련 초기화 진행

            


        }

        public void watchSensor()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1);
                if (ndevOpen == false) break;
                this.Invoke(new delegateUpdateCmdEnc(updateCmdEnc));
                this.Invoke(new delegateUpdateOutIn(updateOutIn));
                
            }
        }
        public void watchLogger()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                if (ndevOpen == false) break;
                this.Invoke(new delegateUpdateLog(updateLog));
            }
        }

        private delegate void delegateUpdateCmdEnc();
        private delegate void delegateUpdateOutIn();
        private delegate void delegateUpdateLog();
        //private delegate void delegateRecoating();

        private void updateOutIn()
        {
            if (ndevOpen == false) return;
            short nret;
            short[] InStatus = new short[8];
            short[] OutStatus = new short[8];
            short[] outpnls = { 0, 4 };
            short[] inpnls = { 4 };
            nret = Paix_MotionController.NMC2.nmc_GetMDIOInput(devID, InStatus);
            foreach (int i in inpnls)
            {
                Panel pnlIn = (Controls.Find("pnlIO_In" + i.ToString(), true)[0] as Panel);
                if (InStatus[i] == ON)
                {
                    pnlIn.BackColor = Color.LimeGreen;
                    pnlIn.ForeColor = Color.LimeGreen;
                }

                else if (InStatus[i] == OFF)
                {
                    pnlIn.BackColor = SystemColors.Info;
                    pnlIn.ForeColor = SystemColors.Info;

                }
            }

            nret = Paix_MotionController.NMC2.nmc_GetMDIOOutput(devID, OutStatus);
            foreach (int i in outpnls)
            {
                Panel pnlOut = (Controls.Find("pnlIO_Out" + i.ToString(), true)[0] as Panel);
                if (OutStatus[i] == ON) pnlOut.BackColor = Color.OrangeRed;
                else if (OutStatus[i] == OFF) pnlOut.BackColor = SystemColors.Info;
            }
            showio(InStatus[4], OutStatus[0], OutStatus[4]);


        }
        public void updateLog()
        {
            if (!logStart)
            {
                //log title
                Logger.WriteLog("Logging Time\t", false, logPath);

                Logger.WriteLog("IN PUT_0\t", false, logPath);
                Logger.WriteLog("IN PUT_1\t", false, logPath);
                Logger.WriteLog("IN PUT_2\t", false, logPath);
                Logger.WriteLog("IN PUT_3\t", false, logPath);
                Logger.WriteLog("IN PUT_4\t", false, logPath);
                Logger.WriteLog("IN PUT_5\t", false, logPath);
                Logger.WriteLog("IN PUT_6\t", false, logPath);
                Logger.WriteLog("IN PUT_7\t", false, logPath);

                Logger.WriteLog("OUT PUT_0\t", false, logPath);
                Logger.WriteLog("OUT PUT_1\t", false, logPath);
                Logger.WriteLog("OUT PUT_2\t", false, logPath);
                Logger.WriteLog("OUT PUT_3\t", false, logPath);
                Logger.WriteLog("OUT PUT_4\t", false, logPath);
                Logger.WriteLog("OUT PUT_5\t", false, logPath);
                Logger.WriteLog("OUT PUT_6\t", false, logPath);
                Logger.WriteLog("OUT PUT_7\t", false, logPath);

                Logger.WriteLog("Command_X\t", false, logPath);
                Logger.WriteLog("Command_X2\t", false, logPath);
                Logger.WriteLog("Command_Y\t", false, logPath);
                Logger.WriteLog("Command_Z\t", false, logPath);

                Logger.WriteLog("Position_X\t", false, logPath);
                Logger.WriteLog("Position_X2\t", false, logPath);
                Logger.WriteLog("Position_Y\t", false, logPath);
                Logger.WriteLog("Position_Z\t", false, logPath);

                Logger.WriteLog("Home Pos_X\t", false, logPath);
                Logger.WriteLog("Home Pos_X2\t", false, logPath);
                Logger.WriteLog("Home Pos_Y\t", false, logPath);
                Logger.WriteLog("Home Pos_Z\t", false, logPath);

                Logger.WriteLog("Minus Limit_X\t", false, logPath);
                Logger.WriteLog("Minus Limit_X2\t", false, logPath);
                Logger.WriteLog("Minus Limit_Y\t", false, logPath);
                Logger.WriteLog("Minus Limit_Z\t", false, logPath);

                Logger.WriteLog("Plus Limit_X\t", false, logPath);
                Logger.WriteLog("Plus Limit_X2\t", false, logPath);
                Logger.WriteLog("Plus Limit_Y\t", false, logPath);
                Logger.WriteLog("Plus Limit_Z\t", true, logPath);


            }

            //time log
            Logger.WriteLog(DateTime.Now.ToString("yyyy - MM - ddtthhmmss"), false, logPath);

            //input log
            short[] InStatus = new short[8];

            short nRet;
            nRet = Paix_MotionController.NMC2.nmc_GetMDIOInput(devID, InStatus);
            for (int i = 0; i < 8; i++)
            {
                Logger.WriteLog("\t"+Convert.ToString(InStatus[i]), false, logPath);
            }
            //output log
            short[] OutStatus = new short[8];
            nRet = Paix_MotionController.NMC2.nmc_GetMDIOOutput(devID, OutStatus);
            for (int i = 0; i < 8; i++)
            {
                Logger.WriteLog("\t"+Convert.ToString(OutStatus[i]), false, logPath);
            }

            //command log
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dCmd[Xaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dCmd[X2axis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dCmd[Yaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dCmd[Zaxis]), false, logPath);

            //position log
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dEnc[Xaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dEnc[X2axis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dEnc[Yaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.dEnc[Zaxis]), false, logPath);

            //home log
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nNear[Xaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nNear[X2axis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nNear[Yaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nNear[Zaxis]), false, logPath);

            //minus limit log
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nMLimit[Xaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nMLimit[X2axis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nMLimit[Yaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nMLimit[Zaxis]), false, logPath);

            //plus limit log
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nPLimit[Xaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nPLimit[X2axis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nPLimit[Yaxis]), false, logPath);
            Logger.WriteLog("\t"+Convert.ToString(NmcData.nPLimit[Zaxis]), true, logPath);

            logStart = true;
        }
        public void updateCmdEnc()
        {
            if (PaixMotion.GetNmcStatus(ref NmcData) == false) return;
            if (PaixMotion.GetNmcStatus2(ref NmcData2) == false) return;

            if (NmcData.dEnc[Xaxis] != NmcData.dCmd[Xaxis])
            {
                pnlPosErrX.BackColor = Color.OrangeRed;
            }
            else pnlPosErrX.BackColor = SystemColors.Info;

            if (NmcData.dEnc[X2axis] != NmcData.dCmd[X2axis])
            {
                pnlPosErrX2.BackColor = Color.OrangeRed;
            }
            else pnlPosErrX2.BackColor = SystemColors.Info;

            if (NmcData.dEnc[Yaxis] != NmcData.dCmd[Yaxis])
            {
                pnlPosErrY.BackColor = Color.OrangeRed;
            }
            else pnlPosErrY.BackColor = SystemColors.Info;

            if (NmcData.dEnc[Zaxis] != NmcData.dCmd[Zaxis])
            {
                pnlPosErrZ.BackColor = Color.OrangeRed;
            }
            else pnlPosErrZ.BackColor = SystemColors.Info; ;

            //pnlEmerX.BackColor = NmcData.nEmer[Xaxis] == 1 ? Color.Red : Color.LightYellow;
            //pnlEmerX2.BackColor = NmcData.nEmer[X2axis] == 1 ? Color.Red : Color.LightYellow;
            //pnlEmerY.BackColor = NmcData.nEmer[Yaxis] == 1 ? Color.Red : Color.LightYellow;
            //pnlEmerZ.BackColor = NmcData.nEmer[Zaxis] == 1 ? Color.Red : Color.LightYellow;

            txtMotionPositionX.Text = (NmcData.dEnc[Xaxis]).ToString(format:"0.00");
            txtMotionPositionX2.Text = (NmcData.dEnc[X2axis]).ToString(format: "0.00");
            txtMotionPositionY.Text = (NmcData.dEnc[Yaxis]).ToString(format: "0.00");
            txtMotionPositionZ.Text = (NmcData.dEnc[Zaxis]).ToString(format: "0.00");


            //비상정지시퀸스
            if (NmcData.dEnc[Xaxis] + NmcData.dEnc[X2axis]* Convert.ToDouble(PrintEnable) > 925 )
            {
                PrintEnable = false;
                PaixMotion.EmergencyStop();
                //Printhead.Disconnect();
                //BLDC_STOP_0();
                //BLDC_STOP_1();
                maxon_stop();
                //BLDC_STOP_2();
                Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 0, 0);
                string processstring = Convert.ToString(DateTime.Now) + "충돌 방지를 위한 비상 정지 동작";
                processbox.Items.Insert(0, processstring);
            }

            pnlBusyX.BackColor = NmcData.nBusy[Xaxis] == 1 ? Color.Red : SystemColors.Info;
            pnlBusyX2.BackColor = NmcData.nBusy[X2axis] == 1 ? Color.Red : SystemColors.Info;
            pnlBusyY.BackColor = NmcData.nBusy[Yaxis] == 1 ? Color.Red : SystemColors.Info;
            pnlBusyZ.BackColor = NmcData.nBusy[Zaxis] == 1 ? Color.Red : SystemColors.Info;

            pnlServoONX.BackColor = NmcData.nAlarm[Xaxis] == 1 ? Color.Orange : Color.LimeGreen;
            pnlServoONX2.BackColor = NmcData.nAlarm[X2axis] == 1 ? Color.Orange : Color.LimeGreen;
            pnlServoONY.BackColor = NmcData.nAlarm[Yaxis] == 1 ? Color.Orange : Color.LimeGreen;
            pnlServoONZ.BackColor = NmcData.nAlarm[Zaxis] == 1 ? Color.Orange : Color.LimeGreen;

            X1load.BackColor = NmcData.nEncZ[Xaxis] == 1 ? Color.Red : Color.LightYellow;
            X2load.BackColor = NmcData.nEncZ[X2axis] == 1 ? Color.Red : Color.LightYellow;
            Yload.BackColor = NmcData.nEncZ[Yaxis] == 1 ? Color.Red : Color.LightYellow;
            Zload.BackColor = NmcData.nEncZ[Zaxis] == 1 ? Color.Red : Color.LightYellow;

            int lx1 = (NmcData.lEnc[Xaxis] - NmcData.lCmd[Xaxis]);
            int lx2 = (NmcData.lEnc[X2axis] - NmcData.lCmd[X2axis]);
            int ly = (NmcData.lEnc[Yaxis] - NmcData.lCmd[Yaxis]);
            int lz = (NmcData.lEnc[Zaxis] - NmcData.lCmd[Zaxis]);
            short[] axiss = { 0, 1, 3, 2 };
            int[] aloads = { lx1, lx2, ly, lz };
            for (int i = 0; i < 4; i++)
            {
                calcload(aloads[i], axiss[i]);
            }

            pnlHomeX.BackColor = NmcData.nNear[Xaxis] == 1 ? Color.Red : Color.LightYellow;
            pnlHomeX2.BackColor = NmcData.nNear[X2axis] == 1 ? Color.Red : Color.LightYellow;
            pnlHomeY.BackColor = NmcData.nNear[Yaxis] == 1 ? Color.Red : Color.LightYellow;
            pnlHomeZ.BackColor = NmcData.nNear[Zaxis] == 1 ? Color.Red : Color.LightYellow;

            pnlLimitMaxX.BackColor = NmcData.nPLimit[Xaxis] == 1 ? Color.Red : Color.LightYellow;
            pnlLimitMaxX2.BackColor = NmcData.nPLimit[X2axis] == 1 ? Color.Red : Color.LightYellow;
            pnlLimitMaxY.BackColor = NmcData.nPLimit[Yaxis] == 1 ? Color.Red : Color.LightYellow;
            pnlLimitMaxZ.BackColor = NmcData.nPLimit[Zaxis] == 1 ? Color.Red : Color.LightYellow;

            pnlLimitMinX.BackColor = NmcData.nMLimit[Xaxis] == 1 ? Color.Red : Color.LightYellow;
            pnlLimitMinX2.BackColor = NmcData.nMLimit[X2axis] == 1 ? Color.Red : Color.LightYellow;
            pnlLimitMinY.BackColor = NmcData.nMLimit[Yaxis] == 1 ? Color.Red : Color.LightYellow;
            pnlLimitMinZ.BackColor = NmcData.nMLimit[Zaxis] == 1 ? Color.Red : Color.LightYellow;

            Button[] posbtns = { btnX1pos, btnX2pos, btnYpos, btnZpos };

            for (int i = 0; i < 4; i++)
            {
                short ishome = NmcData.nNear[axiss[i]];
                short ismax = NmcData.nPLimit[axiss[i]];
                short ismin = NmcData.nMLimit[axiss[i]];
                if (ishome + ismax + ismin > 1)
                {
                    if (posbtns[i].BackColor == Color.OrangeRed)
                    {
                        posbtns[i].BackColor = SystemColors.Info;
                    }
                    else
                    {
                        posbtns[i].BackColor = Color.OrangeRed;
                    }
                }
                else
                {

                    if (ishome == 1)
                    {
                        posbtns[i].BackColor = Color.LightGreen;
                        posbtns[i].Text = "Home";
                    }
                    else if (ismax == 1)
                    {
                        posbtns[i].BackColor = Color.Red;
                        posbtns[i].Text = "Max";
                    }
                    else if (ismin == 1)
                    {
                        posbtns[i].BackColor = Color.Red;
                        posbtns[i].Text = "min";
                    }
                    else
                    {
                        posbtns[i].BackColor = SystemColors.Info;
                        posbtns[i].Text = "";
                    }
                }
            }
            showmachine(NmcData.dEnc[Xaxis], NmcData.dEnc[X2axis], NmcData.dEnc[Yaxis], NmcData.dEnc[Zaxis]);

            showstep(NmcData2.nBusy[0], NmcData2.nBusy[1]);
            /////////////////// IO part 
            ///
        }

        private void btnServoONX_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(Xaxis, ON);
            pnlServoONX.BackColor = Color.LimeGreen;
            Logger.WriteButtonLog("X axis Servo ON", logButtonPath);
        }

        private void btnServoOFFX_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(Xaxis, OFF);
            pnlServoONX.BackColor = Color.White;
            Logger.WriteButtonLog("X axis Servo OFF", logButtonPath);
        }
        private void btnServoONX2_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(X2axis, ON);
            pnlServoONX2.BackColor = Color.LimeGreen;
            Logger.WriteButtonLog("X2 axis Servo ON", logButtonPath);
        }

        private void btnServoOFFX2_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(X2axis, OFF);
            pnlServoONX2.BackColor = Color.White;
            Logger.WriteButtonLog("X2 axis Servo OFF", logButtonPath);
        }

        private void btnServoONY_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(Yaxis, ON);
            pnlServoONY.BackColor = Color.LimeGreen;
            Logger.WriteButtonLog("Y axis Servo ON", logButtonPath);
        }

        private void btnServoOFFY_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(Yaxis, OFF);
            pnlServoONY.BackColor = Color.White;
            Logger.WriteButtonLog("Y axis Servo OFF", logButtonPath);
        }
        private void btnServoONZ_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(Zaxis, ON);
            pnlServoONZ.BackColor = Color.LimeGreen;
            Logger.WriteButtonLog("Z axis Servo ON", logButtonPath);
        }

        private void btnServoOFFZ_Click(object sender, EventArgs e)
        {
            PaixMotion.SetServoOnOff(Zaxis, OFF);
            pnlServoONZ.BackColor = Color.White;
            Logger.WriteButtonLog("Z axis Servo OFF", logButtonPath);
        }

        private void btnMotionHomeX_Click(object sender, EventArgs e)
        {
            short HomeStatus = NmcData.nNear[Xaxis];
            if (HomeStatus == ON) return;
            else if (HomeStatus == OFF) PaixMotion.HomeMove(Xaxis, HomeMode);
            Logger.WriteButtonLog("X axis Homing", logButtonPath);
        }
        private void btnMotionHomeX2_Click(object sender, EventArgs e)
        {
            short HomeStatus = NmcData.nNear[X2axis];
            if (HomeStatus == ON) return;
            else if (HomeStatus == OFF) PaixMotion.HomeMove(X2axis, HomeMode);
            Logger.WriteButtonLog("X2 axis Homing", logButtonPath);
        }

        private void btnMotionHomeY_Click(object sender, EventArgs e)
        {
            short HomeStatus = NmcData.nNear[Yaxis];
            if (HomeStatus == ON) return;
            else if (HomeStatus == OFF) PaixMotion.HomeMove(Yaxis, HomeMode);
            Logger.WriteButtonLog("Y axis Homing", logButtonPath);
        }
        private void btnMotionHomeZ_Click(object sender, EventArgs e)
        {
            short HomeStatus = NmcData.nNear[Zaxis];
            if (HomeStatus == ON) return;
            else if (HomeStatus == OFF) PaixMotion.HomeMove(Zaxis, HomeMode);
            Logger.WriteButtonLog("Z axis Homing", logButtonPath);
        }

        private void btnMotionStopX_Click(object sender, EventArgs e)
        {
            PaixMotion.Stop(Xaxis);
            Logger.WriteButtonLog("X axis Stopped", logButtonPath);
        }
        private void btnMotionStopX2_Click(object sender, EventArgs e)
        {
            PaixMotion.Stop(X2axis);
            Logger.WriteButtonLog("X2 axis Stopped", logButtonPath);
        }

        private void btnMotionStopY_Click(object sender, EventArgs e)
        {
            PaixMotion.Stop(Yaxis);
            Logger.WriteButtonLog("Y axis Stopped", logButtonPath);
        }
        private void btnMotionStopZ_Click(object sender, EventArgs e)
        {
            PaixMotion.Stop(Zaxis);
            Logger.WriteButtonLog("Z axis Stopped", logButtonPath);
        }

        private void btnMotionJogXCCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(Xaxis, CCW);
        }

        private void btnMotionJogXCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(Xaxis);
            Logger.WriteButtonLog("X axis Jog CW", logButtonPath);
        }
        private void btnMotionJogXCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(Xaxis, CW);
        }
        private void btnMotionJogXCCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(Xaxis);
            Logger.WriteButtonLog("X axis Jog CCW", logButtonPath);
        }
        private void btnMotionJogX2CW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(X2axis, CW);
        }

        private void btnMotionJogX2CW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(X2axis);
            Logger.WriteButtonLog("X2 axis Jog CW", logButtonPath);
        }
        private void btnMotionJogX2CCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(X2axis, CCW);
        }
        private void btnMotionJogX2CCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(X2axis);
            Logger.WriteButtonLog("X2 axis Jog CCW", logButtonPath);
        }

        private void btnMotionJogYCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(Yaxis, CW);
        }
        private void btnMotionJogYCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(Yaxis);
            Logger.WriteButtonLog("Y axis Jog CW", logButtonPath);
        }

        private void btnMotionJogYCCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(Yaxis, CCW);
        }

        private void btnMotionJogYCCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(Yaxis);
            Logger.WriteButtonLog("Y axis Jog CCW", logButtonPath);
        }
        private void btnMotionJogZCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(Zaxis, CW);
        }
        private void btnMotionJogZCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(Zaxis);
            Logger.WriteButtonLog("Z axis Jog CW", logButtonPath);
        }

        private void btnMotionJogZCCW_LeftMouseDown(object sender, MouseEventArgs e)
        {
            PaixMotion.JogMove(Zaxis, CCW);
        }

        private void btnMotionJogZCCW_LeftMouseUp(object sender, MouseEventArgs e)
        {
            PaixMotion.Stop(Zaxis);
            Logger.WriteButtonLog("Z axis Jog CCW", logButtonPath);
        }

        private void btnMotionAbsPosX_Click(object sender, EventArgs e)
        {
            if (txtMotionAbsPosX.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionAbsPosX.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionAbsPosX.Text);

                if (Pos > xPosMaxLimit)
                {
                    MessageBox.Show("최대위치는 500입니다.");
                    txtMotionAbsPosX.Text = "0";
                }

                else
                {
                    PaixMotion.AbsMove(Xaxis, Convert.ToDouble(txtMotionAbsPosX.Text));
                }
                Logger.WriteButtonLog($"X axis Absolute move to {Pos} ", logButtonPath);
            }

        }

        private void btnMotionAbsPosX2_Click(object sender, EventArgs e)
        {
            if (txtMotionAbsPosX2.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionAbsPosX2.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionAbsPosX2.Text);

                if (Pos > x2PosMaxLimit)
                {
                    MessageBox.Show("최대위치는 500입니다.");
                    txtMotionAbsPosX2.Text = "0";
                }

                else
                {
                    PaixMotion.AbsMove(X2axis, Convert.ToDouble(txtMotionAbsPosX2.Text));
                }
                Logger.WriteButtonLog($"X2 axis Absolute move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionAbsPosY_Click(object sender, EventArgs e)
        {
            if (txtMotionAbsPosY.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionAbsPosY.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionAbsPosY.Text);

                if (Pos > yPosMaxLimit)
                {
                    MessageBox.Show("최대위치는 300입니다.");
                    txtMotionAbsPosY.Text = "0";
                }

                else
                {
                    PaixMotion.AbsMove(Yaxis, Convert.ToDouble(txtMotionAbsPosY.Text));
                }
                Logger.WriteButtonLog($"Y axis Absolute move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionAbsPosZ_Click(object sender, EventArgs e)
        {
            if (txtMotionAbsPosZ.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionAbsPosZ.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionAbsPosZ.Text);

                if (Pos > zPosMaxLimit)
                {
                    MessageBox.Show("최대위치는 90입니다.");
                    txtMotionAbsPosZ.Text = "0";
                }

                else
                {
                    PaixMotion.AbsMove(Zaxis, Convert.ToDouble(txtMotionAbsPosZ.Text));
                }
                Logger.WriteButtonLog($"Z axis Absolute move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionRelPosXP_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosX.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosX.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosX.Text);

                if (Pos > xPosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 500 입니다.");
                    txtMotionRelPosX.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(Xaxis, Convert.ToDouble(txtMotionRelPosX.Text));
                }
                Logger.WriteButtonLog($"X axis Plus Realtive move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionRelPosXM_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosX.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosX.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosX.Text);

                if (Pos > xPosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 500 입니다.");
                    txtMotionRelPosX.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(Xaxis, -Convert.ToDouble(txtMotionRelPosX.Text));
                }
                Logger.WriteButtonLog($"X axis Minus Realtive move to {Pos} ", logButtonPath);
            }
        }
        private void btnMotionRelPosX2P_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosX2.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosX2.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosX2.Text);

                if (Pos > x2PosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 500 입니다.");
                    txtMotionRelPosX2.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(X2axis, Convert.ToDouble(txtMotionRelPosX2.Text));
                }
                Logger.WriteButtonLog($"X2 axis Plus Realtive move to {Pos} ", logButtonPath);
            }
        }
        private void btnMotionRelPosX2M_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosX2.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosX2.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosX2.Text);

                if (Pos > x2PosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 500 입니다.");
                    txtMotionRelPosX2.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(X2axis, -Convert.ToDouble(txtMotionRelPosX2.Text));
                }
                Logger.WriteButtonLog($"X2 axis Minus Realtive move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionRelPosYP_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosY.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosY.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosY.Text);

                if (Pos > yPosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 500 입니다.");
                    txtMotionRelPosY.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(Yaxis, Convert.ToDouble(txtMotionRelPosY.Text));
                }
                Logger.WriteButtonLog($"Y axis Plus Realtive move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionRelPosYM_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosY.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosY.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosY.Text);

                if (Pos > yPosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 500 입니다.");
                    txtMotionRelPosY.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(Yaxis, -Convert.ToDouble(txtMotionRelPosY.Text));
                }
                Logger.WriteButtonLog($"Y axis Minus Realtive move to {Pos} ", logButtonPath);
            }
        }
        private void btnMotionRelPosZP_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosZ.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosZ.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosZ.Text);

                if (Pos > zPosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 80 입니다.");
                    txtMotionRelPosZ.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(Zaxis, Convert.ToDouble(txtMotionRelPosZ.Text));
                }
                Logger.WriteButtonLog($"Z axis Plus Realtive move to {Pos} ", logButtonPath);
            }
        }

        private void btnMotionRelPosZM_Click(object sender, EventArgs e)
        {
            if (txtMotionRelPosZ.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionRelPosZ.Text = "0";
                return;
            }
            else
            {
                double Pos = Convert.ToDouble(txtMotionRelPosZ.Text);

                if (Pos > zPosMaxLimit)
                {
                    MessageBox.Show("최대 상대 이동량은 80 입니다.");
                    txtMotionRelPosZ.Text = "0";
                }

                else
                {
                    PaixMotion.RelMove(Zaxis, -Convert.ToDouble(txtMotionRelPosZ.Text));
                }
                Logger.WriteButtonLog($"Z axis Minus Realtive move to {Pos} ", logButtonPath);
            }
        }

        private void txtIPRead_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtMotionAbsPosX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionAbsPosX2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtMotionAbsPosY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionAbsPosZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtMotionRelPosX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionRelPosX2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtMotionRelPosY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionRelPosZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionSpeedX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionSpeedX2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionSpeedY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void txtMotionSpeedZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void btnMotionSetSpeedX_Click(object sender, EventArgs e)
        {
            short Speed = 0;
            if (txtMotionSpeedX.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionSpeedX.Text = "0";
                Speed = 0;
                return;
            }
            else Speed = Convert.ToInt16(txtMotionSpeedX.Text);

            if (Speed > SpeedLimit)
            {
                MessageBox.Show("속도제한 100 mm/s\n속도를 100mm/s 이하로 입력해주세요.");
                txtMotionSpeedX.Text = "100";
            }
            else if (Speed <= SpeedLimit)
            {
                PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Convert.ToDouble(txtMotionSpeedX.Text)*2, Convert.ToDouble(txtMotionSpeedX.Text)*2, Convert.ToDouble(txtMotionSpeedX.Text));
            }
            Logger.WriteButtonLog($"X axis Speed Set to {Speed} ", logButtonPath);
        }
        private void btnMotionSetSpeedX2_Click(object sender, EventArgs e)
        {
            short Speed = 0;
            if (txtMotionSpeedX2.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionSpeedX2.Text = "0";
                Speed = 0;
                return;
            }
            else Speed = Convert.ToInt16(txtMotionSpeedX2.Text);

            if (Speed > SpeedLimit)
            {
                MessageBox.Show("속도제한 100 mm/s\n속도를 100mm/s 이하로 입력해주세요.");
                txtMotionSpeedX2.Text = "100";
            }
            else if (Speed <= SpeedLimit)
            {
                PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, Convert.ToDouble(txtMotionSpeedX2.Text), Convert.ToDouble(txtMotionSpeedX2.Text), Convert.ToDouble(txtMotionSpeedX2.Text));
            }
            Logger.WriteButtonLog($"X2 axis Speed Set to {Speed} ", logButtonPath);
        }
        private void btnMotionSetSpeedY_Click(object sender, EventArgs e)
        {
            short Speed = 0;
            if (txtMotionSpeedY.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionSpeedY.Text = "0";
                Speed = 0;
                return;
            }
            else Speed = Convert.ToInt16(txtMotionSpeedY.Text);

            if (Speed > SpeedLimit)
            {
                MessageBox.Show("속도제한 100 mm/s\n속도를 100mm/s 이하로 입력해주세요.");
                txtMotionSpeedY.Text = "100";
            }
            else if (Speed <= SpeedLimit)
            {
                PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, Convert.ToDouble(txtMotionSpeedY.Text));
            }
            Logger.WriteButtonLog($"Y axis Speed Set to {Speed} ", logButtonPath);
        }
        private void btnMotionSetSpeedZ_Click(object sender, EventArgs e)
        {
            short Speed = 0;
            if (txtMotionSpeedZ.Text == "")
            {
                MessageBox.Show("공백을 두지 마세요.");
                txtMotionSpeedZ.Text = "0";
                Speed = 0;
                return;
            }
            else Speed = Convert.ToInt16(txtMotionSpeedZ.Text);

            if (Speed > SpeedLimit)
            {
                MessageBox.Show("속도제한 100 mm/s\n속도를 100mm/s 이하로 입력해주세요.");
                txtMotionSpeedZ.Text = "100";
            }
            else if (Speed <= SpeedLimit)
            {
                PaixMotion.SetSCurveSpeed(Zaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, Convert.ToDouble(txtMotionSpeedZ.Text));
            }
            Logger.WriteButtonLog($"Z axis Speed Set to {Speed} ", logButtonPath);
        }

        private void txtMotionSpeedX_TextChanged(object sender, EventArgs e)
        {
            txtMotionSpeedX.Text = txtMotionSpeedX.Text.TrimStart('0');
        }

        private void txtMotionSpeedX2_TextChanged(object sender, EventArgs e)
        {
            txtMotionSpeedX2.Text = txtMotionSpeedX2.Text.TrimStart('0');
        }

        private void txtMotionSpeedY_TextChanged(object sender, EventArgs e)
        {
            txtMotionSpeedY.Text = txtMotionSpeedY.Text.TrimStart('0');
        }

        private void txtMotionSpeedZ_TextChanged(object sender, EventArgs e)
        {
            txtMotionSpeedZ.Text = txtMotionSpeedZ.Text.TrimStart('0');
        }

        private void btnMotionAlarmResetX_Click(object sender, EventArgs e)
        {
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, Xaxis, OFF);
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, X2axis, OFF);
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, Yaxis, OFF);
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, Zaxis, OFF);
        }

        private void btnMotionAlarmResetX2_Click(object sender, EventArgs e)
        {
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, X2axis, OFF);
        }

        private void btnMotionAlarmResetY_Click(object sender, EventArgs e)
        {
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, Yaxis, OFF);
        }

        private void btnMotionAlarmResetZ_Click(object sender, EventArgs e)
        {
            Paix_MotionController.NMC2.nmc_SetAlarmResetOn(devID, Zaxis, OFF);
        }


        private void txtMotionAbsPosX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnMotionAbsPosX_Click(sender, e);
            }
        }

        private void txtMotionAbsPosX2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnMotionAbsPosX2_Click(sender, e);
            }
        }

        private void txtMotionAbsPosY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnMotionAbsPosY_Click(sender, e);
            }
        }

        private void txtMotionAbsPosZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnMotionAbsPosZ_Click(sender, e);
            }
        }
        /// <summary>
        /// IO part 
        /// </summary>
        /// 
        /// 
        private void btnIO_Out0_Click(object sender, EventArgs e)
        {
            if (ndevOpen == false) return;
            Button btn = (Button)sender;
            short bitno = Convert.ToInt16(btn.Text);
            //IR 또는 호퍼 동작시에만 로그 작성되도록 함 (위험 방지용)
            if (bitno == 0)
            {
                string processstring = Convert.ToString(DateTime.Now) + "___IR 동작";
                processbox.Items.Insert(0, processstring);
                PaixMotion.setoutpintime(bitno, 180);
            }
            else if (bitno == 4)
            {
                string processstring = Convert.ToString(DateTime.Now) + "___Hopper 동작";
                processbox.Items.Insert(0, processstring);
            }
            Paix_MotionController.NMC2.nmc_SetMDIOOutputTog(devID, bitno);

        }


        private void btnShutter_Click(object sender, EventArgs e)
        {
            //호퍼 수동 동작 함수
            if (NmcData.dEnc[X2axis] > 2)//리코터의 위치가 정상 상태인지 파악하여 그렇지 않을 경우 셔터 동작 전 확인 진행
            {
                if (MessageBox.Show("리코터가 공급 위치에 있지 않은것으로 감지됩니다. 계속하시겠습니까?", "리코터 위치 이상 감지", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string Aprocessstring = Convert.ToString(DateTime.Now) + "___공급위치 이외에서 호퍼가 작동됨 (현재위치 : " + NmcData.dEnc[X2axis] + ")";
                    processbox.Items.Insert(0, Aprocessstring);
                }
                else
                {
                    return;
                }
            }
            string processstring = Convert.ToString(DateTime.Now) + "___Hopper 동작";
            processbox.Items.Insert(0, processstring);
            //호퍼 인터벌이 0일 경우 다시 이볅하는 확인창
            if (Convert.ToInt32(txtShutterOpenTime.Text) == 0)
            {
                MessageBox.Show("다시 입력하세요.");
                return;
            }
            //실질 셔터 동작 함수
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 3, 1);  //3번 ON 7번 OFF 가 개방
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 7, 0);  
            Thread.Sleep(Convert.ToInt32(txtShutterOpenTime.Text));//해당 시간동안 동작 (공압에 따라 다름)
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 3, 0); //3번 OFF 7번 ON이 폐쇄
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 7, 1);
            Logger.WriteButtonLog("Shutter Opend", logButtonPath);
        }


        /// <summary>
        /// 리코터 STEP 모터 회전 코드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        /// <summary>
        /// 여기는 BLDC 제어 파트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        private async void btn485OPEN()
        {
            try
            {
                if (!Serial_RS485.IsOpen)
                {
                    Serial_RS485.PortName = "COM3";
                    Serial_RS485.BaudRate = 9600;
                    Serial_RS485.DataBits = 8;
                    Serial_RS485.StopBits = StopBits.One;
                    Serial_RS485.Parity = Parity.None;
                    Serial_RS485.Open();
                    pnl485connection.BackColor = Color.LimeGreen;
                    Serial_RS485.DataReceived += new SerialDataReceivedEventHandler(Serial_RS485_DataReceived);
                    Logger.WriteButtonLog("RS485 communication connecting", logButtonPath);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(Convert.ToString(ex));
            }
        }


        private void Serial485_Send(byte[] SendCommand_Packet, int len)
        {
            try
            {
                if (Serial_RS485.IsOpen)
                {
                    //Console.WriteLine("IsOpen");
                    Serial_RS485.Write(SendCommand_Packet, 0, len);
                }
                else
                {
                    Serial_RS485.Close();
                    Thread.Sleep(100);
                    Serial_RS485.Open();
                    Serial_RS485.Write(SendCommand_Packet, 0, len);
                    //Console.WriteLine("IsOpen Else");
                }
                Logger.WriteButtonLog($"RS485 Data: {string.Join(" ", SendCommand_Packet)}", logButtonPath);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        private void Serial485_Read(object s, EventArgs e)
        {
            try
            {
                if (Serial_RS485.IsOpen)
                {
                    arrSerialbuff.Clear();
                    int bytes = Serial_RS485.BytesToRead;
                    byte[] buffBytes = new byte[bytes];

                    if (bytes > 0)
                    {
                        Serial_RS485.Read(buffBytes, 0, bytes);
                        for (int i = 0; i < bytes; i++)
                        {
                            arrSerialbuff.Add(buffBytes[i]);
                        }
                    }

                    if (bytes >= 2)
                    {
                        temp = (((256 * Convert.ToInt16(arrSerialbuff[0])) + Convert.ToInt16(arrSerialbuff[1])) - 1000) / 10;
                        rtx485Result.Text = Convert.ToString(temp);
                    }

                }
                arrSerialbuff.Clear();

            }
            catch (ArgumentOutOfRangeException ex)
            {
                arrSerialbuff.Clear();
                throw ex;
            }
        }
        private void TempRead()
        {
            temp = 500;//Temp 초기화를 500도로 진행함 (유효성 검증을 위함)
            while ((10 > temp) || (temp > 150))//실제 가능한 온도 범위를 초과할 경우 유효한 입력값으로 간주하지 않음
            {
                byte[] sendByte = { 0xFF, 0x01, 0x01, 0xFF };//현재 온도 송신을 요청하는 코드
                Serial485_Send(sendByte, sendByte.Length);//요청 전송
                try
                {
                    if (Serial_RS485.IsOpen)
                    {
                        int bytes = Serial_RS485.BytesToRead;
                        byte[] buffBytes = new byte[bytes];
                        if (bytes > 0)
                        {
                            Serial_RS485.Read(buffBytes, 0, bytes);
                            for (int i = 0; i < bytes; i++)
                            {
                                arrSerialbuff.Add(buffBytes[i]);
                            }
                        }

                        if (bytes > 4) //정상 수신시 온도 번역 동작 수행 (오류 가능성 있으나 While 문 조건에서 걸러짐)
                        {
                            temp = (((256 * Convert.ToDouble(arrSerialbuff[2])) + Convert.ToDouble(arrSerialbuff[3])) - 1000) / 10;
                            rtx485Result.Text = Convert.ToString(temp);
                        }

                    }
                    arrSerialbuff.Clear();

                }
                catch (ArgumentOutOfRangeException ex)
                {
                    arrSerialbuff.Clear();
                    throw ex;
                }
                Thread.Sleep(500);
                //Console.WriteLine("PROCESSING...");

            }
            //Console.WriteLine(Convert.ToString(temp));
            rtx485Result.Text = Convert.ToString(temp);
            if (temp > 140)//온도가 비정상적으로 높을 경우 동작
            {
                PrintEnable = false;
                PaixMotion.EmergencyStop();
                //BLDC_STOP_0();
                //BLDC_STOP_1();
                //BLDC_STOP_2();
                Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 0, 0);
            }
        }


        // Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, Raxis, rStartSpeed, rAccelSpeed, rDeccelSpeed, Convert.ToDouble(txtMotionSpeedR.Text));
        //Paix_MotionController.NMC2.nmc_JogMove(devID2, Raxis, CW);


        private void btnPrint_Click(object sender, EventArgs e)
        {
            log_printingsettings();
            Print();

        }

        private void ShutterOpen(int millis)
        {
            //호퍼 동작 (자동 시퀸스 포함일 경우)
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 3, 1);  //3번 ON 7번 OFF 가 개방
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 7, 0);  //3번 OFF 7번 ON이 폐쇄
            Thread.Sleep(Convert.ToInt32(millis));
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 3, 0);
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 7, 1);
        }

        private void BLDC_RUN_0()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x00, 0x03, 0xF0, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                //0xFF, 0xFE, 0x00, 0x06, 0x96, 0x03, 0x01, 0x03, 0x52, 0x0A  //0번 모터 CW 방향 회전 현재 85RPM
                //0xFF, 0xFE, 0x00, 0x06, 0x00, 0x03, 0x01, 0x01, 0xE0, 0x14  //0번 모터 CW 방향 회전 현재 48RPM
                //0xFF, 0xFE, 0x00, 0x06, 0xF1, 0x03, 0x01, 0x00, 0xF0, 0x14  //0번 모터 CW 방향 회전 현재 24RPM-2
                0xFF, 0xFE, 0x00, 0x06, 0xDD, 0x03, 0x01, 0x00, 0xF0, 0x28  //0번 모터 CW 방향 회전 현재 24RPM-4
                //0xFF, 0xFE, 0x00, 0x06, 0x1E, 0x03, 0x01, 0x03, 0xC0, 0x14  //0번 모터 CW 방향 회전 현재 96RPM-2

            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void BLDC_RUN_0_REV()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x00, 0x03, 0xF0, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                //0xFF, 0xFE, 0x00, 0x06, 0x96, 0x03, 0x01, 0x03, 0x52, 0x0A  //0번 모터 CW 방향 회전 현재 85RPM
                //0xFF, 0xFE, 0x00, 0x06, 0x00, 0x03, 0x01, 0x01, 0xE0, 0x14  //0번 모터 CW 방향 회전 현재 48RPM
                0xFF, 0xFE, 0x00, 0x06, 0x01, 0x03, 0x00, 0x01, 0xE0, 0x14  //0번 모터 CCW 방향 회전 현재 48RPM
            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void BLDC_RUN_1() //롤러 모터
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x01, 0x03, 0xEF, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                0xFF, 0xFE, 0x01, 0x06, 0x45, 0x03, 0x01, 0x13, 0x88, 0x14  //1번 모터 CW 방향 회전 현재 50RPM
            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void BLDC_RUN_1_rev(byte[] bldcbytecode) //롤러 모터-역방향
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x01, 0x03, 0xEF, 0x0C, 0x00
            };
            byte[] Packet_Run = bldcbytecode;
            Console.WriteLine(String.Join(", ", Packet_Run));
            showpnlroller.BackColor = Color.DodgerBlue;
            /*{
                //0xFF, 0xFE, 0x01, 0x06, 0x29, 0x03, 0x00, 0x38, 0x80, 0x14//1번 모터 CCW방향 회전 8000RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x97, 0x03, 0x00, 0xEA, 0x60, 0x14//1번 모터 CCW방향 회전 6000RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x05, 0x03, 0x00, 0x9C, 0x40, 0x14//1번 모터 CCW방향 회전 4000RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x73, 0x03, 0x00, 0x4E, 0x20, 0x14//1번 모터 CCW방향 회전 2000RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0xAA, 0x03, 0x00, 0x27, 0x10, 0x14//1번 모터 CCW방향 회전 1000RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x1E, 0x03, 0x00, 0x0B, 0xB8, 0x14//1번 모터 CCW방향 회전 300RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0xF6, 0x03, 0x00, 0x03, 0xE8, 0x14//1번 모터 CCW방향 회전 100RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0xAF, 0x03, 0x00, 0x00, 0x32, 0x14//1번 모터 CCW방향 회전 5RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x19, 0x03, 0x00, 0x00, 0xC8, 0x14//1번 모터 CCW방향 회전 20RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0xEC, 0x03, 0x00, 0x01, 0xF4, 0x14//1번 모터 CCW방향 회전 50RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x82, 0x03, 0x00, 0x01, 0x5E, 0x14//1번 모터 CCW방향 회전 35RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0xBE, 0x03, 0x00, 0x03, 0x20, 0x14//1번 모터 CCW방향 회전 80RPM(기어비 1)


      
                //0xFF, 0xFE, 0x01, 0x06, 0x0A, 0x03, 0x00, 0x07, 0xD0, 0x14  //1번 모터 CCW 방향 회전 현재 200RPM
                //0xFF, 0xFE, 0x01, 0x06, 0xE2, 0x03, 0x00, 0x03, 0xE8, 0x28
                //0xFF, 0xFE, 0x01, 0x06, 0x09, 0x03, 0x01, 0x07, 0xD0, 0x14  //1번 모터 CW 방향 회전 현재 200RPM
                //0xFF, 0xFE, 0x01, 0x06, 0xE0, 0x03, 0x01, 0x00, 0x00, 0x14  //1번 모터 CW 방향 회전 현재 0RPM

            };
            */
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void maxon_run(int rpm,bool direction)
        {
            //맥슨모터 동작 (rpm/방향)
            MaxBLDC.MotorMove_CW_CCW(rpm, 10000, 10000, direction);
        }

        private void maxon_stop()
        {
            //맥슨모터 정지
            MaxBLDC.MotorStop();
        }

        private void BLDC_RUN_1_throw()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x01, 0x03, 0xEF, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                0xFF, 0xFE, 0x01, 0x06, 0x29, 0x03, 0x01, 0x38, 0x80, 0x14//1번 모터 CW방향 회전 8000RPM(기어비 1)
                //0xFF, 0xFE, 0x01, 0x06, 0x09, 0x03, 0x01, 0x07, 0xD0, 0x14 //1번 모터 CCW 방향 회전 현재 200RPM
            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void BLDC_RUN_1_stop() //롤러 모터-stop
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x01, 0x03, 0xEF, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                0xFF, 0xFE, 0x01, 0x06, 0xE1, 0x03, 0x00, 0x00, 0x00, 0x14 //stop

            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
            
        }

        private void BLDC_RUN_2()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x02, 0x03, 0xEE, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                //0xFF, 0xFE, 0x02, 0x06, 0xAD, 0x03, 0x01, 0x02, 0x30, 0x14  //2번 모터 CW 방향 회전 현재 56RPM
                //0xFF, 0xFE, 0x02, 0x06, 0xFE, 0x03, 0x01, 0x03, 0xE8, 0x0A  //2번 모터 CW 방향 회전 현재 50RPM
                //0xFF, 0xFE, 0x02, 0x06, 0xC6, 0x03, 0x01, 0x01, 0x18, 0x14  //2번 모터 CW 방향 회전 현재 28RPM-2
                0xFF, 0xFE, 0x02, 0x06, 0xB2, 0x03, 0x01, 0x01, 0x18, 0x28  //2번 모터 CW 방향 회전 현재 28RPM-4
                //0xFF, 0xFE, 0x02, 0x06, 0x7B, 0x03, 0x01, 0x04, 0x60, 0x14  //2번 모터 CW 방향 회전 현재 112RPM-2


            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void BLDC_RUN_2_REV()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_ON =
            {
                0xFF, 0xFE, 0x02, 0x03, 0xEE, 0x0C, 0x00
            };
            byte[] Packet_Run =
            {
                //0xFF, 0xFE, 0x02, 0x06, 0xFE, 0x03, 0x01, 0x03, 0xE8, 0x0A  //2번 모터 CW 방향 회전 현재 50RPM
                //0xFF, 0xFE, 0x02, 0x06, 0xAD, 0x03, 0x01, 0x02, 0x30, 0x14  //2번 모터 CW 방향 회전 현재 54RPM
                //0xFF, 0xFE, 0x02, 0x06, 0xC2, 0x03, 0x00, 0x02, 0x1C, 0x14  //2번 모터 CCW 방향 회전 현재 28RPM
                0xFF, 0xFE, 0x02, 0x06, 0xD1, 0x03, 0x00, 0x01, 0x18, 0x0A //2번 모터 CCW방향 28 -1
 


            };
            Serial485_Send(Packet_ON, Packet_ON.Length);
            Serial485_Send(Packet_Run, Packet_Run.Length);
        }

        private void BLDC_STOP_0()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_OFF =
            {
                0xFF, 0xFE, 0x00, 0x03, 0xEF, 0x0C, 0x01
            };
            byte[] Packet_Stop =
            {
                0xFF, 0xFE, 0x00, 0x06, 0xEB, 0x03, 0x00, 0x00, 0x00, 0x0A
            };
            Serial485_Send(Packet_Stop, Packet_Stop.Length);
            Serial485_Send(Packet_OFF, Packet_OFF.Length);
        }

        private void BLDC_STOP_1()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_OFF =
            {
                0xFF, 0xFE, 0x01, 0x03, 0xEE, 0x0C, 0x01
            };
            byte[] Packet_Stop =
            {
                0xFF, 0xFE, 0x01, 0x06, 0xEA, 0x03, 0x01, 0x00, 0x00, 0x0A
            };
            showpnlroller.BackColor = Color.SkyBlue;
            Serial485_Send(Packet_Stop, Packet_Stop.Length);
            Serial485_Send(Packet_OFF, Packet_OFF.Length);
        }
        private void BLDC_STOP_2()
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            byte[] Packet_OFF =
            {
                0xFF, 0xFE, 0x02, 0x03, 0xED, 0x0C, 0x01
            };
            byte[] Packet_Stop =
            {
                0xFF, 0xFE, 0x02, 0x06, 0xE9, 0x03, 0x01, 0x00, 0x00, 0x0A
            };
            Serial485_Send(Packet_Stop, Packet_Stop.Length);
            Serial485_Send(Packet_OFF, Packet_OFF.Length);
        }

        private void btnBLDCRun_Click(object sender, EventArgs e)
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            //BLDC_RUN_0();
            //BLDC_RUN_1_rev();
            //BLDC_RUN_2();
        }

        private void btnBLDCStop_Click(object sender, EventArgs e)
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            //BLDC_STOP_0();
            //BLDC_STOP_1();
            //BLDC_STOP_2();
        }

        private void btnPrintheadConnect_Click(object sender, EventArgs e)
        {
            Printhead.Connect();
            Printhead.config();
            Printhead.SetPaletteTable(new byte[] { 4, 3, 3, 2, 2, 2, 0, 0 });
            pnlheadcon.BackColor = Color.LimeGreen;
        }

        private void btnPrintImageOpen_Click(object sender, EventArgs e)
        {

            string OpenFilePath = System.Environment.CurrentDirectory;

            ImageFileDialog.InitialDirectory = OpenFilePath;
            ImageFileDialog.RestoreDirectory = true;

            ImageFileDialog.Title = "이미지 선택";
            ImageFileDialog.Filter = "png files (*.png)|*.png";
            ImageFileDialog.FileName = "";
            int a;

            DialogResult dr = ImageFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (ImageFileDialog.FileNames.Length > 0)
                {
                    listImage.Items.Clear();
                }
                foreach (string filepath in ImageFileDialog.FileNames)
                {
                    //list_file.Add(filepath.ToString());
                    string filepath_cut;
                    Console.WriteLine(filepath);
                    //a = filepath.IndexOf("Images");
                    //Console.WriteLine(a);
                    //filepath_cut = filepath.Substring(a);
                    //filepath_cut = filepath_cut.Substring(7);
                    //listImage.Items.Add(filepath_cut);
                    listImage.Items.Add(filepath);
                }
            }

            txtTotalLayer.Text = listImage.Items.Count.ToString();
            if (listImage.Items.Count > 0)
            {
                imgtrackbar.Minimum = 0;
                imgtrackbar.Maximum = listImage.Items.Count - 1;
                selectedimgshow(Convert.ToInt16(imgtrackbar.Value));
            }
            
        }

        private async void btnTestWhile_Click(object sender, EventArgs e)
        {
            //이전에 사용하였으나 현재 사용하지 않음.
            PrintEnable = true;
            //프린팅 시퀸스 추가 변수 선언부


            //빌드 관련 변수
            double Layer_thickness = Convert.ToDouble(txtlayerthickness.Text) / 1000;//레이어 두께
            byte[] Binder_Value = { Convert.ToByte(txtb6.Text), Convert.ToByte(txtb5.Text), Convert.ToByte(txtb4.Text), Convert.ToByte(txtb3.Text), Convert.ToByte(txtb2.Text), Convert.ToByte(txtb1.Text), 0, 0 };

            //리코팅 관련 변수
            double templimit = Convert.ToDouble(txttemplimit.Text);//온도기준값
            double temprange = Convert.ToDouble(txttemprange.Text);//온도여유값

            int Recoating_repeat = 0;//5;//리코팅 반복횟수
            double X2axis_nspeed = 25;//기본 리코팅 작업시 Feeder speed
            double X2axis_fspeed = 5;//마지막 리코팅 작업시 Feeder speed
            double X2axis_pspeed = Convert.ToDouble(txtprspeed.Text);//파우더 도포시 Feeder speed
            double X2axis_bspeed = 100;//X축 복귀 속도
            double step_rspeed = Convert.ToDouble(txtstepper);//파우더 공급시 스텝모터 회전속도

            //출력 관련 변수
            double Xaxis_pspeed = 200;//헤드 출력시 X축 speed
            double Xaxis_rspeed = 200;//헤드 복귀시 X축 speed
            double Yaxis_speed = 100; //Y축 이동 속도

            short Dwell_time = Convert.ToInt16(txtdwelltime);//프린팅 사이 대기 시간 (초)

            //위치 관련 변수

            //Head_X
            double Xaxis_ready_pos = 90;//헤드 대기 위치
            double Xaxis_head_max_pos = 840;//최대 출력 위치
            double Xaxis_Cleaning_pos = 0;



            //Head_Y
            //double Yaxis_ready_pos = 144;
            //double Yaxis_first_lane_pos = ;
            //double Yaxis_second_lane_pos = ;
            double Yaxis_single_lane_pos = 144;
            double Yaxis_Cleaning_start_pos = 245;
            double Yaxis_Cleaning_end_pos = 45;
            double Yaxis_Parking_pos = 40;
            double Yaxis_Cleaning_speed = 8;

            byte[] bldcbytecode = Bldc_byteget(Convert.ToInt32(txtroller.Text));//{ 0xFF, 0xFE, 0x01, 0x06, 0x1E, 0x03, 0x00, 0x0B, 0xB8, 0x14 };
            //레거시 코드이나 문제 있을수 있어 유지


            //프린팅 시퀀스 여기서부터 시작
            Printhead.SetPaletteTable(Binder_Value);
            short Printing_Sequence = pAfter_Powder;
            //Recoating_repeat = Recoating_repeat;//값보정(반복횟수) - 삭제(시퀸스 재정비로 인한 보정 삭제)
            string processstring = Convert.ToString(DateTime.Now) + "___테스트 출력 시작";
            processbox.Items.Insert(0, processstring);


            //변수설정부
            Currentlayer = 0;
            ImageNum = 0;
            //JobLayer = listImage.Items.Count;
            selectedimgshow(Convert.ToInt16(Currentlayer));

            //구동파라미터
            PaixMotion.SetSCurveSpeed(Zaxis, 1, 1, 1, 1);//Z축 이동 속도를 1mm/s로 고정 (z축 퉁퉁거리는거 방지, start/end speed도 target speed에 맞추는것이 필요)

            //PrintEnable = true;


            //축 대기위치 이동
            AxisMovement(X2axis, 0);
            AxisMovement(Xaxis, Xaxis_ready_pos);
            AxisMovement(Yaxis, Yaxis_single_lane_pos);

            //var hcc = Task.Run(() => Head_Cleaning(Xaxis_Cleaning_pos, Xaxis_ready_pos, Yaxis_Cleaning_start_pos, Yaxis_Cleaning_end_pos, Yaxis_single_lane_pos, Yaxis_Cleaning_speed, Yaxis_speed));
            var hcc = Task.Run(() => Head_cleaning2());
            while (Currentlayer <= 1 && PrintEnable == true)
            {
                if (Currentlayer == 1)
                {
                    await hcc;
                    PaixMotion.AbsMove(Yaxis, Yaxis_Parking_pos);
                    updateCmdEnc();
                    while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                    PaixMotion.AbsMove(Xaxis, Xaxis_Cleaning_pos);
                    updateCmdEnc();
                    while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                    PaixMotion.AbsMove(Yaxis, Yaxis_Parking_pos);
                    updateCmdEnc();
                    while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                    break;
                }
                switch (Printing_Sequence)
                {

                    case pAfter_Powder:
                        Printhead.ImageConProc(listImage.Items[0].ToString());
                        //Printhead.imageswathedown(ImageNum);
                        Thread.Sleep(100);
                        await hcc;
                        if (Printhead.imagewidth <= 957)
                        {
                            Printing_Sequence = pImageProcess;
                        }
                        else
                        {
                            Printing_Sequence = 9; // 220617 이미지 사이즈가 958 pixel 이상이면 2 Way로
                        }
                        break;
                    case pImageProcess:
                        if (PrintEnable == false) return;
                        //Printhead.ImageConProc(listImage.Items[ImageNum].ToString());
                        //Printhead.imageswathedown(ImageNum);
                        Printhead.Swathedown();
                        Thread.Sleep(100);
                        // 220607
                        //Printhead.creatprintsquence(ImageNum);
                        Printhead.creatprintbisquence(ImageNum);
                        ImageNum++;
                        Thread.Sleep(100);

                        // 220608 randomization 여백 방향에 맞춰 Y축 움직임
                        if (Printhead.dirrandnum == 1)
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos + 3.03);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }

                        Printing_Sequence = pPrint;
                        break;
                    case pPrint:
                        if (PrintEnable == false) return;
                        Printhead.printstart();
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_pspeed, Xaxis_pspeed, Xaxis_pspeed);//Xaxis 출력 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_head_max_pos);                                //프린트헤드 MAX까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                        // Printhead.printstop();
                        Thread.Sleep(100);

                        // 220608 randomization 돌아올 때 인쇄되는 이미지의 여백이 반대로 생성되어 Y축이 위와 반대로 움직임 
                        if (Printhead.dirrandnum != 1)
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos + 3.03);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }

                        Printing_Sequence = pPrinthead_MoveToMin;
                        break;
                    case pPrinthead_MoveToMin:
                        if (PrintEnable == false) return;
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_rspeed, Xaxis_rspeed, Xaxis_rspeed);//Xaxis 복귀 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_ready_pos);                                   //프린트헤드 MIN까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                        Printhead.printstop();
                        Printhead.swathesquencedel();

                        Currentlayer++;
                        txtNowLayer.Text = Currentlayer.ToString();
                        //Printhead.swathesquencedel();
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);//히터 ON (IO OUT 1번 상태를 1로 바꿈) - Shutter Send

                        Thread.Sleep(100);
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);//히터 ON (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

                        await Task.Run(() => Task.Delay(Dwell_time * 1000).Wait());

                        //파우더 공급

                        //await Powder_Supply(Hopper_Open_interval, X2axis_hopper_pos, X2axis_ready_pos);

                        Printing_Sequence = pZ_Down;


                        break;
                    case 9:
                        if (PrintEnable == false) return;
                        Printhead.imageswathedown(ImageNum);
                        Printhead.creatprintbisquence(ImageNum);
                        ImageNum++;
                        await Task.Run(() => Task.Delay(100).Wait());
                        // 220617 2Way 인쇄를 위해 Y축 2Way 시작위치로 이동
                        // 220608 randomization 여백 방향에 맞춰 Y축 움직임
                        if (Printhead.dirrandnum == 1)
                        {
                            AxisMovement(Yaxis, 108.75);                                //Y축 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, 108.75 + 1.55);                                //Y축 이동
                        }

                        Printing_Sequence = 10;
                        break;
                    case 10:
                        if (PrintEnable == false) return;
                        Printhead.printstart();
                        await Task.Run(() => Task.Delay(1000).Wait());
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_pspeed, Xaxis_pspeed, Xaxis_pspeed);//Xaxis 출력 스피드로 변경
                        AxisMovement(Xaxis, Xaxis_head_max_pos);                                //프린트헤드 MAX까지 이동

                        //Printhead.printstop();
                        await Task.Run(() => Task.Delay(1000).Wait());
                        Printing_Sequence = 12;

                        /*
                        // 220608 randomization 돌아올 때 인쇄되는 이미지의 여백이 반대로 생성되어 Y축이 위와 반대로 움직임 
                        if (Printhead.dirrandnum != 1)
                        {
                            PaixMotion.AbsMove(Yaxis, 108.75);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, 108.75 + 1.55);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        Printhead.printstop();
                        Printing_Sequence = 11;*/


                        break;
                    case 11:
                        //if (PrintEnable == false) return;
                        //Thread.Sleep(100);
                        /*PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_rspeed, Xaxis_rspeed, Xaxis_rspeed);//Xaxis 복귀 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_ready_pos);                                   //프린트헤드 MIN까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                        Printhead.printstop();
                        Printhead.squencedel();

                        Printing_Sequence = 12;*/

                        break;
                    case 12:
                        AxisMovement(Yaxis, 177.33);                                //Y축 이동

                        //Printhead.creatprintbisquence(ImageNum + 2);
                        //ImageNum++;
                        await Task.Run(() => Task.Delay(1000).Wait());

                        // 220608 randomization 여백 방향에 맞춰 Y축 움직임
                        if (Printhead.dirrandnum != 1)
                        {
                            AxisMovement(Yaxis, 177.33);                                //Y축 이동
                        }
                        else
                        {
                            AxisMovement(Yaxis, 177.33 + 1.55);                                //Y축 이동
                        }

                        Printing_Sequence = 14;
                        break;
                    case 13:
                        /*if (PrintEnable == false) return;
                        Printhead.printstart();
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_pspeed, Xaxis_pspeed, Xaxis_pspeed);//Xaxis 출력 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_head_max_pos);                                //프린트헤드 MAX까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                        // Printhead.printstop();
                        Thread.Sleep(100);

                        // 220608 randomization 돌아올 때 인쇄되는 이미지의 여백이 반대로 생성되어 Y축이 위와 반대로 움직임 
                        if (Printhead.dirrandnum != 1)
                        {
                            PaixMotion.AbsMove(Yaxis, 177.33);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, 177.33 + 1.55);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        Printhead.printstop();
                        Printing_Sequence = 14;*/
                        break;
                    case 14:
                        if (PrintEnable == false) return;
                        Thread.Sleep(1000);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_rspeed, Xaxis_rspeed, Xaxis_rspeed);//Xaxis 복귀 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_ready_pos);                                   //프린트헤드 MIN까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                        Printhead.printstop();
                        Printhead.swathesquencedel();


                        //Printhead.swathesquencedel();
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);// (IO OUT 1번 상태를 1로 바꿈) - Shutter Send

                        Thread.Sleep(100);
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);// (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

                        var dwell = Task.Run(() => Task.Delay(Dwell_time * 1000).Wait());
                        TempRead();
                        Templogging(Currentlayer + 1, 0.75);
                        Currentlayer++;
                        txtNowLayer.Text = Currentlayer.ToString();

                        if (Currentlayer % 10 == 0 || Currentlayer == 1)
                        {
                            //hcc = Task.Run(() => Head_Cleaning(Xaxis_Cleaning_pos, Xaxis_ready_pos, Yaxis_Cleaning_start_pos, Yaxis_Cleaning_end_pos, Yaxis_single_lane_pos, Yaxis_Cleaning_speed, Yaxis_speed));
                            hcc = Task.Run(() => Head_cleaning2());
                            await dwell;

                        }
                        else
                        {
                            await dwell;
                        }
                        Printing_Sequence = pAfter_Powder;
                        break;
                }

            }
            await Task.Run(() => Task.Delay(1000).Wait());
            PrintEnable = false;
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 0, 0);
            PaixMotion.EmergencyStop();
            //BLDC_STOP_1();
            processstring = Convert.ToString(DateTime.Now) + "___테스트 출력 종료";
            processbox.Items.Insert(0, processstring);
        }



        private void btnEmerStop_Click(object sender, EventArgs e)
        {
            PrintEnable = false;
            PaixMotion.EmergencyStop();
            //Printhead.Disconnect();
            //BLDC_STOP_0();
            //BLDC_STOP_1();
            //BLDC_STOP_2();
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 0, 0);
            //StartPrinting.Abort();
            //StartPrinting.Join();


            //MaxBLDC.MotorStop();
            MaxBLDC.Disable();
            MaxBLDC.Disconnect();



        }
        /*
        private void Load_printparameter()
        {

            string OpenFilePath = System.Environment.CurrentDirectory;

            ParameterDialog.InitialDirectory = OpenFilePath;
            ParameterDialog.RestoreDirectory = true;

            ParameterDialog.Title = "이미지 선택";
            ParameterDialog.DefaultExt = ".cfg";
            ParameterDialog.FileName = "";
            int a;

            DialogResult dr = ParameterDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var ParameterFileName = ParameterDialog.FileName;
                var Parameter
                var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), filename);
                var image = ImageUtilities.LoadImageFromFile(imageFullName);

            }

            txtTotalLayer.Text = listImage.Items.Count.ToString();
        }
        */

        private void LoadParameter_Click(object sender, EventArgs e)
        {
            //파라미터 로드 기능
            Logger.WriteButtonLog("Load Parameter", logButtonPath);
            string OpenFilePath = System.Environment.CurrentDirectory;

            ParameterDialog.InitialDirectory = OpenFilePath;
            ParameterDialog.RestoreDirectory = true;

            ParameterDialog.Title = "파라미터 파일 선택";
            ParameterDialog.Filter = "cfg files (*.cfg)|*.cfg";
            ParameterDialog.FileName = "";
            DialogResult dr = ParameterDialog.ShowDialog();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //파라미터 복호화 기능
            /*
            Dictionary<char, char> codex = new Dictionary<char, char>
            {
                {'b', 'B'}, {'d', 'D'}, {'D', 'd'}, {'e', 'E'}, {'F', 'S'}, {'K', 'n'}, {'4', '9'}, {'o', 'O'}, {'J', 'j'}, {'Z', 'z'}, {'U', 'x'}, {'3', '3'}, {'1', '4'}, {'Y', 'W'}, {'W', 'Y'}, {'x', 'U'}, {'6', '0'}, {'0', '7'}, {'u', 'h'}, {'S', 'F'}, {'n', 'K'}, {'O', 'o'}, {'C', 'i'}, {'E', 'e'}, {'g', 'G'}, {'A', 'a'}, {'B', 'b'}, {'G', 'g'}, {'9', '1'}, {'2', '2'}, {'i', 'C'}, {'7', '5'}, {'8', '6'}, {'j', 'J'}, {'z', 'Z'}, {'h', 'u'}, {'5', '8'}, {'a', 'A'},
            };
            */
            if (ParameterDialog.FileName.Length > 1)
            {
                paraBox.Items.Clear();
                //Console.WriteLine("IN");
                //string[] bbstrings = ;

                string eee = File.ReadAllText(ParameterDialog.FileName, Encoding.UTF8);
                /*
                string converted = eee.Replace('-', '+');
                converted = converted.Replace('_', '/');
                string decoded = "";
                foreach (char c in converted)
                {
                    if (codex.ContainsKey(c))
                    {
                        decoded += codex[c];
                    }
                    else
                    {
                        decoded += c;
                    }
                }
                
                //Console.WriteLine(decoded);
                converted = decoded;
                string bsstring = Encoding.UTF8.GetString(Convert.FromBase64String(converted));
                */
                string showstring = "";
                string[] sparameters = eee.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string parameter in sparameters)
                {
                    if (parameter.Length < 1)
                    {
                        break;
                    }
                    //Console.WriteLine(parameter);
                    string[] splited = parameter.Split(new string[] { " = " }, StringSplitOptions.None);
                    string[] splitedw = splited[0].Split(new string[] { " " }, StringSplitOptions.None);
                    Console.WriteLine(splitedw[1]);
                    string[] splitedv = splited[1].Split(new string[] { ";" }, StringSplitOptions.None);
                    //Console.WriteLine(splitedv[0]);

                    if (splitedw[1] == "Layer_Thickness")
                    {
                        Console.WriteLine("IN");
                        txtlayerthickness.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Dwell_Time")
                    {
                        txtdwelltime.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Temp_Limit")
                    {
                        txttemplimit.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Temp_Range")
                    {
                        txttemprange.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Roller_Speed")
                    {
                        txtrollerspeed.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Stepper_Speed")
                    {
                        txtstepper.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Recoating_Speed")
                    {
                        txtprspeed.Text = splitedv[0];
                    }
                    else if (splitedw[1] == "Binder_Value")
                    {
                        string[] bindercut = splitedv[0].Split(' ');
                        Console.WriteLine(String.Join(" ", bindercut));
                        txtb1.Text = bindercut[2];
                        txtb2.Text = bindercut[3];
                        txtb3.Text = bindercut[4];
                        txtb4.Text = bindercut[5];
                        txtb5.Text = bindercut[6];
                        txtb6.Text = bindercut[7];
                    }



                    /*
                    if (splitedw[1] == "bldcbytecode")
                    {
                        string[] bldccut = splitedv[0].Split(' ');
                        string bldcspeed = Convert.ToString((Convert.ToInt16(bldccut[7], 16) * 256 + Convert.ToInt16(bldccut[8], 16)) / 10);
                        showstring = "Roller Speed = " + bldcspeed;
                    }
                    else
                    {
                        showstring = splitedw[1] + "=" + splitedv[0];
                    }
                    paraBox.Items.Add(showstring);
                    */
                }
            }
        }
        private async void Powder_Feed(double step_rspeed)
        {
            //파우더 도포 (비동기 call 용)
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, Raxis, rStartSpeed, rAccelSpeed, rDeccelSpeed, step_rspeed * 12);
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, R2axis, rStartSpeed, rAccelSpeed, rDeccelSpeed, step_rspeed * 14);
            Paix_MotionController.NMC2.nmc_JogMove(devID2, Raxis, CCW);
            Paix_MotionController.NMC2.nmc_JogMove(devID2, R2axis, CCW);
        }
        private async Task Powder_Draw(double step_wspeed)
        {
            //파우더 도포 후 역회전 (비동기 call 용)
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, Raxis, rStartSpeed, rAccelSpeed, rDeccelSpeed, step_wspeed * 12);
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, R2axis, rStartSpeed, rAccelSpeed, rDeccelSpeed, step_wspeed * 14);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
            await Task.Run(() => Task.Delay(1000).Wait());
            //Paix_MotionController.NMC2.nmc_JogMove(devID2, Raxis, CW);
            //Paix_MotionController.NMC2.nmc_JogMove(devID2, R2axis, CW);
            await Task.Run(() => Task.Delay(2500).Wait());
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
        }

        private async Task Powder_Supply(int Hopper_Open_interval, double Supply_Pos, double Ready_Pos)
        {
            //파우더 공급 (호퍼 -> 리코터), 비동기 call용
            Console.WriteLine("Hopper Sequence IN");
            await AxisMovement_async(X2axis, 44);//비동기 센서위치 이동
            Paix_MotionController.NMC2.nmc_GetMDIOInPin(devID, 4, out PowderIO);//파우더 잔량 센서 값 확인
            Console.WriteLine(PowderIO.ToString());
            while (PowderIO == 0)//파우더 잔량이 부족할 경우
            {
                Console.WriteLine(PowderIO.ToString());
                await AxisMovement_async(X2axis, Supply_Pos);//공급위치로 이동
                Console.WriteLine("IN");
                Thread.Sleep(1500);//이동대기
                var t1 = Task.Run(() => Powder_Draw(80));//파우더 역회전
                Thread.Sleep(500);
                ShutterOpen(Hopper_Open_interval);//셔터 오픈
                Thread.Sleep(2000);
                await AxisMovement_async(X2axis, Ready_Pos);//비동기 센서위치 이동
                Thread.Sleep(1500);
                //return new ind_powder_supply();
                Paix_MotionController.NMC2.nmc_GetMDIOInPin(devID, 4, out PowderIO);//파우더 잔량 재 측정 (부족할경우 반복 진행)
            }


        }
        private async Task Head_cleaning2()
        {
            //헤드 클리닝 (신모듈)
            await AxisMovement_async(Xaxis, 90);//헤드 X 대기위치 이동
            await AxisMovement_async(Yaxis, 240);//헤드 Y 클리닝 시작위치 이동
            Console.WriteLine("In cleaning2");
            await Task.Delay(1000);
            await AxisMovement_async(Xaxis, 0);//헤드 X 클리닝 시작위치 이동

            //퍼지 위치로 이동
            await Task.Delay(500);

            Printhead.cleanstart1();//퍼지 설정
            await Task.Delay(500);
            Printhead.cleanstart2();//퍼지 수행
            await Task.Delay(500);
            Printhead.cleanstop();//퍼지 종료
            //퍼지 완료

            //펌프 온
            Outpin(6, ON);//맥동펌프 ON
            await Task.Delay(1000);
            PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, 10);//헤드 이동을 위한 속도 부여
            await Task.Delay(500);
            await AxisMovement_async(Yaxis, 100);//헤드 이동 진행
            Outpin(6,OFF);//맥동펌프 OFF
            //펌프 오프

            //에어 클리닝
            Outpin(5, ON);//에어 ON
            PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, 5);//헤드 이동을 위한 속도 재부여
            await Task.Delay(500);
            await AxisMovement_async(Yaxis, -40);//종료위치까지 헤드 이동
            Outpin(5, OFF);//에어 OFF
            PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, 20);//헤드 속도 복귀
            await Task.Delay(500);
            AxisMovement_async(Xaxis, 90);//헤드 준비위치 이동
            await AxisMovement_async(Yaxis, 55);//헤드 준비위치 이동
            

        }
        private async Task Head_Cleaning(double Xstart, double Xend, double Ystart, double Yend, double Yend2, double Yspeed, double Yspeed2)
        {
            //이전에 사용하던 헤드 클리닝 함수, 현재 사용하지 않음.
            /*
            await AxisMovement_async(Xaxis, Xend);
            await AxisMovement_async(Yaxis, Ystart);
            await AxisMovement_async(Xaxis, Xstart);
            PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, Yspeed);//클리닝 속도로 변경
            await AxisMovement_async(Yaxis, Yend);
            await AxisMovement_async(Xaxis, Xend);
            PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, Yspeed2);//일반 속도로 변경
            PaixMotion.SetSCurveSpeed(Yaxis, 50, 200, 200, 100);
            await AxisMovement_async(Yaxis, Yend2);
            */
            //Console.WriteLine("클리닝생략");
            //return new ind_head_cleaning();

            await Task.Delay(1000);
            Console.WriteLine("InCleaning");
            await AxisMovement_async(Xaxis, Xstart);//90(헤드 대기 위치)이동
            await AxisMovement_async(Yaxis, 100);//y축 이동 - 57이 되어야함(DH)
            //await AxisMovement_async(Xaxis, Xstart);//20(DH)
            ////PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, Yspeed);//클리닝 속도로 변경
            ////await AxisMovement_async(Yaxis, Yend);
            await Task.Delay(500);
            Printhead.cleanstart1();
            //textBox1.Text = Printhead.Row0WaveformIDRead();
            //textBox2.Text = Printhead.Row1WaveformIDRead();
            //textBox3.Text = Printhead.Row3WaveformIDRead();
            //textBox4.Text = Printhead.Row4WaveformIDRead();
            await Task.Delay(500);
            Printhead.cleanstart2();
            await Task.Delay(500);
            Printhead.cleanstop();
            //textBox1.Text = Printhead.Row0WaveformIDRead();
            //textBox2.Text = Printhead.Row1WaveformIDRead();
            //textBox3.Text = Printhead.Row3WaveformIDRead();
            //textBox4.Text = Printhead.Row4WaveformIDRead();
            await Task.Delay(500);
            AxisMovement_async(Xaxis, Xend);
            await AxisMovement_async(Yaxis, Yend2);
            ////PaixMotion.SetSCurveSpeed(Yaxis, iStartSpeed, iAccelSpeed, iDeccelSpeed, Yspeed2);//일반 속도로 변경
            ////PaixMotion.SetSCurveSpeed(Yaxis, 50, 200, 200, 100);

            ////return new ind_head_cleaning();

        }
        private void AxisMovement(short axis, double position)
        {
            //축 이동용 함수
            if (axis == Zaxis)
            {
                PaixMotion.RelMove(axis, position);
                updateCmdEnc();
                while (NmcData.nBusy[axis] == 1)
                {
                    Application.DoEvents(); updateCmdEnc();
                }
            }
            else
            {
                PaixMotion.AbsMove(axis, position);
                updateCmdEnc();
                while (NmcData.nBusy[axis] == 1)
                {
                    Application.DoEvents(); updateCmdEnc();
                }
            }
        }
        private void Outpin(short pinid, short onoff)
        {
            //IO out 제어용 함수
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, pinid, onoff);
        }
        /*
        private async Task BLDC_Stopper(short axis, double position, short SmallorBig)
        {
            Serial_RS485.Close();
            Thread.Sleep(100);
            Serial_RS485.Open();
            if (SmallorBig == small)
            {
                //Console.WriteLine(Serial_RS485.IsOpen);
                while (NmcData.dEnc[axis] > position+1)
                {
                    //Console.WriteLine(Serial_RS485.IsOpen);
                    //Console.WriteLine(@NmcData.dEnc[axis]);
                    await Task.Run(() => Task.Delay(100).Wait());
                }
                BLDC_STOP_0();
                Thread.Sleep(100);
                BLDC_STOP_2();
            }
            else
            {
                //Console.WriteLine(Serial_RS485.IsOpen);
                while (NmcData.dEnc[axis] < position - 1)
                {
                    //Console.WriteLine(Serial_RS485.IsOpen);
                    //Console.WriteLine(@NmcData.dEnc[axis]);
                    await Task.Run(() => Task.Delay(100).Wait());
                }
                BLDC_STOP_0();
                Thread.Sleep(100);
                BLDC_STOP_2();
            }
        }

        */

        private async Task BLDC_Stopper(short axis, double position, short SmallorBig)
        {
            //비동기 stepper 모터 정지용 함수
            if (SmallorBig == small)//작은 쪽으로 이동 할 때
            {
                while (NmcData.dEnc[axis] > position + 1)//현재 포지션이 설정 포지션보다 큰 동안
                {
                    await Task.Run(() => Task.Delay(100).Wait());//0.1초단위로 축을 추적하다가
                }
                Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
                Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
                //STEPPER 모터 정지 신호 발송
                Powder_Draw(80);//파우더 역회전 진행
            }
            else//큰 쪽으로 갈때
            {
                while (NmcData.dEnc[axis] < position - 1)//현재 포지션이 설정 포지션보다 작은 동안
                {
                    await Task.Run(() => Task.Delay(100).Wait());//0.1초 단위로 축을 추적하다가
                }
                Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
                Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
                //stepper 모터 정지 신호 발송
                Powder_Draw(80);//파우더 역회전 진행
            }
        }

        private async Task roller_Starter(short axis, double position, short SmallorBig, int maxonrpm)
        {
            //비동기로 롤러를 시작하기 위한 함수
            if (SmallorBig == small)//작은 쪽으로 이동할때
            {
                while (NmcData.dEnc[axis] > position)//현재 포지션이 설정 포지션보다 큰 동안
                {
                    await Task.Run(() => Task.Delay(100).Wait());//0.1초 단위로 추적하다가
                    //Console.WriteLine(@NmcData.dEnc[axis]);
                }
                //BLDC_RUN_1_rev(bldcbytecode);
                maxon_run(maxonrpm, false);//맥슨모터 가동
            }
            else//큰 쪽으로 이동할때
            {
                while (NmcData.dEnc[axis] < position)//현재 포지션이 설정 포지션보다 작은 동안
                {
                    await Task.Run(() => Task.Delay(100).Wait());//0.1초 단위로 추적하다가
                }
                //BLDC_RUN_1_rev(bldcbytecode);
                maxon_run(maxonrpm, false);//맥슨모터 가동
            }
        }
        private async Task roller_Stopper(short axis, double position, short SmallorBig)
        {
            //비동기로 롤러를 정지하기 위한 함수
            if (SmallorBig == small)//작은 쪽으로 이동할때
            {
                while (NmcData.dEnc[axis] > position)//현재 포지션이 설정 포지션보다 큰 동안
                {
                    await Task.Run(() => Task.Delay(100).Wait());//0.1초 단위로 추적하다가
                }
                //BLDC_STOP_1();
                maxon_stop();//맥슨모터 정지
            }
            else//큰 쪽으로 이동할때
            {
                while (NmcData.dEnc[axis] < position)//현재 포지션이 설정 포지션보다 작은 동안
                {
                    await Task.Run(() => Task.Delay(100).Wait());//0.1초 단위로 추적하다가
                }
                //BLDC_STOP_1();
                maxon_stop();//맥슨모터 정지
            }
        }
        private async Task AxisMovement_async(short axis, double position)
        {
            //비동기로 축을 이동하기 위한 함수 (절대좌표)
            short SmallorBig;
            if (NmcData.dEnc[axis] >= position)//포지션을 판단하여 small 또는 big 부여
            {
                SmallorBig = small;
            }
            else
            {
                SmallorBig = big;
            }
            //Console.WriteLine(SmallorBig);
            //Console.WriteLine(position);
            if (SmallorBig == small)//작은 쪽으로 진행할때
            {
                PaixMotion.AbsMove(axis, position);//절대좌표 이동명령
                while (NmcData.dEnc[axis] > position + 1)//이동 완료시까지 축 좌표 추적
                {
                    //Console.WriteLine(@NmcData.dEnc[axis]);
                    Thread.Sleep(100);
                }
            }
            else
            {
                PaixMotion.AbsMove(axis, position);//절대좌표 이동명령
                while (NmcData.dEnc[axis] < position - 1)//이동 완료시까지 축 좌표 추적
                {
                    //Console.WriteLine(@NmcData.dEnc[axis]);
                    Thread.Sleep(100);
                }
            }
            Thread.Sleep(1000);
        }

        private async Task Templogging(int ln, double sn)
        {
            //온도 로깅 함수
            Console.WriteLine(logtempPath);//저장 위치
            Logger.WrtieTempLog(Convert.ToString(ln + sn) + "\t" + Convert.ToString(temp), logtempPath);//현재 레이어 + 세부레이어 + 탭 + 현재온도 저장
        }

        private async Task TempEmergency()
        {
            //현재 사용하지 않음 (실용성 없음)
            while (true)
            {
                if (NmcData.dEnc[Xaxis] < 100 || NmcData.dEnc[X2axis] < 50)
                {
                    short counter = 0;
                    while (true)
                    {
                        if (counter > 20)
                        {
                            PrintEnable = false;
                            PaixMotion.EmergencyStop();
                            Printhead.Disconnect();
                            //BLDC_STOP_0();
                            //BLDC_STOP_1();
                            //BLDC_STOP_2();
                            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 0, 0);
                            break;
                        }
                        TempRead();
                        if (temp > 120)
                        {
                            counter++;
                        }
                        Thread.Sleep(1000);
                    }
                }
            }

        }

        private async void Print()
        {

            Logger.WriteButtonLog("Print Start", logButtonPath);
            PrintEnable = true;
            //프린팅 시퀸스 추가 변수 선언부


            //빌드 관련 변수
            double Layer_thickness = Convert.ToDouble(txtlayerthickness.Text) / 1000;//레이어 두께
            int Hopper_Open_interval = 300;//Hopper 오픈 시간 (ms)
            byte[] Binder_Value = { Convert.ToByte(txtb6.Text), Convert.ToByte(txtb5.Text), Convert.ToByte(txtb4.Text), Convert.ToByte(txtb3.Text), Convert.ToByte(txtb2.Text), Convert.ToByte(txtb1.Text), 0, 0 };

            //리코팅 관련 변수
            double X2axis_dspeed = 25;//건조시 램프 이동 speed
            short IR_preheat_time = 2;//램프 예열 시간
            double templimit = Convert.ToDouble(txttemplimit.Text);//온도기준값
            double temprange = Convert.ToDouble(txttemprange.Text);//온도여유값

            int Recoating_repeat = 0;//5;//리코팅 반복횟수
            double X2axis_nspeed = 25;//기본 리코팅 작업시 Feeder speed
            double X2axis_fspeed = 5;//마지막 리코팅 작업시 Feeder speed
            double X2axis_pspeed = Convert.ToDouble(txtprspeed.Text);//파우더 도포시 Feeder speed
            double X2axis_bspeed = 100;//X축 복귀 속도
            double step_rspeed = Convert.ToDouble(txtstepper.Text);//파우더 공급시 스텝모터 회전속도

            //출력 관련 변수
            double Xaxis_pspeed = 300;//헤드 출력시 X축 speed
            double Xaxis_rspeed = 300;//헤드 복귀시 X축 speed
            double Yaxis_speed = 100; //Y축 이동 속도

            short Dwell_time = Convert.ToInt16(txtdwelltime.Text);//프린팅 사이 대기 시간 (초)

            //위치 관련 변수

            //Feeder
            double X2axis_hopper_pos = 0;//hopper 위치
            double X2axis_ready_pos = 44; //Feeder대기 위치
            double X2axis_roller_max_pos = 615;//Roller Max 위치(packing 시작)
            double X2axis_roller_min_pos = 325;//Roller Min 위치(packing 중단)
            double X2axis_powder_max_pos = 680;//Powder Max 위치(도포 시작)
            double X2axis_powder_min_pos = 420;//Powder Min 위치(도포 중단)550;//모터 이상으로 인해 반만 도포하는것으로 수정 220524 //420;//Powder Min 위치(도포 중단)
            double X2axis_heater_max_pos = 815;//Heater Max위치(히터 OFF)

            //Head_X
            double Xaxis_ready_pos = 90;//헤드 대기 위치
            double Xaxis_head_max_pos = 840;//최대 출력 위치
            double Xaxis_Cleaning_pos = 0;

            //Head_Y
            //double Yaxis_ready_pos = 144;
            //double Yaxis_first_lane_pos = ;
            //double Yaxis_second_lane_pos = ;
            double Yaxis_single_lane_pos = 55;
            double Yaxis_Cleaning_start_pos = 245;
            double Yaxis_Cleaning_end_pos = 45;
            double Yaxis_Parking_pos = 40;
            double Yaxis_Cleaning_speed = 8;
            double Yaxis_corr = -28;

            int rollerrpm = Convert.ToInt32(txtroller.Text);
            //byte[] bldcbytecode = Bldc_byteget(Convert.ToInt32(txtroller.Text));//{ 0xFF, 0xFE, 0x01, 0x06, 0x1E, 0x03, 0x00, 0x0B, 0xB8, 0x14 };



            //프린팅 시퀀스 여기서부터 시작
            Printhead.SetPaletteTable(Binder_Value);
            short Printing_Sequence = pZ_Down;
            //Recoating_repeat = Recoating_repeat;//값보정(반복횟수) - 삭제(시퀸스 재정비로 인한 보정 삭제)
            string processstring = Convert.ToString(DateTime.Now) + "___프린팅 시작";
            processbox.Items.Insert(0, processstring);
            percentprogress.Text = Convert.ToString(0) + "%";


            

            //변수설정부
            Currentlayer = 0;
            ImageNum = 0;
            JobLayer = listImage.Items.Count;
            printprogress.Style = ProgressBarStyle.Continuous;
            printprogress.Minimum = 0;
            printprogress.Maximum = JobLayer;
            printprogress.Step = 1;
            printprogress.Value = 0;
            printprogress.Visible = true;



            //구동파라미터
            PaixMotion.SetSCurveSpeed(Zaxis, 0.5, 1, 1, 1);//Z축 이동 속도를 1mm/s로 고정 (z축 퉁퉁거리는거 방지, start/end speed도 target speed에 맞추는것이 필요)

            //PrintEnable = true;
            //맥슨준비작업
            


            //축 대기위치 이동
            AxisMovement(Xaxis, Xaxis_ready_pos);
            AxisMovement(Yaxis, Yaxis_single_lane_pos);
            AxisMovement(X2axis, X2axis_ready_pos);

            if (head_onv==true)//헤드 클리닝 반복 관련 변수
            {
                head_onv = false;
            }    
            

            //파우더 공급 및 헤드 클리닝 비동기 실행
            var pss = Task.Run(() => Powder_Supply(Hopper_Open_interval, X2axis_hopper_pos, X2axis_ready_pos));
            var hcc = Task.Run(() => Head_cleaning2());

            //var hcc = Task.Run(() => Head_Cleaning(Xaxis_Cleaning_pos, Xaxis_ready_pos, Yaxis_Cleaning_start_pos, Yaxis_Cleaning_end_pos, Yaxis_single_lane_pos, Yaxis_Cleaning_speed, Yaxis_speed));


            while (Currentlayer <= JobLayer && PrintEnable == true)
            {
                if (Currentlayer == JobLayer)
                {
                    percentprogress.Text = Convert.ToString(100) + "%";
                    processstring = Convert.ToString(DateTime.Now) + "___[ " + Convert.ToString(Currentlayer) + " / " + Convert.ToString(JobLayer) + " ] 출력 작업 완료, 마무리 중...";
                    processbox.Items.Insert(0, processstring);
                    await hcc;
                    PaixMotion.AbsMove(Yaxis, Yaxis_Parking_pos);
                    updateCmdEnc();
                    while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                    PaixMotion.AbsMove(Xaxis, Xaxis_Cleaning_pos);
                    updateCmdEnc();
                    while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                    await Task.Delay(5000);

                    PaixMotion.AbsMove(Yaxis, Yaxis_Parking_pos);
                    updateCmdEnc();
                    while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                    break;
                }
                switch (Printing_Sequence)
                {
                    case pZ_Down:// [  1/   8]
                        txtNowLayer.Text = (Currentlayer + 1).ToString();
                        if (Currentlayer == 0)
                        {
                            layertimes.Clear();
                            layertimes.Add(DateTime.Now);
                            processstring = Convert.ToString(DateTime.Now) + "___[ " + Convert.ToString(Currentlayer + 1) + " / " + Convert.ToString(JobLayer) + " ] 출력 작업 중";
                        }
                        else
                        {
                            layertimes.Add(DateTime.Now);
                            double sumtime = 0;
                            for (int i = 1; i < layertimes.Count; i++)
                            {
                                sumtime = sumtime + Convert.ToInt32(layertimes[i].Subtract(layertimes[i - 1]).TotalSeconds) * Math.Log(i);
                            }
                            double weightedsum=layertimes.Count * Math.Log(layertimes.Count) - layertimes.Count;
                            processstring = Convert.ToString(DateTime.Now) + "___[ " + Convert.ToString(Currentlayer + 1) + " / " + Convert.ToString(JobLayer) + " ] 출력 작업 중, 예상 완료 시간 : " + Convert.ToString(DateTime.Now.AddSeconds(Convert.ToInt32(sumtime / weightedsum * (listImage.Items.Count - layertimes.Count))));
                        }
                        processbox.Items.Insert(0, processstring);

                        if (ModChk.Checked == true)
                        {
                            while (ModChk.Checked)
                            {
                                var modkask = MessageBox.Show("파라미터 변경을 위해 프린트가 중단되었습니다. 현재 입력값을 적용하여 프린팅하시겠습니까? \n아니오를 선택할 시, 10초 후 다시 이 창이 나타납니다.\n취소를 선택하면 이전 값으로 돌아가 프린팅합니다.", "파라미터 변경 감지", MessageBoxButtons.YesNoCancel);
                                if (modkask == DialogResult.Yes)
                                {
                                    log_printingsettings();
                                    Layer_thickness = Convert.ToDouble(txtlayerthickness.Text) / 1000;//레이어 두께
                                    byte[] temp_Value = { Convert.ToByte(txtb6.Text), Convert.ToByte(txtb5.Text), Convert.ToByte(txtb4.Text), Convert.ToByte(txtb3.Text), Convert.ToByte(txtb2.Text), Convert.ToByte(txtb1.Text), 0, 0 };
                                    Binder_Value = temp_Value;
                                    templimit = Convert.ToDouble(txttemplimit.Text);//온도기준값
                                    temprange = Convert.ToDouble(txttemprange.Text);//온도여유값
                                    X2axis_pspeed = Convert.ToDouble(txtprspeed.Text);
                                    step_rspeed = Convert.ToDouble(txtstepper.Text);
                                    Dwell_time = Convert.ToInt16(txtdwelltime.Text);//프린팅 사이 대기 시간 (초)
                                    rollerrpm = Convert.ToInt32(txtroller.Text) * 1000;
                                    //bldcbytecode = Bldc_byteget(Convert.ToInt32(txtroller.Text));
                                    ModChk.Checked = false;
                                }
                                else if (modkask == DialogResult.No)
                                {
                                    await Task.Delay(10000);
                                }
                                else
                                {
                                    txtlayerthickness.Text = Convert.ToString(Layer_thickness * 1000);
                                    txtroller.Text = Convert.ToString((rollerrpm / 1000));
                                    //txtroller.Text = Bldc_speedget(bldcbytecode);
                                    txtdwelltime.Text = Convert.ToString(Dwell_time);
                                    txtstepper.Text = Convert.ToString(step_rspeed);
                                    txtprspeed.Text = Convert.ToString(X2axis_pspeed);
                                    txttemprange.Text = Convert.ToString(temprange);
                                    txttemplimit.Text = Convert.ToString(templimit);
                                    txtb1.Text = Convert.ToString(Binder_Value[5]);
                                    txtb2.Text = Convert.ToString(Binder_Value[4]);
                                    txtb3.Text = Convert.ToString(Binder_Value[3]);
                                    txtb4.Text = Convert.ToString(Binder_Value[2]);
                                    txtb5.Text = Convert.ToString(Binder_Value[1]);
                                    txtb6.Text = Convert.ToString(Binder_Value[0]);
                                    ModChk.Checked = false;
                                }

                            }

                        }
                        await pss;
                        selectedimgshow(Convert.ToInt16(Currentlayer));
                        await Task.Delay(400);
                        AxisMovement(X2axis, Xaxis_ready_pos);
                        imgtrackbar.Value = Currentlayer;
                        AxisMovement(Zaxis, -1 + (-1) * ((Layer_thickness * (Recoating_repeat + 2))));
                        Printing_Sequence = pHeater_ON;
                        break;

                    case pHeater_ON:
                        if (PrintEnable == false) return;
                        TempRead();
                        await Templogging(Currentlayer + 1, 0);
                        PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, X2axis_bspeed, X2axis_bspeed, X2axis_bspeed);//X2축 이동 속도를 복귀 속도로 변경
                        Console.WriteLine(temp);
                        Console.WriteLine(templimit);
                        Console.WriteLine(temprange);
                        //온도 설정값, 여유값에 따른 베드 온도 컨트롤 기능
                        if (temp < templimit-temprange/2)//온도가 낮을때 (일반적인 상황) - 히터 가열
                        {
                            AxisMovement(X2axis, X2axis_roller_min_pos - 50);
                            Outpin(0, TurnON);
                            await Task.Run(() => Task.Delay(IR_preheat_time * 1000).Wait());
                            PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, X2axis_dspeed, X2axis_dspeed, X2axis_dspeed);//X2축 이동 속도를 건조 속도로 변경
                        }
                        else//온도가 높을때 - 히터 가열하지 않고 축만 이동
                        {
                            AxisMovement(X2axis, X2axis_roller_min_pos - 50);
                        }
                        Printing_Sequence = pRecoater_MoveToMax;
                        break;

                    case pRecoater_MoveToMax:
                        AxisMovement(X2axis, X2axis_heater_max_pos);
                        Outpin(0, TurnOFF);
                        await Task.Run(() => Task.Delay(Dwell_time * 500).Wait());
                        Printing_Sequence = pZ_Up;
                        break;
                    case pZ_Up:
                        if (PrintEnable == false) return;
                        TempRead();
                        await Templogging(Currentlayer + 1, 0.25);
                        //과열 방지 관련 내용
                        if (temp < templimit - temprange)//온도가 낮을 경우 재가열
                        {
                            Printing_Sequence = pHeater_ON;
                        }
                        else if (temp > templimit + temprange)//온도가 높을 경우 대기 후 재측정
                        {
                            Console.WriteLine("과열 대기중");
                            await Task.Run(() => Task.Delay(5000).Wait());
                            Printing_Sequence = pZ_Up;
                        }
                        else//온도가 적당할 경우 다음 시퀸스 진행
                        {
                            Printing_Sequence = pPowder_Feed_Ready;
                        }
                        break;
                    case pPowder_Feed_Ready:
                        AxisMovement(Zaxis, Layer_thickness + 1);                                //1-layer thickness 만큼 Z축 상승
                        PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, X2axis_bspeed, X2axis_bspeed, X2axis_bspeed);//X2축 이동 속도를 복귀 속도로 변경
                        var rs = Task.Run(() => roller_Starter(X2axis, X2axis_roller_max_pos, small, rollerrpm));
                        AxisMovement(X2axis, X2axis_powder_max_pos);                                //리코터 파우더 도포 시작 위치까지 이동
                        //BLDC_RUN_1_rev();
                        //await Task.Run(() => Task.Delay(4000).Wait());
                        Powder_Feed(step_rspeed);
                        Printing_Sequence = pRecoater_Recoating;
                        break;

                    case pRecoater_Recoating:
                        if (PrintEnable == false) return;
                        PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, X2axis_pspeed, X2axis_pspeed, X2axis_pspeed);//파우더 도포 스피드로 변경
                        var pf = Task.Run(() => BLDC_Stopper(X2axis, X2axis_powder_min_pos, small));
                        var pf2 = Task.Run(() => roller_Stopper(X2axis, X2axis_roller_min_pos, small));
                        AxisMovement(X2axis, X2axis_roller_min_pos - 30);                                //리코터 베드 roller out MIN 방향으로 이동
                                                                                                         //AxisMovement(X2axis, X2axis_powder_min_pos);                                //리코터 베드 표면 리코팅 위치까지만 MIN 방향으로 이동
                                                                                                         //Powder_Draw(step_rspeed);

                        //리코팅 시퀸스 시작
                        short Recoating_sequence = rZ_Down;//리코팅 시퀸스 기준변수
                        int Current_recoat = 0;//현재 리코팅 횟수 카운터
                        //리코팅 시퀸스 사용하지 않음
                        while (Current_recoat < Recoating_repeat)
                        {
                            if (PrintEnable == false) return;
                            if (Current_recoat == Recoating_repeat)//리코팅 시퀸스 탈출조건
                            {
                                break;
                            }
                            switch (Recoating_sequence)
                            {
                                case rZ_Down:
                                    PaixMotion.RelMove(Zaxis, -1 * Layer_thickness * 10);//10레이어 단위만큼 Z축 하강
                                    updateCmdEnc();
                                    while (NmcData.nBusy[Zaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                                    Recoating_sequence = rF_Forward;
                                    break;
                                case rF_Forward:
                                    PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, iAccelSpeed, iDeccelSpeed, X2axis_bspeed);//X2axis 복귀 속도로 변경
                                    PaixMotion.AbsMove(X2axis, X2axis_roller_max_pos);
                                    updateCmdEnc();
                                    while (NmcData.nBusy[X2axis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                                    //BLDC_RUN_1_throw();
                                    Thread.Sleep(3000);
                                    //BLDC_STOP_1();
                                    Recoating_sequence = rZ_up;
                                    break;
                                case rZ_up:
                                    PaixMotion.RelMove(Zaxis, Layer_thickness * 11);//11레이어 단위만큼 Z축 상승
                                    updateCmdEnc();
                                    while (NmcData.nBusy[Zaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                                    Recoating_sequence = rF_Backward;
                                    break;
                                case rF_Backward:
                                    if (Current_recoat == Recoating_repeat - 1)
                                    {
                                        PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, iAccelSpeed, iDeccelSpeed, X2axis_fspeed);//마지막 리코팅시 마지막 리코팅 속도로 변경
                                        //BLDC_RUN_1_stop();//Roller_Stop
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, iAccelSpeed, iDeccelSpeed, X2axis_nspeed);//마지막 리코팅이 아닐시 일반 리코팅 속도로 변경
                                        //BLDC_RUN_1();//Roller작동
                                        Thread.Sleep(2000);
                                    }
                                    PaixMotion.AbsMove(X2axis, X2axis_roller_min_pos);
                                    updateCmdEnc();
                                    while (NmcData.nBusy[X2axis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                                    Recoating_sequence = r_End;
                                    break;
                                case r_End:
                                    //BLDC_STOP_1();//Roller작동 해제
                                    Thread.Sleep(1000);
                                    //BLDC_RUN_1_throw();
                                    Thread.Sleep(3000);
                                    //BLDC_STOP_1();
                                    Current_recoat = Current_recoat + 1;
                                    Recoating_sequence = rZ_Down;
                                    break;
                            }
                        }
                        //리코팅 시퀸스 전체 사용하지 않음
                        PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, iAccelSpeed, iDeccelSpeed, X2axis_bspeed);//복귀 속도로 변경
                        AxisMovement(X2axis, X2axis_ready_pos);                                  //리코터 MIN까지 이동

                        TempRead();
                        Templogging(Currentlayer + 1, 0.5);

                        //camera_동작 추가230405
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);// (IO OUT 1번 상태를 1로 바꿈) - Shutter Send
                        Thread.Sleep(100);
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);// (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

                        //ImageNum++; //이동
                        //Currentlayer++; //이동

                        //Printing_Sequence = pZ_Down;
                        // 이미지 사이즈에 맞춰 스텝을 만들어야...

                        Printhead.ImageConProc(listImage.Items[ImageNum].ToString());
                        //Printhead.imageswathedown(ImageNum);

                        Thread.Sleep(100);

                        await hcc;

                        PaixMotion.SetSCurveSpeed(Yaxis, 50, 200, 200, 100);
                        pss = Task.Run(() => Powder_Supply(Hopper_Open_interval, X2axis_hopper_pos, X2axis_ready_pos));

                        if (Printhead.imagewidth <= 957)
                        {
                            Printing_Sequence = pImageProcess;
                        }
                        else
                        {
                            Printing_Sequence = 9; // 220617 이미지 사이즈가 958 pixel 이상이면 2 Way로
                        }

                        break;
                    case pImageProcess:
                        if (PrintEnable == false) return;
                        //Printhead.ImageConProc(listImage.Items[ImageNum].ToString());
                        //Printhead.imageswathedown(ImageNum);
                        Printhead.Swathedown();
                        Thread.Sleep(100);
                        // 220607
                        //Printhead.creatprintsquence(ImageNum);
                        Printhead.creatprintbisquence(ImageNum);
                        ImageNum++;
                        Thread.Sleep(100);
                        
                        // 220608 randomization 여백 방향에 맞춰 Y축 움직임
                        if (Printhead.dirrandnum == 1)
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos + 3.03);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }

                        Printing_Sequence = pPrint;
                        break;
                    case pPrint:
                        if (PrintEnable == false) return;
                        Printhead.printstart();
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_pspeed, Xaxis_pspeed, Xaxis_pspeed);//Xaxis 출력 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_head_max_pos);                                //프린트헤드 MAX까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                        // Printhead.printstop();
                        Thread.Sleep(100);

                        // 220608 randomization 돌아올 때 인쇄되는 이미지의 여백이 반대로 생성되어 Y축이 위와 반대로 움직임 
                        if (Printhead.dirrandnum != 1)
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, Yaxis_single_lane_pos + 3.03);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }

                        Printing_Sequence = pPrinthead_MoveToMin;
                        break;
                    case pPrinthead_MoveToMin:
                        if (PrintEnable == false) return;
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_rspeed, Xaxis_rspeed, Xaxis_rspeed);//Xaxis 복귀 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_ready_pos);                                   //프린트헤드 MIN까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                        Printhead.printstop();
                        Printhead.swathesquencedel();

                        Currentlayer++;
                        txtNowLayer.Text = Currentlayer.ToString();
                        //Printhead.swathesquencedel();
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);//히터 ON (IO OUT 1번 상태를 1로 바꿈) - Shutter Send

                        Thread.Sleep(100);
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);//히터 ON (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

                        await Task.Run(() => Task.Delay(Dwell_time * 1000).Wait());

                        //파우더 공급

                        //await Powder_Supply(Hopper_Open_interval, X2axis_hopper_pos, X2axis_ready_pos);

                        Printing_Sequence = pZ_Down;


                        break;
                    case 9:
                        if (PrintEnable == false) return;
                        Printhead.imageswathedown(ImageNum);
                        Printhead.creatprintbisquence(ImageNum);
                        ImageNum++;
                        await Task.Run(() => Task.Delay(100).Wait());
                        // 220617 2Way 인쇄를 위해 Y축 2Way 시작위치로 이동
                        // 220608 randomization 여백 방향에 맞춰 Y축 움직임
                        if (Printhead.dirrandnum == 1)
                        {
                            AxisMovement(Yaxis, 108.75+Yaxis_corr);                                //Y축 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, 108.75 + 1.55+Yaxis_corr);                                //Y축 이동
                        }

                        Printing_Sequence = 10;
                        break;
                    case 10:
                        if (PrintEnable == false) return;
                        Printhead.printstart();
                        showpnlY.BackColor = Color.DarkViolet;
                        await Task.Run(() => Task.Delay(100).Wait());
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_pspeed, Xaxis_pspeed, Xaxis_pspeed);//Xaxis 출력 스피드로 변경
                        AxisMovement(Xaxis, Xaxis_head_max_pos);                                //프린트헤드 MAX까지 이동

                        //Printhead.printstop();
                        await Task.Run(() => Task.Delay(100).Wait());
                        Printing_Sequence = 12;

                        /*
                        // 220608 randomization 돌아올 때 인쇄되는 이미지의 여백이 반대로 생성되어 Y축이 위와 반대로 움직임 
                        if (Printhead.dirrandnum != 1)
                        {
                            PaixMotion.AbsMove(Yaxis, 108.75);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, 108.75 + 1.55);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        Printhead.printstop();
                        Printing_Sequence = 11;*/


                        break;
                    case 11:
                        //if (PrintEnable == false) return;
                        //Thread.Sleep(100);
                        /*PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_rspeed, Xaxis_rspeed, Xaxis_rspeed);//Xaxis 복귀 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_ready_pos);                                   //프린트헤드 MIN까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                        Printhead.printstop();
                        Printhead.squencedel();

                        Printing_Sequence = 12;*/

                        break;
                    case 12:
                        AxisMovement(Yaxis, 177.33+Yaxis_corr);                                //Y축 이동

                        //Printhead.creatprintbisquence(ImageNum + 2);
                        //ImageNum++;
                        await Task.Run(() => Task.Delay(100).Wait());

                        // 220608 randomization 여백 방향에 맞춰 Y축 움직임
                        if (Printhead.dirrandnum != 1)
                        {
                            AxisMovement(Yaxis, 177.33+Yaxis_corr);                                //Y축 이동
                        }
                        else
                        {
                            AxisMovement(Yaxis, 177.33 + 1.55+Yaxis_corr);                                //Y축 이동
                        }

                        Printing_Sequence = 14;
                        break;
                    case 13:
                        /*if (PrintEnable == false) return;
                        Printhead.printstart();
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_pspeed, Xaxis_pspeed, Xaxis_pspeed);//Xaxis 출력 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_head_max_pos);                                //프린트헤드 MAX까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }

                        // Printhead.printstop();
                        Thread.Sleep(100);

                        // 220608 randomization 돌아올 때 인쇄되는 이미지의 여백이 반대로 생성되어 Y축이 위와 반대로 움직임 
                        if (Printhead.dirrandnum != 1)
                        {
                            PaixMotion.AbsMove(Yaxis, 177.33);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        else
                        {
                            PaixMotion.AbsMove(Yaxis, 177.33 + 1.55);                                //Y축 이동
                            updateCmdEnc();
                            while (NmcData.nBusy[Yaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }//Y축 대기위치 이동
                        }
                        Printhead.printstop();
                        Printing_Sequence = 14;*/
                        break;
                    case 14:
                        if (PrintEnable == false) return;
                        Thread.Sleep(100);
                        PaixMotion.SetSCurveSpeed(Xaxis, iStartSpeed, Xaxis_rspeed, Xaxis_rspeed, Xaxis_rspeed);//Xaxis 복귀 스피드로 변경
                        PaixMotion.AbsMove(Xaxis, Xaxis_ready_pos);                                   //프린트헤드 MIN까지 이동
                        updateCmdEnc();
                        while (NmcData.nBusy[Xaxis] == 1) { Application.DoEvents(); updateCmdEnc(); if (PrintEnable == false) return; }
                        Printhead.printstop();
                        showpnlY.BackColor = Color.Plum;
                        Printhead.swathesquencedel();


                        //Printhead.swathesquencedel();
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);// (IO OUT 1번 상태를 1로 바꿈) - Shutter Send

                        Thread.Sleep(100);
                        Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);// (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

                        var dwell = Task.Run(() => Task.Delay(Dwell_time * 1000).Wait());
                        TempRead();
                        Templogging(Currentlayer + 1, 0.75);
                        layertimes[Currentlayer] = DateTime.Now;
                        Currentlayer++;
                        txtNowLayer.Text = Currentlayer.ToString();
                        printprogress.PerformStep();
                        double perc = 100.0 * Convert.ToDouble(Currentlayer) / JobLayer;
                        percentprogress.Text = Convert.ToString(Math.Round(perc, 1)) + "%";

                        if (Currentlayer % 1 == 0 || Currentlayer == JobLayer)//4번마다 헤드 클리닝 수행
                        {
                            hcc = Task.Run(() => Head_cleaning2());
                            //hcc = Task.Run(() => Head_Cleaning(Xaxis_Cleaning_pos, Xaxis_ready_pos, Yaxis_Cleaning_start_pos, Yaxis_Cleaning_end_pos, Yaxis_single_lane_pos, Yaxis_Cleaning_speed, Yaxis_speed));
                            await dwell;
                        }
                        else
                        {
                            await dwell;
                        }
                        Printing_Sequence = pZ_Down;
                        break;
                }

            }
            await Task.Run(() => Task.Delay(1000).Wait());
            PrintEnable = false;
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 0, 0);
            //PaixMotion.EmergencyStop();
            maxon_stop();
            
            percentprogress.Text = Convert.ToString(100) + "%";
            processstring = Convert.ToString(DateTime.Now) + "___프린팅 완료";
            processbox.Items.Insert(0, processstring);
            await hcc;
            head_onv = true;
            byte[] end_Value = { 7, 7, 7, 7, 7, 7, 7, 0 };
            Printhead.SetPaletteTable(end_Value);
            processstring = Convert.ToString(DateTime.Now) + "프린트헤드 정기 퍼지 수행 시작";
            processbox.Items.Insert(0, processstring);
            while (head_onv == true)
            {
                Logger.WriteButtonLog("Printhead Clean", logButtonPath);
                processstring = Convert.ToString(DateTime.Now) + "프린트헤드 자동 퍼지 수행";
                processbox.Items.Insert(0, processstring);
                hcc = Task.Run(() => Head_cleaning2());
                //hcc = Task.Run(() => Head_Cleaning(0, 90, 245, 45, 100, 8, 50));
                await hcc;
                await Task.Run(() => Task.Delay(3600 * 1000).Wait());
            }

        }


        private async void headcleaning_Click(object sender, EventArgs e)
        {
            //헤드 클리닝 버튼 수행하였을때
            Logger.WriteButtonLog("Printhead Clean", logButtonPath);

            //var hcc = Task.Run(() => Head_Cleaning(0, 90, 245, 45, 100, 8, 50));
            var hcc = Task.Run(() => Head_cleaning2());
            await hcc;
            //비동기로 헤드 클리닝 수행 시키지만 wait  하여 동기

        }

        private async void suplp_Click(object sender, EventArgs e)
        {
            //파우더 공급 버튼 수행하였을때
            Logger.WriteButtonLog("Powder_Supply", logButtonPath);

            var pss = Task.Run(() => Powder_Supply(Convert.ToInt32(txtShutterOpenTime.Text), 0, 44));
            await pss;
            //비동기로 파우더 공급 하지만  wait 하여 동기
        }

        private void recoatwf_Click(object sender, EventArgs e)
        {
            //리코터 버튼 가동하였을때

            //파라미터 Reading
            Logger.WriteButtonLog("Recoating (Adv) Started with " + " Powder " + Convert.ToString(rpowder) + "/Roller " + Convert.ToString(rroller) + "/Zmove " + Convert.ToString(rzmove), logButtonPath);
            double step_rspeed = Convert.ToDouble(txtstepspeed.Text);
            double recoating_speed = Convert.ToDouble(txtrspeed.Text);
            //byte[] bldcbytecode = Bldc_byteget(Convert.ToInt32(txtrollerspeed.Text));
            int maxonrpm = Convert.ToInt32(txtmaxonrpm.Text) * 1000;
            string processstring = Convert.ToString(DateTime.Now) + "___리코팅 동작 가동 조건 :" + " Powder " + Convert.ToString(rpowder) + "/Roller " + Convert.ToString(rroller) + "/Zmove " + Convert.ToString(rzmove);
            processbox.Items.Insert(0, processstring);


            PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, 100, 100, 100);//X2축 이동 속도를 복귀 속도로 변경
            if (rzmove == true)
            {
                AxisMovement(Zaxis, -1);
                AxisMovement(X2axis, 680);                                //리코터 파우더 도포 시작 위치까지 이동
                AxisMovement(Zaxis, 1);
            }
            else
            {
                AxisMovement(X2axis, 680);                                //리코터 파우더 도포 시작 위치까지 이동
            }
            PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, recoating_speed, recoating_speed, recoating_speed);//파우더 도포 스피드로 변경
            if (rroller == true)
            {
                var rs = Task.Run(() => roller_Starter(X2axis, 610, small, maxonrpm));
                var pf2 = Task.Run(() => roller_Stopper(X2axis, 325, small));

            }
            if (rpowder == true)
            {
                var pf = Task.Run(() => BLDC_Stopper(X2axis, 420, small));
                Powder_Feed(step_rspeed);
            }
            AxisMovement(X2axis, 300);                                //리코터 베드 roller out MIN 방향으로 이동
            PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, 100, 100, 100);//X2축 이동 속도를 복귀 속도로 변경
            Thread.Sleep(100);
            processstring = Convert.ToString(DateTime.Now) + "___리코팅 동작 종료";
            processbox.Items.Insert(0, processstring);


        }


        private void camera_Click(object sender, EventArgs e)
        {
            //카메라 셔터 테스트용 
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);// (IO OUT 1번 상태를 1로 바꿈) - Shutter Send

            Thread.Sleep(100);
            Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);// (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

        }


        private void btnSerialRecieve_Click(object sender, EventArgs e)
        {
            //온도 시험 측정용 (기록됨)
            Logger.WriteButtonLog("Read Temperature", logButtonPath);
            TempRead();
            Templogging(0, 0);
            //byte[] sendByte = { 0x01, 0x01};
            //Serial485_Send(sendByte, sendByte.Length);
        }
        private void Serial_RS485_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //this.Invoke(new EventHandler(Serial485_Read));
        }

        private void MotionController_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.WriteButtonLog("PROGRAM END", logButtonPath);
            Printhead.Disconnect();
            //맥슨대기해제
            MaxBLDC.Disable();
            MaxBLDC.Disconnect();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
        private void btnXposToZero_Click(object sender, EventArgs e)
        {
            Logger.WriteButtonLog("X position to zero", logButtonPath);
            PaixMotion.SetCmd(Xaxis, 0);    //초기 커맨드 세팅 값   0
            PaixMotion.SetEnc(Xaxis, 0);    //초기 엔코더 세팅 값   0
        }

        private void btnX2posToZero_Click(object sender, EventArgs e)
        {
            Logger.WriteButtonLog("X2 position to zero", logButtonPath);
            PaixMotion.SetCmd(X2axis, 0);    //초기 커맨드 세팅 값   0
            PaixMotion.SetEnc(X2axis, 0);    //초기 엔코더 세팅 값   0
        }

        private void btnYposToZero_Click(object sender, EventArgs e)
        {
            Logger.WriteButtonLog("Y position to zero", logButtonPath);
            PaixMotion.SetCmd(Yaxis, 0);    //초기 커맨드 세팅 값   0
            PaixMotion.SetEnc(Yaxis, 0);    //초기 엔코더 세팅 값   0
        }

        private void btnZposToZero_Click(object sender, EventArgs e)
        {
            Logger.WriteButtonLog("Z position to zero", logButtonPath);
            PaixMotion.SetCmd(Zaxis, 0);    //초기 커맨드 세팅 값   0
            PaixMotion.SetEnc(Zaxis, 0);    //초기 엔코더 세팅 값   0
        }

        private async Task step_on(double spd, int dur)
        {
            //비동기로 stepper모터를 동작시키는 함수
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, Raxis, rStartSpeed, rAccelSpeed, rDeccelSpeed, spd * 12);
            Paix_MotionController.NMC2.nmc_SetSCurveSpeed(devID2, R2axis, rStartSpeed, rAccelSpeed, rDeccelSpeed, spd * 14);
            Paix_MotionController.NMC2.nmc_JogMove(devID2, Raxis, CCW);
            Paix_MotionController.NMC2.nmc_JogMove(devID2, R2axis, CCW);
            if (dur > 0)
            {
                await Task.Run(() => Task.Delay(dur).Wait());
                Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
                Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
            }
        }


        private async void btnstepstart_Click(object sender, EventArgs e)
        {
            //stepper 모터 동작 버튼
            Logger.WriteButtonLog("Step Start Button with speed : "+Convert.ToString(txtstepspeed.Text)+" duration: "+Convert.ToString(txtsteptime.Text), logButtonPath);
            btnstepstart.BackColor = Color.Red;
            btnstepstart.Enabled = false;
            double spd = Convert.ToDouble(txtstepspeed.Text);
            int dur = Convert.ToInt16(txtsteptime.Text) * 1000;//maxon 관련 보정
            Console.WriteLine(DateTime.Now);
            string processstring = Convert.ToString(DateTime.Now) + "___파우더 공급 스텝모터 가동 -> " + Convert.ToString(DateTime.Now.AddMilliseconds(Convert.ToDouble(dur))) + " 종료";
            if (dur == 0)//duration이 0일경우 연속동작
            {
                processstring = Convert.ToString(DateTime.Now) + "___파우더 공급 스텝모터 가동 - " + "연속동작";
            }
            processbox.Items.Insert(0, processstring);
            await step_on(spd, dur);
            Console.WriteLine(DateTime.Now);
            btnstepstart.BackColor = SystemColors.ControlDarkDark;
            btnstepstart.Enabled = true;
        }

        private void btnstepstop_Click(object sender, EventArgs e)
        {
            //stepper 수동 정지 동작 버튼
            Logger.WriteButtonLog("Step Stop Button", logButtonPath);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
            string processstring = Convert.ToString(DateTime.Now) + "___파우더 공급 스텝모터 가동 중단";
            processbox.Items.Insert(0, processstring);
        }
        private byte[] Bldc_byteget(int speed)
        {
            //모터뱅크 BLDC를 사용할때 진행했던 암호화 함수, 현재 사용하지 않음.
            //레거시 코드이나 문제 있을수 있어 유지
            string spd1 = "00";
            string spd2 = "00";
            string spdstring = (speed * 10).ToString("X");
            //Console.WriteLine(spdstring);
            if (spdstring.Length < 3)
            {
                spd1 = "00";
                spd2 = spdstring.PadLeft(2).Replace(" ", "0");
            }
            else if (spdstring.Length == 3)
            {
                spd1 = "0" + spdstring[0];
                spd2 = spdstring.Substring(1, 2);
            }
            else if (spdstring.Length == 4)
            {
                spd1 = spdstring.Substring(0, 2);
                spd2 = spdstring.Substring(2, 2);
            }
            else if (spdstring.Length == 5)
            {
                spd1 = spdstring.Substring(1, 2);
                spd2 = spdstring.Substring(3, 2);
            }
            //Console.WriteLine(spd1);
            //Console.WriteLine(spd2);
            string sumofdec = (Convert.ToInt32(spd1, 16) + Convert.ToInt32(spd2, 16) + 30).ToString("X").PadLeft(6).Substring(4, 2);
            //Console.WriteLine(sumofdec);
            string f = Convert.ToInt32(Convert.ToString(Convert.ToInt32(sumofdec.Substring(0, 1), 16), 2).PadLeft(4, '0').Replace('0', '2').Replace('1', '3').Replace('2', '1').Replace('3', '0'), 2).ToString("X") + Convert.ToInt32(Convert.ToString(Convert.ToInt32(sumofdec.Substring(1, 1), 16), 2).PadLeft(4, '0').Replace('0', '2').Replace('1', '3').Replace('2', '1').Replace('3', '0'), 2).ToString("X");
            //Console.WriteLine(f);
            string pstring = "FFFE0106" + f + "0300" + spd1 + spd2 + "14";
            byte[] result = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                result[i] = Convert.ToByte(pstring.Substring(i * 2, 2), 16);
            }
            return result;
        }

        private string Bldc_speedget(byte[] bldcbytecode)
        {
            //모터뱅크 BLDC를 사용할때 진행했던 암호화 함수, 현재 사용하지 않음.
            string bldcspeed = "";

            string sp1 = bldcbytecode[7].ToString("X");
            Console.WriteLine(sp1);
            sp1.PadLeft(2).Replace(' ', '0');
            Console.WriteLine(sp1);
            string sp2 = bldcbytecode[8].ToString("X");
            Console.WriteLine(sp2);
            sp2.PadLeft(2).Replace(' ', '0');
            Console.WriteLine(sp2);
            string spbtye = sp1 + sp2;
            Console.WriteLine(spbtye);


            int spdi = Convert.ToInt32(spbtye, 16) / 10;
            Console.WriteLine(spdi.ToString());
            bldcspeed = Convert.ToString(spdi);
            Console.WriteLine(bldcspeed);
            return bldcspeed;
        }
        private async Task roller_on(int rpm, int dur)
        {
            //맥슨 Roller 컨트롤용 함수
            maxon_run(rpm*1000, false);
            //BLDC_RUN_1_rev(bldcbytecode);
            if (dur > 0)
            {
                await Task.Run(() => Task.Delay(dur).Wait());
                maxon_stop();
                //BLDC_STOP_1();
            }

        }
        private async void btnrollerstart_Click(object sender, EventArgs e)
        {
            //맥슨 롤러 시작버튼 함수
            Logger.WriteButtonLog("Roller Start Button with speed : " + Convert.ToString(txtrollerspeed.Text) + " duration: " + Convert.ToString(txtrollertime.Text), logButtonPath);
            btnrollerstart.BackColor = Color.Red;
            btnrollerstart.Enabled = false;
            //byte[] bldcbytecode = Bldc_byteget(Convert.ToInt32(txtrollerspeed.Text));
            int dur = Convert.ToInt16(txtrollertime.Text) * 1000;
            string processstring = Convert.ToString(DateTime.Now) + "___평탄화 롤러 가동 -> " + Convert.ToString(DateTime.Now.AddMilliseconds(Convert.ToDouble(dur))) + " 종료";
            if (dur == 0)
            {
                processstring = Convert.ToString(DateTime.Now) + "___평탄화 롤러 가동 - " + "연속동작";
            }
            processbox.Items.Insert(0, processstring);
            await roller_on(Convert.ToInt32(txtrollerspeed.Text), dur);
            btnrollerstart.BackColor = SystemColors.ControlDarkDark;
            btnrollerstart.Enabled = true;

            //MaxonMotorClass added
            
            //MaxBLDC.Connect();
            //MaxBLDC.Enable();
            //MaxBLDC.MotorMove_CW_CCW(Convert.ToInt32(txtmaxonrpm.Text)*1000, 10000, 10000, false);
            

        }

        private void btnrollerstop_Click(object sender, EventArgs e)
        {
            //맥슨 롤러 정지용 함수
            Logger.WriteButtonLog("Roller Stop Button", logButtonPath);
            //BLDC_STOP_1();
            string processstring = Convert.ToString(DateTime.Now) + "___평탄화 롤러 가동 중단";
            processbox.Items.Insert(0, processstring);

            //MaxonMotorClass Added
            
            MaxBLDC.MotorStop();
            //MaxBLDC.Disable();
            //MaxBLDC.Disconnect();
            
            
        }

        private async void btnstepjog_MouseDown(object sender, MouseEventArgs e)
        {
            //stepper 조그 컨트롤용
            Logger.WriteButtonLog("Step jog control start with speed : "+Convert.ToString(txtstepspeed.Text), logButtonPath);
            btnstepjog.BackColor = Color.Red;
            double spd = Convert.ToDouble(txtstepspeed.Text);
            string processstring = Convert.ToString(DateTime.Now) + "___스텝모터 조그 컨트롤 시작";
            processbox.Items.Insert(0, processstring);
            step_on(spd, 0);
        }

        private void btnstepjog_MouseUp(object sender, MouseEventArgs e)
        {
            //stepper 조그 컨트롤용
            Logger.WriteButtonLog("Step jog control end with speed : " + Convert.ToString(txtstepspeed.Text), logButtonPath);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, Raxis);
            Paix_MotionController.NMC2.nmc_SuddenStop(devID2, R2axis);
            string processstring = Convert.ToString(DateTime.Now) + "___스텝모터 조그 컨트롤 종료";
            processbox.Items.Insert(0, processstring);
            btnstepjog.BackColor = SystemColors.ControlDarkDark;
        }

        private async void btnrollerjog_MouseDown(object sender, MouseEventArgs e)
        {
            //맥슨 롤러 조그 컨트롤용
            Logger.WriteButtonLog("Roller jog control start with speed : " + Convert.ToString(txtrollerspeed.Text), logButtonPath);
            btnrollerjog.BackColor = Color.Red;
            //byte[] bldcbytecode = Bldc_byteget(Convert.ToInt32(txtrollerspeed.Text));
            string processstring = Convert.ToString(DateTime.Now) + "___롤러 조그 컨트롤 시작";
            processbox.Items.Insert(0, processstring);
            roller_on(Convert.ToInt32(txtrollerspeed.Text), Convert.ToInt32(txtrollertime.Text));
            
            //MaxonMotor Added.
            
            // if true, CW & if false, CCW
            //MaxBLDC.MotorMove_CW_CCW(Convert.ToInt32(txtrollerspeed.Text)*1000, 10000, 10000, false);
            
        }

        private void btnrollerjog_MouseUp(object sender, MouseEventArgs e)
        {
            //맥슨 롤러 조그 컨트롤용
            Logger.WriteButtonLog("Roller jog control end with speed : " + Convert.ToString(txtrollerspeed.Text), logButtonPath);
            //BLDC_STOP_1();
            string processstring = Convert.ToString(DateTime.Now) + "___롤러 조그 컨트롤 종료";
            processbox.Items.Insert(0, processstring);
            btnrollerjog.BackColor = SystemColors.ControlDarkDark;

            //MaxonMotor Added.
            
            MaxBLDC.MotorStop();
            
        }

        private void PowderChk_CheckStateChanged(object sender, EventArgs e)
        {
            //리코팅시 파우더 공급 여부에 대한 체크박스 동작용
            if (PowderChk.Checked == true)
            {
                //PowderChk.BackColor = Color.DarkGreen;
                //PowderChk.ForeColor = Color.Black;
                rpowder = true;
                Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "checked.png")));
                Size resize = new Size(26, 22);
                Bitmap rimage = new Bitmap(oimage, resize);
                picpowderchk.Image = rimage;
            }
            else
            {
                //PowderChk.BackColor = Color.DarkRed;
                //PowderChk.ForeColor = Color.White;
                rpowder = false;
                Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "unchecked.png")));
                Size resize = new Size(22, 22);
                Bitmap rimage = new Bitmap(oimage, resize);
                picpowderchk.Image = rimage;
            }
        }

        private void RollerChk_CheckStateChanged(object sender, EventArgs e)
        {
            //리코팅시 롤러 동작 여부에 대한 체크박스 동작용
            if (RollerChk.Checked == true)
            {
                //RollerChk.BackColor = Color.DarkGreen;
                //RollerChk.ForeColor = Color.Black;
                rroller = true;
                Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "checked.png")));
                Size resize = new Size(26, 22);
                Bitmap rimage = new Bitmap(oimage, resize);
                picrollerchk.Image = rimage;
            }
            else
            {
                //RollerChk.BackColor = Color.DarkRed;
                //RollerChk.ForeColor = Color.White;
                rroller = false;
                Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "unchecked.png")));
                Size resize = new Size(22, 22);
                Bitmap rimage = new Bitmap(oimage, resize);
                picrollerchk.Image = rimage;
            }
        }

        private void ZmoveChk_CheckedChanged(object sender, EventArgs e)
        {
            if (ZmoveChk.Checked == true)
            {
                //리코팅시 Z축 이동 여부에 대한 체크박스 동작용
                //ZmoveChk.BackColor = Color.DarkGreen;
                //ZmoveChk.ForeColor = Color.Black;
                rzmove = true;
                Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "checked.png")));
                Size resize = new Size(26, 22);
                Bitmap rimage = new Bitmap(oimage, resize);
                piczmovechk.Image = rimage;
            }
            else
            {
                //ZmoveChk.BackColor = Color.DarkRed;
                //ZmoveChk.ForeColor = Color.White;
                rzmove = false;
                Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "unchecked.png")));
                Size resize = new Size(22, 22);
                Bitmap rimage = new Bitmap(oimage, resize);
                piczmovechk.Image = rimage;
            }
        }

        private void btnparasave_Click(object sender, EventArgs e)
        {
            //파라미터 세이브용 (수정 필요하나, 실제적으로 사용하지 않음.)
            Logger.WriteButtonLog("Save Parameter", logButtonPath);
            string OpenFilePath = System.Environment.CurrentDirectory;
            ParameterSDialog.InitialDirectory = OpenFilePath;
            ParameterSDialog.Title = "파라미터 파일 저장 경로 선택";
            ParameterSDialog.Filter = "cfg files (*.cfg)|*.cfg";
            ParameterSDialog.FileName = "";
            DialogResult dr = ParameterSDialog.ShowDialog();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (ParameterSDialog.FileName.Length > 1)
            {
                //Console.WriteLine("IN");
                //string[] bbstrings = ;
                string savestring = "";
                string[] CatString = { "double Layer_Thickness", "short Dwell_Time", "double Temp_Limit", "double Temp_Range", "string Roller_Speed", "double Stepper_Speed", "double Recoating_Speed", "string Binder_Value" };
                foreach (string Cat in CatString)
                {
                    savestring = savestring + Cat + " = ";
                    if (Array.IndexOf(CatString, Cat) == 0)
                    {
                        savestring = savestring + txtlayerthickness.Text + ";//레이어 두께 (um)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 1)
                    {
                        savestring = savestring + txtdwelltime.Text + ";//바인더 분사 후 대기시간 (sec)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 2)
                    {
                        savestring = savestring + txttemplimit.Text + ";//파우더 베드 기준 온도(℃)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 3)
                    {
                        savestring = savestring + txttemprange.Text + ";//온도 허용 오차치(℃)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 4)
                    {
                        savestring = savestring + txtrollerspeed.Text + ";//Fine롤러 회전 속도(RPM)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 5)
                    {
                        savestring = savestring + txtstepper.Text + ";//파우더 공급 속도(RPM)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 6)
                    {
                        savestring = savestring + txtprspeed.Text + ";//파우더 도포 속도(mm/s)\n";
                    }
                    else if (Array.IndexOf(CatString, Cat) == 7)
                    {
                        string[] bindercut = { txtb1.Text, txtb2.Text, txtb3.Text, txtb4.Text, txtb5.Text, txtb6.Text };
                        savestring = savestring + "0 0 " + (String.Join(" ", bindercut)) + ";//바인더 값 (0-7)";
                    }
                }
                File.WriteAllText(ParameterSDialog.FileName, savestring);
            }
        }

        private async void btnFloop_Click(object sender, EventArgs e)
        {
            //프린팅 시작 전 자동 파우더 형성 함수
            Logger.WriteButtonLog("F loop button pressed with loop : "+Convert.ToString(txtFloop.Text), logButtonPath);
            for (int i = 0; i < Convert.ToInt32(txtFloop.Text); i++)
            {
                string processstring = Convert.ToString(DateTime.Now) + "___프린팅 전 파우더 채우기 작업 중 (" + Convert.ToString(i + 1) + " / " + txtFloop.Text + ")";
                processbox.Items.Insert(0, processstring);
                var pss = Task.Run(() => Powder_Supply(Convert.ToInt32(txtShutterOpenTime.Text), 0, 44));
                await pss;
                double step_rspeed = Convert.ToDouble(txtstepper.Text);
                double recoating_speed = Convert.ToDouble(txtrspeed.Text);
                //byte[] bldcbytecode = Bldc_byteget(Convert.ToInt32(txtroller.Text));
                int maxonrpm = Convert.ToInt32(txtmaxonrpm.Text) * 1000;
                PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, 100, 100, 100);//X2축 이동 속도를 복귀 속도로 변경
                AxisMovement(Zaxis, -1);
                AxisMovement(X2axis, 680);                                //리코터 파우더 도포 시작 위치까지 이동
                AxisMovement(Zaxis, 1);
                PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, recoating_speed, recoating_speed, recoating_speed);//파우더 도포 스피드로 변경
                var rs = Task.Run(() => roller_Starter(X2axis, 610, small, maxonrpm));
                var pf2 = Task.Run(() => roller_Stopper(X2axis, 325, small));
                var pf = Task.Run(() => BLDC_Stopper(X2axis, 420, small));
                Powder_Feed(step_rspeed);
                AxisMovement(X2axis, 300);                                //리코터 베드 roller out MIN 방향으로 이동
                PaixMotion.SetSCurveSpeed(X2axis, iStartSpeed, 100, 100, 100);//X2축 이동 속도를 복귀 속도로 변경
                Thread.Sleep(100);
                Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 1);// (IO OUT 1번 상태를 1로 바꿈) - Shutter Send
                Thread.Sleep(100);
                Paix_MotionController.NMC2.nmc_SetMDIOOutPin(devID, 1, 0);// (IO OUT 1번 상태를 0로 바꿈) - Shutter Send End

            }
            string Aprocessstring = Convert.ToString(DateTime.Now) + "___프린팅 전 파우더 채우기 작업 완료";
            processbox.Items.Insert(0, Aprocessstring);


        }

        private async void btnallhome_Click(object sender, EventArgs e)
        {
            //모든 축 homming 진행
            Logger.WriteButtonLog("All Home in Auto mode", logButtonPath);
            string processstring = Convert.ToString(DateTime.Now) + "___모든 축 Homing 시작";
            processbox.Items.Insert(0, processstring);
            short[] axislist = { X2axis, Yaxis, Xaxis, Zaxis };
            foreach (short axis in axislist)
            {
                short HomeStatus = NmcData.nNear[axis];
                if (HomeStatus == OFF)
                {
                    PaixMotion.HomeMove(axis, HomeMode);
                    while (HomeStatus == OFF)
                    {
                        await Task.Delay(1000);
                        HomeStatus = NmcData.nNear[axis];
                    }
                    await Task.Delay(4500);
                }
                PaixMotion.SetCmd(axis, 0);    //초기 커맨드 세팅 값   0
                PaixMotion.SetEnc(axis, 0);    //초기 엔코더 세팅 값   0
            }
            Logger.WriteButtonLog("All Axis Homing", logButtonPath);
            Logger.WriteButtonLog("All position to zero", logButtonPath);
            processstring = Convert.ToString(DateTime.Now) + "___모든 축 Homing 완료";
            processbox.Items.Insert(0, processstring);
        }

        private async void btnbedin_Click(object sender, EventArgs e)
        {
            //Bed in 동작 자동화 함수
            Logger.WriteButtonLog("Bed in button pressed", logButtonPath);
            string processstring = Convert.ToString(DateTime.Now) + "___Bed IN 진행중";
            PaixMotion.SetSCurveSpeed(Zaxis, iStartSpeed, 1, 1, 2);
            processbox.Items.Insert(0, processstring);
            await Task.Delay(1000);
            Outpin(4, ON);
            await Task.Delay(1000);
            Outpin(4, OFF);
            await Task.Delay(1000);
            Outpin(4, ON);
            await Task.Delay(1000);
            AxisMovement(Zaxis, 20);
            await Task.Delay(1000);
            PaixMotion.SetSCurveSpeed(Zaxis, iStartSpeed, 1, 1, 1);
            for (int i = 0; i < 15; i++)
            {
                AxisMovement(Zaxis, 2);
                await Task.Delay(1000);
                AxisMovement(Zaxis, -1);
                await Task.Delay(1000);
            }


            PaixMotion.SetSCurveSpeed(Zaxis, iStartSpeed, 1, 1, 2);
            await Task.Delay(1000);
            Outpin(4, OFF);
            processstring = Convert.ToString(DateTime.Now) + "___Bed IN 완료";
            processbox.Items.Insert(0, processstring);
        }

        private async void btnbedout_Click(object sender, EventArgs e)
        {
            //Bed out 동작 자동화 함수
            Logger.WriteButtonLog("Bed out button pressed", logButtonPath);
            string processstring = Convert.ToString(DateTime.Now) + "___Bed OUT 진행중";
            processbox.Items.Insert(0, processstring);
            short zHomeStatus = NmcData.nNear[Zaxis];
            while (true)
            {
                if (zHomeStatus == ON)
                {
                    processstring = Convert.ToString(DateTime.Now) + "___Clamp 분리 위치 도달 완료";
                    processbox.Items.Insert(0, processstring);
                    break;
                }
                else if (zHomeStatus == OFF)
                {
                    PaixMotion.HomeMove(Zaxis, HomeMode);
                    await Task.Delay(30000);
                }
            }
            AxisMovement(X2axis, 820);
            Outpin(4, ON);
            await Task.Delay(1000);
            Outpin(4, OFF);
            await Task.Delay(1000);
            Outpin(4, ON);
            await Task.Delay(1000);
            PaixMotion.SetSCurveSpeed(Zaxis, iStartSpeed, 1, 1, 1);
            processstring = Convert.ToString(DateTime.Now) + "___Clamp 분리 시작";
            processbox.Items.Insert(0, processstring);
            short zlimitStatus = NmcData.nMLimit[Zaxis];
            while (zlimitStatus == OFF)
            {
                AxisMovement(Zaxis, -0.5);
                zlimitStatus = NmcData.nMLimit[Zaxis];
                await Task.Delay(400);
            }
            Outpin(4, OFF);
            await Task.Delay(1000);
            processstring = Convert.ToString(DateTime.Now) + "___Bed OUT 완료";
            processbox.Items.Insert(0, processstring);
        }

        private async void selectedimgshow(short imgnum)
        {
            //선택된 이미지를 보여주는 함수
            string item_name = listImage.Items[Convert.ToInt16(imgnum)].ToString();
            item_name = Path.Combine("Images", item_name);
            //Console.WriteLine(Path.GetFullPath(item_name));
            Bitmap oimage = new Bitmap(Path.GetFullPath(item_name));
            //oimage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            int owidth = oimage.Width;
            //Console.WriteLine(owidth);
            int oheight = oimage.Height;
            //Console.WriteLine(oheight);
            int width = imagebox.Width;
            int height = imagebox.Height;
            Size resize = new Size(height, width);
            if (owidth > oheight)
            {
                //Console.WriteLine("1");
                double aratio = Convert.ToDouble(owidth) / Convert.ToDouble(oheight);
                //Console.WriteLine(aratio);
                width = Convert.ToInt32(height * aratio);
                //Console.WriteLine(width);
                resize.Width = width;
                resize.Height = height;
            }
            else
            {
                //Console.WriteLine("2");
                double aratio = Convert.ToDouble(oheight) / Convert.ToDouble(owidth);
                //Console.WriteLine(aratio);
                height = Convert.ToInt32(width * aratio);
                //Console.WriteLine(width);
                resize.Width = width;
                resize.Height = height;
            }
            Bitmap rimage = new Bitmap(oimage, resize);
            imagebox.Size = resize;
            imagebox.Image = rimage;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //개발용
            Paix_MotionController.NMC2.nmc_SetMDIOOutputTog(devID, 0);
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            //개발용
            //Paix_MotionController.NMC2.nmc_SetMDIOOutputTog(devID, 4);

            Console.WriteLine(Currentlayer.ToString());
            if (Currentlayer == 0)
            {
                DateTime nowtime = DateTime.Now;
                layertimes.Add(nowtime);
                processbox.Items.Insert(0, Convert.ToString(nowtime));
                Currentlayer++;
            }
            else
            {
                DateTime nowtime = DateTime.Now;
                layertimes.Add(nowtime);
                double sumtime = 0;
                for (int i = 1; i < layertimes.Count; i++)
                {
                    sumtime = sumtime + Math.Log(Convert.ToDouble(i))*Convert.ToDouble(layertimes[i].Subtract(layertimes[i - 1]).TotalSeconds);
                }
                processbox.Items.Insert(0, Convert.ToString(nowtime));
                processbox.Items.Insert(0, "Average Time : " + Convert.ToString((double)sumtime / layertimes.Count));
                Console.WriteLine(Convert.ToInt32(((double)sumtime / layertimes.Count) * listImage.Items.Count));
                processbox.Items.Insert(0, "ETA : " + Convert.ToString(DateTime.Now.AddSeconds(Convert.ToInt32((double)sumtime / ((layertimes.Count)*(Math.Log((double)layertimes.Count))) -(double)layertimes.Count) * (listImage.Items.Count - layertimes.Count))));
                Currentlayer++;
            }



            /*
            printprogress.Style = ProgressBarStyle.Continuous;
            printprogress.Minimum = 0;
            printprogress.Maximum = 350;
            printprogress.Step = 1;
            imgtrackbar.Minimum = 0;
            imgtrackbar.Maximum = 349;
            for (int i = 0; i < 350; i++)
            {
                double perc =100.0*Convert.ToDouble(i) / 350.0;
                percentprogress.Text = Convert.ToString(Math.Round(perc, 1))+"%";
                //Console.WriteLine(percentprogress.Text);
                imgtrackbar.Value = i;
                printprogress.PerformStep();
                //printprogress.Text = Convert.ToString(Math.Round(perc, 1)) + "%";
                await Task.Delay(10); 
            }
            */


        }

        private async void imgtrackbar_Scroll(object sender, EventArgs e)
        {
            //이미지 스크롤용
            if (listImage.Items.Count > 0)
            {
                imgtrackbar.Minimum = 0;
                imgtrackbar.Maximum = listImage.Items.Count - 1;
                selectedimgshow(Convert.ToInt16(imgtrackbar.Value));
            }

        }

        private void updatelbllayer(object sender, EventArgs e)
        {
            //레이어 증가에 따른 이미지 변화
            lbllayer.Text = (imgtrackbar.Value + 1).ToString();
        }

        private void PModChanged(object sender, EventArgs e)
        {
            //공정변수 변경용 함수
            if (ModChk.Checked == true)
            {
                txtlayerthickness.Enabled = true;
                txtb1.Enabled = true;
                txtb2.Enabled = true;
                txtb3.Enabled = true;
                txtb4.Enabled = true;
                txtb5.Enabled = true;
                txtb6.Enabled = true;
                txttemplimit.Enabled = true;
                txttemprange.Enabled = true;
                txtdwelltime.Enabled = true;
                txtstepper.Enabled = true;
                txtroller.Enabled = true;
                txtprspeed.Enabled = true;
            }
            else
            {
                txtlayerthickness.Enabled = false;
                txtb1.Enabled = false;
                txtb2.Enabled = false;
                txtb3.Enabled = false;
                txtb4.Enabled = false;
                txtb5.Enabled = false;
                txtb6.Enabled = false;
                txttemplimit.Enabled = false;
                txttemprange.Enabled = false;
                txtdwelltime.Enabled = false;
                txtstepper.Enabled = false;
                txtroller.Enabled = false;
                txtprspeed.Enabled = false;
            }
        }
        private void calcload(int load, short axis)
        {
            //축 부하 측정용 함수
            load = Math.Abs(load);
            int perload = 0;
            switch (axis)
            {
                case 0:
                    perload = load / 20000;
                    X1load.Text = Convert.ToString(perload) + "%";
                    coloringload(perload, X1load);
                    break;
                case 1:
                    perload = load / 10000;
                    X2load.Text = Convert.ToString(perload) + "%";
                    coloringload(perload, X2load);
                    break;
                case 2:
                    perload = load / 1000;
                    Zload.Text = Convert.ToString(perload) + "%";
                    coloringload(perload, Zload);
                    break;
                case 3:
                    perload = load / 2000;
                    Yload.Text = Convert.ToString(perload) + "%";
                    coloringload(perload, Yload);
                    break;
            }

        }
        private void coloringload(int perload, Button target)
        {
            //축 부하에 따른 색상 표시용 함수
            if (perload < 1)
            {
                target.BackColor = SystemColors.Info;
            }
            else if (perload < 20)
            {
                target.BackColor = Color.LightBlue;
            }
            else if (perload < 50)
            {
                target.BackColor = Color.LightGreen;
            }
            else if (perload < 90)
            {
                target.BackColor = Color.OrangeRed;
            }
            else
            {
                target.BackColor = Color.Red;
                PaixMotion.EmergencyStop();
                string processstring = Convert.ToString(DateTime.Now) + "___축 부하로 인한 비상정지. 대상 : " + Convert.ToString(target.Name.Replace("load", "축"));
                processbox.Items.Insert(0, processstring);
            }

        }
        private void showmachine(double Xpos, double X2pos, double Ypos, double Zpos)
        {
            //기기 상태 그래픽 표시용 함수
            showpnlX.Location = new Point(Convert.ToInt32(Xpos/4), 15);
            showpnlX2.Location = new Point(250-Convert.ToInt32(X2pos / 4), 15);
            showpnlY.Location = new Point(0 , Convert.ToInt32(Ypos / 4));
            showpnlpowder.Location = new Point(5, 20);
            showpnlir.Location = new Point(250 - Convert.ToInt32(X2pos / 4)+30, 35);
            showpnlroller.Location = new Point(250 - Convert.ToInt32(X2pos / 4) - 10, 35);
            showpnlZ.Location = new Point(340, 100-Convert.ToInt32(Zpos/2));
            showpnlZ.Size = new Size(10,15+Convert.ToInt32(Zpos/2));
            showpnlclamp.Location = new Point(330, 90 - Convert.ToInt32(Zpos / 2));

        }
        private void showio(short powder, short ir, short clamp)
        {
            //기기 상태 (IO 표시용 함수)
            if (ir==1)
            {
                showpnlir.BackColor = Color.Crimson;
            }
            else
            {
                showpnlir.BackColor = Color.Khaki;
            }
            if (clamp == 0)
            {
                showpnlclampin.BackColor = Color.Crimson;
                if (NmcData.dEnc[Zaxis] > 0)
                {
                    showpnlplate.Location = new Point(320, 85 - Convert.ToInt32(NmcData.dEnc[Zaxis] / 2));
                }
            }
            else
            {
                showpnlclampin.BackColor = Color.LimeGreen;
                
            }
        }
        private void picinitialize()
        {
            //이미지 창 관련 초기화 함수
            Bitmap oimage = new Bitmap(Path.GetFullPath(Path.Combine("Image", "unchecked.png"))); //체크되지 않은 기본 이미지를 불러옴
            picpowderchk.Image = oimage;//이미지 내부 할당
            Size resize = new Size(22, 22);//이미지 리사이즈 지정
            Bitmap rimage = new Bitmap(oimage, resize); //이미지 리사이징 진행
            picpowderchk.Image = rimage;//파우더 관련 체크 이미지 표시
            picrollerchk.Image = rimage;//롤러 관련 체크 이미지 표시
            piczmovechk.Image = rimage;//Z축 움직임 관련 체크 이미지 표시
        }
        private void showstep(short r1,short r2)
        {
            //step 동작 상태를 나타내주는 함수
            //Console.WriteLine(r2);
            if (r1*r2 >0)
            {
                showpnlpowder.BackColor = Color.SpringGreen;
            }
            else
            {
                showpnlpowder.BackColor = Color.Aquamarine;
            }
            
        }

        private void pnlIO_Out4_Click(object sender, EventArgs e)
        {
            Paix_MotionController.NMC2.nmc_SetMDIOOutputTog(devID, 4);
        }
        private void log_printingsettings()
        {
            //프린트 세팅 로깅 함수
            string savestring = "";
            string[] CatString = { "double Layer_Thickness", "short Dwell_Time", "double Temp_Limit", "double Temp_Range", "string Roller_Speed", "double Stepper_Speed", "double Recoating_Speed", "string Binder_Value" };
            foreach (string Cat in CatString)
            {
                savestring = savestring + Cat + " = ";
                if (Array.IndexOf(CatString, Cat) == 0)
                {
                    savestring = savestring + txtlayerthickness.Text + ";//레이어 두께 (um)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 1)
                {
                    savestring = savestring + txtdwelltime.Text + ";//바인더 분사 후 대기시간 (sec)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 2)
                {
                    savestring = savestring + txttemplimit.Text + ";//파우더 베드 기준 온도(℃)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 3)
                {
                    savestring = savestring + txttemprange.Text + ";//온도 허용 오차치(℃)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 4)
                {
                    savestring = savestring + txtrollerspeed.Text + ";//Fine롤러 회전 속도(RPM)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 5)
                {
                    savestring = savestring + txtstepper.Text + ";//파우더 공급 속도(RPM)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 6)
                {
                    savestring = savestring + txtprspeed.Text + ";//파우더 도포 속도(mm/s)\n";
                }
                else if (Array.IndexOf(CatString, Cat) == 7)
                {
                    string[] bindercut = { txtb1.Text, txtb2.Text, txtb3.Text, txtb4.Text, txtb5.Text, txtb6.Text };
                    savestring = savestring + "0 0 " + (String.Join(" ", bindercut)) + ";//바인더 값 (0-7)";
                }
            }
            Logger.WriteButtonLog(savestring,logButtonPath);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Printhead.WriteWaveForm_dh93();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Printhead.cleanstart1();
            Printhead.cleanstart2();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Printhead.cleanstop();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Printhead.WriteWaveForm_h884();
        }

        private async void Head_on_Click(object sender, EventArgs e)
        {
            //헤드 주기 세척 관련 함수
            head_onv =true;
            string processstring = Convert.ToString(DateTime.Now) + "프린트헤드 정기 퍼지 수행 시작";
            processbox.Items.Insert(0, processstring);
            while (head_onv == true)
            {
                Logger.WriteButtonLog("Printhead Clean", logButtonPath);
                processstring = Convert.ToString(DateTime.Now) + "프린트헤드 자동 퍼지 수행";
                processbox.Items.Insert(0, processstring);
                var hcc = Task.Run(() => Head_cleaning2());
                //var hcc = Task.Run(() => Head_Cleaning(0, 90, 245, 45, 100, 8, 50));
                await hcc;
                await Task.Run(() => Task.Delay(1800 * 1000).Wait());
            }
        }

        private void Head_off_Click(object sender, EventArgs e)
        {
            //헤드 주기 세척 정지 함수
            string processstring = Convert.ToString(DateTime.Now) + "프린트헤드 정기 퍼지 수행 종료";
            processbox.Items.Insert(0, processstring);
            head_onv = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Outpin(5, ON);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Outpin(5, OFF);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Outpin(6, ON);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Outpin(6, OFF);
        }
    }
}
