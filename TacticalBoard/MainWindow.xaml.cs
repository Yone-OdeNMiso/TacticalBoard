using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TacticalBoard
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //直線書いてる最中かどうかの判定
        bool IsDraw;

        //現在選択中のコマの直線の名前
        String lineName;

        //コマのリスト
        List<Thumb> thumbs = new List<Thumb>();

        //コマの初期配置座標リスト
        private PointCollection thumbDefaultPoints = new PointCollection();

        //スタンプ押してる最中判定用
        bool IsStamp;

        //スタンプのリスト
        List<Image> stampImages = new List<Image>();

        //スタンプリストのIndex
        int stampIndex = 0;

        //現在のレイヤー
        String nowLayer = "Layer1";

        public MainWindow()
        {
            InitializeComponent();

        }


        //Thumbドラッグ開始
        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var thumb = sender as Thumb;
            lineName = thumb.Name + "Line";
            Line line = PeaceCanvas.FindName(lineName) as Line;
            line.Visibility = Visibility.Collapsed;
            if (null != thumb)
            {
                var border = thumb.Template.FindName("Thumb_Border", thumb) as Border;
                if (null != border)
                {
                    border.BorderThickness = new Thickness(1);
                }
            }
        }

        //Thumbドラッグ終了
        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var thumb = sender as Thumb;
            if (null != thumb)
            {
                var border = thumb.Template.FindName("Thumb_Border", thumb) as Border;
                if (null != border)
                {
                    border.BorderThickness = new Thickness(0);
                }
            }
        }

        //Thumbドラッグ中
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (null != thumb)
            {
                var x = Canvas.GetLeft(thumb) + e.HorizontalChange;
                var y = Canvas.GetTop(thumb) + e.VerticalChange;

                var canvas = thumb.Parent as Canvas;
                if (null != canvas)
                {
                    x = Math.Max(x, 0);
                    y = Math.Max(y, 0);
                    x = Math.Min(x, canvas.ActualWidth - thumb.ActualWidth);
                    y = Math.Min(y, canvas.ActualHeight - thumb.ActualHeight);
                }

                Canvas.SetLeft(thumb, x);
                Canvas.SetTop(thumb, y);
            }
        }

        //背景画像(MAP画像)をファイルから選択
        private void MAPButton_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "イメージファイル (*.png, *.jpg)|*.png;*.jpg";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(dialog.FileName, UriKind.Relative));
                ib.Stretch = Stretch.Uniform;
                inkCanvasLayer1.Background = ib;
            }
        }

        //コマから右ドラッグで直線を引くためのメソッド(右クリック押し込み動作)
        private void ThumbRightDown(object sender, MouseButtonEventArgs e)
        {
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;
            var Thumb = sender as Thumb;
            Point thumbPoint = Thumb.TranslatePoint(new Point(0, 0), PeaceCanvas);
            Point mousePoint = Mouse.GetPosition(PeaceCanvas);
            lineName = Thumb.Name + "Line";
            Line line = PeaceCanvas.FindName(lineName) as Line;
            if (line != null)
            {
                line.X1 = thumbPoint.X + 14;
                line.Y1 = thumbPoint.Y + 14;
            }

            if (IsDraw)
            {
                IsDraw = false;
                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
            }
            else
            {
                IsDraw = true;
            }

        }

        //キャンバス上のマウス移動関係
        private void peaceMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = Mouse.GetPosition(PeaceCanvas);
            //直線描画部分
            Line line = PeaceCanvas.FindName(lineName) as Line;
            if (IsDraw)
            {

                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
                line.Visibility = Visibility.Visible;
            }

            //Stamp押す部分
            if (IsStamp)
            {
                stampImages[stampIndex - 1].Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
            }
        }

        //thumbから右クリックを離した場合
        private void peaceMouseRightUp(object sender, MouseButtonEventArgs e)
        {
            Line line = PeaceCanvas.FindName(lineName) as Line;
            Point mousePoint = Mouse.GetPosition(PeaceCanvas);
            if (IsDraw)
            {
                IsDraw = false;
                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
            }
        }

        //リセットボタンの動作
        private void resetButton(object sender, RoutedEventArgs e)
        {
            Line line;
            int i;

            //ボタン周りのリセット
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;

            //インクのリセット
            for (i = 0; i < 10; i++)
            {
                Canvas.SetLeft(thumbs[i], thumbDefaultPoints[i].X);
                Canvas.SetTop(thumbs[i], thumbDefaultPoints[i].Y);
                lineName = thumbs[i].Name + "Line";
                line = PeaceCanvas.FindName(lineName) as Line;
                line.Visibility = Visibility.Collapsed;
                inkCanvasLayer1.Strokes.Clear();

            }

            //スタンプのリセット
            for (; stampIndex > 0; stampIndex--)
            {
                stampImages[stampIndex - 1].Visibility = Visibility.Collapsed;
            }
            stampImages.Clear();
        }

        //Thumbをロードしたらリストに入れて管理しやすくする
        private void thumbLoaded(object sender, RoutedEventArgs e)
        {
            var thumb = sender as Thumb;
            thumbs.Add((Thumb)sender);
            thumbDefaultPoints.Add(thumb.TranslatePoint(new Point(0, 0), PeaceCanvas));
        }

        //ペンモードの切り替え
        private void PenButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PenButton == null)
            {
                return;
            }

            if (PenButton.IsChecked == true)
            {
                EraseButton.IsChecked = false;
                inkCanvasLayer1.EditingMode = InkCanvasEditingMode.Ink;
            }
            else
            {
                EraseButton.IsChecked = false;
                inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;
            }

        }

        //消しゴムモードの切替
        private void EraseChecked(object sender, RoutedEventArgs e)
        {
            if (EraseButton == null)
            {
                return;
            }

            if (EraseButton.IsChecked == true)
            {
                inkCanvasLayer1.EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
            else
            {
                if (PenButton.IsChecked == true)
                {
                    inkCanvasLayer1.EditingMode = InkCanvasEditingMode.Ink;
                }
                else
                {
                    inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;
                }
            }
        }

        //画面上でのクリック時の動作
        private void peaceMouseClick(object sender, MouseButtonEventArgs e)
        {
            //スタンプ押す状態のとき
            if (IsStamp)
            {
                IsStamp = false;

            }
        }

        //グレスタンプボタン
        private void fragButtonClick(object sender, RoutedEventArgs e)
        {
            //ペン関係をオフにする
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;

            //元画像読み込み
            BitmapImage image = new BitmapImage(new Uri("Resources/frag.png", UriKind.Relative));
            Image frag = new Image();
            frag.Source = image;
            frag.Width = 50;
            IsStamp = true;
            stampIndex++;

            //右クリックで削除できるようにイベントハンドラを用意する
            frag.MouseRightButtonDown += new MouseButtonEventHandler(StampClear);
            Point mousePoint = Mouse.GetPosition(inkCanvasLayer1);

            //マウスカーソルの位置に画像をセットする
            frag.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);

            //スタンプを配置
            StampCanvasLayer1.Children.Add(frag);
            stampImages.Add(frag);
        }

        //スモークスタンプボタン
        private void smokeButtonClick(object sender, RoutedEventArgs e)
        {
            //ペン関係をオフにする
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;

            //元画像読み込み
            BitmapImage image = new BitmapImage(new Uri("Resources/smoke.png", UriKind.Relative));
            Image smoke = new Image();
            smoke.Source = image;
            smoke.Width = 50;
            IsStamp = true;
            stampIndex++;

            //右クリックで削除できるようにイベントハンドラを用意する
            smoke.MouseRightButtonDown += new MouseButtonEventHandler(StampClear);
            Point mousePoint = Mouse.GetPosition(inkCanvasLayer1);

            //マウスカーソルの位置に画像をセットする
            smoke.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);

            //スタンプを配置
            StampCanvasLayer1.Children.Add(smoke);
            stampImages.Add(smoke);
        }

        //スタンスタンプボタン
        private void stunButtonClick(object sender, RoutedEventArgs e)
        {
            //ペン関係をオフにする
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvasLayer1.EditingMode = InkCanvasEditingMode.None;

            //元画像読み込み
            BitmapImage image = new BitmapImage(new Uri("Resources/stun.png", UriKind.Relative));
            Image stun = new Image();
            stun.Source = image;
            stun.Width = 50;
            IsStamp = true;
            stampIndex++;

            //右クリックで削除できるようにイベントハンドラを用意する
            stun.MouseRightButtonDown += new MouseButtonEventHandler(StampClear);
            Point mousePoint = Mouse.GetPosition(inkCanvasLayer1);

            //マウスカーソルの位置に画像をセットする
            stun.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);

            //スタンプを配置
            StampCanvasLayer1.Children.Add(stun);
            stampImages.Add(stun);
        }

        //その他スタンプボタン
        private void stampButtonClick(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "イメージファイル (*.png, *.jpg)|*.png;*.jpg";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                Image stamp = new Image();
                stamp.Source = new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute));
                stamp.Width = 50;
                IsStamp = true;
                stampIndex++;
                
                //右クリックで削除できるようにイベントハンドラを用意する
                stamp.MouseRightButtonDown += new MouseButtonEventHandler(StampClear);
                Point mousePoint = Mouse.GetPosition(inkCanvasLayer1);

                //マウスカーソルの位置に画像をセットする
                stamp.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);

                //スタンプを配置
                StampCanvasLayer1.Children.Add(stamp);
                stampImages.Add(stamp);
            }
        }


        //レイヤー関連
        private void layerButtonClick(object sender, RoutedEventArgs e)
        {

            Button clickedLayerButton = sender as Button;
            String clickedLayer = clickedLayerButton.Content.ToString();
            Button beforeLayerButton = FindName(clickedLayer + "Button") as Button;
            InkCanvas nextLayerInk = FindName("inkCanvas" + clickedLayer) as InkCanvas;
            Canvas nextLayerStamp = FindName("StampCanvas" + clickedLayer) as Canvas;
            InkCanvas beforeLayerInk = FindName("inkCanvas" + nowLayer) as InkCanvas;
            Canvas beforeLayerStamp = FindName("StampCanvas" + nowLayer) as Canvas;

            nextLayerInk.Visibility = Visibility.Visible;
            nextLayerStamp.Visibility = Visibility.Visible;
            beforeLayerInk.Visibility = Visibility.Collapsed;
            beforeLayerStamp.Visibility = Visibility.Collapsed;

            clickedLayerButton.IsEnabled = false;
            beforeLayerButton.IsEnabled = true;

            nowLayer = clickedLayer;
        }

        //スタンプを右クリックしたときの動作(右クリックしたスタンプを消す)
        private void StampClear(object sender, RoutedEventArgs e)
        {
            var Stamp = sender as Image;
            Stamp.Visibility = Visibility.Collapsed;
        }

    }
}
