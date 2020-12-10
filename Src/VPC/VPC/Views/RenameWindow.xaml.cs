using System.Threading.Tasks;
using System.Windows;

namespace VPC.Views
{
    public partial class RenameWindow : Window
    {
        public RenameWindow(Window owner, string filename = "")
        {
            Owner = owner;
            Filename = filename;

            InitializeComponent();
            MouseLeftButtonDown += (s, e) => { DragMove(); e.Handled = true; };

            DataContext = this;
            Loaded += RenameWindow_Loaded;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Loaded -= RenameWindow_Loaded;
        }


        async void RenameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            f1.Focus();
            await Task.Delay(99);
            tbFilename.Focus();
        }

        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register("Filename", typeof(string), typeof(RenameWindow), new PropertyMetadata()); public string Filename { get { return (string)GetValue(FilenameProperty); } set { SetValue(FilenameProperty, value); } }

        void onEnter(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
        void onCancel(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}
