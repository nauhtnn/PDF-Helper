using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLib
{
    public class ImageSegmenter
    {
        private static ImageSegmenter _instance = null;
        public static ImageSegmenter GetInstance()
        {
            if (_instance == null)
                _instance = new ImageSegmenter();
            return _instance;
        }

        List<(int start, int end)> SplitLines(Mat gray)
        {
            // Horizontal projection for line detection
            int[] rowSums = new int[gray.Rows];
            for (int y = 0; y < gray.Rows; y++)
                rowSums[y] = gray.Cols - Cv2.CountNonZero(gray.Row(y));

            List<(int start, int end)> lineRegions = new List<(int, int)>();
            int bottomLine = 0;
            int topBorder = -1;
            bool isBorderWaiting = false;
            for (int y = 0; y < rowSums.Length; y++)
            {
                if (rowSums[y] > 0)
                {
                    if (isBorderWaiting)
                    {
                        isBorderWaiting = false;

                        if (topBorder < 0)
                            topBorder = 0;
                        else
                        {
                            int bottomBorder = (bottomLine + y) / 2;
                            lineRegions.Add((topBorder, bottomBorder));
                            topBorder = bottomBorder + 1;
                        }
                    }

                    bottomLine = y;
                }
                else if (y - bottomLine > OcrSettings.GetInstance().LineProjectionThreshold)
                {
                    isBorderWaiting = true;
                }
            }

            if (topBorder < 0)
                topBorder = 0;

            if (gray.Rows > topBorder)
                lineRegions.Add((topBorder, gray.Rows - 1));

            return lineRegions;
        }

        public List<Mat> SegmentByProjection(Mat src)
        {
            List<Mat> segments = new List<Mat>();

            Mat gray = src;

            var lineRegions = SplitLines(gray);

            // For each line, detect columns
            foreach (var (start, end) in lineRegions)
            {
                using(Mat lineRegion = gray.RowRange(start, end))
                {
                    int[] colSums = new int[lineRegion.Cols];
                    for (int x = 0; x < lineRegion.Cols; x++)
                        colSums[x] = lineRegion.Col(x).Height - Cv2.CountNonZero(lineRegion.Col(x));

                    int rightLine = 0;
                    int leftBorder = -1;
                    bool isBorderWaiting = false;
                    for (int x = 0; x < colSums.Length; x++)
                    {
                        if (colSums[x] > 0)
                        {
                            if (isBorderWaiting)
                            {
                                isBorderWaiting = false;

                                if (leftBorder < 0)
                                    leftBorder = 0;
                                else
                                {
                                    int rightBorder = (rightLine + x) / 2;
                                    segments.Add(lineRegion.ColRange(leftBorder, rightBorder).Clone());
                                    leftBorder = rightBorder + 1;
                                }
                            }

                            rightLine = x;
                        }
                        else if (x - rightLine > OcrSettings.GetInstance().ColumnProjectionThreshold)
                        {
                            isBorderWaiting = true;
                        }
                    }

                    if (leftBorder < 0)
                        leftBorder = 0;

                    if (lineRegion.Cols > leftBorder)
                    {
                        segments.Add(lineRegion.ColRange(leftBorder, gray.Cols - 1).Clone());
                    }
                }
            }
#if DEBUG
            foreach(var seg in segments)
            {
                // Save each segment to file
                Cv2.ImWrite(PathHelper.Instance.GenerateLocalFile("segment.png"), seg);
            }
#endif
            return segments;
        }

        public List<Mat> SegmentImage2(Mat src)
        {
            // Apply Gaussian blur
            Mat blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);

            // Threshold (binary image)
            Mat thresh = new Mat();
            Cv2.Threshold(blurred, thresh, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

            // Find contours
            Cv2.FindContours(thresh, out Point[][] contours, out HierarchyIndex[] hierarchy,
                             RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // List to store cropped segments
            List<Mat> segments = new List<Mat>();

            int index = 0;
            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);

                // Optional: filter out very small regions (noise)
                if (rect.Width < 50 || rect.Height < 50)
                    continue;

                // Crop segment
                Mat segment = new Mat(src, rect);
                segments.Add(segment);

                index++;
            }

#if DEBUG
            StatusMessage.Instance.AddMessage("The number of segments: " + segments.Count);
#endif

            return segments;
        }
    }
}
