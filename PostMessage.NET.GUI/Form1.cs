using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Concurrent;

namespace PostMessage.NET.GUI
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]  //声明FindWindowAPI
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "PostMessage", CallingConvention = CallingConvention.Winapi)]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, uint wParam, uint lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        public static extern int SetWindowText(IntPtr hwnd, string lpString);

        public int PerTime = 4600;

        private string processName = "Wow";

        System.Threading.Timer threadTimer;
        private ConcurrentBag<Process> m_Procs = new ConcurrentBag<Process> { };
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            threadTimer = new System.Threading.Timer(new TimerCallback(TimerUp), null, Timeout.Infinite, PerTime);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var checkedItems = new HashSet<string>(wowList.CheckedItems.Cast<string>());

            // 获取当前 "Wow" 进程的列表
            Process[] processes = Process.GetProcessesByName(processName);

            // 使用 HashSet 来存储进程显示信息
            var newProcesses = new HashSet<string>();

            // 添加新的进程信息
            foreach (var process in processes)
            {
                string processDisplay = process.ProcessName + " (ID: " + process.Id + ")";
                newProcesses.Add(processDisplay);
            }

            // 清除不再存在的进程信息
            for (int i = wowList.Items.Count - 1; i >= 0; i--)
            {
                string item = wowList.Items[i].ToString();
                if (!newProcesses.Contains(item))
                {
                    wowList.Items.RemoveAt(i);
                }
            }

            // 添加新的进程信息并设置选中状态
            foreach (var processDisplay in newProcesses)
            {
                if (!wowList.Items.Contains(processDisplay))
                {
                    wowList.Items.Add(processDisplay);
                }
            }

            // 恢复之前保存的选中状态
            foreach (var item in newProcesses)
            {
                if (checkedItems.Contains(item))
                {
                    int index = wowList.Items.IndexOf(item);
                    if (index != -1)
                    {
                        wowList.SetItemChecked(index, true);
                    }
                }
            }
        }

        ~Form1()
        {
            threadTimer.Change(Timeout.Infinite, PerTime);
            threadTimer.Dispose();
        }

        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const uint WM_MOUSEMOVE = 0x0200;
        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint KEY_1 = 0x31;
        private void TimerUp(object state)
        {
            foreach (var wow in m_Procs)
            {
                Process p = Process.GetProcessById(wow.Id);
                IntPtr h = p.MainWindowHandle;
                uint nPos = (uint)( 5<< 16 | 5);
                PostMessage(h, WM_MOUSEMOVE, 0, nPos);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_LBUTTONDOWN, 0, nPos);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_LBUTTONUP, 0, nPos);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_LBUTTONDOWN, 0, nPos);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_LBUTTONUP, 0, nPos);
                System.Threading.Thread.Sleep(2000);
                System.Threading.Thread.Sleep(200);
                PostMessage(h, WM_KEYDOWN, KEY_1, 0);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_KEYUP, KEY_1, 0);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_KEYDOWN, KEY_1, 0);
                System.Threading.Thread.Sleep(30);
                PostMessage(h, WM_KEYUP, KEY_1, 0);
                System.Threading.Thread.Sleep(1750);
                System.Threading.Thread.Sleep(200);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            // 遍历选中的进程项
            foreach (var item in wowList.CheckedItems)
            {
                string selectedItem = item.ToString();

                // 从选中的项中提取进程ID
                string processIdStr = selectedItem.Split(new string[] { "(ID: ", ")" }, StringSplitOptions.None)[1];
                int processId = int.Parse(processIdStr);

                // 查找对应的进程
                Process process = Process.GetProcessById(processId);

                // 如果进程不在 m_Procs 列表中，添加它
                if (!m_Procs.Any(p => p.Id == process.Id))
                {
                    m_Procs.Add(process);
                }
            }
            threadTimer.Change(0, PerTime);
            btnStop.Enabled = true;
            btnStart.Enabled = false;
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            threadTimer.Change(Timeout.Infinite, PerTime);
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            m_Procs = new ConcurrentBag<Process>();
        }
        private int m_SpaceHeight, m_SpaceWidth, m_SplitHeight;

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
