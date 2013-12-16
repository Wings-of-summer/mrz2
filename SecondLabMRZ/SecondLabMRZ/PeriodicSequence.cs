using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLabMRZ
{
    class PeriodicSequence
    {
        private const int PERIOD_LENGTH = 4;

        private const Dictionary<int, int> NUMBER_AT_PERIOD_POSITION = new Dictionary<int, int>() {{0, 1}, {1, 0}, {2, -1}, {3, 0}};

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
            int positionInPeriod = x - (x / PERIOD_LENGTH) * PERIOD_LENGTH;
            return NUMBER_AT_PERIOD_POSITION[positionInPeriod];
        }
    }
}
