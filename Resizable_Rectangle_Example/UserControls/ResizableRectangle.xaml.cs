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

namespace Resizable_Rectangle_Example.UserControls
{
    /// <summary>
    /// ResizableRectangle.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ResizableRectangle : UserControl
    {
        #region Fields
        private const double NW_DEFAULT_RADIAN = -135 * (Math.PI / 180);
        private const double NE_DEFAULT_RADIAN = -45 * (Math.PI / 180);
        private const double SW_DEFAULT_RADIAN = 135 * (Math.PI / 180);
        private const double SE_DEFAULT_RADIAN = 45 * (Math.PI / 180);

        private readonly object m_MoveLock = new object();

        private bool m_IsCaptured;
        private Point m_LastMovePoint;
        private Line m_RotationLine;
        private double m_Radian;

        #endregion

        #region Properties

        #region Common Properties

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty OriginXProperty =
            DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(ResizableRectangle));

        public static readonly DependencyProperty OriginYProperty =                
            DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(ResizableRectangle));

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(nameof(Rotation), typeof(double), typeof(ResizableRectangle));

        public double OriginX
        {
            get { return (double)GetValue(OriginXProperty); }
            set { SetValue(OriginXProperty, value); }
        }

        public double OriginY
        {
            get { return (double)GetValue(OriginYProperty); }
            set { SetValue(OriginYProperty, value); }
        }

        public double Rotation
        {
            get { return (double)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }
        #endregion

        #endregion

        public ResizableRectangle()
        {
            InitializeComponent();
        }

        #region Methods

        #endregion

        #region Events
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
            if(element != null)
            {
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
                Parent_Grid.Children.Remove(m_RotationLine);
                m_RotationLine = null;
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
                        switch (control.Name)
                        {
                            case "Size_NW":
                                Vector nwSizeOffset = e.GetPosition(canvas) - new Point(this.OriginX + 20, this.OriginY + 20); //this.GetPointByRotation(this.OriginX + 20, this.OriginY + 20);
                                if (this.OriginX + nwSizeOffset.X > 0 && this.Width - nwSizeOffset.X > this.MinWidth)
                                {
                                    this.OriginX += nwSizeOffset.X;
                                    this.Width -= nwSizeOffset.X;
                                }
                                if (this.OriginY + nwSizeOffset.Y > 0 && this.Height - nwSizeOffset.Y > this.MinHeight)
                                {
                                    this.OriginY += nwSizeOffset.Y;
                                    this.Height -= nwSizeOffset.Y;
                                }
                                break;
                            case "Size_NE":
                                Vector neSizeOffset = e.GetPosition(canvas) - new Point(this.Width + this.OriginX + 10, this.OriginY + 20);
                                if (this.Width + neSizeOffset.X > this.MinWidth)
                                {
                                    this.Width += neSizeOffset.X;
                                }
                                if (this.OriginY + neSizeOffset.Y > 0 && this.Height - neSizeOffset.Y > this.MinHeight)
                                {
                                    this.OriginY += neSizeOffset.Y;
                                    this.Height -= neSizeOffset.Y;
                                }

                                break;
                            case "Size_SW":
                                Vector swSizeOffset = e.GetPosition(canvas) - new Point(this.OriginX + 20, this.Height + this.OriginY + 10);
                                if (this.OriginX + swSizeOffset.X > 0 && this.Width - swSizeOffset.X > this.MinWidth)
                                {
                                    this.OriginX += swSizeOffset.X;
                                    this.Width -= swSizeOffset.X;
                                }
                                if (this.Height + swSizeOffset.Y > this.MinHeight)
                                {
                                    this.Height += swSizeOffset.Y;
                                }
                                break;
                            case "Size_SE":
                                Vector seSizeOffset = e.GetPosition(canvas) - new Point(this.Width + this.OriginX + 10, this.Height + this.OriginY + 10); //this.GetPointByRotation(this.Width + this.OriginX + 10, this.Height + this.OriginY + 10);
                                if (this.Width + seSizeOffset.X > this.MinWidth) this.Width += seSizeOffset.X;
                                if (this.Height + seSizeOffset.Y > this.MinHeight) this.Height += seSizeOffset.Y;
                                break;
                            case "Movable_Grid":
                                if(m_LastMovePoint.X == 0 && m_LastMovePoint.Y == 0)
                                {
                                    m_LastMovePoint = e.GetPosition(canvas);
                                    break;
                                }
                                Vector moveOffset = e.GetPosition(canvas) - m_LastMovePoint;
                                if (this.OriginX + moveOffset.X > 0) this.OriginX += moveOffset.X;
                                if (this.OriginY + moveOffset.Y > 0) this.OriginY += moveOffset.Y;
                                m_LastMovePoint = e.GetPosition(canvas);
                                break;
                            case "Rotate_Grid":
                                //if(m_RotationLine == null)
                                //{
                                //    m_RotationLine = new Line {
                                //        X1 = this.Width / 2,
                                //        Y1 = this.Height / 2,
                                //        X2 = e.GetPosition(canvas).X,
                                //        Y2 = e.GetPosition(canvas).Y,
                                //        Stroke = Brushes.Red,
                                //        StrokeDashArray = DoubleCollection.Parse("4,3")
                                //    };
                                //    Parent_Grid.Children.Add(m_RotationLine);
                                //    break;
                                //}
                                //m_RotationLine.X2 = e.GetPosition(canvas).X;
                                //m_RotationLine.Y2 = e.GetPosition(canvas).Y;
                                //m_Radian = Math.Atan2(m_RotationLine.Y2, m_RotationLine.X2);
                                //Rotation = m_Radian * (180 / Math.PI);
                                break;
                        }
                    }
                }
            }
        }

        private Point GetPointByRotation(double x, double y)
        {
            return new Point(Math.Cos(m_Radian) * x - Math.Sin(m_Radian) * y, Math.Sin(m_Radian) * x + Math.Cos(m_Radian) * y);
        }

        #endregion

    }
}
