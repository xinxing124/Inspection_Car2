using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.IO;
using System.Reflection;
using Eval;

namespace Inspection_Car2
{
    public partial class Form1 : Form
    {
        [DllImport("Coredll.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
        /// <summary> 
        /// 得到当前活动的窗口 
        /// </summary> 
        /// <returns></returns> 
        [DllImport("Coredll.dll", CharSet = CharSet.Auto)]
        private static extern System.IntPtr GetForegroundWindow();

        /// <summary>
        /// 设置鼠标状态的计数器（非状态）
        /// </summary>
        /// <param name="bShow">状态</param>
        /// <returns>状态技术</returns>
        [DllImport("Coredll.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public static extern int ShowCursor(bool bShow);

        [DllImport("HDI_API.dll")]
        public static extern bool API_GPIO_Output(byte channel, byte level);

        [DllImport("HDI_API.dll")]
        //public static extern bool API_GPIO_Input(byte channel, byte* level);
        public static extern bool API_GPIO_Input(byte channel, out byte level);

        [DllImport("HDI_API.dll")]
        //public static extern bool API_ADC_Read(byte channel, Int16* pval);
        public static extern bool API_ADC_Read(byte channel,out Int16 pval);

        const double PI = 3.1415926;
        private double dVoltage=0.0;
        private double dSpeed=0.0;
        private double dVoltage2 = 0.0;
        private double dVoltageParameter=0.0;
        private double dVelocityParameter=0.0;
        private double dVoltageParameter2 = 0.0;
        private string sVoltageParameter = "0.0";
        private string sVelocityParameter = "0.0";
        private string sVoltageParameter2 = "0.0";
        private string dt;
        private int iExitCount=0;
        private int iTimerCount = 0;
        delegate void HandleInterfaceUpdateDelegate(string text);

        private int TSpeedLimit = 10;
        private int TSpeedIndex = 0;        
        private double[] TSpeedVal = new double[20];
        private Image myImage;//声明一个图形类变量
        private Image myImageA;//声明一个图形类变量
        private Image myImageB;//声明一个图形类变量
        private Image myImageC;//声明一个图形类变量
        private Image myImageD;//声明一个图形类变量
        private Image myImageE;//声明一个图形类变量
        private Image myImageF;//声明一个图形类变量
        private Bitmap bmp;
        private Graphics bgr;
        private Brush br;
        private int ClientHeight;
        private int ClientWidth;
        private bool blnPrintFlag = false;
        public Form1()
        {
            InitializeComponent();
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

            // SetStyle(ControlStyles.Opaque, true);   //在窗口构造函数中指定有关透明与否的控件绘制模式样式
            myImageA = new Bitmap(Inspection_Car2.Properties.Resources.PRINT1); //读取事先已加入资源的位图
            myImageB = new Bitmap(Inspection_Car2.Properties.Resources.PRINT2); //读取事先已加入资源的位图
            myImageC = new Bitmap(Inspection_Car2.Properties.Resources.PRINT3); //读取事先已加入资源的位图
            myImageD = new Bitmap(Inspection_Car2.Properties.Resources.PRINT4); //读取事先已加入资源的位图
            myImageE = new Bitmap(Inspection_Car2.Properties.Resources.PRINT5); //读取事先已加入资源的位图
            myImageF = new Bitmap(Inspection_Car2.Properties.Resources.PRINT6); //读取事先已加入资源的位图
            myImage = new Bitmap(Inspection_Car2.Properties.Resources.PRINT7); //读取事先已加入资源的位图
            
            ClientWidth=this.ClientSize.Width;
            ClientHeight = this.ClientSize.Height;


            bmp = new Bitmap(ClientWidth, ClientHeight);
            bgr = Graphics.FromImage(bmp);
            br = new SolidBrush(Color.White);
  
            //bgr.DrawImage(myImageF, 0, 0);
            //Graphics g = this.CreateGraphics();
            //g.DrawImage(bmp, 0, 0);
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen) serialPort1.Close();

            // Set the port's settings
            serialPort1.BaudRate = 9600;//int.Parse(comboBox2.Text);
            serialPort1.DataBits = 8;//int.Parse(comboBox3.Text);
            serialPort1.StopBits = StopBits.One ; //(StopBits)Enum.Parse(typeof(StopBits), comboBox5.Text, true);
            serialPort1.Parity =Parity.None; //(Parity)Enum.Parse(typeof(Parity), comboBox4.Text, true);
            serialPort1.PortName = "COM3";// comboBox1.Text;
            // Open the port
            serialPort1.Open();
            
            dVoltage=0.0;
            dSpeed=0.0;
            dt=DateTime.Now.ToString("yyyy/MM/dd HH:mm");

            string sFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            
            if (File.Exists("\\硬盘\\xishu.txt")) //判断文件是否存在
            {
                if (sFilePath.Equals("\\硬盘") == false)
                {
                    File.Copy("\\硬盘\\xishu.txt", sFilePath + "\\xishu.txt", true);
                }

            }
            if (File.Exists(sFilePath + "\\xishu.txt")) //判断文件是否存在
            {
                //按行读取为字符串数组
                StreamReader strReader = File.OpenText(sFilePath + "\\xishu.txt");
                string sTemp = strReader.ReadLine();  
                //string strTemp = strReader.ReadToEnd();  
                strReader.Close();  
                
                
                string[] sStr = sTemp.Split(new char[1] { ',' });
                //try
                //{
                //    dVoltageParameter = Convert.ToDouble(sStr[0]);
                //    dVelocityParameter = Convert.ToDouble(sStr[1]);
                //    dVoltageParameter2 = Convert.ToDouble(sStr[2]);
                //}
                //catch
                //{
                    sVoltageParameter = sStr[0];
                    sVelocityParameter = sStr[1];
                    sVoltageParameter2 = sStr[2];
                //}
                //MessageBox.Show(sStr[0] + "____" + sStr[1]);
                //textBox1.Text = sTemp;
            }
            
            
            timer1.Enabled = true;
            ShowCursor(false);
            SetWindowPos(this.Handle, -1, 0, 0, 0, 0, 1|2);
        }

        
        //private void UpdateReceiveTextBox(string text)
        //{
        //    double dtSpeed = 0.0;
        //    double dtVoltage = 0.0;
        //    string dtt;
            

            
        //    //不在同一线程
        //    if (textBox1.InvokeRequired)
        //    {
        //        HandleInterfaceUpdateDelegate InterfaceUpdate = new HandleInterfaceUpdateDelegate(UpdateReceiveTextBox);
        //        Invoke(InterfaceUpdate, new object[] { text });
        //    }
        //    //在同一线程
        //    else
        //    {
        //        textBox1.Text = text;
        //        dtSpeed = Convert.ToDouble(text.Substring(2, 6));
        //        dtVoltage = Convert.ToDouble(text.Substring(9, 6));
        //        dtt = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        //        if (dt.Equals(dtt) == false || dtSpeed != dSpeed || dtVoltage != dVoltage)
        //            this.Invalidate();
        //    }
            
        //}

        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        private void Form1_Closed(object sender, EventArgs e)
        {
            //int i;
            //port_level = &i;
            //read_digital_input(port_level, 0);

            //set_digital_output(1, 2);
            //read_digital_input(port_level, 6);

            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }        
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            iExitCount = iExitCount + 1;
            if (iExitCount > 5)
            {
                this.Close();
            }
        }

        private void Form1_Paint_1(object sender, PaintEventArgs e)
        {
            //base.OnPaint(e);
            Graphics g = e.Graphics;
            if (dVoltage >= 9.5) {
                bgr.DrawImage(myImageA, 0, 0);
            }
            else if (dVoltage >= 9.0){
                bgr.DrawImage(myImageB, 0, 0);
            }
            else if (dVoltage >= 8.5){
                bgr.DrawImage(myImageC, 0, 0);
            }
            else if (dVoltage >= 8.0){
                bgr.DrawImage(myImageD, 0, 0);
            }
            else if (dVoltage >= 7.5){
                bgr.DrawImage(myImageE, 0, 0);
            }
            else if (dVoltage < 7.5){
                bgr.DrawImage(myImageF, 0, 0);
            }

            bgr.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm"), new Font("黑体", 12, FontStyle.Bold), br, 168, 22);
            //bgr.DrawString(DateTime.Now.ToString("HH:mm"), new Font("黑体", 12, FontStyle.Bold), br, 275, 22);
            //g.DrawString(DateTime.Now.ToLongTimeString().ToString(), new Font("黑体", 16, FontStyle.Regular), br, 250, 20);
            bgr.DrawString((dVoltage * dVoltageParameter).ToString("f1") + "V", new Font("黑体", 15, FontStyle.Bold), br, 387, 105);

            bgr.DrawString((dSpeed*dVelocityParameter).ToString("f2") + "KM/h", new Font("黑体", 15, FontStyle.Bold), br, 387, 180);

            bgr.DrawLine(new Pen(Color.Red, 3), 183, 228, Convert.ToInt32(183 + Math.Cos((360 - (180 - (-15 + (dSpeed * dVelocityParameter) * 5.2 / 5 * 180 / 27))) * PI / 180) * 126), Convert.ToInt32(228 + Math.Sin((360 - (180 - (-15 + (dSpeed * dVelocityParameter) * 5.2 / 5 * 180 / 27))) * PI / 180) * 126));
            //bgr.FillRectangle(br, 180, 190, Convert.ToInt32(149 + Math.Cos((360 - (180 - (-38 + dSpeed * 7.12 / 5 * 180 / 27))) * PI / 180) * 126), Convert.ToInt32(152 + Math.Sin((360 - (180 - (-38 + dSpeed * 7.12 / 5 * 180 / 27))) * PI / 180) * 126));
            
    //            oldbrush=pControlDC->SelectObject(&brush);
    //pControlDC->Ellipse(10,10,40,40);
    //pControlDC->SelectObject(oldbrush);

    //CPen RectPen(PS_DASH,5,0x0000FF);
    //pControlDC->SelectObject(&RectPen); 

    ////-----------------------------------------------
    ////-- Draw
    ////-----------------------------------------------
    ////pControlDC->Rectangle(rct.left+10,rct.top+10,rct.right-10,rct.bottom-10); 

    //pControlDC->MoveTo(148,151);
    ////pControlDC->LineTo(25,151);
    ////pControlDC->LineTo(149+cos((360-(180-(-38)))*PI/180)*126,152+sin((360-(180-(-38)))*PI/180)*126);
	
    ////pControlDC->LineTo(149+cos((360-(180-(218)))*PI/180)*126,152+sin((360-(180-(218)))*PI/180)*126);
    ////double Val=25.0;
    //pControlDC->LineTo(149+cos((360-(180-(-38+SpeedVal*7.12/5*180/27)))*PI/180)*126,152+sin((360-(180-(-38+SpeedVal*7.12/5*180/27)))*PI/180)*126);
    ////pControlDC->LineTo(100,100);

            //dSpeed
            

            e.Graphics.DrawImage(bmp, 0, 0);
            /*
            using (Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height))
            {
                using (Graphics bgr = Graphics.FromImage(bmp))
                {
                    bgr.Clear(this.BackColor);
                    if (myImageE != null) bgr.DrawImage(myImageE, 0, 0);
                }
                e.Graphics.DrawImage(bmp, 0, 0);
            }
            */

            //g.DrawImage(myImageA, ClientRectangle);//将图绘制在整个客户区（图形大小随客户区）
            //Rectangle myRec = new Rectangle(10, 10, 100, 100); //指定显示区域的位置的大小


            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int iRevCount=0;
            double dtSpeed = 0.0;
            double dtVoltage = 0.0;
            double dtVoltage2 = 0.0;
            string dtt,buf;
            string sSpeed,stSpeed;
            string sVoltage,stVoltage;
            string sVoltage2, stVoltage2;
 
            iTimerCount = iTimerCount + 1;
            if (iTimerCount > 5)
            {
                iTimerCount = 0;
                iExitCount = 0;
            }

            if (!serialPort1.IsOpen)
                return;

            iRevCount = serialPort1.BytesToRead;
            if (iRevCount > 2)
            {
                // Read all the data waiting in the buffer
                //data = serialPort1.ReadExisting();
                byte []revbytes=new byte[13];
                int []revint16=new int[4];
                serialPort1.Read(revbytes,0,13);
                
                for(int i=3,j=0;i<11;j++,i+=2)
                    revint16[j] =revbytes[i] * 256+ revbytes[i + 1];
                
                dtVoltage=revint16[0]/1000.0;
                dtSpeed =revint16[1]/1000.0;
                dtVoltage2 = revint16[2] / 1000.0;

                dtt = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

                buf = Convert.ToString(dtVoltage);
                stVoltage = buf.Substring(0, buf.IndexOf(".") + 2);

                buf = Convert.ToString(dtSpeed);
                stSpeed = buf.Substring(0, buf.IndexOf(".") + 2);

                buf = Convert.ToString(dtVoltage2);
                stVoltage2 = buf.Substring(0, buf.IndexOf(".") + 2);

                buf = Convert.ToString(dVoltage);
                sVoltage = buf.Substring(0, buf.IndexOf(".") + 2);

                buf = Convert.ToString(dSpeed);
                sSpeed = buf.Substring(0, buf.IndexOf(".") + 2);

                buf = Convert.ToString(dVoltage2);
                sVoltage2 = buf.Substring(0, buf.IndexOf(".") + 2);

                if (dt.Equals(dtt) == false || sSpeed.Equals(stSpeed) == false || sVoltage.Equals(stVoltage) == false || sVoltage2.Equals(stVoltage2) == false)
                {
                    dt = dtt;
                    dVoltage = dtVoltage;
                    dSpeed = dtSpeed;
                    dVoltage2 = dtVoltage2;

                    //this.Invalidate();
                    //Graphics g = this.CreateGraphics();
                    //this.Invalidate(new Rectangle(10, 10, 100, 100));
                    //Image img = Image.FromFile("g1.jpg");//建立Image对象
                    //Graphics g = Graphics.FromImage(img);//创建Graphics对象
                    TSpeedVal[TSpeedIndex++] = dSpeed;
                    if (TSpeedIndex == TSpeedLimit)
                    {
                        TSpeedIndex = 0;
                        TSpeedVal[TSpeedIndex++] = dSpeed;

                        dSpeed = Filter(TSpeedVal);
                        Show_Refresh();
                    }
                    //Show_Refresh();
                }
                
            }
            else
            {
                try
                {
                    // Send the user's text straight out the port
                    //serialPort1.Write("#01\r");
                    //00 06 00 00 01 00 89 8D
                    //01 03 02 58 00 04 C4 62
                    byte[] sendbytes = { 0x01, 0x03, 0x02, 0x58, 0x00, 0x04, 0xC4,0x62 };
                    serialPort1.Write(sendbytes,0,8);
                }
                catch (FormatException)
                {
                    Console.WriteLine("发送指令失败！");
                }

                if (!blnPrintFlag)
                {
                    dVoltage = 0.0;
                    dSpeed = 0.0;
                    dVoltage2 = 0.0;

                    Show_Refresh();
                    blnPrintFlag = true;
                }
            }



            //    if (DataMode == 0)//text show
            //{
            //    // Read all the data waiting in the buffer
            //    string data = serialPort1.ReadExisting();
                    

            //    // Display the text to the user in the terminal
            //    UpdateReceiveTextBox(data);
            //}
            //else//hex show
            //{
            //    // Obtain the number of bytes waiting in the port's buffer
            //    int bytes = serialPort1.BytesToRead;

            //    // Create a byte array buffer to hold the incoming data
            //    byte[] buffer = new byte[bytes];

            //    // Read the data from the port and store it in our buffer
            //    serialPort1.Read(buffer, 0, bytes);

            //    // Show the user the incoming data in hex format
            //    string data = ByteArrayToHexString(buffer);
            //    // Display the text to the user in the terminal
            //    UpdateReceiveTextBox(data);
            //}


            //     if (!serialPort1.IsOpen)
            //    return;
            //if (DataMode == 0)//text send
            //{
            //    try
            //    {
            //        // Send the user's text straight out the port
            //        serialPort1.Write(textBox1.Text);
            //    }
            //    catch (FormatException)
            //    {
            //        MessageBox.Show("send error.");
            //    }
            //}
            //else//hex send
            //{
            //    try
            //    {
            //        // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
            //        byte[] data = HexStringToByteArray(textBox1.Text);

            //        // Send the binary data out the port
            //        serialPort1.Write(data, 0, data.Length);
            //    }
            //    catch (FormatException)
            //    {
            //        MessageBox.Show("send error.");
            //    }
            //}
        }

        private uint crc16_modebus(byte[] modbusdata, uint modbusdatalength, byte[] crcval)
        {
            uint i, j;
            uint crc16 = 0xFFFF;

            for (i = 0; i < modbusdatalength; i++)
            {
                crc16 ^= modbusdata[i];
                for (j = 0; j < 8; j++)
                {
                    if ((crc16 & 0x01) == 1)
                        crc16 = (crc16 >> 1) ^ 0xA001;
                    else
                        crc16 = crc16 >> 1;
                }
            }
            crcval[0] = (byte)((crc16 & 0xff00) >> 8);//高位置
            crcval[1] = (byte)(crc16 & 0x00ff);  //低位置
            return crc16;
        }

        public double Filter(double[] a)
        {  
            double temp=0.0;
            for (int i = 0; i < TSpeedLimit; i++)
            {
                for (int j = 0; j < TSpeedLimit - i; j++)
                {
                    if (a[j] > a[j + 1])
                    {
                        temp = a[j];
                        a[j] = a[j + 1];
                        a[j + 1] = temp;
                    }
                }
            }
            //return a[0];
            return a[TSpeedLimit/2+1];

            //for (int i = 0; i < TSpeedLimit; i++)
            //{
            //    temp += a[i];
            //}
            //return temp / TSpeedLimit;
        }  

        private void Show_Refresh()
        {
            string dtt, buf;
            bgr.DrawImage(myImage, 0, 0);
            double tmpVoltage = 0.0, tmpVoltage2 = 0.0, tmpdSpeed = 0.0;

            string tmpExp = "1.0";
            RPN rpn = new RPN();
            if (sVoltageParameter.IndexOf("X") > 0)
            {
                tmpExp = sVoltageParameter.Replace("X", dVoltage.ToString());
                if (rpn.Parse(tmpExp))
                {
                    tmpVoltage = Convert.ToDouble(rpn.Evaluate());
                }
            }
            else 
            {
                dVoltageParameter = Convert.ToDouble(sVoltageParameter);
                tmpVoltage = dVoltage * dVoltageParameter;
            }
            if (sVoltageParameter2.IndexOf("X") > 0)
            {
                tmpExp = sVoltageParameter2.Replace("X", dVoltage2.ToString());
                if (rpn.Parse(tmpExp))
                {
                    tmpVoltage2 = Convert.ToDouble(rpn.Evaluate());
                }
            }
            else
            {
                dVoltageParameter2 = Convert.ToDouble(sVoltageParameter2);
                tmpVoltage2 = dVoltage2 * dVoltageParameter2;
            }
            if (sVelocityParameter.IndexOf("X") > 0)
            {
                tmpExp = sVelocityParameter.Replace("X", dSpeed.ToString());
                if (rpn.Parse(tmpExp))
                {
                    tmpdSpeed = Convert.ToDouble(rpn.Evaluate())+0.005;
                }
            }
            else
            {
                dVelocityParameter = Convert.ToDouble(sVelocityParameter);
                tmpdSpeed = dSpeed * dVelocityParameter + 0.005;

            }
            
            //tmpVoltage=dVoltage * dVoltageParameter;
            //tmpVoltage2 = dVoltage2 * dVoltageParameter2;
            //tmpdSpeed = dSpeed * dVelocityParameter;

            if (tmpVoltage >= 52.0)
            {
                bgr.DrawImage(myImageA, 400, 28);
            }
            else if (tmpVoltage >= 50.0)
            {
                bgr.DrawImage(myImageB, 400, 28);
            }
            else if (tmpVoltage >= 48.0)
            {
                bgr.DrawImage(myImageC, 400, 28);
            }
            else if (tmpVoltage >= 46.0)
            {
                bgr.DrawImage(myImageD, 400, 28);
            }
            else if (tmpVoltage >= 44.0)
            {
                bgr.DrawImage(myImageE, 400, 28);
            }
            else if (tmpVoltage < 43.0)
            {
                bgr.DrawImage(myImageF, 400, 28);
            }

            if (tmpVoltage2 >= 52.0)
            {
                bgr.DrawImage(myImageA, 400, 159);
            }
            else if (tmpVoltage2 >= 50.0)
            {
                bgr.DrawImage(myImageB, 400, 159);
            }
            else if (tmpVoltage2 >= 48.0)
            {
                bgr.DrawImage(myImageC, 400, 159);
            }
            else if (tmpVoltage2 >= 46.0)
            {
                bgr.DrawImage(myImageD, 400, 159);
            }
            else if (tmpVoltage2 >= 44.0)
            {
                bgr.DrawImage(myImageE, 400, 159);
            }
            else if (tmpVoltage2 < 43.0)
            {
                bgr.DrawImage(myImageF, 400, 159);
            }
            //ClientWidth = this.ClientSize.Width;
            //ClientHeight = this.ClientSize.Height;

            bgr.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm"), new Font("黑体", 15, FontStyle.Bold), br, 149, 20);
            //bgr.DrawString(DateTime.Now.ToString("HH:mm"), new Font("黑体", 12, FontStyle.Bold), br, 275, 22);
            //g.DrawString(DateTime.Now.ToLongTimeString().ToString(), new Font("黑体", 16, FontStyle.Regular), br, 250, 20);
            bgr.DrawString((tmpVoltage).ToString("f1") + "V", new Font("黑体", 15, FontStyle.Bold), br, 384, 81);

            bgr.DrawString((tmpVoltage2).ToString("f1") + "V", new Font("黑体", 15, FontStyle.Bold), br, 384, 212);

            if (tmpdSpeed < 0)
            {
                tmpdSpeed = 0.0;
            }
            //bgr.DrawString((dSpeed * dVelocityParameter).ToString("f2") + "KM/h", new Font("黑体", 15, FontStyle.Bold), br, 130, 180);
            bgr.DrawString((tmpdSpeed).ToString("f2") + "KM/h", new Font("黑体", 15, FontStyle.Bold), br, 129, 243);

            //bgr.DrawLine(new Pen(Color.Red, 3), 174, 228, Convert.ToInt32(174 + Math.Cos((360 - (180 - (-15 + (tmpdSpeed) * 5.2 / 5 * 180 / 27))) * PI / 180) * 126), Convert.ToInt32(228 + Math.Sin((360 - (180 - (-15 + (tmpdSpeed) * 5.2 / 5 * 180 / 27))) * PI / 180) * 126));
            bgr.DrawLine(new Pen(Color.Red, 3), 176, 226, Convert.ToInt32(176 + Math.Cos((360 - (180 - (-15 + (tmpdSpeed) * 2.6 / 5 * 180 / 27))) * PI / 180) * 126), Convert.ToInt32(226 + Math.Sin((360 - (180 - (-15 + (tmpdSpeed) * 2.6 / 5 * 180 / 27))) * PI / 180) * 126));

            pictureBox2.Image = bmp;

            //Graphics g = this.CreateGraphics();
            //g.DrawImage(bmp, 0, 0);
            //this.Refresh();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            double s_s = 0.0;
            string tmpExp = "1.0";
            RPN rpn = new RPN();
            if (rpn.Parse(tmpExp))
            {
                
                s_s=Convert.ToDouble(rpn.Evaluate());
                System.Diagnostics.Debug.WriteLine(s_s.ToString());
                System.Diagnostics.Debug.WriteLine(rpn.Evaluate());
            }  
        }

    }
}