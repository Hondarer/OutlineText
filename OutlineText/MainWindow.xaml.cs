using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace OutlineTextSample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// MainWindow の新しいインスタンスを初期化します。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    // Based on "WPF – 文字の縁取りをする"
    // http://astel-labs.net/blog/diary/2012/05/06-1.html

    [ContentProperty("Text")]
    internal class OutlineText : FrameworkElement
    {
        private FormattedText FormattedText = null;
        private Geometry TextGeometry = null;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextInvalidated));

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
            "TextDecorations", typeof(TextDecorationCollection), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(OutlineText),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty OutlineProperty = DependencyProperty.Register(
            "Outline", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            "OutlineThickness", typeof(double), typeof(OutlineText),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty OutlineVisibilityProperty = DependencyProperty.Register(
            "OutlineVisibility", typeof(Visibility), typeof(OutlineText),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Outline
        {
            get { return (Brush)GetValue(OutlineProperty); }
            set { SetValue(OutlineProperty, value); }
        }

        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        public Visibility OutlineVisibility
        {
            get { return (Visibility)GetValue(OutlineVisibilityProperty); }
            set { SetValue(OutlineVisibilityProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public OutlineText()
        {
            TextDecorations = new TextDecorationCollection();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            EnsureGeometry();

            //// TODO: Background の描画
            //// Background を考慮する必要あり
            //// Padding を考慮する必要あり
            //drawingContext.DrawRectangle(Brushes.Tomato, null, TextGeometry.Bounds);

            // 縁取りの必要がある場合にのみ描画
            if ((OutlineVisibility == Visibility.Visible) && (OutlineThickness > 0))
            {
                // DrawGeometry はパスの中心から OutlineThickness の太さで描画するので、
                // 外側の太さとしては、2 倍にして描画させる
                drawingContext.DrawGeometry(Outline, new Pen(Outline, OutlineThickness * 2), TextGeometry);
            }

            // DrawGeometry は ClearType が効かないので、改めて文字を描画する
            drawingContext.DrawText(FormattedText, new Point());
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureFormattedText();

            FormattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
            FormattedText.MaxTextHeight = availableSize.Height;

            return new Size(FormattedText.Width, FormattedText.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureFormattedText();

            FormattedText.MaxTextWidth = finalSize.Width;
            FormattedText.MaxTextHeight = finalSize.Height;

            TextGeometry = null;

            return finalSize;
        }

        private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;
            outlinedTextBlock.FormattedText = null;
            outlinedTextBlock.TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;
            outlinedTextBlock.UpdateFormattedText();
            outlinedTextBlock.TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private void EnsureFormattedText()
        {
            if (FormattedText != null || Text == null)
                return;

            FormattedText = new FormattedText(
                Text,
                CultureInfo.CurrentUICulture,
                FlowDirection,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                FontSize,
                Brushes.Black);

            UpdateFormattedText();
        }

        private void UpdateFormattedText()
        {
            if (FormattedText == null)
                return;

            FormattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
            FormattedText.TextAlignment = TextAlignment;
            FormattedText.Trimming = TextTrimming;

            FormattedText.SetFontSize(FontSize);
            FormattedText.SetFontStyle(FontStyle);
            FormattedText.SetFontWeight(FontWeight);
            FormattedText.SetFontFamily(FontFamily);
            FormattedText.SetFontStretch(FontStretch);
            FormattedText.SetTextDecorations(TextDecorations);
            FormattedText.SetForegroundBrush(Foreground);
        }

        private void EnsureGeometry()
        {
            if (TextGeometry != null)
                return;

            EnsureFormattedText();
            TextGeometry = FormattedText.BuildGeometry(new Point());
        }
    }
}
