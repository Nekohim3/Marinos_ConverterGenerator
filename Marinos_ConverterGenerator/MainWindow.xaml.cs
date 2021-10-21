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
            g.ConverterTextEditor         = ConverterTextEditor;
            g.SeriazableEntityTextEditor  = SeriazableEntityTextEditor;
            g.LoaderTextEditor            = LoaderTextEditor;
            g.EntityTextEditor            = EntityTextEditor;
            g.SeriazablePackageTextEditor = SeriazablePackageTextEditor;
            var vm = MWVM.LoadFromXml();
            DataContext  = vm;
            vm.ResultConverter    = vm.Builder.GetConverterResult();
            
        }
    }
}