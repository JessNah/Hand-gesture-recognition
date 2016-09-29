using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace _HandGestureDetection
{
    class ConvertBitmap
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr handle);
        public static BitmapSource bitmapSource;
        public static IntPtr intPointer;
        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            intPointer = bitmap.GetHbitmap();
            bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPointer,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(intPointer);
            return bitmapSource;
        }
    }
}
