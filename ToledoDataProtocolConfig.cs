using System;
using System.IO.Ports;
using System.Xml.Serialization;

namespace xabg.GroundScaleSimulator
{
    /// <summary>
    /// 数据协议配置
    /// </summary>
    public class ToledoDataProtocolConfig
    {
        public StateWord Standard { get; set; }
        public StateWord Extend { get; set; }
        public SerialSetting SerialPortSetting { get; set; }

        public DataOutputConfig OutputModeSetting { get; set; }

        private static readonly ToledoDataProtocolConfig _defaultConfig;

        public string ModeName { get; set; }
        public DateTime? UpdateTime { get; set; }

        public static ToledoDataProtocolConfig DefaultConfig { get => _defaultConfig; }

        static ToledoDataProtocolConfig()
        {
            if (null == _defaultConfig)
            {
                _defaultConfig = new ToledoDataProtocolConfig
                {
                    UpdateTime = null,
                    SerialPortSetting = new SerialSetting(),
                    OutputModeSetting = new DataOutputConfig
                    {
                        InOutput = InputOutputMode.StandardOutput,
                        ModeScene = WeightModeScene.UpToPound,
                        PeakValue = 48500,
                        StartValue = 0,
                        StepLength = 20
                    },

                    // 标准
                    Standard = new StateWord
                    {
                        SWA = 0x31,
                        SWB = 0x30,
                        SWC = 0x20
                    },

                    //扩展
                    Extend = new StateWord(true)
                    {
                        SWA = 0x32,
                        SWB = 0x20,
                        SWC = 0x20,
                        SWD = 0x20
                    },

                    ModeName = "IND880"
                };

            }
        }
        public ToledoDataProtocolConfig()
        {
            Standard = new StateWord();
            Extend = new StateWord(true);
            SerialPortSetting = new SerialSetting();
            OutputModeSetting = new DataOutputConfig();
            ModeName = "";
            UpdateTime = DateTime.Now;
        }

    }

    /// <summary>
    /// 数据输出配置
    /// </summary>
    public class DataOutputConfig
    {
        /// <summary>
        /// 输入输出方式
        /// </summary>
        public InputOutputMode InOutput { get; set; }

        /// <summary>
        /// 步长，模拟上下磅时，数据变化步长值
        /// </summary>
        public int StepLength { get; set; }

        /// <summary>
        /// 峰值，模拟上磅场景时，设定最大载重。
        /// </summary>
        public int PeakValue { get; set; }

        /// <summary>
        /// 数据刷新频率
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 模拟场景
        /// </summary>
        public WeightModeScene ModeScene { get; set; }

        /// <summary>
        /// 起始值
        /// </summary>
        public int StartValue { get; set; }

        public DataOutputConfig()
        {
            InOutput = InputOutputMode.StandardOutput;
            StepLength = 10;
            PeakValue = 0;
            ModeScene = WeightModeScene.UpToPound;

        }
    }


    /// <summary>
    /// 状态字1 或A   SWA SB1
    /// </summary>
    public class StateWord
    {
        private byte _swa;
        private byte _swb;
        private byte _swc;
        private byte _swd;

        /// <summary>
        /// 获取或设置状态字1
        /// </summary>
        public byte SWA { get => _swa; set => _swa = value; }
        public byte SWB { get => _swb; set => _swb = value; }
        public byte SWC { get => _swc; set => _swc = value; }
        public byte SWD { get => _swd; set => _swd = value; }

        public StateWord() : this(false)
        {
        }

        public StateWord(bool isExtend)
        {
            if (isExtend)
            {
                _swa = 0x72;
                _swb = 0x20;
            }
            else
            {
                _swa = 0x30;
                _swb = 0x30;
            }
            _swd = 0x20;
            _swc = 0x20;
        }
    }

    /// <summary>
    /// 小数点位置，状态字A 
    /// </summary>
    public enum PointPosition : byte
    {
        XXXX00 = 0,
        XXXXX0 = 1,
        XXXXXX = 2,
        XXXXX_X = 3,
        XXXX_XX = 4,
        XXX_XXX = 5,
        XX_XXXX = 6,
        X_XXXXX = 7
    }

    /// <summary>
    /// 分度值因子，状态字A
    /// </summary>
    public enum IndexingFactor : byte
    {
        X1 = 1,
        X2 = 2,
        X5 = 3
    }

    /// <summary>
    /// 计量单位 状态字C
    /// </summary>
    public enum WeightUnit : byte
    {
        /// <summary>
        /// 磅值或千克
        /// </summary>
        lbOrkg = 0,
        g = 1,
        t = 2,
        oz = 3,
        ozt = 4,
        dwt = 5,
        ton = 6,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom = 7
    }

    /// <summary>
    /// 扩展计量单位 状态字1
    /// </summary>
    public enum ExWeightUnit : byte
    {
        None = 0,
        lb = 1,
        kg = 2,
        g = 3,
        t = 4,
        ton = 5,
        ozt = 6,
        dwt = 7,
        oz = 8,
        Custom = 9
    }

    /// <summary>
    /// 扩展皮重类型 状态字2
    /// </summary>
    public enum ExTareType
    {
        None = 0,
        KeyTare = 1,
        PresetTare = 2,
        TareMemory = 3
    }

    /// <summary>
    /// 扩展重量范围  状态2
    /// </summary>
    public enum ExWeightRange
    {
        /// <summary>
        /// 单量程
        /// </summary>
        SingleRange = 0,
        /// <summary>
        /// 量程1
        /// </summary>
        RangeOne = 1,
        /// <summary>
        /// 量程2
        /// </summary>
        RangeTwo = 2,
        /// <summary>
        /// 量程3
        /// </summary>
        RangeThree = 3,
    }

    /// <summary>
    /// 输入输出方式
    /// </summary>
    public enum InputOutputMode
    {
        /// <summary>
        /// 标准连续输出
        /// </summary>
        StandardOutput,
        /// <summary>
        /// 扩展连续输出
        /// </summary>
        ExtendOutput,
        /// <summary>
        /// 连续输出8位ASCII
        /// </summary>
        ASCII_8,
        /// <summary>
        /// 连续输出9位ASCII
        /// </summary>
        ASCII_9,
        /// <summary>
        /// 标准格式输入
        /// </summary>
        StandardInput,
        /// <summary>
        /// 扩展格式输入
        /// </summary>
        ExtendInput,
        /// <summary>
        /// 重量数据
        /// </summary>
        WeightValue,
        /// <summary>
        /// SICS Level 0 格式
        /// </summary>
        SICSLevel0,
        /// <summary>
        /// 应答协议格式
        /// </summary>
        Answering

    }

    /// <summary>
    /// 模拟场景
    /// </summary>
    public enum WeightModeScene
    {
        /// <summary>
        /// 模拟上磅
        /// </summary>
        UpToPound,
        /// <summary>
        /// 模拟下磅
        /// </summary>
        GoDownPound,
        /// <summary>
        /// 模拟上下磅
        /// </summary>
        UpAndDownPound,
        /// <summary>
        /// 输入重量
        /// </summary>
        KeyInput
    }

    /// <summary>
    /// 磅衡通信参数
    /// </summary>
    [XmlRoot("serials", ElementName = "serial")]
    public class SerialSetting
    {
        /// <summary>
        /// 串口端口号
        /// </summary>
        [XmlAttribute("portname")]
        public string ComName { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        [XmlAttribute("baudrate")]
        public int BaudRate { get; set; }

        /// <summary>
        /// 数据位
        /// </summary>
        [XmlAttribute("databits")]
        public int DataBits { get; set; }
        /// <summary>
        /// 校验位
        /// </summary>
        [XmlAttribute("parity")]
        public Parity Parity { get; set; }

        [XmlAttribute("stopbits")]
        public StopBits StopBits { get; set; }

        public SerialSetting()
        {
            ComName = "";
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
        }
    }
}
