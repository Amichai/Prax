using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.Recognition
{
    class HeuristicsControlPanel
    {
        private int[] probabilityHistorgram = new int[21];
        public void buildHeuristicProbabilityHistorgram(double probability, int labelUnderInspection, int heuristicUnderInspection)
        {
            if (probability == 1)
                probabilityHistorgram[20]++;
            else
                for (int i = 0; i < 20; i++)
                    if (probability >= 0 * .05 && probability < (i + 1) * .05)
                    {
                        probabilityHistorgram[i]++;
                        i = 20;
                    }
        }

        //TODO: Improve the lookup of labels
        //Consider ignoring heuristics that are proven to be useless

        //In order to improve the lookup of labels, expose a variable for the width of the segment so it can be adjusted as words are being resolved
        //do something similar for segmentation


        //Automated training - compare existing coordinates with list of exact coordinates
    }
}
