using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TreeDemo
{
    public partial class frmTree : Form
    {
        public frmTree()
        {
            InitializeComponent();
        }

        private List<string> GetAllNodeInfo(TreeView tvDept)
        {
            List<string> lst = new List<string>();
            for (int i = 0; i < tvDept.Nodes.Count; i++)
            {
                string od = string.Empty;
                od = string.Format("Level：{0}，Nodes：{1}，Name：{2}，Text：{3}\r\n",
                    tvDept.Nodes[i].Level.ToString().PadLeft(3), tvDept.Nodes[i].Nodes.Count.ToString().PadLeft(3),
                    tvDept.Nodes[i].Name, tvDept.Nodes[i].Text);
                lst.Add(od);

                if (tvDept.Nodes[i].Nodes.Count > 0)
                {
                    GetAllNodeInfoSub(tvDept.Nodes[i], lst);
                }
            }

            return lst;
        }
        private void GetAllNodeInfoSub(TreeNode nodeRoot, List<string> lst)
        {
            for (int i = 0; i < nodeRoot.Nodes.Count; i++)
            {
                string od = string.Empty;
                od = string.Format("Level：{0}，Nodes：{1}，Name：{2}，Text：{3}\r\n",
                    nodeRoot.Nodes[i].Level.ToString().PadLeft(3), nodeRoot.Nodes[i].Nodes.Count.ToString().PadLeft(3),
                    nodeRoot.Nodes[i].Name, nodeRoot.Nodes[i].Text);
                lst.Add(od);

                if (nodeRoot.Nodes[i].Nodes.Count > 0)
                {
                    GetAllNodeInfoSub(nodeRoot.Nodes[i], lst);
                }
            }
        }

        private void frmTree_Load(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
            treeView1.HideSelection = false;
            treeView1.CheckBoxes = checkBox1.Checked;
            treeView1.ShowLines = checkBox2.Checked;
            treeView1.ShowRootLines = checkBox3.Checked;
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                //节点信息
                string strNodeInfo = string.Empty;
                strNodeInfo += string.Format("Name：{0}\r\n", e.Node.Name);
                strNodeInfo += string.Format("Text：{0}\r\n", e.Node.Text);
                strNodeInfo += string.Format("Nodes：{0}\r\n", e.Node.Nodes.Count.ToString());
                strNodeInfo += string.Format("Level：{0}\r\n", e.Node.Level);
                txtNodeInfo.Text = strNodeInfo;
                //显示窗体
                //switch (e.Node.Text)
                //{
                //    case "ndForm1":
                //        (new Form1()).ShowDialog();
                //        break;
                //    case "ndForm2":
                //        (new Form2()).ShowDialog();
                //        break;
                //    case "ndForm3":
                //        (new Form3()).ShowDialog();
                //        break;
                //}
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode tn = new TreeNode();
            tn.Name = tn.Text = "tn" + DateTime.Now.ToString("yyMMddhhmmss") + DateTime.Now.Millisecond.ToString("D3");
            treeView1.Nodes.Add(tn);
            System.Threading.Thread.Sleep(1);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }
            else
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    button1_Click(null, null);
                }
                else
                {
                    TreeNode tn = new TreeNode();
                    tn.Name = tn.Text = "tn" + DateTime.Now.ToString("yyMMddhhmmss") + DateTime.Now.Millisecond.ToString("D3");
                    treeView1.SelectedNode.Parent.Nodes.Add(tn);
                    System.Threading.Thread.Sleep(1);
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }
            else
            {
                TreeNode tn = new TreeNode();
                tn.Name = tn.Text = "tn" + DateTime.Now.ToString("yyMMddhhmmss") + DateTime.Now.Millisecond.ToString("D3");
                treeView1.SelectedNode.Nodes.Add(tn);
                System.Threading.Thread.Sleep(1);
                treeView1.ExpandAll();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                treeView1.Nodes.Remove(treeView1.SelectedNode);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                treeView1.SelectedNode.Nodes.Clear();
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            treeView1.CheckBoxes = checkBox1.Checked;
            treeView1.ExpandAll();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            treeView1.ShowLines = checkBox2.Checked;
            treeView1.ExpandAll();
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            treeView1.ShowRootLines = checkBox3.Checked;
            treeView1.ExpandAll();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            List<string> lst = GetAllNodeInfo(treeView1);
            txtNodeInfo.Text = string.Empty;

            for (int i = 0; i < lst.Count; i++)
            {
                txtNodeInfo.Text += lst[i];
                Application.DoEvents();
            }
        }

    }
}