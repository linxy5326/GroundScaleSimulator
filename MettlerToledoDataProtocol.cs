using System;
using System.Diagnostics;
using System.Text;
using xabg.Core.Text;

namespace xabg.GroundScaleSimulator
{
    /// <summary>
    /// 梅特勒托利多汽车磅串行数据协议构建解析类
    /// 适用机型 IND880 INDT800 X1333等表头
    /// 标准格式示例数据报：02 31 30 20 20 31 36 39 36 30 20 20 20 20 30 30 0D 
    /// </summary>
    public class MettlerToledoDataProtocol
    {
        //起始标志 02 1个字节。
        private const byte STX = 0x02;
        private readonly byte SOH = 0x01;
        private readonly byte ADR = 0x31;

        //结束标志 0D 1个字节
        private byte CR = 0x0D;

        //校验和，仅在设置为有效时进行传输
        private const byte CHK = 0x00;
        //标准格式数据报总长度
        private const int DATALENGTH = 17;
        //重量数据长度为6个字节
        private const int WEIGHTCOUNT = 6;
        //扩展格式数据报文总长度
        private const int DATALENGTHEXTEND = 24;
        //重量数据长度为8个字节
        private const int WEIGHTCOUNTEXTEND = 8;


        //未找到指定数组的数据下标值时，返回值为-1
        private const int INDEXOFNONE = -1;

        //状态字 A SB1
        private byte _SWA = 0x00;
        //状态字 B SB2
        private byte _SWB = 0x00;
        //状态字 C SB3 
        private byte _SWC = 0x00;

        //扩展格式的状态字 SB4 
        private byte _SWD = 0x00;

        private byte[] weightData = new byte[6];
        private byte[] tareData = new byte[6];

        //扩展协议报文数据长度为24个字节，标准协议报文为17个字节。
        private byte[] _protocolData = new byte[24];

        private int _grossWeight;
        private int _tare;

        public event EventHandler<WeightParsingCompleteArgs> ParsingComplete;

        /// <summary>
        /// 获取毛重数据，单位Kg
        /// </summary>
        public int GrossWeight { get => _grossWeight; set => _grossWeight = value; }
        /// <summary>
        /// 获取皮重数据，单位Kg
        /// </summary>
        public int Tare { get => _tare; set => _tare = value; }

        /// <summary>
        /// 获取报文协议数据
        /// </summary>
        public byte[] ProtocolData { get => _protocolData; }

        /// <summary>
        /// 获取十六进制格式的报文协议
        /// </summary>
        public string ProtocolDataHex { get => ByteConvert.ByteToHex(_protocolData); }


        /// <summary>
        /// 数据解析方法
        /// </summary>
        /// <param name="dataBuffer">报文数据</param>
        /// <remarks>
        /// 该方法输出通过两个属性完成。
        /// </remarks>
        public void DataParsing(byte[] dataBuffer)
        {
            int sIndex = 0;
            int length = dataBuffer.Length;
            if (dataBuffer[0] != STX)
            {
                sIndex = Array.IndexOf(dataBuffer, STX);
                if (sIndex == INDEXOFNONE)
                {
                    //输出调试信息
                    Debug.WriteLine("地磅数据报文不正确。报文内容：" + ByteConvert.ByteToHex(dataBuffer));
                    _grossWeight = 0;
                    _tare = 0;
                    return;
                }
             int   tIndex = Array.IndexOf(dataBuffer, CR, sIndex);
                length = tIndex - sIndex + 1;

                //如果报文长度不对，放弃本次解析，退出方法。
                if (tIndex == INDEXOFNONE || length != DATALENGTH)
                {
                    //输出调试信息
                    Debug.WriteLine("地磅数据报文不正确。报文内容：" + ByteConvert.ByteToHex(dataBuffer));

                    _grossWeight = 0;
                    _tare = 0;
                    return;
                }
            }

            byte[] tempBuffer = new byte[length];
            Array.Copy(dataBuffer, sIndex, tempBuffer, 0, length);

            //第1个和第17个数据
            if (tempBuffer[0] == STX && tempBuffer[length - 1] == CR)
            {
                //分解状态字 状态字A 从LSB 开始 取 5 bit ,
                //2,1,0 表示小数位  
                //4，3 表示分度因子数 0，1 =X1； 1，0=X2; 1,1=X3
                //详见协议说明书。
                _SWA = tempBuffer[1];
                _SWB = tempBuffer[2];
                _SWC = tempBuffer[3];
                //读取数据符号
                byte SWBit = (byte)(_SWB & 0x02);
                //分解毛重数据5-10 20 31 36 39 36 30 
                //反转数组
                // Array.Reverse(data);
                Array.Copy(tempBuffer, 4, weightData, 0, 6);
                string strValue = Encoding.Default.GetString(weightData, 0, weightData.Length);

                //转换类型
                int.TryParse(strValue, out _grossWeight);

                //分解皮重数据 11-16 20 20 20 20 30 30
                Array.Copy(tempBuffer, 10, tareData, 0, 6);
                strValue = Encoding.Default.GetString(tareData, 0, tareData.Length);
                int.TryParse(strValue, out _tare);

                //转成负数
                if (SWBit == 2)
                {
                    _grossWeight *= -1;
                    _tare *= -1;
                }


                //事件通知
                WeightParsingCompleteArgs args = new WeightParsingCompleteArgs(_grossWeight, _tare);
                byte[] statusWord = { _SWC, _SWB, _SWA, 0 };
                args.WeightStatusWord = BitConverter.ToInt32(statusWord, 0);
                OnParsingComplete(args);
            }
            else
            {
                //输出调试信息
                Debug.WriteLine("地磅数据报文不正确。报文内容：" + ByteConvert.ByteToHex(dataBuffer));

                _grossWeight = 0;
                _tare = 0;
            }

            //清除数据
            // Array.Clear(weightData, 0, weightData.Length);
            // Array.Clear(tareData, 0, tareData.Length);
        }

        //数据包解析完成后
        private void OnParsingComplete(WeightParsingCompleteArgs args)
        {
            ParsingComplete?.Invoke(this, args);
        }


        //构建数据报文
        public void BiludProtocol(ToledoDataProtocolConfig config)
        {
            if (null == config) throw new ArgumentNullException("config");

            //读取输出方式
            switch (config.OutputModeSetting.InOutput)
            {
                case InputOutputMode.StandardOutput:
                    StandardOutput(config);
                    break;
                case InputOutputMode.ExtendOutput:
                    ExtendOutput(config);
                    break;
                case InputOutputMode.WeightValue:
                    WeightValue();
                    break;
                case InputOutputMode.SICSLevel0:
                    SICSLevel0Output(config);
                    break;
            }
        }

        //标准格式报文，长度17个字节
        private void StandardOutput(ToledoDataProtocolConfig config)
        {
            _protocolData = new byte[DATALENGTH];

            _protocolData[0] = STX;
            //状态字
            _protocolData[1] = config.Standard.SWA;
            _protocolData[2] = config.Standard.SWB;
            _protocolData[3] = config.Standard.SWC;

            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();

            //重量值 
            string strWeight = _grossWeight.ToString().PadLeft(6, ' ');
            //皮重值
            string strTare = _tare.ToString().PadLeft(6, ' ');

            //数据超长
            if (strWeight.Length > WEIGHTCOUNT)
            {
                strWeight = strWeight.Substring(0, 6);
            }
            if (strTare.Length > WEIGHTCOUNT)
            {
                strTare = strTare.Substring(0, 6);
            }
            byte[] weight = asciiEncoding.GetBytes(strWeight);
            byte[] trae = asciiEncoding.GetBytes(strTare);
            Array.Copy(weight, 0, _protocolData, 4, weight.Length);
            Array.Copy(trae, 0, _protocolData, 10, trae.Length);


            _protocolData[DATALENGTH - 1] = CR;

        }

        //扩展格式报文，长度24个字
        private void ExtendOutput(ToledoDataProtocolConfig config)
        {
            _protocolData = new byte[DATALENGTHEXTEND];
            _protocolData[0] = SOH;
            //状态字
            _protocolData[1] = ADR;
            _protocolData[2] = config.Standard.SWA;
            _protocolData[3] = config.Standard.SWB;
            _protocolData[4] = config.Standard.SWC;
            _protocolData[5] = config.Standard.SWD;

            //重量值

            //皮重值

            _protocolData[DATALENGTH - 1] = CR;
        }


        //ASCII码重量值  格式：2E 30 30 30 30 30 30 3D  .000000=
        private void WeightValue()
        {
            _protocolData = new byte[WEIGHTCOUNTEXTEND];
            _protocolData[0] = 0x2E;
            //重量值 
            string strWeight = _grossWeight.ToString().PadLeft(6, ' ');
            //数据超长
            if (strWeight.Length > WEIGHTCOUNT)
            {
                strWeight = strWeight.Substring(0, 6);
            }

            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            byte[] weight = asciiEncoding.GetBytes(strWeight);
            Array.Reverse(weight);
            Array.Copy(weight, 0, _protocolData, 1, weight.Length);
            _protocolData[WEIGHTCOUNTEXTEND - 1] = 0x3D;
        }

        /// <summary>
        /// SICS 格式 级别 0
        /// </summary>
        /// <param name="config"></param>
        private void SICSLevel0Output(ToledoDataProtocolConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _protocolData = new byte[] { 0x53,0x20,0x53,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x6B,0x67,0x0D,0x0A};
            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            string strWeight = _grossWeight.ToString().PadLeft(WEIGHTCOUNT, ' ');
            byte[] weight= asciiEncoding.GetBytes(strWeight);

            Array.Copy(weight, 0, _protocolData, 8, weight.Length);

        }
    }


    /// <summary>
    /// 地磅数据解析完成通知事件参数类
    /// </summary>
    public class WeightParsingCompleteArgs : EventArgs
    {
        private int _grossWeight;
        private int _tare;

        private int weightStatusWord;


        /// <summary>
        /// 获取或设置毛重数据，单位Kg
        /// </summary>
        public int GrossWeight { get => _grossWeight; }

        /// <summary>
        /// 获取或设置皮重数据，单位Kg
        /// </summary>
        public int Tare { get => _tare; }

        /// <summary>
        /// 获取或设置重量数据状态字，由三个字节组成，从低到高为C,B,A;
        /// </summary>
        public int WeightStatusWord { get => weightStatusWord; set => weightStatusWord = value; }

        /// <summary>
        /// 初始化参数实例
        /// </summary>
        public WeightParsingCompleteArgs() { }

        /// <summary>
        /// 初始化参数实例
        /// </summary>
        /// <param name="grossWeight"></param>
        /// <param name="tare"></param>
        public WeightParsingCompleteArgs(int grossWeight, int tare)
        {
            _grossWeight = grossWeight;
            _tare = tare;
        }

    }

    /// <summary>
    /// 数据校验码常用算法
    /// </summary>
    public sealed class DataCheckCode
    {
        /// <summary>
        /// 异或和校验 
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        public static byte XORByte(byte[] data)
        {
            byte Xor = 0;
            for (int i = 0; i < data.Length; i++)
            {
                Xor = (byte)(Xor + (Xor ^ data[i]));
            }

            return Xor;
        }


        /// <summary>
        /// 校验和
        /// </summary>
        /// <param name="data">byte 数组</param>
        /// <returns></returns>
        public static byte CheckSum(byte[] data)
        {
            byte Sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                Sum = (byte)(Sum + data[i]);
            }
            return (byte)((~Sum) + 1);
        }


        /// <summary>
        /// 异或和校验 
        /// </summary>
        /// <param name="hexString">命令字符串</param>
        /// <returns></returns>
        public static string XORHex(string hexString)
        {
            return XORHexToByte(hexString).ToString("X2");
        }



        /// <summary>
        /// 异或和校验 
        /// </summary>
        /// <param name="hexString">命令字符串</param>
        /// <returns></returns>
        public static byte XORHexToByte(string hexString)
        {

            if (string.IsNullOrEmpty(hexString)) return 0;
            hexString = hexString.Replace(" ", "");
            //CRC寄存器
            int CRCCode = 0;
            //将字符串拆分成为16进制字节数据然后两位两位进行异或校验
            for (int i = 1; i < hexString.Length / 2; i++)
            {
                string cmdHex = hexString.Substring(i * 2, 2);
                if (i == 1)
                {
                    string cmdPrvHex = hexString.Substring((i - 1) * 2, 2);
                    CRCCode = (byte)Convert.ToInt32(cmdPrvHex, 16) ^ (byte)Convert.ToInt32(cmdHex, 16);
                }
                else
                {
                    CRCCode = (byte)CRCCode ^ (byte)Convert.ToInt32(cmdHex, 16);
                }
            }
            return (byte)CRCCode;//返回16进制校验码
        }
    }
}
