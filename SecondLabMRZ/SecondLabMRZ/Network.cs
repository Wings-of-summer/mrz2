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

        private RowVector weightMatrix2;

        private ColumnVector contextNeurons;

        private int delay = 0;

        private int logStep = 1;

        public Network(int windowSize, int imagesNumber, double learningCoefficient, double maxError, int maxIterations) {

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

        public double[] Predict(double[] sequence, int predictedAmount) {

            double[] predictedSequence = new double[predictedAmount];

            for (int i = 0; i < predictedAmount; i++) {

                double[] image = new double[windowSize];

                if (windowSize - i > 0) {
                    System.arraycopy(sequence, sequence.length - windowSize + i, image, 0, windowSize - i);
                    System.arraycopy(predictedSequence, 0, image, windowSize - i, i);
                }
                else {
                    System.arraycopy(predictedSequence, i - windowSize, image, 0, windowSize);
                }

                DoubleMatrix X = DoubleMatrix.concatVertically(new DoubleMatrix(image), contextNeurons);

                DoubleMatrix Y1 = weightMatrix1.mmul(X);
                DoubleMatrix Y2 = weightMatrix2.mmul(Y1);

                predictedSequence[i] = Y2.data[0];
            }

            return predictedSequence;
        }

        public void learn(double[] sequence) {
            DoubleMatrix[] learningMatrix = createLearningMatrix(sequence);
            double[] etalons = createEtalons(sequence);

            double totalError;
            int iterations = 0;

            do {

                // learn
                for (int i = 0; i < learningMatrix.length; i++) {

                    DoubleMatrix X = DoubleMatrix.concatVertically(learningMatrix[i], contextNeurons);

                    DoubleMatrix Xn = normalize(X);
                    double norm = X.norm2();

                    DoubleMatrix Y1 = weightMatrix1.mmul(Xn);
                    DoubleMatrix Y2 = weightMatrix2.mmul(Y1);

                    DoubleMatrix gamma2 = Y2.mul(norm).sub(etalons[i]);
                    DoubleMatrix gamma1 = gamma2.mmul(weightMatrix2);

                    weightMatrix1 = weightMatrix1.sub(Xn.mmul(gamma1.mul(learningCoefficient)));
                    weightMatrix2 = weightMatrix2.sub(Y1.mmul(gamma2.mul(learningCoefficient)));

                    contextNeurons = Y1;
                }

                totalError = 0;

                // calculate total error
                for (int i = 0; i < learningMatrix.length; i++) {

                    DoubleMatrix X = DoubleMatrix.concatVertically(learningMatrix[i], contextNeurons);

                    DoubleMatrix Xn = normalize(X);
                    double norm = X.norm2();

                    DoubleMatrix Y1 = weightMatrix1.mmul(Xn);
                    DoubleMatrix Y2 = weightMatrix2.mmul(Y1);

                    DoubleMatrix gamma2 = Y2.mul(norm).sub(etalons[i]);

                    totalError += pow(gamma2.data[0], 2);
                }

                iterations++;

                logByStep(iterations, totalError, logStep);
                makeDelay(delay);
            }
            while (totalError >= maxError && iterations <= maxIterations && !Thread.interrupted());

            logger.log(totalError, iterations);
        }

        public void setLogger(Logger logger) {
            this.logger = logger;
        }

        public void setDelay(int delay) {
            this.delay = delay;
        }

        public void setLogStep(int logStep) {
            this.logStep = logStep != 0 ? logStep : 1;
        }

        public DoubleMatrix getWeightMatrix1() {
            return weightMatrix1;
        }

        public DoubleMatrix getWeightMatrix2() {
            return weightMatrix2;
        }

        private double[] createEtalons(double[] sequence) {
            return Arrays.copyOfRange(sequence, windowSize, windowSize + imagesNumber);
        }

        private DoubleMatrix normalize(DoubleMatrix vector) {

            double normalizationValue = 0;
            for (int i = 0; i < vector.length; i++) {
                normalizationValue += pow(vector.get(i), 2);
            }

            if (normalizationValue != 0) {
                DoubleMatrix normalizedVector = new DoubleMatrix(vector.length);

                for (int i = 0; i < vector.length; i++) {
                    normalizedVector.data[i] = vector.get(i) / sqrt(normalizationValue);
                }

                return normalizedVector;
            }
            else {
                return vector;
            }
        }

        private DoubleMatrix[] createLearningMatrix(double[] sequence) {
            DoubleMatrix[] learningMatrix = new DoubleMatrix[imagesNumber];
            for (int i = 0; i < imagesNumber; i++) {
                learningMatrix[i] = new DoubleMatrix(Arrays.copyOfRange(sequence, i, i + windowSize));
            }
            return learningMatrix;
        }

        private void CreateRandomMatrix(RectangularMatrix weightMatrix) {
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

        private void makeDelay(int delay) {
            try {
                Thread.sleep(delay);
            }
            catch (InterruptedException e) {
                Thread.currentThread().interrupt();
            }
        }

        private void logByStep(int iterations, double totalError, int logStep) {
            if (iterations % logStep == 0) {
                logger.log(totalError, iterations);
            }
        }
    }
}
