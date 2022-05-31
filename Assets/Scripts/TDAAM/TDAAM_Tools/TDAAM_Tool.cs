using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TDAAM.Tool
{
    public class ToolState
    {
        public GameObject toolGo;
        public bool isHide;
        public bool isStop;
        public bool isStart;

        public ToolState(GameObject toolGo, bool isHide, bool isStop, bool isStart)
        {
            this.toolGo = toolGo;
            this.isHide = isHide;
            this.isStop = isStop;
            this.isStart = isStart;
        }
        public override string ToString()
        {
            return toolGo.ToString() + " " + isHide + " " + isStop + " " + isStart;
        }
    }
    public abstract class TDAAM_Tool<T> where T : TDAAM_Tool<T>, new()
    {

        private static T instance;
        public static T GetTool()
        {
            if (instance == null)
            {
                instance = new T();
                return instance;
            }
            else return instance;
        }
    }
}