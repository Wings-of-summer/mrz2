using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace SecondLabMRZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BackgroundWorker backgroundWorker;
        private Network network;
        private double[] sequense;
        private bool? showIteration = false;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = (BackgroundWorker)this.FindResource("backgroundWorker");
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        private void sequenceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            int index = comboBox.SelectedIndex;

            sequenseTextBox.Text = GetGeneratedSeqence(index);
        }

        private void sequenceTypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            List<string> data = new List<string>();

            //selected index 0
            data.Add("Ряд Фиббоначи");
            //selected index 1
            data.Add("Факториальная функция");
            //selected index 2
            data.Add("Периодическая функция");
            //selected index 3
            data.Add("Степенная функция ");

            comboBox.ItemsSource = data;

            comboBox.SelectedIndex = 0;

            sequenseTextBox.Text = GetGeneratedSeqence(0);
        }

        private string GetGeneratedSeqence(int index) 
        {
            string fromText = fromTextBox.Text;
            string toText = toTextBox.Text;

            int from = 0;
            int to = 0;

            double[] generatedSeqence;

            if (!Int32.TryParse(fromText, out from) || !Int32.TryParse(toText, out to)) 
            {
                return String.Empty;
            }

            if (index == 1)
            {
                FactorialSequence factorialSequence = new FactorialSequence();
                generatedSeqence = factorialSequence.GenerateSequence(from, to);
            }
            else if (index == 2)
            {
                PeriodicSequence periodicSequence = new PeriodicSequence();
                generatedSeqence = periodicSequence.GenerateSequence(from, to);
            }
            else if (index == 3)
            {
                PowerSequence powerSequence = new PowerSequence(2);
                generatedSeqence = powerSequence.GenerateSequence(from, to);
            }
            else
            {
                FibonacciSequence fibonacciSequence = new FibonacciSequence();
                generatedSeqence = fibonacciSequence.GenerateSequence(from, to);
            }

            StringBuilder builder = new StringBuilder();

            foreach (int value in generatedSeqence) 
            {
                builder.Append(value.ToString() + " ");
            }

            return builder.ToString();
        }

        private void predictSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            StartThread();
        }

        private void StartThread()
        {
            string windowSizeText = lTextBox.Text;
            string imagesNumberText = pTextBox.Text;
            string learningCoefficientText = aTextBox.Text;
            string maxErrorText = errorTextBox.Text;
            string maxIterationsText = nTextBox.Text;

            int windowSize = 0;
            int imagesNumber = 0;
            double learningCoefficient = 0.0;
            double maxError = 0.0;
            int maxIterations = 0;

            if (!Int32.TryParse(windowSizeText, out windowSize) ||
                !Int32.TryParse(imagesNumberText, out imagesNumber) ||
                !Double.TryParse(learningCoefficientText, out learningCoefficient) ||
                !Double.TryParse(maxErrorText, out maxError) ||
                !Int32.TryParse(maxIterationsText, out maxIterations))
            {
                return;
            }

            sequenseTextBox.IsEnabled = false;
            showIteration = showIterationCheckBox.IsChecked;
            predictedSequenceLabel.Text = "";

            if (showIteration.Equals(true)) 
            {
                stopButton.IsEnabled = true;
            }

            GetSequense();

            network = new Network(windowSize, imagesNumber, learningCoefficient, maxError, maxIterations);

            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);

            backgroundWorker.RunWorkerAsync(network);
        }

        private void ShowPredictSequense(double[] predictSequense) 
        {
            StringBuilder builder = new StringBuilder();

            foreach (double value in predictSequense)
            {
                builder.Append(Math.Round(value, 4).ToString() + " ");
            }

            predictedSequenceLabel.Text = builder.ToString();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Network network = (Network)e.Argument;

            network.Learn(backgroundWorker, e, sequense, showIteration);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Network.BackgroundResult result = (Network.BackgroundResult)e.UserState;
            if (result.IsComplete)
            {
                string predictNumberText = predictNumberTextBox.Text;
                int predictNumber = 0;

                if (Int32.TryParse(predictNumberText, out predictNumber)) 
                {
                    double[] predictSequense = network.Predict(sequense, predictNumber);
                    ShowPredictSequense(predictSequense);
                }
                sequenseTextBox.IsEnabled = true;
            }
            else 
            {
                iterationNumberLabel.Content = result.IterationNumber.ToString();
                currentErrorLabel.Content = result.Error.ToString();
            }
        }

        private void GetSequense() 
        {
            string sequenseText = sequenseTextBox.Text;
            string[] sequenseItemsText = sequenseText.Split(' ');

            List<double> tempSequense = new List<double>();

            foreach (string itemText in sequenseItemsText) 
            {
                double number = 0;
                if (Double.TryParse(itemText, out number)) 
                {
                    tempSequense.Add(number);
                }
            }

             sequense = tempSequense.ToArray();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = false;
            this.backgroundWorker.CancelAsync();
        }
    }
}
