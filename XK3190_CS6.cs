using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xabg.Core.Text;

namespace xabg.GroundScaleSimulator
{
    /// <summary>
    /// 耀华XK3190-CS6 仪表通信协议
    /// 
    /// </summary>
    public class XK3190_CS6
    {

        //起始标志 02 1个字节。
        private const byte STX = 0x02;
        private readonly byte SOH = 0x01;
        private readonly byte ADR = 0x31;
        //标准报文结束
        private readonly byte ETX = 0x03;
        //重量数据长度为6个字节
        public const int WEIGHTCOUNT = 6;
        private int _grossWeight;
        private int _tare;

        //扩展协议报文数据长度为24个字节，标准协议报文为17个字节。
        private byte[] _protocolData = new byte[128];
        //重量数据
        private byte[] weightData = new byte[6];

        System.Text.ASCIIEncoding asciiEncoding;
        /// <summary>
        /// 获取毛重数据，单位Kg
        /// </summary>
        public int GrossWeight { get => _grossWeight; set => _grossWeight = value; }

        public event EventHandler<WeightParsingCompleteArgs> ParsingComplete;

        //数据包解析完成后
        private void OnParsingComplete(WeightParsingCompleteArgs args)
        {
            ParsingComplete?.Invoke(this, args);
        }

        /// <summary>
        /// 获取报文协议数据
        /// </summary>
        public byte[] ProtocolData { get => _protocolData; }

        /// <summary>
        /// 获取十六进制格式的报文协议
        /// </summary>
        public string ProtocolDataHex { get => ByteConvert.ByteToHex(_protocolData); }


        public XK3190_CS6()
        {
            asciiEncoding = new System.Text.ASCIIEncoding();
        }


        //构建数据报文
        public void BiludProtocol(XK3190_CS6Config config)
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
                case InputOutputMode.Answering:
                    AnsweringOutput(config);
                    break;
                case InputOutputMode.StandardInput:
                    StandardInput(config);
                    break;
            }
        }


        private void StandardInput(XK3190_CS6Config config)
        {
            _protocolData = new byte[10];
            _protocolData[0] = STX;

           // _protocolData[1] = config.SignedNumber;
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
            Array.Copy(weight, 0, _protocolData, 1, weight.Length);

            _protocolData[7] = 0x0D;
            _protocolData[8] = 0x0A;
            _protocolData[9] = ETX;
        }

        private void AnsweringOutput(XK3190_CS6Config config)
        {
            //应答报文

            //实现重量应答

            //实现清零

            //实现稳定
            //实现 串口2连续输出切换

        }

        private void ExtendOutput(XK3190_CS6Config config)
        {
            //串口2 连续输出报文格式 47 3D 20 20 20 20 2D 38 30 20 0D 0A 
            _protocolData = new byte[12];
            _protocolData[0] = 0x47;
            _protocolData[1] = 0x3D;
            weightData = new byte[7];

            //重量值 
            string strWeight = _grossWeight.ToString().PadLeft(7, ' ');


            //数据超长
            if (strWeight.Length > 7)
            {
                strWeight = strWeight.Substring(0, 7);
            }

            byte[] weight = asciiEncoding.GetBytes(strWeight);
            Array.Copy(weight, 0, _protocolData, 2, weight.Length);

            _protocolData[9] = 0x20;
            _protocolData[10] = 0x0D;
            _protocolData[11] = 0x0A;
        }

        private void StandardOutput(XK3190_CS6Config config)
        {
            //串口1 连续输出报文格式 02 20 20 20 2D 36 30 0D 0A 03 
            _protocolData = new byte[10];
            _protocolData[0] = STX;

           // _protocolData[1] = config.SignedNumber;
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
            Array.Copy(weight, 0, _protocolData, 1, weight.Length);

            _protocolData[7] = 0x0D;
            _protocolData[8] = 0x0A;
            _protocolData[9] = ETX;
        }
    }


    //串口1 连续格式：02 20 20 20 2D 36 30 0D 0A 03 
    //串口2 连续格式：47 3D 20 20 20 20 2D 38 30 20 0D 0A 
    //应答报文： 毛重：02 41 43 2D 30 30 30 30 30 37 39 20 33 31 03 
    public class XK3190_CS6Config
    {
        private static readonly XK3190_CS6Config _defaultConfig;
        public StateWord Standard { get; set; }
        public StateWord Extend { get; set; }
        public SerialSetting SerialPortSetting { get; set; }
        public DataOutputConfig OutputModeSetting { get; set; }
        /// <summary>
        /// 正符号标识
        /// </summary>
        public const byte PLUS_SIGN = 0x2B;

        /// <summary>
        /// 负符号标识
        /// </summary>
        public const byte MINUS_SIGN = 0x2D;
        public string ModeName { get; set; }

        /// <summary>
        /// 设置+/-符号
        /// </summary>
        public byte SignedNumber { get; set; }
        public DateTime? UpdateTime { get; set; }

        public static XK3190_CS6Config DefaultConfig { get => _defaultConfig; }

        /// <summary>
        /// 小数位
        /// </summary>
        public byte DecimalPlaces { get; set; }

        //静态构造
        static XK3190_CS6Config()
        {
            //默认配置
            if (null == _defaultConfig)
            {
                _defaultConfig = new XK3190_CS6Config
                {

                    SerialPortSetting = new SerialSetting(),
                    OutputModeSetting = new DataOutputConfig
                    {
                        InOutput = InputOutputMode.StandardOutput,
                        ModeScene = WeightModeScene.UpToPound,
                        PeakValue = 48500,
                        StartValue = 0,
                        StepLength = 20
                    },
                    ModeName = "XK3190-CS6"
                };

             
            }
        }

        public XK3190_CS6Config()
        {
            //Standard = new StateWord();
            //Extend = new StateWord(true);
            //SerialPortSetting = new SerialSetting();
            //OutputModeSetting = new DataOutputConfig();
            //ModeName = "";
            //UpdateTime = DateTime.Now;
        }
    }


}
