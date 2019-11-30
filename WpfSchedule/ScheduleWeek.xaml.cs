using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfSchedule
{
    /// <summary>
    /// Interaction logic for ScheduleMonth.xaml
    /// </summary>
    public partial class ScheduleWeek : UserControl
    {
        private DateTime currentDate;
        public DateTime CurrentDate
        {
            get => currentDate;
            set {
                currentDate = value.Date;
                for(; currentDate.DayOfWeek != DayOfWeek.Monday; currentDate -= new TimeSpan(1, 0, 0, 0)) { };
                _guitTitle.Content = string.Format("Week starting {0} {1:00}/{2}", CurrentDate.DayOfWeek.ToString(), CurrentDate.Day, CurrentDate.Month);
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
        private List<Canvas> guicCanvas = new List<Canvas>();

        public event EventHandler ScheduleItemClick;

        private static double TimeVisualIntervalHeight = 50.0;

        public ScheduleWeek()
        {
            InitializeComponent();
            Items = new List<ScheduleItem>();

            guicCanvas.Add(_guicCanvasSunday);
            guicCanvas.Add(_guicCanvasMonday);
            guicCanvas.Add(_guicCanvasTuesday);
            guicCanvas.Add(_guicCanvasWednesday);
            guicCanvas.Add(_guicCanvasThursday);
            guicCanvas.Add(_guicCanvasFriday);
            guicCanvas.Add(_guicCanvasSaturday);

            foreach(Canvas c in guicCanvas) {
                c.MouseLeftButtonDown += OnScheduleItemClick;
            }

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
            item.GeneratePanel(guicCanvas[(int)item.Start.DayOfWeek].ActualWidth, guicCanvas[(int)item.Start.DayOfWeek].ActualHeight, TimeStart.TotalSeconds, TimeEnd.TotalSeconds, totalSeconds);
            Items.Add(item);
        }

        public void Remove(ScheduleItem item)
        {
            Items.Remove(item);
        }

        public void Remove(Guid id)
        {
            Items.RemoveAll(x => x.ID == id);
        }

        public void Clear()
        {
            for(int i = 0; i < 7; i++) {
                guicCanvas[i].Children.Clear();
            }
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
            for(int i = 0; i < 7; i++) {
                guicCanvas[i].Children.Clear();
            }

            int StartItem = Items.FindIndex(x => x.Start >= CurrentDate.Add(TimeStart));
            if(StartItem > -1) {
                for(int i = StartItem; i < Items.Count && Items[i].Start < CurrentDate.Add(TimeEnd); i++) {
                    guicCanvas[(int)Items[i].Start.DayOfWeek].Children.Add(Items[i].Panel);
                }
            }
        }

        public void Redraw()
        {
            _guicGrid.Children.Clear();
            _guicGrid.RowDefinitions.Clear();
            for(int i = 0; i < 7; i++) {
                guicCanvas[i].Children.Clear();
            }

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
            //create column borders
            for(int i = 1; i < 7; i++) {
                Border border = new Border();
                border.BorderBrush = _guicGridTimeline.BorderBrush;
                border.BorderThickness = new Thickness(0, 0, 1, 0);
                border.SetValue(Grid.ColumnProperty, i);
                border.SetValue(Grid.RowSpanProperty, rows);
                _guicGrid.Children.Add(border);
            }
            foreach(Canvas c in guicCanvas) {
                c.SetValue(Grid.RowSpanProperty, rows);
            }

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
                item.GeneratePanel(guicCanvas[(int)item.Start.DayOfWeek].ActualWidth, guicCanvas[(int)item.Start.DayOfWeek].ActualHeight, TimeStart.TotalSeconds, TimeEnd.TotalSeconds, totalSeconds);
            }

            int StartItem = Items.FindIndex(x => x.Start >= CurrentDate.Add(TimeStart));
            if(StartItem > -1) {
                for(int i = StartItem; i < Items.Count && Items[i].Start < CurrentDate.Add(TimeEnd); i++) {
                    guicCanvas[(int)Items[i].Start.DayOfWeek].Children.Add(Items[i].Panel);
                }
            }
        }
    }
}
