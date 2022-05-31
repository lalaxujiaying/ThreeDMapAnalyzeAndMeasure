using System;

namespace TDAAM.Tool.Editor
{
    public abstract class ITDAAM_Window
    {
        public Action OnQuitAction;
        public abstract void OnDraw();
    }
}
