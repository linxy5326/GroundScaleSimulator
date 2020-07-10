using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xabg.Core.Text;

namespace xabg.GroundScaleSimulator
{
    /// <summary>
    /// 地磅仪表连续输出重量数据解析接口
    /// </summary>
    public interface IOutWeight
    {
        /// <summary>
        /// 接口方法，返回解析后的重量值,
        /// </summary>
        /// <param name="srcData">仪表输出数据</param>
        /// <returns></returns>
        int WeightData(byte[] srcData);
    }


    /// <summary>
    /// 地磅通用协议，连续输出方式
    /// </summary>
    public class CurrentProtocol : IOutWeight
    {
        /// <summary>
        /// 报文数据长度
        /// </summary>
        public virtual int Lenght { get; set; }
        /// <summary>
        /// 校验字起始地址
        /// </summary>
        public virtual int CRCStartIndex { get; set; }
        /// <summary>
        /// 校验字个数
        /// </summary>
        public virtual int CRCCount { get; set; }

        /// <summary>
        /// 报文起始字标识符
        /// </summary>
        public virtual int XON { get; set; }

        /// <summary>
        /// 报文结尾标识符
        /// </summary>
        public virtual int XOFF { get; set; }

        public virtual int WeightData(byte[] srcData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异或校验
        /// </summary>
        /// <param name="data">计算校验值的数据</param>
        /// <returns></returns>
        public virtual byte XORCRC(byte[] data)
        {
            byte xorValue = 0;
            for (int i = 0; i < data.Length; i++)
            {
                xorValue ^= data[i];
            }
            return xorValue;
        }


        /// <summary>
        /// 十六进制校验值转10进制
        /// </summary>
        /// <param name="hexValue">十六进制校验值</param>
        /// <returns></returns>
        public virtual byte CRCConvert(byte hexValue)
        {
            //  return (byte)Convert.ToInt32(XorCRC.ToString(), 16);
            return (byte)Int32.Parse(hexValue.ToString(), System.Globalization.NumberStyles.HexNumber);
        }
    }

    /// <summary>
    /// 报文构建配置类
    /// </summary>
    public class XK3190DataProtocolConfig
    {

        /// <summary>
        /// 起始符
        /// </summary>
        public const byte XON = 0x02;

        /// <summary>
        /// 结束符
        /// </summary>
        public const byte XOFF = 0x03;

        /// <summary>
        /// 正符号标识
        /// </summary>
        public const byte PLUS_SIGN = 0x2B;

        /// <summary>
        /// 负符号标识
        /// </summary>
        public const byte MINUS_SIGN = 0x2D;

        /// <summary>
        /// 报文长度
        /// </summary>
        public const int DATA_LENGTH = 12;

        /// <summary>
        /// 小数位
        /// </summary>
        public byte DecimalPlaces { get; set; }

        public SerialSetting SerialPortSetting { get; set; }

        /// <summary>
        /// 获取或设置输出模式
        /// </summary>
        public DataOutputConfig OutputModeSetting { get; set; }

        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 设置+/-符号
        /// </summary>
        public byte SignedNumber { get; set; }


        public string ModeName { get; set; }

        private static readonly XK3190DataProtocolConfig _defaultConfig;

        public static XK3190DataProtocolConfig DefaultConfig { get => _defaultConfig; }
        static XK3190DataProtocolConfig()
        {
            if (null == _defaultConfig)
            {
                _defaultConfig = new XK3190DataProtocolConfig();
                _defaultConfig.UpdateTime = null;
                _defaultConfig.SerialPortSetting = new SerialSetting();
                _defaultConfig.OutputModeSetting = new DataOutputConfig
                {
                    InOutput = InputOutputMode.StandardOutput,
                    ModeScene = WeightModeScene.UpToPound,
                    PeakValue = 48500,
                    StartValue = 0,
                    StepLength = 20
                };

                //设置默认小数位
                _defaultConfig.DecimalPlaces = 0;
                //正值
                _defaultConfig.SignedNumber = PLUS_SIGN;
            }
        }

        public XK3190DataProtocolConfig()
        {
            SerialPortSetting = new SerialSetting();
            OutputModeSetting = new DataOutputConfig();
            UpdateTime = DateTime.Now;
            ModeName = "";
        }
    }

    /// <summary>
    /// 耀华XK3190-A9+  仪表的连续输出协议
    /// 0x02, 0x2B, 0x30, 0x35, 0x33, 0x32, 0x38, 0x30, 0x30, 0x31, 0x37, 0x03
    /// </summary>
    public class XK3190A9P : CurrentProtocol
    {
        //重量数据长度为6个字节
        public const int WEIGHTCOUNT = 6;

        //报文数据
        private byte[] _protocolData;

        private ASCIIEncoding asciiEncoding;
        /// <summary>
        /// 获取报文协议数据
        /// </summary>
        public byte[] ProtocolData { get => _protocolData; }

        /// <summary>
        /// 获取十六进制格式的报文协议
        /// </summary>
        public string ProtocolDataHex { get => ByteConvert.ByteToHex(_protocolData); }

        /// <summary>
        /// 获取毛重数据，单位Kg
        /// </summary>
        public int GrossWeight { get; set; }

        public XK3190A9P()
        {
            XON = XK3190DataProtocolConfig.XON;
            XOFF = XK3190DataProtocolConfig.XOFF;
            Lenght = XK3190DataProtocolConfig.DATA_LENGTH;
            CRCStartIndex = 9;
            CRCCount = 2;

            asciiEncoding = new System.Text.ASCIIEncoding();
        }

        /// <summary>
        /// 从接收到的数据报文中解析重量数据，单位Kg。报文最小长度，12个字节。能处理错包问题。
        /// </summary>
        /// <param name="srcData">报文数据。格式：XON=02  XOFF=03  符号位=2B/2D </param>
        /// <returns></returns>
        public override int WeightData(byte[] srcData)
        {
            int weight = 0;

            int index = 0;
        //从index 指定的下标开始，读取数据。
        RE:
            if (srcData.Length >= Lenght && srcData[index] == XON && srcData[index + 11] == XOFF)
            {
                byte xorValue = (byte)((((srcData[index + 9] ^ 0x30) & 0x0f) << 4) + (srcData[index + 10] ^ 0x30));

                byte[] data = new byte[8];
                Array.Copy(srcData, index + 1, data, 0, data.Length);



                //计算校验值
                if (xorValue == XORCRC(data))
                {
                    for (int i = 1; i < data.Length - 2; i++)
                    {
                        weight += (data[i] ^ 0x30) * (int)(Math.Pow(10, data.Length - 2 - i));
                    }
                }
                else
                {
                    //如果校验失败， 跳过当前数据包，判断余下的数据是否够一包。
                    index += Lenght;
                    if (srcData.Length - index >= Lenght)
                        goto RE;
                }
                //负值加上符号位
                byte mark = data[0];
                if (mark == 45)
                {
                    weight = 0 - weight;
                }
            }
            else
            {
                if (srcData.Length - index >= Lenght && srcData[index] != XON)
                {
                    index++;
                    //跳到数组的下一个元素开始，重新解读
                    goto RE;
                }
            }

            return weight;
        }

        public void BiludProtocol(XK3190DataProtocolConfig config)
        {
            if (null == config) throw new ArgumentNullException("config");

            if (config.OutputModeSetting.InOutput == InputOutputMode.StandardOutput)
            {
                StandardOutput(config);
            }
        }


        //构建标准连续输出
        private void StandardOutput(XK3190DataProtocolConfig config)
        {
            _protocolData = new byte[XK3190DataProtocolConfig.DATA_LENGTH];
            _protocolData[0] = XK3190DataProtocolConfig.XON;
            _protocolData[11] = XK3190DataProtocolConfig.XOFF;
            _protocolData[1] = config.SignedNumber;
            //小数位，最多不能超过4位
            _protocolData[8] = (byte)(48 + config.DecimalPlaces);

            string strWeight = GrossWeight.ToString().PadLeft(6, '0');

            //数据超长
            if (strWeight.Length > WEIGHTCOUNT)
            {
                strWeight = strWeight.Substring(0, 6);
            }

            byte[] weight = asciiEncoding.GetBytes(strWeight);
            //复制
            Array.Copy(weight, 0, _protocolData, 2, weight.Length);

            //校验
            byte[] crcValue = new byte[2];

            //读出计算校验值的数据
            byte[] data = new byte[8];
            Array.Copy(_protocolData, 1, data, 0, data.Length);

            //校验值
            byte CRCValue = XORCRC(data);


            //取高4位
            byte h = (byte)(((CRCValue & 0xF0) >> 4) + 48);
            byte l = (byte)((CRCValue & 0x0F) + 48);
            //取低4位

            _protocolData[9] = h;
            _protocolData[10] = l;

        }

        /// <summary>
        /// 地磅仪表型号
        /// </summary>
        public enum PoundGaugeType
        {
            /// <summary>
            /// 未知
            /// </summary>
            None = 0,
            /// <summary>
            /// 耀华XK3190-A9+
            /// </summary>
            XK3190A9P = 1,
            /// <summary>
            /// 托利多T800
            /// </summary>
            TLDT800 = 2
        }
    }
}
