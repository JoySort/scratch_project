using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraLib.Lib.Util
{
    internal class CRCHelper
    {
        public static byte[] CRC16Calc(byte[] dataBuff, int index, int dataLen) // CRC 低位在前 高位在后
        {
            byte[] crc = new byte[2];
            int CRCResult = 0xFFFF;
            for (int i = 0; i < dataLen; i++)
            {
                CRCResult = CRCResult ^ dataBuff[index + i];
                for (int j = 0; j < 8; j++)
                {
                    if ((CRCResult & 1) == 1)
                        CRCResult = (CRCResult >> 1) ^ 0xA001;
                    else
                        CRCResult >>= 1;
                }
            }
            crc[0] = Convert.ToByte(CRCResult & 0xff);
            crc[1] = Convert.ToByte(CRCResult >> 8);

            return crc;
        }
    }
}
