﻿using AForge;
using AForge.Imaging.Filters;
using loci.common.services;
using loci.formats;
using loci.formats.services;
using ome.xml.model.primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BitMiracle.LibTiff.Classic;

namespace BioImage
{
    public static class Table
    {
        public static Hashtable hashID = new Hashtable();
        public static Hashtable buffers = new Hashtable();
        public static Hashtable bioimages = new Hashtable();
        public static Hashtable viewers  = new Hashtable();
        public static object GetObj(int hashid)
        {
            return buffers[hashid];
        }
        public static BioImage GetImage(string ids)
        {
            int hash = ids.GetHashCode();
            return (BioImage)bioimages[hash];
        }
        public static BioImage GetImageByHash(int hash, BioImage.SZCT coord)
        {
            return (BioImage)bioimages[hash];
        }
        public static BioImage.Buf GetBufferByHash(int hashid)
        {
            return (BioImage.Buf)buffers[hashid];
        }
        public static BioImage.Buf GetBuffer(string id)
        {
            int hash = id.GetHashCode();
            return (BioImage.Buf)buffers[hash];
        }
        public static string GetStringByHash(int hashid)
        {
            return buffers[hashid].ToString();
        }
        public static string GetStringByID(string id)
        {
            int hash = id.GetHashCode();
            return buffers[hash].ToString();
        }
        private static void AddBuf(int hashid, BioImage.Buf buf)
        {
            try
            {
                buffers.Add(hashid, buf);
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }
        private static int duplicates = 0;
        public static void AddBuffer(BioImage.Buf buf)
        {
            int hash = buf.info.HashID;
            //We check to see if this buffer has already been added.
            if(buffers.ContainsKey(hash))
            {
                buf.info.stringId += "-" + duplicates;
                duplicates++;
            }
            AddBuf(buf.info.HashID, buf);
        }
        private static int duplicatesB = 0;
        public static void AddImage(BioImage im)
        {
            int hash = im.HashID;
            //We check to see if this buffer has already been added.
            if (buffers.ContainsKey(hash))
            {
                im.Filename += "-" + duplicatesB;
                duplicatesB++;
            }
            if (!bioimages.ContainsKey(im.HashID))
            {
                bioimages.Add(im.HashID, im);
            }
        }
        public static void RemoveImage(BioImage im)
        {
            foreach (BioImage.Buf item in im.Buffers)
            {
                if(buffers.Contains(item))
                buffers.Remove(item);
            }
            if(bioimages.Contains(im))
            bioimages.Remove(im);
        }
        public static void RemoveImage(string id)
        {
            if (bioimages.Contains(id))
            {
                bioimages.Remove(id);
                BioImage im = (BioImage)bioimages[id];
                foreach (BioImage.Buf item in im.Buffers)
                {
                    if (buffers.Contains(item))
                        buffers.Remove(item);
                }
            }
        }
        public static void AddViewer(ImageViewer v)
        {
            if(!viewers.ContainsKey(v.Text))
            viewers.Add(v.Text, v);
        }
        public static void RemoveViewer(ImageViewer v)
        {
            viewers.Remove(v.Text);
        }
        public static void RemoveViewer(string name)
        {
            viewers.Remove(name);
        }
        public static void CloseViewer(string name)
        {
            ((ImageViewer)viewers[name]).Close();
        }
        public static ImageViewer GetViewer(string s)
        {
            return (ImageViewer)viewers[s];
        }
    }
    public class BioImage
    {
        public const char sep = '/';
        public int GetHash(int s, int z, int c, int t)
        {
            return CreateHash(filename, s, Coords[s, z, c, t]);
        }
        public int GetHash(SZCT coord)
        {
            int h = CreateHash(filename, coord.S, Coords[coord.S, coord.Z, coord.C, coord.T]);
            return h;
        }

        public static int CreateHash(string filepath, int ser, int index)
        {
            filepath = filepath.Replace("\\", "/");
            string ids = CreateID(filepath, ser, index);
            return ids.GetHashCode();
        }
        public static string CreateID(string filepath, int ser, int index)
        {
            filepath = filepath.Replace("\\", "/");
            string s = filepath + sep + 's' + sep + ser + sep + 'i' + sep + index;
            return s;
        }

        public int[] GetSZCRTCoordInts(int i)
        {
            Buf bf = Buffers[i];
            int[] ints = new int[4];
            ints[0] = bf.serie;
            ints[1] = bf.info.Coordinate.Z;
            ints[2] = bf.info.Coordinate.C;
            ints[3] = bf.info.Coordinate.T;
            return ints;
        }
        public SZCT GetSZCRTCoords(int i)
        {
            return Buffers[i].info.Coordinate;
        }

        public int HashID
        {
            get
            {
                return filename.GetHashCode();
            }
        }
        int[,,,] Coords;
        public Hashtable fileHashTable = new Hashtable();
        public Random random = new Random();
        public ImageReader reader;
        public loci.formats.ImageWriter imageWriter;
        public loci.formats.meta.IMetadata meta;
        public List<Buf> Buffers = new List<Buf>();
        public List<Channel> Channels = new List<Channel>();
        public List<VolumeD> Volumes = new List<VolumeD>();
        public List<Annotation> Annotations = new List<Annotation>();
        public string IdString
        {
            get { return Path.GetFileName(filename); }
            set { filename = value; }
        }
        public int[] rgbChannels = new int[3];
        public int rGBChannelCount = 1;
        public int bitsPerPixel;
        private PixelFormat pixelFormat;
        private string filename;
        public string Filename
        {
            get { return filename; }
            set 
            { 
                filename = value; 
                IdString = Path.GetFileName(Filename);
            }
        }

        public double physicalSizeX = -1;
        public double physicalSizeY = -1;
        public double physicalSizeZ = -1;
        public double stageSizeX = -1;
        public double stageSizeY = -1;
        public double stageSizeZ = -1;

        public BioImage(string file, int SizeX, int SizeY)
        {
            Filename = file;
            rgbChannels[0] = 0;
            rgbChannels[1] = 0;
            rgbChannels[2] = 0;
            rgbBitmap16 = new Bitmap(SizeX, SizeY, PixelFormat.Format48bppRgb);
            rgbBitmap8 = new Bitmap(SizeX, SizeY, PixelFormat.Format24bppRgb);
        }
        public BioImage(BioImage orig, string file, int ser, int zs, int ze, int cs, int ce, int ts, int te)
        {
            Filename = file;
            serie = ser;
            sizeZ = (int)ze-zs;
            sizeC = (int)ce-cs;
            sizeT = (int)te-ts;
            sizeX = orig.SizeX;
            sizeY = orig.SizeY;
            rGBChannelCount = orig.rGBChannelCount;
            rgbChannels[0] = 0;
            rgbChannels[1] = 0;
            rgbChannels[2] = 0;
            bitsPerPixel = orig.bitsPerPixel;

            if (orig.physicalSizeX != -1)
                physicalSizeX = orig.physicalSizeX;
            if (orig.physicalSizeY != -1)
                physicalSizeY = orig.physicalSizeY;
            if (orig.physicalSizeZ != -1)
                physicalSizeZ = orig.physicalSizeZ;
            if (orig.stageSizeX != -1)
                stageSizeX = orig.stageSizeX;
            if (orig.stageSizeY != -1)
                stageSizeY = orig.stageSizeY;
            if (orig.stageSizeX != -1)
                stageSizeZ = orig.stageSizeZ;

            littleEndian = orig.littleEndian;
            seriesCount = orig.seriesCount;
            imagesPerSeries = ImageCount / seriesCount;
            Coords = new int[seriesCount, SizeZ, SizeC, SizeT];
            Filename = file;
            if (RGBChannelCount == 1)
            {
                if (bitsPerPixel > 8)
                    pixelFormat = PixelFormat.Format16bppGrayScale;
                else
                    pixelFormat = PixelFormat.Format8bppIndexed;
            }
            else
            {
                if (bitsPerPixel > 8)
                    pixelFormat = PixelFormat.Format48bppRgb;
                else
                    pixelFormat = PixelFormat.Format24bppRgb;
            }

            fileHashTable.Add(IdString, IdString.GetHashCode());
            int i = 0;
            for (int ci = 0; ci < SizeC; ci++)
            {
                for (int zi = 0; zi < SizeZ; zi++)
                {
                    for (int ti = 0; ti < SizeT; ti++)
                    {
                        Buf copy = Buf.Copy(orig.GetBufByCoord(ser, zs + zi, cs + ci, ts + ti),IdString, i);
                        copy.info.Coordinate.Z = zi;
                        copy.info.Coordinate.C = ci;
                        copy.info.Coordinate.T = ti;
                        Coords[ser, zi, ci, ti] = i;
                        Buffers.Add(copy);
                        Table.AddBuffer(copy);
                        //Lets copy the ROI's from the original image.
                        List<Annotation> anns = orig.GetAnnotations(ser, zs + zi, cs + ci, ts + ti);
                        if(anns.Count > 0)
                        Annotations.AddRange(anns);
                        i++;
                    }
                }
                Channels.Add(orig.Channels[ci].Copy());
            }
            Table.AddImage(this);
            rgbBitmap16 = new Bitmap(SizeX, SizeY, PixelFormat.Format48bppRgb);
            rgbBitmap8 = new Bitmap(SizeX, SizeY, PixelFormat.Format24bppRgb);
        }
        public BioImage(string file,int ser)
        {
            Filename = file;
            serie = ser;
            if (file.EndsWith("ome.tif"))
                OpenSeries(file, ser);
            else
            if (file.EndsWith("tif"))
            {
                Open(file);
            }
            else
            {
                OpenSeries(file, ser);
            }
            rgbChannels[0] = 0;
            rgbChannels[1] = 0;
            rgbChannels[2] = 0;
            rgbBitmap16 = new Bitmap(SizeX, SizeY, PixelFormat.Format48bppRgb);
            rgbBitmap8 = new Bitmap(SizeX, SizeY, PixelFormat.Format24bppRgb);
            Table.AddImage(this);
        }
        public static BioImage MergeChannels(BioImage b, BioImage b2)
        {
            Recorder.AddLine("BioImage.MergeChannels(" + '"' + b.IdString + '"' + "," + '"' + b2.IdString + '"' + ");");
            BioImage res = new BioImage(b2.Filename, b2.SizeX, b2.SizeY);
            res.serie = b2.serie;
            res.sizeZ = b2.SizeZ;
            int cOrig = b2.SizeC;
            res.sizeC = b2.SizeC + b.SizeC;
            res.sizeT = b2.SizeT;
            res.sizeX = b2.SizeX;
            res.sizeY = b2.SizeY;
            res.rGBChannelCount = b2.rGBChannelCount;
            res.bitsPerPixel = b2.bitsPerPixel;
            if (b.physicalSizeX != -1)
                res.physicalSizeX = b2.physicalSizeX;
            if (b.physicalSizeY != -1)
                res.physicalSizeY = b2.physicalSizeY;
            if (b.physicalSizeZ != -1)
                res.physicalSizeZ = b2.physicalSizeZ;
            if (b.stageSizeX != -1)
                res.stageSizeX = b2.stageSizeX;
            if (b.stageSizeY != -1)
                res.stageSizeY = b2.stageSizeY;
            if (b.stageSizeX != -1)
                res.stageSizeZ = b2.stageSizeZ;

            //res.imageCount = res.SizeZ * res.SizeC * res.SizeT;
            res.littleEndian = b2.littleEndian;
            res.seriesCount = b2.seriesCount;

            res.imagesPerSeries = res.ImageCount / res.seriesCount;
            res.Coords = new int[res.seriesCount, res.SizeZ, res.SizeC, res.SizeT];
            res.IdString = Path.GetFileName(b2.filename) + "-1";

            res.pixelFormat = b2.pixelFormat;

            res.fileHashTable.Add(res.IdString, res.IdString.GetHashCode());
            int i = 0;
            int cc = 0;
            for (int ci = 0; ci < res.SizeC; ci++)
            {
                for (int zi = 0; zi < res.SizeZ; zi++)
                {
                    for (int ti = 0; ti < res.SizeT; ti++)
                    {
                        if (ci < cOrig)
                        {
                            //If this channel is part of the image b1 we add planes from it.
                            Buf copy = Buf.Copy(b2.GetBufByCoord(res.serie, zi, ci, ti), res.IdString, i);
                            copy.info.Coordinate.Z = zi;
                            copy.info.Coordinate.C = ci;
                            copy.info.Coordinate.T = ti;
                            res.Coords[b2.serie, zi, ci, ti] = i;
                            res.Buffers.Add(copy);
                            Table.AddBuffer(copy);
                            //Lets copy the ROI's from the original image.
                            List<Annotation> anns = b2.GetAnnotations(res.serie, zi, ci, ti);
                            if (anns.Count > 0)
                                res.Annotations.AddRange(anns);
                        }
                        else
                        {
                            //This plane is not part of b1 so we add the planes from b2 channels.
                            Buf copy = Buf.Copy(b.GetBufByCoord(res.serie, zi, cc, ti), res.IdString, i);
                            copy.info.Coordinate.Z = zi;
                            copy.info.Coordinate.C = ci;
                            copy.info.Coordinate.T = ti;
                            res.Coords[b2.serie, zi, cc, ti] = i;
                            res.Buffers.Add(copy);
                            Table.AddBuffer(copy);
                            //Lets copy the ROI's from the original image.
                            List<Annotation> anns = b.GetAnnotations(res.serie, zi, cc, ti);
                            if (anns.Count > 0)
                                res.Annotations.AddRange(anns);
                        }
                        i++;
                    }
                }
                if (ci < cOrig)
                {
                    res.Channels.Add(b2.Channels[ci].Copy());
                }
                else
                {
                    res.Channels.Add(b2.Channels[cc].Copy());
                    res.Channels[cc+1].Index = ci;
                    cc++;
                }
            }
            Table.AddImage(res);
            return res;
        }
        public static BioImage MergeChannels(string bname, string b2name)
        {
            BioImage b = Table.GetImage(bname);
            BioImage b2 = Table.GetImage(b2name);
            Recorder.AddLine("BioImage.MergeChannels(" + '"' + bname + '"' + "," + '"' + b2name + '"' + ");");
            return MergeChannels(b, b2);
        }
        public void SplitChannels(bool showDialog)
        {
            Recorder.AddLine("BioImage.SplitChannels(" + '"' + IdString + '"' + "," + showDialog + ");");
            for (int c = 0; c < SizeC; c++)
            {
                string id = IdString + "-" + (c + 1).ToString();
                BioImage b = new BioImage(this, id, 0, 0, SizeZ, c, c+1, 0, SizeT);
                ImageViewer iv = new ImageViewer(b);
                if (showDialog)
                    iv.ShowDialog();
                else
                    iv.Show();
            }
        }
        public static void SplitChannels(BioImage bb, bool showDialog)
        {
            Recorder.AddLine("BioImage.SplitChannels(" + '"' + bb.IdString + '"' + "," + showDialog + ");");
            bb.SplitChannels(showDialog);
        }
        public static void SplitChannels(string name, bool showDialog)
        {
            Recorder.AddLine("BioImage.SplitChannels(" + '"' + name + '"' + "," + showDialog + ");");
            SplitChannels(Table.GetImage(name), showDialog);
        }
        public Channel RChannel
        {
            get
            {
                return Channels[rgbChannels[0]];
            }
        }
        public Channel GChannel
        {
            get
            {
                return Channels[rgbChannels[1]];
            }
        }
        public Channel BChannel
        {
            get
            {
                return Channels[rgbChannels[2]];
            }
        }

        public class SZCT
        {
            public int S, Z, C, T;
            public SZCT(int s, int z, int c, int t)
            {
                S = s;
                Z = z;
                C = c;
                T = t;
            }
            public static bool operator ==(SZCT c1, SZCT c2)
            {
                if (c1.S == c2.S && c1.Z == c2.Z && c1.C == c2.C && c1.T == c2.T)
                    return true;
                else
                    return false;
            }
            public static bool operator !=(SZCT c1, SZCT c2)
            {
                if (c1.S != c2.S || c1.Z != c2.Z || c1.C != c2.C || c1.T != c2.T)
                    return false;
                else
                    return true;
            }
            public override string ToString()
            {
                return S + "," + Z + "," + C + "," + T;
            }
        }
        public struct SZCTXY
        {
            public int S, Z, C, T, X, Y;
            public SZCTXY(int s, int z, int c, int t, int x, int y)
            {
                S = s;
                Z = z;
                C = c;
                T = t;
                X = x;
                Y = y;
            }
            public override string ToString()
            {
                return S + "," + Z + "," + C + "," + T + "," + X + "," + Y;
            }

            public static bool operator ==(SZCTXY c1, SZCTXY c2)
            {
                if (c1.S == c2.S && c1.Z == c2.Z && c1.C == c2.C && c1.T == c2.T && c1.X == c2.X && c1.Y == c2.Y)
                    return true;
                else
                    return false;
            }
            public static bool operator !=(SZCTXY c1, SZCTXY c2)
            {
                if (c1.S != c2.S || c1.Z != c2.Z || c1.C != c2.C || c1.T != c2.T || c1.X != c2.X || c1.Y != c2.Y)
                    return false;
                else
                    return true;
            }
        }

        public class ImageJDesc
        {
            public string ImageJ;
            public int images = 0;
            public int channels = 0;
            public int slices = 0;
            public int frames = 0;
            public bool hyperstack;
            public string mode;
            public string unit;
            public double finterval = 0;
            public double spacing = 0;
            public bool loop;
            public double min = 0;
            public double max = 0;

            public ImageJDesc FromImage(BioImage b)
            {
                ImageJ = "";
                images = b.ImageCount;
                channels = b.SizeC;
                slices = b.SizeZ;
                frames = b.SizeT;
                hyperstack = true;
                mode = "grayscale";
                unit = "micron";
                finterval = b.frameInterval;
                spacing = b.physicalSizeZ;
                loop = false;
                /*
                double dmax = double.MinValue;
                double dmin = double.MaxValue;
                foreach (Channel c in b.Channels)
                {
                    if(dmax < c.Max)
                        dmax = c.Max;
                    if(dmin > c.Min)
                        dmin = c.Min;
                }
                min = dmin;
                max = dmax;
                */
                min = b.Channels[0].Min;
                max = b.Channels[0].Max;
                return this;
            }
            public string GetString()
            {
                string s = "";
                s+= "ImageJ=" + ImageJ + "\n";
                s += "images=" + images + "\n";
                s += "channels=" + channels.ToString() + "\n";
                s += "slices=" + slices.ToString() + "\n";
                s += "frames=" + frames.ToString() + "\n";
                s += "hyperstack=" + hyperstack.ToString() + "\n";
                s += "mode=" + mode.ToString() + "\n";
                s += "unit=" + unit.ToString() + "\n";
                s += "finterval=" + finterval.ToString() + "\n";
                s += "spacing=" + spacing.ToString() + "\n";
                s += "loop=" + loop.ToString() + "\n";
                s += "min=" + min.ToString() + "\n";
                s += "max=" + max.ToString() + "\n";
                return s;
            }
            public void SetString(string desc)
            {
                string[] lines = desc.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] sp = lines[i].Split('=');
                    if (sp[0] == "ImageJ")
                        ImageJ = sp[1];
                    if (sp[0] == "images")
                        images = int.Parse(sp[1]);
                    if (sp[0] == "channels")
                        channels = int.Parse(sp[1]);
                    if (sp[0] == "slices")
                        slices = int.Parse(sp[1]);
                    if (sp[0] == "frames")
                        frames = int.Parse(sp[1]);
                    if (sp[0] == "hyperstack")
                        hyperstack = bool.Parse(sp[1]);
                    if (sp[0] == "mode")
                        mode = sp[1];
                    if (sp[0] == "unit")
                        unit = sp[1];
                    if (sp[0] == "finterval")
                        finterval = double.Parse(sp[1]);
                    if (sp[0] == "spacing")
                        spacing = double.Parse(sp[1]);
                    if (sp[0] == "loop")
                        loop = bool.Parse(sp[1]);
                    if (sp[0] == "min")
                        min = double.Parse(sp[1]);
                    if (sp[0] == "max")
                        max = double.Parse(sp[1]);
                }
            }
        }

        public int serie = 0;
        public int ImageCount
        {
            get
            {
                return SizeZ * SizeC * SizeT;
            }
        }
        public int imagesPerSeries = 0;
        public int seriesCount = 1;
        public double frameInterval = 0;
        public bool littleEndian = false;
        public bool isGroup = false;
        public long loadTimeMS = 0;
        public long loadTimeTicks = 0;
        private int sizeX, sizeY, sizeZ, sizeC, sizeT;
        public int SizeX
        {
            get { return sizeX; }
            set
            {
                sizeX = value;
            }
        }
        public int SizeY
        {
            get { return sizeY; }
            set
            {
                sizeY = value;
            }
        }
        public int SizeZ
        {
            get { return sizeZ; }
        }
        public int SizeC
        {
            get { return sizeC; }
        }
        public int SizeT
        {
            get { return sizeT; }
        }

        public Stopwatch watch = new Stopwatch();
        public int RGBChannelCount
        {
            get
            {
                return rGBChannelCount;
            }
        }
        public PixelFormat PixelFormat
        {
            get
            {
                return pixelFormat;
            }
        }
        public bool isRGB
        {
            get
            {
                if (rGBChannelCount == 3)
                    return true;
                else
                    return false;
            }
        }
        public bool isTime
        {
            get
            {
                if (SizeT > 1)
                    return true;
                else
                    return false;
            }
        }

        public static LevelsLinear filter8 = new LevelsLinear();
        public static LevelsLinear16bpp filter16 = new LevelsLinear16bpp();
        private ReplaceChannel replaceRFilter;
        private ReplaceChannel replaceGFilter;
        private ReplaceChannel replaceBFilter;
        private static ExtractChannel extractR = new ExtractChannel(AForge.Imaging.RGB.R);
        private static ExtractChannel extractG = new ExtractChannel(AForge.Imaging.RGB.G);
        private static ExtractChannel extractB = new ExtractChannel(AForge.Imaging.RGB.B);

        public ushort GetValue(SZCTXY coord)
        {
            return GetBufByCoord(coord.S, coord.Z, coord.C, coord.T).GetValue(coord.X, coord.Y);
        }
        public ushort GetValue(int s, int z, int c, int t, int x, int y)
        {
            return GetBufByCoord(s, z, c, t).GetValue(x, y);
        }
        public ushort GetValue(SZCT coord, int x, int y)
        {
            return GetBufByCoord(coord.S, coord.Z, coord.C, coord.T).GetValue(x, y);
        }
        public ushort GetValueRGB(SZCT coord, int x, int y, int RGBindex)
        {
            return GetBufByCoord(coord.S, coord.Z, coord.C, coord.T).GetValueRGB(x, y, RGBindex);
        }
        public ushort GetValueRGB(int s, int z, int c, int t, int x, int y, int RGBindex)
        {
            return GetBufByCoord(s, z, c, t).GetValueRGB(x, y, RGBindex);
        }

        public void SetValue(SZCTXY coord, ushort val)
        {
            GetBufByCoord(coord.S, coord.Z, coord.C, coord.T).SetValue(coord.X, coord.Y, val);
        }
        public void SetValue(int ix, int iy, int ind, ushort value)
        {
            Buffers[ind].SetValue(ix, iy, value);
        }
        public void SetValue(int ix, int iy, SZCT coord, ushort value)
        {
            int ind = Coords[coord.S, coord.Z, coord.C, coord.T];
            Buffers[ind].SetValue(ix, iy, value);
        }
        public void SetValueRGB(int s, int z, int c, int t, int x, int y, int RGBindex, ushort value)
        {
            GetBufByCoord(s, z, c, t).SetValueRGB(x, y, RGBindex, value);
        }

        public Buf GetBufByCoord(int s, int z, int c, int t)
        {
            return Buffers[Coords[s, z, c, t]];
        }
        public Buf GetBufByCoord(SZCT coord)
        {
            return Buffers[Coords[coord.S, coord.Z, coord.C, coord.T]];
        }

        public Bitmap GetImageRGB(SZCT coord, IntRange rr, IntRange rg, IntRange rb)
        {
            if(bitsPerPixel > 8)
            {
                return AForge.Imaging.Image.Convert16bppTo8bpp(GetRGBBitmap16(coord, rr, rg, rb));
            }
            else
            {
                return GetRGBBitmap8(coord);
            }
        }
        public Bitmap GetImageRGB(SZCT coord)
        {
            return GetRGBBitmap8(coord);
        }
        public Bitmap GetImageFiltered(SZCT coord, IntRange rr)
        {
            if (bitsPerPixel > 8)
            {
                Bitmap b = GetFiltered(coord, rr);
                Bitmap bb = AForge.Imaging.Image.Convert16bppTo8bpp(b);
                b.Dispose();
                return bb;
            }
            else
            {
                return GetBitmap8(coord);
            }
        }
        public Bitmap GetImageRaw(SZCT coord)
        {
            if (bitsPerPixel > 8)
            {
                return AForge.Imaging.Image.Convert16bppTo8bpp(GetBitmap(coord));
            }
            else
            {
                return GetBitmap8(coord);
            }
        }

        public Bitmap GetBitmap(SZCT coord)
        {
            return GetBufByCoord(coord.S, coord.Z, coord.C, coord.T).GetBitmap();
        }
        public Bitmap GetFiltered(SZCT coord, IntRange filt)
        {
            return GetBufByCoord(coord.S, coord.Z, coord.C, coord.T).GetFiltered(filt);
        }

        private Bitmap rgbBitmap16 = null;
        public Bitmap GetRGBBitmap16(SZCT coord, IntRange rf, IntRange gf, IntRange bf)
        {
            watch.Restart();
            int ri = Coords[serie, coord.Z, coord.C, coord.T];
            if(replaceRFilter == null || replaceGFilter == null || replaceBFilter == null)
            {
                replaceRFilter = new ReplaceChannel(AForge.Imaging.RGB.R, Buffers[ri + RChannel.Index].GetFiltered(rf, gf, bf));
                replaceGFilter = new ReplaceChannel(AForge.Imaging.RGB.G, Buffers[ri + GChannel.Index].GetFiltered(rf, gf, bf));
                replaceBFilter = new ReplaceChannel(AForge.Imaging.RGB.B, Buffers[ri + BChannel.Index].GetFiltered(rf, gf, bf));
            }

            if (RGBChannelCount == 1)
            {
                replaceRFilter.ChannelImage.Dispose();
                replaceRFilter.ChannelImage = Buffers[ri + RChannel.Index].GetFiltered(rf, gf, bf);
                replaceRFilter.ApplyInPlace(rgbBitmap16);

                replaceGFilter.ChannelImage.Dispose();
                replaceGFilter.ChannelImage = Buffers[ri + GChannel.Index].GetFiltered(rf, gf, bf);
                replaceGFilter.ApplyInPlace(rgbBitmap16);

                replaceBFilter.ChannelImage.Dispose();
                replaceBFilter.ChannelImage = Buffers[ri + BChannel.Index].GetFiltered(rf, gf, bf);
                replaceBFilter.ApplyInPlace(rgbBitmap16);
            }
            else
            {
                rgbBitmap16 = Buffers[ri].GetBitmap();
            }
            watch.Stop();
            loadTimeMS = watch.ElapsedMilliseconds;
            loadTimeTicks = watch.ElapsedTicks;
            return rgbBitmap16;
        }

        private Bitmap rgbBitmap8 = null;
        public Bitmap GetRGBBitmap8(SZCT coord)
        {
            watch.Restart();
            int ri = Coords[serie, coord.Z, coord.C, coord.T];
            
            if (RGBChannelCount == 1)
            {
                if (replaceRFilter == null || replaceGFilter == null || replaceBFilter == null)
                {
                    replaceRFilter = new ReplaceChannel(AForge.Imaging.RGB.R, Buffers[ri + RChannel.Index].GetBitmap());
                    replaceGFilter = new ReplaceChannel(AForge.Imaging.RGB.G, Buffers[ri + GChannel.Index].GetBitmap());
                    replaceBFilter = new ReplaceChannel(AForge.Imaging.RGB.B, Buffers[ri + BChannel.Index].GetBitmap());
                }
                replaceRFilter.ChannelImage.Dispose();
                replaceRFilter.ChannelImage = Buffers[ri + RChannel.Index].GetBitmap();
                replaceRFilter.ApplyInPlace(rgbBitmap8);

                replaceGFilter.ChannelImage.Dispose();
                replaceGFilter.ChannelImage = Buffers[ri + GChannel.Index].GetBitmap();
                replaceGFilter.ApplyInPlace(rgbBitmap8);

                replaceBFilter.ChannelImage.Dispose();
                replaceBFilter.ChannelImage = Buffers[ri + BChannel.Index].GetBitmap();
                replaceBFilter.ApplyInPlace(rgbBitmap8);
            }
            else
            {
                rgbBitmap8 = Buffers[ri].GetBitmap();
            }
            watch.Stop();
            loadTimeMS = watch.ElapsedMilliseconds;
            loadTimeTicks = watch.ElapsedTicks;
            return rgbBitmap8;
        }
        public Bitmap GetBitmap8(SZCT coord)
        {
            watch.Restart();
            int ri = Coords[serie, coord.Z, coord.C, coord.T];
            
            if (RGBChannelCount == 1)
            {
                if (replaceRFilter == null || replaceGFilter == null || replaceBFilter == null)
                {
                    replaceRFilter = new ReplaceChannel(AForge.Imaging.RGB.R, Buffers[ri].GetBitmap());
                    replaceGFilter = new ReplaceChannel(AForge.Imaging.RGB.G, Buffers[ri].GetBitmap());
                    replaceBFilter = new ReplaceChannel(AForge.Imaging.RGB.B, Buffers[ri].GetBitmap());
                }
                replaceRFilter.ChannelImage.Dispose();
                replaceRFilter.ChannelImage = Buffers[ri].GetBitmap();
                replaceRFilter.ApplyInPlace(rgbBitmap8);

                replaceGFilter.ChannelImage.Dispose();
                replaceGFilter.ChannelImage = Buffers[ri].GetBitmap();
                replaceGFilter.ApplyInPlace(rgbBitmap8);

                replaceBFilter.ChannelImage.Dispose();
                replaceBFilter.ChannelImage = Buffers[ri].GetBitmap();
                replaceBFilter.ApplyInPlace(rgbBitmap8);
            }
            else
            {
                rgbBitmap8 = Buffers[ri].GetBitmap();
            }
            watch.Stop();
            loadTimeMS = watch.ElapsedMilliseconds;
            loadTimeTicks = watch.ElapsedTicks;
            return rgbBitmap8;
        }

        public static Stopwatch swatch = new Stopwatch();
        public enum RGB
        {
            R,
            G,
            B
        }
        public class ColorS
        {
            public ushort R = 0;
            public ushort G = 0;
            public ushort B = 0;
            public ColorS()
            {

            }
            public ColorS(ushort s)
            {
                R = s;
                G = s;
                B = s;
            }
            public ColorS(ushort r, ushort g, ushort b)
            {
                R = r;
                G = g;
                B = b;
            }
            public static ColorS FromColor(System.Drawing.Color col)
            {
                float r = (((float)col.R) / 255) * ushort.MaxValue;
                float g = (((float)col.G) / 255) * ushort.MaxValue;
                float b = (((float)col.B) / 255) * ushort.MaxValue;
                ColorS color = new ColorS();
                color.R = (ushort)r;
                color.G = (ushort)g;
                color.B = (ushort)b;
                return color;
            }
            public static System.Drawing.Color ToColor(ColorS col)
            {
                float r = ((float)(col.R) / 65535) * 255;
                float g = ((float)(col.G) / 65535) * 255;
                float b = ((float)(col.B) / 65535) * 255;
                System.Drawing.Color c = System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
                return c;
            }
            public override string ToString()
            {
                return R + "," + G + "," + B;
            }
        }
        public struct PointD
        {
            public double X;
            public double Y;
            public PointD(double x, double y)
            {
                X = x;
                Y = y;
            }
            public PointF ToPointF()
            {
                return new PointF((float)X, (float)Y);
            }
            public System.Drawing.Point ToPointInt()
            {
                return new System.Drawing.Point((int)X, (int)Y);
            }

            public override string ToString()
            {
                return X.ToString() + ", " + Y.ToString();
            }
        }
        public struct RectangleD
        {
            public double X;
            public double Y;
            public double W;
            public double H;

            public RectangleD(double x, double y, double w, double h)
            {
                X = x;
                Y = y;
                W = w;
                H = h;
            }
            public System.Drawing.Rectangle ToRectangleInt()
            {
                return new System.Drawing.Rectangle((int)X, (int)Y, (int)W, (int)H);
            }
            public bool IntersectsWith(PointD p)
            {
                if (X <= p.X && (X + W) >= p.X && Y <= p.Y && (Y + H) >= p.Y)
                    return true;
                else
                    return false;
            }
            public bool IntersectsWith(double x, double y)
            {
                if (X <= x && (X + W) >= x && Y <= y && (Y + H) >= y)
                    return true;
                else
                    return false;
            }
            public RectangleF ToRectangleF()
            {
                return new RectangleF((float)X, (float)Y, (float)W, (float)H);
            }
            public override string ToString()
            {
                return X.ToString() + ", " + Y.ToString() + ", " + W.ToString() + ", " + H.ToString();
            }
        }
        public List<Annotation> GetAnnotations(SZCT coord)
        {
            List<Annotation> annotations = new List<Annotation>();
            foreach (Annotation an in Annotations)
            {
                if (an.coord == coord)
                    annotations.Add(an);
            }
            return annotations;
        }
        public List<Annotation> GetAnnotations(int S, int Z, int C, int T)
        {
            List<Annotation> annotations = new List<Annotation>();
            foreach (Annotation an in Annotations)
            {
                if (an.coord.S == S && an.coord.Z == Z && an.coord.Z == Z && an.coord.C == C && an.coord.T == T)
                    annotations.Add(an);
            }
            return annotations;
        }
        public class Annotation
        {
            public float selectBoxSize = 4;
            public enum Type
            {
                Rectangle,
                Point,
                Line,
                Polygon,
                Polyline,
                Freeform,
                Ellipse,
                Label
            }
            public Type type;
            public PointD Point
            {
                get
                {
                    if (type == Type.Line || type == Type.Ellipse || type == Type.Label || type == Type.Freeform)
                        return new PointD(BoundingBox.X, BoundingBox.Y);
                    return Points[0];
                }
                set
                {
                    if (Points.Count == 0)
                    {
                        AddPoint(value);
                    }
                    else
                        UpdatePoint(value, 0);
                    UpdateSelectBoxs();
                    UpdateBoundingBox();
                }
            }
            public RectangleD Rect
            {
                get
                {
                    if (type == Type.Line || type == Type.Polyline || type == Type.Polygon || type == Type.Freeform || type == Type.Label)
                        return BoundingBox;
                    if (type == Type.Rectangle || type == Type.Ellipse)
                        return new RectangleD(Points[0].X, Points[0].Y, Points[1].X - Points[0].X, Points[2].Y - Points[0].Y);
                    else
                        return new RectangleD(Points[0].X, Points[0].Y, 1, 1);
                }
                set
                {
                    if (type == Type.Line || type == Type.Polyline || type == Type.Polygon || type == Type.Freeform)
                    {
                        BoundingBox = value;
                    }
                    else
                    if (Points.Count < 4 && (type == Type.Rectangle || type == Type.Ellipse))
                    {
                        AddPoint(new PointD(value.X, value.Y));
                        AddPoint(new PointD(value.X + value.W, value.Y));
                        AddPoint(new PointD(value.X, value.Y + value.H));
                        AddPoint(new PointD(value.X + value.W, value.Y + value.H));
                    }
                    else
                    if (type == Type.Rectangle || type == Type.Ellipse)
                    {
                        Points[0] = new PointD(value.X, value.Y);
                        Points[1] = new PointD(value.X + value.W, value.Y);
                        Points[2] = new PointD(value.X, value.Y + value.H);
                        Points[3] = new PointD(value.X + value.W, value.Y + value.H);
                    }
                    UpdateSelectBoxs();
                    UpdateBoundingBox();
                }
            }
            public double X
            {
                get
                {
                    return Point.X;
                }
                set
                {
                    Rect = new RectangleD(value, Y, W, H);
                }
            }
            public double Y
            {
                get
                {
                    return Point.Y;
                }
                set
                {
                    Rect = new RectangleD(X, value, W, H);
                }
            }
            public double W
            {
                get
                {
                    if (type == Type.Point)
                        return strokeWidth;
                    else
                        return BoundingBox.W;
                }
                set
                {
                    Rect = new RectangleD(X, Y, value, H);
                }
            }
            public double H
            {
                get
                {
                    if (type == Type.Point)
                        return strokeWidth;
                    else
                        return BoundingBox.H;
                }
                set
                {
                    Rect = new RectangleD(X, Y, W, value);
                }
            }

            private List<PointD> Points = new List<PointD>();
            public List<PointD> PointsD
            {
                get
                {
                    return Points;
                }
            }
            public List<RectangleF> selectBoxs = new List<RectangleF>();
            public List<int> selectedPoints = new List<int>();
            public RectangleD BoundingBox;
            public Font font = System.Drawing.SystemFonts.DefaultFont;
            public SZCT coord;
            public System.Drawing.Color strokeColor;
            public System.Drawing.Color fillColor;
            public bool isFilled = false;
            public string id = "";
            public string roiID = "";
            public string roiName = "";
            private string text = "";
            public string Text
            {
                get
                {
                    return text;
                }
                set
                {
                    text = value;
                    if(type == Type.Label)
                    {
                        UpdateBoundingBox();
                        UpdateSelectBoxs();
                    }
                }
            }
            public double strokeWidth = 1;
            public int shapeIndex = 0;
            public bool closed = false;
            public bool selected = false;
            
            public RectangleD GetSelectBound()
            {
                double f = selectBoxSize / 2;
                return new RectangleD(BoundingBox.X - f, BoundingBox.Y - f, BoundingBox.W + f, BoundingBox.H + f);
            }
            public Annotation()
            {
                coord = new SZCT(0, 0, 0, 0);
                strokeColor = System.Drawing.Color.Yellow;
                font = SystemFonts.DefaultFont;
                BoundingBox = new RectangleD(0, 0, 1, 1);
            }

            public static Annotation CreatePoint(SZCT coord, double x, double y)
            {
                Annotation an = new Annotation();
                an.coord = coord;
                an.AddPoint(new PointD(x, y));
                an.type = Type.Point;
                return an;
            }
            public static Annotation CreateLine(SZCT coord, PointD x1, PointD x2)
            {
                Annotation an = new Annotation();
                an.coord = coord;
                an.type = Type.Line;
                an.AddPoint(x1);
                an.AddPoint(x2);
                return an;
            }
            public static Annotation CreateRectangle(SZCT coord, double x, double y, double w, double h)
            {
                Annotation an = new Annotation();
                an.coord = coord;
                an.type = Type.Rectangle;
                an.Rect = new RectangleD(x,y, w, h);
                return an;
            }
            public static Annotation CreateEllipse(SZCT coord, double x, double y, double w, double h)
            {
                Annotation an = new Annotation();
                an.coord = coord;
                an.type = Type.Ellipse;
                an.Rect = new RectangleD(x, y, w, h);
                return an;
            }
            public static Annotation CreatePolygon(SZCT coord, PointD[] pts)
            {
                Annotation an = new Annotation();
                an.coord = coord;
                an.type = Type.Polygon;
                an.AddPoints(pts);
                an.closed = true;
                return an;
            }
            public static Annotation CreateFreeform(SZCT coord, PointD[] pts)
            {
                Annotation an = new Annotation();
                an.coord = coord;
                an.type = Type.Freeform;
                an.AddPoints(pts);
                an.closed = true;
                return an;
            }

            public void UpdatePoint(PointD p, int i)
            {
                if (i < Points.Count)
                {
                    Points[i] = p;
                }
                UpdateBoundingBox();
                UpdateSelectBoxs();
            }
            public PointD GetPoint(int i)
            {
                return Points[i];
            }
            public PointD[] GetPoints()
            {
                return Points.ToArray();
            }
            public PointF[] GetPointsF()
            {
                PointF[] pfs = new PointF[Points.Count];
                for (int i = 0; i < Points.Count; i++)
                {
                    pfs[i].X = (float)Points[i].X;
                    pfs[i].Y = (float)Points[i].Y;
                }
                return pfs;
            }
            public void AddPoint(PointD p)
            {
                Points.Add(p);
                UpdateSelectBoxs();
                UpdateBoundingBox();
            }
            public void AddPoints(PointD[] p)
            {
                Points.AddRange(p);
                UpdateSelectBoxs();
                UpdateBoundingBox();
            }
            public void RemovePoints(int[] indexs)
            {
                List<PointD> inds = new List<PointD>();
                for (int i = 0; i < Points.Count; i++)
                {
                    bool found = false;
                    for (int ind = 0; ind < indexs.Length; ind++)
                    {
                        if (indexs[ind] == i)
                            found = true;
                    }
                    if (!found)
                        inds.Add(Points[i]);
                }
                Points = inds;
                UpdateBoundingBox();
                UpdateSelectBoxs();
            }
            public int GetPointCount()
            {
                return Points.Count;
            }
            public PointD[] stringToPoints(string s)
            {
                List<PointD> pts = new List<PointD>();
                string[] ints = s.Split(' ');
                for (int i = 0; i < ints.Length; i++)
                {
                    string[] sints = ints[i].Split(',');
                    int x = int.Parse(sints[0]);
                    int y = int.Parse(sints[1]);
                    pts.Add(new PointD(x, y));
                }
                return pts.ToArray();
            }
            public string PointsToString()
            {
                string pts = "";
                for (int j = 0; j < Points.Count; j++)
                {
                    if (j == Points.Count - 1)
                        pts += Points[j].X.ToString() + "," + Points[j].Y.ToString();
                    else
                        pts += Points[j].X.ToString() + "," + Points[j].Y.ToString() + " ";
                }
                return pts;
            }
            public string PointsToString(PointD[] Points)
            {
                string pts = "";
                for (int j = 0; j < Points.Length; j++)
                {
                    if (j == Points.Length - 1)
                        pts += Points[j].X.ToString() + "," + Points[j].Y.ToString();
                    else
                        pts += Points[j].X.ToString() + "," + Points[j].Y.ToString() + " ";
                }
                return pts;
            }
            public void UpdateSelectBoxs()
            {
                float f = selectBoxSize / 2;
                selectBoxs.Clear();
                if(type == Type.Label)
                {
                    selectBoxs.Add(new RectangleF((float)Points[0].X - f, (float)Points[0].Y - f, selectBoxSize, selectBoxSize));
                }
                else
                for (int i = 0; i < Points.Count; i++)
                {
                    selectBoxs.Add(new RectangleF((float)Points[i].X - f, (float)Points[i].Y - f, selectBoxSize, selectBoxSize));
                }
            }
            public void UpdateBoundingBox()
            {
                if (type == Type.Label)
                {
                    if (text != "")
                    {
                        Size s = TextRenderer.MeasureText(text, font);
                        BoundingBox = new RectangleD(Points[0].X, Points[0].Y, s.Width, s.Height);
                    }
                }
                else
                {
                    RectangleD r = new RectangleD(float.MaxValue, float.MaxValue, 0, 0);
                    foreach (PointD p in Points)
                    {
                        if (r.X > p.X)
                            r.X = p.X;
                        if (r.Y > p.Y)
                            r.Y = p.Y;
                        if (r.W < p.X)
                            r.W = p.X;
                        if (r.H < p.Y)
                            r.H = p.Y;
                    }
                    r.W = r.W - r.X;
                    r.H = r.H - r.Y;
                    if (r.W == 0)
                        r.W = 1;
                    if (r.H == 0)
                        r.H = 1;
                    BoundingBox = r;
                }
            }
            public override string ToString()
            {
                return "(" + Point.X + ", " + Point.Y + ") " + coord.ToString() + " " + type.ToString() + " " + roiName;
            }
        }
        public class Channel
        {
            public string Name = "";
            public string ID = "";
            private int index = 0;
            public string Fluor = "";
            public int SamplesPerPixel;
            public System.Drawing.Color? color;
            public int Emission = -1;
            public int Excitation = -1;
            public int Exposure = -1;
            public string LightSource = "";
            public double LightSourceIntensity = -1;
            public int LightSourceWavelength = -1;
            public string ContrastMethod = "";
            public string IlluminationType = "";
            public int bitsPerPixel;

            public IntRange range;
            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }

            }
            public int Max
            {
                get
                {
                    return range.Max;
                }
                set
                {
                    range.Max = value;
                }
            }
            public int Min
            {
                get
                {
                    return range.Min;
                }
                set
                {
                    range.Min = value;
                }
            }
            public RGB rgb = RGB.R;
            public Channel(int ind, int bitsPerPixel)
            {
                if (bitsPerPixel == 16)
                    Max = 65535;
                if (bitsPerPixel == 14)
                    Max = 16383;
                if (bitsPerPixel == 12)
                    Max = 4096;
                if (bitsPerPixel == 10)
                    Max = 1024;
                if (bitsPerPixel == 8)
                    Max = byte.MaxValue;
                range = new IntRange(0, Max);
                Min = 0;
                index = ind;
            }
            public Channel Copy()
            {
                Channel c = new Channel(index, bitsPerPixel);
                c.Name = Name;
                c.ID = ID;
                c.range = range;
                c.color = color;
                c.Fluor = Fluor;
                c.SamplesPerPixel = SamplesPerPixel;
                c.Emission = Emission;
                c.Excitation = Excitation;
                c.Exposure = Exposure;
                c.LightSource = LightSource;
                c.LightSourceIntensity = LightSourceIntensity;
                c.LightSourceWavelength = LightSourceWavelength;
                c.ContrastMethod = ContrastMethod;
                c.IlluminationType = IlluminationType;
                return c;
            }
            public override string ToString()
            {
                if (Name == "")
                    return index.ToString();
                else
                    return index + ", " + Name;
            }
        }
        public struct BufferInfo
        {
            public static LevelsLinear filter8 = new LevelsLinear();
            public static LevelsLinear16bpp filter16 = new LevelsLinear16bpp();
            public string stringId;
            public int HashID
            {
                get
                {
                    return stringId.GetHashCode();
                }
            }
            public bool littleEndian;
            public bool ConvertToLittleEndian;
            public bool RedGreenFlipped;
            public int length;
            public int SizeX, SizeY;
            public int stride;
            public int RGBChannelsCount;
            public int bitsPerPixel;
            public PixelFormat pixelFormat;
            public BioImage.SZCT Coordinate;

            public BufferInfo(string filepath, int len, int w, int h, int strid, int RGBChsCount, int bitsPerPx, PixelFormat px, BioImage.SZCT coord, int index, bool litleEnd, bool convertToLittleEndian)
            {
                stringId = CreateID(filepath, coord.S, index);
                littleEndian = litleEnd;
                ConvertToLittleEndian = convertToLittleEndian;
                RedGreenFlipped = false;
                length = len;
                SizeX = w;
                SizeY = h;
                stride = strid;
                RGBChannelsCount = RGBChsCount;
                bitsPerPixel = bitsPerPx;
                pixelFormat = px;
                Coordinate = coord;
            }

            public override string ToString()
            {
                return stringId;
            }

        }
        public class Buf
        {
            public BufferInfo info;
            public MemoryMappedFile file;
            public MemoryMappedViewStream stream;
            public BinaryWriter writer;
            public BinaryReader read;
            public int serie;
            public byte[] bytes
            {
                get
                {
                    return GetAllBytes();
                }
                set
                {
                    SetAllBytes(value);
                }
            }
            public Buf(BufferInfo inf, byte[] bts)
            {
                info = inf;
                file = MemoryMappedFile.CreateOrOpen(info.ToString(), inf.length);
                stream = file.CreateViewStream(0, inf.length, MemoryMappedFileAccess.ReadWrite);
                
                
                read = new BinaryReader(stream);
                writer = new BinaryWriter(stream);
                serie = inf.Coordinate.S;
                bytes = bts;

                if (info.RGBChannelsCount == 3 && info.RedGreenFlipped == false)
                {
                    info.RedGreenFlipped = true;
                    //RGB Channels are stored in BGR so we switch them to RGB
                    SetBuffer(switchRedBlue(GetBitmap()));
                    //Then turn the 48bpp buffers to 3 RGB 16bpp buffers
                }
                if (!info.littleEndian && info.ConvertToLittleEndian == true)
                {
                    info.ConvertToLittleEndian = true;
                    //We need to convert this buffer to little endian.
                    ToLittleEndian();
                }

            }
            public static Buf GetFromID(string bid)
            {
                return Table.GetBuffer(bid);
            }
            public Buf BufFromHash(int hash)
            {
                return Table.GetBufferByHash(hash);
            }
            public Buf BufFromID(string id)
            {
                int hash = id.GetHashCode();
                return BufFromHash(hash);
            }
            public static Buf Copy(Buf b, string file, int index)
            {
                byte[] bts = new byte[b.info.length];
                Buffer.BlockCopy(b.bytes, 0, bts, 0, b.bytes.Length);
                BufferInfo bi = b.info;
                bi.stringId = CreateID(file, b.info.Coordinate.S, index);
                Buf bf = new Buf(bi, bts);
                return bf;
            }
            public static Buf Copy(Buf b)
            {
                return new Buf(b.info, b.bytes);
            }

            public override string ToString()
            {
                return info.stringId;
            }

            public byte[] GetAllBytes()
            {
                read.BaseStream.Position = 0;
                return read.ReadBytes(info.length);
            }
            public byte[] GetBytes(int start, int count)
            {
                read.BaseStream.Position = start;
                return read.ReadBytes(count);
            }
            public void Seek0()
            {
                read.BaseStream.Position = 0;
            }
            public void SetAllBytes(byte[] bytes)
            {
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write(bytes, 0, info.length);
            }
            public void SetByte(byte bt, int ind)
            {
                writer.Seek(ind, SeekOrigin.Begin);
                writer.Write(bt);
            }
            public void SetBytes(byte[] bytes, int ind)
            {
                writer.Seek(ind, SeekOrigin.Begin);
                writer.Write(bytes, ind, bytes.Length);
            }
            public void SetShorts(ushort[] shorts, int ind)
            {
                byte[] target = new byte[shorts.Length * 2];
                Buffer.BlockCopy(shorts, 0, target, ind, shorts.Length * 2);
            }
            public void SetBuffer(Bitmap bitmap)
            {
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, info.SizeX, info.SizeY), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                info.pixelFormat = data.PixelFormat;
                IntPtr ptr = data.Scan0;
                int length = data.Stride * bitmap.Height;
                byte[] bytes = new byte[length];
                Marshal.Copy(ptr, bytes, 0, length);
                SetAllBytes(bytes);
                bitmap.UnlockBits(data);
                bitmap.Dispose();
            }

            public void Flush()
            {
                //Here we write any changes made to the 
                writer.Flush();
            }
            Bitmap bitmap;
            public unsafe Bitmap GetBitmap()
            {
                if(bitmap!=null)
                    bitmap.Dispose();
                fixed (byte* ptr = GetAllBytes())
                {
                    bitmap = new Bitmap(info.SizeX, info.SizeY, info.stride, info.pixelFormat, new IntPtr(ptr));
                }
                return bitmap;
            }

            public unsafe Bitmap GetBuffer(PixelFormat pixel)
            {
                if (bitmap != null)
                    bitmap.Dispose();
                fixed (byte* ptr = GetAllBytes())
                {
                    bitmap = new Bitmap(info.SizeX, info.SizeY, info.stride, pixel, new IntPtr(ptr));
                }
                return bitmap;
            }

            public static unsafe Bitmap GetBitmap(byte[] bts, int SizeX, int SizeY, int stride, PixelFormat pixel)
            {
                fixed (byte* ptr = bts)
                {
                    return new Bitmap(SizeX, SizeY, stride, pixel, new IntPtr(ptr));
                }
            }

            public static byte[] GetBuffer(Bitmap bitmap)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                IntPtr ptr = data.Scan0;
                int stride = 0;
                if(bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    stride = bitmap.Width;
                }
                else
                if(bitmap.PixelFormat == PixelFormat.Format16bppGrayScale)
                {
                    stride = bitmap.Width * 2;
                }
                else
                if(bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    stride = bitmap.Width * 3;
                }
                else
                if (bitmap.PixelFormat == PixelFormat.Format48bppRgb)
                {
                    stride = bitmap.Width * 3 * 2;
                }
                int length = stride * bitmap.Height;
                byte[] bytes = new byte[length];
                Marshal.Copy(ptr, bytes, 0, length);
                Array.Reverse(bytes);
                bitmap.UnlockBits(data);
                return bytes;
            }

            public unsafe void ToLittleEndian()
            {
                //Here we convert this buffer to little endian.
                byte[] bts = GetAllBytes();
                Bitmap bitmap;
                Array.Reverse(bts);
                fixed (byte* ptr = bts)
                {
                    bitmap = new Bitmap(info.SizeX, info.SizeY, info.stride, info.pixelFormat, new IntPtr(ptr));
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                }
                SetBuffer(bitmap);
                info.ConvertToLittleEndian = true;
                info.littleEndian = true;
            }

            public unsafe void ToBigEndian()
            {
                //Here we convert this buffer to big endian.
                byte[] bytes = GetAllBytes();
                Array.Reverse(bytes);
                Bitmap bitmap;
                fixed (byte* ptr = bytes)
                {
                    bitmap = new Bitmap(info.SizeX, info.SizeY, info.stride, info.pixelFormat, new IntPtr(ptr));
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                }
                SetBuffer(bitmap);
                bitmap.Dispose();
            }

            public byte[] GetSaveBytes()
            {
                Bitmap bitmap;
                if (info.RGBChannelsCount == 1)
                    bitmap = GetBitmap();
                else
                    bitmap = switchRedBlue(GetBitmap());
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, info.SizeX, info.SizeY), ImageLockMode.ReadWrite, info.pixelFormat);
                IntPtr ptr = data.Scan0;
                int length = this.bytes.Length;
                byte[] bytes = new byte[length];
                Marshal.Copy(ptr, bytes, 0, length);
                Array.Reverse(bytes);
                bitmap.UnlockBits(data);
                bitmap.Dispose();
                return bytes;
            }

            public unsafe Bitmap GetFiltered(IntRange range)
            {
                bitmap = GetBitmap();
                if (info.bitsPerPixel > 8)
                {
                    // set ranges
                    filter16.InRed = range;
                    filter16.InGreen = range;
                    filter16.InBlue = range;
                    return filter16.Apply(bitmap);
                }
                else
                {
                    // set ranges
                    filter8.InRed = range;
                    filter8.InGreen = range;
                    filter8.InBlue = range;
                    return filter8.Apply(bitmap);
                }
            }
            public unsafe Bitmap GetFiltered(IntRange r, IntRange g, IntRange b)
            {
                bitmap = GetBitmap();
                if (info.bitsPerPixel > 8)
                {
                    // set ranges
                    filter16.InRed = r;
                    filter16.InGreen = g;
                    filter16.InBlue = b;
                    return filter16.Apply(bitmap);
                }
                else
                {
                    // set ranges
                    filter8.InRed = r;
                    filter8.InGreen = g;
                    filter8.InBlue = b;
                    return filter8.Apply(bitmap);
                }
            }
            public Bitmap switchRedBlue(Bitmap image)
            {
                ExtractChannel cr = new ExtractChannel(AForge.Imaging.RGB.R);
                ExtractChannel cb = new ExtractChannel(AForge.Imaging.RGB.B);
                // apply the filter
                Bitmap rImage = cr.Apply(image);
                Bitmap bImage = cb.Apply(image);

                ReplaceChannel replaceRFilter = new ReplaceChannel(AForge.Imaging.RGB.R, bImage);
                replaceRFilter.ApplyInPlace(image);

                ReplaceChannel replaceBFilter = new ReplaceChannel(AForge.Imaging.RGB.B, rImage);
                replaceBFilter.ApplyInPlace(image);
                rImage.Dispose();
                bImage.Dispose();
                return image;
            }
            public int GetIndex(int ix, int iy)
            {
                int stridex = info.SizeX;
                int x = ix;
                int y = iy;
                if (!info.littleEndian)
                {
                    x = (info.SizeX - 1) - x;
                    y = (info.SizeY - 1) - y;
                    if (info.bitsPerPixel > 8)
                    {
                        return (y * stridex + x) * 2;
                    }
                    else
                    {
                        return (y * stridex + x);
                    }
                }
                else
                {
                    if (info.bitsPerPixel > 8)
                    {
                        return (y * stridex + x) * 2;
                    }
                    else
                    {
                        return (y * stridex + x);
                    }
                }
            }
            public int GetIndexRGB(int ix, int iy, int index)
            {
                int stridex = info.SizeX;
                //For 16bit (2*8bit) images we multiply buffer index by 2
                int x = ix;
                int y = iy;
                if (!info.littleEndian)
                {
                    x = (info.SizeX - 1) - x;
                    y = (info.SizeY - 1) - y;
                    if (info.bitsPerPixel > 8)
                    {
                        return (y * stridex + x) * 2 * index;
                    }
                    else
                    {
                        return (y * stridex + x) * index;
                    }
                }
                else
                {
                    if (info.bitsPerPixel > 8)
                    {
                        return (y * stridex + x) * 2 * index;
                    }
                    else
                    {
                        return (y * stridex + x) * index;
                    }
                }
            }
            public ushort GetValueRGB(int ix, int iy, int index)
            {
                int i = -1;
                int stridex = info.SizeX;
                //For 16bit (2*8bit) images we multiply buffer index by 2
                int x = ix;
                int y = iy;
                if (!info.littleEndian)
                {
                    x = (info.SizeX - 1) - x;
                    y = (info.SizeY - 1) - y;
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = (y * stridex + x) * 2 * index;
                        i = BitConverter.ToUInt16(bytes, index2);
                        return (ushort)i;
                    }
                    else
                    {
                        int stride = info.SizeX;
                        int indexb = (y * stridex + x) * index;
                        i = bytes[indexb];
                        return (ushort)i;
                    }
                }
                else
                {
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = (y * stridex + x) * 2 * index;
                        i = BitConverter.ToUInt16(bytes, index2);
                        return (ushort)i;
                    }
                    else
                    {
                        int stride = info.SizeX;
                        int indexb = (y * stridex + x) * index;
                        i = bytes[indexb];
                        return (ushort)i;
                    }
                }
            }
            public ushort GetValue(int ix, int iy)
            {
                int i = 0;
                int stridex = info.SizeX;
                //For 16bit (2*8bit) images we multiply buffer index by 2
                int x = ix;
                int y = iy;
                if (ix < 0)
                    x = 0;
                if (iy < 0)
                    y = 0;
                if (ix >= info.SizeX)
                    x = info.SizeX - 1;
                if (iy >= info.SizeY)
                    y = info.SizeY - 1;
                if (!info.littleEndian)
                {
                    x = (info.SizeX - 1) - x;
                    y = (info.SizeY - 1) - y;
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = (y * stridex + x) * 2 * info.RGBChannelsCount;
                        i = BitConverter.ToUInt16(bytes, index2);
                        return (ushort)i;
                    }
                    else
                    {
                        int index = (y * stridex + x) * info.RGBChannelsCount;
                        i = bytes[index];
                        return (ushort)i;
                    }
                }
                else
                {
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = (y * stridex + x) * 2 * info.RGBChannelsCount;
                        i = BitConverter.ToUInt16(bytes, index2);
                        return (ushort)i;
                    }
                    else
                    {
                        int index = (y * stridex + x) * info.RGBChannelsCount;
                        i = bytes[index];
                        return (ushort)i;
                    }
                }
            }
            public void SetValue(int ix, int iy, ushort value)
            {
                byte[] bts = bytes;
                int stridex = info.SizeX;
                //For 16bit (2*8bit) images we multiply buffer index by 2
                int x = ix;
                int y = iy;
                if (!info.littleEndian)
                {
                    x = (info.SizeX - 1) - x;
                    y = (info.SizeY - 1) - y;
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = (y * stridex + x) * 2 * info.RGBChannelsCount;
                        byte upper = (byte)(value >> 8);
                        byte lower = (byte)(value & 0xff);
                        SetByte(upper, index2);
                        SetByte(lower, index2 + 1);
                    }
                    else
                    {
                        int index = (y * stridex + x) * info.RGBChannelsCount;
                        SetByte((byte)value, index);
                    }
                }
                else
                {
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = ((y * stridex + x) * 2 * info.RGBChannelsCount);
                        byte upper = (byte)(value >> 8);
                        byte lower = (byte)(value & 0xff);
                        SetByte(lower, index2);
                        SetByte(upper, index2 + 1);
                    }
                    else
                    {
                        int index = (y * stridex + x) * info.RGBChannelsCount;
                        SetByte((byte)value, index);
                    }
                }
            }
            public void SetValueRGB(int ix, int iy, int RGBChannel, ushort value)
            {
                //Planes are in BGR order so we invert the RGBChannel parameter.
                if (RGBChannel == 0)
                    RGBChannel = 2;
                else
                if (RGBChannel == 2)
                    RGBChannel = 0;
                int stride = info.SizeX;
                int x = ix;
                int y = iy;
                if (!info.littleEndian)
                {
                    x = (info.SizeX - 1) - x;
                    y = (info.SizeY - 1) - y;
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = ((y * stride + x) * 2 * info.RGBChannelsCount) + (RGBChannel);
                        byte upper = (byte)(value >> 8);
                        byte lower = (byte)(value & 0xff);
                        SetByte(upper, index2);
                        SetByte(lower, index2 + 1);
                    }
                    else
                    {
                        int index = ((y * stride + x) * info.RGBChannelsCount) + (RGBChannel);
                        bytes[index] = (byte)value;
                    }
                }
                else
                {
                    if (info.bitsPerPixel > 8)
                    {
                        int index2 = ((y * stride + x) * 2 * info.RGBChannelsCount);
                        byte upper = (byte)(value >> 8);
                        byte lower = (byte)(value & 0xff);
                        SetByte(lower, index2);
                        SetByte(upper, index2 + 1);
                    }
                    else
                    {
                        int index = ((y * stride + x) * info.RGBChannelsCount) + (RGBChannel);
                        bytes[index] = (byte)value;
                    }
                }
            }
            public ushort[,] GetBlock(int ix, int iy, int iw, int ih)
            {
                byte[] bt = new byte[2];
                ushort[,] sh = new ushort[iw,ih];
                for (int y = iy; y < iy + ih; y++)
                {
                    for (int x = ix; x < ix + iw; x++)
                    {
                        int l = GetIndex(x, y);
                        if (info.bitsPerPixel > 8)
                        {
                            sh[x,y] = BitConverter.ToUInt16(bytes, l);
                        }
                        else
                            sh[x,y] = (ushort)bytes[l];
                    }
                }
                return sh;
            }
            public void SetBlock(int ix, int iy, int iw, int ih, ushort[,] sh)
            {
                int xx = ix;
                int yy = iy;
                for (int y = 0; y < ih; y++)
                {
                    for (int x = 0; x <iw; x++)
                    {
                        if (info.bitsPerPixel > 8)
                        {
                            if (info.littleEndian)
                            {
                                SetValue(x+ix,y+iy, sh[x,y]);
                                //SetByte(bt[0],l);
                                //SetByte(bt[1],l+1);
                            }
                            else
                            {
                                SetValue(x, y, sh[x, y]);
                                //SetByte(bt[0], l);
                                //SetByte(bt[1], l+1);
                            }
                        }
                        else
                        {
                            if (info.littleEndian)
                            {
                                //SetByte(bt[0], l);
                            }
                            else
                            {
                                //SetByte(bt[0], l);
                            }
                        }
                       xx++;
                    }
                    yy++;
                } 
                
            }
            public void Dispose()
            {
                file.Dispose();
                stream.Dispose();
                writer.Dispose();
                read.Dispose();
            }
        }
        public ushort[,] GetBlock(int s, int z, int c, int t, int x, int y, int w, int h)
        {
            return Buffers[Coords[s, z, c, t]].GetBlock(x, y, w, h);
        }
        public void SetBlock(int s, int z, int c, int t, int x, int y, int w, int h, ushort[,] sh)
        {
            Buffers[Coords[s, z, c, t]].SetBlock(x, y, w, h, sh);
        }
        public ushort[,] GetBlock(SZCT coord, int x, int y, int w, int h)
        {
            return Buffers[Coords[coord.S, coord.Z, coord.C, coord.T]].GetBlock(x, y, w, h);
        }
        public void SetBlock(SZCT coord, int x, int y, int w, int h, ushort[,] sh)
        {
            Buffers[Coords[coord.S, coord.Z, coord.C, coord.T]].SetBlock(x, y, w, h, sh);
        }

        public static void Save(BioImage im, string file, int ser)
        {
            im.SaveSeries(file, ser);
        }
        public static void Save(string id, string file, int ser)
        {
            BioImage b = Table.GetImage(id);
            b.SaveSeries(file, ser);
        }
        public bool SaveSeries(string file, int series)
        {
            // create OME-XML metadata store
            ServiceFactory factory = new ServiceFactory();
            OMEXMLService service = (OMEXMLService)factory.getInstance(typeof(OMEXMLService));
            loci.formats.meta.IMetadata omexml = service.createOMEXMLMetadata();
            omexml.setImageID("Image:0", series);
            omexml.setPixelsID("Pixels:0", series);
            if (littleEndian)
                omexml.setPixelsBinDataBigEndian(java.lang.Boolean.TRUE, 0, 0);
            else
                omexml.setPixelsBinDataBigEndian(java.lang.Boolean.FALSE, 0, 0);

            omexml.setPixelsDimensionOrder(ome.xml.model.enums.DimensionOrder.XYCZT, series);
            if (bitsPerPixel > 8)
                omexml.setPixelsType(ome.xml.model.enums.PixelType.UINT16, series);
            else
                omexml.setPixelsType(ome.xml.model.enums.PixelType.UINT8, series);
            omexml.setPixelsSizeX(new PositiveInteger(java.lang.Integer.valueOf(SizeX)), series);
            omexml.setPixelsSizeY(new PositiveInteger(java.lang.Integer.valueOf(SizeY)), series);
            omexml.setPixelsSizeZ(new PositiveInteger(java.lang.Integer.valueOf(SizeZ)), series);
            int samples = 1;
            if (isRGB)
                samples = 3;
            omexml.setPixelsSizeC(new PositiveInteger(java.lang.Integer.valueOf(SizeC * samples)), series);
            omexml.setPixelsSizeT(new PositiveInteger(java.lang.Integer.valueOf(SizeT)), series);

            if (physicalSizeX != -1)
            {
                ome.units.quantity.Length p = new ome.units.quantity.Length(java.lang.Double.valueOf(physicalSizeX), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeX(p, series);
            }
            if (physicalSizeY != -1)
            {
                ome.units.quantity.Length p = new ome.units.quantity.Length(java.lang.Double.valueOf(physicalSizeY), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeY(p, series);
            }
            if (physicalSizeZ != -1)
            {
                ome.units.quantity.Length p = new ome.units.quantity.Length(java.lang.Double.valueOf(physicalSizeZ), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeZ(p, series);
            }
            if (stageSizeX != -1)
            {
                ome.units.quantity.Length s = new ome.units.quantity.Length(java.lang.Double.valueOf(stageSizeX), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelX(s, series);
            }
            if (stageSizeY != -1)
            {
                ome.units.quantity.Length s = new ome.units.quantity.Length(java.lang.Double.valueOf(stageSizeY), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelY(s, series);
            }
            if (stageSizeX != -1)
            {
                ome.units.quantity.Length s = new ome.units.quantity.Length(java.lang.Double.valueOf(stageSizeZ), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelZ(s, series);
            }

            for (int channel = 0; channel < Channels.Count; channel++)
            {
                Channel c = Channels[channel];
                omexml.setChannelID("Channel:" + channel + ":" + series, series, channel);
                omexml.setChannelSamplesPerPixel(new PositiveInteger(java.lang.Integer.valueOf(samples)), series, channel);
                if (c.Name != "")
                    omexml.setChannelName(c.Name, series, channel);
                if (c.color != null)
                {
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(c.color.Value.R, c.color.Value.G, c.color.Value.B, c.color.Value.A);
                    omexml.setChannelColor(col, series, channel);
                }
                if (c.Emission != -1)
                {
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Emission), ome.units.UNITS.NANOMETER);
                    omexml.setChannelEmissionWavelength(fl, series, channel);
                }
                if (c.Excitation != -1)
                {
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Excitation), ome.units.UNITS.NANOMETER);
                    omexml.setChannelEmissionWavelength(fl, series, channel);
                }
                if (c.Exposure != -1)
                {
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Exposure), ome.units.UNITS.MILLISECOND);
                    omexml.setChannelEmissionWavelength(fl, series, channel);
                }
                if (c.ContrastMethod != "")
                {
                    ome.xml.model.enums.ContrastMethod cm = (ome.xml.model.enums.ContrastMethod)Enum.Parse(typeof(ome.xml.model.enums.ContrastMethod), c.ContrastMethod);
                    omexml.setChannelContrastMethod(cm, series, channel);
                }
                if (c.Fluor != "")
                {
                    omexml.setChannelFluor(c.Fluor, series, channel);
                }
                if (c.IlluminationType != "")
                {
                    ome.xml.model.enums.IlluminationType cm = (ome.xml.model.enums.IlluminationType)Enum.Parse(typeof(ome.xml.model.enums.IlluminationType), c.IlluminationType);
                    omexml.setChannelIlluminationType(cm, series, channel);
                }
                if (c.LightSourceIntensity != -1)
                {
                    ome.units.quantity.Power fl = new ome.units.quantity.Power(java.lang.Double.valueOf(c.LightSourceIntensity), ome.units.UNITS.VOLT);
                    omexml.setLightEmittingDiodePower(fl, series, channel);
                }
            }

            int i = 0;
            foreach (Annotation an in Annotations)
            {
                if (an.roiID == "")
                    omexml.setROIID("ROI:" + i.ToString() + ":" + series, i);
                else
                    omexml.setROIID(an.roiID, i);
                omexml.setROIName(an.roiName, i);
                if (an.type == Annotation.Type.Point)
                {
                    if (an.id == "")
                        omexml.setPointID(an.id, i, series);
                    else
                        omexml.setPointID("Shape:" + i + ":" + series, i, series);
                    omexml.setPointX(java.lang.Double.valueOf(an.X), i, series);
                    omexml.setPointY(java.lang.Double.valueOf(an.Y), i, series);
                    omexml.setPointTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, series);
                    omexml.setPointTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, series);
                    omexml.setPointTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, series);
                    if (an.Text != "")
                        omexml.setPointText(an.Text, i, series);
                    else
                        omexml.setPointText(i.ToString(), i, series);
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                    meta.setPointFontSize(fl, i, series);
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                    omexml.setPointStrokeColor(col, i, series);
                    ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                    omexml.setPointStrokeWidth(sw, i, series);
                    ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                    omexml.setPointFillColor(colf, i, series);
                }
                else
                if (an.type == Annotation.Type.Polygon || an.type == Annotation.Type.Freeform)
                {
                    if (an.id == "")
                        omexml.setPolygonID(an.id, i, series);
                    else
                        omexml.setPolygonID("Shape:" + i + ":" + series, i, series);
                    omexml.setPolygonPoints(an.PointsToString(), i, series);
                    omexml.setPolygonTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, series);
                    omexml.setPolygonTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, series);
                    omexml.setPolygonTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, series);
                    if (an.Text != "")
                        omexml.setPolygonText(an.Text, i, series);
                    else
                        omexml.setPolygonText(i.ToString(), i, series);
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                    omexml.setPolygonFontSize(fl, i, series);
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                    omexml.setPolygonStrokeColor(col, i, series);
                    ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                    omexml.setPolygonStrokeWidth(sw, i, series);
                    ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                    omexml.setPolygonFillColor(colf, i, series);
                }
                else
                if (an.type == Annotation.Type.Rectangle)
                {
                    if (an.id != "")
                        omexml.setRectangleID(an.id, i, series);
                    else
                        omexml.setRectangleID("Shape:" + i + ":" + series, i, series);
                    omexml.setRectangleWidth(java.lang.Double.valueOf(an.W), i, series);
                    omexml.setRectangleHeight(java.lang.Double.valueOf(an.H), i, series);
                    omexml.setRectangleX(java.lang.Double.valueOf(an.Rect.X), i, series);
                    omexml.setRectangleY(java.lang.Double.valueOf(an.Rect.Y), i, series);
                    omexml.setRectangleTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, series);
                    omexml.setRectangleTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, series);
                    omexml.setRectangleTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, series);
                    omexml.setRectangleText(i.ToString(), i, series);
                    if (an.Text != "")
                        omexml.setRectangleText(an.Text, i, series);
                    else
                        omexml.setRectangleText(i.ToString(), i, series);
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                    omexml.setRectangleFontSize(fl, i, series);
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                    omexml.setRectangleStrokeColor(col, i, series);
                    ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                    omexml.setRectangleStrokeWidth(sw, i, series);
                    ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                    omexml.setRectangleFillColor(colf, i, series);
                }
                else
                if (an.type == Annotation.Type.Line)
                {
                    if (an.id == "")
                        omexml.setLineID(an.id, i, series);
                    else
                        omexml.setLineID("Shape:" + i + ":" + series, i, series);
                    omexml.setLineX1(java.lang.Double.valueOf(an.GetPoint(0).X), i, series);
                    omexml.setLineY1(java.lang.Double.valueOf(an.GetPoint(0).Y), i, series);
                    omexml.setLineX2(java.lang.Double.valueOf(an.GetPoint(1).X), i, series);
                    omexml.setLineY2(java.lang.Double.valueOf(an.GetPoint(1).Y), i, series);
                    omexml.setLineTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, series);
                    omexml.setLineTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, series);
                    omexml.setLineTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, series);
                    if (an.Text != "")
                        omexml.setLineText(an.Text, i, series);
                    else
                        omexml.setLineText(i.ToString(), i, series);
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                    omexml.setLineFontSize(fl, i, series);
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                    omexml.setLineStrokeColor(col, i, series);
                    ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                    omexml.setLineStrokeWidth(sw, i, series);
                    ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                    omexml.setLineFillColor(colf, i, series);
                }
                else
                if (an.type == Annotation.Type.Ellipse)
                {
                    
                    if (an.id == "")
                        omexml.setEllipseID(an.id, i, series);
                    else
                        omexml.setEllipseID("Shape:" + i + ":" + series, i, series);
                    //We need to change System.Drawing.Rectangle to ellipse radius;
                    double w = (double)an.W / 2;
                    double h = (double)an.H / 2;
                    omexml.setEllipseRadiusX(java.lang.Double.valueOf(w), i, series);
                    omexml.setEllipseRadiusY(java.lang.Double.valueOf(h), i, series);

                    double x = an.Point.X + w;
                    double y = an.Point.Y + h;
                    omexml.setEllipseX(java.lang.Double.valueOf(x), i, series);
                    omexml.setEllipseY(java.lang.Double.valueOf(y), i, series);
                    omexml.setEllipseTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, series);
                    omexml.setEllipseTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, series);
                    omexml.setEllipseTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, series);
                    if (an.Text != "")
                        omexml.setEllipseText(an.Text, i, series);
                    else
                        omexml.setEllipseText(i.ToString(), i, series);
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                    omexml.setEllipseFontSize(fl, i, series);
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                    omexml.setEllipseStrokeColor(col, i, series);
                    ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                    omexml.setEllipseStrokeWidth(sw, i, series);
                    ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                    omexml.setEllipseFillColor(colf, i, series);
                }
                else
                if (an.type == Annotation.Type.Label)
                {
                    if (an.id != "")
                        omexml.setLabelID(an.id, i, series);
                    else
                        omexml.setLabelID("Shape:" + i + ":" + series, i, series);
                    omexml.setLabelX(java.lang.Double.valueOf(an.Rect.X), i, series);
                    omexml.setLabelY(java.lang.Double.valueOf(an.Rect.Y), i, series);
                    omexml.setLabelTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, series);
                    omexml.setLabelTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, series);
                    omexml.setLabelTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, series);
                    omexml.setLabelText(i.ToString(), i, series);
                    if (an.Text != "")
                        omexml.setLabelText(an.Text, i, series);
                    else
                        omexml.setLabelText(i.ToString(), i, series);
                    ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                    omexml.setLabelFontSize(fl, i, series);
                    ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                    omexml.setLabelStrokeColor(col, i, series);
                    ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                    omexml.setLabelStrokeWidth(sw, i, series);
                    ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                    omexml.setLabelFillColor(colf, i, series);
                }
                i++;
            }

            //Method used to save a range of an image stack defined by start & count.
            loci.formats.ImageWriter writer = new loci.formats.ImageWriter();
            writer.setMetadataRetrieve(omexml);
            //We delete the file so we don't just add more images to an existing file;
            if (File.Exists(file))
                File.Delete(file);
            writer.setId(file);
            writer.setSeries(series);
            writer.setWriteSequentially(true);

            wr = writer;
            threadFile = Path.GetFileName(file);
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(WriteBytes));
            t.Start();
            threadImage = this;
            Progress pr = new Progress(threadFile, "Saving");
            pr.Show();
            do
            {
                pr.UpdateProgress((int)threadProgress);
                Application.DoEvents();
            } while (!done);
            pr.Close();
            pr.Dispose();
            Recorder.AddLine("BioImage.Save(" + '"' + IdString + '"'+ "," + '"' + file + '"' + "," + series + ");");
            return true;
        }

        private static BioImage threadImage = null;
        public void Save(string file)
        {
            //This is the default saving mode we save the roi's in CSV and save tiff fast with BitMiracle.
            threadImage = this;
            threadFile = file;
            threadProgress = 0;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(Save));
            t.Start();
            Progress pr = new Progress(threadFile, "Saving");
            pr.Show();
            do
            {
                pr.UpdateProgress((int)threadProgress);
                Application.DoEvents();
            } while (!done);
            pr.Close();
        }
        public void Open(string file)
        {
            //This is the default opening mode we load the roi's in CSV and open tiff fast with BitMiracle.
            threadImage = this;
            threadFile = file;
            threadProgress = 0;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(Open));
            t.Start();
            Progress pr = new Progress(threadFile, "Opening");
            pr.Show();
            do
            {
                pr.UpdateProgress((int)threadProgress);
                Application.DoEvents();
            } while (!done);
            pr.Close();
        }

        private static void Save()
        {
            string file = threadFile;
            BioImage b = threadImage;
            done = false;
            string fn = Path.GetFileNameWithoutExtension(file);
            string dir = Path.GetDirectoryName(file);
            if(b.Annotations.Count >0)
            {
                string f = fn + ".csv";
                ExportROIsCSV(f, b.Annotations);
            }
            ImageJDesc j = new ImageJDesc();
            j.FromImage(b);
            string desc = j.GetString();
            using (Tiff image = Tiff.Open(file, "w"))
            {
                int stride = b.Buffers[0].info.stride;
                int im = 0;

                for (int c = 0; c < b.SizeC; c++)
                {
                    for (int z = 0; z < b.SizeZ; z++)
                    {
                        for (int t = 0; t < b.SizeT; t++)
                        {
                            image.SetDirectory((short)im);
                            image.SetField(TiffTag.IMAGEWIDTH, b.SizeX);
                            image.SetField(TiffTag.IMAGEDESCRIPTION, desc);
                            image.SetField(TiffTag.IMAGELENGTH, b.SizeY);
                            image.SetField(TiffTag.BITSPERSAMPLE, b.bitsPerPixel);
                            image.SetField(TiffTag.SAMPLESPERPIXEL, b.rGBChannelCount);
                            image.SetField(TiffTag.ROWSPERSTRIP, b.SizeY);
                            if (im % 2 == 0)
                                image.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                            else
                                image.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISWHITE);
                            image.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                            image.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                            image.SetField(TiffTag.ROWSPERSTRIP, image.DefaultStripSize(0));
                            if (b.physicalSizeX !=-1 && b.physicalSizeY != -1)
                            {
                                image.SetField(TiffTag.XRESOLUTION, (b.physicalSizeX * b.SizeX) / ((b.physicalSizeX * b.SizeX) * b.physicalSizeX));
                                image.SetField(TiffTag.YRESOLUTION, (b.physicalSizeY * b.SizeY) / ((b.physicalSizeY * b.SizeY) * b.physicalSizeY));
                                image.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.NONE);
                            }
                            else
                            {
                                image.SetField(TiffTag.XRESOLUTION, 100.0);
                                image.SetField(TiffTag.YRESOLUTION, 100.0);
                                image.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                            }
                            // specify that it's a page within the multipage file
                            image.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                            // specify the page number
                            byte[] buffer = b.Buffers[im].GetAllBytes();
                            image.SetField(TiffTag.PAGENUMBER, c, b.Buffers.Count);
                            for (int i = 0, offset = 0; i < b.SizeY; i++)
                            {
                                image.WriteScanline(buffer, offset, i, 0);
                                offset += stride;
                            }
                            image.WriteDirectory();
                            threadProgress = ((float)im / (float)b.ImageCount) * 100;
                            im++;
                        }
                    }
                }
            }
            done = true;
        }
        private static void Open()
        {
            string file = threadFile;
            BioImage b = threadImage;
            b.serie = 0;
            done = false;
            string fn = Path.GetFileNameWithoutExtension(file);
            string dir = Path.GetDirectoryName(file);

            if (File.Exists(fn + ".csv"))
            {
                string f = fn + ".csv";
                b.Annotations = BioImage.ImportROIsCSV(f);
            }

            using (Tiff image = Tiff.Open(file, "r"))
            {
                b.SizeX = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                b.SizeY = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                b.bitsPerPixel = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                b.littleEndian = image.IsBigEndian();
                b.rGBChannelCount = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
                string desc = image.GetField(TiffTag.IMAGEDESCRIPTION)[0].ToString();
                ImageJDesc imDesc = new ImageJDesc();
                if (desc.StartsWith("ImageJ"))
                {
                    imDesc.SetString(desc);
                    b.sizeC = imDesc.channels;
                    if (b.sizeC == 0)
                        b.sizeC = 1;
                    b.sizeZ = imDesc.slices;
                    if (b.sizeZ == 0)
                        b.sizeZ = 1;
                    b.sizeT = imDesc.frames;
                    if (b.sizeT == 0)
                        b.sizeT = 1;
                    b.frameInterval = imDesc.finterval;
                    b.physicalSizeZ = imDesc.spacing;
                }
                else
                {
                    throw new InvalidDataException("This tiff file is of unknown format. BioImage supports OME.TIF & ImageJ tiff files.");
                }
                b.Coords = new int[1, b.SizeZ, b.SizeC, b.SizeT];
                try
                {
                    string unit = (string)image.GetField(TiffTag.RESOLUTIONUNIT)[0].ToString();
                    if(unit == "CENTIMETER")
                    {
                        b.physicalSizeX = image.GetField(TiffTag.XRESOLUTION)[0].ToDouble() / 1000;
                        b.physicalSizeX = image.GetField(TiffTag.YRESOLUTION)[0].ToDouble() / 1000;
                    }
                    else
                    if (unit == "INCH")
                    {
                        //inch to centimeter
                        b.physicalSizeX = (2.54 / image.GetField(TiffTag.XRESOLUTION)[0].ToDouble()) /1000;
                        b.physicalSizeY = (2.54 / image.GetField(TiffTag.YRESOLUTION)[0].ToDouble()) /1000;
                    }
                    else
                    if (unit == "NONE")
                    {
                        if (imDesc.unit == "micron")
                        {
                            //size micron
                            b.physicalSizeX = (b.SizeX / image.GetField(TiffTag.XRESOLUTION)[0].ToDouble()) / b.SizeX;
                            b.physicalSizeY = (b.SizeY / image.GetField(TiffTag.YRESOLUTION)[0].ToDouble()) / b.SizeY;
                        }
                        else
                        {
                            throw new InvalidDataException("Image has unknown size unit.");
                        }
                    }
                }
                catch (Exception)
                {

                }
                
                
                int stride = 0;
                if (b.bitsPerPixel > 8)
                {
                    stride = b.SizeX * b.rGBChannelCount * 2;
                }
                else
                {
                    stride = b.SizeX * b.rGBChannelCount;
                }
                if (b.RGBChannelCount == 1)
                {
                    if (b.bitsPerPixel > 8)
                    {
                        b.pixelFormat = PixelFormat.Format16bppGrayScale;
                    }
                    else
                    {
                        b.pixelFormat = PixelFormat.Format8bppIndexed;
                    }
                }
                else
                {
                    if (b.bitsPerPixel > 8)
                        b.pixelFormat = PixelFormat.Format48bppRgb;
                    else
                        b.pixelFormat = PixelFormat.Format24bppRgb;
                }

                for (int i = 0; i < b.SizeC; i++)
                {
                    Channel ch = new Channel(i,b.bitsPerPixel);
                    b.Channels.Add(ch);
                }
                int z = 0;
                int c = 0;
                int t = 0;
                for (int im = 0; im < b.ImageCount; im++)
                {
                    image.SetDirectory((short)im);
                    b.Coords[0, z, c, t] = im;
                    byte[] bytes = new byte[stride * b.SizeY];
                    for (int i = 0, offset = 0; i < b.SizeY; i++)
                    {
                        image.ReadScanline(bytes, offset, i, 0);
                        offset += stride;
                    }
                    if (b.pixelFormat == PixelFormat.Format48bppRgb)
                    {
                        //We split the RGB channels to 3 seperate planes.
                        //The planes are in BGR order.
                        Bitmap rbi = extractR.Apply(Buf.GetBitmap(bytes, b.SizeX, b.SizeY, stride, b.PixelFormat));
                        Bitmap gbi = extractG.Apply(Buf.GetBitmap(bytes, b.SizeX, b.SizeY, stride, b.PixelFormat));
                        Bitmap bbi = extractB.Apply(Buf.GetBitmap(bytes, b.SizeX, b.SizeY, stride, b.PixelFormat));
                        byte[] rb = Buf.GetBuffer(bbi);
                        byte[] gb = Buf.GetBuffer(gbi);
                        byte[] bb = Buf.GetBuffer(rbi);
                        b.rGBChannelCount = 1;
                        PixelFormat px;
                        if (b.pixelFormat == PixelFormat.Format48bppRgb)
                        {
                            px = PixelFormat.Format16bppGrayScale;
                            stride = b.SizeX * b.rGBChannelCount * 2;
                        }
                        else
                        {
                            px = PixelFormat.Format8bppIndexed;
                            stride = b.SizeX * b.rGBChannelCount;
                        }
                        int ic = im * 3;
                        int lenbuf = rb.Length;
                        b.Coords[b.serie, z, c, t] = ic;
                        BufferInfo rfi = new BufferInfo(file, lenbuf, b.SizeX, b.SizeY, stride, b.RGBChannelCount, b.bitsPerPixel, px, new SZCT(b.serie,z,c,t), ic, b.littleEndian, true);
                        Buf rbuf = new Buf(rfi, rb);
                        b.Buffers.Add(rbuf);
                        Table.AddBuffer(rbuf);
                        ic++;
                        c++;
                        b.Coords[b.serie, z, c, t] = ic;
                        BufferInfo gfi = new BufferInfo(file, lenbuf, b.SizeX, b.SizeY, stride, b.RGBChannelCount, b.bitsPerPixel, px, new SZCT(b.serie, z, c, t), ic, b.littleEndian, true);
                        Buf gbuf = new Buf(gfi, gb);
                        b.Buffers.Add(gbuf);
                        Table.AddBuffer(gbuf);
                        ic++;
                        c++;
                        b.Coords[b.serie, z, c, t] = ic;
                        BufferInfo bfi = new BufferInfo(file, lenbuf, b.SizeX, b.SizeY, stride, b.RGBChannelCount, b.bitsPerPixel, px, new SZCT(b.serie, z, c, t), ic, b.littleEndian, true);
                        Buf bbuf = new Buf(bfi, bb);
                        b.Buffers.Add(bbuf);
                        Table.AddBuffer(bbuf);

                    }
                    else
                    {
                        BufferInfo info = new BufferInfo(file, bytes.Length, b.SizeX, b.SizeY, stride, b.RGBChannelCount, b.bitsPerPixel, b.pixelFormat, new SZCT(b.serie, z, c, t), im, b.littleEndian, true);
                        Buf bf = new Buf(info, bytes);
                        b.Buffers.Add(bf);
                    }
                    if (c < b.SizeC - 1)
                        c++;
                    else
                    {
                        c = 0;
                        if (z < b.SizeZ - 1)
                            z++;
                        else
                        {
                            z = 0;
                            if (t < b.SizeT -1)
                                t++;
                            else
                                t = 0;
                        }
                    }
                    threadProgress = ((float)im / (float)b.ImageCount) * 100;
                }
            }
            done = true;
        }

        private static ImageWriter wr;
        private static string threadFile = "";
        private static bool done = false;
        public static float threadProgress = 0;
        public static void WriteBytes()
        {
            threadProgress = 0;
            done = false;
            for (int im = 0; im < threadImage.ImageCount; im++)
            {
                wr.saveBytes(im, threadImage.Buffers[im].GetSaveBytes());
                threadProgress = ((float)im / (float)threadImage.ImageCount) *100;
            }
            done = true;
            wr.close();
        }

        public static BioImage Open(string file, int ser)
        {
            BioImage res = new BioImage(file, ser);
            return res;
        }
        public void OpenSeries(string file, int ser)
        {
            // create OME-XML metadata store
            ServiceFactory factory = new ServiceFactory();
            OMEXMLService service = (OMEXMLService)factory.getInstance(typeof(OMEXMLService));
            meta = service.createOMEXMLMetadata();
            // create format reader
            reader = new ImageReader();
            reader.setMetadataStore(meta);
            // initialize file
            reader.setId(file);
            reader.setSeries(ser);
            rGBChannelCount = reader.getRGBChannelCount();
            bitsPerPixel = reader.getBitsPerPixel();
            filename = file;
            SizeX = reader.getSizeX();
            SizeY = reader.getSizeY();
            sizeC = reader.getSizeC();
            sizeZ = reader.getSizeZ();
            sizeT = reader.getSizeT();
            littleEndian = reader.isLittleEndian();
            seriesCount = reader.getSeriesCount();
            imagesPerSeries = ImageCount / seriesCount;
            Coords = new int[seriesCount, SizeZ, SizeC, SizeT];

            if (RGBChannelCount == 1)
            {
                if (bitsPerPixel > 8)
                {
                    pixelFormat = PixelFormat.Format16bppGrayScale;
                }
                else
                {
                    pixelFormat = PixelFormat.Format8bppIndexed;
                }
            }
            else
            {
                if (bitsPerPixel > 8)
                    pixelFormat = PixelFormat.Format48bppRgb;
                else
                    pixelFormat = PixelFormat.Format24bppRgb;
            }

            //Lets get the channels amd initialize them.
            for (int i = 0; i < SizeC; i++)
            {
                Channel ch = new Channel(i, reader.getBitsPerPixel());
                try
                {
                    if (meta.getChannelName(0, i) != null)
                        ch.Name = meta.getChannelName(0, i);
                    if (meta.getChannelSamplesPerPixel(0, i) != null)
                    {
                        int s = meta.getChannelSamplesPerPixel(0, i).getNumberValue().intValue();
                        ch.SamplesPerPixel = s;
                    }
                    if (meta.getChannelID(0, i) != null)
                        ch.ID = meta.getChannelID(0, i);
                    if (meta.getChannelFluor(0, i) != null)
                        ch.Fluor = meta.getChannelFluor(0, i);
                    if (meta.getChannelColor(0, i) != null)
                    {
                        ome.xml.model.primitives.Color c = meta.getChannelColor(0, i);
                        ch.color = System.Drawing.Color.FromArgb(c.getRed(), c.getGreen(), c.getBlue());
                        if (ch.color.Value.R == 255 && ch.color.Value.G == 0 && ch.color.Value.B == 0)
                            ch.rgb = RGB.R;
                        if (ch.color.Value.R == 0 && ch.color.Value.G == 255 && ch.color.Value.B == 0)
                            ch.rgb = RGB.G;
                        if (ch.color.Value.R == 0 && ch.color.Value.G == 0 && ch.color.Value.B == 255)
                            ch.rgb = RGB.B;
                    }
                    if (meta.getChannelIlluminationType(0, i) != null)
                        ch.IlluminationType = meta.getChannelIlluminationType(0, i).toString();
                    if (meta.getChannelContrastMethod(0, i) != null)
                        ch.ContrastMethod = meta.getChannelContrastMethod(0, i).toString();
                    if (meta.getPlaneExposureTime(0, i) != null)
                        ch.Exposure = meta.getPlaneExposureTime(0, i).value().intValue();
                    if (meta.getChannelEmissionWavelength(0, i) != null)
                        ch.Emission = meta.getChannelEmissionWavelength(0, i).value().intValue();
                    if (meta.getChannelExcitationWavelength(0, i) != null)
                        ch.Excitation = meta.getChannelExcitationWavelength(0, i).value().intValue();
                    if (meta.getChannelLightSourceSettingsAttenuation(0, i) != null)
                        ch.LightSourceIntensity = meta.getChannelLightSourceSettingsAttenuation(0, i).getNumberValue().doubleValue();
                    if (meta.getLightEmittingDiodePower(0, i) != null)
                        ch.LightSourceIntensity = meta.getLightEmittingDiodePower(0, i).value().doubleValue();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                if (i == 0)
                {
                    rgbChannels[0] = 0;
                    ch.rgb = RGB.R;
                }
                else
                if (i == 1)
                {
                    rgbChannels[1] = 1;
                    ch.rgb = RGB.G;
                }
                else
                if (i == 2)
                {
                    rgbChannels[2] = 2;
                    ch.rgb = RGB.B;
                }
                Channels.Add(ch);
            }

            int rc = meta.getROICount();
            for (int i = 0; i < rc; i++)
            {
                string roiID = meta.getROIID(i);
                string roiName = meta.getROIName(i);
                SZCT co = new SZCT(0, 0, 0, 0);
                int scount = 1;
                try
                {
                    scount = meta.getShapeCount(i);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }

                
                for (int sc = 0; sc < scount; sc++)
                {
                    string type = meta.getShapeType(i, sc);
                    BioImage.Annotation an = new Annotation();
                    an.roiID = roiID;
                    an.roiName = roiName;
                    an.shapeIndex = sc;
                    if (type == "Point")
                    {
                        an.type = Annotation.Type.Point;
                        an.id = meta.getPointID(i, sc);
                        double dx = meta.getPointX(i, sc).doubleValue();
                        double dy = meta.getPointY(i, sc).doubleValue();
                        an.AddPoint(new PointD(dx, dy));
                        if (ImageCount > 1)
                        {
                            ome.xml.model.primitives.NonNegativeInteger nz = meta.getPointTheZ(i, sc);
                            if (nz != null)
                                co.Z = nz.getNumberValue().intValue();
                            ome.xml.model.primitives.NonNegativeInteger nc = meta.getPointTheC(i, sc);
                            if (nc != null)
                                co.C = nc.getNumberValue().intValue();
                            ome.xml.model.primitives.NonNegativeInteger nt = meta.getPointTheT(i, sc);
                            if (nt != null)
                                co.T = nt.getNumberValue().intValue();
                            an.coord = co;

                        }

                        an.Text = meta.getPointText(i, sc);
                        ome.units.quantity.Length fl = meta.getPointFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPointStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPointStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPointStrokeColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Line")
                    {
                        an.type = Annotation.Type.Line;
                        an.id = meta.getLineID(i, sc);
                        double px1 = meta.getLineX1(i, sc).doubleValue();
                        double py1 = meta.getLineY1(i, sc).doubleValue();
                        double px2 = meta.getLineX2(i, sc).doubleValue();
                        double py2 = meta.getLineY2(i, sc).doubleValue();
                        an.AddPoint(new PointD(px1, py1));
                        an.AddPoint(new PointD(px2, py2));
                        if( ImageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getLineTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getLineTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getLineTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getLineText(i, sc);
                        ome.units.quantity.Length fl = meta.getLineFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getLineStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getLineStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getLineFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Rectangle")
                    {
                        an.type = Annotation.Type.Rectangle;
                        an.id = meta.getRectangleID(i, sc);
                        double px = meta.getRectangleX(i, sc).doubleValue();
                        double py = meta.getRectangleY(i, sc).doubleValue();
                        double pw = meta.getRectangleWidth(i, sc).doubleValue();
                        double ph = meta.getRectangleHeight(i, sc).doubleValue();
                        an.Rect = new RectangleD(px, py, pw, ph);
                        if (ImageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getRectangleTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getRectangleTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getRectangleTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getRectangleText(i, sc);
                        ome.units.quantity.Length fl = meta.getRectangleFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getRectangleStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getRectangleStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getRectangleFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        ome.xml.model.enums.FillRule fr = meta.getRectangleFillRule(i, sc);
                    }
                    else
                    if (type == "Ellipse")
                    {
                        an.type = Annotation.Type.Ellipse;
                        an.id = meta.getEllipseID(i, sc);
                        double px = meta.getEllipseX(i, sc).doubleValue();
                        double py = meta.getEllipseY(i, sc).doubleValue();
                        double ew = meta.getEllipseRadiusX(i, sc).doubleValue();
                        double eh = meta.getEllipseRadiusY(i, sc).doubleValue();
                        //We convert the ellipse radius to System.Drawing.Rectangle
                        double w = ew * 2;
                        double h = eh * 2;
                        double x = px - ew;
                        double y = py - eh;
                        an.Rect = new RectangleD(x, y, w, h);
                        if (ImageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getEllipseTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getEllipseTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getEllipseTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getEllipseText(i, sc);
                        ome.units.quantity.Length fl = meta.getEllipseFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getEllipseStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getEllipseStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getEllipseFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polygon")
                    {
                        an.type = Annotation.Type.Polygon;
                        an.id = meta.getPolygonID(i, sc);
                        an.closed = true;
                        string pxs = meta.getPolygonPoints(i, sc);
                        PointD[] pts = an.stringToPoints(pxs);
                        if (pts.Length > 100)
                        {
                            an.type = Annotation.Type.Freeform;
                        }
                        an.AddPoints(pts);
                        if (ImageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getPolygonTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getPolygonTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getPolygonTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getPolygonText(i, sc);
                        ome.units.quantity.Length fl = meta.getPolygonFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPolygonStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPolygonStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPolygonFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polyline")
                    {
                        an.type = Annotation.Type.Polyline;
                        an.id = meta.getPolylineID(i, sc);
                        string pxs = meta.getPolylinePoints(i, sc);
                        an.AddPoints(an.stringToPoints(pxs));
                        if (ImageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getPolylineTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getPolylineTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getPolylineTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getPolylineText(i, sc);
                        ome.units.quantity.Length fl = meta.getPolylineFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPolylineStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPolylineStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPolylineFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Label")
                    {
                        an.type = Annotation.Type.Label;
                        an.id = meta.getLabelID(i, sc);
                        
                        if (ImageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getLabelTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getLabelTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getLabelTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        
                        ome.units.quantity.Length fl = meta.getLabelFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getLabelStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getLabelStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getLabelFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        //We set this last so the text is measured correctly.
                        an.AddPoint(new PointD(meta.getLabelX(i, sc).doubleValue(), meta.getLabelY(i, sc).doubleValue()));
                        an.Text = meta.getLabelText(i, sc);
                    }
                    Annotations.Add(an);
                }
            }

            int stride;
            if (bitsPerPixel > 8)
                stride = SizeX * 2 * rGBChannelCount;
            else
                stride = SizeX * rGBChannelCount;

            fileHashTable.Add(file, file.GetHashCode());

            List<string> serFiles = new List<string>();
            serFiles.AddRange(reader.getSeriesUsedFiles());
            //List<BufferInfo> BufferInfos = new List<BufferInfo>(); 
            //List<string> Files = new List<string>();
            int imc = ImageCount / RGBChannelCount;
            for (int i = 0; i < imc; i++)
            {
                int[] ints = reader.getZCTCoords(i);
                byte[] bytes = reader.openBytes(i);
                int z = ints[0];
                int c = ints[1];
                int t = ints[2];
                if (pixelFormat == PixelFormat.Format48bppRgb)
                {
                    //We split the RGB channels to 3 seperate planes.
                    //The planes are in BGR order.
                    Bitmap r = extractB.Apply(Buf.GetBitmap(bytes, SizeX, SizeY, stride, PixelFormat));
                    byte[] rb = Buf.GetBuffer(r);
                    Bitmap g = extractG.Apply(Buf.GetBitmap(bytes, SizeX, SizeY, stride, PixelFormat));
                    byte[] gb = Buf.GetBuffer(g);
                    Bitmap b = extractR.Apply(Buf.GetBitmap(bytes, SizeX, SizeY, stride, PixelFormat));
                    byte[] bb = Buf.GetBuffer(b);
                    rGBChannelCount = 1;
                    PixelFormat px;

                    px = PixelFormat.Format16bppGrayScale;
                    stride = SizeX * rGBChannelCount * 2;

                    int ic = i*3;
                    int lenbuf = rb.Length;
                    Coords[ser, z, c, t] = ic;
                    BufferInfo rfi = new BufferInfo(file, lenbuf, SizeX, SizeY, stride, RGBChannelCount, bitsPerPixel, px, new SZCT(ser, z, c, t), ic, littleEndian, true);
                    Buf rbuf = new Buf(rfi, rb);
                    Buffers.Add(rbuf);
                    Table.AddBuffer(rbuf);
                    ic++;
                    c++;
                    Coords[ser, z, c, t] = ic;
                    BufferInfo gfi = new BufferInfo(file, lenbuf, SizeX, SizeY, stride, RGBChannelCount, bitsPerPixel, px, new SZCT(ser, z, c, t), ic, littleEndian, true);
                    Buf gbuf = new Buf(gfi, gb);
                    Buffers.Add(gbuf);
                    Table.AddBuffer(gbuf);
                    ic++;
                    c++;
                    Coords[ser, z, c, t] = ic;
                    BufferInfo bfi = new BufferInfo(file, lenbuf, SizeX, SizeY, stride, RGBChannelCount, bitsPerPixel, px, new SZCT(ser, z, c, t), ic, littleEndian, true);
                    Buf bbuf = new Buf(bfi, bb);
                    Buffers.Add(bbuf);
                    Table.AddBuffer(bbuf);

                }
                else
                {
                    int lenbuf = bytes.Length;
                    Coords[ser, z, c, t] = i;
                    BufferInfo fi = new BufferInfo(file, lenbuf, SizeX, SizeY, stride, RGBChannelCount, bitsPerPixel, pixelFormat, new SZCT(ser, z, c, t), i, littleEndian, true);
                    Buf buf = new Buf(fi, bytes);
                    Buffers.Add(buf);
                    Table.AddBuffer(buf);
                }
            }
            double stx = 0;
            double sty = 0;
            double stz = 0;
            double six = 0;
            double siy = 0;
            double siz = 0;

            try
            {
                if (meta.getPixelsPhysicalSizeX(ser) != null)
                    physicalSizeX = meta.getPixelsPhysicalSizeX(ser).value().doubleValue();
                if (meta.getPixelsPhysicalSizeY(ser) != null)
                    physicalSizeY = meta.getPixelsPhysicalSizeY(ser).value().doubleValue();
                if (meta.getPixelsPhysicalSizeZ(ser) != null)
                    physicalSizeZ = meta.getPixelsPhysicalSizeZ(ser).value().doubleValue();

                //Calling these when they are not defined causes an error so we use the try catch block.
                if (meta.getStageLabelX(ser) != null)
                    stageSizeX = meta.getStageLabelX(ser).value().doubleValue();
                if (meta.getStageLabelY(ser) != null)
                    stageSizeY = meta.getStageLabelY(ser).value().doubleValue();
                if (meta.getStageLabelZ(ser) != null)
                    stageSizeZ = meta.getStageLabelZ(ser).value().doubleValue();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                Volumes.Add(new VolumeD(new Point3D(stx, sty, stz), new Point3D(six * SizeX, siy * SizeY, siz * SizeZ)));
            }
            catch (Exception)
            {
                //Volume is used only for stage coordinates if error is thrown it is because this image doens't have any size information or it is incomplete as read by Bioformats.
            }
            Table.AddImage(this);
            reader.close();
            Recorder.AddLine("BioImage.Open(" + '"' + file + '"' + "," + ser + ");");
        }
        public int GetSeriesCount(string file)
        {
            // create OME-XML metadata store
            ServiceFactory factory = new ServiceFactory();
            OMEXMLService service = (OMEXMLService)factory.getInstance(typeof(OMEXMLService));
            loci.formats.ome.OMEXMLMetadata meta = service.createOMEXMLMetadata();
            // create format reader
            ImageReader imageReader = new ImageReader();
            imageReader.setMetadataStore(meta);
            // initialize file
            imageReader.setId(file);
            int c = imageReader.getSeriesCount();
            imageReader.close();
            return c;
        }
        public static List<Annotation> OpenROIs(string file)
        {
            List<Annotation> Annotations = new List<Annotation>();
            // create OME-XML metadata store
            ServiceFactory factory = new ServiceFactory();
            OMEXMLService service = (OMEXMLService)factory.getInstance(typeof(OMEXMLService));
            loci.formats.ome.OMEXMLMetadata meta = service.createOMEXMLMetadata();
            // create format reader
            ImageReader imageReader = new ImageReader();
            imageReader.setMetadataStore(meta);
            // initialize file
            imageReader.setId(file);
            int imageCount = imageReader.getImageCount();
            int seriesCount = imageReader.getSeriesCount();

            int rc = meta.getROICount();
            for (int i = 0; i < rc; i++)
            {
                string roiID = meta.getROIID(i);
                string roiName = meta.getROIName(i);
                SZCT co = new SZCT(0, 0, 0, 0);
                int scount = 1;
                try
                {
                    scount = meta.getShapeCount(i);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }


                for (int sc = 0; sc < scount; sc++)
                {
                    string type = meta.getShapeType(i, sc);
                    BioImage.Annotation an = new Annotation();
                    an.roiID = roiID;
                    an.roiName = roiName;
                    an.shapeIndex = sc;
                    if (type == "Point")
                    {
                        an.type = Annotation.Type.Point;
                        an.id = meta.getPointID(i, sc);
                        double dx = meta.getPointX(i, sc).doubleValue();
                        double dy = meta.getPointY(i, sc).doubleValue();
                        an.AddPoint(new PointD(dx, dy));
                        if (imageCount > 1)
                        {
                            ome.xml.model.primitives.NonNegativeInteger nz = meta.getPointTheZ(i, sc);
                            if (nz != null)
                                co.Z = nz.getNumberValue().intValue();
                            ome.xml.model.primitives.NonNegativeInteger nc = meta.getPointTheC(i, sc);
                            if (nc != null)
                                co.C = nc.getNumberValue().intValue();
                            ome.xml.model.primitives.NonNegativeInteger nt = meta.getPointTheT(i, sc);
                            if (nt != null)
                                co.T = nt.getNumberValue().intValue();
                            an.coord = co;

                        }

                        an.Text = meta.getPointText(i, sc);
                        ome.units.quantity.Length fl = meta.getPointFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPointStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPointStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPointStrokeColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Line")
                    {
                        an.type = Annotation.Type.Line;
                        an.id = meta.getLineID(i, sc);
                        double px1 = meta.getLineX1(i, sc).doubleValue();
                        double py1 = meta.getLineY1(i, sc).doubleValue();
                        double px2 = meta.getLineX2(i, sc).doubleValue();
                        double py2 = meta.getLineY2(i, sc).doubleValue();
                        an.AddPoint(new PointD(px1, py1));
                        an.AddPoint(new PointD(px2, py2));
                        if (imageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getLineTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getLineTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getLineTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getLineText(i, sc);
                        ome.units.quantity.Length fl = meta.getLineFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getLineStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getLineStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getLineFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Rectangle")
                    {
                        an.type = Annotation.Type.Rectangle;
                        an.id = meta.getRectangleID(i, sc);
                        double px = meta.getRectangleX(i, sc).doubleValue();
                        double py = meta.getRectangleY(i, sc).doubleValue();
                        double pw = meta.getRectangleWidth(i, sc).doubleValue();
                        double ph = meta.getRectangleHeight(i, sc).doubleValue();
                        an.Rect = new RectangleD(px, py, pw, ph);
                        if (imageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getRectangleTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getRectangleTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getRectangleTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getRectangleText(i, sc);
                        ome.units.quantity.Length fl = meta.getRectangleFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getRectangleStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getRectangleStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getRectangleFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        ome.xml.model.enums.FillRule fr = meta.getRectangleFillRule(i, sc);
                    }
                    else
                    if (type == "Ellipse")
                    {
                        an.type = Annotation.Type.Ellipse;
                        an.id = meta.getEllipseID(i, sc);
                        double px = meta.getEllipseX(i, sc).doubleValue();
                        double py = meta.getEllipseY(i, sc).doubleValue();
                        double ew = meta.getEllipseRadiusX(i, sc).doubleValue();
                        double eh = meta.getEllipseRadiusY(i, sc).doubleValue();
                        //We convert the ellipse radius to System.Drawing.Rectangle
                        double w = ew * 2;
                        double h = eh * 2;
                        double x = px - ew;
                        double y = py - eh;
                        an.Rect = new RectangleD(x, y, w, h);
                        if (imageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getEllipseTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getEllipseTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getEllipseTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getEllipseText(i, sc);
                        ome.units.quantity.Length fl = meta.getEllipseFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getEllipseStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getEllipseStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getEllipseFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polygon")
                    {
                        an.type = Annotation.Type.Polygon;
                        an.id = meta.getPolygonID(i, sc);
                        an.closed = true;
                        string pxs = meta.getPolygonPoints(i, sc);
                        PointD[] pts = an.stringToPoints(pxs);
                        if (pts.Length > 100)
                        {
                            an.type = Annotation.Type.Freeform;
                        }
                        an.AddPoints(pts);
                        if (imageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getPolygonTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getPolygonTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getPolygonTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getPolygonText(i, sc);
                        ome.units.quantity.Length fl = meta.getPolygonFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPolygonStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPolygonStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPolygonFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polyline")
                    {
                        an.type = Annotation.Type.Polyline;
                        an.id = meta.getPolylineID(i, sc);
                        string pxs = meta.getPolylinePoints(i, sc);
                        an.AddPoints(an.stringToPoints(pxs));
                        if (imageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getPolylineTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getPolylineTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getPolylineTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }
                        an.Text = meta.getPolylineText(i, sc);
                        ome.units.quantity.Length fl = meta.getPolylineFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPolylineStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPolylineStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPolylineFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Label")
                    {
                        an.type = Annotation.Type.Label;
                        an.id = meta.getLabelID(i, sc);

                        if (imageCount > 1)
                        {
                            if (sc > 0)
                            {
                                an.coord = co;
                            }
                            else
                            {
                                ome.xml.model.primitives.NonNegativeInteger nz = meta.getLabelTheZ(i, sc);
                                if (nz != null)
                                    co.Z = nz.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nc = meta.getLabelTheC(i, sc);
                                if (nc != null)
                                    co.C = nc.getNumberValue().intValue();
                                ome.xml.model.primitives.NonNegativeInteger nt = meta.getLabelTheT(i, sc);
                                if (nt != null)
                                    co.T = nt.getNumberValue().intValue();
                                an.coord = co;
                            }
                        }

                        ome.units.quantity.Length fl = meta.getLabelFontSize(i, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getLabelStrokeColor(i, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getLabelStrokeWidth(i, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getLabelFillColor(i, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        //We set this last so the text is measured correctly.
                        an.AddPoint(new PointD(meta.getLabelX(i, sc).doubleValue(), meta.getLabelY(i, sc).doubleValue()));
                        an.Text = meta.getLabelText(i, sc);
                    }
                    Annotations.Add(an);
                }
            }
            imageReader.close();
            return Annotations;
        }
        public static void ExportROIsCSV(string filename, List<Annotation> Annotations)
        {
            string con = "";
            string cols = "ROIID,ROINAME,TYPE,ID,SHAPEINDEX,TEXT,S,C,Z,T,X,Y,W,H,POINTS,STROKECOLOR,STROKECOLORW,FILLCOLOR,FONTSIZE" + Environment.NewLine;
            con += cols;
            for (int i = 0; i < Annotations.Count; i++)
            {
                BioImage.Annotation an = Annotations[i];
                BioImage.PointD[] points = an.GetPoints();
                string pts = "";
                for (int j = 0; j < points.Length; j++)
                {
                    if (j == points.Length - 1)
                        pts += points[j].X.ToString() + "," + points[j].Y.ToString();
                    else
                        pts += points[j].X.ToString() + "," + points[j].Y.ToString() + " ";
                }

                char sep = (char)34;
                string sColor = sep.ToString() + an.strokeColor.A.ToString() + ',' + an.strokeColor.R.ToString() + ',' + an.strokeColor.G.ToString() + ',' + an.strokeColor.B.ToString() + sep.ToString();
                string bColor = sep.ToString() + an.fillColor.A.ToString() + ',' + an.fillColor.R.ToString() + ',' + an.fillColor.G.ToString() + ',' + an.fillColor.B.ToString() + sep.ToString();

                string line = an.roiID + ',' + an.roiName + ',' + an.type.ToString() + ',' + an.id + ',' + an.shapeIndex.ToString() + ',' +
                    an.Text + ',' + an.coord.S.ToString() + ',' + an.coord.Z.ToString() + ',' + an.coord.C.ToString() + ',' + an.coord.T.ToString() + ',' + an.X.ToString() + ',' + an.Y.ToString() + ',' +
                    an.W.ToString() + ',' + an.H.ToString() + ',' + sep.ToString() + pts + sep.ToString() + ',' + sColor + ',' + an.strokeWidth.ToString() + ',' + bColor + ',' + an.font.Size.ToString() + ',' + Environment.NewLine;
                con += line;
            }
            File.WriteAllText(filename, con);
        }
        public static List<Annotation> ImportROIsCSV(string filename)
        {
            List<Annotation> list = new List<Annotation>();
            string[] sts = File.ReadAllLines(filename);
            for (int l = 1; l < sts.Length; l++)
            {
                BioImage.Annotation an = new BioImage.Annotation();
                string val = "";
                bool inSep = false;
                int col = 0;
                double x = 0;
                double y = 0;
                double w = 0;
                double h = 0;
                string line = sts[l];

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (c == (char)34)
                    {
                        if (!inSep)
                        {
                            inSep = true;
                        }
                        else
                            inSep = false;
                        continue;
                    }

                    if (c == ',' && !inSep)
                    {
                        //ROIID,ROINAME,TYPE,ID,SHAPEINDEX,TEXT,S,C,Z,T,X,Y,W,H,POINTS,STROKECOLOR,STROKECOLORW,FILLCOLOR,FONTSIZE
                        if (col == 0)
                        {
                            //ROIID
                            an.roiID = val;
                        }
                        else
                        if (col == 1)
                        {
                            //ROINAME
                            an.roiName = val;
                        }
                        else
                        if (col == 2)
                        {
                            //TYPE
                            an.type = (BioImage.Annotation.Type)Enum.Parse(typeof(BioImage.Annotation.Type), val);
                        }
                        else
                        if (col == 3)
                        {
                            //ID
                            an.id = val;
                        }
                        else
                        if (col == 4)
                        {
                            //SHAPEINDEX/
                            an.shapeIndex = int.Parse(val);
                        }
                        else
                        if (col == 5)
                        {
                            //TEXT/
                            an.Text = val;
                        }
                        else
                        if (col == 6)
                        {
                            an.coord.S = int.Parse(val);
                        }
                        else
                        if (col == 7)
                        {
                            an.coord.Z = int.Parse(val);
                        }
                        else
                        if (col == 8)
                        {
                            an.coord.C = int.Parse(val);
                        }
                        else
                        if (col == 9)
                        {
                            an.coord.T = int.Parse(val);
                        }
                        else
                        if (col == 10)
                        {
                            x = double.Parse(val);
                        }
                        else
                        if (col == 11)
                        {
                            y = double.Parse(val);
                        }
                        else
                        if (col == 12)
                        {
                            w = double.Parse(val);
                        }
                        else
                        if (col == 13)
                        {
                            h = double.Parse(val);
                        }
                        else
                        if (col == 14)
                        {
                            //POINTS
                            an.AddPoints(an.stringToPoints(val));
                            an.Rect = new BioImage.RectangleD(x, y, w, h);
                        }
                        else
                        if (col == 15)
                        {
                            //STROKECOLOR
                            string[] st = val.Split(',');
                            an.strokeColor = System.Drawing.Color.FromArgb(int.Parse(st[0]), int.Parse(st[1]), int.Parse(st[2]), int.Parse(st[3]));
                        }
                        else
                        if (col == 16)
                        {
                            //STROKECOLORW
                            an.strokeWidth = double.Parse(val);
                        }
                        else
                        if (col == 17)
                        {
                            //FILLCOLOR
                            string[] st = val.Split(',');
                            an.fillColor = System.Drawing.Color.FromArgb(int.Parse(st[0]), int.Parse(st[1]), int.Parse(st[2]), int.Parse(st[3]));
                        }
                        else
                        if (col == 18)
                        {
                            //FONTSIZE
                            double s = double.Parse(val);
                            an.font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, (float)s, System.Drawing.FontStyle.Regular);
                        }
                        col++;
                        val = "";
                    }
                    else
                        val += c;
                }
                list.Add(an);
            }
            return list;
        }
        public static void ExportROIFolder(string path, string filename)
        {
            string[] fs = Directory.GetFiles(path);
            int i = 0;
            foreach (string f in fs)
            {
                List<Annotation> annotations = OpenROIs(f);
                string ff = Path.GetFileNameWithoutExtension(f);
                ExportROIsCSV(path + "//" + ff + "-" + i.ToString() + ".csv", annotations);
                i++;
            }
        }

        private byte[] autoBytes;
        public bool AutoThresholdChannel(Channel c1)
        {
            if (bitsPerPixel > 8)
                for (int time = 0; time < SizeT; time++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        int i = Coords[serie, z, c1.Index, time];
                        autoBytes = Buffers[i].bytes;
                        int index, index2, x, y;
                        if (!littleEndian)
                        {
                            //stride 1 is for the destination image and stride2 for source image.
                            //destination image will always be 3channels(RGB) with 24bits
                            int stride1 = SizeX * 3;
                            int stride2 = SizeX * RGBChannelCount;
                            for (y = SizeY - 1; y > -1; y--)
                            {
                                for (x = SizeX - 1; x > -1; x--)
                                {
                                    //index is for destination image and index2 for source image
                                    index = ((SizeY - y) * stride1) + ((SizeX - x) * 3);
                                    //For 16bit (2*8bit) images we multiply buffer index by 2
                                    index2 = (y * stride2 + (x * RGBChannelCount)) * 2;
                                    int px = BitConverter.ToUInt16(autoBytes, index2);
                                    if (px > c1.Max)
                                        c1.Max = px;
                                }
                            }
                        }
                        else
                        {
                            //stride 1 is for the destination image and stride2 for source image.
                            //destination image will always be 3channels(RGB) with 24bits
                            int stride1 = SizeX * 3;
                            int stride2 = SizeX * RGBChannelCount;
                            for (y = 0; y < SizeY; y++)
                            {
                                for (x = 0; x < SizeX; x++)
                                {
                                    //index is for destination image and index2 for source image
                                    index = ((SizeY - y) * stride1) + ((SizeX - x) * 3);
                                    //For 16bit (2*8bit) images we multiply buffer index by 2
                                    index2 = (y * stride2 + (x * RGBChannelCount)) * 2;
                                    int px = BitConverter.ToUInt16(autoBytes, index2);
                                    if (px > c1.Max)
                                        c1.Max = px;
                                }
                            }
                        }
                    }
                }
            return true;
        }
        public void AutoThreshold()
        {
            foreach (Channel c in Channels)
            {
                c.Max = 0;
                AutoThresholdChannel(c);
            }
            //RefreshPlane();
        }
        public void Dispose()
        {
            Table.RemoveImage(this);
            for (int i = 0; i < Buffers.Count; i++)
            {
                Buffers[i].Dispose();
            }
        }

        public override string ToString()
        {
            return IdString.ToString();
        }

    }

}
