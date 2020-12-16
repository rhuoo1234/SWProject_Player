using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using QuartzTypeLib;

namespace SWProject
{
    public partial class MainForm : Form
    {
        private const int WM_APP = 0x8000;
        private const int WM_GRAPHNOTIFY = WM_APP + 1;
        private const int EC_COMPLETE = 0x01;
        private const int WS_CHILD = 0x40000000;
        private const int WS_CLIPCHILDREN = 0x2000000;

        private IMediaPosition mediaPosition = null;
        private FilgraphManager filterGraphManager = null;
        private IMediaEvent mediaEvent = null;
        private IMediaEventEx mediaEventEX = null;
        private DirectShowLib.IBasicAudio basicAudio = null;
        private IVideoWindow videoWindow = null;
        private IMediaControl mediaControl = null;
        private MediaStatus mediaStatus = MediaStatus.NONE;

        public MainForm()
        {
            InitializeComponent();
            this.openMenuItem.Click += openMenuItem_Click;
            this.exitMenuItem.Click += exitMenuItem_Click;
            this.playButton.Click += playButton_Click;
            this.pauseButton.Click += pauseButton_Click;
            this.timer.Tick += timer_Tick;
        }

        // 메뉴바 파일 찾기 버튼
        private void openMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "미디어 파일|*.mpg;*.avi;*.wma;*.wmv;*.mov;*.wav;*.mp2;*.mp3|모든 파일|*.*";
            if (DialogResult.OK == openFileDialog.ShowDialog())
            {
                if (this.mediaControl != null)
                {
                    Stop();
                }
                FilterGraph(canvasPanel, openFileDialog.FileName);
                FilterGraph(canvasPanel, openFileDialog.FileName);
                this.Text = "Player - [" + openFileDialog.FileName + "]";
            }
        }

        // 필터그레프
        private void FilterGraph(Control hWin, string filename)
        {
            this.filterGraphManager = new FilgraphManager();
            this.filterGraphManager.RenderFile(filename);
            this.basicAudio = this.filterGraphManager as DirectShowLib.IBasicAudio;

            try
            {
                this.videoWindow = this.filterGraphManager as IVideoWindow;

                this.videoWindow.Owner = (int)this.canvasPanel.Handle;
                this.videoWindow.WindowStyle = WS_CHILD | WS_CLIPCHILDREN;

                this.videoWindow.SetWindowPosition
                (
                    this.canvasPanel.ClientRectangle.Left,
                    this.canvasPanel.ClientRectangle.Top,
                    this.canvasPanel.ClientRectangle.Width,
                    this.canvasPanel.ClientRectangle.Height
                );
            }
            catch (Exception)
            {
                this.videoWindow = null;
            }
            this.mediaEvent = this.filterGraphManager as IMediaEvent;
            this.mediaEventEX = this.filterGraphManager as IMediaEventEx;
            this.mediaEventEX.SetNotifyWindow((int)this.Handle, WM_GRAPHNOTIFY, 0);
            this.mediaPosition = this.filterGraphManager as IMediaPosition;
            this.mediaControl = this.filterGraphManager as IMediaControl;


            basicAudio.put_Volume(this.trackBar1.Value);
            mediaStatus = MediaStatus.READY;
            UpdateToolBar();
            UpdateStatusBar();
        }

        // 닫기
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // 메뉴바 도움말 버튼
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("S/W 프로젝트\nDirectShow를 활용한 간단한 영상 플레이어\n재생 가능한 파일 포맷 : avi, mpeg, wmv");
        }

        // 재생 버튼
        private void playButton_Click(object sender, EventArgs e)
        {
            this.mediaControl.Run();
            this.mediaStatus = MediaStatus.RUNNING;
            UpdateToolBar();
            UpdateStatusBar();
        }

        // 일시정지 버튼
        private void pauseButton_Click(object sender, EventArgs e)
        {
            this.mediaControl.Pause();
            this.mediaStatus = MediaStatus.PAUSED;
            UpdateToolBar();
            UpdateStatusBar();
        }

        // 중단 버튼
        private void stopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }
        // 중단 처리
        private void Stop()
        {
            this.mediaControl.Stop();
            this.mediaPosition.CurrentPosition = 0;
            this.mediaStatus = MediaStatus.STOPPED;

            UpdateToolBar();
            UpdateStatusBar();
        }

        // 상단 버튼 조절
        private void UpdateToolBar()
        {
            switch (this.mediaStatus)
            {
                case MediaStatus.READY:

                    this.playButton.Enabled = true;
                    this.pauseButton.Enabled = false;
                    this.stopButton.Enabled = false;
                    this.trackBar1.Enabled = true;
                    this.trackBar2.Enabled = true;
                    this.OptionToolStripMenuItem.Enabled = true;

                    break;

                case MediaStatus.PAUSED:

                    this.playButton.Enabled = true;
                    this.pauseButton.Enabled = false;
                    this.stopButton.Enabled = true;

                    break;

                case MediaStatus.RUNNING:

                    this.playButton.Enabled = false;
                    this.pauseButton.Enabled = true;
                    this.stopButton.Enabled = true;

                    break;

                case MediaStatus.STOPPED:

                    this.playButton.Enabled = true;
                    this.pauseButton.Enabled = false;
                    this.stopButton.Enabled = false;

                    break;
            }
        }

        // 하단 상태바
        private void UpdateStatusBar()
        {
            switch (this.mediaStatus)
            {
                case MediaStatus.READY: this.messageToolStripStatusLabel.Text = "준비완료"; break;
                case MediaStatus.STOPPED: this.messageToolStripStatusLabel.Text = "중단"; break;
                case MediaStatus.PAUSED: this.messageToolStripStatusLabel.Text = "중지"; break;
                case MediaStatus.RUNNING: this.messageToolStripStatusLabel.Text = "재생중"; break;
            }
            if (this.mediaPosition != null)
            {
                //타이머 처리
                int secondCount = (int)this.mediaPosition.Duration;
                int hourCount = secondCount / 3600;
                int minuteCount = (secondCount - (hourCount * 3600)) / 60;

                secondCount = secondCount - (hourCount * 3600 + minuteCount * 60);

                this.totalTimeToolStripStatusLabel.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", hourCount, minuteCount, secondCount);

                secondCount = (int)this.mediaPosition.CurrentPosition;
                hourCount = secondCount / 3600;
                minuteCount = (secondCount - (hourCount * 3600)) / 60;
                secondCount = secondCount - (hourCount * 3600 + minuteCount * 60);

                this.playTimeToolStripStatusLabel.Text = String.Format("{0:D2}:{1:D2}:{2:D2}", hourCount, minuteCount, secondCount);

                //재생 바 처리
                trackBar2.Value = 10000 * (int)this.mediaPosition.CurrentPosition / (int)this.mediaPosition.Duration;
            }
            else
            {
                this.totalTimeToolStripStatusLabel.Text = "00:00:00";
                this.playTimeToolStripStatusLabel.Text = "00:00:00";
            }
        }

        // 타이머 작동 / 영상 재생 완료
        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.mediaStatus == MediaStatus.RUNNING)
            {
                if (this.mediaPosition.CurrentPosition == this.mediaPosition.Duration) // 영상 재생 끝났을때
                {
                    Stop();
                }
                UpdateStatusBar();
            }
        }

        // 볼륨 조절 트렉바
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.trackBar1.SetRange(-10000, 0);
            basicAudio.put_Volume(this.trackBar1.Value);
            this.messageToolStripStatusLabel.Text = "볼륨 : " + (this.trackBar1.Value + 10000) / 100 + "%";
        }
       
        // 재생 바
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            this.trackBar2.SetRange(0, 10000);
            this.mediaPosition.CurrentPosition = this.mediaPosition.Duration / 10000 * trackBar2.Value;
        }

        // 메뉴바 설정 버튼
        private void OptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            basicAudio.get_Balance(out int a);
            OptionForm of = new OptionForm(mediaPosition.Rate, a) ;
            of.senddt += new OptionForm.senddata(OptionSet);
            of.ShowDialog();
        }

        // 설정 버튼 적용 처리
        public void OptionSet(double rate, int balance)
        {
            this.mediaPosition.Rate = rate;
            basicAudio.put_Balance(balance);
            this.rate.Text = "x " + mediaPosition.Rate;
            basicAudio.get_Balance(out int i);
        }
    }
}