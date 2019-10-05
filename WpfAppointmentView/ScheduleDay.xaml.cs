using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfAppointmentView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ScheduleDay : UserControl
    {
        public TimeSpan Interval;
        public DateTime CurrentDate;
        public bool FitToSize;

        private int timeDisplayInterval;
        private double intervalHeight;
        public double IntervalHeight
        {
            get => intervalHeight;
            set {
                intervalHeight = value;
                if(TimeVisualIntervalHeight % value == 0) {
                    timeDisplayInterval = (int)(TimeVisualIntervalHeight / value);
                } else {
                    timeDisplayInterval = (int)(TimeVisualIntervalHeight / value) + 1;
                }
            }
        }

        public List<ScheduleItem> Items = new List<ScheduleItem>();

        private string CurrentDate_t
        {
            get => string.Format("{0} {1:00}/{2}", CurrentDate.DayOfWeek.ToString(), CurrentDate.Day, CurrentDate.Month);
        }
        private static double TimeVisualIntervalHeight = 50.0;      //minimum height between displaying the time interval


        public ScheduleDay()
        {
            InitializeComponent();

            //default values
            Interval = new TimeSpan(0, 30, 0);
            CurrentDate = DateTime.Today.Date;
            FitToSize = false;
            IntervalHeight = 25.0;


        }


        public void Redraw()
        {
            _guicGrid.Children.Clear();
            _guicCanvas.Children.Clear();
            

            double canvasHeight = 0.0;

            int rows = 0;
            //calculate number of intervals in day and height of drawing canvas
            for(TimeSpan c = new TimeSpan(1, 0, 0, 0); c > TimeSpan.Zero; c -= Interval) {
                rows++;
                canvasHeight += IntervalHeight;
            }

            _guicCanvas.SetValue(Grid.RowSpanProperty, rows);

            //recalculate intervals and override canvas height if FitToSize is enabled
            if(FitToSize) {
                canvasHeight = _guicScroll.Height;
                IntervalHeight = canvasHeight / (double)rows;
            }

            //create grid rows and draw time intervals
            _guicGrid.RowDefinitions.Clear();
            TimeSpan time = TimeSpan.Zero;
            for(int i = 0; i < rows; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(IntervalHeight);
                _guicGrid.RowDefinitions.Add(row);

                if(i == 0 || i % timeDisplayInterval == 0) {
                    Label label = new Label();
                    label.Content = string.Format("{0:00}:{1:00}", time.Hours, time.Minutes);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    label.SetValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
                    _guicGrid.Children.Add(label);
                }

                time = time.Add(Interval);
            }

            //draw items
            Items.Sort(ScheduleItem.SortByStart);
            int StartItem = Items.FindIndex(x => x.Start >= CurrentDate);
            double totalSeconds = new TimeSpan(1, 0, 0, 0).TotalSeconds;
            if(StartItem > -1) {
                for(int i = StartItem; i < Items.Count && Items[i].End < CurrentDate.Add(new TimeSpan(1, 0, 0, 0)); i++) {
                    Border apptItem = new Border
                    {
                        BorderThickness = new Thickness(1.0),
                        BorderBrush = Items[i].BorderColor,
                        Background = Items[i].FillColor
                    };

                    double yPos = (Items[i].Start.TimeOfDay.TotalSeconds / totalSeconds) * canvasHeight;

                    apptItem.Width = _guicCanvas.ActualWidth - 16.0;
                    apptItem.Height = (Items[i].End.TimeOfDay.TotalSeconds / totalSeconds * canvasHeight) - yPos;
                    Canvas.SetLeft(apptItem, 8);
                    Canvas.SetTop(apptItem, yPos);

                    StackPanel panel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    Label apptTime = new Label
                    {
                        Content = Items[i].Start.ToShortTimeString(),
                        Margin = new Thickness(1.0, 0.0, 0.0, 0.0),
                        FontSize = 11,
                        Padding = new Thickness(0.0),
                        Foreground = Brushes.Black
                    };

                    Label apptDesc = new Label
                    {
                        Content = Items[i].Title,
                        Margin = new Thickness(1.0, 0.0, 0.0, 0.0),
                        FontSize = 11,
                        Padding = new Thickness(0.0),
                        Foreground = Brushes.Black
                    };

                    panel.Children.Add(apptTime);
                    panel.Children.Add(apptDesc);
                    apptItem.Child = panel;
                    _guicCanvas.Children.Add(apptItem);
                }
            }
        }
    }

    public class ScheduleItem
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public Brush BorderColor { get; set; }
        public Brush FillColor { get; set; }

        public static Brush DefaultBorderColor = Brushes.Black;
        public static Brush DefaultFillColor = Brushes.LightGreen;

        public readonly Guid ID;
        public object Data;     //can hold a reference to an object if needed

        public ScheduleItem()
        {
            ID = Guid.NewGuid();

            //defaults
            BorderColor = DefaultBorderColor;
            FillColor = DefaultFillColor;
        }

        internal static int SortByStart(ScheduleItem x, ScheduleItem y)
        {
            if(x != null) {
                if(y != null) {
                    return DateTime.Compare(x.Start, y.Start);
                } else {
                    return -1;  //y is null, put x first
                }
            } else {
                if(y != null) {
                    return 1;   //x is null, put y first
                } else {
                    return 0;   //they're both null
                }
            }
        }
    }
}
