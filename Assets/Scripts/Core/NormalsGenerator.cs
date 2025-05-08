using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class NormalsGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Smooth Normals")]
    static void GenerateSmoothNormals()
    {
        GameObject SelectedObject_ = Selection.activeGameObject;
        if (SelectedObject_ == null)
        {
            return;
        }

        MeshFilter MeshFilter_ = SelectedObject_.GetComponent<MeshFilter>();
        if (MeshFilter_ == null || MeshFilter_.sharedMesh == null)
        {
            return;
        }

        Mesh OriginalMesh_ = MeshFilter_.sharedMesh;
        Mesh NewMesh_ = Instantiate(OriginalMesh_);
        Vector3[] Vertices_ = NewMesh_.vertices;
        Vector3[] Normals_ = NewMesh_.normals;
        Vector3[] SmoothNormals_ = new Vector3[Normals_.Length];

        var Groups_ = new Dictionary<Vector3, List<int>>(new Vector3EqualityComparer(1e-6f));
        for (int i = 0; i < NewMesh_.vertexCount; i++)
        {
            Vector3 Pos_ = Vertices_[i];
            if (!Groups_.ContainsKey(Pos_))
                Groups_[Pos_] = new List<int>();
            Groups_[Pos_].Add(i);
        }

        foreach (var Group_ in Groups_)
        {
            Vector3 Sum_ = Vector3.zero;
            foreach (int i in Group_.Value)
                Sum_ += Normals_[i];

            Sum_.Normalize();

            foreach (int i in Group_.Value)
                SmoothNormals_[i] = Sum_;
        }

        NewMesh_.SetUVs(2, SmoothNormals_.ToList());
        NewMesh_.name = OriginalMesh_.name + "_SmoothNormals";

        string Path_ = "Assets/" + NewMesh_.name + ".asset";
        AssetDatabase.CreateAsset(NewMesh_, Path_);
        AssetDatabase.SaveAssets();
    }

    class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        private readonly float Tolerance_;

        public Vector3EqualityComparer(float tolerance)
        {
            this.Tolerance_ = tolerance;
        }

        public bool Equals(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude < Tolerance_ * Tolerance_;
        }

        public int GetHashCode(Vector3 obj)
        {
            unchecked
            {
                int Hash_ = 17;
                Hash_ = Hash_ * 23 + Mathf.RoundToInt(obj.x / Tolerance_);
                Hash_ = Hash_ * 23 + Mathf.RoundToInt(obj.y / Tolerance_);
                Hash_ = Hash_ * 23 + Mathf.RoundToInt(obj.z / Tolerance_);
                return Hash_;
            }
        }
    }
}

// © 2025 KOIYOT. All rights reserved.