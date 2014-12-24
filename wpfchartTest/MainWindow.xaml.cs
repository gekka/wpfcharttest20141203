// MICROSOFT LIMITED PUBLIC LICENSE

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.myData = new MyData();
            this.myData.TestInitialize();

            this.DataContext = this.myData;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private MyData myData;
        private System.Windows.Threading.DispatcherTimer timer;
        void timer_Tick(object sender, EventArgs e)
        {
            myData.TestTick();
        }
    }

    /// <summary>連続した線のデータ</summary>
    public class LineData : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged メンバ

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler pc = PropertyChanged;
            if (pc != null)
            {
                pc(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public string Name { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }

        /// <summary></summary>
        public double XMin
        {
            get
            {
                return _XMin;
            }
            set
            {
                if (_XMin != value)
                {
                    _XMin = value;
                    OnPropertyChanged("XMin");
                }
            }
        }
        private double _XMin;

        /// <summary></summary>
        public double XMax
        {
            get
            {
                return _XMax;
            }
            set
            {
                if (_XMax != value)
                {
                    _XMax = value;
                    OnPropertyChanged("XMax");
                }
            }
        }
        private double _XMax;



        /// <summary></summary>
        public IEnumerable<Point> Points
        {
            get
            {
                return _Points;
            }
            set
            {
                //if(_Points != value)
                {
                    _Points = value;
                    OnPropertyChanged("Points");
                }
            }
        }
        private IEnumerable<Point> _Points;


        public Color Color { get; set; }
    }

    /// <summary>Canvasの座標に変換できるTransformに変換する</summary>
    [ValueConversion(typeof(double[]), typeof(GeneralTransform))]
    class CanvasTransfrom : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double x = (double)values[0];
                double y = (double)values[1];
                double xmin = (double)values[2];
                double xmax = (double)values[3];
                double ymin = (double)values[4];
                double ymax = (double)values[5];

                double scaleX = x / (xmax - xmin);
                double scaleY = y / (ymax - ymin);
                //System.Diagnostics.Debug.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", x, y, scaleX, scaleY));
                ScaleTransform scale = new ScaleTransform(scaleX, -scaleY);
                TranslateTransform translate = new TranslateTransform(-xmin, -ymax);

                TransformGroup group = new TransformGroup();
                group.Children.Add(translate);
                group.Children.Add(scale);

                return group;
            }
            catch
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Pointの集合からPointCollectionを作る</summary>
    [ValueConversion(typeof(IEnumerable<Point>), typeof(PointCollection))]
    class PointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            PointCollection collection = value as PointCollection;
            if (collection != null)
            {
                return collection;
            }
            IEnumerable<Point> points = value as IEnumerable<Point>;
            if (value != null)
            {
                return new PointCollection(points);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PointCollection collection = value as PointCollection;
            if (collection != null)
            {
                return collection;
            }
            throw new NotSupportedException();
        }
    }

    /// <summary>ColorをSolidBrushに変換</summary>
    class ColorToSolotBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return new SolidColorBrush((Color)value);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = value as SolidColorBrush;
            if (brush != null)
            {
                return brush.Color;
            }
            throw new NotSupportedException();
        }
    }



    public class PointXy : INotifyPropertyChanged
    {
        public PointXy(DateTime x, double y) { _x = x; _y = y; }

        public DateTime X
        {
            get { return _x; }
            set
            {
                _x = value;
                OnPropertyChanged("X");
            }
        }
        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }
        }
        private DateTime _x;
        private double _y;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class MyData : INotifyPropertyChanged
    {
        public MyData()
        {
            _items.Add(new PointXy(DateTime.Now, double.NaN));
            StartTime = Items[0].X;
            EndTime = Items[0].X.AddHours(8);
            Lines = new List<LineData>();
        }

        /// <summary>線を作るためのデータ</summary>

        public List<LineData> Lines { get; private set; }

        #region TEST

        private const int INTERVAL_SECOND = 10;//データは10秒おきの
        private const int STEP = 360;//Testで一度に追加するデータ数(1時間分)
        private const int BLOCK = 24 * 3; //3日分

        /// <summary>テスト用の線を用意</summary>
        public void TestInitialize()
        {
            LineData lineData1 = CreateLineData(Colors.Blue, 5, 0, "測定値１");
            LineData lineData2 = CreateLineData(Colors.Green, 3, 20, "測定値２");

            this.Lines.Add(lineData1);
            this.Lines.Add(lineData2);

            this.StartTime = new DateTime(2014, 12, 1);
            this.EndTime = this.StartTime.AddSeconds(lineData1.XMax * INTERVAL_SECOND);
        }
        private LineData CreateLineData(Color color, double amplitude, double offset, string title)
        {
            int block_step = BLOCK * STEP;
            LineData lineData = new LineData();
            lineData.XMin = 0;
            lineData.XMax = block_step;
            lineData.YMin = -10;
            lineData.YMax = 30;
            lineData.Color = color;
            lineData.Name = title;
            List<Point> points = new List<Point>();
            for (int deg = 0; deg <= block_step; deg++)
            {
                points.Add(new Point(deg, Math.Sin(deg * Math.PI / 180.0 / 10) * amplitude + offset));
            }
            lineData.Points = points;
            return lineData;
        }

        /// <summary>テスト用の線にデータを追加する</summary>
        public void TestTick()
        {
            UpdateLineData(Lines[0], 5, 0);
            UpdateLineData(Lines[1], 3, 20);

            this.StartTime = this.StartTime.AddSeconds(STEP * INTERVAL_SECOND);
            this.EndTime = this.EndTime.AddSeconds(STEP * INTERVAL_SECOND);

            GC.Collect();
            long mem = GC.GetTotalMemory(true);
            System.Diagnostics.Debug.WriteLine("データ数={0:#,0}件,メモリ消費量={1:#,0}byte", BLOCK * STEP, mem);
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("mm:ss.fff"));
        }
        private void UpdateLineData(LineData line, double amplitude, double offset)
        {
            List<Point> points = (List<Point>)line.Points;
            points.RemoveRange(0, STEP);
            Random rnd = new Random();
            double deg = points[points.Count - 1].X;

            for (int i = 0; i < STEP; i++)
            {
                deg++;
                points.Add(new Point(deg, Math.Sin(deg * Math.PI / 180.0 / 10) * amplitude + offset));
            }

            line.Points = points;
            line.XMin += STEP;
            line.XMax += STEP;
        }
        #endregion


        #region Chartの軸を作るためのデータ
        /// <summary>チャートにItemsSoruceを入れないと軸ラベルが表示されないので</summary>
        public List<PointXy> Items
        {
            get
            {
                return _items;
            }
        }
        private List<PointXy> _items = new List<PointXy>();

        /// <summary></summary>
        public DateTime StartTime
        {
            get
            {
                return _StartTime;
            }
            set
            {
                if (_StartTime != value)
                {
                    _StartTime = value;
                    OnPropertyChanged("StartTime");
                }
            }
        }
        private DateTime _StartTime;

        /// <summary></summary>
        public DateTime EndTime
        {
            get
            {
                return _EndTime;
            }
            set
            {
                if (_EndTime != value)
                {
                    _EndTime = value;
                    OnPropertyChanged("EndTime");
                }
            }
        }
        private DateTime _EndTime;

        #endregion

        #region INotifyPropertyChanged

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}