using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSMSpirometer
{

    public class DeviceProperties
    {
        public const String DeviceInstanceId = "System.Devices.DeviceInstanceId";
    }

    public enum DeviceType
    {
        OsrFx2,
        SuperMutt,
        All,    // Can be any device
        None
    };

    public class OsrFx2
    {
        public static class VendorCommand
        {
            public const Byte GetSevenSegment = 0xD4;
            public const Byte GetSwitchState = 0xD6;
            public const Byte SetSevenSegment = 0xDB;
        }

        public class Pipe
        {
            public const UInt32 InterruptInPipeIndex = 0;
            public const UInt32 BulkInPipeIndex = 0;
            public const UInt32 BulkOutPipeIndex = 0;
        }


        // Seven Segment Masks and their associated numeric values
        public static Byte[] SevenLedSegmentMask =
            {
                0xD7, // 0
                0x06, // 1
                0xB3, // 2
                0xA7, // 3
                0x66, // 4
                0xE5, // 5
                0xF4, // 6
                0x07, // 7
                0xF7, // 8
                0x67, // 9
            };

        // TODO
        public const UInt16 DeviceVid = 0x2102;
        public const UInt16 DevicePid = 0x0001;
        //public const UInt16 DeviceVid = 0x045E;
        //public const UInt16 DevicePid = 0x078E;
    }

    public class SuperMutt
    {
        public class VendorCommand
        {
            public const Byte GetLedBlinkPattern = 0x03;
            public const Byte SetLedBlinkPattern = 0x03;
        }

        public class Pipe
        {
            public const UInt32 InterruptInPipeIndex = 0;
            public const UInt32 InterruptOutPipeIndex = 0;
            public const UInt32 BulkInPipeIndex = 0;
            public const UInt32 BulkOutPipeIndex = 0;
        }

        public const UInt16 DeviceVid = 0x2102;
        public const UInt16 DevicePid = 0x0001;
        public static Guid DeviceInterfaceClass = new Guid("{875d47fc-d331-4663-b339-624001a2dc5e}");
    }

   
}
