using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Medical_Sys
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
                } else {
                    _guicGridTimeline.BorderBrush = Brushes.Black;
                    ScheduleItem.DefaultBorderColor = Brushes.Black;
                }
            }
        }

        public List<ScheduleItem> Items = new List<ScheduleItem>();
        private List<Border> CanvasElements = new List<Border>();

        public event EventHandler ScheduleItemClick;

        private static double TimeVisualIntervalHeight = 50.0;      //minimum height between displaying the time interval


        public ScheduleDay()
        {
            InitializeComponent();

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

        private void OnScheduleItemClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int i;  //find clicked item
            for(i = 0; i < CanvasElements.Count && !CanvasElements[i].IsMouseOver; i++) { }
            if(i < CanvasElements.Count && (CanvasElements[i].DataContext as ScheduleItem).Clickable) {
                ScheduleItemClick?.Invoke(CanvasElements[i].DataContext, EventArgs.Empty);
            } else {
                    //nothing was clicked, fire event with no object
                ScheduleItemClick?.Invoke(null, EventArgs.Empty);
            }
        }

        public void Redraw()
        {
            _guicGrid.Children.Clear();
            _guicGrid.RowDefinitions.Clear();
            _guicCanvas.Children.Clear();
            CanvasElements.Clear();

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
            int StartItem = Items.FindIndex(x => x.Start >= CurrentDate.Add(TimeStart));
            DateTime EndTime = CurrentDate.Add(TimeEnd);
            double totalSeconds = TimeEnd.TotalSeconds - TimeStart.TotalSeconds;
            if(StartItem > -1) {
                for(int i = StartItem; i < Items.Count && Items[i].Start < EndTime; i++) {
                    Border apptItem = new Border
                    {
                        BorderThickness = new Thickness(1.0),
                        BorderBrush = Items[i].BorderColor,
                        Background = Items[i].FillColor
                    };

                    double yPos = (Items[i].Start.TimeOfDay.TotalSeconds - TimeStart.TotalSeconds) / totalSeconds * canvasHeight;

                    if(_guicCanvas.ActualWidth < 16) {
                        apptItem.Width = 0.0;
                    } else {
                        apptItem.Width = _guicCanvas.ActualWidth - 16.0;
                    }
                    if(Items[i].End < Items[i].Start) {
                        //end time is before start time
                        apptItem.Height = 0.0;
                    } else {
                        apptItem.Height = (Items[i].End.TimeOfDay.TotalSeconds - TimeStart.TotalSeconds) / totalSeconds * canvasHeight - yPos;
                    }
                    Canvas.SetLeft(apptItem, 8);
                    Canvas.SetTop(apptItem, yPos);

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

                    apptTime.SetBinding(Label.ContentProperty, "Time");

                    Label apptDesc = new Label
                    {
                        Margin = new Thickness(1.0, 0.0, 0.0, 0.0),
                        FontSize = 11,
                        Padding = new Thickness(0.0),
                        Foreground = Brushes.Black
                    };

                    apptDesc.SetBinding(Label.ContentProperty, "Title");

                    panel.Children.Add(apptTime);
                    panel.Children.Add(apptDesc);
                    apptItem.Child = panel;
                    apptItem.DataContext = Items[i];
                    _guicCanvas.Children.Add(apptItem);
                    CanvasElements.Add(apptItem);
                }
            }
        }

    }

    public class ScheduleItem
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Time { get => string.Format("{0} - {1}", Start.ToShortTimeString(), End.ToShortTimeString()); }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool Clickable { get; set; }
        public Brush BorderColor { get; set; }
        public Brush FillColor { get; set; }

        internal static Brush DefaultBorderColor = Brushes.Black;
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
