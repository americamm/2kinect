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
        int depth1Stride; 
        int depth2Stride;
        short[] pixelesDepth1;
        short[] pixelesDepth2;
        byte[] pixelesColor1;
        byte[] pixelesColor2; 
        byte[] colorDepth1; 
        byte[] colorDepth2; 
        Int32Rect rect1Color;
        Int32Rect rect2Color;
        Int32Rect rect1Depth;
        Int32Rect rect2Depth; 
        WriteableBitmap bitmap1Color;
        WriteableBitmap bitmap2Color;
        WriteableBitmap bitmap1Depth;
        WriteableBitmap bitmap2Depth; 


        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            InicializaKinects();
            PollStreamColor();
            PollStreamDepth();
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
                    kinect1.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    kinect1.DepthStream.Range = DepthRange.Near;
                    kinect2.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    kinect2.DepthStream.Range = DepthRange.Near;

                    ColorImageStream colorStream1 = kinect1.ColorStream;
                    ColorImageStream colorStream2 = kinect2.ColorStream;
                    DepthImageStream depthStream1 = kinect1.DepthStream;
                    DepthImageStream depthStream2 = kinect2.DepthStream; 

                    pixelesColor1 = new byte[colorStream1.FramePixelDataLength];
                    pixelesColor2 = new byte[colorStream2.FramePixelDataLength];
                    pixelesDepth1 = new short[depthStream1.FramePixelDataLength];
                    pixelesDepth2 = new short[depthStream2.FramePixelDataLength];
                    colorDepth1 = new byte[depthStream1.FramePixelDataLength * 4];
                    colorDepth2 = new byte[depthStream2.FramePixelDataLength * 4]; 

                    color1Stride = colorStream1.FrameWidth * colorStream1.FrameBytesPerPixel;
                    color2Stride = colorStream2.FrameWidth * colorStream2.FrameBytesPerPixel;
                    depth1Stride = depthStream1.FrameWidth * 4;
                    depth2Stride = depthStream2.FrameWidth * 4; 
                    
                    rect1Color = new Int32Rect(0, 0, colorStream1.FrameWidth, colorStream1.FrameHeight);
                    rect2Color = new Int32Rect(0, 0, colorStream2.FrameWidth, colorStream2.FrameHeight);
                    rect1Depth = new Int32Rect(0, 0, depthStream1.FrameWidth, depthStream1.FrameHeight);
                    rect2Depth = new Int32Rect(0, 0, depthStream2.FrameWidth, depthStream2.FrameHeight); 

                    bitmap1Color = new WriteableBitmap(colorStream1.FrameWidth, colorStream1.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    bitmap2Color = new WriteableBitmap(colorStream2.FrameWidth, colorStream2.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    bitmap1Depth = new WriteableBitmap(depthStream1.FrameWidth, depthStream1.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    bitmap2Depth = new WriteableBitmap(depthStream2.FrameWidth, depthStream2.FrameHeight, 96, 96, PixelFormats.Bgr32, null); 

                    viewKinect1.Source = bitmap1Color;
                    viewKinect2.Source = bitmap2Color;
                    viewDepth1.Source = bitmap1Depth;
                    viewDepth2.Source = bitmap2Depth;
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
                        bitmap2Color.WritePixels(rect2Color, pixelesColor2, color2Stride, 0); 
                    }
                }
            }
            catch
            {
                MessageBox.Show("No se pueden leer los datos del dispositivo", "Error");
                Application.Current.Shutdown();
            }
        } 

        private void PollStreamDepth()
        {
            try
            {
                using (DepthImageFrame frame1 = kinect1.DepthStream.OpenNextFrame(100), frame2 = kinect2.DepthStream.OpenNextFrame(100))
                {
                    if (frame1 != null)
                    {
                        frame1.CopyPixelDataTo(pixelesDepth1);

                        int index = 0;
                        for (int i = 0; i < frame1.PixelDataLength; i++)
                        {
                            int valorDist = pixelesDepth1[i] >> 3;

                            if (valorDist == kinect1.DepthStream.UnknownDepth)
                            {
                                colorDepth1[index] = 0;           //B
                                colorDepth1[index + 1] = 0;       //G
                                colorDepth1[index + 2] = 255;     //R
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
                                colorDepth1[index + 1] = byteDistancia;       //verde
                                colorDepth1[index + 2] = byteDistancia;     //rojo
                            }

                            index = index + 4;
                        }

                        bitmap1Depth.WritePixels(rect1Depth, colorDepth1, depth1Stride, 0); 
                    }

                    if (frame2 != null)
                    {
                        frame2.CopyPixelDataTo(pixelesDepth2);
                        
                        int index = 0;
                        for (int i = 0; i < frame2.PixelDataLength; i++)
                        {
                            int valorDist = pixelesDepth2[i] >> 3;

                            if (valorDist == kinect2.DepthStream.UnknownDepth)
                            {
                                colorDepth2[index] = 0;         //B
                                colorDepth2[index + 1] = 0;       //G
                                colorDepth2[index + 2] = 255;   //R
                            }
                            else if (valorDist == kinect2.DepthStream.TooFarDepth)
                            {
                                colorDepth2[index] = 255;       //B
                                colorDepth2[index + 1] = 0;     //G
                                colorDepth2[index + 2] = 0;     //R
                            }
                            else
                            {
                                byte byteDistancia = (byte)(255 - (valorDist >> 5));
                                colorDepth2[index] = byteDistancia;         //azul
                                colorDepth2[index + 1] = byteDistancia;       //verde
                                colorDepth2[index + 2] = byteDistancia;     //rojo
                            }

                            index = index + 4; 
                        }

                        bitmap2Depth.WritePixels(rect2Depth, colorDepth2, depth2Stride, 0); 
                    }
                }
            }
            catch 
            {
                MessageBox.Show("Los datos del dispositivo no han sido captados", "Error");
            }
        }

        
    }
}
