/**
  **************************************************************************************************
  * @file       NMC2.cs
  * @author     PAIX Lab.
  * @version
  * @date       2020-06-19
  * @brief      NMC2, NMC2E Controller ���������Դϴ�.
  * @note       C#
  * @copyright  2018. PAIX Co., Ltd. All rights reserved.
  * @see        http://www.paix.co.kr/
  **************************************************************************************************
  * @note       2018-11-22    update release
  *             2019-03-25    4�ຸ�� �߰�
  *             2019-04-25    NMF ��� �߰�
  *             2019-04-30    nmc_GetCmdEncListPos �Լ� �߰�
  *             2019-05-23    NMF ��� �и�
  *             2019-06-05    nmc_EMpgCounterSet, nmc_EMpgCounterGet �Լ� �߰�
  *             2019-07-15    nmc_VarAbsDecStop �Լ� �߰�
  *             2019-08-23    NMC2E Near(Home)On�� �������� ��� ������Ʈ
  *                           �� nmc_SetAlarmStopMode �Լ� �߰�
  *             2019-11-21    nmc_SetPulseOutMask �Լ� �߰�
  *             2020-06-19    nmc_SetBacklashSlip, nmc_SetInSigFilter �Լ� �߰�
  *             2021-06-17    nmf_GetCompo �Լ� �߰�
  **************************************************************************************************
  */

using System.Runtime.InteropServices;     // DLL support

namespace Paix_MotionController
{
    internal class NMC2
    {
        /**
         * @brief   NMC2 Equip Type
         */
        internal const short NMC2_220S        = 0;
        internal const short NMC2_420S        = 1;
        internal const short NMC2_620S        = 2;
        internal const short NMC2_820S        = 3;

        internal const short NMC2_220_DIO32   = 4;
        internal const short NMC2_220_DIO64   = 5;
        internal const short NMC2_420_DIO32   = 6;
        internal const short NMC2_420_DIO64   = 7;
        internal const short NMC2_820_DIO32   = 8;
        internal const short NMC2_820_DIO64   = 9;

        internal const short NMC2_DIO32       = 10;
        internal const short NMC2_DIO64       = 11;
        internal const short NMC2_DIO96       = 12;
        internal const short NMC2_DIO128      = 13;

        internal const short NMC2_220         = 14;
        internal const short NMC2_420         = 15;
        internal const short NMC2_620         = 16;
        internal const short NMC2_820         = 17;
        internal const short NMC2_620_DIO32   = 18;
        internal const short NMC2_620_DIO64   = 19;
        internal const short NMC2_UDIO        = 100;

        /**
         * @brief   NMC2 Enum Type
         */
        internal const short EQUIP_TYPE_NMC_2_AXIS    = 0x0001;
        internal const short EQUIP_TYPE_NMC_4_AXIS    = 0x0003;
        internal const short EQUIP_TYPE_NMC_6_AXIS    = 0x0007;
        internal const short EQUIP_TYPE_NMC_8_AXIS    = 0x000F;
        internal const short EQUIP_TYPE_NMC_IO_32     = 0x0010;   /*!< IN 16, OUT 16 */
        internal const short EQUIP_TYPE_NMC_IO_64     = 0x0030;   /*!< IN 32, OUT 32 */
        internal const short EQUIP_TYPE_NMC_IO_96     = 0x0070;   /*!< IN 48, OUT 48 */
        internal const short EQUIP_TYPE_NMC_IO_128    = 0x00F0;   /*!< IN 64, OUT 64 */
        internal const short EQUIP_TYPE_NMC_IO_160    = 0x01F0;   /*!< IN 80, OUT 80 */
        internal const short EQUIP_TYPE_NMC_IO_192    = 0x03F0;   /*!< IN 96, OUT 96 */
        internal const short EQUIP_TYPE_NMC_IO_224    = 0x07F0;   /*!< IN 112, OUT 112 */
        internal const short EQUIP_TYPE_NMC_IO_256    = 0x0FF0;   /*!< IN 128, OUT 128 */

        internal const short EQUIP_TYPE_NMC_IO_IE     = 0x1000;
        internal const short EQUIP_TYPE_NMC_IO_OE     = 0x2000;

        internal const short EQUIP_TYPE_NMC_M_IO_8    = 0x4000;

        /**
         * @brief   ��� �Լ��� ���ϰ�
         */
        internal const short NMC_CANNOT_APPLY         = -18;  /*!< ���� ���� ���� ��ǿ��� �������� �ʴ� ��ɾ ������ ��� */
        internal const short NMC_NO_CONSTSPDDRV       = -17;  /*!< ���� ������ �ƴ� ����,���� �� ��ɾ �Էµ� ��� */
        internal const short NMC_SET_1ST_SPDPPS       = -16;  /*!< �ӵ� ���������� ���� �Է��Ͻʽÿ� */
        internal const short NMC_CONTI_BUF_FULL       = -15;  /*!< ������ ���Ӻ����� ���۰� ��� ä���� ���� */
        internal const short NMC_CONTI_BUF_EMPTY      = -14;  /*!< ������ ���Ӻ����� ���ۿ� �����Ͱ� ���� ���� */
        internal const short NMC_INTERPOLATION        = -13;  /*!< ����(�Ϲ�) ���� ���� �߿� ���� ��ɾ �Էµ� ��� */
        internal const short NMC_FILE_LOAD_FAIL       = -12;  /*!< F/W file �ε� ���� */
        internal const short NMC_ICMP_LOAD_FAIL       = -11;  /*!< ICMP.dLL �ε� ����, nmc_PingCheck ȣ��� �߻�. PC�� DLL������ Ȯ�� */
        internal const short NMC_NOT_EXISTS           = -10;  /*!< ��Ʈ��ũ ��ġ�� �ĺ����� �ʴ� ���, ��ȭ���̳� ���� ���¸� Ȯ�� */
        internal const short NMC_CMDNO_ERROR          = -9;   /*!< �Լ� ȣ�� ��, �ĺ��ڵ忡 ���� �߻� */
        internal const short NMC_NOTRESPONSE          = -8;   /*!< �Լ� ȣ�� ��, ������ ���� ��� */
        internal const short NMC_BUSY                 = -7;   /*!< ���� ���� ���� ���� ��� */
        internal const short NMC_COMMERR              = -6;   /*!< �Լ� ȣ�� ��, ��� ���� �߻� */
        internal const short NMC_SYNTAXERR            = -5;   /*!< �Լ� ȣ�� ��, ���� ���� �߻� */
        internal const short NMC_INVALID              = -4;   /*!< �Լ� ���ڰ��� �����߻� */
        internal const short NMC_UNKOWN               = -3;   /*!< �������� �ʴ� �Լ� ȣ�� */
        internal const short NMC_SOCKINITERR          = -2;   /*!< ���� �ʱ�ȭ ���� */
        internal const short NMC_NOTCONNECT           = -1;   /*!< ��ġ�� ������ ������ ��� */
        internal const short NMC_OK                   = 0;    /*!< ���� */

        /**
         * @brief   STOP MODE
         */
        internal const short NMC_STOP_OK              = 0;
        internal const short NMC_STOP_EMG             = 1;
        internal const short NMC_STOP_MLIMIT          = 2;
        internal const short NMC_STOP_PLIMIT          = 3;
        internal const short NMC_STOP_ALARM           = 4;
        internal const short NMC_STOP_NEARORG         = 5;
        internal const short NMC_STOP_ENCZ            = 6;

        /**
         * @brief   HOME MODE
         */
        internal const short NMC_HOME_LIMIT_P         = 0;
        internal const short NMC_HOME_LIMIT_M         = 1;
        internal const short NMC_HOME_NEAR_P          = 2;
        internal const short NMC_HOME_NEAR_M          = 3;
        internal const short NMC_HOME_Z_P             = 4;
        internal const short NMC_HOME_Z_M             = 5;

        internal const short NMC_HOME_USE_Z           = 0x80;

        internal const short NMC_END_NONE                 = 0x00;
        internal const short NMC_END_CMD_CLEAR_A_OFFSET   = 0x01;
        internal const short NMC_END_ENC_CLEAR_A_OFFSET   = 0x02;
        internal const short NMC_END_CMD_CLEAR_B_OFFSET   = 0x04;
        internal const short NMC_END_ENC_CLEAR_B_OFFSET   = 0x08;

        /**
         * @brief   ��ġ�� ����
         */
        internal const short NMC_LOG_NONE         = 0;
        internal const short NMC_LOG_DEV          = 0x01;
        internal const short NMC_LOG_MO_MOV       = 0x02; /*!< ����Լ��� MOVE */
        internal const short NMC_LOG_MO_SET       = 0x04; /*!< ����Լ��� GET */
        internal const short NMC_LOG_MO_GET       = 0x08; /*!< ����Լ��� SET */
        internal const short NMC_LOG_MO_EXPRESS   = 0x10; /*!< ����Լ��� ���� ���°� �д�(����� �߻�) */
        internal const short NMC_LOG_IO_SET       = 0x20;
        internal const short NMC_LOG_IO_GET       = 0x40;

        /**
         * @brief   �� �ະ Logic ���� ���� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCPARALOGIC
        {

            internal short    nEmg;                   /*!< Emergency �Է� ��ȣ */
            internal short    nEncCount;              /*!< ���ڴ� ī��Ʈ ��� */
            internal short    nEncDir;                /*!< ���ڴ� ī��Ʈ ���� */
            internal short    nEncZ;                  /*!< ���ڴ� Z �Է� ��ȣ */
            internal short    nNear;                  /*!< �������� �Է� ��ȣ */
            internal short    nMLimit;                /*!< - Limit �Է� ��ȣ */
            internal short    nPLimit;                /*!< + Limit �Է� ��ȣ*/
            internal short    nAlarm;                 /*!< �˶� �Է� ��ȣ */
            internal short    nInp;                   /*!< Inposition �Է� ��ȣ */
            internal short    nSReady;                /*!< Servo Ready �Է� ��ȣ */
            internal short    nPulseMode;             /*!< �޽� ��� ��� */

            internal short    nLimitStopMode;         /*!< ��Limit �Է� ��, ������� */
            internal short    nBusyOff;               /*!< Busy off ��� ���� */
            internal short    nSWEnable;              /*!< SW limit Ȱ��ȭ ���� */
            internal double   dSWMLimitPos;           /*!< - SW Limit ��ġ */
            internal double   dSWPLimitPos;           /*!< + SW Limit ��ġ */
        };

        /**
         * @brief   Ȯ��� �� �ະ Logic ���� ���� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCPARALOGICEX
        {
            internal short    nEmg;                   /*!< Emergency �Է� ��ȣ */
            internal short    nEncCount;              /*!< ���ڴ� ī��Ʈ ��� */
            internal short    nEncDir;                /*!< ���ڴ� ī��Ʈ ���� */
            internal short    nEncZ;                  /*!< ���ڴ� Z �Է� ��ȣ */
            internal short    nNear;                  /*!< �������� �Է� ��ȣ */
            internal short    nMLimit;                /*!< - Limit �Է� ��ȣ */
            internal short    nPLimit;                /*!< + Limit �Է� ��ȣ*/
            internal short    nAlarm;                 /*!< �˶� �Է� ��ȣ */
            internal short    nInp;                   /*!< Inposition �Է� ��ȣ */
            internal short    nSReady;                /*!< Servo Ready �Է� ��ȣ */
            internal short    nPulseMode;             /*!< �޽� ��� ��� */

            internal short    nLimitStopMode;         /*!< ��Limit �Է� ��, ������� */
            internal short    nBusyOff;               /*!< Busy off ��� ���� */
            internal short    nSWEnable;              /*!< SW limit Ȱ��ȭ ���� */
            internal double   dSWMLimitPos;           /*!< - SW Limit ��ġ */
            internal double   dSWPLimitPos;           /*!< + SW Limit ��ġ */

            /* ���� �Ϸ���� ���� ��뿩�� */
            internal short    nHDoneCancelAlarm;      /*!< Alarm �߻� �� ��뿩�� */
            internal short    nHDoneCancelServoOff;   /*!< Servo Off �� ��뿩�� */
            internal short    nHDoneCancelCurrentOff; /*!< Current Off �� ��뿩�� */
            internal short    nHDoneCancelServoReady; /*!< Servo Ready ���� �� ��뿩�� */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            internal short[]  nDummy;                /*!< ���� ���� */
        };
        //------------------------------------------------------------------------------

        /**
         * @brief �� �ະ ������ �ӵ� ���� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCPARASPEED
        {
            internal double   dStart;                 /*!< ���ۼӵ� */
            internal double   dAcc;                   /*!< ���ӵ� */
            internal double   dDec;                   /*!< ���ӵ� */
            internal double   dDrive;                 /*!< �����ӵ� */
            internal double   dJerkAcc;               /*!< ���� Jerk, S-Curve ������ ��� */
            internal double   dJerkDec;               /*!< ���� Jerk, S-Curve ������ ��� */
        };
        //------------------------------------------------------------------------------

        /**
         * @brief 8�� ��� ���� ��½�ȣ �� �������� Ȯ�� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCAXESMOTIONOUT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nCurrentOn;              /*!< ���� ���� ��� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nServoOn;                /*!< Servo On ��� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nDCCOn;                  /*!< DCC ��� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nAlarmResetOn;           /*!< Alarm Reset ��� ����(0=Off, 1=On) */
        };
        //------------------------------------------------------------------------------

        /**
         * @brief 8���� ���� �����Է� ����, ��ġ��, ���� ���� ���� Ȯ���ϴ� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCAXESEXPR
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nBusy;                  /*!< �޽� ��� ����(0=Idle, 1=Busy) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nError;                 /*!< Error �߻� ����(0=None, 1=Error) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nNear;                  /*!< ���� ���� �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nPLimit;                /*!< + Limit ���� �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nMLimit;                /*!< - Limit ���� �Է� ����(0=Off, 1=On) */

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nAlarm;                 /*!< �˶� ���� �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal short[]  nEmer;                  /*!< �׷캰 EMG �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nSwPLimit;              /*!< SW +Limit �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nInpo;                  /*!< Inposition �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nHome;                  /*!< Home Search ���� ����(0=������, 1=None) */

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nEncZ;                  /*!< ���ڴ� Z�� �Է� ����(0=Off, 1=On) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nOrg;                   /*!< ���� ���� �Է� ����(0=Off, 1=On)(NMC-403S ������ ����) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nSReady;                /*!< Servo Ready �Է� ����(0=Off, 1=On) */

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal short[]  nContStatus;            /*!< ���Ӻ��� ���� ����(0=�Ϸ�, 1=������) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            internal short[]  nDummy;                 /*!< ���� ���� */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nSwMLimit;              /*!< SW -Limit �Է� ����(0=Off, 1=On) */

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal int[]    lEnc;                   /*!< ���ڴ� ��ġ(UnitPerPulse ������� ����) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal int[]    lCmd;                   /*!< ���� ��ġ(UnitPerPulse ������� ����) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal double[] dEnc;                   /*!< ���ڴ� ��ġ(UnitPerPulse ����) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal double[] dCmd ;                  /*!< ���� ��ġ(UnitPerPulse ����) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            internal byte[]   dummy;                  /*!< ���� ���� */
        };
        //------------------------------------------------------------------------------

        /**
         * @brief ��ġ �������� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCEQUIPLIST
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            internal int[] lIp;               /*!< ��ġ�� IP��ȣ */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            internal int[] lModelType;        /*!< �𵨸� ���� */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            internal short[] nMotionType;     /*!< ���� �� ��(0=None, 2, 4, 6, 8) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            internal short[] nDioType;        /*!< DIO ����(0=None, 1=16/16, 2=32/32, 3=48/48, 4=64/64) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            internal short[] nEXDIo ;         /*!< EXDIO ����(0=None, 1=In16, 2=Out16) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            internal short[] nMDio;           /*!< MDIO ����(0=None, 1=8/8) */
        };
        
        /**
				  * @brief  ���� ������ ��ġ(NMF)���� ����
				  */
				[StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TNMF_COMPO
				{
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            internal short[]	nIP;             	/*!< IP�ּ� */
						internal short   	nModelPart;       /*!< �� Part ���� (0=NMF, 1=UDIO) �����Ǵ� ��(NMF),��(UDIO)�� ���� */
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
				    internal short[]  nType;						/*!< ���� ���� (0=����, 1=D016, 2=DI16, 3=AI8(�ش� ����), 4=AO8(�ش� ����), 5=AI8AO8(�ش� ����), 6=DO8, 7=DI8, 8=DI8DO8, 9~49=����) */
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
						internal short[]  nCntBrd;   				/*!< ���º� ���� ����(Max.8) �迭 ��ȣ�� ���� ���� �� ��)DI8=2��, DO16=3�� �� ��� nCntBrd[1]=3, nCntBrd[7]=2���� ��. */
				    internal short   	nTotalCntBrd;     /*!< ����� ��ü ���� ���� (Max.8) */
				    internal short   	nReserved;				/*!< ���� */
				};

        /**************************************************************************************************/

        /**
          * @brief      ��ġ�� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ(���͸� ����ġ ��ȣ���� IP��ȣ)
          * @param[in]  lWaitTime       ������ �ð�
          * @return     PAIX_RETURN_VALUE
          * @see        nmc_CloseDevice
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_OpenDevice(short nNmcNo);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_OpenDeviceEx(short nNmcNo, int lWaitTime);

        /**
          * @brief      ��ġ���� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @see        nmc_OpenDevice
          */
        [DllImport("NMC2.dll")]
        internal static extern void nmc_CloseDevice(short nNmcNo);

        /**
          * @brief      ��ġ���� ������ ��Ʈ��ũ ������ Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  lWaitTime       ������ð�(ms)
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_PingCheck(short nNmcNo, int lWaitTime);

        /**
          * @brief      ��ġ�� �����ϱ� ���� IP������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nField0         IP�ּ� ù��° (ex. 192.xxx.xxx.xxx)
          * @param[in]  nField1         IP�ּ� �ι�° (ex. xxx.168.xxx.xxx)
          * @param[in]  nField2         IP�ּ� ����° (ex. xxx.xxx.100.xxx)
          */
        [DllImport("NMC2.dll")]
        internal static extern void nmc_SetIPAddress(short nNmcNo, short nField0, short nField1, short nField2);

        /**
          * @brief      ��ġ�� IP�� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  pnIP            ������ IP�ּ� �迭 ������(ex. 192.168.0)
          * @param[in]  pnSubNet        ������ SubnetMask �迭 ������(ex. 255.255.255)
          * @param[in]  nGateway        ������ Gateway �迭 ������(ex. 1)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_WriteIPAddress(short nNmcNo, short[] pnIP, short[] pnSubNet, short nGateway);

        /**
          * @brief      ��ġ�� ��Ź���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMethod         �Ź��(0=TCP, 1=UDP)
          */
        [DllImport("NMC2.dll")]
        internal static extern void nmc_SetProtocolMethod(short nNmcNo, short nMethod);

        /**
          * @brief      ��ġ���� ��� Ȯ�� �ð��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  lTimeInterval   ���Ȯ�νð� (0=�������)(ms)
          * @param[in]  nStopMode       �������(0=��������, 1=�������)
          * @return     PAIX_RETURN_VALUE
          * @warning    �ð��� �ʰ��Ͽ� ����� �簳���� ������ ������ ������忡 ���� ��� ���� �����մϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDisconnectedStopMode(short nNmcNo, int lTimeInterval, short nStopMode);

        /**
          * @brief      �̵� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dRatio          �̵�����
          * @return     PAIX_RETURN_VALUE
          * @warning    �������� ���� �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetUnitPerPulse(short nNmcNo, short nAxisNo, double dRatio);

        /**
          * @brief      ���� ���� ���� ���������� �о� �´�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ���������� �о�� �� ��ȣ
          * @param[out] pLogic          ���������� ������ PARALOGIC ����ü ������
          * @param[out] pLogicEx        ���������� ������ PARALOGICEX ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @see        NMC_PARA_LOGIC
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetParaLogic(short nNmcNo, short nAxisNo, out NMCPARALOGIC pLogic);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetParaLogicEx(short nNmcNo, short nAxisNo, out NMCPARALOGICEX pLogicEx);

        /**
          * @brief      �� �׷��� Emergency ��ȣ �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ������ �� �׷�(0�׷�=0~3, 1�׷�=1~7)
          * @param[in]  nLogic          ���� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEmgLogic(short nNmcNo, short nGroupNo, short nLogic);

        /**
          * @brief      Emergency �Է��� Ȱ��ȭ�մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nEnable         Ȱ������(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEmgEnable(short nNmcNo, short nEnable);

        /**
          * @brief      ���� +Limit ���� �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ���� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetPlusLimitLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� -Limit ���� �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ���� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMinusLimitLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� Inposition �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nUse            Software Limit Ȱ��ȭ����(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @param[in]  dSwMinusPos     -���� ������ġ
          * @param[in]  dSwPlusPos      +���� ������ġ
          * @param[in]  nOpt            ��ġ �� ����(0=������ġ, 1=���ڴ���ġ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSWLimitLogic(short nNmcNo, short nAxisNo, short nUse, double dSwMinusPos, double dSwPlusPos);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSWLimitLogicEx(short nNmcNo, short nAxisNo, short nUse, double dSwMinusPos, double dSwPlusPos, short nOpt);

        /**
          * @brief      ���� �˶� �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ���� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetAlarmLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� �������� �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ���� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetNearLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� Inposition �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ���� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetInPoLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� Servo Ready �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ���������� �о�� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSReadyLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� ���ڴ� Z�� �Է� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          �Է� ����(0=Low, 1=High)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEncoderZLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      ���� ���ڴ� ü�踦 �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nCountMode      ü�� ������(0=4ü��, 1=2ü��, 2=1ü��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEncoderCount(short nNmcNo, short nAxisNo, short nCountMode);

        /**
          * @brief      ���� ���ڴ� ī��Ʈ ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nCountDir       ī���� ����(0=A|B(+), 1=B|A(-), 2=Up|Down, 3=Down|Up)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEncoderDir(short nNmcNo, short nAxisNo, short nCountDir);

        /**
          * @brief      ��ġ���� ������ ��µǴ� �޽� ��带 �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nLogic          ������� �����Ͻʽÿ�.(�޽� ��� ���)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetPulseLogic(short nNmcNo, short nAxisNo, short nLogic);

        /**
          * @brief      �����̵� �Ϸ���¸� ������ ���¸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ���� �о�� �� ��ȣ
          * @param[in]  nAlarm          �˶� �߻��� ����(0=����, 1=���)
          * @param[in]  nServoOff       Servo Off�� ����(0=����, 1=���)
          * @param[in]  nCurrentOff     Current Off�� ����(0=����, 1=���)
          * @param[in]  nServoReady     Servo Ready�� ����(0=����, 1=���)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetHomeDoneAutoCancel(short nNmcNo, short nAxisNo, short nAlarm, short nServoOff, short nCurrentOff, short nServoReady);

        /**
          * @brief      �����̵� �Ϸ���� ���� �������¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[out] pnAlarm         �˶� �߻��� ���� ������ ����
          * @param[out] pnServoOff      Servo Off�� ���� ������ ����
          * @param[out] pnCurrentOff    Current Off�� ���� ������ ����
          * @param[out] pnServoReady    Servo Ready�� ���� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetHomeDoneAutoCancel(short nNmcNo, short nAxisNo, out short pnAlarm, out short pnServoOff, out short pnCurrentOff, out short pnServoReady);

        /**
          * @brief      ���� ���� ������ �����Ѵ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ���� ������ ������ �� ��ȣ
          * @param[in]  pLogic          ���������� �� PARALOGIC ����ü ������
          * @param[in]  pLogicEx        ���������� �� PARALOGICEX ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @see        NMC_PARA_LOGIC
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetParaLogic(short nNmcNo, short nAxisNo, ref NMCPARALOGIC pLogic);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetParaLogicEx(short nNmcNo, short nAxisNo, ref NMCPARALOGICEX pLogicEx);

        /**
          * @brief      8���� ���� ������ ������ ���Ͽ��� �����Ѵ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  pStr            8�� ���������� ����ִ� �����̸�
          * @return     PAIX_RETURN_VALUE
          * @see        NMC_PARA_LOGIC
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetParaLogicFile(short nNmcNo, byte[] pStr);

        /**
          * @brief      ���� ���� ���� ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetCurrentOn(short nNmcNo, short nAxisNo, short nOut);

        /**
          * @brief      ���� ���� Servo On�� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetServoOn(short nNmcNo, short nAxisNo, short nOut);

        /**
          * @brief      ���� ���� Alarm Reset�� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetAlarmResetOn(short nNmcNo, short nAxisNo, short nOut);

        /**
          * @brief      ���� ���� DCC ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDccOn(short nNmcNo, short nAxisNo, short nOut);

        /**
          * @brief      �����ϴ� ������ ���� ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisSelect    �� ��ȣ �迭 ������
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMultiCurrentOn(short nNmcNo, short nCount, short[] pnAxisSelect, short nOut);

        /**
          * @brief      �����ϴ� ������ Servon On ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisSelect    �� ��ȣ �迭 ������
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMultiServoOn(short nNmcNo, short nCount, short[] pnAxisSelect, short nOut);

        /**
          * @brief      �����ϴ� ������ Alarm Reset ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisSelect    �� ��ȣ �迭 ������
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMultiAlarmResetOn(short nNmcNo, short nCount, short[] pnAxisSelect, short nOut);

        /**
          * @brief      �����ϴ� ������ DCC ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisSelect    �� ��ȣ �迭 ������
          * @param[in]  nOut            ��°�(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMultiDccOn(short nNmcNo, short nCount, short[] pnAxisSelect, short nOut);

        /**
          * @brief      ��ٸ��� ���·� �ӵ� ���������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dStartSpeed     ���ۼӵ�(�̵�����/��)
          * @param[in]  dAcc            ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDec            ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDriveSpeed     �����ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSpeed(short nNmcNo, short nAxisNo, double dStartSpeed, double dAcc, double dDec, double dDriveSpeed);

        /**
          * @brief      S-Curve ���·� �ӵ� ���������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dStartSpeed     ���ۼӵ�(�̵�����/��)
          * @param[in]  dAcc            ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDec            ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDriveSpeed     �����ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSCurveSpeed(short nNmcNo, short nAxisNo, double dStartSpeed, double dAcc, double dDec, double dDriveSpeed);

        /**
          * @brief      ���ӵ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dAcc            ���ӵ�(�̵�����/�ʩ�)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetAccSpeed(short nNmcNo, short nAxisNo, double dAcc);

        /**
          * @brief      ���ӵ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dDec            ���ӵ�(�̵�����/�ʩ�)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDecSpeed(short nNmcNo, short nAxisNo, double dDec);

        /**
          * @brief      ���� ���� ���� �ӵ��� �������̵� �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dAcc            ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDec            ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDriveSpeed     �����ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetOverrideRunSpeed(short nNmcNo, short nAxisNo, double dAcc, double dDec, double dDriveSpeed);

        /**
          * @brief      ���� ���� ���� �����ӵ��� �������̵� �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dDriveSpeed     �����ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetOverrideDriveSpeed(short nNmcNo, short nAxisNo, double dDriveSpeed);

        /**
          * @brief      ���� �����̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos            �̵��� ������ġ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_AbsMove(short nNmcNo, short nAxisNo, double dPos);

        /**
          * @brief      ���� ����̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dAmount         �̵��� �����ġ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_RelMove(short nNmcNo, short nAxisNo, double dAmount);

        /**
          * @brief      �ԷµǴ� �ӵ��� �̵� ��忡���� ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos            �̵��� ��ġ
          * @param[in]  dDrive          �����ӵ�(�̵�����/��)
          * @param[in]  nMode           �̵��� ��ġ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_VelMove(short nNmcNo, short nAxisNo, double dPos, double dDrive, short nMode);

        /**
          * @brief      ���� �����̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisList      �� ��ȣ �迭 ������
          * @param[in]  pdPosList       �̵��� ������ġ �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_VarAbsMove(short nNmcNo, short nAxisCount, short[] pnAxisList, double[] pdPosList);

        /**
          * @brief      ���� ����̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisList      �� ��ȣ �迭 ������
          * @param[in]  pdAmount        �̵��� �����ġ �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_VarRelMove(short nNmcNo, short nAxisCount, short[] pnAxisList, double[] pdAmount);

        /**
          * @brief      ���� ���� �࿡ ��ġ �������̵带 ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos            �������̵��� ������ġ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_AbsOver(short nNmcNo, short nAxisNo, double dPos);

        /**
          * @brief      ���� ���� ������ ��ġ �������̵带 ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisList      �� ��ȣ �迭 ������
          * @param[in]  pdPosList       �������̵��� ������ġ �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_VarAbsOver(short nNmcNo, short nAxisCount, short[] pnAxisList, double[] pdPosList);

        /**
          * @brief      ������ ������ ���� ���� ������(�̵����� �����ϰ� ������ ���Ӹ����� ����) �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisList      �� ��ȣ �迭 ������
          * @param[in]  pdDecList       ���Ӱ��� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_VarAbsDecStop(short nNmcNo, short nAxisCount, short[] pnAxisList, double[] pdDecList);

        /**
          * @brief      ������ �����Ͽ� �ӵ��̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nDir            ��������(0=CW(������), 1=CCW(������))
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_JogMove(short nNmcNo, short nAxis, short nDir);

        /**
          * @brief      ���� ���� ���� ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         �� ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SuddenStop(short nNmcNo, short nAxisNo);

        /**
          * @brief      ���� ���� ���� ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         �� ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_DecStop(short nNmcNo, short nAxisNo);

        /**
          * @brief      ���� ��忡 ���� ���� ���� ��ü ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMode           �������(0=��������, 1=�������)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_AllAxisStop(short nNmcNo, short nMode);

        /**
          * @brief      ���� ��忡 ���� ���� ���� ���� ���� ���������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisSelect    ������ �� ��ȣ �迭 ������
          * @param[in]  nMode           �������(0=��������, 1=�������)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MultiAxisStop(short nNmcNo, short nCount, short[] pnAxisSelect, short nMode);

        /**
          * @brief      ���� ������ġ ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos ������ ��ġ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetCmdPos(short nNmcNo, short nAxisNo, double dPos);

        /**
          * @brief      ���� ���ڴ���ġ ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos            ������ ��ġ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEncPos(short nNmcNo, short nAxisNo, double dPos);

        /**
          * @brief      ���� ������ġ�� ���ڴ� ��ġ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos            ������ ��ġ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetCmdEncPos(short nNmcNo, short nAxisNo, double dPos);

        /**
          * @brief      �����̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nHomeMode       �����̵� ���¸� �����մϴ�.\n
                                        (0=+Limit, 1=-Limit, 2=+Near, 3=-Near, 4= -Z, 5= +Z)\n
                                        �ڼ��� ������ ������� �����Ͻʽÿ�.
          * @param[in]  nHomeEndMode    �����̵� �Ϸ� ��, ��ġ�� ����(����� ����)
          * @param[in]  dOffset         �����̵� �Ϸ� ��, Offset �̵� ��ġ��
          * @param[in]  nReserve        ���ຯ��(0�� �����Ͻʽÿ�.)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_HomeMove(short nNmcNo, short nAxisNo, short nHomeMode, short nHomeEndMode, double dOffset, short nReserve);

        /**
          * @brief      �����̵��� ���Ǵ� �����ӵ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dHomeSpeed0     1�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dHomeSpeed1     2�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dHomeSpeed2     3�� �̵��ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetHomeSpeed(short nNmcNo, short nAxisNo, double dHomeSpeed0, double dHomeSpeed1, double dHomeSpeed2);

        /**
          * @brief      �����̵��� ���Ǵ� �����ӵ��� �����մϴ�.(Offset �̵� �ӵ��� �߰��մϴ�.)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dHomeSpeed0     1�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dHomeSpeed1     2�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dHomeSpeed2     3�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dOffsetSpeed    Offset��ġ �̵��ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetHomeSpeedEx(short nNmcNo, short nAxisNo, double dHomeSpeed0, double dHomeSpeed1, double dHomeSpeed2, double dOffsetSpeed);

        /**
          * @brief      �����̵��� ���Ǵ� �������� ������ �ӵ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dHomeSpeed0     1�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dStart0         1�� ���ۼӵ�(�̵�����/��)
          * @param[in]  dAcc0           1�� ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDec0           1�� ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dHomeSpeed2     2�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dStart1         2�� ���ۼӵ�(�̵�����/��)
          * @param[in]  dAcc1           2�� ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDec1           2�� ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dHomeSpeed2     3�� �̵��ӵ�(�̵�����/��)
          * @param[in]  dStart2         3�� ���ۼӵ�(�̵�����/��)
          * @param[in]  dAcc2           3�� ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dDec2           3�� ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dOffsetSpeed    Offset��ġ �̵��ӵ�(�̵�����/��)
          * @param[in]  dOffsetStart    Offset ���ۼӵ�(�̵�����/��)
          * @param[in]  dOffsetAcc      Offset ���ӵ�(�̵�����/�ʩ�)
          * @param[in]  dOffsetDec      Offset ���ӵ�(�̵�����/�ʩ�)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetHomeSpeedAccDec(short nNmcNo, short nAxisNo, double dHomeSpeed0, double dStart0, double dAcc0, double dDec0,
                                                        double dHomeSpeed1, double dStart1, double dAcc1, double dDec1,
                                                        double dHomeSpeed2, double dStart2, double dAcc2, double dDec2,
                                                        double dOffsetSpeed, double dOffsetStart, double dOffsetAcc, double dOffsetDec);

        /**
         * @brief ���� �̵����� Ȯ�� ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCHOMEFLAG
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nSrchFlag;                       /*!< �����̵� ���ۿ���(0=�̵��Ϸ�, 1=�̵���) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nStatusFlag;                     /*!< �����̵� ���� ���� ����\n
                                                                (0=�̵��Ϸ�, 1=�̵���, 2= ����ڿ� ���� ����, 3=�����̵� �̽���\n
                                                                4=�������, 5=�˶�����, ... ) */
        };

        /**
          * @brief      �����̵� ���¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pHomeFlag       �����̵� ���¸� ���� ����ü ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetHomeStatus(short nNmcNo, out NMCHOMEFLAG pHomeFlag);

        /**
          * @brief      �����̵� �����, �ܰ躰 ����ȭ�� ���� �����ð��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nHomeDelay      ������ �ð�(ms)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetHomeDelay(short nNmcNo, int nHomeDelay);

        /**
          * @brief      �����̵� ����Ϸ� ���¸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_HomeDoneCancel(short nNmcNo, short nAxisNo);

        /**
          * @brief      2�� ���������� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dPos[x]         �̵��� ��ġ(X,Y)
          * @param[in]  nOpt            �̵����(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_Interpolation2Axis(short nNmcNo, short nAxisNo0, double dPos0, short nAxisNo1, double dPos1, short nOpt);

        /**
          * @brief      3�� ���������� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dPos[x]         �̵��� ��ġ(X,Y,Z)
          * @param[in]  nOpt            �̵����(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_Interpolation3Axis(short nNmcNo, short nAxisNo0, double dPos0, short nAxisNo1, double dPos1, short nAxisNo2, double dPos2, short nOpt);

        /**
          * @brief      4�� ���������� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dPos[x]         �̵��� ��ġ(X,Y,Z,U)
          * @param[in]  nOpt            �̵����(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_Interpolation4Axis(short nNmcNo, short nAxisNo0, double dPos0, short nAxisNo1, double dPos1,
                short nAxisNo2, double dPos2, short nAxisNo3, double dPos3, short nOpt);

        /**
          * @brief      ��ȣ������ ����մϴ�. �߽���ġ�� ȸ�������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dCenter[x]      �߽���ġ(X,Y)
          * @param[in]  dAngle          ȸ������(���� = CW(������), ��� = CCW(������))
          * @param[in]  nOpt            �߽���ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationArc(short nNmcNo, short nAxisNo0, short nAxisNo1,
                double dCenter0, double dCenter1, double dAngle, short nOpt);

        /**
          * @brief      ��ȣ������ ����մϴ�. �߽���ġ�� ������ġ, ȸ�������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dCenter[x]      �߽���ġ(X,Y)
          * @param[in]  dEnd[x]         ������ġ(X,Y)
          * @param[in]  nDir            ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  nOpt            �߽���ġ�� ������ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationArcCE(short nNmcNo, short nAxisNo0, short nAxisNo1,
                double dCenter0, double dCenter1, double dEnd0, double dEnd1, short nDir, short nOpt);

        /**
          * @brief      ��ȣ������ ����մϴ�. �����ġ�� ������ġ�� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dPass[x]        �����ġ(X,Y)
          * @param[in]  dEnd[x]         ������ġ(X,Y)
          * @param[in]  nOpt            �����ġ�� ������ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationArcPE(short nNmcNo, short nAxisNo0, short nAxisNo1,
                double dPass0, double dPass1, double dEnd0, double dEnd1, short nOpt);

        /**
          * @brief      ��ȣ������ ����մϴ�. �������� ������ġ, ȸ������, �̵��Ÿ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dRadius         ���� ������
          * @param[in]  dEnd[x]         ������ġ(X,Y)
          * @param[in]  nLen            ��ȣ�� �̵��Ÿ�(0=ª���Ÿ�, 1=��Ÿ�)
          * @param[in]  nDir            ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  nOpt            ������ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationArcRE(short nNmcNo, short nAxisNo0, short nAxisNo1,
                double dRadius, double dEnd0, double dEnd1, short nLen, short nDir, short nOpt);

        /**
          * @brief      �︮�ú����� ����մϴ�. �߽���ġ�� ȸ�������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dCenter[x]      �߽���ġ(X,Y)
          * @param[in]  dAngle          ȸ������(���� = CW(������), ��� = CCW(������))
          * @param[in]  nArcOpt         ��ȣ �߽���ġ ���(0=���, 1=����)
          * @param[in]  dZPos           Z�� �̵���ġ��
          * @param[in]  nZOpt           Z�� ��ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationHelical(short nNmcNo, short nAxisNo0, short nAxisNo1, short nAxisNo2,
                double dCenter0, double dCenter1, double dAngle, short nArcOpt, double dZPos, short nZOpt);

        /**
          * @brief      �︮�ú����� ����մϴ�. �߽���ġ�� ������ġ, ȸ�������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dCenter[x]      �߽���ġ(X,Y)
          * @param[in]  dEnd[x]         ������ġ(X,Y)
          * @param[in]  nDir            ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  nArcOpt         ��ȣ �߽���ġ�� ������ġ ���(0=���, 1=����)
          * @param[in]  dZPos           Z�� �̵���ġ��
          * @param[in]  nZOpt           Z�� ��ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationHelicalCE(short nNmcNo, short nAxisNo0, short nAxisNo1, short nAxisNo2,
                double dCenter0, double dCenter1, double dEnd0, double dEnd1, short nDir, short nArcOpt, double dZPos, short nZOpt);

        /**
          * @brief      �︮�ú����� ����մϴ�. �����ġ�� ������ġ�� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dPass[x]        �����ġ(X,Y)
          * @param[in]  dEnd[x]         ������ġ(X,Y)
          * @param[in]  nArcOpt         ��ȣ �����ġ�� ������ġ ���(0=���, 1=����)
          * @param[in]  dZPos           Z�� �̵���ġ��
          * @param[in]  nZOpt           Z�� ��ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationHelicalPE(short nNmcNo, short nAxisNo0, short nAxisNo1, short nAxisNo2,
                double dPass0, double dPass1, double dEnd0, double dEnd1, short nArcOpt, double dZPos, short nZOpt);

        /**
          * @brief      �︮�ú����� ����մϴ�. �������� ������ġ, ȸ������, �̵��Ÿ��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo[x]      ������ �� ��ȣ(0���� ������������ ����, nAxisNo0�� ������)
          * @param[in]  dRadius         ���� ������
          * @param[in]  dEnd[x]         ������ġ(X,Y)
          * @param[in]  nLen            ��ȣ�� �̵��Ÿ�(0=ª���Ÿ�, 1=��Ÿ�)
          * @param[in]  nDir            ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  nArcOpt         ��ȣ ������ġ ���(0=���, 1=����)
          * @param[in]  dZPos           Z�� �̵���ġ��
          * @param[in]  nZOpt           Z�� ��ġ ���(0=���, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationHelicalRE(short nNmcNo, short nAxisNo0, short nAxisNo1, short nAxisNo2,
                double dRadius, double dEnd0, double dEnd1, short nLen, short nDir, short nArcOpt, double dZPos, short nZOpt);

        /**
          * @brief      ��ī���� ��� ��� ������ �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  lMaxPulse       1ȸ���� �ϱ����� ���� �޽��� ��
          * @param[in]  lMaxEncoder     1ȸ���� ���ڴ� ��
          * @param[in]  nRingMode       ��ī��Ʈ ��� ��뿩��(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetRingCountMode(short nNmcNo, short nAxisNo, int lMaxPulse, int lMaxEncoder, short nRingMode);

        /**
          * @brief      ���� ��ī��Ʈ ������ �о� �´�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] plMaxPulse      ��ī��Ʈ �޽� �ִ밪�� �о�� ������ ����
          * @param[out] plMaxEncoder    ��ī��Ʈ ���ڴ� �ִ밪�� �о�� ������ ����
          * @param[out] pnRingMode      ��ī��Ʈ Ȱ��ȭ ���θ� �о�� ������ ����(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetRingCountMode(short nNmcNo, short nAxisNo, out int plMaxPulse, out int plMaxEncoder, out short pnRingMode);

        /**
          * @brief      ��ī���Ͱ� ������ ���� �̵�����Դϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dPos            �̵��� ������ġ
          * @param[in]  nMoveMode       ��������(0=CW(������), 1=CCW(������))
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MoveRing(short nNmcNo, short nAxisNo, double dPos, short nMoveMode);

        /**
          * @brief      ��Limit ��ȣ�Է� ��, ������带 �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nStopMode       ������(0=�������, 1=��������)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetLimitStopMode(short nNmcNo, short nAxisNo, short nStopMode);

        /**
          * @brief      Alarm ��ȣ�Է� ��, ������带 �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nStopMode       ������(0=�������, 1=��������)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetAlarmStopMode(short nNmcNo, short nAxisNo, short nStopMode);

        /**
          * @brief      �������� ��ȣ�Է� ��, ���������� ���࿩�θ� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nUse            ������(0=����, 1=���)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEnableNear(short nNmcNo, short nAxisNo, short nMode);

        /**
          * @brief      ���ڴ� Z�� ��ȣ�Է� ��, ���������� ���࿩�θ� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nMode           ������(0=����, 1=���)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEnableEncZ(short nNmcNo, short nAxisNo, short nMode);

        /**
          * @brief      Busy Off ��ȣ�� ��� ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nMode           ������(0=�޽� ��¿Ϸ�, 1=��ġ���� �Ϸ�)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetBusyOffMode(short nNmcNo, short nAxisNo, short nMode);

        /**
          * @brief      MPG��带 �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ ���ȣ(�׷캰 0,1��° �ุ ����)
          * @param[in]  nMode           MPG���(0=������, 1=�����޽�, 1=�����޽�, 2=�����޽�)
          * @param[in]  lPulse          �����޽� ���ڴ��� �Է´� ����� �޽��� ��
          * @return     PAIX_RETURN_VALUE
          * @warning    ������� ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMpgMode(short nNmcNo, short nAxisNo, short nMode, int lPulse);

        /**
          * @brief      MPG��带 �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ ���ȣ(�׷캰 0,1��° �ุ ����)
          * @param[in]  nRunMode        MPG �������(0=������, 1=ByPass, 2=Step ����)
          * @param[in]  nInMode         �Է¸��(0=1ü��, 1=2ü��, 2=4ü��, 3=2Pulse)
          * @param[in]  nDir            A,B �Է��� Counter ����(0=������, 1=������)
          * @param[in]  nX              ������ ü��(1~32)
          * @param[in]  nN              ������ ���ֺ�(1=������, 2~2048)
          * @return     PAIX_RETURN_VALUE
          * @warning    E-Version ���� ���˴ϴ�. ������� ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEMpg(short nNmcNo, short nAxisNo, short nRunMode, short nInMode, short nDir, short nX, short nN);

        /**
          * @brief      E-MPG�� Count����
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ ���ȣ(�׷캰 0,1��° �ุ ����)
          * @param[in]  nRunMode        MPG �������(0=������, 1=���)
          * @param[in]  nInMode         �Է¸��(0=1ü��, 1=2ü��, 2=4ü��, 3=2Pulse)
          * @param[in]  nDir            A,B �Է��� Counter ����(0=������, 1=������)
          * @return     PAIX_RETURN_VALUE
          * @warning    E-Version ���� ���˴ϴ�. ������� ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_EMpgCounterSet(short nNmcNo, short nAxisNo, short nRunMode, short nInMode, short nDir);

        /**
          * @brief      E-MPG�� Count�� �б�
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] plRetCount      Count���� ���� 4���� int�� �迭 ������(�� �׷��� 0,1�� �ุ MPG����� ���ǹǷ� �� �迭[0]=0��,[1]=1��,[2]=4��,[3]=5��
          * @return     PAIX_RETURN_VALUE
          * @warning    E-Version ���� ���˴ϴ�. ������� ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_EMpgCounterGet(short nNmcNo, int[] plRetCount);

        /**
          * @brief      ��ġ ��� ���� �ø��� ��� ����� Ȱ��ȭ�մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMode           ��� Ȱ��ȭ(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSerialMode(short nNmcNo, short nMode);

        /**
          * @brief      �ø��� ��� ȯ���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nBaud           ��żӵ�(0=9600, 1=19200, 2=38400 bps)
          * @param[in]  nData           ������ ��Ʈ�� (������ 0~7 = 1~8 bit)
          * @param[in]  nStop           ���� ��Ʈ �� (0 = 1, 1 = 2)
          * @param[in]  nParity         Parity ��Ʈ (0=None, 1=Odd, 2=Even)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSerialConfig(short nNmcNo, short nBaud, short nData, short nStop, short nParity);

        /**
          * @brief      ��ġ�� �����͸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nLen            ���� �������� ����Ʈ ��
          * @param[in]  pStr            ������ ���� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SerialWrite(short nNmcNo, short nLen, byte[] pStr);

        /**
          * @brief      ��ġ�� ������ �����͸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnReadLen       ���� �������� ����Ʈ ��(�ִ� 384bytes)
          * @param[out] pReadStr        ������ ������ ���� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SerialRead(short nNmcNo, out short pnReadLen, byte[] pReadStr);

        /**
          * @brief      �ﰢ���� ��������� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nAVTRIMode      ������� ����(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_AVTRISetMode(short nNmcNo, short nAxis, short nAVTRIMode);

        /**
          * @brief      �ﰢ���� ��������� �������¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[out] pnAVTRIMode     ������� ���¸� �о�� ������ ����(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_AVTRIGetMode(short nNmcNo, short nAxis, out short pnAVTRIMode);

        /**
          * @brief      Backlash, Slip���� ��� ����
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nMode           ��� ����(0:None, 1:Backlash, 2:Slip)
          * @param[in]  dAmount         �̵���
          * @param[in]  nCMask          ī���� ���ۿ���(Bit0:Counter1(����)����, Bit1:Counter2(���ڴ�), Bit3:Counter3(����), bit4:Counter4(����))
          * @param[in]  dSpeed          �����޽��� ��� �ӵ�(�̵�����/��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetBacklashSlip(short nNmcNo, short nAxisNo, short nMode, double dAmount, short nCMask, double dSpeed);

        /**
          * @brief      �Է½�ȣ�� ���Ͽ� ���͸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nEnc            [�⺻=0]���ڴ� ��ȣ�� ���ͼ���(0=50.862ns, 1=152ns)
          * @param[in]  nMPG            [�⺻=0]MPG ��ȣ�� ���ͼ���(0=50.862ns, 1=152ns)
          * @param[in]  nMSig           [�⺻=0]Limit, Near, Alarm, Inposition(0=101ns,  1=140us)
          * @param[in]  nDR             [�⺻=0]DR�Է½�ȣ(0=101ns, 1=32ms)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetInSigFilter(short nNmcNo, short nAxisNo, short nEnc, short nMPG, short nMSig, short nDR);

        /**
          * @brief      ��� ���� ���ð��� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  lWaitTime       ������ ������ð�(ms)
          */
        [DllImport("NMC2.dll")]
        internal static extern void nmc_SetWaitTime(short nNmcNo, int lWaitTime);

        /**
          * @brief      ��Ʈ �������� ���� ��Ʈ ������ �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  lPortNum        ������ ��Ʈ��ȣ
          * @return     PAIX_RETURN_VALUE
          * @warning    E-Version������ �����˴ϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetNetPortNum(short nNmcNo, int lPortNum);

        /**
          * @brief      Gantry ���� ������ �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ������ �׷� ��ȣ(0=(0����~3����), 1=(4����~7����))
          * @param[in]  nMain Gantry    ���� �� ��ȣ
          * @param[in]  nSub Gantry     ���� �� ��ȣ
          * @return     PAIX_RETURN_VALUE
          * @warning    �׷쿡 ���� ������ ���ִ� ������ �ٸ��Ƿ� �����Ͽ� �ֽʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetGantryAxis(short nNmcNo, short nGroupNo, short nMain, short nSub);

        /**
          * @brief      Gantry ���� ������ Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnEnable        Ȱ��ȭ ���°� ��ȯ�� �迭 ������(2��)
          * @param[out] pnMainAxes      ���� �� ��ȣ�� ��ȯ�� �迭 ������(2��)
          * @param[out] pnSubAxes       Gantry ���� �� ��ȣ�� ��ȯ�� �迭 ������(2��)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetGantryInfo(short nNmcNo, short[] pnEnable, short[] pnMainAxes, short[] pnSubAxes);

        /**
          * @brief      Gantry ���� ������ �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ������ �׷� ��ȣ(0=(0����~3����), 1=(4����~7����))
          * @param[in]  nEnable         Gantry ������ Ȱ��ȭ �մϴ�.(0=��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetGantryEnable(short nNmcNo, short nGroupNo, short nGantryEnable);

        /**
          * @brief      MDIO �Է»��¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnInStatus      �Է�pin 8���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMDIOInput(short nNmcNo, short[] pnInStatus);

        /**
          * @brief      MDIO ��»��¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnOutStatus     ���pin 8���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMDIOOutput(short nNmcNo, short[] pnOutStatus);

        /**
          * @brief      MDIO ��ü ��»��¸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  pnOutStatus     ���pin 8���� ��¼��� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutput(short nNmcNo, short[] pnOutStatus);

        /**
          * @brief      MDIO ���� Pin�� ��»��¸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nPinNo          ��� Pin ��ȣ
          * @param[in]  nOutStatus      ��� ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutPin(short nNmcNo, short nPinNo, short nOutStatus);

        /**
          * @brief      MDIO ���� Pin�� ��»��¸� ���� ��ŵ�ϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nPinNo          Pin ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutTogPin(short nNmcNo, short nPinNo);

        /**
          * @brief      MDIO �����ϴ� �������� Pin ��»��¸� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nCount          ��� Pin ����
          * @param[in]  pnPinNo         ��� Pin ��ȣ �迭 ������
          * @param[in]  pnStatus        ��� ������ �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutPins(short nNmcNo, short nCount, short[] pnPinNo, short[] pnStatus);

        /**
          * @brief      MDIO �����ϴ� �������� Pin ��»��¸� ���� ��ŵ�ϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nCount          Pin ����
          * @param[in]  pnPinNo         Pin ��ȣ �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutTogPins(short nNmcNo, short nCount, short[] pnPinNo);

        /**
          * @brief      DIO ����� �Է»��¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          DIO ��ġ��ȣ
          * @param[in]  pnInStatus      �Է�pin 64���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInput(short nNmcNo, short[] pnInStatus);

        /**
          * @brief      DIO/UDIO ����� �Է»��¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[in]  pnInStatus      �Է�pin 128���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          * @warning    DIO���� 64���� �Ѿ�� ���� Reserved ó���˴ϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInput128(short nNmcNo, short[] pnInStatus);

        /**
          * @brief      DIO ����� ��»��¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          DIO ��ġ��ȣ
          * @param[in]  pnOutStatus     ���pin 64���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOOutput(short nNmcNo, short[] pnOutStatus);

        /**
          * @brief      DIO/UDIO ����� ��»��¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[in]  pnOutStatus     ���pin 128���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          * @warning    DIO���� 64���� �Ѿ�� ���� Reserved ó���˴ϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOOutput128(short nNmcNo, short[] pnOutStatus);

        /**
          * @brief      DIO ����� ��� ���¸� �����մϴ�.
          * @param[in]  nNmcNo          DIO ��ġ��ȣ
          * @param[out] pnOutStatus     ���pin 64���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutput(short nNmcNo, short[] pnOutStatus);

        /**
          * @brief      DIO/UDIO ����� ��»��¸� �����մϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[out] pnOutStatus     ���pin 128���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          * @warning    DIO���� 64���� �Ѿ�� ���� Reserved ó���˴ϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutput128(short nNmcNo, short[] pnOutStatus);

        /**
          * @brief      DIO/UDIO ����� Pin �ϳ��� ��»��¸� �����մϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[in]  nPinNo          Pin ��ȣ
          * @param[in]  nOutStatus      ���pin 128���� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          * @warning    �𵨺� ��� Pin ������ Ȯ���Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutPin(short nNmcNo, short nPinNo, short nOutStatus);

        /**
          * @brief      DIO/UDIO ����� Pin �ϳ��� ��»��¸� ������ŵ�ϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[in]  nPinNo          Pin ��ȣ
          * @return     PAIX_RETURN_VALUE
          * @warning    �𵨺� ��� Pin ������ Ȯ���Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutTogPin(short nNmcNo, short nPinNo);

        /**
          * @brief      DIO/UDIO ����� �����ϴ� Pin���� ����� �����մϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[in]  nCount          ��� Pin ����
          * @param[in]  pnPinNo         Pin ��ȣ �迭 ������
          * @param[in]  pnStatus        ��� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          * @warning    �𵨺� ��� Pin ������ Ȯ���Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutPins(short nNmcNo, short nCount, short[] pnPinNo, short[] pnStatus);

        /**
          * @brief      DIO/UDIO ����� �����ϴ� Pin ����� ������ŵ�ϴ�.
          * @param[in]  nNmcNo          DIO/UDIO ��ġ��ȣ
          * @param[in]  nCount          Pin ����
          * @param[in]  pnPinNo         Pin ��ȣ �迭 ������
          * @return     PAIX_RETURN_VALUE
          * @warning    �𵨺� ��� Pin ������ Ȯ���Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutTogPins(short nNmcNo, short nCount, short[] pnPinNo);

        /**
          * @brief Ȯ���� DIO ��ǰ�� ��� �Լ��Դϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEXDIOInput(short nNmcNo, short[] pnInStatus);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEXDIOOutput(short nNmcNo, short[] pnOutStatus);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutput(short nNmcNo, short[] pnOutStatus);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutPin(short nNmcNo, short nPinNo, short nOutStatus);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutTogPin(short nNmcNo, short nPinNo);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutPins(short nNmcNo, short nCount, short[] pnPinNo, short[] pnStatus);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutTogPins(short nNmcNo, short nCount, short[] pnPinNo);

        /**
          * @brief      ��� Pin�� �����ð� ���ѱ���� �����Ͽ� ��� On/Off�� �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nIoType         IO ����(0=MDIO, 1=DIO, 2=EXDIO)
          * @param[in]  nPinNo          IO�������� �����Ǵ� Pin��ȣ
          * @param[in]  nOutStatus      ������ ��°� (0=Off, 1=On)
          * @param[in]  nTime           ������ �������ѽð�(ms)
          * @return     PAIX_RETURN_VALUE
          * @warning    �𵨺� ��� Pin ������ Ȯ���Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetOutLimitTimePin(short nNmcNo, short nIoType, short nPinNo, short nOn , int nTime);

        /**
          * @brief      ��� Pin�� ������ �����ð� ���ѱ���� Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nIoType         IO ����(0=MDIO, 1=DIO, 2=EXDIO)
          * @param[in]  nPinNo          IO�������� �����Ǵ� Pin��ȣ
          * @param[out] pnSet           ���� ���¸� �о�� ������ ����(0=�����ȵ�, 1=On, 2=Off)
          * @param[out] pnStatus        ���� ���ѽð��� ���¸� �о�� ������ ����(0=�����ȵ�, 1=���ѽð� ������, 2=���� �ð� ����)
          * @param[out] pnOutStatus     ���� Pin ��»��¸� �о�� ������ ����(0=Off, 1=On)
          * @param[out] pnRemainTime    ���� �ð��� �о�� ������ ����(ms)
          * @return     PAIX_RETURN_VALUE
          * @warning    �𵨺� ��� Pin ������ Ȯ���Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetOutLimitTimePin(short nNmcNo, short nIoType, short nPinNo, out short pnSet, out short pnStatus, out short pnOutStatus, out int pnRemainTime);

        /**
          * @brief      ��ġ�� ������ ���� �� ���ȯ�氪�� ��ġ ���� ROM�� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMode           ���� ����(0=���� �� ��Ǽ���, 1=��������, 2= ��Ǽ���)
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MotCfgSaveToROM(short nNmcNo, short nMode);

        /**
          * @brief      ��ġ�� �������� �ʱ�ȭ �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMode           �ʱ�ȭ ����(0=���� �� ��Ǽ���, 1=��������, 2= ��Ǽ���)
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MotCfgSetDefaultROM(short nNmcNo, short nMode);

        /**
          * @brief      ��ġ�� ROM�� ����� �������� ���� ��ġ�� �ݿ��մϴ�.(RAM->RAM)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMode           ������ ����(0=���� �� ��Ǽ���, 1=��������, 2= ��Ǽ���)
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MotCfgLoadFromROM(short nNmcNo, short nMode);

        /**
          * @brief      ��ġ�� ��ǰ ������ �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] plDeviceType    ������ ������� �����Ͻʽÿ�
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDeviceType(short nNmcNo, out int plDeviceType);

        /**
          * @brief      ��ġ�� ���� ���������� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnMotionType    ������� �� ��(0=None, 2, 4, 6, 8)
          * @param[out] pnDioType       DIO ����(0=None, 1=16/16, 2=32/32, 3=48/48, 4=64/64)
          * @param[out] pnEXDio         EXDIO ����(0=None, 1=In16, 2=Out16)
          * @param[out] pnMDio          MDIO ����(0=None, 1=8/8)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDeviceInfo(short nNmcNo, out short pnMotionType, out short pnDioType, out short pnEXDio, out short pnMDio);

        /**
          * @brief      ��ġ�� ���� ���������� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pNmcList         ���������� �о�� NMC_EQUIP_LIST ����ü ������
          * @return     ��ġ ����Ʈ�� ����
          */
        [DllImport("NMC2.dll")]
        internal static extern int nmc_GetEnumList(short[] pnIp, out NMCEQUIPLIST pNmcList);

        /**
          * @brief      UDIO ��ġ�� IO ���������� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnInCount       �Է� Pin�� ����
          * @param[out] pnOutCount      ��� Pin�� ����
          * @return     PAIX_RETURN_VALUE
          * @warning    UDIO ��ǰ������ ����˴ϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInfo(short nNmcNo, out short pnInCount, out short pnOutCount);

        /**
         * @brief ���� ������ ����ü
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCMAPDATA
        {
            internal  int         nMapCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
            internal  int[]       lMapData;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
            internal  double[]    dMapData;
        };

        /**
          * @brief      Mapping �̵��� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ���ȣ
          * @param[in]  dPos            �̵��� ��ġ
          * @param[in]  nMapIndex       MDIO�� Search Pin(0,1)
          * @param[in]  nOpt            �̵����(0=���, 1=����)
          * @param[in]  nPosType        Mapping �̵����� ������ ��ġ��(���ڴ� ��ġ, ������ġ)
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          * @warning    nmc_MapMove �Լ��� ��ġ���� ���ڴ� ��ġ�� ��ȯ�մϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MapMove(short nNmcNo, short nAxis, double dPos, short nMapIndex, short nOpt);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_MapMoveEx(short nNmcNo, short nAxis, double dPos, short nMapIndex, short nOpt, short nPosType);

        /**
          * @brief      ��� 0���� Mapping �����͸� �����ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMapIndex       MDIO�� Search Pin(0,1)
          * @param[out] pNmcMapData     Ȯ���� TNMC_MAP_DATA ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMapData(short nNmcNo, short nMapIndex, out NMCMAPDATA pNmcMapData);

        /**
          * @brief      Mapping �����͸� ��Ϻ��� �����ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMapIndex       MDIO�� Search Pin(0,1)
          * @param[in]  nDataIndex      ������ ��� ��ȣ (0~3)
          * @param[out] pNmcMapData     Ȯ���� TNMC_MAP_DATA ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @warning    �� ������ ������� �����Ͻʽÿ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMapDataEx(short nNmcNo, short nMapIndex, short nDataIndex, out NMCMAPDATA pNmcMapData);

        /**
          * @brief      MDIO�� Search 0, 1�� ���¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnMapInStatus   Search�� �Է� ���� �迭 ������(0=Off, 1=On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMapIO(short nNmcNo, short[] pnMapInStatus);

        /**
          * @brief      ī��Ʈ ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nPinNo          ��������� MDIO Pin��ȣ(-1=������� ����, 0~7)
          * @param[in]  nEdge           ī��Ʈ ����(0=Rising, 1=Falling, 2=Rising+Falling)
          * @param[in]  nOutStatus      ������ MDIO Pin�� ������ ��»���(0=Off, 1=On)
          * @param[in]  lCount          ī��Ʈ�� ����(1~65535)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SearchCountStart(short nNmcNo, short nPinNo, short nEdge, short nOutStatus, int lCount);

        /**
          * @brief      ī��Ʈ�� ���� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] plCount         ī��Ʈ ���� �о�� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SearchCountGet(short nNmcNo, out int plCount);

        /**
          * @brief      ī��Ʈ ����� ���� �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SearchCountStop(short nNmcNo);
        
        
        /**
          * @brief      Search Pin(0,1) �Է½� �����ӵ� Override ��ɽ���
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nPinNo          MDIO�� Search Pin(0,1)
          * @param[in]  nEdge           ī��Ʈ ����(0=Rising, 1=Falling, 2=Rising+Falling)
          * @param[in]  nAxisCount      ������ �� ����
          * @param[in]  pnAxisList      �� ��ȣ �迭 ������
          * @param[in]  pdSpeedList     Override�� �����ӵ� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SearchOverDrvSpeedStart(short nNmcNo, short nPinNo, short nEdge, short nAxisCount, short[] pnAxisList, double[] pdSpeedList);
        
        
        /**
          * @brief      Search Pin(0,1) �Է½� �����ӵ� Override ��ɻ��� �б�
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nPinNo          MDIO�� Search Pin(0,1)
          * @param[out] pnRetPinState   ������ Search Pin�� �Է� ���� (0=Off, 1=On)
          * @param[out] pnRetCount		Pin�� �Էµ� Ƚ���� ���� ������
          * @param[out] pdLatchPosList  �����ӵ� Override�� ������ Latch�� ������ġ�� ���� 8�� �迭�� ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SearchOverDrvSpeedGetStatus(short nNmcNo, short nPinNo, out short pnRetPinState, out short pnRetCount, double[] pdLatchPosList);
        
        /**
          * @brief      Search Pin(0,1) �Է½� �����ӵ� Override �������
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nPinNo          MDIO�� Search Pin(0,1)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SearchOverDrvSpeedStop(short nNmcNo, short nPinNo);


        /**
          * @brief      Trigger �������� ����ü
          */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCTRIGSTATUS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nRun;           /*<! Trigger ���� ����(0=�̽���, 1=������) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nOutLogic;      /*<! ������ ��� ����(0=Active High, 1=Active Low) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]  nMode;          /*<! ������ Trigger ����(0=�̽���, 1=Line Scan, 2=������ġ, 3=Range) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal int[]    nCount;         /*<! Trigger ��� ���� (0 ~ 65535) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            internal short[]  nDummy;         /*<! ������� */
        };

        /**
          * @brief      Trigger �Է� �� ��»���� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nInType         Trigger�� ����� �޽�(0=�����޽�, 1=���ڴ��޽�)
          * @param[in]  nOutLogic       ��·���(0=Active High, 1=Active Low)
          * @param[in]  nOutDelay       ��½����� �����ð�, �����ð���ŭ ����� ���(0~65535us)
          * @param[in]  nOutWidth       Trigger ��� �޽� ��(1~65536us)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetTriggerIO(short nNmcNo, short nAxisNo, short nInType, short nOutLogic, int nOutDelay, int nOutWidth);

        /**
          * @brief      ���۰� �� �������� ������ �ֱ� Pulse ���� Trigger�� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dStartPos       Trigger ������ġ
          * @param[in]  dEndPos         Trigger ������ġ
          * @param[in]  dInterval       ��� ����(����θ� ����)
          * @param[in]  nDir            Trigger ��� ����(0=�����, 1=Counter Up, 2=Counter Down)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_TriggerOutLineScan(short nNmcNo, short nAxisNo, double dStartPos, double dEndPos, double dInterval, short nDir);

        /**
          * @brief      ������ ���� ���� ������ġ���� Trigger�� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nCount          ������ġ ����
          * @param[in]  pdPosList       ������ġ�� �迭
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_TriggerOutAbsPos(short nNmcNo, short nAxisNo, short nCount, double[] pdPosList);

        /**
          * @brief      ������ ���� ���� �������� Trigger�� ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nCount          ������ġ ����(�ִ� 64��)
          * @param[in]  pdStartPosList  ��� ������ġ�� �迭
          * @param[in]  pdEndPosList    ��� ������ġ�� �迭
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_TriggerOutRange(short nNmcNo, short nAxisNo, short nCount, double[] pdStartPosList, double[] pdEndPosList);

        /**
          * @brief      Trigger�� ����մϴ�. nmc_SetTriggerIO �Լ��� ������ �޽��� ��ŭ ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ����� �� ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_TriggerOutOneShot(short nNmcNo, short nAxisNo);

        /**
          * @brief      Trigger ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_TriggerOutStop(short nNmcNo, short nAxisNo);

        /**
          * @brief      ��ü ���� Trigger ���¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pTriggerStatus  ���¸� �о�� NMC_TRIG_STATUS ����ü ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetTriggerStatus(short nNmcNo, out NMCTRIGSTATUS pTriggerStatus);

        /**
          * @brief      List Motion �������� ����ü
          */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCLMSTATUS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nRun;            /*<! ListMotion ���� ����(0=�̽���, 1=������) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nWait;           /*<! Queue����(0=������ ��� ����, 1=����߰���� ���) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nRemainNum;      /*<! ��� �ִ� ť������ �� */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal uint [] uiExeNum;        /*<! ���� ���� ����Ʈ ����� ��(0 ~ 4,294,967,295) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            internal short[] nDummy;          /*<! ������� */
        };

        /**
          * @brief      ListMotion ȯ�漳���� �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nEmptyMode      ���� �� ��ϵ� ��尡 ��� ������ ������ ����(0=��������, 1=����߰���� ���)
          * @param[in]  nIoType         ��� ���� IO Ÿ��(0=MDIO, 1=DIO, 2=�������)
          * @param[in]  nIoCtrlPinMask  IO PinMask ����(MDIO=0~0xFF, DIO=0~0xFFFF)
          * @param[in]  nIoCtrlEndVal   ����Ʈ��� �����, ��� Pin��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ListMotSetMode(short nNmcNo, short nAxisNo, short nEmptyMode, short nIoType, int nIoCtrlPinMask, int nIoCtrlEndVal);

        /**
          * @brief      ListMotion ��带 ����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nNodeCount      ������ ��� ����(�ִ� 25��)
          * @param[in]  pnMode          ��帶�� �����Ǵ� ������(0=�����ġ/��ٸ���, 1=������ġ/��ٸ���, 2=�����ġ/��-Curve, 3=������ġ/��-Curve)
          * @param[in]  pdPos           ��ϵǴ� ��帶�� �̵��� �Ÿ� �迭
          * @param[in]  pdStart         ���ۼӵ� �� �迭
          * @param[in]  pdAcc           ���ӵ� �� �迭
          * @param[in]  pdDec           ���ӵ� �� �迭
          * @param[in]  pdDrvSpeed      �����ӵ� �� �迭
          * @param[in]  pnIoCtrlVal     ��ϵǴ� ��帶�� ����� Pin ������
          * @param[out] pnRetErrCode    ��ȯ�Ǵ� �����ڵ�(0=������, 1=�����ʰ�)
          * @param[out] pnRetRemainNum  �� 512���� ���ۿ��� ���� ���� ��ȯ
          * @return     PAIX_RETURN_VALUE
        */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ListMotAddNodes(short nNmcNo, short nAxisNo, short nNodeCount, short[] pnMode,
                                                double[] pdPos, double[] pdStart, double[] pdAcc, double[] pdDec, double[] pdDrvSpeed,
                                                int[] pnIoCtrlVal, short[] pnRetErrCode, short[] pnRetRemainNum);

        /**
          * @brief      ListMotion ��� ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  dEndSpeed       ����� �ӵ�
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ListMotCloseNode(short nNmcNo, short nAxisNo, double dEndSpeed);

        /**
          * @brief      ListMotion ����� ����/�����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  nRunMode        ��� ����(0=����, 1=����)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ListMotRunStop(short nNmcNo, short nAxisNo, short nRunMode);

        /**
          * @brief      ��ü ���� ListMotion ���¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pListMotStatus  ���¸� �о�� NMC_LM_STATUS  ����ü ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ListMotGetStatus(short nNmcNo, out NMCLMSTATUS pListMotStatus);

        /**
         * @brief ���Ӻ��� ���� ���� ����ü
         * @remarks 2���� �׷쿡 ���� Ȯ���� ���� 2���� �迭 ���¸� �����ϴ�.
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCCONTISTATUS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal  short [] nContiRun;                 /*<! ���� ���� ���� ����(0=Stop, 1=Run) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal  short [] nContiWait;                /*<! Queue����(0=Queue�� ���� �� Data�� ����, \n
                                                                       1=ť������ ��带 ��� �����Ͽ� ���� ��� ������ ���) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal  short [] nContiRemainBuffNum;       /*<! ��� �ִ� ť ������ ��(0 ~ 1024) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal  uint  [] uiContiExecutionNum;       /*<! �������� ť ������ ��ġ(0 ~ 4,294,967,295) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal  short [] nContiStopReason;          /*<! �������� ���� ������ ���� ���� (E-Version only) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            internal  short [] nDummy;                    /*<! ������� */
        };

        /**
          * @brief      ���Ӻ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ���Ӻ����� ����� �׷� ��ȣ
          * @param[in]  nAVTRIMode      NMC2:�ﰢ���� �������(0:��Ȱ��ȭ, 1=Ȱ��ȭ)\n
                                        E-Ver:�︮�ú��� ���(0:��Ȱ��ȭ, 1=Ȱ��ȭ)
          * @param[in]  nEmptyMode      ���Ӻ��� Queue ���� ���� �� ����(0=����, 1=����߰� ���)
          * @param[in]  n1stAxis        �׷� �� ������ ����� �� ��ȣ
          * @param[in]  n2ndAxis        �׷� �� ������ ����� �� ��ȣ
          * @param[in]  n3rdAxis        �׷� �� ������ ����� �� ��ȣ
          * @param[in]  dMaxDrvSpeed    �ִ� �����ӵ�
          * @param[in]  nIoType         ��� IOŸ��(0=MDIO, 1=DIO, 2=��������)
          * @param[in]  nIoCtrlPinMask  ������� PinMask
          * @param[in]  nIoCtrlEndVal   ���� �� ���� ��°�
          * @return     PAIX_RETURN_VALUE
          * @warning    ���Ӻ����� ���� �׷쳻�� ������ ���۵˴ϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiSetMode(short nNmcNo, short nGroupNo, short nAVTRIMode, short nEmptyMode,
                    short n1stAxis, short n2ndAxis, short n3rdAxis, double dMaxDrvSpeed, short nIoType , int nIoCtrlPinMask , int nIoCtrlEndVal);

        /**
          * @brief      ���Ӻ����� ���Ǵ� Queue ���۸� �ʱ�ȭ �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ���Ӻ����� ����� �׷� ��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiSetNodeClear(short nNmcNo, short nGroupNo);

        /**
          * @brief      ���Ӻ����� ���������� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pContiStatus    ���Ӻ��� �������� NMC_CONTI_STATUS ����ü ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiGetStatus(short nNmcNo, out NMCCONTISTATUS pContiStatus);

        /**
          * @brief      2�� ���� ��� ���
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dPos[x]         �̵��� ������ġ(X,Y)
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeLine2Axis(short nNmcNo, short nGroupNo, double dPos0, double dPos1,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      3�� ���� ��� ���
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dPos[x]         �̵��� ������ġ(X,Y,Z)
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeLine3Axis(short nNmcNo, short nGroupNo, double dPos0, double dPos1, double dPos2,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      4�� ���� ��� ���
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dPos[x]         �̵��� ������ġ(X,Y,Z,U)
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeLine4Axis(short nNmcNo, short nGroupNo, double dPos0, double dPos1, double dPos2, double dPos3,
                double dStart, double dAcc, double dDec, double dDrvSpeed, int nIoCtrlVal);

        /**
          * @brief      ��ȣ���� ����� (�߽���ġ�� ȸ������)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dCenter[x]      ��ȣ �߽� ������ġ(X,Y)
          * @param[in]  dAngle          ��ȣ ȸ������(����=CW(������), ���=CCW(������))
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeArc(short nNmcNo, short nGroupNo, double dCenter0, double dCenter1, double dAngle,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      ��ȣ���� ����� (�߽���ġ�� ������ġ)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dCenter[x]      ��ȣ �߽� ������ġ(X,Y)
          * @param[in]  dEnd[x]         ��ȣ ���� ������ġ(X,Y)
          * @param[in]  nDir            ��ȣ ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeArcCE(short nNmcNo, short nGroupNo, double dCenter0, double dCenter1, double dEnd0, double dEnd1, short nDir,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      ��ȣ���� ����� (�����ġ�� ������ġ)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dPass[x]        ��ȣ ��� ������ġ(X,Y)
          * @param[in]  dEnd[x]         ��ȣ ���� ������ġ(X,Y)
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeArcPE(short nNmcNo, short nGroupNo, double dPass0, double dPass1, double dEnd0, double dEnd1,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      ��ȣ���� ����� (�������� ������ġ)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dRadius         ��ȣ�� ������
          * @param[in]  dEnd[x]         ��ȣ ���� ������ġ(X,Y)
          * @param[in]  nLen            ��ȣ �̵��Ÿ�(0=ª���Ÿ�, 1=��Ÿ�)
          * @param[in]  nDir            ��ȣ ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeArcRE(short nNmcNo, short nGroupNo, double dRadius, double dEnd0, double dEnd1,
                short nLen, short nDir, double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      �︮�ú��� ����� (�߽���ġ�� ȸ������)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dCenter[x]      ��ȣ �߽� ������ġ(X,Y)
          * @param[in]  dAngle          ��ȣ ȸ������(����=CW(������), ���=CCW(������))
          * @param[in]  dZPos           Z�� �̵� ������ġ
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeHelical(short nNmcNo, short nGroupNo, double dCenter0, double dCenter1, double dAngle, double dZPos,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      �︮�ú��� ����� (�߽���ġ�� ������ġ)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dCenter[x]      ��ȣ �߽� ������ġ(X,Y)
          * @param[in]  dEnd[x]         ��ȣ ���� ������ġ(X,Y)
          * @param[in]  nDir            ��ȣ ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  dZPos           Z�� �̵� ������ġ
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeHelicalCE(short nNmcNo, short nGroupNo, double dCenter0, double dCenter1, double dEnd0, double dEnd1, short nDir, double dZPos,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      �︮�ú��� ����� (�����ġ�� ������ġ)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dPass[x]        ��ȣ ��� ������ġ(X,Y)
          * @param[in]  dEnd[x]         ��ȣ ���� ������ġ(X,Y)
          * @param[in]  dZPos           Z�� �̵� ������ġ
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeHelicalPE(short nNmcNo, short nGroupNo, double dPass0, double dPass1, double dEnd0, double dEnd1, double dZPos,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      �︮�ú��� ����� (�������� ������ġ)
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  dRadius         ��ȣ�� ������
          * @param[in]  dEnd[x]         ��ȣ ���� ������ġ(X,Y)
          * @param[in]  nLen            ��ȣ �̵��Ÿ�(0=ª���Ÿ�, 1=��Ÿ�)
          * @param[in]  nDir            ��ȣ ȸ������(0=CW(������), 1=CCW(������))
          * @param[in]  dZPos           Z�� �̵� ������ġ
          * @param[in]  dStart          ���ۼӵ�
          * @param[in]  dAcc            ���ӵ�
          * @param[in]  dDec            ���ӵ�
          * @param[in]  dDrvSpeed       �����ӵ�
          * @param[in]  nIoCtrlVal      ���Pin ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiAddNodeHelicalRE(short nNmcNo, short nGroupNo, double dRadius, double dEnd0, double dEnd1, short nLen, short nDir, double dZPos,
                double dStart, double dAcc, double dDec, double dDrvSpeed , int nIoCtrlVal);

        /**
          * @brief      ���Ӻ��� ��� ����� �����մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiSetCloseNode(short nNmcNo, short nGroupNo);

        /**
          * @brief      ���Ӻ����� ����/���� �մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nGroupNo        ��ϵ� �׷��ȣ
          * @param[in]  nRunMode        ������(0=Stop, 1=Run)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContiRunStop(short nNmcNo, short nGroupNo, short nRunMode);

        /**
          * @brief      ���� ���� �޽� ��� ���θ� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         �� ��ȣ
          * @param[out] pnBusyStatus �޽� ��»���(0=��� Off, 1=��� On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetBusyStatus(short nNmcNo, short nAxisNo, out short pnBusyStatus);

        /**
          * @brief      ��� ��(8��)�� �޽� ��� ���θ� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnBusyStatus    �޽� ��»��� �迭 ������(0=��� Off, 1=��� On)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetBusyStatusAll(short nNmcNo, short[] pnBusyStatus);

        /**
          * @brief      ���� ���� ��ġ�� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         �� ��ȣ
          * @param[out] plCmdPos        ���� �о�� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetCmdPos(short nNmcNo, short nAxis, out int plCmdPos);

        /**
          * @brief      ���� ���ڴ� ��ġ�� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         �� ��ȣ
          * @param[out] plEncPos        ���� �о�� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEncPos(short nNmcNo, short nAxis, out int plEncPos);

        /**
          * @brief      ��� ��ġŸ���� �����Ͽ� ��ġ������ �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nListCount      ���� ȹ���� ��ġ���� ����(pdPotList�� �迭ũ��)
          * @param[in]  pnAxisList      �� ��ȣ �迭 ������ (����: 0 ~ 7)
          * @param[in]  pnPosMode       �࿡�� �о�� ��ġ ���� (0=Cmd, 1=Enc)
          * @param[out] pdPosList       ��ġ���� �о�� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetCmdEncListPos(short nNmcNo, short nListCount, short[] pnAxisList, short[] pnPosMode, double[] pdPosList);

        /**
          * @brief      ���� ���� �ӵ� ������ �о� �´�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         �ӵ� ������ �о�� �� ��ȣ
          * @param[in]  pSpeed          �ӵ� ������ �� PARASPEED ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @see        NMC_PARA_LOGIC
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetParaSpeed(short nNmcNo, short nAxisNo, out NMCPARASPEED pSpeed);

        /**
          * @brief      ������ġ ������ �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nAxisNo         ��ġ�� �о�� �� ��ȣ
          * @param[out] pdTargetPos     ��ġ�� �о�� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetParaTargetPos(short nNmcNo, short nAxisNo, out double pdTargetPos);

        /**
          * @brief      ��� ���� ��½�ȣ�� ���¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pOutStatus      ��»��¸� �о�� ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @see        NMC_AXES_MOTION_OUT
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetAxesMotionOut(short nNmcNo, out NMCAXESMOTIONOUT pOutStatus);

        /**
          * @brief      ���� ��¼ӵ��� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pDrvSpeed       ��¼ӵ��� �о�� ������ ����
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDriveAxesSpeed(short nNmcNo, double[] pDrvSpeed);

        /**
          * @brief      �Լ� ��� ������ 8�� ��ü�� ���ɼӵ��� ���ڴ��ӵ��� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pdCmdSpeed      ���ɼӵ��� �о�� �迭 ������
          * @param[out] pdEncSpeed      ���ڴ��ӵ��� �о�� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetAxesCmdEncSpeed(short nNmcNo, double[] pdCmdSpeed, double[] pdEncSpeed);

        /**
          * @brief      ��ġ�� ���� �� �������¸� Ȯ���մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pnStopInfo      �������¸� Ȯ���� �迭 ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetStopInfo(short nNmcNo, short[] pnStopMode);

        /**
          * @brief      ��ġ�� ���� �� ���¸� �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pNmcData        �� ���¸� �о�� ����ü ������
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetAxesExpress(short nNmcNo, out NMCAXESEXPR pNmcData);

        /**
          * @brief  �� ���� NMC �������� ����ü
          */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCAXISINFO
        {
            internal short   nBusy;               /*!< �޽� ��� ����(0=Idle, 1=Busy) */
            internal short   nError;              /*!< Error �߻� ����(0=None, 1=Error) */
            internal short   nNear;               /*!< ���� ���� �Է� ����(0=Off, 1=On) */
            internal short   nPLimit;             /*!< + Limit ���� �Է� ����(0=Off, 1=On) */
            internal short   nMLimit;             /*!< - Limit ���� �Է� ����(0=Off, 1=On) */
            internal short   nAlarm;              /*!< �˶� ���� �Է� ����(0=Off, 1=On) */
            internal short   nEmer;               /*!< �׷캰 EMG �Է� ����(0=Off, 1=On) */
            internal short   nSwPLimit;           /*!< SW +Limit �Է� ����(0=Off, 1=On) */
            internal short   nInpo;               /*!< Inposition �Է� ����(0=Off, 1=On) */
            internal short   nHome;               /*!< Home Search ���� ����(0=������, 1=None) */
            internal short   nEncZ;               /*!< ���ڴ� Z�� �Է� ����(0=Off, 1=On) */

            internal short   nOrg;                /*!< ���� ���� �Է� ����(0=Off, 1=On)(NMC-403S ������ ����) */

            internal short   nSReady;             /*!< Servo Ready �Է� ����(0=Off, 1=On) */
            internal short   nContStatus;         /*!< ���Ӻ��� ���� ����(0=�Ϸ�, 1=������) */
            internal short   nSwMLimit;           /*!< SW -Limit �Է� ����(0=Off, 1=On) */

            internal int     lEnc;                /*!< ���ڴ� ��ġ(UnitPerPulse ������� ����) */
            internal int     lCmd;                /*!< ���� ��ġ(UnitPerPulse ������� ����) */
            internal double  dEnc;                /*!< ���ڴ� ��ġ(UnitPerPulse ����) */
            internal double  dCmd;                /*!< ���� ��ġ(UnitPerPulse ����) */

            internal short   nCurrentOn;          /*!< ���� ���� ��� ����(0=Off, 1=On) */
            internal short   nServoOn;            /*!< Servo On ��� ����(0=Off, 1=On) */
            internal short   nDCCOn;              /*!< DCC ��� ����(0=Off, 1=On) */
            internal short   nAlarmResetOn;       /*!< Alarm Reset ��� ����(0=Off, 1=On) */

            internal short   nHomeSrchFlag;       /*!< �����̵� ���ۿ���(0=�̵��Ϸ�, 1=�̵���) */
            internal short   nHomeStatusFlag;     /*!< �����̵� ���� ���� ���� */

            internal short   nStopInfo;           /*!< ���� ���� */
        };

        /**
          * @brief  ��ü ��� DIO�� ������ NMC �������� ����ü
          */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCSTATEINFO
        {
            internal NMCAXESEXPR          NmcAxesExpr;
            internal NMCAXESMOTIONOUT     NmcAxesMotOut;
            internal NMCHOMEFLAG          HomeFlag;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]              nStopInfo;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            internal short[]              nInDio;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            internal short[]              nInExDio;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]              nInMDio;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            internal short[]              nOutDio;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            internal short[]              nOutExDio;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[]              nOutMDio;
        };

        /**
          * @brief      ��ġ�� ��������(�� ����,��� ���� ��½�ȣ,��������, ��������,DIO, EXDIO, MDIO) �о�ɴϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[out] pNmcStateInfo   ���������� �о�� ����ü ������
          * @param[in]  nStructSize     TNMC_STATE_INFO ����ü ũ��
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetStateInfo(short nNmcNo, out NMCSTATEINFO pNmcStateInfo, int nStructSize);

        /**
          * @brief      ��ġ�� ��������(�� ����,��� ���� ��½�ȣ,��������, ��������,DIO, EXDIO, MDIO)�� �� ���� ���� ����(�� ����,��� ���� ��½�ȣ,��������,��������)���� ����
          * @param[in]  nAxisNo         ������ �� ��ȣ
          * @param[in]  pState          ���������� �о�� ����ü ������
          * @param[out] pAxis           ������ ���������� ���� ����ü ������
          * @return     PAIX_RETURN_VALUE
          * @details    DLL���� ��ü �������� ����ü(PNMCSTATEINFO) �����κ��� �� ����(PNMCAXISINFO)�� �����մϴ�.
          */
        [DllImport("NMC2.dll")]
        internal static extern void nmc_StateInfoToAxisInfo(short nAxisNo, out NMCSTATEINFO pState, out NMCAXISINFO pAxis);

        /**
          * @brief      �޽� ����� Mask(�ະ Bit����)�� �����Ͽ� ����� ON/OFF�մϴ�.
          * @param[in]  nNmcNo          ��ġ��ȣ
          * @param[in]  nMask           �ະ(0~7)Bit����(1=Mask�����Ͽ� �޽� ���OFF, 0=Mask�� �������� �ʾ� �޽� ���)
          * @return     PAIX_RETURN_VALUE
          */
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetPulseOutMask(short nNmcNo, short nMask);
        
        /**
			  * @brief      ��ġ�� ����(Composition) ������ Ȯ���մϴ�.
			  * @param[in]  nNmfNo          ��ġ��ȣ
			  * @param[out] ptCompo         ���� ������ �о�� TNMF_COMPO ����ü ������
			  * @return     PAIX_RETURN_VALUE
			  */
				[DllImport("NMC2.dll")]
        internal static extern short nmf_GetCompo(short nNmfNo, out TNMF_COMPO ptCompo);


        /**************************************************************************************************/
        /**
          * @brief  ������ ������ ������, ���� NMC ��ġ���� �����Ǵ� �Լ� �� ����ü �Դϴ�.\n
                    �ű� �� �ֱ� ��ǰ�� ����ڲ����� �Ʒ� �Լ��� ������� ���ʽÿ�.
          */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCSTOPMODE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nEmg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nMLimit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nPLimit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nAlarm;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nNear;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal short[] nEncZ;
        };
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct NMCCONTSTATUS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal short[] nStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal short[] nExeNodeNo;
        };
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMDIOInPin(short nNmcNo, short nPinNo, out short pnInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMDIOInputBit(short nNmcNo, short nBitNo, out short pnInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutputBit(short nNmcNo, short nBitNo, short nOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutputTog(short nNmcNo, short nBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutputAll(short nNmcNo, short[] pnOnBitNo, short[] pnOffBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutputTogAll(short nNmcNo, short[] pnBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMDIOInput32(short nNmcNo, out int plInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetMDIOOutput32(short nNmcNo, out int plOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetMDIOOutput32(short nNmcNo, int lOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInPin(short nNmcNo, short nPinNo, out short pnInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInputBit(short nNmcNo, short nBitNo, out short pnInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutputBit(short nNmcNo, short nBitno, short nOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutputTog(short nNmcNo, short nBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutputAll(short nNmcNo, short[] pnOnBitNo, short[] pnOffBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutputTogAll(short nNmcNo, short[] pnBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInput64(short nNmcNo, out long plInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOOutput64(short nNmcNo, out long plOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutput64(short nNmcNo, long lOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOInput32(short nNmcNo, short nIndex, out int plInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetDIOOutput32(short nNmcNo, short nIndex, out int plOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDIOOutput32(short nNmcNo, short nIndex, int lOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEXDIOInPin(short nNmcNo, short nPinNo, out short pnInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEXDIOInputBit(short nNmcNo, short nBitNo, out short pnInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutputBit(short nNmcNo, short nBitNo, short nOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutputTog(short nNmcNo, short nBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutputAll(short nNmcNo, short[] pnOnBitNo, short[] pnOffBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutputTogAll(short nNmcNo, short[] pnBitNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEXDIOInput32(short nNmcNo, out int plInStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetEXDIOOutput32(short nNmcNo, out int plOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEXDIOOutput32(short nNmcNo, int lOutStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetFirmVersion(short nNmcNo, out byte pStr);// ������
        [DllImport("NMC2.dll")]
        internal static extern double nmc_GetUnitPerPulse(short nNmcNo, short nAxisNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetProtocolMethod(short nNmcNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetIPAddress(out short pnField0, out short pnField1, out short pnField2, out short pnField3);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDefaultIPAddress(short nNmcNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_DIOTest(short nNmcNo, short nMode, short nDelay);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_AccOffsetCount(short nNmcNo, short nAxisNo, int lPulse);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetTriggerCfg(short nNmcNo, short nAxis, short nCmpMode, int lCmpAmount, double dDioOnTime, short nPinNo, short nDioType, short nReserve);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetTriggerEnable(short nNmcNo, short nAxis, short nEnable);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetAxesCmdSpeed(short nNmcNo, double[] pDrvSpeed);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetAxesEncSpeed(short nNmcNo, double[] pdEncSpeed);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContRun(short nNmcNo, short nGroupNo, short nRunMode);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetContStatus (short nNmcNo, out NMCCONTSTATUS pContStatus);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetContNodeLine(short nNmcNo, short nGroupNo, short nNodeNo,
                short nAxisNo0, short nAxisNo1, double dPos0, double dPos1,
                double dStart, double dAcc, double dDec , double dDriveSpeed);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetContNodeLineIO(short nNmcNo, short nGroupNo, short nNodeNo,
                short nAxisNo0, short nAxisNo1, double dPos0, double dPos1,
                double dStart, double dAcc, double dDec , double dDriveSpeed, short nOnOff);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetContNode3Line(short nNmcNo, short nGroupNo, short nNodeNo,
                short nAxisNo0, short nAxisNo1, short nAxisNo2, double dPos0, double dPos1, double dPos2,
                double dStart, double dAcc, double dDec , double dDriveSpeed);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetContNode3LineIO(short nNmcNo, short nGroupNo, short nNodeNo,
                short nAxisNo0, short nAxisNo1, short nAxisNo2, double dPos0, double dPos1, double dPos2,
                double dStart, double dAcc, double dDec , double dDriveSpeed, short nOnOff);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetContNodeArc(short nNmcNo, short nGroupNo, short nNodeNo,
                short nAxisNo0, short nAxisNo1, double dCenter0, double dCenter1, double dAngle,
                double dStart, double dAcc, double dDec, double dDriveSpeed);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetContNodeArcIO(short nNmcNo, short nGroupNo, short nNodeNo,
                short nAxisNo0, short nAxisNo1, double dCenter0, double dCenter1, double dAngle,
                double dStart, double dAcc, double dDec, double dDriveSpeed, short nOnOff);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContNodeClear(short nNmcNo, short nGroupNo);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_ContSetIO(short nNmcNo, short nGroupNo, short nIoType, short nIoPinNo, short nEndNodeOnOff);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetDisconectedStopMode(short nNmcNo, int lTimeInterval, short nStopMode);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationRelCircle(short nNmcNo, short nAxisNo0, double CenterPulse0, double EndPulse0,
                short nAxisNo1, double CenterPulse1, double EndPulse1, short nDir);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_InterpolationAbsCircle(short nNmcNo, short nAxisNo0, double CenterPulse0, double EndPulse0,
                short nAxisNo1, double CenterPulse1, double EndPulse1, short nDir);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_GetGantryAxis(short nNmcNo, short[] pnMainAxes, short[] pnSubAxes);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetEncoderMode(short nNmcNo, short nAxisNo, short nMode);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetPulseMode(short nNmcNo, short nAxisNo, short nMode);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_Set2PulseDir(short nNmcNo, short nAxisNo, short nDir);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_Set1PulseDir(short nNmcNo, short nAxisNo, short nDir);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetPulseActive(short nNmcNo, short nAxisNo, short nPulseActive);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSpecialFunction(short nNmcNo, short nData);
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSyncSetup(short nNmcNo, short nMainAxisNo, short nSubAxisNoEnable0, short nSubAxisNoEnable1, short nSubAxisNoEnable2);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SetSync(short nNmcNo, short nGroupNo, short[] pnSyncGrpList0, short[] pnSyncGrpList1);// ������
        [DllImport("NMC2.dll")]
        internal static extern short nmc_SyncFree(short nNmcNo, short nGroupNo);// ������
        //------------------------------------------------------------------------------

    };
};
//------------------------------------------------------------------------------

//DESCRIPTION  'NMC Windows Dynamic Link Library'     -- *def file* description ....

