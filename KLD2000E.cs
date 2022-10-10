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
                case InputOutputMode.StandardInput:

                    break;
            }
        }

        // 连续输出TF=2 报文格式 2E 30 30 30 30 30 30 3D 
        private void ContinuousOutput(D2000E_DPCfg config)
        {
            BufferLength = 8;
            _protocolData = new byte[BufferLength];
            _protocolData[0] = STX;
            _protocolData[7] = ETX;

            string strWeight = GrossWeight.ToString().PadLeft(WEIGHTCOUNT, '0');

            //数据超长
            if (strWeight.Length > WEIGHTCOUNT)
            {
                strWeight = strWeight.Substring(0, WEIGHTCOUNT);
            }

            byte[] weight = asciiEncoding.GetBytes(strWeight);

            Array.Reverse(weight);
            //复制
            Array.Copy(weight, 0, _protocolData, 1, weight.Length);

            //if (config.SignedNumber == 0x2D)
            //    _protocolData[6] = config.SignedNumber;
            //else
            //    _protocolData[6] = 48;
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
        public const int DATA_LENGTH = 8;

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
