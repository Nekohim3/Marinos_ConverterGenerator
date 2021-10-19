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
            g.TextEditor = TextEditor;
            var vm = MWVM.LoadFromXml();
            DataContext  = vm;
            vm.Result    = vm.Builder.GetToConverterResult();
            
        }
    }
}