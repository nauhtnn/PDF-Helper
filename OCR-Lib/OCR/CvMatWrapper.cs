using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace OCR_Lib
{
    //Abandoned approach. Reason: Too naive. Too crude. Cannot handle edge cases.
    public class CvMatWrapper
    {
        Mat _region;
        Tuple<int, int> _origLeftTop;
        Tuple<int, int> _textArea;

        RectangleGaps GetGaps(int r, int height)
        {
            RectangleGaps gaps = new RectangleGaps(r);

            int[] colSums = new int[_region.Cols];
            for (int x = 0; x < _region.Cols; x++)
                colSums[x] = _region.Col(x).Height - Cv2.CountNonZero(_region.Col(x));

            bool hasGap = false;
            int rightLine = 0;
            for (int x = 0; x < colSums.Length; ++x)
            {
                if (colSums[x] > 0)
                {
                    if (hasGap)
                    {
                        hasGap = false;
                        gaps.AddRange(rightLine + 1, x - 1);
                    }

                    rightLine = x;
                }
                else if (x - rightLine > OcrSettings.GetInstance().ColumnProjectionThreshold)
                {
                    hasGap = true;
                }
            }

            return gaps;
        }

        void SegmentByGridScan(Mat src)
        {
            List<RectangleGaps> separators = new List<RectangleGaps>();

            int y = 0;
            int step = OcrSettings.GetInstance().LineProjectionThreshold;
            while(y + step < src.Rows)
            {
                separators.Add(GetGaps(y, step));
                y += step;
            }

            if(y < src.Rows)
                separators.Add(GetGaps(y, src.Rows - y));

            var sepItor = separators.GetEnumerator();
            var prevSep = sepItor;
            bool isFirst = true;
            RectangleGaps currentSep = null;
            while(sepItor.MoveNext())
            {
                if(isFirst)
                {
                    prevSep = sepItor;
                    currentSep = prevSep.Current;
                    isFirst = false;
                }
                else if(currentSep.Count == sepItor.Current.Count)
                {
                    var newSep = currentSep.Merge(sepItor.Current);

                    if (newSep.Count == currentSep.Count)
                        currentSep = newSep;
                }
                else if(currentSep.Count == 0 && sepItor.Current.Count > 0)
                {
                    //Split
                }
                else if (currentSep.Count > 0 && sepItor.Current.Count == 0)
                {
                    //Split
                }
            }
        }
    }
}
