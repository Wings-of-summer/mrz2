using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLabMRZ
{
    class FibonacciSequence
    {
        public double[] GenerateSequence(int from, int to)
        {
            double[] resultSequence = new double[to - from];

            for (int i = from; i < to; i++)
            {
                resultSequence[i - from] = Apply(i);
            }

            return resultSequence;
        }

        private int Apply(int x) {
            return (int) ((Math.Pow((1 + Math.Sqrt(5)) / 2, x) - Math.Pow((1 - Math.Sqrt(5)) / 2, x)) / Math.Sqrt(5));
        }
    }
}
