using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace GifGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

        }

        private static void Test1(string path)
        {
            using var sourceStream = new FileStream(path, FileMode.Open);
            var decoder = new GifBitmapDecoder(sourceStream, BitmapCreateOptions.None, BitmapCacheOption.None);

            App.DumpMetadata("decorder:", decoder.Metadata);

            var frameIndex = 0;
            foreach(var frame in decoder.Frames)
            {
                if (frame.Metadata is BitmapMetadata meta)
                {
                    App.DumpMetadata($"frame {frameIndex}", decoder.Metadata);
                }

                frameIndex++;
            }
        }
    }
}
