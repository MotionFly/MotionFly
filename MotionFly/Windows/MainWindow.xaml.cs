using CrazyflieAPI;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MotionFly.Kinect;
using MotionFly.ViewModels;

namespace MotionFly.Windows
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for window resize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimulationImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                double columnWidth = (16.0 / 9.0) * e.NewSize.Height;
                SimulationViewColumn.Width = new GridLength(columnWidth);
                MinWidth = columnWidth + 400;
            }
        }

        /// <summary>
        /// Event handler for Exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Event handler for Start Crazyflie Client button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartCfClientMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var pythonPath = Path.Combine(dlg.SelectedPath, "python-3.4.3");
                var cfcPath = Path.Combine(dlg.SelectedPath, "crazyflie-clients-python-2015.09");

                if (Directory.Exists(pythonPath) && Directory.Exists(cfcPath))
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = cfcPath,
                        UseShellExecute = false,
                        FileName = Path.Combine(pythonPath, "python.exe"),
                        Arguments = "bin/cfclient"
                    };

                    Process.Start(startInfo);
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect Connection Toggle button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KinectConnectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.ToggleKinectConnection();
        }

        /// <summary>
        /// Event handler for Crazyflie Connection Toggle button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrazyflieConnectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.ToggleCrazyflieConnection();
        }

        /// <summary>
        /// Event handler for Next Step button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextStepButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.NextTrackingStep();
        }

        /// <summary>
        /// Event handler for Cancel Tracking button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelTrackingButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.CancelTracking();
        }

        /// <summary>
        /// Event handler for closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConnectionHelper.Instance.StopThread();
            BasicSensor.Instance.CloseSensor();
        }
    }
}
