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

using WpfAppointmentView;

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
            _schedule.CurrentDate = DateTime.Today;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ScheduleItem item = new ScheduleItem();
            item.Title = _title.Text;
            item.Description = _desc.Text;
            DateTime d = DateTime.Today;
            item.Start = d.Add(new TimeSpan((int)_hours.Value, (int)_min.Value, 0));
            item.End = d.Add(new TimeSpan((int)_hours.Value + 1, (int)_min.Value, 0));
            _schedule.Items.Add(item);
            _schedule.Redraw();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _schedule.Redraw();
        }
    }
}
