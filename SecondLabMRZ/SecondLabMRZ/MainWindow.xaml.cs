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

namespace SecondLabMRZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double[] sequense;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void sequenceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            int index = comboBox.SelectedIndex;

            sequenceLabel.Text = GetSeqence(index);
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

            sequenceLabel.Text = GetSeqence(0);
        }

        private string GetSeqence(int index) 
        {
            string fromText = fromTextBox.Text;
            string toText = toTextBox.Text;

            int from = 0;
            int to = 0;

            if (!Int32.TryParse(fromText, out from) || !Int32.TryParse(toText, out to)) 
            {
                return String.Empty;
            }

            if (index == 0) 
            {
                FibonacciSequence fibonacciSequence = new FibonacciSequence();
                sequense = fibonacciSequence.GenerateSequence(from, to);
            }
            else if (index == 1)
            {
                FactorialSequence factorialSequence = new FactorialSequence();
                sequense = factorialSequence.GenerateSequence(from, to);
            }
            else if (index == 2)
            {
                PeriodicSequence periodicSequence = new PeriodicSequence();
                sequense = periodicSequence.GenerateSequence(from, to);
            }
            else if (index == 3)
            {
                PowerSequence powerSequence = new PowerSequence(2);
                sequense = powerSequence.GenerateSequence(from, to);
            }

            StringBuilder builder = new StringBuilder();

            foreach (int value in sequense) 
            {
                builder.Append(value.ToString() + " ");
            }

            return builder.ToString();
        }

        private void predictSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            string windowSizeText = lTextBox.Text;
            string imagesNumberText = pTextBox.Text;
            string learningCoefficientText = aTextBox.Text;
            string maxErrorText = errorTextBox.Text;
            string maxIterationsText = nTextBox.Text;

            int windowSize = 0;
            int imagesNumber = 0;
            double learningCoefficient = 0;
            double maxError = 0;
            int maxIterations = 0;

            if (!Int32.TryParse(windowSizeText, out windowSize) ||
                !Int32.TryParse(imagesNumberText, out imagesNumber) ||
                !Double.TryParse(learningCoefficientText, out learningCoefficient) ||
                !Double.TryParse(maxErrorText, out maxError) ||
                !Int32.TryParse(maxIterationsText, out maxIterations))
            {
                return;
            }

            Network network = new Network(windowSize, imagesNumber, learningCoefficient, maxError, maxIterations);

            network.Learn(sequense);
            double[] predictSequense = network.Predict(sequense, 5);

        }

        private void ShowPredictSequense(double[] predictSequense) 
        {
            StringBuilder builder = new StringBuilder();

            foreach (int value in predictSequense)
            {
                builder.Append(value.ToString() + " ");
            }

            predictedSequenceLabel.Text = builder.ToString();
        }
    }
}
