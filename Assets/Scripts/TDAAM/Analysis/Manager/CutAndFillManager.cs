using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TDAAM.Analysis.Manager
{
    public class CutAndFillManager : TDAAM_Mono<CutAndFillManager>
    {
        [SerializeField]
        private MeshFilter[] meshFilters;
        bool isCanCreate = true;
        public Texture icon;

        public List<CutAndFillAnalysis> childScirpts = new List<CutAndFillAnalysis>();

        public bool isAll = true;
        public event Action<CutAndFillAnalysis> OnCreateAnalysisCompleted;
        public bool isStop = false;

        void Update()
        {
            if (isStop) return;

            if (Input.GetMouseButtonDown(0) && isCanCreate)
            {
                CreateCutAndFillFunc();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                isCanCreate = true;

                foreach (var childScirpt in childScirpts)
                {
                    Destroy(childScirpt.gameObject);
                }
                childScirpts.Clear();
            }

        }

        private void CreateCutAndFillFunc()
        {
            if (CombineMesh())
            {
                isCanCreate = false;
                GameObject childGo = new GameObject("CutAndFill");
                childGo.AddComponent<MeshFilter>().mesh = combineAllMesh;
                childGo.transform.parent = transform;
                childGo.transform.position = Vector3.zero;
                Shader shader = Shader.Find("Unlit/CutAndFillShader");
                Material mat = new Material(shader);
                childGo.AddComponent<MeshRenderer>().material = mat;
                var script = childGo.AddComponent<CutAndFillAnalysis>();
                script.OnAnalysisCompleted += () =>
                {
                    //ui = new CutAndFill_UI();
                    //ui.ShowUI(transform);
                    isCanCreate = true;
                };
                script.icon = icon;
                script.Init();
                childScirpts.Add(script);
                OnCreateAnalysisCompleted?.Invoke(script);
            }
        }
        Mesh combineAllMesh;
        private bool CombineMesh()
        {
            if (combineAllMesh == null)
            {
                combineAllMesh = new Mesh();
                if (meshFilters == null || meshFilters.Length == 0)
                {
                    Debug.LogError("没有可以放置需要操作的mesh");
                    return false;
                }
                CombineInstance[] combineInstance = new CombineInstance[meshFilters.Length];
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    combineInstance[i].mesh = meshFilters[i].sharedMesh;
                    combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix;
                }
                combineAllMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combineAllMesh.CombineMeshes(combineInstance);       //合并
                return true;
            }
            return true;
        }

    }
}