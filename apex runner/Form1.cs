﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsKey;
using static InputMethod;
using static AltShift;
using System.Diagnostics;
using System.Threading;
using System.IO;
using apex_runner.Properties;
using System.Security.Policy;
using System.Runtime.InteropServices;

namespace apex_runner
{
    public partial class Form1 : Form
    {
        // 添加 Windows API 函数声明
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 定义一些常量
        private const int HOTKEY_ID = 9000;
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;   
        }
        //让窗口可以拖拽
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        protected override void WndProc(ref Message message)
        {
            const int WM_HOTKEY = 0x0312;

            switch (message.Msg)
            {
                case WM_HOTKEY:
                    if (message.WParam.ToInt32() == HOTKEY_ID)
                    {
                        // 在这里处理热键事件
                        HandleHotKey();
                    }
                    break;
                
                case WM_NCHITTEST:
                    base.WndProc(ref message);
                    if ((int)message.Result == HTCLIENT)
                        message.Result = (IntPtr)HTCAPTION;
                    return;
            }
            
            base.WndProc(ref message);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //获取当前输入法状态并且标识
            if(InputMethod.CurrentMethod() == InputMethodType.Chinese)
            {
                radioButton_chinese.Checked = true;
            }
            else
            {
                radioButton_eng.Checked = true;
            }
            //获取设置中的 uu 加速器路径\语音软件\ Steam 位置
            textBox1.Text = Settings.Default.uupath;
            textBox2.Text = Settings.Default.oopzpath;
            textBox3.Text = Settings.Default.steampath;

            // 加载保存的快捷键
            string savedShortcut = Settings.Default.shortcut;
            if (!string.IsNullOrEmpty(savedShortcut))
            {
                textBoxShortcut.Text = savedShortcut;
                label8.Text = "当前快捷键: " + savedShortcut;
                BindShortcut(savedShortcut);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 注册热键 (例如: Ctrl + Alt + K)
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, (uint)Keys.K);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // 注销所有热键
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            if (currentHandler != null)
            {
                UnregisterHotKey(this.Handle, 100);
            }
        }

        //禁用 Windows 键功能
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                WindowsKey.Disable();
            }
            else
            {
                WindowsKey.Enable();
            }
        }

        //切换英文输入法功能
        private void radioButton_eng_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_eng.Checked == true)
            {
                InputMethod.ChangeToEnglish();
            }
            else
            {
                InputMethod.ChangeToChinese();
            }
        }

        //切换 alt shift 开关功能
        private void radioButtonaltshiftOff_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonaltshiftOff.Checked == true)
            {
                AltShift.Disable();
            }
            else
            {
                AltShift.Enable();
            }
        }

        //手抖多点出来的,懒得删了
        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }



        //一键优化 开启游戏模式
        private void button1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
            radioButton_eng.Checked = true;
            radioButtonaltshiftOff.Checked = true;
        }

        //一键恢复 开启正常电脑模式
        private void button2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton_chinese.Checked = true;
            radioButtonaltshiftOn.Checked = true;
        }

        //textbox1 在修改时自动保存加速器路径
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.uupath = textBox1.Text;
            Settings.Default.Save();
        }
        //textbox2 在修改时自动保存路径
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.oopzpath = textBox2.Text;
            Settings.Default.Save();
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.steampath = textBox3.Text;
            Settings.Default.Save();
        }
        //右下关于按钮
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"此程序可以帮助你快速的禁用快捷键、禁用中文输入法。\n可以避免一些游戏如MC、Apex中出现影响游戏体验的中文输入法弹窗。\n" +
                $"\nThis program helps you quickly disable shortcut keys and Chinese input methods.\r\nIt can prevent pop-ups from Chinese input methods that may disrupt gameplay in games like Minecraft (MC) or Apex Legends.");
        }
        
        //右下角链接 
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://github.com/cornradio");
        }

        //用于启动加速器的启动函数
        static void StartProgram(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("程序路径不存在或无法启动，请检查路径是否正确。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 创建一个新的进程启动信息
                ProcessStartInfo startInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // 启动程序
                Process process = Process.Start(startInfo);

                // 模拟双击的延迟
                Thread.Sleep(200); // 200毫秒的延迟

                // 再次启动程序
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法启动程序：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //用于开启网页链接的函数
        static void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法打开URL：" + ex.Message);
            }
        }

        //开启加速器 图片按钮
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;
            StartProgram(path);
        }
        //开启语音软件
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string path = textBox2.Text;
            StartProgram(path);
        }
        //开起 Steam
        private void pictureBox_steam_Click(object sender, EventArgs e)
        {
            string path = textBox3.Text;
            StartProgram(path);
        }


        //打开显示设置
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                // 打开Windows的显示设置
                Process.Start("ms-settings:display");
                Console.WriteLine("显示设置已打开。");
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法打开显示设置: " + ex.Message);
            }
        }
        //双击开启音频设置
        private void pictureBox4_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("ms-settings:apps-volume");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        //点击 apex 大图 启动steam
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        bool IsFloded = true;
        private void button3_Click(object sender, EventArgs e)
        {
            double dpi = GetDpiPercent();
            double dpi00 = dpi / 100; //这个参数用来计算展开和收起时,嗯对于hi DPI 屏幕的影响
            if (IsFloded)
            {
                this.Height = Convert.ToInt32(490 * dpi00);
                IsFloded = false;
                button3.Text = "收起路径设置";
            }
            else
            {
                this.Height = Convert.ToInt32(310 * dpi00);
                IsFloded = true;
                button3.Text = "展开路径设置";

            }

        }
        //模拟缩小操作
        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        //关闭按钮
        private void exitbutton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //在窗体被激活时,同时将窗体变成常规状态,这样在尝试开启多个实力时
        //窗体不会以最小化状态被激活(视觉效果就是不会弹出来)
        private void Form1_Activated(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }


        // private void Form1_KeyDown(object sender, KeyEventArgs e)
        // {
            //// 监控具体按键
            ////switch e.KeyCode 并执行功能 (模拟点击)
            ////1. enter 执行 button1
            ////2. BackSpace 执行 button2
            ////3. u 执行 picturebox2
            ////4. d 执行 picturebox4
            ////5. v 执行 picturebox4 双击功能
            ////6. s 执行 pictureBox_steam 
            ////7. o 执行 pictureBox3
            //// 使用 switch 语句监控具体按键
            //switch (e.KeyCode)
            //{
            //    case Keys.Enter:
            //        // 模拟点击 button1
            //        button1.PerformClick();
            //        break;

            //    case Keys.Back:
            //        // 模拟点击 button2
            //        button2.PerformClick();
            //        break;

            //    case Keys.U:
            //        // 模拟点击 pictureBox2
            //        pictureBox2_Click(sender, EventArgs.Empty);
            //        break;

            //    case Keys.D:
            //        // 模拟点击 pictureBox4
            //        pictureBox4_Click(sender, EventArgs.Empty);
            //        break;

            //    case Keys.V:
            //        // 模拟双击 pictureBox4
            //        pictureBox4_DoubleClick(sender, EventArgs.Empty);
            //        break;

            //    case Keys.S:
            //        // 模拟点击 pictureBox_steam
            //        pictureBox_steam_Click(sender, EventArgs.Empty);
            //        break;

            //    case Keys.O:
            //        // 模拟点击 pictureBox3
            //        pictureBox3_Click(sender, EventArgs.Empty);
            //        break;

            //    default:
            //        // 处理其他按键
            //        break;
            //}
        // }
        //获取当前屏幕 DPI
        public double GetDpiPercent()
        {
            double dpiX, dpiY;
            using (Graphics graphics = this.CreateGraphics())
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            // 默认DPI为96（100%），计算DPI百分比
            double dpiPercentageX = (dpiX / 96) * 100;
            double dpiPercentageY = (dpiY / 96) * 100;
            return dpiPercentageX;
            //MessageBox.Show($"DPI 百分比: {dpiPercentageX}% (水平), {dpiPercentageY}% (垂直)");
        }

        //kill ahk
        private void button5_Click(object sender, EventArgs e)
        {
//try{($p=Get-Process|?{$_.Name-eq"AutoHotkey"}).Count;$p|Stop-Process -Force}catch{};Write-Host "成功终止了 $($p.Count) 个 AutoHotkey 进程"
//run this
            try
            {
                Process[] processes = Process.GetProcessesByName("AutoHotkey");
               
                foreach (Process process in processes)
                {
                    process.Kill();
                }
                MessageBox.Show("成功终止了 " + processes.Length + " 个 AutoHotkey 进程");
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法终止 AutoHotkey 进程: " + ex.Message);
            }
            try
            {
                Process[] processes = Process.GetProcessesByName("kasusa_util");

                foreach (Process process in processes)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法终止 AutoHotkey 进程: " + ex.Message);
            }
        }

        private bool isRecordingShortcut = false;
        private List<string> currentKeys = new List<string>();

        private void buttonShortcut_Click(object sender, EventArgs e)
        {
            // 开始记录快捷键
            isRecordingShortcut = true;
            currentKeys.Clear();
            textBoxShortcut.Text = "按下快捷键...";
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isRecordingShortcut) return;

            string keyName = e.KeyCode.ToString();
            if (!currentKeys.Contains(keyName))
            {
                currentKeys.Add(keyName);
            }

            // 更新显示
            textBoxShortcut.Text = string.Join(" + ", currentKeys);
            e.Handled = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isRecordingShortcut) return;

            // 当所有按键都释放时，结束记录
            if (!e.Control && !e.Alt && !e.Shift && 
                e.KeyCode != Keys.ControlKey && 
                e.KeyCode != Keys.ShiftKey && 
                e.KeyCode != Keys.Menu)
            {
                isRecordingShortcut = false;
                this.KeyPreview = false;
                this.KeyDown -= Form1_KeyDown;
                this.KeyUp -= Form1_KeyUp;

                // 保存快捷键到Settings
                string shortcutStr = string.Join(" + ", currentKeys);
                Settings.Default.shortcut = shortcutStr;
                Settings.Default.Save();

                // 绑定快捷键
                BindShortcut(shortcutStr);
            }

            e.Handled = true;
        }

        private int clickCount = 0;
        private KeyEventHandler currentHandler = null;

        private void BindShortcut(string shortcutStr)
        {
            // 如果已有处理程序,先移除
            if (currentHandler != null)
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID);
                this.KeyDown -= currentHandler;
            }

            // 解析快捷键字符串
            string[] keys = shortcutStr.Split(new[] { " + " }, StringSplitOptions.None);
            
            // 设置修饰键
            uint modifiers = 0;
            Keys mainKey = Keys.None;
            foreach (string key in keys)
            {
                if (key == "ControlKey") modifiers |= MOD_CONTROL;
                else if (key == "Alt" || key == "Menu") modifiers |= MOD_ALT;
                else if (key == "ShiftKey") modifiers |= MOD_SHIFT;
                else mainKey = (Keys)Enum.Parse(typeof(Keys), key);
            }

            // 注册全局热键
            if (!RegisterHotKey(this.Handle, HOTKEY_ID, modifiers, (uint)mainKey))
            {
                MessageBox.Show("热键注册失败！可能是该热键已被其他程序占用。");
                return;
            }

            // 更新label8显示
            label8.Text = "当前快捷键: " + shortcutStr;

            // 保存当前的按键设置
            currentHandler = (sender, e) =>
            {
                bool match = true;
                foreach (string key in keys)
                {
                    Keys keyCode = (Keys)Enum.Parse(typeof(Keys), key);
                    if ((Control.ModifierKeys & Keys.Control) == 0 && key == "ControlKey") match = false;
                    if ((Control.ModifierKeys & Keys.Alt) == 0 && (key == "Alt" || key == "Menu")) match = false;
                    if ((Control.ModifierKeys & Keys.Shift) == 0 && key == "ShiftKey") match = false;
                    if (!e.KeyCode.HasFlag(keyCode) && key != "ControlKey" && key != "Menu" && key != "ShiftKey") match = false;
                }
                
                if (match)
                {
                    HandleHotKey();
                    e.Handled = true;
                }
            };

            this.KeyDown += currentHandler;
        }

        private void HandleHotKey()
        {
            // 确保窗口在前台
            if (!this.IsActive)
            {
                this.Activate();
                // 给窗口一点时间来激活
                Thread.Sleep(50);
            }

            switch (clickCount % 3)
            {
                case 0:
                    button1_Click(this, EventArgs.Empty);
                    break;
                case 1:
                    button2_Click(this, EventArgs.Empty);
                    break;
                case 2:
                    button1_Click(this, EventArgs.Empty);
                    break;
            }
            clickCount++;
        }

        // 添加一个IsActive属性
        private bool IsActive
        {
            get
            {
                return Form.ActiveForm == this;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
                // 保存快捷键到Settings
                string shortcutStr = textBoxShortcut.Text;
                Settings.Default.shortcut = shortcutStr;
                Settings.Default.Save();

                // 绑定快捷键
                BindShortcut(shortcutStr);
        }
    }
}
