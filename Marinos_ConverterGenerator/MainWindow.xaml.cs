namespace Marinos_ConverterGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext  = new MWVM();
            g.TextEditor = TextEditor;
        }
    }
}