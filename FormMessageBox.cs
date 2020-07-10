using MetroFramework.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace xabg.GroundScaleSimulator
{
    public partial class FormMessageBox : MetroForm
    {
        private static FormMessageBox _instance;

        public RichTextBox MessageConText
        {
            get { return RxtDataOut; }
        }

        public void SetMessageText(string message)
        {
            if (RxtDataOut.IsDisposed) return;

            AppendTextColorFull(message, Color.Black, true);
            RxtDataOut.HideSelection = false;
           // RxtDataOut.Select(message.Length, 0);
           // RxtDataOut.ScrollToCaret();
        }

        public void SetMessageText(string message, Color foreColor)
        {
            if (RxtDataOut.IsDisposed) return;

            AppendTextColorFull(message, foreColor, true);
            RxtDataOut.Select(message.Length, 0);
            RxtDataOut.ScrollToCaret();
        }

        private void AppendTextColorFull(string text, Color color, bool addNewLine)
        {

            if (addNewLine)
            { text += Environment.NewLine; }
            RxtDataOut.SelectionStart = RxtDataOut.TextLength;
            RxtDataOut.SelectionLength = 0;
            RxtDataOut.SelectionColor = color;
            RxtDataOut.AppendText(text);
            RxtDataOut.SelectionColor = RxtDataOut.ForeColor;
        }


        public FormMessageBox()
        {
            InitializeComponent();
            //隐藏窗体标题栏
            this.FormBorderStyle = FormBorderStyle.None;
        }

        /// <summary>
        /// 创建FormMessageBox新实例
        /// </summary>
        /// <returns></returns>
        public static FormMessageBox GetInstance()
        {
            if (null == _instance || _instance.IsDisposed)
                _instance = new FormMessageBox();

            return _instance;
        }

        private void FormMessageBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            _instance = null;
        }

        private void TSBtnClear_Click(object sender, EventArgs e)
        {
            RxtDataOut.Clear();
        }
    }
}
