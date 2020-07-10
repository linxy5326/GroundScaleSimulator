using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xabg.GroundScaleSimulator
{
    /// <summary>
    /// 串口数据收发处理类
    /// </summary>
    public sealed class SerialHelper : IDisposable
    {
        Queue<byte[]> receiveBuffer;
        List<byte> dealBuffer = new List<byte>();
        SerialPort _serialPort;

        bool isRollPoling = false;
        //数据处理接口
        IDealReceive dealReceive;
        int _bufferSize = 1024;

        /// <summary>
        /// 获取或设置数据接收处理对象
        /// </summary>
        public IDealReceive DealReceive
        {
            get { return dealReceive; }
            set { dealReceive = value; }
        }

        /// <summary>
        /// 获取或设置缓存区容量
        /// </summary>
        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        public SerialHelper(SerialPort comPort)
        {
            receiveBuffer = new Queue<byte[]>();
            _serialPort = comPort;
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        /// <summary>
        /// 定义数据接收事件
        /// </summary>
        public event EventHandler<SerialPortDealReceivedEventArgs> DealReceived;

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(200);
            byte[] buffer = new byte[_serialPort.BytesToRead];
            try
            {
                _serialPort.Read(buffer, 0, buffer.Length);
                receiveBuffer.Enqueue(buffer);
            }
            catch
            {
            }
        }

        //启动缓存池数据监听线程，开始监听缓存池。
        public void Start()
        {
            isRollPoling = true;
            ThreadStart ths = new ThreadStart(SerialPortDataReceviedListener);
            Thread thre = new Thread(ths)
            {
                Name = "SerialPortDataReceviedListener",
                IsBackground = true//设置为后台线程
            };
            thre.Start();
        }

        //缓存池数据监听方法。
        private void SerialPortDataReceviedListener()
        {
            bool isReceived = false;
            while (isRollPoling)//轮询数据
            {
                if (receiveBuffer.Count >= 1)
                {
                    isReceived = false;
                    byte[] buffer = receiveBuffer.Dequeue();
                    dealBuffer.AddRange(buffer);
                }
                else
                {
                    if (dealBuffer.Count >= 12)
                    {
                        if (!isReceived)
                        {
                            if (dealReceive != null)
                            {
                                dealReceive.DealWithReceive(dealBuffer);
                                isReceived = true;
                            }
                            else
                            {
                                byte[] temp = dealBuffer.ToArray();
                                SerialPortDealReceivedEventArgs args = new SerialPortDealReceivedEventArgs();
                                if (DealReceived != null)
                                {
                                    args.UserToKen = dealBuffer;
                                    DealReceived(this, args);
                                }
                            }

                        }
                        //如果超出缓存的最大容量，清空缓存
                        if (dealBuffer.Count > _bufferSize)
                        {
                            dealBuffer.RemoveRange(0, (_bufferSize / 2));
                        }
                    }
                    //等待20毫秒
                    Thread.Sleep(20);
                }
            }
        }

        private void OnDealReceived(string dealResult)
        {
            if (DealReceived != null)
            {
                DealReceived(this, new SerialPortDealReceivedEventArgs(dealResult));
            }
        }

        private void Stop()
        {
            lock (this)
            {
                isRollPoling = false;
            }
        }

        #region IDispose

        private bool disposed = false;
        /// <summary>
        /// 释放资源
        /// 调用该方法，会把相关联的资源也释放掉。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    Stop();
                    dealBuffer = null;
                    receiveBuffer = null;
                    DealReceive = null;
                }
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed.
                _serialPort.Dispose();
            }
            disposed = true;
        }
        #endregion
    }
    /// <summary>
    /// 串行端口数据处理抽象类
    /// </summary>
    public abstract class SerialPortDealReceive : IDealReceive, IDisposable
    {
        /// <summary>
        /// 定义数据接收事件
        /// </summary>
        public event EventHandler<SerialPortDealReceivedEventArgs> DealReceived;

        /// <summary>
        /// 虚方法，处理接收的数据
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void DealWithReceive(List<byte> buffer)
        {

        }

        /// <summary>
        /// 激活数据接收处理通知事件。
        /// </summary>
        /// <param name="dealResult"></param>
        protected virtual void OnDealReceived(string dealResult)
        {
            OnDealReceived(dealResult, null);
        }

        protected virtual void OnDealReceived(string dealResult, object UserToKen)
        {
            if (DealReceived != null)
            {
                SerialPortDealReceivedEventArgs args = new SerialPortDealReceivedEventArgs(dealResult)
                {
                    UserToKen = UserToKen
                };
                DealReceived(this, args);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public virtual void Dispose()
        {

        }
    }

    /// <summary>
    /// 数据处理事件参数类
    /// </summary>
    public class SerialPortDealReceivedEventArgs : EventArgs
    {
        string _result;
        object _userToKen;

        /// <summary>
        /// 获取或设置一个UserToken
        /// </summary>
        public object UserToKen
        {
            get { return _userToKen; }
            set { _userToKen = value; }
        }

        /// <summary>
        /// 获取处理后的数据
        /// </summary>
        public string DealResult
        {
            get { return _result; }
        }

        public SerialPortDealReceivedEventArgs()
        {
            _result = string.Empty;
        }

        public SerialPortDealReceivedEventArgs(string result)
        {
            _result = result;
        }
    }

    /// <summary>
    /// 串行数据接收处理接口
    /// </summary>
    public interface IDealReceive
    {
        /// <summary>
        /// 实现数据处理接口方法，该方法允许外部处理数据。
        /// </summary>
        /// <param name="buffer">接收的数据</param>
        void DealWithReceive(List<byte> buffer);

    }
}
