using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xabg.Core.Text;

namespace xabg.GroundScaleSimulator
{
    /// <summary>
    /// 柯力 D2000 系列  T2 连续输出方式报文格式
    /// </summary>
    public class KLD2000E
    {
        //起始标志 02 1个字节。
        private const byte STX = 0x2E;
        //标准报文结束
        private readonly byte ETX = 0x3D;
        //重量数据长度为6个字节
        public const int WEIGHTCOUNT = 6;
        private int _grossWeight;

        //扩展协议报文数据长度为24个字节，标准协议报文为17个字节。
        private byte[] _protocolData = new byte[20];
        private int bufferLength = 20;
        //重量数据
        private byte[] weightData = new byte[6];


        System.Text.ASCIIEncoding asciiEncoding;
        /// <summary>
        /// 获取毛重数据，单位Kg
        /// </summary>
        public int GrossWeight { get => _grossWeight; set => _grossWeight = value; }

        public event EventHandler<WeightParsingCompleteArgs> ParsingComplete;

        /// <summary>
        /// 获取报文协议数据
        /// </summary>
        public byte[] ProtocolData { get => _protocolData; }

        /// <summary>
        /// 获取十六进制格式的报文协议
        /// </summary>
        public string ProtocolDataHex { get => ByteConvert.ByteToHex(_protocolData); }
        public int BufferLength { get => bufferLength; set => bufferLength = value; }

        public KLD2000E()
        {
            asciiEncoding = new System.Text.ASCIIEncoding();
        }

        public void BiludProtocol(D2000E_DPCfg config)
        {
            if (null == config) throw new ArgumentNullException("config");

            //读取输出方式
            switch (config.OutputModeSetting.InOutput)
            {
                case InputOutputMode.ASCII_8:
                    ContinuousOutput(config);
                    break;
                case InputOutputMode.ASCII_9:
                    ContinuousOutputASCII_9(config);
                    break;
                case InputOutputMode.StandardOutput:
                    StandarOutput(config);
                    break;
            }
        }

        private void StandarOutput(D2000E_DPCfg config)
        {
            BufferLength = config.DataLenght;
            _protocolData = new byte[config.DataLenght];
            _protocolData[0] = 0x02;
            _protocolData[11] = 0x03;
            _protocolData[1] = config.SignedNumber;
            //小数位，最多不能超过4位
            _protocolData[8] = (byte)(48 + config.DecimalPlaces);

            //string strWeight = GrossWeight.ToString().PadLeft(6, '0');
            string strWeight = Math.Abs(GrossWeight).ToString().PadLeft(6, '0');
            //数据超长
            if (strWeight.Length > 6)
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

        // 连续输出TF=2 报文格式 2E 30 30 30 30 30 30 3D 
        private void ContinuousOutput(D2000E_DPCfg config)
        {
            BufferLength = 8;
            _protocolData = new byte[BufferLength];
            _protocolData[0] = STX;
            _protocolData[7] = ETX;


            string strWeight = Math.Abs(GrossWeight).ToString().PadLeft(5, '0');
            //数据超长
            if (strWeight.Length > 5)
            {
                strWeight = strWeight.Substring(0, 5);
            }

            byte[] weight = asciiEncoding.GetBytes(strWeight);

            Array.Reverse(weight);
            //复制
            Array.Copy(weight, 0, _protocolData, 1, weight.Length);
            if (config.SignedNumber == 0x2D)
            {
                _protocolData[6] = 0x2D;
            }
            else
            {
                _protocolData[6] = 0x30;
            }
        }

        private void ContinuousOutputASCII_9(D2000E_DPCfg config)
        {
            BufferLength =9;
            _protocolData = new byte[BufferLength];
            _protocolData[0] = STX;
            _protocolData[8] = ETX;

            string strWeight = Math.Abs(GrossWeight).ToString().PadLeft(6,'0');
           
            //数据超长
            if (strWeight.Length > 6)
            {
                strWeight = strWeight.Substring(0, 6);
            }

            byte[] weight = asciiEncoding.GetBytes(strWeight);

            Array.Reverse(weight);
            //复制
            Array.Copy(weight, 0, _protocolData, 1, weight.Length);

            if (config.SignedNumber == 0x2D)
            {
                _protocolData[7] = 0x2D;
            }
            else
            {
                _protocolData[7] = 0x30;
            }
        }

        /// <summary>
        /// 异或校验
        /// </summary>
        /// <param name="data">计算校验值的数据</param>
        /// <returns></returns>
        public byte XORCRC(byte[] data)
        {
            byte xorValue = 0;
            for (int i = 0; i < data.Length; i++)
            {
                xorValue ^= data[i];
            }
            return xorValue;
        }
    }

    /// <summary>
    /// 协议配置类
    /// </summary>
    public class D2000E_DPCfg
    {
        /// <summary>
        /// 起始符
        /// </summary>
        public const byte XON = 0x2E;
        /// <summary>
        /// 结束符
        /// </summary>
        public const byte XOFF = 0x3D;
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
        private  int _dataLenght = 12;

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


        private static readonly D2000E_DPCfg _defaultConfig;

        public static D2000E_DPCfg DefaultConfig { get => _defaultConfig; }

        /// <summary>
        /// 报文字节数
        /// </summary>
        public int DataLenght { get => _dataLenght; set => _dataLenght = value; }

        static D2000E_DPCfg()
        {
            if (null == _defaultConfig)
            {
                _defaultConfig = new D2000E_DPCfg();
                _defaultConfig.UpdateTime = null;
                _defaultConfig.ModeName = "KL-D2000E";
                _defaultConfig.SerialPortSetting = new SerialSetting();
                _defaultConfig.OutputModeSetting = new DataOutputConfig
                {
                    InOutput = InputOutputMode.StandardOutput,
                    ModeScene = WeightModeScene.UpToPound,
                    PeakValue = 48500,
                    StartValue = 0,
                    StepLength = 20
                };

                _defaultConfig.DataLenght = 12;
                //设置默认小数位
                _defaultConfig.DecimalPlaces = 0;
                //正值
                _defaultConfig.SignedNumber = PLUS_SIGN;
            }
        }

        public D2000E_DPCfg()
        {
            SerialPortSetting = new SerialSetting();
            OutputModeSetting = new DataOutputConfig();
            UpdateTime = DateTime.Now;
            ModeName = "KL-D2000E";
        }
    }
}
