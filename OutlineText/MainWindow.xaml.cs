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
        #region 定数

        /// <summary>
        /// デフォルトの縁取りの幅を定義します。
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
        /// 
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
        /// 取得または設定、 Brush コンテンツ領域の背景の塗りつぶしに使用します。
        /// </summary>
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
        /// TextBlock に対して、最上位レベルのフォント サイズを取得または設定します。
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
        /// TextBlock の最上位レベルのフォント伸縮特性を取得または設定します。
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
        /// TextBlock に対して、最上位レベルのフォント スタイルを取得または設定します。
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
        /// TextBlock に対して、最上位レベルのフォントの太さを取得または設定します。
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
        /// <see cref="OutlineText"/> のテキスト コンテンツの縁取りに適用する <see cref="Brush"/> を取得または設定します。
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
        /// <see cref="OutlineText"/> のテキスト コンテンツの縁取りに適用する幅を取得または設定します。
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
        /// <see cref="OutlineText"/> のテキスト コンテンツの縁取りに適用する <see cref="Visibility"/> を取得または設定します。
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
        /// 取得または設定のテキストの内容、 TextBlockです。
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
        /// テキスト コンテンツの水平方向の配置を示す値を取得または設定します。
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
        /// 取得または設定、 TextDecorationCollection のテキストに適用する効果を含む、 TextBlockです。
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
        /// 
        /// </summary>
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
        /// 
        /// </summary>
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
        /// <see cref="OutlineText"/> の新しいインスタンスを初期化します。
        /// </summary>
        public OutlineText()
        {
            TextDecorations = new TextDecorationCollection();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 内容を表示、 TextBlockです。
        /// </summary>
        /// <param name="drawingContext">DrawingContext にコントロールの表示にします。</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            EnsureGeometry();

            // 背景の描画
            if (Background != null)
            {
                drawingContext.DrawRectangle(Background, null, backgroundRect);
            }

            // 縁取りの描画
            if ((OutlineVisibility == Visibility.Visible) && (Outline != null) && (OutlineThickness > 0))
            {
                // DrawGeometry はパスの中心から OutlineThickness の太さで描画するので、
                // 外側の太さとしては、2 倍にして描画させる

                //GeometryGroup geometryGroup = new GeometryGroup();
                //geometryGroup.Children.Add(textGeometry);
                //geometryGroup.Children.Add(new LineGeometry(new Point(textGeometry.Bounds.Left, textGeometry.Bounds.Bottom- 2), new Point(textGeometry.Bounds.Right, textGeometry.Bounds.Bottom - 2)));

                drawingContext.DrawGeometry(null, new Pen(Outline, OutlineThickness * 2), textGeometry);
            }

            //drawingContext.DrawLine(new Pen(Foreground, OutlineThickness), new Point(textGeometry.Bounds.Left + 1, textGeometry.Bounds.Bottom - 2), new Point(textGeometry.Bounds.Right - 1, textGeometry.Bounds.Bottom - 2));

            // DrawGeometry は ClearType が効かないので、改めて文字を描画する
            drawingContext.DrawText(formattedText, new Point(Padding.Left, Padding.Top));
        }

        /// <summary>
        /// 再を測定すると呼ばれる、 TextBlockです。
        /// </summary>
        /// <param name="constraint">Size のサイズに対する制約を指定する構造体、 TextBlockです。</param>
        /// <returns>Size 構造体の新しいサイズを示す、 TextBlockです。</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            EnsureFormattedText();

            formattedText.MaxTextWidth = Math.Min(3579139, constraint.Width - Padding.Left - Padding.Right);
            formattedText.MaxTextHeight = constraint.Height - Padding.Top - Padding.Bottom;

            return new Size(formattedText.Width + Padding.Left + Padding.Right, formattedText.Height + Padding.Top + Padding.Bottom);
        }

        /// <summary>
        /// 子要素を配置しのサイズを決定、 TextBlockです。
        /// </summary>
        /// <param name="arrangeSize">
        /// Size ホストの親要素の中を TextBlock 自体とその子要素を配置に使用する必要があります。 
        /// サイズ制約とこの要求のサイズに影響を与える可能性があります。
        /// </param>
        /// <returns>実際、 Size 要素の配置に使用します。</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            EnsureFormattedText();

            formattedText.MaxTextWidth = arrangeSize.Width - Padding.Left - Padding.Right;
            formattedText.MaxTextHeight = arrangeSize.Height - Padding.Top - Padding.Bottom;

            textGeometry = null;

            return arrangeSize;
        }

        /// <summary>
        /// <see cref="formattedText"/> が無効になった際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された System.Windows.DependencyObject。</param>
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
        /// <param name="dependencyObject">プロパティの値が変更された System.Windows.DependencyObject。</param>
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
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void PaddingUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlinedTextBlock = (OutlineText)dependencyObject;

            outlinedTextBlock.OnPaddingUpdated(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
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
