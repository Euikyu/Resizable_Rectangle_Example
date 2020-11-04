using Resizable_Rectangle_Example.Inspect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ZoomPanCon;

namespace Resizable_Rectangle_Example
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private System.Drawing.Bitmap m_CurrentBitmap;
        private BitmapSource m_OriginSource;
        private CvsRectangleAffine m_Region;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propName)
        {
            if(PropertyChanged != null)PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public BitmapSource OriginSource
        {
            get { return m_OriginSource; }
            set
            {
                m_OriginSource = value;
                this.RaisePropertyChanged("OriginSource");
                if(m_CurrentBitmap != null)
                {
                    ImageWidth = m_CurrentBitmap.Width;
                    ImageHeight = m_CurrentBitmap.Height;
                    this.RaisePropertyChanged("ImageWidth");
                    this.RaisePropertyChanged("ImageHeight");
                }
            }
        }

        public CvsRectangleAffine Region
        {
            get { return m_Region; }
            set { m_Region = value; this.RaisePropertyChanged("Region"); }
        }

        public double LeftTopX
        {
            get { return m_Region.LeftTopX; }
            set
            {
                m_Region.LeftTopX = (int)value;
                RaisePropertyChanged("LeftTopX");
            }
        }
        public double LeftTopY
        {
            get { return m_Region.LeftTopY; }
            set
            {
                m_Region.LeftTopY = (int)value;
                RaisePropertyChanged("LeftTopY");
            }
        }
        public double RegionWidth
        {
            get { return m_Region.Width; }
            set
            {
                m_Region.Width = (int)value;
                RaisePropertyChanged("RegionWidth");
            }
        }
        public double RegionHeight
        {
            get { return m_Region.Height; }
            set
            {
                m_Region.Height = (int)value;
                RaisePropertyChanged("RegionHeight");
            }
        }
        public double Angle
        {
            get { return m_Region.Angle; }
            set
            {
                m_Region.Angle = (int)value;
                RaisePropertyChanged("Angle");
            }
        }

        public double ImageWidth { get; private set; }
        public double ImageHeight { get; private set; }

        public MainWindow()
        {
            Region = new CvsRectangleAffine();
            InitializeComponent();
            DataContext = this;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Bitmap Image Files (*.bmp)|*.bmp"
            };
            if ((bool)d.ShowDialog())
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(d.FileName);
                if(bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    
                }
                else
                {
                    m_CurrentBitmap = bmp;
                    var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    OriginSource = BitmapSource.Create(data.Width, data.Height, bmp.HorizontalResolution, bmp.VerticalResolution, PixelFormats.Gray8, null, data.Scan0, data.Stride * data.Height, data.Stride);
                    OriginSource.Freeze();
                    bmp.UnlockBits(data);
                }

                uc.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var bmp = Region.GetCropImage(m_CurrentBitmap);
            bmp.Save(@"D:\test1234.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }


        #region - Zoompan Control
        /*
         * * memberVar
         */
        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton mouseButtonDown;

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;
        /*
         * * property
         */

        /*
         * * method
         */
        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale * 1.2, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale / 1.2, contentZoomCenter);
        }
        /*
         * Callback
         */
        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        private void zoomAndPanControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(ImgCanvas);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(ImgCanvas);
                ZoomOut(curContentMousePoint);
            }

        }
        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImgCanvas.Focus();

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomAndPanControl);
            origContentMouseDownPoint = e.GetPosition(ImgCanvas);

            if (mouseButtonDown == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                zoomAndPanControl.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                if (mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (mouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the ImgCanvas.
                        ZoomIn(origContentMouseDownPoint);
                    }
                    else if (mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the ImgCanvas.
                        ZoomOut(origContentMouseDownPoint);
                    }
                }
                zoomAndPanControl.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
        {
            //shseol85: overlay랑 충돌
            //Color c = GetPixelColor(e.GetPosition(ImgCanvas));
            //PixelInfo = string.Format("{0}", c.B);

            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(ImgCanvas);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                zoomAndPanControl.ContentOffsetX -= dragOffset.X;
                zoomAndPanControl.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.Zooming)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (mouseButtonDown == MouseButton.Left && (Math.Abs(dragOffset.X) > dragThreshold ||
                                                            Math.Abs(dragOffset.Y) > dragThreshold))
                {
                    //
                    // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                    // initiate drag zooming mode where the user can drag out a rectangle to select the area
                    // to zoom in on.
                    //
                    mouseHandlingMode = MouseHandlingMode.DragZooming;
                    Point curContentMousePoint = e.GetPosition(ImgCanvas);
                    //InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint); //LDH9999 추후
                }

                e.Handled = true;
            }
            /*LDH9999 추후
            else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                //
                // When in drag zooming mode continously update the position of the rectangle
                // that the user is dragging out.
                //
                Point curContentMousePoint = e.GetPosition(content);
                SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

                e.Handled = true;
            }*/
        }
        #endregion
    }





}
