using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace setup2Kinects
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        //Declaracion de variables 
        KinectSensor kinect1;
        KinectSensor kinect2;
        int color1Stride;
        int color2Stride; 
        //int depth1Stride; 
        //int depth2Stride;
        //short[] pixelesDepth1;
        //short[] pixelesDepth2;
        byte[] pixelesColor1;
        byte[] pixelesColor2; 
        //byte[] colorDepth1; 
        //byte[] colorDepth2; 
        Int32Rect rect1Color;
        Int32Rect rect2Color;
        //Int32Rect rect1Depth;
        //Int32Rect rect2Depth; 
        WriteableBitmap bitmap1Color;
        WriteableBitmap bitmap2Color;
        //WriteableBitmap bitmap1Depth;
        //WriteableBitmap bitmap2Depth; 


        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            InicializaKinects();
            PollStreamColor();
            //PollStreamDepth();
        }

        private void InicializaKinects()
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No se ha detectado ningun kinect.", "Error");
                Application.Current.Shutdown();
            }
            else
            { 
                try
                {
                    kinect1 = KinectSensor.KinectSensors[0];
                    kinect2 = KinectSensor.KinectSensors[1];

                    kinect1.Start();
                    kinect2.Start();

                    kinect1.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    kinect2.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                    /*kinect1.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    kinect1.DepthStream.Range = DepthRange.Near;
                    kinect2.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    kinect2.DepthStream.Range = DepthRange.Near;*/

                    ColorImageStream colorStream1 = kinect1.ColorStream;
                    ColorImageStream colorStream2 = kinect2.ColorStream;

                    pixelesColor1 = new byte[colorStream1.FramePixelDataLength];
                    pixelesColor2 = new byte[colorStream2.FramePixelDataLength];

                    color1Stride = colorStream1.FrameWidth * colorStream1.FrameBytesPerPixel;
                    color2Stride = colorStream2.FrameWidth * colorStream2.FrameBytesPerPixel;

                    rect1Color = new Int32Rect(0, 0, colorStream1.FrameWidth, colorStream1.FrameHeight);
                    rect2Color = new Int32Rect(0, 0, colorStream2.FrameWidth, colorStream2.FrameHeight);

                    bitmap1Color = new WriteableBitmap(colorStream1.FrameWidth, colorStream1.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    bitmap2Color = new WriteableBitmap(colorStream2.FrameWidth, colorStream2.FrameHeight, 96, 96, PixelFormats.Bgr32, null);

                    viewKinect1.Source = bitmap1Color;
                    viewKinect2.Source = bitmap2Color; 
                }
                catch
                {
                    MessageBox.Show("Ocurrio un error al iniciar los dispositivos Kinects.");
                    Application.Current.Shutdown();
                }
            }
            
        }

        private void PollStreamColor()
        {
            try
            {
                using (ColorImageFrame frame1 = kinect1.ColorStream.OpenNextFrame(100), frame2 = kinect2.ColorStream.OpenNextFrame(100))
                {
                    if (frame1 != null)
                    {
                        frame1.CopyPixelDataTo(pixelesColor1);
                        bitmap1Color.WritePixels(rect1Color, pixelesColor1, color1Stride, 0);
                    }
                    if (frame2 != null)
                    {
                        frame2.CopyPixelDataTo(pixelesColor2);
                        bitmap1Color.WritePixels(rect2Color, pixelesColor2, color2Stride, 0); 
                    }
                }
            }
            catch
            {
                MessageBox.Show("No se pueden leer los datos del dispositivo", "Error");
                Application.Current.Shutdown();
            }
        } 

        /*private void PollStreamDepth()
        {

        }*/

        /*void kinect1_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using(DepthImageFrame frameDepth1 = e.OpenDepthImageFrame())
            {
                if (frameDepth1 == null) return;

                if (distanciaKinect1 == null) 
                {
                    distanciaKinect1 = new short[frameDepth1.PixelDataLength];
                }

                if (colorDepth1 == null)
                {
                    colorDepth1= new byte[frameDepth1.PixelDataLength*4];

                }
                
                frameDepth1.CopyPixelDataTo(distanciaKinect1);

                int index = 0; 
                for (int i=0; i<frameDepth1.PixelDataLength; i++)
                {   
                    int valorDist = distanciaKinect1[i] >> 3;

                    if (valorDist == kinect1.DepthStream.UnknownDepth)
                    {
                        colorDepth1[index] = 0;         //B
                        colorDepth1[index+1] = 0;       //G
                        colorDepth1[index + 2] = 255;   //R
                    }
                    else if (valorDist == kinect1.DepthStream.TooFarDepth)
                    {
                        colorDepth1[index] = 255;       //B
                        colorDepth1[index + 1] = 0;     //G
                        colorDepth1[index + 2] = 0;     //R
                    }
                    else
                    {
                        byte byteDistancia = (byte)(255 - (valorDist >> 5));
                        colorDepth1[index] = byteDistancia;         //azul
                        colorDepth1[index+1] = byteDistancia;       //verde
                        colorDepth1[index + 2] = byteDistancia;     //rojo
                    }
            
                    index = index + 4; 
                } 

                if (bitmapDepth1 == null )
                {
                    bitmapDepth1 = new WriteableBitmap(frameDepth1.Width,frameDepth1.Height,96,96,PixelFormats.Bgr32,null);
                }

                bitmapDepth1.WritePixels(new Int32Rect(0, 0, frameDepth1.Width, frameDepth1.Height), colorDepth1, frameDepth1.Width * 4, 0);
                viewDepth1.Source = bitmapDepth1; 
            }
        }*/
        
    }
}
