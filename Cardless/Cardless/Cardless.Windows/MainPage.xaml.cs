using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using WindowsPreview.Media.Ocr;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Media.Capture;


using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Cardless
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// public static MainPage Current
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        OcrEngine ocrEngine;
        UInt32 width;
        
        MainPage rootPage = MainPage.Current;
        UInt32 height;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            ocrEngine = new OcrEngine(OcrLanguage.English);
        }

        public bool CheckEmail(string x)
        {
            bool sofar = false;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] == '@')
                {
                    sofar = true;
                }
            }
            return sofar;
        }

        public bool CheckNumber(string x)
        {
            List<char> allowedchar = new List<char>(new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '(', ')' });
            for (int i = 0; i < x.Length; i++)
            {
                if (allowedchar.IndexOf(x[i]) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public List<string> GetEmailIds(List<string> inpwords)
        {
            List<string> anslist = new List<string>();
            foreach (string word in inpwords)
            {
                if (CheckEmail(word))
                {
                    anslist.Add(word);
                }
            }
            return anslist;
        }

        public List<string> GetPhoneNumbers(List<string> inpwords)
        {
            List<string> anslist = new List<string>();
            foreach (string word in inpwords)
            {
                if (CheckNumber(word))
                {
                    anslist.Add(word);
                }
            }
            return anslist;
        }

        public string JoinList(List<string> inp)
        {
            string ans = "";
            foreach (string elem in inp)
            {
                ans += elem + ",";
            }
            return ans;
        }

        protected async override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // Load image from install folder.
            //ArrayList allwords = new ArrayList();
            var file = await Package.Current.InstalledLocation.GetFileAsync("shreydad.png");
            List<string> wordlist1 = new List<string>();
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                // Create image decoder.
                var decoder = await BitmapDecoder.CreateAsync(stream);

                width = decoder.PixelWidth;
                height = decoder.PixelHeight;

                // Get pixels in BGRA format.
                var pixels = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    new BitmapTransform(),
                    ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.ColorManageToSRgb);

                // Extract text from image.
                OcrResult result = await ocrEngine.RecognizeAsync(height, width, pixels.DetachPixelData());

                // Check whether text is detected.
                if (result.Lines != null)
                {
                    // Collect recognized text.
                    string recognizedText = "";
                    foreach (var line in result.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            //allwords.Add(word.Text);
                            wordlist1.Add(word.Text);
                            recognizedText += word.Text + " ";
                        }
                        recognizedText += Environment.NewLine;
                    }

                    // Display recognized text.
                    OcrText.Text = recognizedText;
                    EmailText.Text = JoinList(GetEmailIds(wordlist1)) + "\n" + JoinList(GetPhoneNumbers(wordlist1));
                }
            }
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                //rootPage.NotifyUser("", NotifyType.StatusMessage);

                // Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                CameraCaptureUI dialog = new CameraCaptureUI();
                Size aspectRatio = new Size(16, 9);
                dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (file != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        bitmapImage.SetSource(fileStream);
                    }
                    CapturedPhoto.Source = bitmapImage;
                    //ResetButton.Visibility = Visibility.Visible;

                    // Store the file path in Application Data
                    //appSettings[photoKey] = file.Path;
                }
                else
                {
                    //rootPage.NotifyUser("No photo captured.", NotifyType.StatusMessage);
                }
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
            }
        }

    }
}
