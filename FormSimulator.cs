﻿using MetroFramework.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace xabg.GroundScaleSimulator
{
    public partial class FormSimulator : MetroForm
    {
        private FormMessageBox fmb;
        private SerialPort _ports;
        //模拟器开机状态
        private bool _isStart;
        private string key_value;
        //模拟数据计数器
        private int timerIndex = 0;
        //随机数
        private Random ran;
        private Timer _twinkleTimer;
        private Timer _simulateTimer;
        private Timer _createDataTimer;
        //配置
        private ToledoDataProtocolConfig _config;
        //数据报文处理类
        private MettlerToledoDataProtocol _dataProtocol;

        //XK3190A9P
        private XK3190DataProtocolConfig _xkConfig;
        private XK3190A9P _xkDataProtocol;
        private D2000E_DPCfg _kld2000EDPCfg;
        private KLD2000E _klD2000EDP;
        //XK3190-CS6
        private XK3190_CS6Config _xkCS6Config;
        private XK3190_CS6 _xkCS6Protocol;

        public FormSimulator()
        {
            InitializeComponent();
            Init_SateWord();
            Init_SerialPortConfigItem();

            _twinkleTimer = new Timer
            {
                Enabled = true,
                Interval = 500
            };
            _twinkleTimer.Tick += TwinkleTimer_Tick;

            _simulateTimer = new Timer
            {
                Enabled = false,
                Interval = 100
            };
            _simulateTimer.Tick += SimulateTimer_Tick;

            _createDataTimer = new Timer();
            _createDataTimer.Tick += CreateDataTimer_Tick;

            this.FormBorderStyle = FormBorderStyle.None;

            _config = ToledoDataProtocolConfig.DefaultConfig;
            _xkCS6Config = XK3190_CS6Config.DefaultConfig;
        }

        private void SimulateTimer_Tick(object sender, EventArgs e)
        {
            //模拟连续输出
            if (_ports.IsOpen)
            {
                switch (CbxTruckScalesPro.Text)
                {
                    case "XK3190-A9+":
                        if (null != _xkDataProtocol)
                        {
                            _ports.Write(_xkDataProtocol.ProtocolData, 0, _xkDataProtocol.ProtocolData.Length);
                            if (null != fmb)
                                fmb.SetMessageText(_xkDataProtocol.ProtocolDataHex);
                        }
                        break;
                    case "INDT880":
                        if (null != _xkDataProtocol)
                        {
                            _ports.Write(_dataProtocol.ProtocolData, 0, _dataProtocol.ProtocolData.Length);
                            if (null != fmb)
                                fmb.SetMessageText(_dataProtocol.ProtocolDataHex);
                        }
                        break;
                    case "XK3190-CS6":
                        if (null != _xkCS6Protocol)
                        {
                            _ports.Write(_xkCS6Protocol.ProtocolData, 0, _xkCS6Protocol.ProtocolData.Length);
                            if (null != fmb)
                                fmb.SetMessageText(_xkCS6Protocol.ProtocolDataHex);
                        }
                        break;
                    case "KL-D2000E":
                        if (null != _klD2000EDP)
                        {
                            _ports.Write(_klD2000EDP.ProtocolData, 0, _klD2000EDP.BufferLength);
                            if (null != fmb)
                                fmb.SetMessageText(_klD2000EDP.ProtocolDataHex);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void TwinkleTimer_Tick(object sender, EventArgs e)
        {
            SetDataDenote(Color.Red, true);
        }

        private void FormSimulator_Load(object sender, EventArgs e)
        {

            Init_Com();
            Init_SerialPort();
            //数据报文处理类
            _dataProtocol = new MettlerToledoDataProtocol();

            _xkDataProtocol = new XK3190A9P();

            _xkCS6Protocol = new XK3190_CS6();

            _klD2000EDP = new KLD2000E();
            PowerOff();
        }


        private void Init_SerialPortConfigItem()
        {
            CbxCom.SelectedIndex = 0;
            CbxBaudrate.SelectedIndex = 3;
            CbxParity.SelectedIndex = 0;
            CbxDatabits.SelectedIndex = 3;
            CbxStopBit.SelectedIndex = 0;
        }

        private void Init_SerialPort()
        {
            if (null == _ports)
            {
                _ports = new SerialPort();
            }
            if (string.IsNullOrEmpty(CbxCom.Text))
            {
                CbxCom.Text = "COM1";
            }
            _ports.PortName = CbxCom.Text;
            int.TryParse(CbxBaudrate.Text, out int baudRate);
            _ports.BaudRate = baudRate;
            int.TryParse(CbxDatabits.Text, out int dataBits);
            _ports.DataBits = dataBits;
            _ports.Parity = (Parity)Enum.Parse(typeof(Parity), CbxParity.Text);
            _ports.StopBits = (StopBits)Enum.Parse(typeof(StopBits), CbxStopBit.Text);
        }

        //======================USB 插拔检测=========================== 
        protected override void WndProc(ref Message m)
        {
            //Console.WriteLine(m.WParam.ToInt32());    //打印程序检测到的变化信息
            try
            {
                //检测到USB口发生了变化,这里USB口变化时wParam的值是7，表示系统中添加或者删除了设备
                if (m.WParam.ToInt32() == 7)
                {
                    Init_Com();       //检测到USB口有变化时重新连接一次自己要检测的设备，连接不成功则可以判断设备已断开（个函数是USB连接函数）

                    if (m.WParam.ToInt32() == 0x8004)  　　//没找到设备处理事件，我这里 flag=0 表示设备没连接成功
                    {
                        MessageBox.Show(" 串口设备已移除！");
                    }
                    else
                    {
                        //这里可以添加设备没有断开的处理代码
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);　　//异常处理函数
            }
            base.WndProc(ref m);　　//这个是windos的异常处理，一定要添加，不然会运行不了
        }


        private void Init_Com()
        {
            //读取系统串口
            string[] ports = SerialPort.GetPortNames();
            CbxCom.Items.Clear();
            CbxCom.Text = "";
            CbxCom.Items.AddRange(ports);
            if (ports.Length >= 1)
            {
                CbxCom.Text = ports[0];
            }
            else
            {
                CbxCom.Items.Add("COM1");
            }

        }

        private void Init_RadioGroup()
        {
            //对单选框控制进行分组
            RbtGrossWeight.Checked = true;
            RbtPlus.Checked = true;
            RbtSteadyState.Checked = true;
            RbtKG.Checked = true;
            RbtEFDynamic.Checked = true;
            RbtEFGrossWeight.Checked = true;
            RbtExDisplay.Checked = true;
            RbtUnderOverload.Checked = true;
        }


        private void Init_SateWord()
        {
            Init_RadioGroup();

            //下拉框初始值
            CbxPointPosition.SelectedIndex = 1;
            CbxIndexingFactor.SelectedIndex = 1;
            CbxUnit.SelectedIndex = 0;
        }

        private void Init_Control()
        {
            LledWeightValue.ForeColor = Color.DimGray;
        }

        bool isShow = false;
        private void BtnOpenRightPanel_Click(object sender, EventArgs e)
        {
            fmb = FormMessageBox.GetInstance();
            if (!isShow)
            {
                //打开右侧面板
                Point location = this.Location;
                location.X = location.X + this.Width + 2;
                fmb.Location = location;
                fmb.Height = this.Height;

                fmb.Show();
                isShow = true;

            }
            else
            {
                fmb.Close();
                isShow = false;
            }
        }

        private void FormSimulator_LocationChanged(object sender, EventArgs e)
        {
            //位置改变事件
            if (null != fmb)
            {
                Point location = this.Location;
                location.X = location.X + this.Width + 2;
                fmb.Location = location;
            }
        }


        #region 单选按钮点击事件
        private void RbtGrossWeight_Click(object sender, EventArgs e)
        {
            RbtGrossWeight.Checked = true;
            RbtSuttle.Checked = !RbtGrossWeight.Checked;
        }

        private void RbtSuttle_Click(object sender, EventArgs e)
        {
            RbtSuttle.Checked = true;
            RbtGrossWeight.Checked = !RbtSuttle.Checked;
        }

        private void RbtPlus_Click(object sender, EventArgs e)
        {
            RbtPlus.Checked = true;
            RbtMinus.Checked = !RbtPlus.Checked;
        }

        private void RbtMinus_Click(object sender, EventArgs e)
        {
            RbtMinus.Checked = true;
            RbtPlus.Checked = !RbtMinus.Checked;
        }

        private void RbtDynamic_Click(object sender, EventArgs e)
        {
            RbtDynamic.Checked = true;
            RbtSteadyState.Checked = !RbtDynamic.Checked;
        }

        private void RbtSteadyState_Click(object sender, EventArgs e)
        {
            RbtSteadyState.Checked = true;
            RbtDynamic.Checked = !RbtSteadyState.Checked;
        }

        private void RbtLB_Click(object sender, EventArgs e)
        {
            RbtLB.Checked = true;
            RbtKG.Checked = !RbtLB.Checked;
        }

        private void RbtKG_Click(object sender, EventArgs e)
        {
            RbtKG.Checked = true;
            RbtLB.Checked = !RbtKG.Checked;
        }

        private void RbtEFDynamic_Click(object sender, EventArgs e)
        {
            RbtEFDynamic.Checked = true;
            RbtEFSteadyState.Checked = !RbtEFDynamic.Checked;
        }

        private void RbtEFSteadyState_Click(object sender, EventArgs e)
        {
            RbtEFSteadyState.Checked = true;
            RbtEFDynamic.Checked = !RbtEFSteadyState.Checked;
        }

        private void RbtEFGrossWeight_Click(object sender, EventArgs e)
        {
            RbtEFGrossWeight.Checked = true;
            RbtEFSuttle.Checked = !RbtEFGrossWeight.Checked;
        }

        private void RbtEFSuttle_Click(object sender, EventArgs e)
        {
            RbtEFSuttle.Checked = true;
            RbtEFGrossWeight.Checked = !RbtEFSuttle.Checked;
        }

        private void RbtExDisplay_Click(object sender, EventArgs e)
        {
            RbtExDisplay.Checked = true;
            RbtNormal.Checked = !RbtExDisplay.Checked;
        }

        private void RbtNormal_Click(object sender, EventArgs e)
        {
            RbtNormal.Checked = true;
            RbtExDisplay.Checked = !RbtNormal.Checked;
        }

        private void RbtUnderOverload_Click(object sender, EventArgs e)
        {
            RbtUnderOverload.Checked = true;
            RbxOverloading.Checked = !RbtUnderOverload.Checked;
        }

        private void RbxOverloading_Click(object sender, EventArgs e)
        {
            RbxOverloading.Checked = true;
            RbtUnderOverload.Checked = !RbxOverloading.Checked;
        }

        #endregion

        private void BtnOpenSerialPort_Click(object sender, EventArgs e)
        {

        }


        int currentValue = 0;
        int weightScaler = 20;


        private void CreateDataTimer_Tick(object sender, EventArgs e)
        {

            switch (CbxTruckScalesPro.Text)
            {
                case "XK3190-A9+":
                    XK3190A9P();
                    break;
                case "INDT880":
                    ToledoT880();
                    break;
                case "XK3190-CS6":
                    XK3190_CS6();
                    break;
                case "KL-D2000E":
                    KLD2000E();
                    break;
                default:
                    break;
            }

        }


        private void XK3190A9P()
        {
            if (null == _xkConfig) return;

            int startValue = _xkConfig.OutputModeSetting.StartValue;
            int stepValue = _xkConfig.OutputModeSetting.StepLength;
            int peakValue = _xkConfig.OutputModeSetting.PeakValue;

            //从起始值开始模拟
            if (timerIndex == 0)
            {
                currentValue = startValue;
            }

            //改变步长值
            if (CbxScene.Text == "模拟装车")
            {
                int.TryParse(TbxRangeValue.Text.Trim(), out int RangeValue);

                int.TryParse(TbxStepLength.Text.Trim(), out stepValue);
                //生成随机步长
                int RandomStep = ran.Next(1, RangeValue) * 20;
                stepValue += RandomStep;

                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                    if (timerIndex % 10 == 0)
                    {
                        timerIndex = 0;
                        return;
                    }
                }
            }
            //设置数据变化频率
            if (CbxScene.Text == "模拟上磅")
            {
                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                }
            }

            if (CbxScene.Text == "模拟下磅")
            {
                //递减
                if (startValue > 0 && currentValue > 0)
                {
                    currentValue -= stepValue;
                }
                else
                    currentValue = 0;
            }


            if (CbxScene.Text == "模拟上下磅")
            {  //递减
                if (weightScaler == 0)
                {
                    currentValue -= stepValue;
                    if (currentValue <= 0)
                    {
                        currentValue = 0;
                        weightScaler = 20;
                    }
                }
                else
                {
                    //递增
                    currentValue += stepValue;
                    if (currentValue >= peakValue)
                    {
                        currentValue = peakValue;
                        weightScaler--;
                    }
                }
            }
            if (CbxScene.Text == "输入重量")
            {
                decimal.TryParse(key_value, out decimal value);
                currentValue = (int)value;
            }


            //设置产生的新重量值
            _xkDataProtocol.GrossWeight = currentValue;
            _xkDataProtocol.BiludProtocol(_xkConfig);

            int WeightValue = currentValue;

            if (_xkConfig.SignedNumber == XK3190DataProtocolConfig.MINUS_SIGN)
            {
                WeightValue = 0 - WeightValue;
            }

            LledWeightValue.Text = WeightValue.ToString();

            //计数器加1
            timerIndex++;


        }

        private void ToledoT880()
        {
            if (null == _config) return;

            int startValue = _config.OutputModeSetting.StartValue;
            int stepValue = _config.OutputModeSetting.StepLength;
            int peakValue = _config.OutputModeSetting.PeakValue;

            //从起始值开始模拟
            if (timerIndex == 0)
            {
                currentValue = startValue;
            }


            //改变步长值
            if (CbxScene.Text == "模拟装车")
            {
                int.TryParse(TbxRangeValue.Text.Trim(), out int RangeValue);

                int.TryParse(TbxStepLength.Text.Trim(), out stepValue);
                //生成随机步长
                int RandomStep = ran.Next(1, RangeValue) * 20;
                stepValue += RandomStep;

                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                    if (timerIndex % 100 == 0)
                    {
                        timerIndex = 0;
                        return;
                    }
                }
            }

            //设置数据变化频率
            if (CbxScene.Text == "模拟上磅")
            {
                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                }
            }

            if (CbxScene.Text == "模拟下磅")
            {
                //递减
                if (startValue > 0 && currentValue > 0)
                {
                    currentValue -= stepValue;
                }
                else
                    currentValue = 0;
            }


            if (CbxScene.Text == "模拟上下磅")
            {  //递减
                if (weightScaler == 0)
                {
                    currentValue -= stepValue;
                    if (currentValue <= 0)
                    {
                        currentValue = 0;
                        weightScaler = 20;
                    }
                }
                else
                {
                    //递增
                    currentValue += stepValue;
                    if (currentValue >= peakValue)
                    {
                        currentValue = peakValue;
                        weightScaler--;
                    }
                }
            }

            if (CbxScene.Text == "输入重量")
            {
                decimal.TryParse(key_value, out decimal value);
                currentValue = (int)value;
            }


            //设置产生的新重量值
            _dataProtocol.GrossWeight = currentValue;
            _dataProtocol.BiludProtocol(_config);

            int WeightValue = currentValue;

            byte SWB = _config.Standard.SWB;
            if ((SWB & 0x02) == 0x02)
            {
                WeightValue *= -1;
            }

            LledWeightValue.Text = WeightValue.ToString();

            //计数器加1
            timerIndex++;
        }

        private void XK3190_CS6()
        {
            if (null == _xkCS6Config) return;

            int startValue = _xkCS6Config.OutputModeSetting.StartValue;
            int stepValue = _xkCS6Config.OutputModeSetting.StepLength;
            int peakValue = _xkCS6Config.OutputModeSetting.PeakValue;
            //if (RbtMinus.Checked)
            //{
            //    _xkCS6Config.SignedNumber = XK3190_CS6Config.MINUS_SIGN;
            //}
            //if (RbtPlus.Checked)
            //{
            //    _xkCS6Config.SignedNumber = XK3190_CS6Config.MINUS_SIGN;
            //}
            //从起始值开始模拟
            if (timerIndex == 0)
            {
                currentValue = startValue;
            }

            //改变步长值
            if (CbxScene.Text == "模拟装车")
            {
                int.TryParse(TbxRangeValue.Text.Trim(), out int RangeValue);

                int.TryParse(TbxStepLength.Text.Trim(), out stepValue);
                //生成随机步长
                int RandomStep = ran.Next(1, RangeValue) * 20;
                stepValue += RandomStep;

                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                    if (timerIndex % 10 == 0)
                    {
                        timerIndex = 0;
                        return;
                    }
                }
            }
            //设置数据变化频率
            if (CbxScene.Text == "模拟上磅")
            {
                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                }
            }

            if (CbxScene.Text == "模拟下磅")
            {
                //递减
                if (startValue > 0 && currentValue > 0)
                {
                    currentValue -= stepValue;
                }
                else
                    currentValue = 0;
            }

            if (CbxScene.Text == "模拟上下磅")
            {  //递减
                if (weightScaler == 0)
                {
                    currentValue -= stepValue;
                    if (currentValue <= 0)
                    {
                        currentValue = 0;
                        weightScaler = 20;
                    }
                }
                else
                {
                    //递增
                    currentValue += stepValue;
                    if (currentValue >= peakValue)
                    {
                        currentValue = peakValue;
                        weightScaler--;
                    }
                }
            }
            if (CbxScene.Text == "输入重量")
            {
                decimal.TryParse(key_value, out decimal value);
                currentValue = (int)value;
            }

            int WeightValue = currentValue;

            if (_xkCS6Config.SignedNumber == 0x2D && WeightValue > 0)
            {
                WeightValue = 0 - WeightValue;
            }
            //设置产生的新重量值
            _xkCS6Protocol.GrossWeight = WeightValue;
            _xkCS6Protocol.BiludProtocol(_xkCS6Config);

            LledWeightValue.Text = WeightValue.ToString();

            //计数器加1
            timerIndex++;
        }

        private void KLD2000E()
        {
            if (null == _kld2000EDPCfg) return;

            int startValue = _kld2000EDPCfg.OutputModeSetting.StartValue;
            int stepValue = _kld2000EDPCfg.OutputModeSetting.StepLength;
            int peakValue = _kld2000EDPCfg.OutputModeSetting.PeakValue;

            //从起始值开始模拟
            if (timerIndex == 0)
            {
                currentValue = startValue;
            }

            //改变步长值
            if (CbxScene.Text == "模拟装车")
            {
                int.TryParse(TbxRangeValue.Text.Trim(), out int RangeValue);

                int.TryParse(TbxStepLength.Text.Trim(), out stepValue);
                //生成随机步长
                int RandomStep = ran.Next(1, RangeValue) * 20;
                stepValue += RandomStep;

                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                    if (timerIndex % 10 == 0)
                    {
                        timerIndex = 0;
                        return;
                    }
                }
            }
            //设置数据变化频率
            if (CbxScene.Text == "模拟上磅")
            {
                //递增
                currentValue += stepValue;

                if (currentValue >= peakValue)
                {
                    currentValue = peakValue;
                }
            }

            if (CbxScene.Text == "模拟下磅")
            {
                //递减
                if (startValue > 0 && currentValue > 0)
                {
                    currentValue -= stepValue;
                }
                else
                    currentValue = 0;
            }

            if (CbxScene.Text == "模拟上下磅")
            {  //递减
                if (weightScaler == 0)
                {
                    currentValue -= stepValue;
                    if (currentValue <= 0)
                    {
                        currentValue = 0;
                        weightScaler = 20;
                    }
                }
                else
                {
                    //递增
                    currentValue += stepValue;
                    if (currentValue >= peakValue)
                    {
                        currentValue = peakValue;
                        weightScaler--;
                    }
                }
            }
            if (CbxScene.Text == "输入重量")
            {
                decimal.TryParse(key_value, out decimal value);
                currentValue = (int)value;
            }

            int WeightValue = currentValue;

            if (_kld2000EDPCfg.SignedNumber == 0x2D && WeightValue > 0)
            {
                WeightValue = 0 - WeightValue;
            }
            //设置产生的新重量值
            _klD2000EDP.GrossWeight = WeightValue;
            _klD2000EDP.BiludProtocol(_kld2000EDPCfg);

            LledWeightValue.Text = WeightValue.ToString();

            //计数器加1
            timerIndex++;
        }

        private void BtnCloseSerialPort_Click(object sender, EventArgs e)
        {

        }

        private void OpenOrClosePort(bool openState)
        {
            try
            {
                if (!_ports.IsOpen && openState)
                {
                    //打开串口
                    _ports.Open();

                }
                else
                {
                    //关闭串口
                    _ports.Close();
                    _simulateTimer.Enabled = false;
                }

            }
            catch (Exception)
            {
                //TODO: 输出异常信息
                MessageBox.Show("串口打开失败。请检查串口是否有效。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void BtnSaveSetting_Click(object sender, EventArgs e)
        {
            //保存配置

        }

        private void BtnResetDefault_Click(object sender, EventArgs e)
        {
            //恢复默认选项

            LoadConfig(ToledoDataProtocolConfig.DefaultConfig);
        }

        #region 仪表操作台
        private void BtnBootup_Click(object sender, EventArgs e)
        {
            //开机按钮

            if (_isStart)
            {
                PowerOff();
                SetDataDenote(Color.GreenYellow, true);
            }
            else
            {
                PowerOn();
                //计数器归零
                timerIndex = 0;
            }
        }

        //开机方法
        private void PowerOn()
        {
            BtnBootup.Text = "关机";
            BtnBootup.BackColor = Color.Gainsboro;
            _isStart = true;
            //LED
            LledWeightValue.ForeColor = Color.Gold;
            //数据指示灯
            UlnDataDenote.LanternBackground = Color.Gray;

            //初始化串口输出
            Init_SerialPort();
            //打开串口
            OpenOrClosePort(true);


            //初始化数据发生计时器

            if (!_ports.IsOpen) return;

            int.TryParse(tbxFrequency.Text.Trim(), out int ival);
            if (ival == 0) ival = 1000;
            _createDataTimer.Interval = ival;


            //读取配置，向构建类发送配置
            if (!_ports.IsOpen)
            {
                MessageBox.Show("请先打开串口，在进行模拟。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            switch (CbxTruckScalesPro.Text)
            {
                case "XK3190-A9+":
                    XK3190A9Plus();
                    break;
                case "INDT880":
                    DataSimulation();
                    break;
                case "XK3190-CS6":
                    XK3190CS6Simulation();
                    break;
                case "KL-D2000E":
                    KLD2000ESimulation();
                    break;
                default:
                    break;
            }
        }

        //关机方法
        private void PowerOff()
        {
            BtnBootup.Text = "开机";
            BtnBootup.BackColor = SystemColors.Control;
            _isStart = false;

            //LED
            LledWeightValue.ForeColor = Color.Silver;
            //数据指示灯
            UlnDataDenote.LanternBackground = Color.DimGray;

            //关闭串口
            OpenOrClosePort(false);
            _createDataTimer.Enabled = false;

            //清除LED显示的数据
            LledWeightValue.Text = "";
        }

        private void SetDataDenote(Color lbColor, bool isTwinkle)
        {
            if (!_isStart) return;
            if (isTwinkle)
            {
                if (lbColor == UlnDataDenote.LanternBackground)
                {
                    UlnDataDenote.LanternBackground = Color.Silver;
                }
                else
                {
                    UlnDataDenote.LanternBackground = lbColor;
                }
            }
            else
            {
                UlnDataDenote.LanternBackground = lbColor;
            }

        }

        private void BtnRadixPoint_Click(object sender, EventArgs e)
        {
            if (_ports.IsOpen == false)
                KeyInputNumber(".");
        }

        private void BtnNum0_Click(object sender, EventArgs e)
        {
            KeyInputNumber("0");
        }

        private void BtnSpace_Click(object sender, EventArgs e)
        {
            KeyInputNumber("-");
            //设置状态字
            RbtMinus.Checked = true;
            RbtPlus.Checked = false;
        }

        private void BtnNum1_Click(object sender, EventArgs e)
        {
            KeyInputNumber("1");
        }


        //数据
        private void KeyInputNumber(string number)
        {
            if (!_isStart) return;

            key_value += number;
            if (key_value.Length > 1)
                key_value = key_value.TrimStart('0');
            decimal.TryParse(key_value, out decimal value);

            if (key_value.Length > LledWeightValue.TotalCharCount)
            {
                value = 0;
                key_value = "0";
            }


            LledWeightValue.Text = value.ToString();
        }

        private void BtnNum2_Click(object sender, EventArgs e)
        {
            KeyInputNumber("2");
        }

        private void BtnNum3_Click(object sender, EventArgs e)
        {
            KeyInputNumber("3");
        }

        private void BtnNum4_Click(object sender, EventArgs e)
        {
            KeyInputNumber("4");
        }

        private void BtnNum5_Click(object sender, EventArgs e)
        {
            KeyInputNumber("5");
        }

        private void BtnNum6_Click(object sender, EventArgs e)
        {
            KeyInputNumber("6");
        }

        private void BtnNum7_Click(object sender, EventArgs e)
        {
            KeyInputNumber("7");
        }

        private void BtnNum8_Click(object sender, EventArgs e)
        {
            KeyInputNumber("8");
        }

        private void BtnNum9_Click(object sender, EventArgs e)
        {
            KeyInputNumber("9");
        }

        private void BtnRemoval_Click(object sender, EventArgs e)
        {
            //TODO:除皮操作

        }

        private void BtnZeroClearing_Click(object sender, EventArgs e)
        {
            LledWeightValue.Text = "0";
            key_value = "0";
            //当前值归零
            currentValue = 0;
            //计数器归零
            timerIndex = 0;
            RbtMinus.Checked = false;
            RbtPlus.Checked = true;

            _dataProtocol.GrossWeight = 0;
            _dataProtocol.BiludProtocol(_config);
        }

        private void BtnSimulate_Click(object sender, EventArgs e)
        {
            if (_ports.IsOpen == false)
            {
                OutputMessage("串口未打开", Color.Red);
                return;
            }

            if (null == BtnSimulate.Tag || ((bool)BtnSimulate.Tag) == false)
            {
                _createDataTimer.Enabled = false;
                BtnSimulate.Tag = true;
                BtnSimulate.Text = "动态";

                //设置指示灯
                _twinkleTimer.Enabled = false;
                SetDataDenote(Color.Lime, false);
            }
            else
            {

                _createDataTimer.Enabled = true;
                BtnSimulate.Tag = false;
                BtnSimulate.Text = "稳定";

                //设置指示灯
                _twinkleTimer.Enabled = true;
                SetDataDenote(Color.Red, true);
            }
        }


        //柯力  D2000E 型仪表 连续输出TF=0/ TF=2 格式
        private void KLD2000ESimulation()
        {
            D2000E_DPCfg d2000ecfg = new D2000E_DPCfg();
            //正负号
            if (RbtMinus.Checked)
            {
                d2000ecfg.SignedNumber = XK3190DataProtocolConfig.MINUS_SIGN;
            }
            else
            {
                d2000ecfg.SignedNumber = XK3190DataProtocolConfig.PLUS_SIGN;
            }

            //小数位
            string IndFac = CbxIndexingFactor.Text;
            switch (IndFac)
            {
                case "X0":
                    d2000ecfg.DecimalPlaces = 0;
                    break;
                case "X1":
                    d2000ecfg.DecimalPlaces = 1;
                    break;
                case "X2":
                    d2000ecfg.DecimalPlaces = 2;
                    break;
                default:
                    d2000ecfg.DecimalPlaces = 0;
                    break;
            }

            if (CbxOutputMode.Text == "连续输出TF=0")
            {
                d2000ecfg.DataLenght = 12;
                d2000ecfg.OutputModeSetting.InOutput = InputOutputMode.StandardOutput;
            }


            if (CbxOutputMode.Text == "连续输出TF=2")
            {
                d2000ecfg.DataLenght = 8;
                d2000ecfg.OutputModeSetting.InOutput = InputOutputMode.ASCII_8;
            }

            if (CbxOutputMode.Text == "连续输出TF=3")
            {
                d2000ecfg.DataLenght = 9;
                d2000ecfg.OutputModeSetting.InOutput = InputOutputMode.ASCII_9;
            }

            //准备数据
            int.TryParse(TbxStartValue.Text.Trim(), out int startValue);

            int.TryParse(TbxStepLength.Text.Trim(), out int stepValue);

            if (CbxScene.Text == "模拟装车")
            {
                ran = new Random(stepValue);
            }
            int.TryParse(TbxPeakValue.Text.Trim(), out int peakValue);

            d2000ecfg.OutputModeSetting.StartValue = startValue;
            d2000ecfg.OutputModeSetting.StepLength = stepValue;
            d2000ecfg.OutputModeSetting.PeakValue = peakValue;

            _klD2000EDP.GrossWeight = startValue;
            _kld2000EDPCfg = d2000ecfg;
            _klD2000EDP.BiludProtocol(d2000ecfg);

            LledWeightValue.Text = _xkDataProtocol.GrossWeight.ToString();

            _simulateTimer.Enabled = true;
            _createDataTimer.Enabled = true;

        }



        //耀华协议
        private void XK3190A9Plus()
        {
            XK3190DataProtocolConfig xk3190Config = new XK3190DataProtocolConfig();
            if (CbxOutputMode.Text == "标准连续输出")
            {

                //正负号
                if (RbtMinus.Checked)
                {
                    xk3190Config.SignedNumber = XK3190DataProtocolConfig.MINUS_SIGN;
                }
                else
                {
                    xk3190Config.SignedNumber = XK3190DataProtocolConfig.PLUS_SIGN;
                }

                //小数位
                string IndFac = CbxIndexingFactor.Text;
                switch (IndFac)
                {
                    case "X0":
                        xk3190Config.DecimalPlaces = 0;
                        break;
                    case "X1":
                        xk3190Config.DecimalPlaces = 1;
                        break;
                    case "X2":
                        xk3190Config.DecimalPlaces = 2;
                        break;
                    default:
                        xk3190Config.DecimalPlaces = 0;
                        break;
                }


                xk3190Config.OutputModeSetting.InOutput = InputOutputMode.StandardOutput;

            }

            //准备数据
            int.TryParse(TbxStartValue.Text.Trim(), out int startValue);

            int.TryParse(TbxStepLength.Text.Trim(), out int stepValue);

            if (CbxScene.Text == "模拟装车")
            {
                ran = new Random(stepValue);
            }
            int.TryParse(TbxPeakValue.Text.Trim(), out int peakValue);

            xk3190Config.OutputModeSetting.StartValue = startValue;
            xk3190Config.OutputModeSetting.StepLength = stepValue;
            xk3190Config.OutputModeSetting.PeakValue = peakValue;

            _xkDataProtocol.GrossWeight = startValue;

            _xkConfig = xk3190Config;


            _xkDataProtocol.BiludProtocol(xk3190Config);

            LledWeightValue.Text = _xkDataProtocol.GrossWeight.ToString();

            _simulateTimer.Enabled = true;
            _createDataTimer.Enabled = true;

        }


        private void XK3190CS6Simulation()
        {
            _xkCS6Config = XK3190_CS6Config.DefaultConfig;
            if (CbxOutputMode.Text == "标准连续输出")
            {
                _xkCS6Config.OutputModeSetting.InOutput = InputOutputMode.StandardOutput;


            }

            if (CbxOutputMode.Text == "扩展连续输出")
            {
                _xkCS6Config.OutputModeSetting.InOutput = InputOutputMode.ExtendOutput;

            }

            //正负号
            if (RbtMinus.Checked)
            {
                _xkCS6Config.SignedNumber = XK3190DataProtocolConfig.MINUS_SIGN;
                RbtPlus.Checked = false;
            }
            if (RbtPlus.Checked)
            {
                _xkCS6Config.SignedNumber = XK3190DataProtocolConfig.PLUS_SIGN;
                RbtMinus.Checked = false;
            }

            //小数位
            string IndFac = CbxIndexingFactor.Text;
            switch (IndFac)
            {
                case "X0":
                    _xkCS6Config.DecimalPlaces = 0;
                    break;
                case "X1":
                    _xkCS6Config.DecimalPlaces = 1;
                    break;
                case "X2":
                    _xkCS6Config.DecimalPlaces = 2;
                    break;
                default:
                    _xkCS6Config.DecimalPlaces = 0;
                    break;
            }


            //准备数据
            int.TryParse(TbxStartValue.Text.Trim(), out int startValue);

            int.TryParse(TbxStepLength.Text.Trim(), out int stepValue);

            if (CbxScene.Text == "模拟装车")
            {
                ran = new Random(stepValue);
            }

            int.TryParse(TbxPeakValue.Text.Trim(), out int peakValue);

            _xkCS6Config.OutputModeSetting.StartValue = startValue;
            _xkCS6Config.OutputModeSetting.StepLength = stepValue;
            _xkCS6Config.OutputModeSetting.PeakValue = peakValue;

            _xkCS6Protocol.GrossWeight = startValue;
            _xkCS6Protocol.BiludProtocol(_xkCS6Config);
            LledWeightValue.Text = _xkDataProtocol.GrossWeight.ToString();

            _simulateTimer.Enabled = true;
            _createDataTimer.Enabled = true;

        }


        private void DataSimulation()
        {

            //读取配置，向构建类发送配置
            if (!_ports.IsOpen)
            {
                MessageBox.Show("请先打开串口，在进行模拟。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            ToledoDataProtocolConfig tdpConfig = new ToledoDataProtocolConfig();

            if (CbxOutputMode.Text == "标准连续输出")
            {
                tdpConfig.OutputModeSetting.InOutput = InputOutputMode.StandardOutput;

                //状态字A  小数位
                //第5bit为1
                byte SWA = 0x20;
                string PP = CbxPointPosition.Text;
                switch (PP)
                {
                    case "XXXX00":
                        SWA = (byte)(SWA | 0x00);
                        break;
                    case "XXXXX0":
                        SWA = (byte)(SWA | 0x01);
                        break;
                    case "XXXXXX":
                        SWA = (byte)(SWA | 0x02);
                        break;
                    case "XXXXX.X":
                        SWA = (byte)(SWA | 0x03);
                        break;
                    case "XXXX.XX":
                        SWA = (byte)(SWA | 0x04);
                        break;
                    case "XXX.XXX":
                        SWA = (byte)(SWA | 0x05);
                        break;
                    case "XX.XXXX":
                        SWA = (byte)(SWA | 0x06);
                        break;
                    case "X.XXXXX":
                        SWA = (byte)(SWA | 0x07);
                        break;
                    default:
                        SWA = (byte)(SWA | 0x00);
                        break;
                }

                //状态字A  分数因子
                string IndFac = CbxIndexingFactor.Text;
                switch (IndFac)
                {
                    case "X1":
                        SWA = (byte)(SWA | 0x08);
                        break;
                    case "X2":
                        SWA = (byte)(SWA | 0x10);
                        break;
                    case "X5":
                        SWA = (byte)(SWA | 0x18);
                        break;
                }

                tdpConfig.Standard.SWA = SWA;

                //状态字B  小数位
                byte SWB = 0x20;
                //if (RbtGrossWeight.Checked)
                //{
                //    SWB = (byte)(~SWB);
                //    SWB = (byte)(SWB | 0x01);
                //    SWB = (byte)(~SWB);
                //}
                if (RbtSuttle.Checked)
                {
                    SWB = (byte)(SWB | 0x01);

                }

                //正负
                //if (RbtPlus.Checked)
                //{
                //    SWB = (byte)(~SWB);
                //    SWB = (byte)(SWB | 0x02);
                //    SWB = (byte)(~SWB);
                //}
                if (RbtMinus.Checked)
                {
                    SWB = (byte)(SWB | 0x02);
                }

                if (CbxOverload.Checked)
                {
                    SWB = (byte)(SWB | 0x04);
                }


                //动态/稳态
                if (RbtDynamic.Checked)
                {
                    SWB = (byte)(SWB | 0x08);
                }
                //if (RbtSteadyState.Checked)
                //{
                //    SWB = (byte)(~SWB);
                //    SWB = (byte)(SWB | 0x08);
                //    SWB = (byte)(~SWB);
                //}

                //lb或KG
                if (RbtKG.Checked)
                {
                    SWB = (byte)(SWB | 0x10);
                }
                //if (RbtLB.Checked)
                //{
                //    SWB = (byte)(~SWB);
                //    SWB = (byte)(SWB | 0x10);
                //    SWB = (byte)(~SWB);
                //}

                tdpConfig.Standard.SWB = SWB;

                //状态字C
                byte SWC = 0x20;
                string Unit = CbxUnit.Text;
                switch (Unit)
                {
                    case "lb或kg":
                        SWC = (byte)(SWC | 0x00);
                        break;
                    case "g":
                        SWC = (byte)(SWC | 0x01);
                        break;
                    case "t":
                        SWC = (byte)(SWC | 0x02);
                        break;
                    case "oz":
                        SWC = (byte)(SWC | 0x03);
                        break;
                    case "ozt":
                        SWC = (byte)(SWC | 0x04);
                        break;
                    case "dwt":
                        SWC = (byte)(SWC | 0x05);
                        break;
                    case "ton":
                        SWC = (byte)(SWC | 0x06);
                        break;
                    case "自定义单位":
                        SWC = (byte)(SWC | 0x07);
                        break;
                }
                //打印
                if (CbxPrint.Checked)
                {
                    SWC = (byte)(SWC | 0x08);
                }
                if (Cbx10EX.Checked)
                {
                    SWC = (byte)(SWC | 0x10);
                }

                tdpConfig.Standard.SWC = SWC;

            }

            if (CbxOutputMode.Text == "扩展连续输出")
            {
                tdpConfig.OutputModeSetting.InOutput = InputOutputMode.ExtendOutput;

            }
            if (CbxOutputMode.Text == "连续输出1")
            {
                tdpConfig.OutputModeSetting.InOutput = InputOutputMode.WeightValue;
            }

            if (CbxOutputMode.Text == "SICS Level0")
            {
                tdpConfig.OutputModeSetting.InOutput = InputOutputMode.SICSLevel0;
            }


            int.TryParse(TbxStartValue.Text.Trim(), out int startValue);

            int.TryParse(TbxStepLength.Text.Trim(), out int stepValue);

            if (CbxScene.Text == "模拟装车")
            {
                ran = new Random(stepValue);
            }

            int.TryParse(TbxPeakValue.Text.Trim(), out int peakValue);


            tdpConfig.OutputModeSetting.StartValue = startValue;
            tdpConfig.OutputModeSetting.StepLength = stepValue;
            tdpConfig.OutputModeSetting.PeakValue = peakValue;

            _dataProtocol.GrossWeight = startValue;
            _dataProtocol.Tare = 0;

            //设置配置项
            _config = tdpConfig;

            _dataProtocol.BiludProtocol(tdpConfig);

            LledWeightValue.Text = _dataProtocol.GrossWeight.ToString();

            _simulateTimer.Enabled = true;
            _createDataTimer.Enabled = true;
        }

        //加载配置
        private void LoadConfig(ToledoDataProtocolConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
        }

        //读取配置
        private void ReadConfig()
        {
            _config = new ToledoDataProtocolConfig();

        }

        #endregion

        private void CbxBaudrate_TextChanged(object sender, EventArgs e)
        {

        }

        private void CbxBaudrate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && (e.KeyChar != 46) && e.KeyChar != 8)
            {
                MessageBox.Show("请输入数字");
                e.Handled = true;
            }
        }


        private void OutputMessage(string message, Color foreColor)
        {
            if (null != fmb)
            {
                string timeMessage = string.Format("{0:yyyy-MM-dd HH:mm:dd:sss}:\r\n{1}", DateTime.Now, message);
                fmb.SetMessageText(timeMessage, foreColor);
            }
        }

        private void OutputMessage(string message)
        {
            OutputMessage(message, Color.Black);
        }

        private void FormSimulator_MaximumSizeChanged(object sender, EventArgs e)
        {

        }

        private void FormSimulator_SizeChanged(object sender, EventArgs e)
        {

        }

        private void TbxRangeValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && (e.KeyChar != 46) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void CbxTruckScalesPro_SelectedIndexChanged(object sender, EventArgs e)
        {
            //更改仪表型号
            LblDeviceName.Text = CbxTruckScalesPro.Text;
        }
    }
}
