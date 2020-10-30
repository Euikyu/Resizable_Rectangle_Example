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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public BitmapSource OriginSource
        {
            get { return m_OriginSource; }
            set
            {
                m_OriginSource = value;
                this.RaisePropertyChanged(nameof(OriginSource));
                if(m_CurrentBitmap != null)
                {
                    ImageWidth = m_CurrentBitmap.Width;
                    ImageHeight = m_CurrentBitmap.Height;
                    this.RaisePropertyChanged(nameof(ImageWidth));
                    this.RaisePropertyChanged(nameof(ImageHeight));
                }
            }
        }

        public CvsRectangleAffine Region
        {
            get { return m_Region; }
            set { m_Region = value; this.RaisePropertyChanged(nameof(Region)); }
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
            Region.LeftTopX = uc.OriginX;
            Region.LeftTopY = uc.OriginY;
            Region.Width = uc.Width;
            Region.Height = uc.Height;
            Region.Angle = uc.Rotation;


            var bmp = Region.GetCropImage(m_CurrentBitmap);
            bmp.Save(@"D:\test1234.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
