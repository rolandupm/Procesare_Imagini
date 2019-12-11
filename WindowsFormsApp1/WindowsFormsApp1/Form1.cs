using Emgu.CV;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        byte[,,] data;
        byte[,,] outputData;
        Image<Lab, Byte> original;
        Image<Lab, Byte> outputImage;
        // Label - distance
        //List<Pixel> pixels = new List<Pixel>();
        double[,] pixelsD;
        int[,] pixelsL;

        List<List<Center>> pixelGroups = new List<List<Center>>();
        //cluster centers
        List<Center> centers = new List<Center>();
        List<Center> groupColors = new List<Center>();

        Image img;

        int N, k, S_;
        double S;
        int m, itterations;



        public Form1()
        {

            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Run();
        }


        private void Run()
        {
            for (int counter = 0; counter < itterations; counter++)
            {
                //Debug.writeToFile("ii = " + counter + "\n");
                resetDistances();
                assingment();
                update(counter);

            }
            showOutputImage();
        }

        private void resetDistances()
        {

            for (int i = 0; i < original.Rows; i++)
            {
                for (int j = 0; j < original.Cols; j++)
                {
                    pixelsD[i, j] = 99999999999999;
                }
            }
        }
        private void showOutputImage()
        {
            int R, G, B;

            for (int ii = 0; ii < centers.Count; ii++)
            {

                for (int i = 0; i < original.Rows; i++)
                {
                    for (int j = 0; j < original.Cols; j++)
                    {
                        if (pixelsL[i, j] == ii)
                        {
                            outputData[i, j, 0] = (byte)centers[ii].r;
                            outputData[i, j, 1] = (byte)centers[ii].g;
                            outputData[i, j, 2] = (byte)centers[ii].b;
                        }
                    }
                }
            }


            //showCenters();




            outputImage.Data = outputData;
            pictureBox2.Image = outputImage.ToBitmap();
        }
        private void showCenters()
        {
            for (int ii = 0; ii < centers.Count; ii++)
            {
                for (int t = 0; t < 5; t++)
                {
                    try
                    {
                        outputData[centers[ii].x, centers[ii].y + t, 0] = 255;
                        outputData[centers[ii].x, centers[ii].y + t, 0] = 255;
                        outputData[centers[ii].x, centers[ii].y + t, 1] = 0;
                        outputData[centers[ii].x, centers[ii].y + t, 1] = 0;
                        outputData[centers[ii].x, centers[ii].y + t, 2] = 0;
                        outputData[centers[ii].x, centers[ii].y + t, 2] = 0;
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }


        private void assingment()
        {
            pixelGroups = new List<List<Center>>();
            for (int ii = 0; ii < centers.Count; ii++)
            {
                List<Center> tempList = new List<Center>();

                for (int i = centers[ii].x - S_; i < centers[ii].x + S_; i++)
                {
                    for (int j = centers[ii].y - S_; j < centers[ii].y + S_; j++)
                    {
                        if (i >= 0 && j >= 0 && i < original.Rows && j < original.Cols)
                        {
                            double dist = distance(centers[ii], i, j);

                            //Debug.writeToFile(ii + " - " + dist);

                            if (dist < pixelsD[i, j])
                            {
                                pixelsD[i, j] = dist;
                                pixelsL[i, j] = ii;

                                Center c = new Center();

                                c.x = i;
                                c.y = j;
                                c.r = data[i, j, 0];
                                c.g = data[i, j, 1];
                                c.b = data[i, j, 2];

                                tempList.Add(c);
                            }
                        }
                    }
                }

                pixelGroups.Add(tempList);
            }
            // debug(1);
        }
        private void update(int ctr)
        {
            int R = 0, G = 0, B = 0, X = 0, Y = 0;
            int index = 0;



            //foreach (var item in pixelGroups)
            //{
            //    Debug.writeToFile("pixel groups count: " + item.Count + ",");
            //}

            // centers.Clear();

            //compute new centers
            foreach (var item in pixelGroups)
            {
                R = G = B = X = Y = 0;
                foreach (var subitem in item)
                {
                    R += subitem.r;
                    G += subitem.g;
                    B += subitem.b;
                    X += subitem.x;
                    Y += subitem.y;
                }
                try
                {
                    centers[index].r = R / item.Count;
                    centers[index].g = G / item.Count;
                    centers[index].b = B / item.Count;
                    centers[index].x = X / item.Count;
                    centers[index].y = Y / item.Count;
                }
                catch (Exception)
                {

                }
                index++;
            }
            // debug(1);
        }

        private double distance(Center center, int i, int j)
        {

            byte red = data[i, j, 0];
            byte green = data[i, j, 1];
            byte blue = data[i, j, 2];

            double drgb = Math.Sqrt((center.r - red) * (center.r - red) + (center.g - green) * (center.g - green) + (center.b - blue) * (center.b - blue));
            double dxy = Math.Sqrt((center.x - i) * (center.x - i) + (center.y - j) * (center.y - j));

            double D = drgb + (m / S_) * dxy;
            return D;

        }
        private void button2_Click(object sender, EventArgs e)
        {
            
            if (loadFile())
            {
                
                setParameters();
                // initializeParameters();
                data = original.Data;
                setCenters();

            }
        }
        private void clearAll()
        {

            pixelGroups = new List<List<Center>>();
            //cluster centers
            centers = new List<Center>();
            groupColors = new List<Center>();
            pictureBox1.Image = null;
            pictureBox2.Image = null;
        }
        private bool loadFile()
        {
            OpenFileDialog fDialog = new OpenFileDialog();

            fDialog.Title = "Select file to be upload";
            fDialog.Filter = "All Files|*.*";

            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                clearAll();
                pictureBox1.Image = System.Drawing.Image.FromFile(fDialog.FileName.ToString());
                original = new Image<Lab, byte>(fDialog.FileName.ToString());

                outputImage = new Image<Lab, byte>(original.Rows, original.Cols);

                data = original.Data;
                outputData = original.Data;
                img = pictureBox1.Image;
                return true;
            }
            return false;
        }
        private void setLabels()
        {
            label1.Text = this.N.ToString();
            label2.Text = this.k.ToString();
            label3.Text = this.S_.ToString();
            label10.Text = original.Cols.ToString();
            label11.Text = original.Rows.ToString();

        }

        private void setParameters()
        {
            //set N, k , S based on the loaded image.
            this.itterations = (int)numericUpDown2.Value;
            this.N = original.Rows * original.Cols;
            this.k = (int)numericUpDown1.Value;
            this.S = Math.Ceiling(Math.Sqrt(N / k));
            this.S_ = (int)S;
            this.m = (int)numericUpDown3.Value;

            setLabels();

            pixelsD = new double[original.Rows, original.Cols];
            pixelsL = new int[original.Rows, original.Cols];

            //Create output Image, each pixels label is -1 and distance inf
            for (int i = 0; i < original.Rows; i++)
            {
                for (int j = 0; j < original.Cols; j++)
                {
                    //initial label = -1 (labels are from 1..K, e.g.: cluster number (superpixel)
                    // initial distance = inf.
                    pixelsD[i, j] = 99999999999999;
                    pixelsL[i, j] = -1;

                }
            }
        }

        private void setCenters()
        {
            double x, y;
            int startx, starty;

            startx = (int)Math.Ceiling(S / 2);
            starty = (int)Math.Ceiling(S / 2);



            for (int i = starty; i < original.Rows; i += S_)
            {
                for (int j = startx; j < original.Cols; j += S_)
                {

                    Center c = new Center();
                    c.r = data[i, j, 0];
                    c.g = data[i, j, 1];
                    c.b = data[i, j, 2];
                    c.x = i;
                    c.y = j;
                    centers.Add(c);


                }
                // starty += (int)S;
            }

            double min_grad = 99999999999999;
            double grad;
            int min_i = 0, min_j = 0;
            for (int ii = 0; ii < centers.Count; ii++)
            {
                min_grad = 99999999999999;
                for (int i = centers[ii].x - 1; i < centers[ii].x + 2; i += S_)
                {
                    for (int j = centers[ii].y - 1; j < centers[ii].y + 2; j += S_)
                    {
                        int i1 = data[i + 1, j, 0] - data[i - 1, j, 0];
                        int i2 = data[i + 1, j, 1] - data[i - 1, j, 1];
                        int i3 = data[i + 1, j, 2] - data[i - 1, j, 2];


                        int i4 = data[i, j + 1, 0] - data[i, j - 1, 0];
                        int i5 = data[i, j + 1, 1] - data[i, j - 1, 1];
                        int i6 = data[i, j + 1, 2] - data[i, j - 1, 2];


                        grad = Math.Sqrt(i1 * i1 + i2 * i2 + i3 * i3) + Math.Sqrt(i4 * i4 + i5 * i5 + i6 * i6);
                        if (grad < min_grad)
                        {
                            min_grad = grad;
                            min_i = i;
                            min_j = j;
                        }

                    }
                }
                centers[ii].x = min_i;
                centers[ii].y = min_j;
            }


        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExportToBmp("output");
        }


        public void ExportToBmp(string filename)
        {
            pictureBox2.Image.Save(filename + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            debug(2);
            //debug(2);
        }

        private void initializeParameters()
        {
            for (int i = 0; i < k; i++)
            {
                Center c = new Center();
                c.r = c.g = c.b = c.x = c.y = 0;
                centers.Add(c);
            }

        }

        private void debug(byte flag)
        {
            StringBuilder sb = new StringBuilder();
            switch (flag)
            {
                case 1:
                    {
                        foreach (var item in centers)
                        {

                            sb.Append(item.x + " " + item.y + " " + item.r + " " + item.g + " " + item.b + "\n");
                            Debug.writeToFile(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    }

                case 2:
                    {
                        for (int i = 0; i < original.Rows; i++)
                        {
                            for (int j = 0; j < original.Cols; j++)
                            {
                                Debug.writeToFile(i + " " + j + " - " + "Label: " + pixelsL[i, j] + ", Dist: " + pixelsD[i, j] + " -  " + "\n");

                            }
                        }
                        break;
                    }

                case 3:
                    {
                        for (int i = 0; i < original.Rows; i++)
                        {
                            for (int j = 0; j < original.Cols; j++)
                            {
                                Debug.writeToFile(pixelsL[i, j].ToString() + "," + pixelsD[i, j] + "\n");

                            }

                        }
                        break;
                    }
            }

        }


    }

    public class Center
    {
        public int r;
        public int g;
        public int b;
        public int x;
        public int y;
    }
    public class Pixel
    {
        public int l;
        public int d;
        public int x;
        public int y;
    }

}
