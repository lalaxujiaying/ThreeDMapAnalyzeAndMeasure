using System.Collections;
using TDAAM.Tool;
namespace TDAAM
{
    public class AnalyzeAndMeasureTools
    {
        public static DistanceMeasureTool DistanceMeasure
        {
            get { return DistanceMeasureTool.GetTool(); }
        }
        public static VisibilityAnalysisTool VisibilityAnalysis
        {
            get { return VisibilityAnalysisTool.GetTool(); }
        }
        public static RadarAnalysisTool RadarAnalysis
        {
            get { return RadarAnalysisTool.GetTool(); }
        }
        public static CutAndFillAnalysisTool CutAndFillAnalysis
        {
            get { return CutAndFillAnalysisTool.GetTool(); }
        }
        public static VisibleRangeAnalysisTool VisibleRangeAnalysis
        {
            get { return VisibleRangeAnalysisTool.GetTool(); }
        }
    }
}
