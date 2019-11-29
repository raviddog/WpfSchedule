using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfSchedule
{
    /// <summary>
    /// Interaction logic for ScheduleDay.xaml
    /// </summary>
    public partial class ScheduleDay : UserControl
    {
        private DateTime currentDate;
        public DateTime CurrentDate
        {
            get => currentDate;
            set {
                currentDate = value.Date;
                _guitTitle.Content = string.Format("{0} {1:00}/{2}", CurrentDate.DayOfWeek.ToString(), CurrentDate.Day, CurrentDate.Month);
                ChangeDate();
            }
        }

        public TimeSpan Interval { get; set; }
        public bool FitToSize { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }

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

        private bool darkTheme;
        public bool DarkTheme
        {
            get => darkTheme;
            set {
                darkTheme = value;
                if(darkTheme) {
                    _guicGridTimeline.BorderBrush = Brushes.AntiqueWhite;
                    ScheduleItem.DefaultBorderColor = Brushes.AntiqueWhite;
                    foreach(ScheduleItem item in Items) {
                        item.Panel.BorderBrush = Brushes.AntiqueWhite;
                    }
                } else {
                    _guicGridTimeline.BorderBrush = Brushes.Black;
                    ScheduleItem.DefaultBorderColor = Brushes.Black;
                    foreach(ScheduleItem item in Items) {
                        item.Panel.BorderBrush = Brushes.Black;
                    }
                }
            }
        }

        private List<ScheduleItem> Items;

        public event EventHandler ScheduleItemClick;

        private static double TimeVisualIntervalHeight = 50.0;      //minimum height between displaying the time interval        

        public ScheduleDay()
        {
            InitializeComponent();
            Items = new List<ScheduleItem>();

            //assign event handlers
            _guicCanvas.MouseLeftButtonDown += OnScheduleItemClick;

            //default values
            Interval = new TimeSpan(0, 30, 0);
            TimeStart = new TimeSpan(0, 0, 0);
            TimeEnd = new TimeSpan(1, 0, 0, 0);
            CurrentDate = DateTime.Today.Date;
            FitToSize = false;
            IntervalHeight = 35.0;
            DarkTheme = false;

            Redraw();
        }

        public void Add(ScheduleItem item)
        {
            double totalSeconds = TimeEnd.TotalSeconds - TimeStart.TotalSeconds;
            item.GeneratePanel(_guicCanvas.ActualWidth, _guicCanvas.ActualHeight, TimeStart.TotalSeconds, TimeEnd.TotalSeconds, totalSeconds);
            Items.Add(item);
        }

        public void Remove(ScheduleItem item)
        {
            Items.Remove(item);
        }

        public void Clear()
        {
            _guicCanvas.Children.Clear();
            Items.Clear();
        }

        public IReadOnlyCollection<ScheduleItem> GetItems()
        {
            return Items.AsReadOnly();
        }


        private void OnScheduleItemClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int i = Items.FindIndex(x => x.Start >= CurrentDate.Add(TimeStart));
            for(; i < Items.Count && !Items[i].Panel.IsMouseOver; i++) { }
            if(i < Items.Count && Items[i].Clickable) {
                ScheduleItemClick?.Invoke(Items[i], EventArgs.Empty);
            } else {
                    //nothing was clicked, fire event with no object
                ScheduleItemClick?.Invoke(null, EventArgs.Empty);
            }
        }

        private void ChangeDate()
        {
            _guicCanvas.Children.Clear();

            int StartItem = Items.FindIndex(x => x.Start >= CurrentDate.Add(TimeStart));
            if(StartItem > -1) {
                for(int i = StartItem; i < Items.Count && Items[i].Start < CurrentDate.Add(TimeEnd); i++) {
                    _guicCanvas.Children.Add(Items[i].Panel);

                }
            }
        }

        public void Redraw()
        {
            _guicGrid.Children.Clear();
            _guicGrid.RowDefinitions.Clear();
            _guicCanvas.Children.Clear();

            this.UpdateLayout();

            double canvasHeight = 0.0;

            int rows = 0;
            //calculate number of intervals in day and height of drawing canvas
            for(TimeSpan c = TimeEnd; c > TimeStart; c -= Interval) {
                rows++;
                canvasHeight += IntervalHeight;
            }

            _guicGrid.Children.Add(_guicGridTimeline);
            _guicGridTimeline.SetValue(Grid.RowSpanProperty, rows);
            _guicCanvas.SetValue(Grid.RowSpanProperty, rows);

            //recalculate intervals and override canvas height if FitToSize is enabled
            if(FitToSize) {
                canvasHeight = _guicScroll.ActualHeight;
                IntervalHeight = canvasHeight / (double)rows;
            }

            //create grid rows and draw time intervals
            TimeSpan time = TimeStart;
            for(int i = 0; i < rows; i++) {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(IntervalHeight);
                _guicGrid.RowDefinitions.Add(row);

                Border border = new Border();
                border.BorderBrush = _guicGridTimeline.BorderBrush;
                border.BorderThickness = new Thickness(0, 0, 0, 1);
                border.SetValue(Grid.ColumnSpanProperty, 2);
                border.SetValue(Grid.RowProperty, i);
                _guicGrid.Children.Add(border);

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
            double totalSeconds = TimeEnd.TotalSeconds - TimeStart.TotalSeconds;

            foreach(ScheduleItem item in Items) {
                item.GeneratePanel(_guicCanvas.ActualWidth, _guicCanvas.ActualHeight, TimeStart.TotalSeconds, TimeEnd.TotalSeconds, totalSeconds);
            }

            int StartItem = Items.FindIndex(x => x.Start >= CurrentDate.Add(TimeStart));
            if(StartItem > -1) {
                for(int i = StartItem; i < Items.Count && Items[i].Start < CurrentDate.Add(TimeEnd); i++) {
                    _guicCanvas.Children.Add(Items[i].Panel);
                    
                }
            }
        }

    }

    public class ScheduleItem
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        internal string Time { get => string.Format("{0} - {1}", Start.ToShortTimeString(), End.ToShortTimeString()); }
        internal Border Panel;
        
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Clickable { get; set; }
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
            Clickable = true;
        }

        internal void GeneratePanel(double width, double height, double secondsStart, double secondsEnd, double secondsTotal)
        {
            Panel = new Border
            {
                BorderThickness = new Thickness(1.0),
                BorderBrush = BorderColor,
                Background = FillColor
            };

            double yPos = (Start.TimeOfDay.TotalSeconds - secondsStart) / secondsTotal * height;

            Panel.Width = width > 16 ? width : 0.0;
            Panel.Height = End < Start ? 0.0 : (End.TimeOfDay.TotalSeconds - secondsStart) / secondsTotal * height - yPos;

            Canvas.SetLeft(Panel, 8);
            Canvas.SetTop(Panel, yPos);

            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Label apptTime = new Label
            {
                Margin = new Thickness(1.0, 0.0, 0.0, 0.0),
                FontSize = 11,
                Padding = new Thickness(0.0),
                Foreground = Brushes.Black
            };

            apptTime.Content = Time;

            Label apptDesc = new Label
            {
                Margin = new Thickness(1.0, 0.0, 0.0, 0.0),
                FontSize = 11,
                Padding = new Thickness(0.0),
                Foreground = Brushes.Black
            };

            apptDesc.Content = Title;

            panel.Children.Add(apptTime);
            panel.Children.Add(apptDesc);
            Panel.Child = panel;
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
