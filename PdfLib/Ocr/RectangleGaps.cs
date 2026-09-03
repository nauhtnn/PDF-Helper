using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    //Abandoned approach. Reason: Too naive. Too crude. Cannot handle edge cases.
    public class RectangleGaps
    {
        public int TopLine { get; private set; }
        List<(int, int)> HorizontalRanges;
        public int Count { get; private set; }

        public RectangleGaps(int topLine)
        {
            TopLine = topLine;
            HorizontalRanges = new List<(int, int)>();
            Count = 0;
        }

        void AddRange(Tuple<int, int> t)
        {
            AddRange(t.Item1, t.Item2);
        }

        public void AddRange(int left, int right)
        {
            HorizontalRanges.Add((left, right));
            Count = HorizontalRanges.Count;
        }

        public static Tuple<int, int> Intersect((int, int) range1, (int, int) range2)
        {
            (int, int) leftRange;
            (int, int) rightRange;

            if(range1.Item1 <= range2.Item1)
            {
                leftRange = range1;
                rightRange = range2;
            }
            else
            {
                leftRange = range2;
                rightRange = range1;
            }

            if (leftRange.Item2 < rightRange.Item1)
                return null;
            else if (leftRange.Item2 <= rightRange.Item2)
                return new Tuple<int, int>(rightRange.Item1, leftRange.Item2);
            else
                return new Tuple<int, int>(rightRange.Item1, rightRange.Item2);
        }

        public RectangleGaps Merge(RectangleGaps theirGaps)
        {
            var mergedGaps = new RectangleGaps(Math.Min(TopLine, theirGaps.TopLine));

            var myRanges = HorizontalRanges.GetRange(0, Count - 1);
            var theirRanges = theirGaps.HorizontalRanges.GetRange(0, theirGaps.Count - 1);

            foreach(var myRange in myRanges)
            {
                for(int j = 0; j < theirRanges.Count; ++j)
                {
                    var newRange = Intersect(myRange, theirRanges.ElementAt(j));
                    if (newRange != null)
                    {
                        mergedGaps.AddRange(newRange);
                        theirRanges.RemoveAt(j);
                        break;
                    }
                }
            }

            return mergedGaps;
        }
    }
}
