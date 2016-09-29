using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Drawing;

namespace _HandGestureDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread processingThread;
        private PXCMSenseManager senseManager;
        private PXCMHandModule hand;
        private PXCMHandConfiguration handConfig;
        private PXCMHandData handData;
        private PXCMHandData.GestureData gestureData;
        private bool handWaving;
        private bool handTrigger;
        private int msgTimer;

        public MainWindow()
        {
            InitializeComponent();
            handWaving = false;
            handTrigger = false;
            msgTimer = 0;

            //Instantiate and initialize the SenseManager
            senseManager = PXCMSenseManager.CreateInstance();
            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 30);
            senseManager.EnableHand();
            senseManager.Init();

            //Configure the Hand Module
            hand = senseManager.QueryHand();
            handConfig = hand.CreateActiveConfiguration();
            handConfig.EnableGesture("v_sign");
            handConfig.EnableAllAlerts();
            handConfig.ApplyChanges();



            // Start the worker thread
            processingThread = new Thread(new ThreadStart(ProcessingThread));
            processingThread.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblMessage.Content = "*Peace(V) Sign*";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            processingThread.Abort();
            if (handData != null) handData.Dispose();
            handConfig.Dispose();
            senseManager.Dispose();
        }
        private void ProcessingThread()
        {
            // Start AcquireFrame/ReleaseFrame loop
            while (senseManager.AcquireFrame(true) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMCapture.Sample sample = senseManager.QuerySample();
                Bitmap colorBitmap;
                PXCMImage.ImageData colorData;

                // Get color image data
                sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out colorData);
                colorBitmap = colorData.ToBitmap(0, sample.color.info.width, sample.color.info.height);

                // Retrieve gesture data
                hand = senseManager.QueryHand();

                if (hand != null)
                {
                    // Retrieve the most recent processed data
                    handData = hand.CreateOutput();
                    handData.Update();
                    handWaving = handData.IsGestureFired("v_sign", out gestureData);
                }

                // Update the user interface
                UpdateUI(colorBitmap);

                // Release the frame
                if (handData != null) handData.Dispose();
                colorBitmap.Dispose();
                sample.color.ReleaseAccess(colorData);
                senseManager.ReleaseFrame();
            }
        }

        private void UpdateUI(Bitmap bitmap)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (bitmap != null)
                {
                    // Mirror the color stream Image control
                    imgColorStream.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTransform mainTransform = new ScaleTransform();
                    mainTransform.ScaleX = -1;
                    mainTransform.ScaleY = 1;
                    imgColorStream.RenderTransform = mainTransform;

                    // Display the color stream
                    imgColorStream.Source = ConvertBitmap.BitmapToBitmapSource(bitmap);

                    // Update the screen message
                    if (handWaving)
                    {
                        lblMessage.Content = "Peace Detected!";
                        handTrigger = true;
                    }

                    // Reset the screen message after ~50 frames
                    if (handTrigger)
                    {
                        msgTimer++;

                        if (msgTimer >= 50)
                        {
                            lblMessage.Content = "*Peace(V) Sign*";
                            msgTimer = 0;
                            handTrigger = false;
                        }
                    }
                }
            }));
        }
    }
}

