using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWProject
{
    public partial class OptionForm : Form
    {
        public delegate void senddata(double rate, int balance);
        public event senddata senddt;
        public OptionForm(double rate, int balance)
        {
            InitializeComponent();
            switch (rate)
            {
                case 0.5:
                    comboBox1.SelectedIndex = 0;
                    break;
                case 1:
                    comboBox1.SelectedIndex = 1;
                    break;
                case 1.5:
                    comboBox1.SelectedIndex = 2;
                    break;
                case 2:
                    comboBox1.SelectedIndex = 3;
                    break;
            }
            this.trackBar1.Value = balance;
        }


        // 좌우 밸런스 조절 트렉바
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.trackBar1.SetRange(-10000, 10000);
        }


        //설정 적용 버튼
        private void button1_Click(object sender, EventArgs e)
        {
            double rate = 1.0;
            switch (this.comboBox1.SelectedIndex) 
            {
                case 0:
                    rate = 0.5;
                    break;
                case 1:
                    rate = 1.0;
                    break;
                case 2:
                    rate = 1.5;
                    break;
                case 3:
                    rate = 2.0;
                    break;
            }
            senddt(rate, this.trackBar1.Value);
            Close();
        }

        // 설정 초기화 버튼
        private void button2_Click(object sender, EventArgs e)
        {
            this.trackBar1.Value = 0;
            this.comboBox1.SelectedIndex = 1;
        }
    }
}
