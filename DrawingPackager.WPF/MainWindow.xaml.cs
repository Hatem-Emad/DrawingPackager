using DrawingPackager.SolidEdge;
using System;
using System.Windows;

namespace DrawingPackager.WPF
{
    public partial class MainWindow : Window
    {
        private SolidEdgeSession? _session;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TryConnect(() => SolidEdgeSession.AttachToRunning());
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TryConnect(() => SolidEdgeSession.AttachOrStart());
        }

        private void TryConnect(Func<SolidEdgeSession> connect)
        {
            try
            {
                _session = connect();
                StatusText.Text = $"Connected to Solid Edge. Visible: {_session.Visible}";
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }
    }
}
