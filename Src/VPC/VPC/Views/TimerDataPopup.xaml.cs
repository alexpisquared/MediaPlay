using System.Windows;

namespace VPC.Views
{
    public partial class TimerDataPopup : Window
    {
        public TimerDataPopup()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; };
        }

        public int MinutesLeft { get; set; }
        string eOTimeMessage;

        public string EOTimeMessage { get { return eOTimeMessage; } set { t1.Text = eOTimeMessage = value; } }

        void onOk(object sender, RoutedEventArgs e)
        {
            readUiToModel();
            DialogResult = true;
            Close();
        }

        void readUiToModel()
        {
            MinutesLeft =
                r10.IsChecked == true ? 10 :
                r15.IsChecked == true ? 15 :
                r20.IsChecked == true ? 20 :
                r30.IsChecked == true ? 30 :
                r40.IsChecked == true ? 40 :
                r60.IsChecked == true ? 60 : 90;

            t0.Text = MinutesLeft.ToString();
            EOTimeMessage = t1.Text;
        }
        void onCancel(object sender, RoutedEventArgs e) { Close(); }

        void onRBtn(object sender, RoutedEventArgs e) { readUiToModel(); }

    }
}
