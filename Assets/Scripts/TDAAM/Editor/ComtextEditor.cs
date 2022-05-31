using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ComtextEditor
{
    static bool isOpenTest_ShowMesh = false;
    [MenuItem("OpenMacro/OpenTest_ShowMeshMacro")]
	static void OpenTest_ShowMeshMacro()
	{
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        string ori = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        List<string> defineSymbols = new List<string>(ori.Split(';'));
        for (int i = 0; i < defineSymbols.Count; ++i)
        {
            if (defineSymbols[i] == "Test_ShowMesh")
            {
                isOpenTest_ShowMesh = true;
                break;
            }
            if (i == defineSymbols.Count - 1) isOpenTest_ShowMesh = false;
        }
        if (isOpenTest_ShowMesh)
		{
            defineSymbols.Remove("Test_ShowMesh");
            isOpenTest_ShowMesh = false;
        }
        else
		{
            defineSymbols.Add("Test_ShowMesh");
            isOpenTest_ShowMesh = true;
        }
        Menu.SetChecked("OpenMacro/OpenTest_ShowMeshMacro", isOpenTest_ShowMesh);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));

    }
}
