using Meta.Numerics.Matrices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecondLabMRZ
{
    class Network
    {
        private int windowSize;

        private int imagesNumber;

        private double learningCoefficient;

        private double maxError;

        private double maxIterations;

        private RectangularMatrix weightMatrix1;

        private RowVector weightMatrix2;

        private ColumnVector contextNeurons;

        public BackgroundResult backgroundResult { get; set; }

        public Network(int windowSize, int imagesNumber, double learningCoefficient, double maxError, int maxIterations)
        {

            this.windowSize = windowSize;
            this.imagesNumber = imagesNumber;
            this.learningCoefficient = learningCoefficient;
            this.maxError = maxError;
            this.maxIterations = maxIterations;

            weightMatrix1 = new RectangularMatrix(imagesNumber, windowSize + imagesNumber);
            CreateRandomMatrix(weightMatrix1);
            weightMatrix2 = new RowVector(imagesNumber);
            CreateRandomRowVector(weightMatrix2);
            contextNeurons = new ColumnVector(imagesNumber);
        }

        public double[] Predict(double[] sequence, int predictedAmount)
        {

            double[] predictedSequence = new double[predictedAmount];

            for (int i = 0; i < predictedAmount; i++)
            {

                double[] image = new double[windowSize];

                if (windowSize - i > 0)
                {
                    Array.Copy(sequence, sequence.Length - windowSize + i, image, 0, windowSize - i);
                    Array.Copy(predictedSequence, 0, image, windowSize - i, i);
                }
                else
                {
                    Array.Copy(predictedSequence, i - windowSize, image, 0, windowSize);
                }

                ColumnVector imageMatrix = new ColumnVector(image.Length);

                for (int j = 0; j < image.Length; j++)
                {
                    imageMatrix[j] = image[j];
                }

                ColumnVector X = ConcatVertically(imageMatrix, contextNeurons);

                ColumnVector Y1 = weightMatrix1 * X;
                double Y2 = weightMatrix2 * Y1;

                predictedSequence[i] = Y2;
            }

            return predictedSequence;
        }

        public void Learn(BackgroundWorker backgroundWorker, DoWorkEventArgs e, double[] sequence, bool? showIteration)
        {
            ColumnVector[] learningMatrix = createLearningMatrix(sequence);
            double[] etalons = createEtalons(sequence);

            backgroundResult = new BackgroundResult();
            backgroundResult.IsComplete = false;

            double totalError;
            int iterations = 0;

            do
            {
                // learn
                for (int i = 0; i < learningMatrix.Length; i++)
                {

                    ColumnVector X = ConcatVertically(learningMatrix[i], contextNeurons);

                    ColumnVector Xn = Normalize(X);
                    double norm = X.FrobeniusNorm();

                    ColumnVector Y1 = weightMatrix1 * Xn;
                    double Y2 = weightMatrix2 * Y1;

                    double gamma2 = (Y2 * norm) - etalons[i];
                    RowVector gamma1 = gamma2 * weightMatrix2;

                    RowVector a = learningCoefficient * gamma1;
                    RectangularMatrix b = Xn * a;

                    weightMatrix1 = weightMatrix1 - b.Transpose();
                    weightMatrix2 = weightMatrix2 - ((gamma2 * learningCoefficient) * Y1).Transpose();

                    contextNeurons = Y1;
                }

                totalError = 0;

                // calculate total error
                for (int i = 0; i < learningMatrix.Length; i++)
                {

                    ColumnVector X = ConcatVertically(learningMatrix[i], contextNeurons);

                    ColumnVector Xn = Normalize(X);
                    double norm = X.FrobeniusNorm();

                    ColumnVector Y1 = weightMatrix1 * Xn;
                    double Y2 = weightMatrix2 * Y1;

                    double gamma2 = Y2 * norm - etalons[i];

                    totalError += Math.Pow(gamma2, 2);
                }

                backgroundResult.IterationNumber = iterations;
                backgroundResult.Error = totalError;
                backgroundResult.IsComplete = false;

                if (showIteration.Equals(true)) 
                {
                    backgroundWorker.ReportProgress(0, backgroundResult);

                    Thread.Sleep(200);

                    if (backgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                iterations++;
            }
            while (totalError >= maxError && iterations <= maxIterations);

            backgroundResult.IterationNumber = iterations;
            backgroundResult.Error = totalError;
            backgroundResult.IsComplete = true;

            backgroundWorker.ReportProgress(0, backgroundResult);
        }

        private double[] createEtalons(double[] sequence)
        {
            double[] etalons = new double[imagesNumber + 1];
            Array.Copy(sequence, windowSize, etalons, 0, imagesNumber);
            return etalons;
        }

        private ColumnVector Normalize(ColumnVector vector)
        {
            double normalizationValue = 0;
            for (int i = 0; i < vector.RowCount; i++)
            {
                normalizationValue += Math.Pow(vector[i], 2);
            }

            if (normalizationValue != 0)
            {
                ColumnVector normalizedVector = new ColumnVector(vector.RowCount);

                for (int i = 0; i < vector.RowCount; i++)
                {
                    normalizedVector[i] = vector[i] / Math.Sqrt(normalizationValue);
                }

                return normalizedVector;
            }
            else
            {
                return vector;
            }
        }

        private ColumnVector[] createLearningMatrix(double[] sequence)
        {
            ColumnVector[] learningMatrix = new ColumnVector[imagesNumber];
            for (int i = 0; i < imagesNumber; i++)
            {
                double[] data = new double[windowSize];
                Array.Copy(sequence, i, data, 0, windowSize);

                ColumnVector matrix = new ColumnVector(windowSize);
                for (int j = 0; j < windowSize; j++)
                {
                    matrix[j] = data[j];
                }
                learningMatrix[i] = matrix;
            }
            return learningMatrix;
        }

        private void CreateRandomMatrix(RectangularMatrix weightMatrix)
        {
            Random rand = new Random();
            for (int i = 0; i < weightMatrix.RowCount; i++)
            {
                for (int j = 0; j < weightMatrix.ColumnCount; j++)
                {
                    weightMatrix[i, j] = rand.NextDouble() * 0.01;
                }
            }
        }

        private void CreateRandomRowVector(RowVector weightMatrix)
        {
            Random rand = new Random();
            for (int j = 0; j < weightMatrix.ColumnCount; j++)
            {
                weightMatrix[j] = rand.NextDouble() * 0.1;
            }
        }

        private RectangularMatrix CreateScalarMatrix(double value, int size)
        {
            RectangularMatrix scalarMatrix = new RectangularMatrix(size, size);

            for (int i = 0; i < size; i++)
            {
                scalarMatrix[i, i] = value;
            }

            return scalarMatrix;
        }

        public static ColumnVector ConcatVertically(ColumnVector A, ColumnVector B)
        {
            ColumnVector result = new ColumnVector(A.RowCount + B.RowCount);

            for (int i = 0; i < A.RowCount; i++)
            {
                result[i] = A[i];
            }

            for (int i = A.RowCount; i < A.RowCount + B.RowCount; i++)
            {
                result[i] = B[i - A.RowCount];
            }

            return result;
        }

        public class BackgroundResult
        {
            public int IterationNumber;
            public double Error;
            public bool IsComplete;
        }
    }
}
