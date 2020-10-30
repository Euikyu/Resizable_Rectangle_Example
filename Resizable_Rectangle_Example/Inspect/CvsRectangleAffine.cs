using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resizable_Rectangle_Example.Inspect
{

    public class CvsRectangleAffine
    {
        public CvsRectangleAffine()
        {
            // 생성자
            Width = 50;
            Height = 100;
        }

        // float Point 정의
        public struct fPoint
        {
            public float x;
            public float y;
        }

        // 사각형 정보
        public double LeftTopX { get; set; } // 회전하지 않은 사각형의 LeftTop x 좌표
        public double LeftTopY { get; set; } // 회전하지 않은 사각형의 LeftTop y 좌표
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; } // 돌린 각


        // Bitmap Crop
        public System.Drawing.Bitmap GetCropImage(System.Drawing.Bitmap BitmapSrc)
        {
            try
            {
                // Create BitmapDst 
                System.Drawing.Bitmap BitmapDst = new System.Drawing.Bitmap((int)Math.Round(Width), (int)Math.Round(Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Convert to BitmapData
                BitmapData DataSrc = BitmapSrc.LockBits(new System.Drawing.Rectangle(0, 0, BitmapSrc.Width, BitmapSrc.Height), ImageLockMode.ReadWrite, BitmapSrc.PixelFormat);
                BitmapData DataDst = BitmapDst.LockBits(new System.Drawing.Rectangle(0, 0, BitmapDst.Width, BitmapDst.Height), ImageLockMode.ReadWrite, BitmapDst.PixelFormat);

                // 필요 요소 정의
                byte bitsPerPixel = GetBitsPerPixel(BitmapSrc.PixelFormat);
                int SizeSrc = DataSrc.Stride * DataSrc.Height;
                int SizeDst = DataDst.Stride * DataDst.Height;
                byte[] ArraySrc = new byte[SizeSrc];
                byte[] ArrayDst = new byte[SizeDst];

                // Data copy from DataSrc to ArraySrc[]
                System.Runtime.InteropServices.Marshal.Copy(DataSrc.Scan0, ArraySrc, 0, SizeSrc); //  Marshal.Copy로 memcopy마냥 쓸 수 있구먼

                // ArrayDst[] 채우기
                for (int j = 0; j < Math.Round(Height); j++)
                {
                    for (int i = 0; i < Math.Round(Width); i++)
                    {
                        // Pose Matrix (회전 후 이동)
                        fPoint point = PoseMatrix(i, j, (float)LeftTopX, (float)LeftTopY, (float)Angle, (int)Math.Round(Width), (int)Math.Round(Height));
                        int srcx = (int)point.x;
                        int srcy = (int)point.y;
                        ArrayDst[3 * i + 0 + j * DataDst.Stride] = ArraySrc[srcx + srcy * DataSrc.Stride];
                        ArrayDst[3 * i + 1 + j * DataDst.Stride] = ArraySrc[srcx + srcy * DataSrc.Stride];
                        ArrayDst[3 * i + 2 + j * DataDst.Stride] = ArraySrc[srcx + srcy * DataSrc.Stride];
                    }
                }

                // Array에서 BitmapData로 Copy
                System.Runtime.InteropServices.Marshal.Copy(ArrayDst, 0, DataDst.Scan0, ArrayDst.Length);

                BitmapSrc.UnlockBits(DataSrc);
                BitmapDst.UnlockBits(DataDst);
                return BitmapDst;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public byte GetBitsPerPixel(System.Drawing.Imaging.PixelFormat Pixelformat)
        {
            try
            {
                int BitsPerPixel;

                switch (Pixelformat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                        BitsPerPixel = 8;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        BitsPerPixel = 24;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                        BitsPerPixel = 32;
                        break;
                    default:
                        BitsPerPixel = 0;
                        break;
                }

                byte bitsPerPixel = (byte)((float)(BitsPerPixel + 7) / 8);
                return bitsPerPixel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 회전 And 이동 매트릭스 테스트
        static fPoint PoseMatrix(float Model_x, float Model_y, float LeftTopX, float LeftTopY, float Degree, int width, int height)
        {
            try
            {
                // 각도를 라디안으로 변환
                double Radian = Degree * Math.PI / 180;
                float Cos = (float)Math.Cos(Radian);
                float Sin = (float)Math.Sin(Radian);

                // 1. 배열 만들기
                float[] PoseArray = new float[] {
                    Cos, -Sin, LeftTopX + width/2,
                    Sin,  Cos, LeftTopY + height/2,
                      0,    0, 1
                };

                float[] PointArray = new float[] {
                    Model_x - width/2,
                    Model_y - height/2,
                    1
                };

                // 2. 배열로 Mat 만들기
                Mat PoseMat = new Mat(3, 3, MatType.CV_32FC1, PoseArray);
                Mat PointMat = new Mat(3, 1, MatType.CV_32FC1, PointArray);

                // 3. 계산하기
                Mat PosePoint = PoseMat * PointMat;
                float[] Result = new float[3 * 1];
                PosePoint.GetArray(0, 0, Result);

                fPoint fpoint = new fPoint();
                fpoint.x = Result[0]; // x좌표
                fpoint.y = Result[1]; // y좌표

                return fpoint;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }

}
