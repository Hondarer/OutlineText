﻿using System;
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
        #region 定数

        /// <summary>
        /// テキストの最大幅を表します。
        /// </summary>
        private const double MAX_TEXT_WIDTH = 3579139.0D;

        /// <summary>
        /// 既定の縁取りの幅を表します。
        /// </summary>
        private const double DEFAULT_OUTLINE_TICKNESS = 1.0D;

        #endregion

        #region フィールド

        /// <summary>
        /// <see cref="FormattedText"/> を保持します。
        /// </summary>
        protected FormattedText formattedText = null;

        /// <summary>
        /// <see cref="FormattedText"/> の <see cref="Geometry"/> を保持します。
        /// </summary>
        protected Geometry textGeometry = null;

        /// <summary>
        /// コンテンツ領域の背景を保持します。
        /// </summary>
        protected Rect backgroundRect;

        #endregion

        #region 依存関係プロパティ

        /// <summary>
        /// Background 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Text 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(OutlineText),
            new FrameworkPropertyMetadata(string.Empty, FormattedTextInvalidated));

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
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Outline 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineProperty = DependencyProperty.Register(
            "Outline", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// OutlineThickness 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            "OutlineThickness", typeof(double), typeof(OutlineText),
            new FrameworkPropertyMetadata(DEFAULT_OUTLINE_TICKNESS, FrameworkPropertyMetadataOptions.AffectsRender));

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

        /// <summary>
        /// Padding 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding", typeof(Thickness), typeof(OutlineText),
            new FrameworkPropertyMetadata(PaddingUpdated));

        /// <summary>
        /// ClipBackgroundToText 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty ClipBackgroundToTextProperty = DependencyProperty.Register(
            "ClipBackgroundToText", typeof(bool), typeof(OutlineText),
            new FrameworkPropertyMetadata(false, FormattedTextUpdated));

        #endregion

        #region プロパティ

        /// <summary>
        /// コンテンツ領域の背景の塗りつぶしに使用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        /// <value>コンテンツ領域の背景の塗りつぶしに使用する <see cref="Brush"/>。既定値は、<c>null</c> です。</value>
        [Description("コンテンツ領域の背景の塗りつぶしに使用する Brush を取得または設定します。")]
        public Brush Background
        {
            get
            {
                return (Brush)GetValue(BackgroundProperty);
            }
            set
            {
                SetValue(BackgroundProperty, value);
            }
        }

        /// <summary>
        /// <see cref="OutlineText"/> に対して、優先される最上位レベルのフォント ファミリを取得または設定します。
        /// </summary>
        [Category("Font")]
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
        /// <see cref="OutlineText"/> に対して、最上位レベルのフォント サイズを取得または設定します。
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        [Category("Font")]
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
        /// <see cref="OutlineText"/> の最上位レベルのフォント伸縮特性を取得または設定します。
        /// </summary>
        [Category("Font")]
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
        /// <see cref="OutlineText"/> に対して、最上位レベルのフォント スタイルを取得または設定します。
        /// </summary>
        [Category("Font")]
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
        /// <see cref="OutlineText"/> に対して、最上位レベルのフォントの太さを取得または設定します。
        /// </summary>
        [Category("Font")]
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
        /// <see cref="OutlineText"/> のテキスト コンテンツに適用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツに適用するために使用するブラシ。 既定値は、<see cref="Brushes.Black"/> です。</value>
        [Description("テキスト コンテンツに適用する Brush を取得または設定します。")]
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
        /// <see cref="OutlineText"/> のテキスト コンテンツの縁取りに適用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツの縁取りに適用するために使用するブラシ。 既定値は、<c>null</c> です。</value>
        [Description("テキスト コンテンツの縁取りに適用する Brush を取得または設定します。")]
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
        /// <see cref="OutlineText"/> のテキスト コンテンツの縁取りに適用する幅を取得または設定します。
        /// </summary>
        [Category("Appearance")]
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
        /// <see cref="OutlineText"/> のテキスト コンテンツの縁取りに適用する <see cref="Visibility"/> を取得または設定します。
        /// </summary>
        [Category("Appearance")]
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
        /// <see cref="OutlineText"/> のテキスト コンテンツを取得または設定します。
        /// </summary>
        [Description("テキスト コンテンツを取得または設定します。")]
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
        /// テキスト コンテンツの水平方向の配置を示す値を取得または設定します。
        /// </summary>
        [Category("Text")]
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
        /// 取得または設定、 TextDecorationCollection のテキストに適用する効果を含む、 TextBlockです。
        /// </summary>
        [Category("Text")]
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
        /// コンテンツ領域いっぱいになったときに使用するテキストのトリミング動作を取得または設定します。
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
        /// 取得または設定する方法、 TextBlock テキストをラップする必要があります。
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

        /// <summary>
        /// コンテンツ領域の境界と <see cref="OutlineText"/> によって表示されるコンテンツとの間に埋め込むスペースの幅を示す値を取得または設定します。
        /// </summary>
        /// <value>適用する埋め込みの量を指定する <see cref="Thickness"/> 構造体。デバイス非依存のピクセル単位で指定します。 既定値は、<c>0</c> です。</value>
        [Category("Layout")]
        public Thickness Padding
        {
            get
            {
                return (Thickness)GetValue(PaddingProperty);
            }
            set
            {
                SetValue(PaddingProperty, value);
            }
        }

        /// <summary>
        /// 背景をテキストに合わせるかどうかを表す値を取得または設定します。
        /// </summary>
        /// <value>背景をテキストに合わせるかどうかを表す値。既定値は、<c>false</c> です。</value>
        [Category("Appearance")]
        public bool ClipBackgroundToText
        {
            get
            {
                return (bool)GetValue(ClipBackgroundToTextProperty);
            }
            set
            {
                SetValue(ClipBackgroundToTextProperty, value);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// <see cref="OutlineText"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public OutlineText()
        {
            TextDecorations = new TextDecorationCollection();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// <see cref="OutlineText"/> の内容を描画します。
        /// </summary>
        /// <param name="drawingContext">特定の要素の描画の手順です。 このコンテキストは、レイアウト システムで提供されています。</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            EnsureGeometry();

            // 背景の描画
            if (Background != null)
            {
                drawingContext.DrawRectangle(Background, null, backgroundRect);
            }

            if (string.IsNullOrEmpty(Text) == true)
            {
                return;
            }

            // 縁取りの描画
            if ((OutlineVisibility == Visibility.Visible) && (Outline != null) && (OutlineThickness > 0))
            {
                // DrawGeometry はパスの中心から OutlineThickness の太さで描画するので、
                // 外側の太さとしては、2 倍にして描画させる

                drawingContext.DrawGeometry(null, new Pen(Outline, OutlineThickness * 2), textGeometry);
            }

            // DrawGeometry は ClearType が効かないので、改めて文字を描画する
            drawingContext.DrawText(formattedText, new Point(Padding.Left, Padding.Top));
        }

        /// <summary>
        /// 子要素に必要なレイアウトのサイズを測定し、<see cref="OutlineText"/> のサイズを決定します。
        /// </summary>
        /// <param name="availableSize">
        /// この要素が子要素に提供できる使用可能なサイズ。
        /// あらゆるコンテンツに要素がサイズを合わせることを示す値として、無限大を指定できます。
        /// </param>
        /// <returns>子要素のサイズの計算に基づいて、この要素が判断したレイアウト時に必要なサイズ。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureFormattedText();

            if (string.IsNullOrEmpty(Text) == true)
            {
                return new Size(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);
            }

            formattedText.MaxTextWidth = Math.Min(MAX_TEXT_WIDTH, availableSize.Width - Padding.Left - Padding.Right);
            formattedText.MaxTextHeight = availableSize.Height - Padding.Top - Padding.Bottom;

            return new Size(formattedText.Width + Padding.Left + Padding.Right, formattedText.Height + Padding.Top + Padding.Bottom);
        }

        /// <summary>
        /// 子要素を配置し、<see cref="OutlineText"/> のサイズを決定します。
        /// </summary>
        /// <param name="finalSize">この要素が要素自体と子を配置するために使用する親の末尾の領域。</param>
        /// <returns>使用する実際のサイズ。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureFormattedText();

            if (string.IsNullOrEmpty(Text) == true)
            {
                return finalSize;
            }

            formattedText.MaxTextWidth = finalSize.Width - Padding.Left - Padding.Right;
            formattedText.MaxTextHeight = finalSize.Height - Padding.Top - Padding.Bottom;

            textGeometry = null;

            return finalSize;
        }

        /// <summary>
        /// <see cref="formattedText"/> が無効になった際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void FormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;

            outlinedTextBlock.OnFormattedTextInvalidated(e);
        }

        /// <summary>
        /// <see cref="formattedText"/> が無効になった際に発生します。
        /// </summary>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        protected virtual void OnFormattedTextInvalidated(DependencyPropertyChangedEventArgs e)
        {
            formattedText = null;
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// <see cref="formattedText"/> が更新された際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void FormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;

            outlinedTextBlock.OnFormattedTextUpdated(e);
        }

        /// <summary>
        /// <see cref="formattedText"/> が更新された際に発生します。
        /// </summary>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        protected virtual void OnFormattedTextUpdated(DependencyPropertyChangedEventArgs e)
        {
            UpdateFormattedText();
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// <see cref="Padding"/> が更新された際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void PaddingUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;

            outlinedTextBlock.OnPaddingUpdated(e);
        }

        /// <summary>
        /// <see cref="Padding"/> が更新された際に発生します。
        /// </summary>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        protected virtual void OnPaddingUpdated(DependencyPropertyChangedEventArgs e)
        {
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// <see cref="formattedText"/> を再評価します。
        /// </summary>
        protected virtual void EnsureFormattedText()
        {
            if ((formattedText != null) || (string.IsNullOrEmpty(Text) == true))
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
        /// <see cref="formattedText"/> を更新します。
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
        /// <see cref="textGeometry"/> を再評価します。
        /// </summary>
        protected virtual void EnsureGeometry()
        {
            if (textGeometry != null)
            {
                return;
            }

            EnsureFormattedText();

            if (string.IsNullOrEmpty(Text) == true)
            {
                if (ClipBackgroundToText == true)
                {
                    backgroundRect = new Rect();
                }
                else
                {
                    backgroundRect = new Rect(0, 0, ActualWidth, ActualHeight);
                }

                return;
            }

            textGeometry = formattedText.BuildGeometry(new Point(Padding.Left, Padding.Top));

            if (ClipBackgroundToText == true)
            {
                switch (TextAlignment)
                {
                    case TextAlignment.Left:
                        backgroundRect = new Rect(0, 0, formattedText.Width + Padding.Left + Padding.Right, formattedText.Height + Padding.Top + Padding.Bottom);
                        break;
                    case TextAlignment.Right:
                        backgroundRect = new Rect(ActualWidth - formattedText.Width - Padding.Left - Padding.Right, 0, formattedText.Width + Padding.Left + Padding.Right, formattedText.Height + Padding.Top + Padding.Bottom);
                        break;
                    case TextAlignment.Center:
                        backgroundRect = new Rect((ActualWidth - formattedText.Width - Padding.Left - Padding.Right) / 2.0D, 0, formattedText.Width + Padding.Left + Padding.Right, formattedText.Height + Padding.Top + Padding.Bottom);
                        break;
                    case TextAlignment.Justify:
                        backgroundRect = new Rect(0, 0, ActualWidth, formattedText.Height + Padding.Top + Padding.Bottom);
                        break;
                    default:
                        // 通過することはない
                        break;
                }
            }
            else
            {
                // Element の領域背景に色を付ける場合(TextBlock 互換)
                backgroundRect = new Rect(0, 0, ActualWidth, ActualHeight);
            }
        }

        #endregion
    }
}
