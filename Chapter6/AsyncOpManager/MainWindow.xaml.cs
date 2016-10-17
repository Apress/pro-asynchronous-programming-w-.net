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

namespace AsyncOpManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Clock clock = new Clock();

        public MainWindow()
        {
            InitializeComponent();
         
            clock.Tick+=UpdateClock;
        }

        private void UpdateClock(object sender, EventArgs e)
        {
            Hour.Text = clock.Hour.ToString();
            Minute.Text = clock.Minutes.ToString();
            Second.Text = clock.Seconds.ToString();
        }
    }
}
