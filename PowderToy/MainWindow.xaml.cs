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

namespace PowderToy
{


    public enum Types
    {
        Space = 0,
        Sand, 
        Water
    }
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap wb;
        private Types currentType;
        private byte[] ColorData;
        private int Width = 256;
        public MainWindow()
        {
            InitializeComponent();
            ColorData = new byte[4];
            currentType = Types.Sand;
            
            wb = new WriteableBitmap(Width,
                Width, 72, 72, PixelFormats.Bgr32, null);
            
            Field.Source = wb;
            Field.Height = Width * 2.5;
            Field.Width = Width * 2.5;
            
            Field.MouseLeftButtonDown += async (sender, args) =>
            {
                //var location = Dispatcher.Invoke(() => Mouse.GetPosition(Field));
                
                await Task.Run(() =>
                {
                    bool flag;
                    flag = Dispatcher.Invoke(() => Mouse.LeftButton == MouseButtonState.Pressed);
                    while (flag)
                    {
                        var location = Dispatcher.Invoke(() => Mouse.GetPosition(Field));

                        //BGR
                        if (currentType == Types.Sand)
                        {
                            ColorData[0] = 61;
                            ColorData[1] = 129;
                            ColorData[2] = 245;
                        }
                        if (currentType == Types.Water)
                        {
                            ColorData[0] = 128;
                            ColorData[1] = 71;
                            ColorData[2] = 26;
                        }
                        int X = Convert.ToInt32(location.X / 2.5);
                        if (X < 0)
                            X = Width - 1;
                        if (X > Width - 1)
                            X = 0;
                        int Y = Convert.ToInt32(location.Y / 2.5);
                        Int32Rect rect = new Int32Rect(X, Y, 1, 1);
                        
                        // Записать 4 байта из массива в растровое изображение.
                        Dispatcher.Invoke(() => wb.WritePixels(rect, ColorData, 4, 0));
                        flag = Dispatcher.Invoke(() => Mouse.LeftButton == MouseButtonState.Pressed);
                    }
                });


                    
                //MessageBox.Show(location.X + " " + location.Y);
            };

            Field.MouseRightButtonDown += (sender, args) =>
            {
                if (currentType == Types.Water)
                    currentType = Types.Sand;
                
                else if (currentType == Types.Sand)
                    currentType++;
               
            };
            
            StartSim();
        }

        async void StartSim()
        {
            await Task.Run(Cycle);
        }

        async void Cycle()
        {
            
            byte[] field = new byte[Width * Width * 4];
            while (true)
            {
                var pixels = GetPixels().Result;
                for(int i = 1; i < Width - 1; ++i)
                for (int j = 1; j < Width - 1; ++j)
                {
                    if (GetTypeOfPixel(pixels, i, j) == Types.Sand)
                    {
                        if (GetTypeOfPixel(pixels, i + 1, j) == Types.Sand || GetTypeOfPixel(pixels, i + 1, j) == Types.Water)
                        {
                            if (GetTypeOfPixel(pixels, i + 1, j - 1) == Types.Space || GetTypeOfPixel(pixels, i + 1, j - 1) == Types.Water)
                            {
                                ChangePixelAt(ref field, i + 1, j - 1, Types.Sand);
                                ChangePixelAt(ref field, i, j, Types.Space);
                            }
                            

                            else if (GetTypeOfPixel(pixels, i + 1, j + 1) == Types.Space || GetTypeOfPixel(pixels, i + 1, j + 1) == Types.Water)
                            {
                                ChangePixelAt(ref field, i + 1, j + 1, Types.Sand);
                                ChangePixelAt(ref field, i, j, Types.Space);
                            }
                        }
                        else
                        {
                            ChangePixelAt(ref field, i, j, Types.Space);
                            ChangePixelAt(ref field, i + 1, j, Types.Sand);
                        }

                    }
                    else if(GetTypeOfPixel(pixels, i, j) == Types.Water)
                    {

                        if (GetTypeOfPixel(pixels, i + 1, j) == Types.Space)
                        {
                            ChangePixelAt(ref field, i + 1, j, Types.Water);
                            ChangePixelAt(ref field, i, j, Types.Space);
                        }

                        else if (GetTypeOfPixel(pixels, i + 1, j - 1) == Types.Space)
                        {
                            ChangePixelAt(ref field, i + 1, j - 1, Types.Water);
                            ChangePixelAt(ref field, i, j, Types.Space);
                        }

                        else if (GetTypeOfPixel(pixels, i + 1, j + 1) == Types.Space)
                        {
                            ChangePixelAt(ref field, i + 1, j + 1, Types.Water);
                            ChangePixelAt(ref field, i, j, Types.Space);
                        }
                        else if (GetTypeOfPixel(pixels, i, j - 1) == Types.Space)
                        {
                            ChangePixelAt(ref field, i, j - 1, Types.Water);
                            ChangePixelAt(ref field, i, j, Types.Space);
                        }
                        
                        
                        else if (GetTypeOfPixel(pixels, i, j + 1) == Types.Space && GetTypeOfPixel(pixels, i + 1, j) != Types.Water)
                        {
                            ChangePixelAt(ref field, i, j + 1, Types.Water);
                            ChangePixelAt(ref field, i, j, Types.Space);
                        }
                       
                       
                        

                    }
                }


                PutPixels(field);

                await Task.Delay(5);
            }
        }
        async void PutPixels(byte[] pixels)
        {
            await Task.Run(() => Dispatcher.Invoke(() => wb.WritePixels(new Int32Rect(0, 0, Width, Width), pixels, Width * 4, 0)));
        }
        
        async Task <byte[]> GetPixels()
        {
            byte[] result = new byte[Width * Width * 4];
            await Task.Run(() => Dispatcher.Invoke(() => wb.CopyPixels(result, Width * 4, 0)));
            return result;
        }

        Types GetTypeOfPixel(byte[] pixels, int i, int j)
        {
            if (pixels[(i * Width + j) * 4] == 61 && pixels[(i * Width + j) * 4 + 1] == 129 &&
                pixels[(i * Width + j) * 4 + 2] == 245)
                return Types.Sand;
            if (pixels[(i * Width + j) * 4] == 128 && pixels[(i * Width + j) * 4 + 1] == 71 &&
                pixels[(i * Width + j) * 4 + 2] == 26)
                return Types.Water;
            return Types.Space;
        }

        void ChangePixelAt(ref byte[] pixels, int i, int j, Types types)
        {
            if (types == Types.Space)
            {
                pixels[(i * Width + j) * 4] = 0;
                pixels[(i * Width + j) * 4 + 1] = 0;
                pixels[(i * Width + j) * 4 + 2] = 0;
            }

            else if (types == Types.Sand)
            {
                pixels[(i * Width + j) * 4] = 61;
                pixels[(i * Width + j) * 4 + 1] = 129;
                pixels[(i * Width + j) * 4 + 2] = 245;
            }
            else if (types == Types.Water)
            {
                pixels[(i * Width + j) * 4] = 128;
                pixels[(i * Width + j) * 4 + 1] = 71;
                pixels[(i * Width + j) * 4 + 2] = 26;
            }
        }
        
    }
}