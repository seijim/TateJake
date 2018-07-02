using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;

namespace WpfAppTateJake
{
    /// <summary>
    /// 縦ジャケ - 画像とテキストの合成
    /// 横：1250px, 縦：1772px => 886px (半分の高さを利用)
    /// </summary>
    public partial class MainWindow : Window
    {
        string cImageUri = "/Resources/TemplateImage.png";
        List <TitleText> titleTextList;
        double heightOfImage;
        double defaultFontSize;
        double adjustedPixel;

        public void UpdateDefaultSetting()
        {
            ComboBoxFont.Text = Properties.Settings.Default["FontType"].ToString();
            ComboBoxFontWeight.Text = Properties.Settings.Default["FontWeight"].ToString();
            double _fontSize;
            if (double.TryParse(Properties.Settings.Default["FontSize"].ToString(), out _fontSize))
            {
                defaultFontSize = _fontSize;
                TextBoxFontSize.Text = _fontSize.ToString();
            }
            double _adjustedSize;
            if (double.TryParse(Properties.Settings.Default["AdjustedSize"].ToString(), out _adjustedSize))
            {
                adjustedPixel = _adjustedSize;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            UpdateDefaultSetting();

            Uri fileUri = new Uri(cImageUri, UriKind.Relative);
            var info = Application.GetResourceStream(fileUri);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = info.Stream;
            bitmapImage.EndInit();

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(bitmapImage,
                new Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight));
            heightOfImage = bitmapImage.PixelHeight / 2;

            // Draw a rectangle.
            drawingContext.DrawRectangle(
                Brushes.Gray,
                new Pen(Brushes.Gray, 2),
                new Rect(
                    0,
                    0,
                    bitmapImage.PixelWidth,
                    heightOfImage
                )
            );

            // Draw a text.
            double imageFontSize = 80;
            string drawText = "<ジャケット画像>";
            drawingContext.DrawText(
                new FormattedText(
                    drawText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Meiryo UI"),
                    imageFontSize,
                    Brushes.Black
                ),
                new Point(bitmapImage.PixelWidth / 2 - drawText.Length * imageFontSize / 2 + 60, heightOfImage / 2 - imageFontSize / 2)
            );

            drawingContext.Close();
            var rtb = new RenderTargetBitmap((int)bitmapImage.PixelWidth, (int)bitmapImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);

            ImagePreview.Source = rtb;
        }

        private void PreviewText_Click(object sender, RoutedEventArgs e)
        {
            double defaultFontSizeInput;
            if (double.TryParse(TextBoxFontSize.Text, out defaultFontSizeInput))
                defaultFontSize = defaultFontSizeInput;

            titleTextList = new List<TitleText>();
            Uri fileUri = new Uri(cImageUri, UriKind.Relative);
            var info = Application.GetResourceStream(fileUri);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = info.Stream;
            bitmapImage.EndInit();

            ImagePreview.Source = bitmapImage;

            if (textBoxEditor.Text == string.Empty)
            {
                MessageBox.Show("テキストを入力してください", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var text = textBoxEditor.Text;
            string[] rows = text.Split('\n');

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(bitmapImage,
                new Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight));

            // Draw a rectangle.
            drawingContext.DrawRectangle(
                Brushes.Gray,
                new Pen(Brushes.Gray, 2),
                new Rect(
                    0,
                    0,
                    bitmapImage.PixelWidth,
                    heightOfImage
                )
            );

            string resultText = string.Empty;

            double defaultXPos = 10;
            double currentYPos = 10;
            int rowCount = 0;
            foreach (var row in rows)
            {
                string textRow = row.Replace("\r", "").Replace("\n", "");
                string[] cols = textRow.Split('|');
                string textMessage = cols[0];
                double x = 0;
                double y = 0;
                double fontSize = 0;
                bool switchHeight = false;
                string[] attrs;
                int attrsLength = 0;

                if (cols.Length > 1)
                {
                    attrs = cols[1].Split(',');
                    attrsLength = attrs.Length;
                    if (attrs.Length >= 3)
                    {
                        switchHeight = true;
                        if (!double.TryParse(attrs[0].Trim(), out fontSize))
                            fontSize = defaultFontSize;
                        if (!double.TryParse(attrs[1].Trim(), out x))
                            x = defaultXPos;
                        if (!double.TryParse(attrs[2].Trim(), out y))
                            y = currentYPos;
                    }
                    else if (attrs.Length >= 2)
                    {
                        if (!double.TryParse(attrs[0].Trim(), out fontSize))
                            fontSize = defaultFontSize;
                        if (!double.TryParse(attrs[1].Trim(), out x))
                            x = defaultXPos;
                        y = currentYPos;
                    }
                    else if (attrs.Length >= 1)
                    {
                        if (!double.TryParse(attrs[0].Trim(), out fontSize))
                            fontSize = defaultFontSize;
                        x = defaultXPos;
                        y = currentYPos;
                    }
                }
                else
                {
                    fontSize = defaultFontSize;
                    x = defaultXPos;
                    y = currentYPos;
                }

                // Draw a text.
                double imageFontSize = 80;
                string drawText = "<ジャケット画像>";
                drawingContext.DrawText(
                    new FormattedText(
                        drawText,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Meiryo UI"),
                        imageFontSize,
                        Brushes.Black
                    ),
                    new Point(bitmapImage.PixelWidth / 2 - drawText.Length * imageFontSize / 2 + 60, heightOfImage / 2 - imageFontSize / 2)
                );

                string tempText = string.Empty;
                tempText = textRow;

                if (rowCount == 0)
                {
                    resultText = string.Empty;

                    if (!switchHeight) // 最初のテキストで、高さ指定がされていない場合
                    {
                        y = (bitmapImage.PixelHeight - heightOfImage - fontSize * (double)rows.Length) / 2 - 40;
                    }

                    tempText = textMessage + "|" + fontSize.ToString() + ","
                        + x.ToString() + "," + y.ToString();
                }

                if (ComboBoxFont.Text == "メイリオ")
                    adjustedPixel = 0;
                else
                    adjustedPixel = 10;

                if (cols.Length <= 1 || (cols.Length > 1 && attrsLength < 2)) // Ｘ位置が指定されていない時
                {
                    x = (bitmapImage.PixelWidth - fontSize * (double)textMessage.Length) / 2 + adjustedPixel;

                    tempText = textMessage + "|" + fontSize.ToString() + ","
                        + x.ToString() + "," + y.ToString();
                }

                if (resultText == string.Empty)
                    resultText = tempText;
                else
                    resultText += Environment.NewLine + tempText;

                double adjustedHeight = heightOfImage + y;

                // Draw a text.
                var fontWeight = FontWeights.Normal;
                if (ComboBoxFontWeight.Text != "標準")
                {
                    fontWeight = FontWeights.Bold;
                }

                drawingContext.DrawText(
                    new FormattedText(
                        textMessage,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily(ComboBoxFont.Text),FontStyles.Normal, fontWeight, FontStretches.Normal),
                        fontSize,
                        Brushes.Black
                    ),
                    new Point(x, adjustedHeight)
                );

                // Store to List
                TitleText titleText = new TitleText();
                titleText.Text = textMessage;
                titleText.FontName = ComboBoxFont.Text;
                titleText.FontSize = fontSize;
                titleText.FontWeight = fontWeight;
                titleText.XPos = x;
                titleText.YPos = adjustedHeight;
                titleTextList.Add(titleText);

                currentYPos = y + fontSize;
                ++rowCount;
            }
            drawingContext.Close();
            var rtb = new RenderTargetBitmap((int)bitmapImage.PixelWidth, (int)bitmapImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);

            ImagePreview.Source = rtb;
            textBoxEditor.Text = resultText;
        }

        private void LoadImagesAndSynthesis(object sender, RoutedEventArgs e)
        {
            if (titleTextList == null || titleTextList.Count <= 0)
            {
                MessageBox.Show("最初に、テキストをプレビューし、位置決めを確定してください", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string pictureFilePath = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = null;
            Nullable<bool> result = false;

            // ファイルを開くダイアログ
            dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.DefaultExt = ".jpg";
            dlg.Filter = "画像ファイル(jpg;gif;png)|*.jpg;*.jpeg;*.gif;*.png|すべてのファイル(*.*)|*.*";
            dlg.Multiselect = true;
            result = dlg.ShowDialog();
            if (result == null || result == false)
            {
                MessageBox.Show("ファイルが選択されていません", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string outputDirectory = Path.GetDirectoryName(dlg.FileNames[0]) + "\\output";
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            foreach(var fileName in dlg.FileNames)
            {
                // Get output file name
                string outputFileName = outputDirectory + "\\" + Path.GetFileName(fileName);

                // Load targeted image
                BitmapImage bitmapImageTarget = new BitmapImage();
                bitmapImageTarget.BeginInit();
                bitmapImageTarget.CacheOption = BitmapCacheOption.OnLoad;
                FileStream stream = File.OpenRead(fileName);
                bitmapImageTarget.StreamSource = stream;
                bitmapImageTarget.EndInit();
                stream.Close();

                // Load base image
                Uri fileUri = new Uri(cImageUri, UriKind.Relative);
                var info = Application.GetResourceStream(fileUri);

                BitmapImage bitmapImageBase = new BitmapImage();
                bitmapImageBase.BeginInit();
                bitmapImageBase.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImageBase.StreamSource = info.Stream;
                bitmapImageBase.EndInit();

                // Combine target and base images
                var drawingVisual = new DrawingVisual();
                var drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(bitmapImageBase,
                    new Rect(0, 0, bitmapImageBase.PixelWidth, bitmapImageBase.PixelHeight));

                double scale = (double)bitmapImageBase.PixelWidth / (double)bitmapImageTarget.PixelWidth;
                BitmapSource scaledBitmapSource = new TransformedBitmap(bitmapImageTarget, new ScaleTransform(scale, scale));

                drawingContext.DrawImage(scaledBitmapSource,
                    new Rect(0, 0, scaledBitmapSource.PixelWidth, scaledBitmapSource.PixelHeight));

                // Adjust height of text area
                double adjustedYPos = (scaledBitmapSource.PixelHeight - heightOfImage) / 2;

                // Draw all texts.
                foreach (var titleText in titleTextList)
                {
                    drawingContext.DrawText(
                        new FormattedText(
                            titleText.Text,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily(titleText.FontName), FontStyles.Normal, titleText.FontWeight, FontStretches.Normal),
                            titleText.FontSize,
                            Brushes.Black
                        ),
                        new Point(titleText.XPos, titleText.YPos + adjustedYPos)
                    );
                }

                drawingContext.Close();
                var rtb = new RenderTargetBitmap((int)bitmapImageBase.PixelWidth, (int)bitmapImageBase.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(drawingVisual);

                // Output image files
                using (var fs = new FileStream(outputFileName, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(fs);
                }
            }

            MessageBox.Show("画像合成が終了しました" + Environment.NewLine 
                + $"出力先：{outputDirectory}", "メッセージ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItemSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow(this);
            settingWindow.ShowDialog();
        }
    }
}
