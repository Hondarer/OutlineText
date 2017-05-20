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

    /// <summary>
    /// 縁取り付きのテキストを提供します。
    /// </summary>
    /// <remarks>
    /// Based on "WPF – 文字の縁取りをする"
    /// http://astel-labs.net/blog/diary/2012/05/06-1.html
    /// </remarks>
    [ContentProperty("Text")]
    public class OutlineText : FrameworkElement
    {
        #region フィールド

        /// <summary>
        /// <see cref="FormattedText"/> を保持します。
        /// </summary>
        protected FormattedText formattedText = null;

        /// <summary>
        /// <see cref="FormattedText"/> の <see cref="Geometry"/> を保持します。
        /// </summary>
        protected Geometry textGeometry = null;

        #endregion

        #region 依存関係プロパティ

        /// <summary>
        /// Text 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(OutlineText),
            new FrameworkPropertyMetadata(FormattedTextInvalidated));

        /// <summary>
        /// TextAlignment 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(OutlineText),
            new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// TextDecorations 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
            "TextDecorations", typeof(TextDecorationCollection), typeof(OutlineText),
            new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// TextTrimming 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(OutlineText),
            new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// TextWrapping 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(OutlineText),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, FormattedTextUpdated));

        /// <summary>
        /// Foreground 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Outline 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineProperty = DependencyProperty.Register(
            "Outline", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// OutlineThickness 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            "OutlineThickness", typeof(double), typeof(OutlineText),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// OutlineVisibility 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineVisibilityProperty = DependencyProperty.Register(
            "OutlineVisibility", typeof(Visibility), typeof(OutlineText),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// FontFamily 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontSize 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontStretch 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontStyle 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontWeight 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        #endregion

        #region プロパティ

        /// <summary>
        /// <see cref="OutlineText"/> のテキスト コンテンツに適用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        public Brush Foreground
        {
            get
            {
                return (Brush)GetValue(ForegroundProperty);
            }
            set
            {
                SetValue(ForegroundProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)GetValue(FontFamilyProperty);
            }
            set
            {
                SetValue(FontFamilyProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get
            {
                return (double)GetValue(FontSizeProperty);
            }
            set
            {
                SetValue(FontSizeProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FontStretch FontStretch
        {
            get
            {
                return (FontStretch)GetValue(FontStretchProperty);
            }
            set
            {
                SetValue(FontStretchProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)GetValue(FontStyleProperty);
            }
            set
            {
                SetValue(FontStyleProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)GetValue(FontWeightProperty);
            }
            set
            {
                SetValue(FontWeightProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Brush Outline
        {
            get
            {
                return (Brush)GetValue(OutlineProperty);
            }
            set
            {
                SetValue(OutlineProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double OutlineThickness
        {
            get
            {
                return (double)GetValue(OutlineThicknessProperty);
            }
            set
            {
                SetValue(OutlineThicknessProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Visibility OutlineVisibility
        {
            get
            {
                return (Visibility)GetValue(OutlineVisibilityProperty);
            }
            set
            {
                SetValue(OutlineVisibilityProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)GetValue(TextAlignmentProperty);
            }
            set
            {
                SetValue(TextAlignmentProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TextDecorationCollection TextDecorations
        {
            get
            {
                return (TextDecorationCollection)GetValue(TextDecorationsProperty);
            }
            set
            {
                SetValue(TextDecorationsProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TextTrimming TextTrimming
        {
            get
            {
                return (TextTrimming)GetValue(TextTrimmingProperty);
            }
            set
            {
                SetValue(TextTrimmingProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextWrappingProperty);
            }
            set
            {
                SetValue(TextWrappingProperty, value);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// <see cref="OutlineText"/> の新しいインスタンスを初期化します。
        /// </summary>
        public OutlineText()
        {
            TextDecorations = new TextDecorationCollection();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawingContext"></param>
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
                drawingContext.DrawGeometry(Outline, new Pen(Outline, OutlineThickness * 2), textGeometry);
            }

            // DrawGeometry は ClearType が効かないので、改めて文字を描画する
            drawingContext.DrawText(formattedText, new Point());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureFormattedText();

            formattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
            formattedText.MaxTextHeight = availableSize.Height;

            return new Size(formattedText.Width, formattedText.Height);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureFormattedText();

            formattedText.MaxTextWidth = finalSize.Width;
            formattedText.MaxTextHeight = finalSize.Height;

            textGeometry = null;

            return finalSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void FormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;

            outlinedTextBlock.OnFormattedTextInvalidated(e);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFormattedTextInvalidated(DependencyPropertyChangedEventArgs e)
        {
            formattedText = null;
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void FormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;

            outlinedTextBlock.OnFormattedTextUpdated(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFormattedTextUpdated(DependencyPropertyChangedEventArgs e)
        {
            UpdateFormattedText();
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void EnsureFormattedText()
        {
            if (formattedText != null || Text == null)
            {
                return;
            }

            formattedText = new FormattedText(
                Text,
                CultureInfo.CurrentUICulture,
                FlowDirection,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                FontSize,
                Brushes.Black);

            UpdateFormattedText();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void UpdateFormattedText()
        {
            if (formattedText == null)
            {
                return;
            }

            if (TextWrapping == TextWrapping.NoWrap)
            {
                formattedText.MaxLineCount = 1;
            }
            else
            {
                formattedText.MaxLineCount = int.MaxValue;
            }

            formattedText.TextAlignment = TextAlignment;
            formattedText.Trimming = TextTrimming;

            formattedText.SetFontSize(FontSize);
            formattedText.SetFontStyle(FontStyle);
            formattedText.SetFontWeight(FontWeight);
            formattedText.SetFontFamily(FontFamily);
            formattedText.SetFontStretch(FontStretch);
            formattedText.SetTextDecorations(TextDecorations);
            formattedText.SetForegroundBrush(Foreground);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void EnsureGeometry()
        {
            if (textGeometry != null)
            {
                return;
            }

            EnsureFormattedText();
            textGeometry = formattedText.BuildGeometry(new Point());
        }

        #endregion
    }
}
