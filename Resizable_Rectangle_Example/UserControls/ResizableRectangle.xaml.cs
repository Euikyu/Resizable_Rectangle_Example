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

namespace Resizable_Rectangle_Example.UserControls
{
    /// <summary>
    /// ResizableRectangle.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ResizableRectangle : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private const double NW_DEFAULT_RADIAN = -135 * (Math.PI / 180);
        private const double NE_DEFAULT_RADIAN = -45 * (Math.PI / 180);
        private const double SW_DEFAULT_RADIAN = 135 * (Math.PI / 180);
        private const double SE_DEFAULT_RADIAN = 45 * (Math.PI / 180);

        private readonly object m_MoveLock = new object();

        private bool m_IsCaptured;
        private Point m_LastMovePoint;
        private Point m_LastSizePoint;
        private double m_Radian;
        
        private RotateTransform m_RectRotateTransform;
        private double m_RectOriginX;
        private double m_RectOriginY;
        private double m_RectWidth;
        private double m_RectHeight;
        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #region Common Properties

        public RotateTransform RectRotateTransform
        {
            get { return m_RectRotateTransform; }
            set
            {
                m_RectRotateTransform = value;
                RaisePropertyChanged(nameof(RectRotateTransform));
            }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty OriginXProperty =
            DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(ResizableRectangle));
        
        public static readonly DependencyProperty OriginYProperty =                
            DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(ResizableRectangle));

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(nameof(Rotation), typeof(double), typeof(ResizableRectangle));

        public static readonly DependencyProperty RadianProperty =
            DependencyProperty.Register(nameof(Radian), typeof(double), typeof(ResizableRectangle));


        public double OriginX
        {
            get { return (double)GetValue(OriginXProperty); }
            set
            {
                SetValue(OriginXProperty, value);
            }
        }

        public double OriginY
        {
            get { return (double)GetValue(OriginYProperty); }
            set { SetValue(OriginYProperty, value); }
        }

        public double Rotation
        {
            get { return (double)GetValue(RotationProperty); }
            set
            {
                m_Radian = value * (Math.PI / 180);
                SetValue(RadianProperty, m_Radian);
                if (RectRotateTransform is RotateTransform t)
                {
                    t.Angle = value;
                }
                else
                {
                    RectRotateTransform = new RotateTransform(value, this.Width / 2, this.Height / 2);
                }
                RaisePropertyChanged(nameof(RectRotateTransform));
                SetValue(RotationProperty, value);
            }
        }

        public double Radian
        {
            get { return (double)GetValue(RadianProperty); }
            set
            {
                SetValue(RadianProperty, value);
                m_Radian = value;
                var deg = value * (180 / Math.PI);
                if (RectRotateTransform is RotateTransform t)
                {
                    t.Angle = deg;
                }
                else
                {
                    RectRotateTransform = new RotateTransform(deg, this.Width / 2, this.Height / 2);
                }
                RaisePropertyChanged(nameof(RectRotateTransform));
                SetValue(RotationProperty, deg);
            }
        }
        #endregion

        #endregion

        public ResizableRectangle()
        {
            InitializeComponent();
        }

        #region Methods
        private void UpdateRect()
        {
            m_RectWidth = this.Width;
            m_RectHeight = this.Height;
            m_RectOriginX = this.OriginX;
            m_RectOriginY = this.OriginY;

            RectRotateTransform = new RotateTransform(Rotation, this.Width / 2, this.Height / 2);
        }
        private Point GetCenter()
        {
            return new Point(this.OriginX + this.Width / 2, this.OriginY + this.Height / 2);
        }

        private Point GetPointByRotation(Point rotatePoint, double rad, Point center)
        {
            return new Point(Math.Cos(rad) * rotatePoint.X - Math.Sin(rad) * rotatePoint.Y - center.X, Math.Sin(rad) * rotatePoint.X + Math.Cos(rad) * rotatePoint.Y - center.Y);
        }
        #endregion

        #region Events
        private void ResizableRectangle_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateRect();
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            var control = sender as FrameworkElement;
            if(control != null)
            {
                switch (control.Name)
                {
                    case "Size_NW":
                    case "Size_NE":
                    case "Size_SW":
                    case "Size_SE":
                        this.Cursor = Cursors.Cross;
                        break;
                    case "Movable_Grid":
                        this.Cursor = Cursors.SizeAll;
                        break;
                    case "Rotate_Grid":
                        this.Cursor = ((FrameworkElement)this.Resources["Rotate_Cursor"]).Cursor;
                        break;
                }
            }
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            var control = sender as FrameworkElement;
            if (control != null)
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as IInputElement;
            if (element != null && this.Parent != null && this.Parent is Canvas canvas)
            {
                m_LastSizePoint = e.GetPosition(canvas);
                if (RectRotateTransform == null) this.UpdateRect();
                element.CaptureMouse();
                m_IsCaptured = true;
            }
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lock (m_MoveLock)
            {
                Mouse.Capture(null);
                m_IsCaptured = false;
                m_LastMovePoint = new Point();
                m_LastSizePoint = new Point();
                if (this.Width != m_RectWidth || this.Height != m_RectHeight)
                {
                    var centerOffset = this.GetCenter() - new Point(m_RectOriginX + m_RectWidth / 2, m_RectOriginY + m_RectHeight / 2);
                    var destCenter = this.GetPointByRotation(new Point(centerOffset.X, centerOffset.Y), m_Radian, new Point()) - new Point(-(m_RectOriginX + m_RectWidth / 2), -(m_RectOriginY + m_RectHeight / 2));
                    this.OriginX = destCenter.X - this.Width / 2;
                    this.OriginY = destCenter.Y - this.Height / 2;
                }
                this.UpdateRect();
            }
        }

        private void Retangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsCaptured)
            {
                lock (m_MoveLock)
                {
                    var control = sender as FrameworkElement;

                    if (control != null && this.Parent != null && this.Parent is Canvas canvas)
                    {
                        e.Handled = true;

                        Vector sizeOffset = e.GetPosition(canvas) - m_LastSizePoint;
                        Vector deltaSize = this.GetPointByRotation(new Point(sizeOffset.X, sizeOffset.Y), -m_Radian, new Point(RectRotateTransform.CenterX, RectRotateTransform.CenterY)) - new Point(-RectRotateTransform.CenterX, -RectRotateTransform.CenterY);
                        switch (control.Name)
                        {
                            case "Size_NW":
                                RectRotateTransform = new RotateTransform(Rotation, this.Width > this.MinWidth ? m_RectWidth / 2 - deltaSize.X : this.MinWidth / 2 - m_RectWidth / 2, this.Height > this.MinHeight ? m_RectHeight / 2 - deltaSize.Y : this.MinHeight / 2 - m_RectHeight / 2);
                                if (m_RectWidth - deltaSize.X > this.MinWidth)
                                {
                                    this.OriginX = m_RectOriginX + deltaSize.X;
                                    this.Width = m_RectWidth - deltaSize.X;
                                }
                                else
                                {
                                    this.Width = this.MinWidth;
                                }
                                if (m_RectHeight - deltaSize.Y > this.MinHeight)
                                {
                                    this.OriginY = m_RectOriginY + deltaSize.Y;
                                    this.Height = m_RectHeight - deltaSize.Y;
                                }
                                else
                                {
                                    this.Height = this.MinHeight;
                                }
                                break;
                            case "Size_NE":
                                RectRotateTransform = new RotateTransform(Rotation, m_RectWidth / 2, this.Height > this.MinHeight ? m_RectHeight / 2 - deltaSize.Y : this.MinHeight / 2 - m_RectHeight / 2);
                                if (m_RectWidth + deltaSize.X > this.MinWidth)
                                {
                                    this.Width = m_RectWidth + deltaSize.X;
                                }
                                else
                                {
                                    this.Width = this.MinWidth;
                                }
                                if (m_RectHeight - deltaSize.Y > this.MinHeight)
                                {
                                    this.OriginY = m_RectOriginY + deltaSize.Y;
                                    this.Height = m_RectHeight - deltaSize.Y;
                                }
                                else
                                {
                                    this.Height = this.MinHeight;
                                }
                                break;
                            case "Size_SW":
                                RectRotateTransform = new RotateTransform(Rotation, this.Width > this.MinWidth ? m_RectWidth / 2 - deltaSize.X : this.MinWidth / 2 - m_RectWidth / 2, m_RectHeight / 2);
                                if (m_RectWidth - deltaSize.X > this.MinWidth)
                                {
                                    this.OriginX = m_RectOriginX + deltaSize.X;
                                    this.Width = m_RectWidth - deltaSize.X;
                                }
                                else
                                {
                                    this.Width = this.MinWidth;
                                }
                                if (m_RectHeight + deltaSize.Y > this.MinHeight)
                                {
                                    this.Height = m_RectHeight + deltaSize.Y;
                                }
                                else
                                {
                                    this.Height = this.MinHeight;
                                }
                                break;
                            case "Size_SE":
                                RectRotateTransform = new RotateTransform(Rotation, m_RectWidth / 2, m_RectHeight / 2);
                                if (m_RectWidth + deltaSize.X > this.MinWidth) this.Width = m_RectWidth + deltaSize.X;
                                else this.Width = this.MinWidth;
                                if (m_RectHeight + deltaSize.Y > this.MinHeight) this.Height = m_RectHeight + deltaSize.Y;
                                else this.Height = this.MinHeight;
                                break;

                            case "Movable_Grid":
                                if (m_LastMovePoint.X == 0 && m_LastMovePoint.Y == 0)
                                {
                                    m_LastMovePoint = e.GetPosition(canvas);
                                    break;
                                }
                                Vector moveOffset = e.GetPosition(canvas) - m_LastMovePoint;
                                this.OriginX += moveOffset.X;
                                this.OriginY += moveOffset.Y;
                                m_LastMovePoint = e.GetPosition(canvas);
                                break;
                            case "Rotate_Grid":
                                Radian = Math.Atan2(e.GetPosition(canvas).Y - GetCenter().Y, e.GetPosition(canvas).X - GetCenter().X);
                                break;
                        }
                    }
                }
            }
        }

        #endregion

    }
}
