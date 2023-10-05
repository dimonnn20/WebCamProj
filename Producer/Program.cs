using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Producer
{
    internal class Program
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static IPEndPoint consumerEndPoint;
        private static UdpClient udpClient = new UdpClient();
        static void Main(string[] args)
        {
            var consumerIp = ConfigurationManager.AppSettings.Get("consumerIp");
            var consumerPort = ConfigurationManager.AppSettings.Get("consumerPort");
            //consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), consumerPort);
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), Convert.ToInt32(consumerPort));

            Console.WriteLine($"consumer: {consumerEndPoint}");

            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            // videoSource.VideoResolution = videoSource.VideoCapabilities(); Изменение разрешения картинки с веб камеры
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            Console.WriteLine("\n Press Enter to hide the console ...");

            Console.ReadLine();
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            
            var bmp = new Bitmap(eventArgs.Frame, 800, 600);
            try 
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    udpClient.Send(bytes, bytes.Length, consumerEndPoint);
                }
            } catch (Exception ex)
            {
                
            }
        }
    }
}
