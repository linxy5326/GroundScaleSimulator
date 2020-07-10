using System;
using System.Text;

namespace xabg.Core.Text
{
    /// <summary>
    /// 字节数据转换类
    /// </summary>
    public static class ByteConvert
    {
        #region 格式转换

        /// <summary>
        /// 返回Int16
        /// </summary>
        /// <param name="byteValue"></param>
        /// <returns></returns>
        public static Int16 ByteToInt16(byte[] byteValue)
        {
            byte[] temp = new byte[2];
            temp[0] = byteValue[1];
            temp[1] = byteValue[0];
            return BitConverter.ToInt16(temp, 0);
        }

        /// <summary>
        /// 返回指定字节的Int16值
        /// </summary>
        /// <param name="highByte">高位字节索引值</param>
        /// <param name="lowByte">低位字节索引值</param>
        /// <returns></returns>
        public static Int16 ByteToInt16(byte highByte, byte lowByte)
        {
            byte[] temp = new byte[2];
            temp[0] = lowByte;
            temp[1] = highByte;
            return BitConverter.ToInt16(temp, 0);
        }

        /// <summary>
        /// 转换十六进制字符串到字节数组
        /// </summary>
        /// <param name="msg">待转换字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] HexToByte(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return new byte[0];
            }
            if (msg.Length == 1) msg = "0" + msg;
            msg = msg.Replace(" ", "");//移除空格

            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                //convert each set of 2 characters to a byte and add to the array
                comBuffer[i / 2] = Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }

        /// <summary>
        /// 转换字节数组到十六进制字符串
        /// </summary>
        /// <param name="comByte">待转换字节数组</param>
        /// <returns>十六进制字符串</returns>
        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return builder.ToString().ToUpper();
        }

        /// <summary> 
        /// 字符串转16进制字节数组 
        /// </summary> 
        /// <param name="hexString"></param> 
        /// <returns></returns> 
        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "").Replace("\r\n", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        #endregion

        /// <summary>
        /// 从汉字转换到16进制
        /// </summary>
        /// <param name="s"></param>
        /// <param name="charset">编码,如"utf-8","gb2312"</param>
        /// <param name="fenge">是否每字符用逗号分隔</param>
        /// <returns></returns>
        public static string ToHex(string s, string charset, bool fenge)
        {
            if ((s.Length % 2) != 0)
            {
                s += " ";//空格
                //throw new ArgumentException("s is not valid chinese string!");
            }
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding(charset);
            byte[] bytes = chs.GetBytes(s);
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("{0:X}", bytes[i]);
                if (fenge && (i != bytes.Length - 1))
                {
                    str += string.Format("{0}", ",");
                }
            }
            return str.ToLower();
        }

        ///<summary>
        /// 从16进制转换成汉字
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="charset">编码,如"utf-8","gb2312"</param>
        /// <returns></returns>
        public static string UnHex(string hex, string charset)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("/n", "");
            hex = hex.Replace("//", "");
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格
            }
            // 需要将 hex 转换成 byte 数组。 
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。 
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message. 
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding(charset);
            return chs.GetString(bytes);
        }

        /// <summary>
        /// 将4个元素的一维byte数组转为一个整数,字节数组的低位是整型的低字节位 
        /// </summary>
        /// <param name="b">4个元素的一维byte数组</param>
        /// <exception cref="ArgumentException">参数无效</exception>
        /// <returns></returns>
        public static int byte2Int(byte[] b)
        {
            if (b.Length < 4) throw new ArgumentException("Byte数组长度不足4个。");
            int iOutcome = 0;
            //反转数组
            Array.Reverse(b);
            byte bLoop;

            for (int i = 0; i < 4; i++)
            {
                bLoop = b[i];
                iOutcome += (bLoop & 0xFF) << (8 * i);
            }
            return iOutcome;
        }
    }
}
