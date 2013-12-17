using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLabMRZ
{
    class PowerSequence
    {
        private int number;

        public PowerSequence(int number) 
        {
            this.number = number;
        }

        public double[] GenerateSequence(int from, int to)
        {
            double[] resultSequence = new double[to - from];

            for (int i = from; i < to; i++)
            {
                resultSequence[i - from] = Apply(i);
            }

            return resultSequence;
        }

        public int Apply(int x) 
        {
            return (int) Math.Pow(number, x);
        }
    }
}
