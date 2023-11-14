using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Paix_MotionController;

namespace BinderJetMotionControllerVer._1
{
    class PaixMotion
    {
        NMC2.NMCAXESEXPR NmcData = new NMC2.NMCAXESEXPR();


        bool m_bIsOpen;
        bool m_bIsOpen2;
        short m_nDev_no;
        short m_nDev_no2;
        internal const short GROUP = 0;
        bool printStop = false;
        internal void AstekMotion()
        {
            NMC2.NMCEQUIPLIST NmcIPList = new NMC2.NMCEQUIPLIST();
            m_bIsOpen = false;
            m_nDev_no = Convert.ToInt16(NmcIPList.lIp);
            m_nDev_no2 = Convert.ToInt16(NmcIPList.lIp);
        }

        internal void setoutpintime(short ion,short time)
        {
            NMC2.nmc_SetOutLimitTimePin(200, 0, ion, 1, time*1000);
        }
        internal bool Open(short dev_no)
        {
            m_nDev_no = dev_no;

            //Close();
            NMC2.nmc_SetIPAddress(dev_no, 192, 168, 1);
            // 방화벽을 확인해 주십시요.
            if (NMC2.nmc_PingCheck(dev_no, 10) != 0)
            {
                MessageBox.Show("Ping Check Error");
                return false;
            }
            if (NMC2.nmc_OpenDevice(m_nDev_no) == 0)
                m_bIsOpen = true;
            else
                m_bIsOpen = false;

            return m_bIsOpen;
        }

        internal bool Open2(short dev_no)
        {
            m_nDev_no2 = dev_no;

            //Close();
            NMC2.nmc_SetIPAddress(dev_no, 192, 168, 1);
            // 방화벽을 확인해 주십시요.
            if (NMC2.nmc_PingCheck(dev_no, 10) != 0)
            {
                MessageBox.Show("Ping Check Error");
                return false;
            }
            if (NMC2.nmc_OpenDevice(m_nDev_no2) == 0)
                m_bIsOpen2 = true;
            else
                m_bIsOpen2 = false;

            return m_bIsOpen2;
        }

        internal bool Close()
        {
            NMC2.nmc_CloseDevice(m_nDev_no);
            NMC2.nmc_CloseDevice(m_nDev_no2);
            m_bIsOpen = false;
            m_bIsOpen2 = false;

            return true;
        }

        internal bool SetSpeedPPS(short nAxis, double dStart, double dAcc, double dDec, double dMax)
        {

            short nRet = NMC2.nmc_SetSpeed(m_nDev_no, nAxis, dStart, dAcc, dDec, dMax);
            switch( nRet )
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool SetSCurveSpeed(short nAxis, double dStart, double dAcc, double dDec, double dDrive)
        {
            short nRet = NMC2.nmc_SetSCurveSpeed(m_nDev_no, nAxis, dStart, dAcc, dDec, dDrive);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool RelMove( short nAxis, double fDist)
        {
            int nRet;
            nRet = NMC2.nmc_RelMove(m_nDev_no, nAxis, fDist);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool AbsMove(short nAxis, double fDist)
        {

            int nRet;
  
               nRet = NMC2.nmc_AbsMove(m_nDev_no, nAxis, fDist);

               switch (nRet)
               {
                   case NMC2.NMC_NOTCONNECT:
                       m_bIsOpen = false;
                       return false;
                   case 0:
                       return true;
               }

               return false;
        }

        internal bool JogMove(short nAxis, short nDir)
        {
            short nRet = NMC2.nmc_JogMove(m_nDev_no, nAxis, nDir);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool SlowStop(short nAxis)
        {

            short nRet = NMC2.nmc_DecStop(m_nDev_no, nAxis);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool Stop(short nAxis)
        {
            short nRet = NMC2.nmc_SuddenStop(m_nDev_no, nAxis);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool RelMultiTwoMove(double[] fDist)
        {
            short[] nAxis = {0, 1};

            short nRet = NMC2.nmc_VarRelMove(m_nDev_no, 2, nAxis, fDist);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool AbsMultiTwoMove(double[] fDist)
        {
            short[] nAxis = {0,1};

            short nRet = NMC2.nmc_VarAbsMove(m_nDev_no, 2, nAxis, fDist);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        internal bool SyncTwoMove(double pulse1, double pulse2, short opt)
        {
            short nRet = NMC2.nmc_Interpolation2Axis(m_nDev_no, 0, pulse1, 1, pulse2, opt);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }


        internal bool SetCmd(short nAxis, double fValue)
        {
            short nRet = NMC2.nmc_SetCmdPos(m_nDev_no, nAxis, fValue);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        internal bool SetEnc(short nAxis, double fValue)
        {
            short nRet = NMC2.nmc_SetEncPos(m_nDev_no, nAxis, fValue);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        //  false - A 접점  ,   true  - B 접점
        internal bool SetEmerLogic(short logic)
        {
            short nRet = NMC2.nmc_SetEmgLogic(m_nDev_no, 0, logic);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }
        //  false - A 접점  ,   true  - B 접점
        internal bool SetPulseLogic(short nAxis,int logic)
        {
            short nRet = NMC2.nmc_SetPulseLogic(m_nDev_no, nAxis, (short)logic);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }
        //  false - A 접점  ,   true  - B 접점
        internal bool SetNearLogic(short nAxis, short logic)
        {
            short nRet = NMC2.nmc_SetNearLogic(m_nDev_no, nAxis, logic);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        internal bool SetMinusLimitLogic(short nAxis, short logic)
        {
            short nRet = NMC2.nmc_SetMinusLimitLogic(m_nDev_no, nAxis, logic);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        internal bool SetPlusLimitLogic(short nAxis, short logic)
        {
            short nRet = NMC2.nmc_SetPlusLimitLogic(m_nDev_no, nAxis, logic);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }


        internal bool SetAlarmLogic(short nAxis, short logic)
        {
            short nRet = NMC2.nmc_SetAlarmLogic(m_nDev_no, nAxis, logic);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }


        internal bool SetPulseMode(short nAxis,short nClock)
        {
            short nRet = NMC2.nmc_SetPulseMode(m_nDev_no, nAxis, nClock);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }


        /*
        nENCMODE
        0 - 4
        1 - 2
        2 - 1체배
        */
        internal bool SetEncCountMode( short nAxis,short nEncMode)
        {
            short nRet = NMC2.nmc_SetEncoderCount(m_nDev_no, nAxis, nEncMode);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        internal bool SetEncInputMode(short nAxis,short nMode)
        {
            short nRet = NMC2.nmc_SetEncoderDir(m_nDev_no, nAxis, nMode);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        //  false - A 접점  ,   true  - B 접점
        internal bool SetZLogic(short nAxis, short logic)
        {
            short nRet = NMC2.nmc_SetEncoderZLogic(m_nDev_no, nAxis, logic);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }

        internal bool HomeMove(short nAxis, int nHomeMode)
        {
            short nRet = NMC2.nmc_HomeMove(m_nDev_no, nAxis, (short)nHomeMode, 0, 0, 0);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }
    
            return false;
        }

        internal bool GetNmcStatus(ref NMC2.NMCAXESEXPR pNmcData)
        {
            short nRet = NMC2.nmc_GetAxesExpress(m_nDev_no, out pNmcData);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }
        internal bool GetNmcStatus2(ref NMC2.NMCAXESEXPR pNmcData)
        {
            short nRet = NMC2.nmc_GetAxesExpress(m_nDev_no2, out pNmcData);

            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;

        }
        internal void SetUnitPulse(short nAxis, double dRatio)
        {
            NMC2.nmc_SetUnitPerPulse(m_nDev_no, nAxis, dRatio);
        }


        /// <summary>
        ///                                                                             2021. 12. 09 added by nam
        /// </summary>
        /// 


        internal bool SetHomeSpeed(short nAxisNo, double dHomeSpeed0, double dHomeSpeed1, double dHomeSpeed2)
        {
            short nRet = NMC2.nmc_SetHomeSpeed(m_nDev_no, nAxisNo, dHomeSpeed0, dHomeSpeed1, dHomeSpeed2);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool SetSWLimitLogic(short nAxis, short nMode, double pStart, double pEnd)
        {
            short nRet = NMC2.nmc_SetSWLimitLogicEx(m_nDev_no, nAxis, nMode, pStart, pEnd, 0x3);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }
        internal bool SetEmgEnable(short nMode)
        {
            short nRet = NMC2.nmc_SetEmgEnable(m_nDev_no, nMode);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal bool SetServoOnOff(short nAxis, short nMode)
        {
            short nRet = NMC2.nmc_SetServoOn(m_nDev_no, nAxis, nMode);
            switch (nRet)
            {
                case NMC2.NMC_NOTCONNECT:
                    m_bIsOpen = false;
                    return false;
                case 0:
                    return true;
            }

            return false;
        }

        internal void Printing()
        {
            //프린팅 시퀸스 추가 변수 선언부

            //빌드 관련 변수
            double Layer_thickness = 0.1;//레이어 두께

            //리코팅 관련 변수

            int Recoating_repeat = 5;//리코팅 반복횟수
            double X2axis_nspeed = 200;//기본 Feeder speed
            double X2axis_fspeed = 200;//마지막 리코팅 작업시 Feeder speed

            //출력 관련 변수
            double Xaxis_pspeed = 100;//헤드 출력시 X축 speed
            double Xaxis_rspeed = 200;//헤드 복귀시 X축 speed
            short[] nBusyStatus = new short[8];
            int casenum = 0;
            printStop = false;
            while (true)
            {
                NMC2.nmc_GetBusyStatusAll(m_nDev_no, nBusyStatus);
                Application.DoEvents();
                if (printStop == true) break;

                switch (casenum)
                {
                    
                    case 0:
                        AbsMove(0, 50);
                        NMC2.nmc_GetBusyStatusAll(m_nDev_no, nBusyStatus);
                        while (nBusyStatus[0] == 1) { Application.DoEvents(); NMC2.nmc_GetBusyStatusAll(m_nDev_no, nBusyStatus); }
                        casenum = 1;
                        break;
                    case 1:
                        AbsMove(0, 0);
                        NMC2.nmc_GetBusyStatusAll(m_nDev_no, nBusyStatus);
                        while (nBusyStatus[0] == 1) { Application.DoEvents(); NMC2.nmc_GetBusyStatusAll(m_nDev_no, nBusyStatus); }
                        casenum = 0;
                        break;
                }
            }
        }

        internal void EmergencyStop()
        {
            NMC2.nmc_AllAxisStop(m_nDev_no, 1);
            NMC2.nmc_AllAxisStop(m_nDev_no2, 1);
            printStop = true;
        }
    }
}