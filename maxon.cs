using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EposCmd.Net;
using EposCmd.Net.DeviceCmdSet.Operation;
using System.Windows.Forms;

namespace BinderJetMotionControllerVer._1
{
    class Maxon
    {
        private DeviceManager _connector;
        private Device _epos;
        StateMachine sm;

        internal void Connect()
        {
            _connector = new DeviceManager("EPOS4", "MAXON SERIAL V2", "USB", "USB0");

            _connector.Baudrate = 1000000;
            _connector.Timeout = 500;
            // Create new EPOS device,
            _epos = _connector.CreateDevice(1 /*Node ID*/);
            // Clear errors
            _epos.Operation.StateMachine.ClearFault();
            // Each method have the access to the lastError property.
            // This way you can additionally check the error code of the function call.
            //Console.WriteLine("Last error:" + sm.LastError);
            sm = _epos.Operation.StateMachine;
            //return _epos.Operation.OperationMode.GetOperationModeAsString();
        }

        internal void Disconnect()
        {
            if (_connector != null)
            {
                /*
                 * Important notice:
                 * It's recommended to call the Dispose function before application close
                 */
                _connector.Dispose();
            }
        }

        internal void Enable()
        {
            _epos.Operation.ProfileVelocityMode.ActivateProfileVelocityMode();
            sm.SetEnableState();
            //return _epos.Operation.OperationMode.GetOperationModeAsString();
        }

        internal void Disable()
        {
            if (sm != null)
            {
                sm.SetDisableState();
            }
        }

        internal void MotorMove_CW_CCW(int speed, uint acc, uint dec, bool path)
        {
            if(path)
            {
                _epos.Operation.ProfileVelocityMode.SetVelocityProfile(acc, dec);
                _epos.Operation.ProfileVelocityMode.MoveWithVelocity(speed);
            }
            else if(!path)
            {
                speed = speed * (-1);
                _epos.Operation.ProfileVelocityMode.SetVelocityProfile(acc, dec);
                _epos.Operation.ProfileVelocityMode.MoveWithVelocity(speed);
            }
        }

        internal void MotorMoveCW(int speed, uint acc, uint dec)
        {
            _epos.Operation.ProfileVelocityMode.SetVelocityProfile(acc, dec);
            _epos.Operation.ProfileVelocityMode.MoveWithVelocity(speed);
        }

        internal void MotorMoveCCW(int speed, uint acc, uint dec)
        {
            speed = speed * (-1);
            _epos.Operation.ProfileVelocityMode.SetVelocityProfile(acc, dec);
            _epos.Operation.ProfileVelocityMode.MoveWithVelocity(speed);
        }

        internal void MotorStop()
        {
            _epos.Operation.ProfileVelocityMode.HaltVelocityMovement();
        }

    }
}
