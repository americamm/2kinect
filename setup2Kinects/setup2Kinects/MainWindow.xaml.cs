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
        
        KinectSensor kinect1;
        KinectSensor kinect2; 
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count == 0) 
            {
                MessageBox.Show("No se ha detectado ningun kinect.", "Error");
                Application.Current.Shutdown(); 
            }

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

                kinect1.AllFramesReady += kinect1_AllFramesReady;
                kinect2.AllFramesReady += kinect2_AllFramesReady;
           
            }
            catch
            {
                MessageBox.Show("Ocurrio un error al iniciar los dispositivos Kinects.");
                Application.Current.Shutdown(); 
            }
            
        }

        
        byte[] pixelesKinect1 = null;       //Arreglo donde se guardan los pixeles de los datos de color que proporciona el stream.
        byte[] pixelesKinect2 = null;

        short[] distanciaKinect1 = null;    //Arreglo donde se guardan los datos de profundidad que proporciona el stream.
        short[] distanciaKinect2 = null;

        byte[] colorDepth1 = null;          //Arreglo para convertir las distancias a color. 
        byte[] colorDepth2 = null; 

        WriteableBitmap bitmapKinect1 = null;       //Bitmap para mostrar la imagen a color.
        WriteableBitmap bitmapKinect2 = null;

        WriteableBitmap bitmapDepth1 = null;        //Bitmap para mostrar la profundidad con colorcitos. 
        WriteableBitmap bitmapDepth2 = null; 


        void kinect1_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame frameKinect1 = e.OpenColorImageFrame())
            {
                if (frameKinect1 == null) return;

                pixelesKinect1 = new byte[frameKinect1.PixelDataLength];
                frameKinect1.CopyPixelDataTo(pixelesKinect1);

                if (bitmapKinect1 == null)
                { 
                    bitmapKinect1 = new WriteableBitmap(frameKinect1.Width, frameKinect1.Height, 96, 96, PixelFormats.Bgr32, null);
                }

                bitmapKinect1.WritePixels(new Int32Rect(0, 0, frameKinect1.Width, frameKinect1.Height), pixelesKinect1, frameKinect1.Width * frameKinect1.BytesPerPixel, 0);
                viewKinect1.Source = bitmapKinect1; 
            } 

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
        }
        
        
        void kinect2_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame frameKinect2 = e.OpenColorImageFrame())
            {
                if (frameKinect2 == null) return;

                pixelesKinect2 = new byte[frameKinect2.PixelDataLength]; 
                frameKinect2.CopyPixelDataTo(pixelesKinect2);

                if (bitmapKinect2 == null)
                {
                    bitmapKinect2 = new WriteableBitmap(frameKinect2.Width, frameKinect2.Height, 96, 96, PixelFormats.Bgr32, null);
                }

                bitmapKinect2.WritePixels(new Int32Rect(0,0,frameKinect2.Width,frameKinect2.Height),pixelesKinect2,frameKinect2.Width*frameKinect2.BytesPerPixel,0); 
                viewKinect2.Source = bitmapKinect2; 
            }

            using (DepthImageFrame frameDepth2 = e.OpenDepthImageFrame())
            {
                if (frameDepth2 == null) return;
                
                if (distanciaKinect2 == null)
                {
                    distanciaKinect2 = new short[frameDepth2.PixelDataLength];
                }
                
                if (colorDepth2 == null)
                {
                    colorDepth2 = new byte[frameDepth2.PixelDataLength * 4];
                } 

                frameDepth2.CopyPixelDataTo(distanciaKinect2);

                int index = 0;
                for (int i = 0; i < frameDepth2.PixelDataLength; i++)
                {
                    int valorDist = distanciaKinect2[i] >> 3;

                    if (valorDist == kinect2.DepthStream.UnknownDepth)
                    {
                        colorDepth2[index] = 0;         //B
                        colorDepth2[index + 1] = 0;     //G
                        colorDepth2[index + 2] = 255;   //R

                    } 
                    else if (valorDist == kinect2.DepthStream.TooFarDepth)
                    {
                        colorDepth2[index] = 255;           //B
                        colorDepth2[index + 1] = 0;         //G
                        colorDepth2[index + 2] = 0;         //R
                    }
                    else
                    {
                        byte byteDistancia = (byte)(255 - (valorDist >> 5));
                        colorDepth2[index] = byteDistancia;         //B
                        colorDepth2[index + 1] = byteDistancia;     //G
                        colorDepth2[index + 2] = byteDistancia;     //R
                    }
              
                    index = index + 4;
                }

                if (bitmapDepth2 == null)
                {
                    bitmapDepth2 = new WriteableBitmap(frameDepth2.Width, frameDepth2.Height, 96, 96, PixelFormats.Bgr32, null);
                }

                bitmapDepth2.WritePixels(new Int32Rect(0, 0, frameDepth2.Width, frameDepth2.Height), colorDepth2, frameDepth2.Width * 4, 0);
                viewDepth2.Source = bitmapDepth2; 

            }
            
        }

    }
}
