using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using ModbusTCP;

namespace CXLaser
{
    public partial class Form1 : Form
    {
        int cnt = 0;
        CALI p;
        private ModbusTCP.Master MBmaster;
        System.Threading.Timer rThread;
        public Form1()
        {
            InitializeComponent();
            double[,] kkk = new double[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            unsafe
            {
                fixed(double* kk= kkk)
                {
                    //  double* kp = kk;
                    for (int i = 0; i < 9; i++)
                        // *(kp++) += 0.5;
                        kk[i] += 0.5;
                }
            }

            font = new Font("Arial", fontsize, FontStyle.Regular, GraphicsUnit.World);
            paint();
            GetData_Click(null, null);
            ModbusConnect();
        }
        private void ModbusConnect()
        {
            try
            {
                // Create new modbus master and add event functions
                MBmaster = new Master("192.168.2.3", 502, true);
                MBmaster.OnResponseData += new ModbusTCP.Master.ResponseData(MBmaster_OnResponseData);
                MBmaster.OnException += new ModbusTCP.Master.ExceptionData(MBmaster_OnException);
                rThread = new System.Threading.Timer(ReadReg, null, 300, 30);
            }
            catch (SystemException error)
            {
                MessageBox.Show(error.Message);
            }
        }
        // ------------------------------------------------------------------------
        // Event for response data
        // ------------------------------------------------------------------------
        private void MBmaster_OnResponseData(ushort ID, byte unit, byte function, byte[] values)
        {
            // ------------------------------------------------------------------
            // Seperate calling threads
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Master.ResponseData(MBmaster_OnResponseData), new object[] { ID, unit, function, values });
                return;
            }
            if (values != null)
            {
                WC.Text = ((values[1] - values[0]) / 10.0).ToString();
                H.Text = ((values[3] - values[2]) / 10.0).ToString();
            }
        }
        // ------------------------------------------------------------------------
        // Modbus TCP slave exception
        // ------------------------------------------------------------------------
        private void MBmaster_OnException(ushort id, byte unit, byte function, byte exception)
        {
            string exc = "Modbus says error: ";
            switch (exception)
            {
                case Master.excIllegalFunction: exc += "Illegal function!"; break;
                case Master.excIllegalDataAdr: exc += "Illegal data adress!"; break;
                case Master.excIllegalDataVal: exc += "Illegal data value!"; break;
                case Master.excSlaveDeviceFailure: exc += "Slave device failure!"; break;
                case Master.excAck: exc += "Acknoledge!"; break;
                case Master.excGatePathUnavailable: exc += "Gateway path unavailbale!"; break;
                case Master.excExceptionTimeout: exc += "Slave timed out!"; break;
                case Master.excExceptionConnectionLost: exc += "Connection is lost!"; break;
                case Master.excExceptionNotConnected: exc += "Not connected!"; break;
            }

            MessageBox.Show(exc, "Modbus slave exception");
        }

        private void ReadReg(object o)
        {
            MBmaster.ReadHoldingRegister(3, 0, 0, 2);
        }
        Pen axis = new Pen(Color.Magenta);
        int margin = 40;
        float labelw = 12f;
        StringFormat sf = new StringFormat();
        float fontsize = 14f,cy=0;
        Font font;
        int scalax = 40;
        int scalay = 80;
        Pen gpen = new Pen(Color.Green);
        Pen rpen = new Pen(Color.Red,2f);
        int MajorTicX,MajorTicY;
        private readonly object _lock = new object();
        private BufferedGraphics bufferedGraphics;
        internal void paint()
        {
            BufferedGraphicsContext context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.pnlCanvas.Width + 1, this.pnlCanvas.Height + 1);
            bufferedGraphics = context.Allocate(this.pnlCanvas.CreateGraphics(), this.pnlCanvas.ClientRectangle);
            Draw();
       }
        private void Draw()
        {
            if (bufferedGraphics.Graphics == null)
                return;
            Graphics g = bufferedGraphics.Graphics;
            //            Graphics g = Graphics.FromHwnd(pictureBox1.Handle);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Rectangle rect = new Rectangle(margin, 0, pnlCanvas.Width - margin, pnlCanvas.Height - margin);
            g.DrawLine(axis, new Point(margin, pnlCanvas.Height - margin), new Point(pnlCanvas.Width, pnlCanvas.Height - margin));

            g.DrawLine(axis, new Point(margin, 0), new Point(margin, pnlCanvas.Height - margin));
            PointF px1, px2;
            /*
            MajorTicX = (pnlCanvas.Width - margin) / scalax;
            for (int i = 0; i <= scalax; i++)
            {
                px1 = new PointF(margin + i * MajorTicX, pnlCanvas.Height - margin);
                if (i % 10 == 0)
                {
                    px2 = new PointF(margin + i * MajorTicX, pnlCanvas.Height - margin + 10);
                    labelw = 100 - 2;
                    Rectangle ticLabelRect = new Rectangle((int)(px1.X - labelw / 2), (int)(px2.Y + 3), (int)labelw, font.Height);
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (SolidBrush brush = new SolidBrush(Color.Green))
                    {
                        g.DrawString(i.ToString(), font, brush, ticLabelRect, sf);
                    }
                }
                else
                    px2 = new PointF(margin + i * MajorTicX, pnlCanvas.Height - margin + 5);
                g.DrawLine(axis, px1, px2);
            }*/
            MajorTicY = (pnlCanvas.Height - margin) / scalay;
            for (int i = 0; i <= scalay; i++)
            {
                px2 = new PointF(margin, pnlCanvas.Height - margin - i * MajorTicY);
                if (i % 10 == 0)
                {
                    px1 = new PointF(margin - 10, pnlCanvas.Height - margin - i * MajorTicY);

                    labelw = 25;
                    Rectangle ticLabelRect = new Rectangle((int)(px1.X - labelw), (int)(px2.Y - font.Height / 2), (int)labelw, font.Height);
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Center;
                    using (SolidBrush brush = new SolidBrush(Color.Green))
                    {
                        g.DrawString(i.ToString(), font, brush, ticLabelRect, sf);
                    }

                }
                else
                    px1 = new PointF(margin - 5, pnlCanvas.Height - margin - i * MajorTicY);

                g.DrawLine(axis, px1, px2);
            }
            cy = pnlCanvas.Height - margin - 40 * MajorTicY;
            g.DrawLine(axis, margin,cy , pnlCanvas.Width,cy);

            int scalax = (pnlCanvas.Width - margin) / MajorTicY;
            for (int i = 0; i <= scalax; i++)
            {
                px1 = new PointF(margin + i * MajorTicY, pnlCanvas.Height - margin);
                if (i % 10 == 0)
                {
                    px2 = new PointF(margin + i * MajorTicY, pnlCanvas.Height - margin + 10);
                    labelw = 100 - 2;
                    Rectangle ticLabelRect = new Rectangle((int)(px1.X - labelw / 2), (int)(px2.Y + 3), (int)labelw, font.Height);
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (SolidBrush brush = new SolidBrush(Color.Green))
                    {
                        g.DrawString(i.ToString(), font, brush, ticLabelRect, sf);
                    }
                }
                else
                    px2 = new PointF(margin + i * MajorTicY, pnlCanvas.Height - margin + 5);
                g.DrawLine(axis, px1, px2);
            }

            if (filters != null)
            {
                g.DrawCurve(rpen, filters);
            }
            bufferedGraphics.Render();
            GC.Collect();
        }
        private void OpenLaser_Click(object sender, EventArgs e)
        {
            UdpClass.set_laser_state(true);
        }
        private void CloseLaser_Click(object sender, EventArgs e)
        {
            UdpClass.set_laser_state(false);
        }
        private void GetData_Click(object sender, EventArgs e)
        {
            Action f = new Action(getData);
            f.BeginInvoke(null, null);
        }
        StringBuilder sb= new StringBuilder();
        int n = 5;
        float[] z = new float[920];
        float[] x = new float[920];
        PointF[] filters;
        int m =3;
        void filter(PointF[] data)
        {
            int c;
            float max, min;
            float bv;
            for(int i=0;i<data.Length;i++)
            {
                float cv = 0;
                int k = 0;
                //中位值平均滤波法
                min = data[i].X;
                max = data[i].X;
                for (int j=-m;j<=m;j++)
                {
                    c = i + j;
                    if(c< data.Length&&c>=0)
                    {
                        cv += data[c].X;
                        if (data[c].X > max)
                            max = data[c].X;
                        if (data[c].X < min)
                            min = data[c].X;
                        k++;
                    }
                }
                filters[i].X = (cv - max - min) / (k - 2);
                filters[i].Y = data[i].Y;
                /*
                 //Mean
                for (int j = -m; j <= m; j++)
                {
                    c = i + j;
                    if (c < data.Length && c >= 0)
                    {
                        cv += data[c].X;
                        k++;
                    }
                }
                filters[i].X = (cv) / (k);
                filters[i].Y = data[i].Y;
                 */
           }
        }
       unsafe void getData()
        {
            float tx, tz,max,min,cv;
            int cc = 0,ci=0;
      //      sorted = new PointF[920 / n];
            filters = new PointF[920 / n];
            int b = UdpClass.rec_init(ref p);
            while (true)
            {
                int c = UdpClass.rec_cam_line_data(x, z, ref p);
                if (c == 1)
                {
                    sb.Clear();
                    lock(this)
                    {
                        fixed (float* px = x, pz = z)
                        {
                            float* ppx = px;
                            float* ppz = pz;
                            for (int i = 0; i < 920; i += n)
                            {
                                if (i < 920 - n)
                                {
                                    float* ppxk = ppx + n;
                                    float* ppzk = ppz + n;
                                    for (int k = i + 5; k < 920; k += n)
                                    {
                                        if (*ppx > *ppxk)
                                        {
                                            tx = *ppx;
                                            *ppx = *ppxk;
                                            *ppxk = tx;
                                            tz = *ppz;
                                            *ppz = *ppzk;
                                            *ppzk = tz;
                                        }
                                        ppxk += n;
                                        ppzk += n;
                                    }
                                }
                                cv = 0;
                                cc = 0;
                                //中位值平均滤波法
                                min = *ppz;
                                max = *ppz;
                                for (int j = -m; j <= m; j++)
                                {
                                    ci = j*n;
                                    if (ci+i < 920 && ci+i >= 0)
                                    {
                                        cv += ppz[ci];
                                        if (ppz[ci] > max)
                                            max = ppz[ci];
                                        if (ppz[ci] < min)
                                            min = ppz[ci];
                                        cc++;
                                    }
                                }
                                cv = (cv - min - max) / (cc - 2); 

                                /*
                                //Mean
                                for (int j = -m; j <= m; j++)
                                {
                                    ci = j * n;
                                    if (ci + i < 920 && ci + i >= 0)
                                    {
                                        cv += ppz[ci];
                                        cc++;
                                    }
                                }
                                cv = cv / cc;*/

                                sb.Append(string.Format("{2}:x = {0} z = {1}\r\n", *ppx, -*ppz, i/n));

                                filters[i / n].X = (float)((*ppx*2 + 30) * MajorTicY + margin);
                                filters[i / n].Y = (float)(pnlCanvas.Height - margin - (-cv + 40) * MajorTicY);
                                ppx += n;
                                ppz += n;
                            }
                        }
                        cnt++;
                        //    sorted = dps.OrderBy(p => p.Y).ToArray();
                        //    filter(sorted);
                        richTextBox1.BeginInvoke(new EventHandler(delegate
                          {
                              try
                              {
                                  richTextBox1.Text = sb.ToString();
                              }
                              catch { }
                          }));
                        pnlCanvas.BeginInvoke(new EventHandler(delegate
                        {
                            Draw();
                        }));
                    }
                    Thread.Sleep(10);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            float[,] pp = new float[5, 2] {{ 8.2f, 6.5f },  { 2.0f, 0f }, { 1.8f, 3.2f }, { 5.3f, 7.4f }, { 6.6f, 9.8f } };
            sortMD(pp, 0);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.Append(string.Format("{0};{1}\r\n", pp[i, 0], pp[i, 1]));
            }
            richTextBox1.Text = sb.ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            rThread.Dispose();
            if (MBmaster != null)
            {
                MBmaster.Dispose();
                MBmaster = null;
            }
            UdpClass.del_socket();
        }
        private void PulsCanvas_SizeChanged(object sender, EventArgs e)
        {
            paint();
        }
        private void sortMD(float[,] data,int index)
        {
            int i, k;
            int r = data.GetLength(0);
            int c = data.GetLength(1);
            float[] t = new float[c];
            for (i = 0; i < r; i++)
                for (k = i + 1; k < r; k++)
                    if (data[i, index] > data[k, index])
                    {
                        for (int j = 0; j < c; j++)
                        {
                            t[j] = data[i, j];
                            data[i, j] = data[k, j];

                        }
                        for (int j = 0; j < c; j++)
                            data[k, j] = t[j];
                    }
        }
    }
}
