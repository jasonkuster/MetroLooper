using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLooper.Audio
{
    class Helper
    {
        public static byte[] ConvertShortArrayToByteArray(short[] shortData)
        {
            int sizeInSamples = shortData.Length;
            byte[] byteData = new byte[sizeInSamples * sizeof(short)];
            Buffer.BlockCopy(shortData, 0, byteData, 0, byteData.Length);
            return byteData;
        }
    }
}
