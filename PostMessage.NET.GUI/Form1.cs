using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PostMessage.NET.GUI
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]  //声明FindWindowAPI
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("USER32.DLL")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        public static extern int SetWindowText(IntPtr hwnd, string lpString);

        public int PerTime = 25000;
        public int DelTime = 1000;

        System.Threading.Timer threadTimer;
        private List<Process> m_Procs = new List<Process> { };
        private Process[] m_Processes;
        private Dictionary<string,string> m_ProcessesString;
        public Form1()
        {
            InitializeComponent();
            m_ProcessesString = new Dictionary<string, string>();
            CheckForIllegalCrossThreadCalls = false;
            threadTimer = new System.Threading.Timer(new TimerCallback(TimerUp), null, Timeout.Infinite, PerTime);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            m_Processes = Process.GetProcessesByName("WowClassic");
            if (m_Processes.Length < wowList.Items.Count)
            {
                wowList.Items.Clear();
            }
            foreach (var wow in m_Processes)
            {
                if(!wowList.Items.Contains(wow.MainWindowHandle.ToString()))
                {
                    int index = wowList.Items.Add(wow.MainWindowHandle.ToString());
                    if (m_ProcessesString.ContainsKey(wow.MainWindowHandle.ToString()))
                    {
                        wowList.SetItemChecked(index, true);
                    }
                }

            }
        }

        ~Form1()
        {
            m_Procs.Clear();
            threadTimer.Change(Timeout.Infinite, PerTime);
            threadTimer.Dispose();
        }

        private void TimerUp(object state)
        {
            foreach (var wow in m_Procs)
            {
                Thread.Sleep(DelTime);
                SendMessage(wow.MainWindowHandle, 0x0104, 0x00000031, 0x20210001); //1
                SendMessage(wow.MainWindowHandle, 0x0105, 0x00000031, 0xE0210001);
                Thread.Sleep(DelTime);
                SendMessage(wow.MainWindowHandle, 0x0104, 0x00000032, 0x20210001); //2
                SendMessage(wow.MainWindowHandle, 0x0105, 0x00000032, 0xE0210001);
                Thread.Sleep(DelTime);
                SendMessage(wow.MainWindowHandle, 0x0104, 0x00000020, 0x20210001); //space
                SendMessage(wow.MainWindowHandle, 0x0105, 0x00000020, 0xE0210001);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int nCount = 0;
            foreach (Process wow in Process.GetProcessesByName("WowClassic"))
            {
                nCount++;
                if (m_ProcessesString.ContainsKey(wow.MainWindowHandle.ToString()))
                {
                    m_Procs.Add(wow);
                    SetWindowText(wow.MainWindowHandle, wow.MainWindowHandle.ToString());
                }
            }
            if (nCount > 0)
            {
                btnStart.Text = "正在对" + nCount.ToString() + "个客户端发送按键中";
                threadTimer.Change(0, PerTime);
                btnStop.Enabled = true;
                btnStart.Enabled = false;
            }
            else
            {
                MessageBox.Show("进程中没有找到客户端");
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            m_Procs.Clear();
            btnStart.Text = "开始";
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            threadTimer.Change(Timeout.Infinite, PerTime);
        }
        private int m_SpaceHeight, m_SpaceWidth, m_SplitHeight;

        private void wowList_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox newsender = (CheckedListBox)sender;
            if (newsender.Text == "") return;
            if (btnStart.Enabled && !m_ProcessesString.ContainsKey(newsender.Text) && newsender.GetItemChecked(newsender.SelectedIndex))
                m_ProcessesString.Add(newsender.Text , newsender.Text);
            else if (!newsender.GetItemChecked(newsender.SelectedIndex))
                m_ProcessesString.Remove(newsender.Text);
            else
            {
                MessageBox.Show("先停止再添加");
                newsender.SetItemChecked(newsender.SelectedIndex, false);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            btnStart.Width = this.Width - m_SpaceWidth;
            btnStart.Height = (this.Height - m_SpaceHeight) / 2;
            btnStop.Width = this.Width - m_SpaceWidth;
            btnStop.Height = (this.Height - m_SpaceHeight) / 2;
            btnStop.Top = btnStart.Top + btnStart.Height + m_SplitHeight;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            m_SpaceWidth = this.Width - btnStart.Width;
            m_SpaceHeight = this.Height - btnStart.Height - btnStop.Height;
            m_SplitHeight = btnStop.Top - btnStart.Top - btnStart.Height;
        }
    }
}
