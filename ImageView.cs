﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BioImage
{
    public partial class ImageView : UserControl
    {
        public ImageView(string file, int ser, bool folder)
        {
            
            file = file.Replace("\\", "/");
            InitializeComponent();
            serie = ser;

            tools = new Tools();
            if (file == "" || file == null)
                return;
            SetCoordinate(ser, 0, 0, 0);

            image = new BioImage(ser, file);
            if (folder)
            {
                Console.WriteLine("Opening folder files. Serie: " + ser + " File: " + file);
                if (image.seriesCount == 0)
                {
                    for (int seri = 0; seri <= image.seriesCount; seri++)
                    {
                        //this is a folder so we load it's images.
                        string name;
                        string fol = System.IO.Path.GetDirectoryName(file);
                        image.OpenFolder(fol, serie, out name, '_');
                    }
                }
            }
            InitGUI(file);

            Buf = image.GetBufByCoord(GetCoordinate());
            if (image.SizeC > 2)
            {
                Mode = ViewMode.RGBImage;
            }
            else
                Mode = ViewMode.Filtered;

            MouseWheel += new System.Windows.Forms.MouseEventHandler(ImageView_MouseWheel);
            zBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(ZTrackBar_MouseWheel);
            cBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(CTrackBar_MouseWheel);
            timeBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(TimeTrackBar_MouseWheel);
            //We set the trackbar event to handled so that it only scrolls one tick not the default multiple.
            zBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            timeBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            cBar.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            TimeFps = 60;
            ZFps = 60;
            CFps = 1;
            UpdateView();
            viewer = this;
            UpdateOverlaySize();

            // Change parent for overlay PictureBox...
            overlayPictureBox.Parent = pictureBox;
            overlayPictureBox.Location = new Point(0, 0);
        }
        ~ImageView()
        {

        }

        public static bool showBounds = true;
        public static bool showText = false;
        public BioImage image;
        public string filepath = "";
        public int serie = 0;
        private BioImage.SZCT coordinate = new BioImage.SZCT(0, 0, 0, 0);
        public static BioImage.SZCT Coordinate = new BioImage.SZCT(0, 0, 0, 0);
        public void SetCoordinate(int s, int z, int c, int t)
        {
            coordinate = new BioImage.SZCT(s, z, c, t);
            Coordinate = coordinate;
        }
        public BioImage.SZCT GetCoordinate()
        {
            return coordinate;
        }

        public int minSizeX = 50;
        public int minSizeY = 20;
        public bool loopZ = true;
        public bool loopT = true;
        public bool loopC = true;
        public enum ViewMode
        {
            Raw,
            Filtered,
            RGBImage
        }
        private ViewMode viewMode = ViewMode.Filtered;
        public ViewMode Mode
        {
            get
            {
                return viewMode;
            }
            set
            {
                viewMode = value;
                if (viewMode == ViewMode.RGBImage)
                {
                    rGBImageModeToolStripMenuItem.Checked = true;
                    rgbBoxsPanel.BringToFront();
                    cBar.SendToBack();
                    cLabel.SendToBack();
                }
                else
                if (viewMode == ViewMode.Filtered)
                {
                    filteredModeToolStripMenuItem.Checked = true;
                    rgbBoxsPanel.SendToBack();
                    cBar.BringToFront();
                    cLabel.BringToFront();
                }
                else
                if (viewMode == ViewMode.Raw)
                {
                    rawModeToolStripMenuItem.Checked = true;
                    rgbBoxsPanel.SendToBack();
                    cBar.BringToFront();
                    cLabel.BringToFront();
                }
            }
        }
        public string Path
        {
            get
            {
                return filepath;
            }
        }
        public BioImage.Buf Buf;
        public BioImage.Channel RChannel
        {
            get
            {
                return image.Channels[image.rgbChannels[0]];
            }
        }
        public BioImage.Channel GChannel
        {
            get
            {
                return image.Channels[image.rgbChannels[1]];
            }
        }
        public BioImage.Channel BChannel
        {
            get
            {
                return image.Channels[image.rgbChannels[2]];
            }
        }

        public Tools tools;

        public void UpdateRGBChannels()
        {
            Buf = image.GetBufByCoord(GetCoordinate());
            if (channelBoxR.SelectedIndex == -1)
                image.rgbChannels[0] = channelBoxR.SelectedIndex;
            if (channelBoxG.SelectedIndex == -1)
                image.rgbChannels[1] = 0;
            else
                image.rgbChannels[1] = channelBoxG.SelectedIndex;
            if (channelBoxB.SelectedIndex == -1)
                image.rgbChannels[2] = 0;
            else
                image.rgbChannels[2] = channelBoxB.SelectedIndex;
        }

        private bool timeEnabled = false;

        private int zfps;
        public int ZFps
        {
            get
            {
                return zfps;
            }
            set
            {
                zfps = value;
                float f = value;
                zTimer.Interval = (int)Math.Floor(1000 / f);
            }
        }

        private int timefps;
        public int TimeFps
        {
            get
            {
                return timefps;
            }
            set
            {
                timefps = value;
                float f = value;
                timelineTimer.Interval = (int)Math.Floor(1000 / f);
            }
        }

        private int cfps;
        public int CFps
        {
            get
            {
                return cfps;
            }
            set
            {
                cfps = value;
                float f = value;
                cTimer.Interval = (int)Math.Floor(1000 / f);
            }
        }

        public void InitGUI(string path)
        {
            filepath = path;
            //image = new BioImage(filePath);
            zBar.Maximum = image.SizeZ - 1;
            cBar.Maximum = image.SizeC - 1;
            if (image.SizeT > 1)
            {
                timeBar.Maximum = image.reader.getSizeT() - 1;
                timeEnabled = true;
            }
            else
            {
                timeBar.Enabled = false;
                timeEnabled = false;
            }
            //rgbPictureBox.Image = image.plane.GetBitmap();
            //we clear the channel comboboxes incase we have channels from previous loaded image.
            channelBoxR.Items.Clear();
            channelBoxG.Items.Clear();
            channelBoxB.Items.Clear();
            foreach (BioImage.Channel ch in image.Channels)
            {
                channelBoxR.Items.Add(ch);
                channelBoxG.Items.Add(ch);
                channelBoxB.Items.Add(ch);
            }
            if (image.Channels.Count > 2)
            {
                channelBoxR.SelectedIndex = 0;
                channelBoxG.SelectedIndex = 1;
                channelBoxB.SelectedIndex = 2;
            }
            else
            if (image.Channels.Count == 2)
            {
                channelBoxR.SelectedIndex = 0;
                channelBoxG.SelectedIndex = 1;
            }
            else
            {
                channelBoxR.SelectedIndex = 0;
            }
            UpdateRGBChannels();
            //We threshold the image so that the max threshold value is the max pixel value in image. 
            image.AutoThreshold();

        }

        public void UpdateView()
        {
            if (Mode == ViewMode.Raw)
            {
                foreach (BioImage.Channel c in image.Channels)
                {
                    c.Min = 0;
                    c.Max = ushort.MaxValue;
                }
                SetCoordinate(image.serie, zBar.Value, cBar.Value, timeBar.Value);
                pictureBox.Image = image.GetBitmap(GetCoordinate());
            }
            else
            if (Mode == ViewMode.Filtered)
            {
                SetCoordinate(image.serie, zBar.Value, cBar.Value, timeBar.Value);
                pictureBox.Image = image.GetFiltered(GetCoordinate(), image.Channels[cBar.Value].range);
            }
            else
            if (Mode == ViewMode.RGBImage)
            {
                SetCoordinate(image.serie, zBar.Value, image.Channels[image.rgbChannels[0]].Index, timeBar.Value);
                UpdateRGBChannels();
                if (image.RGBChannelCount == 1)
                {
                    if (image.bitsPerPixel > 8)
                    {
                        AForge.IntRange rr = RChannel.range;
                        AForge.IntRange gr;
                        AForge.IntRange br;
                        if (image.SizeC == 1)
                        {
                            rr = RChannel.range;
                            pictureBox.Image = image.GetRGBBitmap16(GetCoordinate(), rr);
                        }
                        else
                        {
                            rr = RChannel.range;
                            gr = GChannel.range;
                            br = BChannel.range;
                            pictureBox.Image = image.GetRGBBitmap16(GetCoordinate(), rr, gr, br);
                        }
                    }
                    else
                    {
                        pictureBox.Image = image.GetRGBBitmap8(GetCoordinate());
                    }
                }
                else
                {
                    pictureBox.Image = image.GetBitmap(GetCoordinate());
                }
            }

            UpdateStatus();
        }

        public void UpdateOverlay()
        {
            overlayPictureBox.Update();
        }

        public void UpdateStatus()
        {
            if (Mode == ViewMode.RGBImage)
            {
                //Since combining 3 planes to one image takes time we show the image load time.
                float ms = image.loadTimeMS;
                ticksLabel.Text = " Ticks: " + image.loadTimeTicks;
                if (timeEnabled)
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (zBar.Maximum + 1) + ", " + (timeBar.Value + 1) + "/" + (timeBar.Maximum + 1) + ", " + mouseColor;
                }
                else
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (cBar.Maximum + 1) + ", " + mouseColor;
                }
                
            }
            else
            {
                if (timeEnabled)
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (zBar.Maximum + 1) + ", " + (cBar.Value + 1) + "/" + (cBar.Maximum + 1) + ", " + (timeBar.Value + 1) + "/" + (timeBar.Maximum + 1) + ", " + mouseColor;
                }
                else
                {
                    statusLabel.Text = (zBar.Value + 1) + "/" + (zBar.Maximum + 1) + ", " + (cBar.Value + 1) + "/" + (cBar.Maximum + 1) + ", " + mouseColor;
                }
            }
        }


        private void channelBoxR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (channelBoxR.SelectedIndex == -1)
                return;
            image.rgbChannels[0] = channelBoxR.SelectedIndex;
            //image.SetRGBChannelIndex(BioImage.RGB.Blue, ch.Index);
            //image.rgbimage.BChannel = ch;
            //UpdateRGBChannels();
            UpdateView();
        }

        private void channelBoxG_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (channelBoxG.SelectedIndex == -1)
                return;
            image.rgbChannels[1] = channelBoxG.SelectedIndex;
            //image.Channels[image.rgbChannels[1]] = ch;
            //image.SetRGBChannelIndex(BioImage.RGB.Green, ch.Index);
            //image.rgbimage.GChannel = ch;
            //UpdateRGBChannels();
            UpdateView();
        }

        private void channelBoxB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (channelBoxB.SelectedIndex == -1)
                return;
            image.rgbChannels[2] = channelBoxB.SelectedIndex;
            //image.Channels[image.rgbChannels[2]] = ch;
            //image.SetRGBChannelIndex(BioImage.RGB.Blue, ch.Index);
            //image.rgbimage.BChannel = ch;
            //UpdateRGBChannels();
            UpdateView();
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoSizeToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = false;
            strechToolStripMenuItem.Checked = false;
            normalToolStripMenuItem.Checked = true;
            zoomToolStripMenuItem.Checked = false;
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void strechToolStripMenuItem_Click(object sender, EventArgs e)
        {
            normalToolStripMenuItem.Checked = false;
            autoSizeToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = false;
            strechToolStripMenuItem.Checked = true;
            zoomToolStripMenuItem.Checked = false;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void autoSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            normalToolStripMenuItem.Checked = false;
            autoSizeToolStripMenuItem.Checked = true;
            centerToolStripMenuItem.Checked = false;
            strechToolStripMenuItem.Checked = false;
            zoomToolStripMenuItem.Checked = false;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void centerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            normalToolStripMenuItem.Checked = false;
            autoSizeToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = true;
            strechToolStripMenuItem.Checked = false;
            zoomToolStripMenuItem.Checked = false;
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            normalToolStripMenuItem.Checked = false;
            autoSizeToolStripMenuItem.Checked = false;
            centerToolStripMenuItem.Checked = false;
            strechToolStripMenuItem.Checked = false;
            zoomToolStripMenuItem.Checked = true;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void showControlsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (trackBarPanel.Visible)
            {
                trackBarPanel.Hide();
                pictureBox.Height += trackBarPanel.Height;
                showControlsToolStripMenuItem.Text = "Show Controls";
            }
            else
            {
                trackBarPanel.Show();
                pictureBox.Height -= trackBarPanel.Height;
                showControlsToolStripMenuItem.Text = "Hide Controls";
            }

        }

        private void playZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (playZToolStripMenuItem.Checked)
            {
                //We stop
                playZToolStripMenuItem.Checked = false;
                stopZToolStripMenuItem.Checked = true;
                zTimer.Stop();
            }
            else
            {
                //We start
                playZToolStripMenuItem.Checked = true;
                stopZToolStripMenuItem.Checked = false;
                zTimer.Start();
            }
        }

        private void stopZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (stopZToolStripMenuItem.Checked)
            {
                //We start
                playZToolStripMenuItem.Checked = true;
                stopZToolStripMenuItem.Checked = false;
                zTimer.Start();

            }
            else
            {
                //We stop
                playZToolStripMenuItem.Checked = false;
                stopZToolStripMenuItem.Checked = true;
                zTimer.Stop();
            }
        }

        private void playTimeToolStripMenu_Click(object sender, EventArgs e)
        {
            if (playTimeToolStripMenu.Checked)
            {
                //We stop
                playTimeToolStripMenu.Checked = false;
                stopTimeToolStripMenu.Checked = true;
                timelineTimer.Stop();
            }
            else
            {
                //We start
                playTimeToolStripMenu.Checked = true;
                stopTimeToolStripMenu.Checked = false;
                timelineTimer.Start();
            }
        }

        private void stopTimeToolStripMenu_Click(object sender, EventArgs e)
        {
            playTimeToolStripMenu.Checked = false;
            stopTimeToolStripMenu.Checked = true;
            timelineTimer.Stop();
        }

        private void playCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopCToolStripMenuItem.Checked = true;
            playCToolStripMenuItem.Checked = false;
            cTimer.Stop();
        }

        private void stopCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopCToolStripMenuItem.Checked = false;
            playCToolStripMenuItem.Checked = true;
            cTimer.Stop();
        }

        public string mouseColor = "";
       
        private void autoContrastChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            image.AutoThreshold();
            UpdateView();
        }

        private void playSpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySpeed sp = null;
            if (Mode == ViewMode.RGBImage)
                sp = new PlaySpeed(timeEnabled, false, ZFps, TimeFps, CFps);
            if (Mode == ViewMode.Filtered)
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (Mode == ViewMode.Raw)
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (sp.ShowDialog() != DialogResult.OK)
                return;
            ZFps = sp.ZFps;
            TimeFps = sp.TimeFps;
            CFps = sp.CFps;
        }
        private void playSpeedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PlaySpeed sp = null;
            if (Mode == ViewMode.RGBImage)
                sp = new PlaySpeed(timeEnabled, false, ZFps, TimeFps, CFps);
            if (Mode == ViewMode.Filtered)
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (sp.ShowDialog() != DialogResult.OK)
                return;
            ZFps = sp.ZFps;
            TimeFps = sp.TimeFps;
            CFps = sp.CFps;
        }
        private void CPlaySpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySpeed sp = null;
            if (Mode == ViewMode.RGBImage)
                sp = new PlaySpeed(timeEnabled, false, ZFps, TimeFps, CFps);
            if (Mode == ViewMode.Filtered)
                sp = new PlaySpeed(timeEnabled, true, ZFps, TimeFps, CFps);
            if (sp.ShowDialog() != DialogResult.OK)
                return;
            ZFps = sp.ZFps;
            TimeFps = sp.TimeFps;
            CFps = sp.CFps;
        }
        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            if (Mode == ViewMode.RGBImage)
                Clipboard.SetImage(image.rgbBitmap);
            if (Mode != ViewMode.Plane)
                Clipboard.SetImage(image.planeBitmap);
            */
        }

        private void ImageView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                /*
                if(Mode == ViewMode.RGBImage)   
                    Clipboard.SetImage(image.rgbBitmap);
                if(Mode != ViewMode.Plane)
                    Clipboard.SetImage(image.planeBitmap);
                */
            }
        }

        public void GetRange()
        {
            RangeTool t;
            if (Mode == ViewMode.Filtered)
                t = new RangeTool(timeEnabled, true, zBar.Minimum, zBar.Maximum, timeBar.Minimum, timeBar.Maximum, cBar.Minimum, cBar.Maximum);
            else
                t = new RangeTool(timeEnabled, false, zBar.Minimum, zBar.Maximum, timeBar.Minimum, timeBar.Maximum, cBar.Minimum, cBar.Maximum);
            if (t.ShowDialog() != DialogResult.OK)
                return;
            zBar.Minimum = t.ZMin;
            zBar.Maximum = t.ZMax;
            timeBar.Minimum = t.TimeMin;
            timeBar.Maximum = t.TimeMax;
            cBar.Minimum = t.CMin;
            cBar.Maximum = t.CMax;
        }

        private void setValueRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetRange();
        }

        private void setValueRangeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GetRange();
        }

        private void setCValueRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetRange();
        }

        private void ImageView_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (zBar.Value + 1 <= zBar.Maximum)
                    zBar.Value += 1;
            }
            else
            {
                if (zBar.Value - 1 >= zBar.Minimum)
                    zBar.Value -= 1;
            }

        }
        private void ZTrackBar_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (zBar.Value + 1 <= zBar.Maximum)
                    zBar.Value += 1;
            }
            else
            {
                if (zBar.Value - 1 >= zBar.Minimum)
                    zBar.Value -= 1;
            }

        }
        private void TimeTrackBar_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!timeEnabled)
                return;
            if (e.Delta > 0)
            {
                if (timeBar.Value + 1 <= timeBar.Maximum)
                    timeBar.Value += 1;
            }
            else
            {
                if (timeBar.Value - 1 >= timeBar.Minimum)
                    timeBar.Value -= 1;
            }
        }
        private void CTrackBar_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Mode != ViewMode.Filtered)
                return;
            if (e.Delta > 0)
            {
                if (cBar.Value + 1 <= cBar.Maximum)
                    cBar.Value += 1;
            }
            else
            {
                if (cBar.Value - 1 >= cBar.Minimum)
                    cBar.Value -= 1;
            }
        }

        private void cTimer_Tick(object sender, EventArgs e)
        {
            if (playCToolStripMenuItem.Checked)
            {
                if (cBar.Maximum >= cBar.Value + 1)
                    cBar.Value++;
                else
                {
                    if (loopC)
                        cBar.Value = cBar.Minimum;
                }
            }
        }
        private void zTimer_Tick(object sender, EventArgs e)
        {
            if (playZToolStripMenuItem.Checked)
            {
                if (zBar.Maximum >= zBar.Value + 1)
                    zBar.Value++;
                else
                {
                    if (loopZ)
                        zBar.Value = zBar.Minimum;
                }
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (playTimeToolStripMenu.Checked)
            {
                if (timeBar.Maximum >= timeBar.Value + 1)
                    timeBar.Value++;
                else
                {
                    if (loopT)
                        timeBar.Value = timeBar.Minimum;
                }
            }
        }

        private int zBarCoord = 0;
        private int tBarCoord = 0;
        private int cBarCoord = 0;
        private void zBar_ValueChanged(object sender, EventArgs e)
        {
            if (zBarCoord != zBar.Value)
            {
                UpdateView();
                zBarCoord = zBar.Value;
            }
        }
        private void timeBar_ValueChanged(object sender, EventArgs e)
        {
            if (tBarCoord != timeBar.Value)
            {
                UpdateView();
                tBarCoord = timeBar.Value;
            }
        }
        private void cBar_ValueChanged(object sender, EventArgs e)
        {
            if (cBarCoord != cBar.Value)
            {
                UpdateView();
                cBarCoord = cBar.Value;
            }
        }

        private void loopTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loopT = loopTimeToolStripMenuItem.Checked;
        }
        private void loopZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loopZ = loopZToolStripMenuItem.Checked;
        }
        private void loopCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loopC = loopCToolStripMenuItem.Checked;
        }

        private void rawModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ViewMode.Raw;
            rGBImageModeToolStripMenuItem.Checked = false;
            filteredModeToolStripMenuItem.Checked = false;
        }
        private void filteredModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ViewMode.Filtered;
            rGBImageModeToolStripMenuItem.Checked = false;
            rawModeToolStripMenuItem.Checked = false;
        }
        private void rGBImageModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ViewMode.RGBImage;
            rawModeToolStripMenuItem.Checked = false;
            filteredModeToolStripMenuItem.Checked = false;
        }
        private Point toImagePoint(int px, int py)
        {
            int x, y;
            Point p = new Point(px, py);
            if (pictureBox.SizeMode == PictureBoxSizeMode.StretchImage)
            {
                x = (int)((((float)p.X) / (float)pictureBox.Width) * (float)image.SizeX);
                y = (int)((((float)p.Y) / (float)pictureBox.Height) * (float)image.SizeY);
                return new Point(x, y);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                //First we calculate the scaling factor for image width based on picturebox width
                float fw = (float)pictureBox.Width / (float)image.SizeX;
                //Next we calculate the scaling factor for image height based on picturebox height
                float fh = (float)pictureBox.Height / (float)image.SizeY;
                //Next we calculate the (0,0) origin point of the image in the picturebox.
                float x0 = (pictureBox.Width - (fh * image.SizeX)) / 2;
                if (x0 < 0)
                    x0 = 0;
                float y0 = (pictureBox.Height - (fw * image.SizeY)) / 2;
                if (y0 < 0)
                    y0 = 0;
                float sw = (float)image.SizeX / (float)(pictureBox.Width - (x0 * 2));
                float sh = (float)image.SizeY / (float)(pictureBox.Height - (y0 * 2));
                float xp = (px - x0) * sw;
                float yp = (py - y0) * sh;
                return new Point((int)xp, (int)yp);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.Normal || pictureBox.SizeMode == PictureBoxSizeMode.AutoSize)
            {
                x = p.X;
                y = p.Y;
                if (x > image.SizeX)
                    x = image.SizeX;
                if (y > image.SizeY)
                    y = image.SizeY;
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
                return new Point(x, y);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.CenterImage)
            {
                //We calculate the center position
                float cpx = (float)(pictureBox.Width / 2);
                float cpy = (float)(pictureBox.Height / 2);
                //we calculate position of image X:0 Y:0.
                float x0 = cpx - ((float)image.SizeX / 2);
                float y0 = cpy - ((float)image.SizeY / 2);
                x = p.X - (int)x0;
                y = p.Y - (int)y0;
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
                if (x > image.SizeX)
                    x = image.SizeX;
                if (y > image.SizeY)
                    y = image.SizeY;
                return new Point(x, y);
            }
            return new Point(-1, -1);
        }

        
        private Point GetImagePoint()
        {
            int x, y;
            if (pictureBox.SizeMode == PictureBoxSizeMode.StretchImage)
            {
                x = (int)((((float)0) / (float)pictureBox.Width) * (float)image.SizeX);
                y = (int)((((float)0) / (float)pictureBox.Height) * (float)image.SizeY);
                return new Point(x, y);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                //First we calculate the scaling factor for image width based on picturebox width
                float fw = (float)pictureBox.Width / (float)image.SizeX;
                //Next we calculate the scaling factor for image height based on picturebox height
                float fh = (float)pictureBox.Height / (float)image.SizeY;
                //Next we calculate the (0,0) origin point of the image in the picturebox.
                float x0 = (pictureBox.Width - (fh * image.SizeX)) / 2;
                if (x0 < 0)
                    x0 = 0;
                float y0 = (pictureBox.Height - (fw * image.SizeY)) / 2;
                if (y0 < 0)
                    y0 = 0;
                return new Point((int)x0, (int)y0);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.Normal || pictureBox.SizeMode == PictureBoxSizeMode.AutoSize)
            {
                return new Point(0, 0);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.CenterImage)
            {
                //We calculate the center position
                float cpx = (float)(pictureBox.Width / 2);
                float cpy = (float)(pictureBox.Height / 2);
                //we calculate position of image X:0 Y:0.
                float x0 = cpx - ((float)image.SizeX / 2);
                float y0 = cpy - ((float)image.SizeY / 2);
                x = (int)x0;
                y = (int)y0;
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
                if (x > image.SizeX)
                    x = image.SizeX;
                if (y > image.SizeY)
                    y = image.SizeY;
                return new Point((int)x0, (int)y0);
            }
            return new Point(-1, -1);
        }
        private Point GetImageSize()
        {
            int x, y;
            if (pictureBox.SizeMode == PictureBoxSizeMode.StretchImage)
            {
                return new Point(pictureBox.Width, pictureBox.Height);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                //First we calculate the scaling factor for image width based on picturebox width
                float fw = (float)pictureBox.Width / (float)image.SizeX;
                //Next we calculate the scaling factor for image height based on picturebox height
                float fh = (float)pictureBox.Height / (float)image.SizeY;
                //Next we calculate the (0,0) origin point of the image in the picturebox.
                float x0 = (pictureBox.Width - (fh * image.SizeX)) / 2;
                if (x0 < 0)
                    x0 = 0;
                float y0 = (pictureBox.Height - (fw * image.SizeY)) / 2;
                if (y0 < 0)
                    y0 = 0;
                float sw = (float)image.SizeX / (float)(pictureBox.Width - (x0 * 2));
                float sh = (float)image.SizeY / (float)(pictureBox.Height - (y0 * 2));

                int wi = (int)(pictureBox.Width - (x0 * 2));
                int hi = (int)(pictureBox.Height - (y0 * 2));

                float xp = x0 * sw;
                float yp = y0 * sh;
                return new Point(wi, hi);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.Normal || pictureBox.SizeMode == PictureBoxSizeMode.AutoSize)
            {
                return new Point(image.SizeX, image.SizeY);
            }
            if (pictureBox.SizeMode == PictureBoxSizeMode.CenterImage)
            {
                //We calculate the center position
                float cpx = (float)(pictureBox.Width / 2);
                float cpy = (float)(pictureBox.Height / 2);
                //we calculate position of image X:0 Y:0.
                float x0 = cpx - ((float)image.SizeX / 2);
                float y0 = cpy - ((float)image.SizeY / 2);
                x = (int)(pictureBox.Width - (x0 * 2));
                y = (int)(pictureBox.Height - (y0 * 2));
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
                if (x > image.SizeX)
                    x = image.SizeX;
                if (y > image.SizeY)
                    y = image.SizeY;
                return new Point(x, y);
            }
            return new Point(-1, -1);
        }

        private List<BioImage.Annotation> annotationsR = new List<BioImage.Annotation>();
        public List<BioImage.Annotation> AnnotationsR
        {
            get
            {
                BioImage.SZCT coord = coordinate;
                return image.GetAnnotations(coord.S, coord.Z, image.RChannel.index, coord.T);
            }
        }
        private List<BioImage.Annotation> annotationsG = new List<BioImage.Annotation>();
        public List<BioImage.Annotation> AnnotationsG
        {
            get
            {
                BioImage.SZCT coord = coordinate;
                return image.GetAnnotations(coord.S, coord.Z, image.GChannel.index, coord.T);
            }
        }
        private List<BioImage.Annotation> annotationsB = new List<BioImage.Annotation>();
        public List<BioImage.Annotation> AnnotationsB
        {
            get
            {
                BioImage.SZCT coord = coordinate;
                return image.GetAnnotations(coord.S, coord.Z, image.BChannel.index, coord.T);
            }
        }
        Graphics g = null;

        public void UpdateOverlaySize()
        {
            Point s = GetImageSize();
            if(s.X > 0 && s.Y > 0)
            image.overlay = new Bitmap(s.X, s.Y, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
        private void overlayPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Point pp = GetImagePoint();
            Point s = GetImageSize();
            g = Graphics.FromImage(image.overlay);
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            float xs = (float)s.X / (float)image.SizeX;
            float ys = (float)s.Y / (float)image.SizeY;
            //We use scale transform so that the drawing don't look pixelated when image is rendered larger than it's original size.
            g.ScaleTransform(xs, ys);
            Pen pen = null;
            Brush b = null;
            bool bounds = showBounds;
            bool labels = showText;
            if (Mode == ViewMode.RGBImage)
            {
                foreach (BioImage.Annotation an in AnnotationsR)
                {
                    pen = new Pen(an.strokeColor, (float)an.strokeWidth);
                    if (an.selected)
                    {
                        b = new SolidBrush(Color.Magenta);
                    }
                    else
                        b = new SolidBrush(an.strokeColor);
                    PointF pc = new PointF((float)(an.BoundingBox.X + (an.BoundingBox.W / 2)), (float)(an.BoundingBox.Y + (an.BoundingBox.H / 2)));
                    if (an.type == BioImage.Annotation.Type.Point)
                    {
                        g.DrawLine(pen, an.Point.ToPointF(), new PointF((float)an.Point.X + 1, (float)an.Point.Y + 1));
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Line)
                    {
                        g.DrawLine(pen, an.GetPoint(0).ToPointF(), an.GetPoint(1).ToPointF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Rectangle)
                    {
                        g.DrawRectangle(pen, an.Rect.ToRectangleInt());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Polygon && an.closed)
                    {
                        g.DrawPolygon(pen, an.GetPointsF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Polygon && !an.closed)
                    {
                        PointF[] points = an.GetPointsF();
                        if (points.Length == 1)
                        {
                            g.DrawLine(pen, an.Point.ToPointF(), new PointF((float)an.Point.X + 1, (float)an.Point.Y + 1));
                        }
                        else
                            g.DrawLines(pen, points);
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Polyline)
                    {
                        g.DrawLines(pen, an.GetPointsF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Ellipse)
                    {
                        g.DrawEllipse(pen, an.Rect.ToRectF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Freeform && an.closed)
                    {
                        PointF[] points = an.GetPointsF();
                        if (points.Length == 1)
                        {
                            g.DrawLine(pen, an.Point.ToPointF(), new PointF((float)an.Point.X + 1, (float)an.Point.Y + 1));
                        }
                        else
                        g.DrawPolygon(pen, an.GetPointsF());
                        if (an.selected)
                            g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Freeform && !an.closed)
                    {
                        PointF[] points = an.GetPointsF();
                        if (points.Length == 1)
                        {
                            g.DrawLine(pen, an.Point.ToPointF(), new PointF((float)an.Point.X + 1, (float)an.Point.Y + 1));
                        }
                        else
                            g.DrawLines(pen, points);
                        if (an.selected)
                            g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    if (an.type == BioImage.Annotation.Type.Label)
                    {
                        g.DrawString(an.Text, an.font, b, an.Point.ToPointF());
                    }
                    if (labels)
                        g.DrawString(an.Text, an.font, b, pc);
                    if (bounds)
                        g.DrawRectangle(Pens.Green, new Rectangle((int)an.BoundingBox.X, (int)an.BoundingBox.Y, (int)an.BoundingBox.W, (int)an.BoundingBox.H));
                    if (an.selected)
                        g.DrawRectangle(Pens.Magenta, new Rectangle((int)an.BoundingBox.X, (int)an.BoundingBox.Y, (int)an.BoundingBox.W, (int)an.BoundingBox.H));
                    if (an.selected)
                    {
                        for (int i = 0; i < selectedPoints.Count; i++)
                        {
                            RectangleF r = an.selectBoxs[selectedPoints[i]];
                            g.DrawRectangle(Pens.Blue, new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height));
                        }
                    }
                    pen.Dispose();
                }
            }
            else
            {
                foreach (BioImage.Annotation an in image.GetAnnotations(GetCoordinate()))
                {
                    pen = new Pen(an.strokeColor, (float)an.strokeWidth);
                    b = new SolidBrush(an.strokeColor);
                    PointF pc = new PointF((float)(an.BoundingBox.X + (an.BoundingBox.W / 2)), (float)(an.BoundingBox.Y + (an.BoundingBox.H / 2)));
                    if (an.type == BioImage.Annotation.Type.Point)
                    {
                        g.DrawLine(pen, an.Point.ToPointF(), new PointF((float)an.Point.X + 1, (float)an.Point.Y + 1));
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());

                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Line)
                    {
                        g.DrawLine(pen, an.GetPoint(0).ToPointF(), an.GetPoint(1).ToPointF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Rectangle)
                    {
                        g.DrawRectangle(pen, an.Rect.ToRectangleInt());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Polygon && an.closed)
                    {
                        g.DrawPolygon(pen, an.GetPointsF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Polygon && !an.closed)
                    {
                        PointF[] points = an.GetPointsF();
                        if (points.Length == 1)
                        {
                            g.DrawLine(pen, an.Point.ToPointF(), new PointF((float)an.Point.X + 1, (float)an.Point.Y + 1));
                        }
                        else
                            g.DrawLines(pen, points);
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }

                    if (an.type == BioImage.Annotation.Type.Polyline)
                    {
                        g.DrawLines(pen, an.GetPointsF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Ellipse)
                    {
                        g.DrawEllipse(pen, an.Rect.ToRectF());
                        g.DrawRectangles(Pens.Red, an.selectBoxs.ToArray());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Freeform)
                    {
                        g.DrawPolygon(pen, an.GetPointsF());
                    }
                    else
                    if (an.type == BioImage.Annotation.Type.Label)
                    {
                        g.DrawString(an.Text, an.font, b, an.Point.ToPointF());
                    }
                    if (labels)
                        g.DrawString(an.Text, an.font, b, pc);
                    if (bounds)
                        g.DrawRectangle(Pens.Green, new Rectangle((int)an.BoundingBox.X, (int)an.BoundingBox.Y, (int)an.BoundingBox.W, (int)an.BoundingBox.H));
                    pen.Dispose();
                }
            }

            if(Tools.currentTool.type == Tools.Tool.Type.rectSel)
            {
                g.DrawRectangle(Pens.Magenta, Tools.rectSel.Selection.ToRectangleInt());
            }

            g.Dispose();
            //e.Graphics.DrawRectangle(Pens.Blue, new Rectangle(overlayPictureBox.Location, overlayPictureBox.Size));
            e.Graphics.DrawImage(image.overlay, pp.X, pp.Y, s.X, s.Y);
        }

        private void hideStatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (statusPanel.Visible == true)
            {
                statusPanel.Visible = false;
                showStatusBarToolStripMenuItem.Visible = true;
            }
            else
            {
                statusPanel.Visible = true;
                showStatusBarToolStripMenuItem.Visible = false;
            }
        }

        public static BioImage.Annotation selectedAnnotation = null;
        public static BioImage selectedImage = null;

        public static ImageView viewer = null;
        public static ImageViewer app = null;
        public static List<int> selectedPoints = new List<int>();

        public static PointF mouseDown;
        public static bool down;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            tools.BringToFront();
            if (!Ctrl)
                selectedPoints.Clear();
            viewer = this;
            selectedImage = image;
            mouseDownButtons = e.Button;
            mouseUpButtons = MouseButtons.None;
            PointF p = toImagePoint(e.Location.X, e.Location.Y);
            MouseEventArgs arg = new MouseEventArgs(e.Button, e.Clicks, (int)p.X, (int)p.Y, e.Delta);
            mouseDown = p;
            down = true;
            up = false;
            if (e.Button == MouseButtons.Left)
            {
                foreach (BioImage.Annotation an in image.Annotations)
                {
                    if (an.GetSelectBound().IntersectsWith(p.X,p.Y))
                    {
                        selectedAnnotation = an;
                        an.selected = true;
                        RectangleF r = new RectangleF(p.X, p.Y, 1, 1);
                        for (int i = 0; i < an.selectBoxs.Count; i++)
                        {
                            if (an.selectBoxs[i].IntersectsWith(r))
                            {
                                selectedPoints.Add(i);
                            }
                        }
                    }
                    else
                        an.selected = false;
                }
            }

            if (e.Button == MouseButtons.Right)
            if (Mode == ViewMode.RGBImage)
            {
                int sc = coordinate.S;
                int zc = coordinate.Z;
                int cc = coordinate.C;
                int tc = coordinate.T;
                if (image.RGBChannelCount == 1)
                {
                    int r = image.GetValue(sc, zc, RChannel.index, tc, (int)p.X, (int)p.Y);
                    int g = image.GetValue(sc, zc, GChannel.index, tc, (int)p.X, (int)p.Y);
                    int b = image.GetValue(sc, zc, BChannel.index, tc, (int)p.X, (int)p.Y);
                    mouseColor += "," + r + "," + g + "," + b;
                }
                else
                {
                    int r = image.GetValue(sc, zc, RChannel.index, tc, (int)p.X, (int)p.Y, 0);
                    int g = image.GetValue(sc, zc, GChannel.index, tc, (int)p.X, (int)p.Y, 1);
                    int b = image.GetValue(sc, zc, BChannel.index, tc, (int)p.X, (int)p.Y, 2);
                    mouseColor += "," + r + "," + g + "," + b;
                }
            }
            else
            if (Mode == ViewMode.Filtered)
            {
                int r = Buf.GetValue((int)p.X, (int)p.Y);
                mouseColor += ", " + r;
            }
            else
            if (Mode == ViewMode.Raw)
            {
                int r = Buf.GetValue((int)p.X, (int)p.Y);
                mouseColor += ", " + r;
            }
            tools.ToolDown(this, arg);
        }
        public static PointF mouseUp;
        public static bool up;

        public static bool Ctrl
        {
            get
            {
                int si = (int)Win32.GetAsyncKeyState(Keys.LControlKey);
                if (si == 0)
                    return false;
                else
                    return true;
            }
        }
        private bool x1State = false;
        private bool x2State = false;
        public static MouseButtons mouseUpButtons;
        public static MouseButtons mouseDownButtons;
        private MouseEventArgs arg;
        private Point p;
        private void rgbPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            selectedImage = image;
            p = toImagePoint(e.Location.X, e.Location.Y);
            arg = new MouseEventArgs(mouseDownButtons, 1, (int)p.X, (int)p.Y, e.Delta);
            tools.ToolMove(this, arg);
            mouseColor = "(" + p.X + ", " + p.Y + ")";
            if (Mode != ViewMode.RGBImage)
            {
                if (e.Button == MouseButtons.XButton1 && !x1State)
                {
                    if (cBar.Value < cBar.Maximum)
                        cBar.Value++;
                    x1State = true;
                }
                if (e.Button != MouseButtons.XButton1)
                    x1State = false;

                if (e.Button == MouseButtons.XButton2 && !x2State)
                {
                    if (cBar.Value > cBar.Minimum)
                        cBar.Value--;
                    x2State = true;
                }
                if (e.Button != MouseButtons.XButton2)
                    x2State = false;
            }

            if (Tools.currentTool != null)
            if(Tools.currentTool.type == Tools.Tool.Type.pencil)
            if (Mode == ViewMode.RGBImage)
            {
                int sc = coordinate.S;
                int zc = coordinate.Z;
                int cc = coordinate.C;
                int tc = coordinate.T;
                
                if (e.Button == MouseButtons.Left && Tools.currentTool.toolType == Tools.Tool.ToolType.color)
                {
                    if (Buf.info.RGBChannelsCount > 1)
                    {
                        Tools.Tool tool = Tools.currentTool;
                        if (Tools.rEnabled)
                            Buf.SetValueRGB(p.X, p.Y, 0, tool.Color.R);
                        if (Tools.gEnabled)
                            Buf.SetValueRGB(p.X, p.Y, 1, tool.Color.G);
                        if (Tools.bEnabled)
                            Buf.SetValueRGB(p.X, p.Y, 2, tool.Color.B);
                    }
                    else
                    if (Mode == ViewMode.RGBImage)
                    {
                        if (Tools.rEnabled)
                            image.SetValue(p.X, p.Y, RChannel.index, Tools.pencil.Color.R);
                        if (Tools.gEnabled)
                            image.SetValue(p.X, p.Y, GChannel.index, Tools.pencil.Color.G);
                        if (Tools.bEnabled)
                            image.SetValue(p.X, p.Y, BChannel.index, Tools.pencil.Color.B);
                    }
                    else
                    {
                        image.SetValue(p.X, p.Y, GetCoordinate(), Tools.pencil.Color.R);
                    }
                    UpdateView();
                }
            }
            else
            if (Mode == ViewMode.Filtered)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (image.RGBChannelCount > 1)
                    {
                        Buf.SetValueRGB(p.X, p.Y, cBar.Value, Tools.pencil.Color.R);
                    }
                    else
                    {
                        image.SetValue(p.X, p.Y, GetCoordinate(), Tools.pencil.Color.R);
                    }
                }
            }
            else
            if (Mode == ViewMode.Raw)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (image.RGBChannelCount > 1)
                    {
                        Buf.SetValueRGB(p.X, p.Y, cBar.Value, ushort.MaxValue);
                    }
                    else
                    {
                        image.SetValue(p.X, p.Y, GetCoordinate(), ushort.MaxValue);
                    }
                }
            }
            UpdateStatus();
        }
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            viewer = this;
            selectedImage = image;
            PointF p = toImagePoint(e.Location.X, e.Location.Y);
            mouseUpButtons = e.Button;
            mouseDownButtons = MouseButtons.None;
            MouseEventArgs arg = new MouseEventArgs(e.Button, e.Clicks, (int)p.X, (int)p.Y, e.Delta);
            
            if (selectedAnnotation == null)
                return;
            mouseUp = p;
            up = true;
            if (Tools.currentTool.type == Tools.Tool.Type.pointSel)
            {
                if (selectedPoints.Count > 0)
                {
                    if (selectedAnnotation.type == BioImage.Annotation.Type.Rectangle || selectedAnnotation.type == BioImage.Annotation.Type.Ellipse ||
                        selectedAnnotation.type == BioImage.Annotation.Type.Freeform || selectedAnnotation.type == BioImage.Annotation.Type.Label)
                    {
                        BioImage.RectangleD d = selectedAnnotation.Rect;
                        if (selectedPoints[0] == 0)
                        {
                            double dw = d.X - p.X;
                            double dh = d.Y - p.Y;
                            d.X = p.X;
                            d.Y = p.Y;
                            d.W += dw;
                            d.H += dh;
                        }
                        else
                        if (selectedPoints[0] == 1)
                        {
                            double dw = p.X - (d.W + d.X);
                            double dh = d.Y - p.Y;
                            d.W += dw;
                            d.H += dh;
                            d.Y -= dh;
                        }
                        else
                        if (selectedPoints[0] == 2)
                        {
                            double dw = d.X - p.X;
                            double dh = p.Y - (d.Y + d.H);
                            d.W += dw;
                            d.H += dh;
                            d.X -= dw;
                        }
                        else
                        if (selectedPoints[0] == 3)
                        {
                            double dw = d.X - p.X;
                            double dh = d.Y - p.Y;
                            d.W = p.X - selectedAnnotation.X;
                            d.H = p.Y - selectedAnnotation.Y;
                        }
                        selectedAnnotation.Rect = d;
                    }
                    else
                    {
                        for (int i = 0; i < selectedPoints.Count; i++)
                        {
                            BioImage.PointD pp = new BioImage.PointD(mouseDown.X - mouseUp.X, mouseDown.Y - mouseUp.Y);
                            BioImage.PointD pd = selectedAnnotation.PointsD[selectedPoints[i]];
                            selectedAnnotation.UpdatePoint(new BioImage.PointD(pd.X - pp.X, pd.Y - pp.Y), selectedPoints[i]);
                        }
                    }
                    selectedAnnotation.UpdateBoundingBox();
                    selectedAnnotation.UpdateSelectBoxs();
                    selectedAnnotation = null;
                }
                
            }
            tools.ToolUp(this, arg);
        }
        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            viewer = this;
            selectedImage = image;
            PointF p = toImagePoint(e.Location.X, e.Location.Y);
            MouseEventArgs arg = new MouseEventArgs(e.Button, e.Clicks, (int)p.X, (int)p.Y, e.Delta);
            tools.ToolDown(this, arg);
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            tools.BringToFront();
            tools.TopMost = true;
            
        }

        private void pictureBox_SizeModeChanged(object sender, EventArgs e)
        {
            overlayPictureBox.SizeMode = pictureBox.SizeMode;
            UpdateOverlaySize();
        }
        private void pictureBox_Resize(object sender, EventArgs e)
        {
            UpdateOverlaySize();
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            UpdateOverlaySize();
        }

    }
}
