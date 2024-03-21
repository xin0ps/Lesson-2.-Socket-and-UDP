using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ShareScreen
{
    public partial class MainWindow : Window
    {
        public UdpClient client;
        public IPEndPoint? remoteEP;

        bool isStart = false;
        bool isStoped = false;

        public MainWindow()
        {
            InitializeComponent();

            remoteEP = null;

            IPEndPoint test = new IPEndPoint(IPAddress.Any, 27001);
            client = new UdpClient(test);
          
        }

        private async void StartStopClickEvent(object sender, RoutedEventArgs e)
        {
            if (!isStart)
            {
                isStart = true;
                StartandStopBTN.Content = "Stop";

                // Start broadcasting
                await Task.Run(() => StartBroadcasting());
            }
            else
            {
                isStart = false;
                StartandStopBTN.Content = "Start";
                isStoped = true;
            }
        }

        private async Task StartBroadcasting()
        {
            while (!isStoped)
            {
                try
                {
                    // Capture the screen
                    System.Drawing.Image? image = await TakeScreenShotAsync();
                    byte[]? imageData = await ImageToByteAsync(image);
                    var chunks = imageData?.Chunk(ushort.MaxValue - 29);

                    // Send the chunks
                    foreach (var chunk in chunks)
                    {
                        await client.SendAsync(chunk, chunk.Length, remoteEP);
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        public async Task<System.Drawing.Image?> TakeScreenShotAsync()
        {
            var width = Screen.PrimaryScreen.Bounds.Width;
            var height = Screen.PrimaryScreen.Bounds.Height;

            Bitmap bitmap = new Bitmap(width, height);

            using Graphics grph = Graphics.FromImage(bitmap);

            grph.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            return bitmap;
        }

        public async Task<byte[]> ImageToByteAsync(System.Drawing.Image image)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the image to memoryStream in JPEG format
                image.Save(memoryStream, ImageFormat.Jpeg);

                // Convert the data in memoryStream to a byte array and return
                return memoryStream.ToArray();
            }
        }
    }
}
