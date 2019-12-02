using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WpfSchedule;

namespace ScheduleTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            guicScheduleDay.IntervalHeight = 25.0;
            guicScheduleDay.Interval = new TimeSpan(1, 0, 0);
            guicScheduleDay.Redraw();

            guicScheduleWeek.IntervalHeight = 25.0;
            guicScheduleWeek.Interval = new TimeSpan(1, 0, 0);
            guicScheduleWeek.Redraw();

            guidDate.SelectedDate = DateTime.Today.Date;
            guicEventEdit.Visibility = Visibility.Collapsed;
        }

        private void ClearNew()
        {
            guiEventEditDate.SelectedDate = DateTime.Today.Date;
            guiEventEditStart.Text = null;
            guiEventEditEnd.Text = null;
            guiEventEditTitle.Text = null;
            guiEventEditDesc.Text = null;

        }

        private void GuidDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            guicScheduleDay.CurrentDate = guidDate.SelectedDate.Value;
        }

        private void GuifDatePrev_Click(object sender, RoutedEventArgs e)
        {
            guidDate.SelectedDate = guidDate.SelectedDate.Value.AddDays(-1.0);
        }

        private void GuifDateToday_Click(object sender, RoutedEventArgs e)
        {
            guidDate.SelectedDate = DateTime.Today.Date;
        }

        private void GuifDateNext_Click(object sender, RoutedEventArgs e)
        {
            guidDate.SelectedDate = guidDate.SelectedDate.Value.AddDays(1.0);
        }

        private void GuifEventNew_Click(object sender, RoutedEventArgs e)
        {
            guicEventEdit.Visibility = Visibility.Visible;
            ClearNew();
        }

        private void GuifEventDelete_Click(object sender, RoutedEventArgs e)
        {
            guicScheduleDay.Remove((guicEvent.DataContext as ScheduleItem).ID);
            guicEvent.DataContext = null;
        }

        private void GuiEventEditSave_Click(object sender, RoutedEventArgs e)
        {
            ScheduleItem item = new ScheduleItem();
            DateTime start = guiEventEditDate.SelectedDate.Value;
            DateTime end = guiEventEditDate.SelectedDate.Value;
            try {
                start = start.Add(TimeSpan.Parse(guiEventEditStart.Text));
                end = end.Add(TimeSpan.Parse(guiEventEditEnd.Text));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            item.Start = start;
            item.End = end;
            item.Title = guiEventEditTitle.Text;
            item.Description = guiEventEditDesc.Text;
            guicScheduleDay.Add(item);
            guicScheduleWeek.Add(item);
        }

        private void GuicSchedule_ScheduleItemClick(object sender, EventArgs e)
        {
            if(sender == null) {
                ClearNew();
                guicEventEdit.Visibility = Visibility.Collapsed;
                guicEvent.DataContext = null;
                guifEventDelete.IsEnabled = false;
            } else {
                guicEvent.DataContext = sender;
                guifEventDelete.IsEnabled = true;
            }
        }
    }
}
