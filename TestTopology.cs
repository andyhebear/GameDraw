using GameDraw;
using System;
using UnityEngine;

[ExecuteInEditMode]
public class TestTopology : MonoBehaviour
{
    public string constructionTime;
    public float halfEdgeCount;
    public GDMesh m;

    private void Update()
    {
        if (this.m == null)
        {
            DateTime now = DateTime.Now;
            MeshFilter component = base.GetComponent<MeshFilter>();
            if ((component != null) && (component.sharedMesh != null))
            {
                this.m = MeshBuildUtil.Build(component.sharedMesh, null);
            }
            if (this.m != null)
            {
                this.constructionTime = ((TimeSpan) (DateTime.Now - now)).ToString();
            }
        }
    }
}

