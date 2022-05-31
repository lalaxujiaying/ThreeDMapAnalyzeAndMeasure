using System.Collections;
using System.Collections.Generic;
using TDAAM.Analysis.Manager;
using UnityEngine;
namespace TDAAM.Tool
{
    public class CutAndFillAnalysisTool : TDAAM_Tool<CutAndFillAnalysisTool>
    {
        private Dictionary<string, ToolState> toolBuffer = new Dictionary<string, ToolState>();
        public bool Create(string toolName)
        {
            if (toolBuffer.TryGetValue(toolName, out ToolState toolState)) return false;
            CutAndFillManager analysis = TDAAM_Mono<CutAndFillManager>.Create("[CutAndFill]", toolName, out GameObject go);
            toolBuffer.Add(toolName, new ToolState(go, false, false, true));
            return true;
        }
    }
}