using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;

namespace MachineLearningTools_x64_
{
    public partial class Form1 : Form
    {
        public static string combuff = " ";
        public static byte[] combuff2;
        public static string sendRecoder;
        public static string receiveRecorder;
        public static int ComOpenState;
        private SerialPort comm = new SerialPort();
        private StringBuilder builder = new StringBuilder();
        public DateTime m_StartTime;

        public void comstart()         //串口扫描与初始化:用于先开程序后插串口。但拔出后再插上还是不行
        {
            //初始化下拉串口名称列表框  comboPortName，为了软件能通用所有电脑,避免每次查询的效率损失，使用微软提供的枚举方式
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            combo_PortName.Items.AddRange(ports);
            combo_PortName.SelectedIndex = combo_PortName.Items.Count > 0 ? 0 : -1;
            combo_Baudrate.SelectedIndex = combo_Baudrate.Items.IndexOf("115200");

            //初始化SerialPort对象
            comm.NewLine = "\r\n";
            comm.RtsEnable = true;//根据实际情况吧。
            comm.ReadTimeout = 1;           //超时1ms 清空缓冲器
            
            //添加事件注册
            comm.DataReceived += comm_DataReceived;//这个事件注册必须做，否则不能接收
            this.textBox_Main.AppendText("Serial Port Scanning Finished \r\n");
        }

        void comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int n = comm.BytesToRead; //先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
            byte[] buf = new byte[n]; //声明一个临时数组存储当前来的串口数据

            comm.Read(buf, 0, n);  //读取缓冲数据
            builder.Clear();       //清除字符串构造器的内容
                                   //因为要访问ui资源，所以需要使用invoke方式同步ui。
            this.Invoke((EventHandler)(delegate          //都要以hex方式接收出来
            {

                builder.Append(Encoding.ASCII.GetString(buf));         //直接按ASCII规则转换成字符串
                                                                       //this.textBox1.AppendText(builder.ToString());                //追加的形式添加到文本框末端，并滚动到最后。
                                                                       //textBox1.Text = builder.ToString();  //单独的每次数据，但只能显示32个ascii ！！！！接收数据接口

                combuff = builder.ToString();
                combuff2 = buf;

                received_Dealing();

            }));
        }

        void received_Dealing()
        {
            string Time_now= DateTime.Now.ToString();
            string comannd_explain = "";

            if ((combuff != null) && (combuff.Length == 0))
            {
                //textBox_Main.AppendText(" \r\n");     //空字符串检验
            }
            else
            {

                if (combuff[0] == '{')   //起始位校验：成功
                {
                    switch (combuff[1])                   //(int)Convert.ToChar(combuff[1]))
                    {

                        case 'A': m_StartTime = DateTime.Now; comannd_explain = ": Processing Start";timer2.Enabled = true; checkBox_Timer.Checked = true;  break;

                        case 'Z': checkBox_Timer.Checked = false; comannd_explain = ": Processing End"; timer2.Enabled = false; break;
                        default: break;
                    }
                }

               
            }

           // textBox_Main.AppendText(Time_now + ": "+combuff+ comannd_explain);
            textBox_Main.AppendText( combuff);

        }
        public void comopen()          //打开串口
        {

            if (comm.IsOpen)
            {

                //防止重复开启串口
                this.textBox_Main.AppendText("Serial Port is Already Opened！ \r\n");

            }

            else
            {
                comstart();
                comm.PortName = combo_PortName.Text;
                comm.BaudRate = int.Parse(combo_Baudrate.Text);
                try
                {
                    comm.Open();
                    ComOpenState = 1;
                    this.textBox_Main.AppendText("Serial Port is Already Opened！ \r\n");
                }
                catch (Exception ex)
                {
                    //捕获到异常信息，创建一个新的comm对象，之前的不能用了。
                    comm = new SerialPort();
                    //现实异常信息给客户。
                    MessageBox.Show(ex.Message);

                }
            }
        }


        public Form1()
        {
            InitializeComponent();
        }




        private void FilestoolStripDropDownButton1_Click(object sender, EventArgs e)
        {
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comstart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comopen();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            TimeSpan span = DateTime.Now - m_StartTime;
            DateTime n = new DateTime(span.Ticks);
            if (checkBox_Timer.Checked==true)
            {
              label_Timer.Text = n.ToString("HH:mm:ss:fff");
            }
            
        }

        private void SetTim_Button_Click(object sender, EventArgs e)
        {

        }
    }
}
