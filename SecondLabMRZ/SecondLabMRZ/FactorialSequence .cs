using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLabMRZ
{
    class FactorialSequence
    {
        public int[] GenerateSequence(int from, int to)
        {
            int[] resultSequence = new int[to - from];

            for (int i = from; i < to; i++)
            {
                resultSequence[i - from] = Apply(i);
            }

            return resultSequence;
        }

        private int Apply(int x)
        {
            return x;
        }
    }
}
