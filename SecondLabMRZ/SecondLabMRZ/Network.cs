using Meta.Numerics.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private RectangularMatrix weightMatrix2;

        private RectangularMatrix contextNeurons;

        private int delay = 0;

        private int logStep = 1;

        public Network(int windowSize, int imagesNumber, double learningCoefficient, double maxError, int maxIterations)
        {

            this.windowSize = windowSize;
            this.imagesNumber = imagesNumber;
            this.learningCoefficient = learningCoefficient;
            this.maxError = maxError;
            this.maxIterations = maxIterations;

            weightMatrix1 = new RectangularMatrix(imagesNumber, windowSize + imagesNumber);
            CreateRandomMatrix(weightMatrix1);
            weightMatrix2 = new RectangularMatrix(1, imagesNumber);
            CreateRandomMatrix(weightMatrix2);
            contextNeurons = new RectangularMatrix(imagesNumber, 1);
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

                RectangularMatrix imageMatrix = new RectangularMatrix(image.Length, 1);

                for (int j = 0; j < image.Length; j++)
                {
                    imageMatrix[j, 0] = image[j];
                }

                RectangularMatrix X = ConcatVertically(imageMatrix, contextNeurons);

                RectangularMatrix Y1 = weightMatrix1 * X;
                RectangularMatrix Y2 = weightMatrix2 * Y1;

                predictedSequence[i] = Y2[0, 0];
            }

            return predictedSequence;
        }

        public void Learn(double[] sequence)
        {
            RectangularMatrix[] learningMatrix = createLearningMatrix(sequence);
            double[] etalons = createEtalons(sequence);

            double totalError;
            int iterations = 0;

            do
            {

                // learn
                for (int i = 0; i < learningMatrix.Length; i++)
                {

                    RectangularMatrix X = ConcatVertically(learningMatrix[i], contextNeurons);

                    RectangularMatrix Xn = Normalize(X);
                    double norm = X.FrobeniusNorm();

                    RectangularMatrix Y1 = weightMatrix1 * Xn;
                    RectangularMatrix Y2 = weightMatrix2 * Y1;

                    RectangularMatrix gamma2 = (Y2 * norm) - CreateScalarMatrix(etalons[i], Y2.RowCount);
                    RectangularMatrix gamma1 = gamma2 * weightMatrix2;

                    weightMatrix1 = weightMatrix1 - (Xn * (gamma1 * learningCoefficient));
                    weightMatrix2 = weightMatrix2 - (Y1 * (gamma2 * learningCoefficient));

                    contextNeurons = Y1;
                }

                totalError = 0;

                // calculate total error
                for (int i = 0; i < learningMatrix.Length; i++)
                {

                    RectangularMatrix X = ConcatVertically(learningMatrix[i], contextNeurons);

                    RectangularMatrix Xn = Normalize(X);
                    double norm = X.FrobeniusNorm();

                    RectangularMatrix Y1 = weightMatrix1 * Xn;
                    RectangularMatrix Y2 = weightMatrix2 * Y1;

                    RectangularMatrix gamma2 = Y2 * norm - CreateScalarMatrix(etalons[i], Y2.RowCount);

                    totalError += Math.Pow(gamma2[0, 0], 2);
                }

                iterations++;

                //logByStep(iterations, totalError, logStep);
            }
            while (totalError >= maxError && iterations <= maxIterations);

            //logger.log(totalError, iterations);
        }

        public void setDelay(int delay)
        {
            this.delay = delay;
        }

        public void setLogStep(int logStep)
        {
            this.logStep = logStep != 0 ? logStep : 1;
        }

        public RectangularMatrix getWeightMatrix1()
        {
            return weightMatrix1;
        }

        public RectangularMatrix getWeightMatrix2()
        {
            return weightMatrix2;
        }

        private double[] createEtalons(double[] sequence)
        {
            double[] etalons = new double[imagesNumber];
            Array.Copy(sequence, windowSize, etalons, 0, imagesNumber);
            return etalons;
        }

        private RectangularMatrix Normalize(RectangularMatrix vector)
        {
            double normalizationValue = 0;
            for (int i = 0; i < vector.RowCount; i++)
            {
                normalizationValue += Math.Pow(vector[i, 0], 2);
            }

            if (normalizationValue != 0)
            {
                RectangularMatrix normalizedVector = new RectangularMatrix(vector.RowCount, vector.ColumnCount);

                for (int i = 0; i < vector.RowCount; i++)
                {
                    normalizedVector[i, 0] = vector[i, 0] / Math.Sqrt(normalizationValue);
                }

                return normalizedVector;
            }
            else
            {
                return vector;
            }
        }

        private RectangularMatrix[] createLearningMatrix(double[] sequence)
        {
            RectangularMatrix[] learningMatrix = new RectangularMatrix[imagesNumber];
            for (int i = 0; i < imagesNumber; i++)
            {
                RectangularMatrix matrix = new RectangularMatrix(windowSize, 1);
                for (int j = i; j < i + windowSize; j++)
                {
                    matrix[j - i, 0] = sequence[i];
                }
                learningMatrix[i] = matrix;
            }
            return learningMatrix;
        }

        private void CreateRandomMatrix(RectangularMatrix weightMatrix)
        {
            Random rand = new Random();
            for (int i = 0; i < weightMatrix.ColumnCount; i++)
            {
                for (int j = 0; j < weightMatrix.RowCount; j++)
                {
                    weightMatrix[i, j] = rand.NextDouble() * 0.1;
                }
            }
        }

        private void CreateRandomRowVector(RowVector weightMatrix)
        {
            Random rand = new Random();
            for (int j = 0; j < weightMatrix.RowCount; j++)
            {
                weightMatrix[0, j] = rand.NextDouble() * 0.1;
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

        public static RectangularMatrix ConcatVertically(RectangularMatrix A, RectangularMatrix B)
        {
            RectangularMatrix result = new RectangularMatrix(A.RowCount + B.RowCount, A.ColumnCount);

            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; i < A.ColumnCount; j++)
                {
                    result[i, j] = A[i, j];
                }
            }

            for (int i = A.RowCount; i < A.RowCount + B.RowCount; i++)
            {
                for (int j = 0; i < B.ColumnCount; j++)
                {
                    result[i, j] = A[i - B.RowCount, j];
                }
            }

            return result;
        }
    }
}
