namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable]
    public class GDController
    {
        public GameDraw.GDMesh _gdmesh;
        public List<MeshBuffer> buffers;
        public bool built;
        public CharacterCustomizer characterCustomizer;
        private MeshCollider collider;
        public DecimatedMeshBuffer decimatedMesh;
        public bool disabled = true;
        [NonSerialized]
        public bool flag;
        public GUIBuffer gui;
        public bool isSkinned;
        public bool locked;
        public List<Mesh> meshes;
        private DateTime now;
        private MeshCollider objectCollider;
        public Renderer objectRenderer;
        public Mesh original;
        public LayerMask projectionMask;
        public bool Rebuild;
        public MeshFilter referencedFilter;
        public GameObject referencedGO;
        public SkinnedMeshRenderer referencedRenderer;
        [SerializeField]
        private int selected;
        public MeshSelection selection;
        public Space space;
        public bool useQuads;
        public static int vertexCount;
        public Mesh workingMesh;

        public GDController(GameObject go)
        {
            if (go != null)
            {
                this.referencedGO = go;
                this.decimatedMesh = new DecimatedMeshBuffer();
                this.buffers = new List<MeshBuffer>(10);
                this.referencedFilter = go.GetComponent<MeshFilter>();
                if ((this.referencedFilter != null) && (this.referencedFilter.sharedMesh != null))
                {
                    this.selection = new MeshSelection(this.referencedFilter.sharedMesh);
                    this.buffer = new MeshBuffer(this.referencedFilter.sharedMesh);
                }
                else
                {
                    this.referencedRenderer = go.GetComponent<SkinnedMeshRenderer>();
                    if ((this.referencedRenderer != null) && (this.referencedRenderer.sharedMesh != null))
                    {
                        this.buffer = new MeshBuffer(this.referencedRenderer.sharedMesh);
                        this.isSkinned = true;
                    }
                }
                this.referencedCollider = GDManager.getManager().GetComponent<MeshCollider>();
                this.objectCollider = this.referencedGO.GetComponent<MeshCollider>();
                this.objectRenderer = this.referencedGO.GetComponent<Renderer>();
                if (this.referencedCollider == null)
                {
                    this.referencedCollider = GDManager.getManager().gameObject.AddComponent<MeshCollider>();
                }
                if (!this.isSkinned)
                {
                    this.referencedCollider.sharedMesh = this.referencedFilter.sharedMesh;
                }
                else
                {
                    this.referencedCollider.sharedMesh = this.referencedRenderer.sharedMesh;
                }
                this.gui = new GUIBuffer();
                this.meshes = new List<Mesh>(10);
                this.original = this.referencedFilter.sharedMesh;
                this.meshes.Add(this.original);
                this.flag = true;
                this.characterCustomizer = new CharacterCustomizer();
            }
        }

        public GameDraw.GDMesh Build(  bool skinned=false,  Mesh m=null,  GameDraw.GDMesh hm=null)
        {
            this.now = DateTime.Now;
            if (m != null)
            {
                if (hm != null)
                {
                    this._gdmesh = MeshBuildUtil.Build(m, hm);
                }
                else
                {
                    this._gdmesh = MeshBuildUtil.Build(m, null);
                }
            }
            else if (!skinned)
            {
                if ((this.referencedFilter != null) && (this.referencedFilter.sharedMesh != null))
                {
                    if (hm != null)
                    {
                        this._gdmesh = MeshBuildUtil.Build(this.referencedFilter.sharedMesh, hm);
                    }
                    else
                    {
                        this._gdmesh = MeshBuildUtil.Build(this.referencedFilter.sharedMesh, null);
                    }
                }
            }
            else if ((this.referencedRenderer != null) && (this.referencedRenderer.sharedMesh != null))
            {
                if (hm != null)
                {
                    this._gdmesh = MeshBuildUtil.Build(this.referencedRenderer.sharedMesh, hm);
                }
                else
                {
                    this._gdmesh = MeshBuildUtil.Build(this.referencedRenderer.sharedMesh, null);
                }
            }
            this.built = true;
            return this._gdmesh;
        }

        public GameDraw.GDMesh BuildSingle( bool skinned=false,  Mesh m=null,  GameDraw.GDMesh hm=null)
        {
            this.now = DateTime.Now;
            if (m != null)
            {
                if (hm != null)
                {
                    this._gdmesh = MeshBuildUtil.Build(m, hm);
                }
                else
                {
                    this._gdmesh = MeshBuildUtil.Build(m, null);
                }
            }
            else if (!skinned)
            {
                if ((this.referencedFilter != null) && (this.referencedFilter.sharedMesh != null))
                {
                    if (hm != null)
                    {
                        this._gdmesh = MeshBuildUtil.Build(this.referencedFilter.sharedMesh, hm);
                    }
                    else
                    {
                        this._gdmesh = MeshBuildUtil.Build(this.referencedFilter.sharedMesh, null);
                    }
                }
            }
            else if ((this.referencedRenderer != null) && (this.referencedRenderer.sharedMesh != null))
            {
                if (hm != null)
                {
                    this._gdmesh = MeshBuildUtil.Build(this.referencedRenderer.sharedMesh, hm);
                }
                else
                {
                    this._gdmesh = MeshBuildUtil.Build(this.referencedRenderer.sharedMesh, null);
                }
            }
            this.built = true;
            return this._gdmesh;
        }

        public void CheckAndBuild( bool threaded=false)
        {
            if (!this.built)
            {
                if (threaded)
                {
                    this.Build(false, null, null);
                }
                else
                {
                    this.BuildSingle(false, null, null);
                }
                if ((GDManager.manager != null) && (this.mesh != null))
                {
                    GDManager.manager.buildFlippedMesh(this.mesh);
                }
            }
        }

        public void GuaranteeConsistency()
        {
            this.locked = true;
            for (int i = 0; i < this._gdmesh.vertices.Count; i++)
            {
                this._gdmesh.vertices[i].Traits.position = this.mesh.vertices[this._gdmesh.relatedVertices[this._gdmesh.vertices[i].Traits.hashCode][0]];
            }
            this.locked = false;
        }

        public void UpdateBuffer()
        {
            if (!this.isSkinned)
            {
                this.buffer.Update(this.referencedFilter.sharedMesh);
            }
            else
            {
                this.buffer.Update(this.referencedRenderer.sharedMesh);
            }
            this.buffer.selection = new MeshSelection(this.selection);
        }

        public void UpdateCollider()
        {
            this.referencedCollider.sharedMesh = null;
            this.referencedCollider.sharedMesh = this.referencedFilter.sharedMesh;
            if (this.objectCollider != null)
            {
                this.objectCollider.sharedMesh = null;
                this.objectCollider.sharedMesh = this.referencedFilter.sharedMesh;
            }
        }

        public MeshBuffer buffer
        {
            get
            {
                if (this.buffers == null)
                {
                    this.buffers = new List<MeshBuffer>(10);
                }
                if ((this.selected < this.buffers.Count) && (this.buffers[this.selected] != null))
                {
                    return this.buffers[this.selected];
                }
                this.buffers.Add(new MeshBuffer(this.referencedMesh));
                return this.buffers[this.buffers.Count - 1];
            }
            set
            {
                if (this.selected >= this.buffers.Count)
                {
                    this.buffers.Add(value);
                }
                else
                {
                    this.buffers[this.selected] = value;
                }
            }
        }

        public GameDraw.GDMesh GDMesh
        {
            get
            {
                if (this._gdmesh == null)
                {
                    if (this.buffer == null)
                    {
                        if (!this.isSkinned)
                        {
                            this.buffer = new MeshBuffer(this.referencedFilter.sharedMesh);
                        }
                        else
                        {
                            this.buffer = new MeshBuffer(this.referencedRenderer.sharedMesh);
                        }
                    }
                    if (!this.isSkinned)
                    {
                        this._gdmesh = this.Build(false, null, null);
                    }
                    else
                    {
                        this._gdmesh = this.Build(true, null, null);
                    }
                    return this._gdmesh;
                }
                if ((!this.flag || (this._gdmesh.relatedVertices == null)) || (this._gdmesh.relatedVertices.Count == 0))
                {
                    this.locked = true;
                    this._gdmesh.FillDictionary();
                    if (!this.isSkinned)
                    {
                        this.selection = this.buffer.selection;
                        if (this.buffer.vertices.Count == this._gdmesh.vertexCount)
                        {
                            this._gdmesh = MeshBuildUtil.Build(this.referencedFilter.sharedMesh, this._gdmesh);
                        }
                        else
                        {
                            this._gdmesh = MeshBuildUtil.Build(this.referencedFilter.sharedMesh, null);
                        }
                    }
                    else
                    {
                        this.selection = this.buffer.selection;
                        if (this.buffer.vertices.Count == this._gdmesh.vertexCount)
                        {
                            this._gdmesh = MeshBuildUtil.Build(this.referencedRenderer.sharedMesh, this._gdmesh);
                        }
                        else
                        {
                            this._gdmesh = MeshBuildUtil.Build(this.referencedRenderer.sharedMesh, null);
                        }
                    }
                    MeshBuildUtil.RemapAllocation(this._gdmesh);
                    this.locked = false;
                    this.flag = true;
                }
                return this._gdmesh;
            }
        }

        public Mesh mesh
        {
            get
            {
                try
                {
                    if (this.meshes != null)
                    {
                        if (this.meshes.Count > this.selected)
                        {
                            if (this.selected <= 0)
                            {
                                return this.original;
                            }
                            return this.meshes[this.selected];
                        }
                        this.selected = this.meshes.Count - 1;
                        return this.meshes[this.selected];
                    }
                    this.meshes = new List<Mesh>(10);
                    this.original = this.referencedMesh;
                    this.meshes.Add(this.original);
                    this.selected = 0;
                    return this.referencedMesh;
                }
                catch
                {
                    return this.referencedMesh;
                }
            }
            set
            {
                if (this.meshes != null)
                {
                    if (this.meshes.Count > this.selected)
                    {
                        this.meshes[this.selected] = value;
                    }
                    else
                    {
                        this.selected = this.meshes.Count - 1;
                        this.meshes[this.selected] = value;
                    }
                }
                else
                {
                    this.meshes = new List<Mesh>(10);
                    this.original = value;
                    this.meshes.Add(this.original);
                }
                this.referencedMesh = value;
            }
        }

        public MeshCollider referencedCollider
        {
            get
            {
                if (this.collider == null)
                {
                    this.collider = GDManager.getManager().gameObject.GetComponent<MeshCollider>();
                    if (this.collider == null)
                    {
                        GDManager.getManager().gameObject.AddComponent<MeshCollider>();
                    }
                }
                return this.collider;
            }
            set
            {
                this.collider = value;
            }
        }

        public Mesh referencedMesh
        {
            get
            {
                if (!this.isSkinned)
                {
                    if (this.referencedFilter == null)
                    {
                        this.referencedFilter = this.referencedGO.GetComponent<MeshFilter>();
                    }
                    return this.referencedFilter.sharedMesh;
                }
                if (this.referencedRenderer == null)
                {
                    this.referencedRenderer = this.referencedGO.GetComponent<SkinnedMeshRenderer>();
                }
                return this.referencedRenderer.sharedMesh;
            }
            set
            {
                if (!this.isSkinned)
                {
                    this.referencedFilter.sharedMesh = value;
                }
                else
                {
                    this.referencedRenderer.sharedMesh = value;
                }
                this.UpdateCollider();
            }
        }

        public Renderer renderer
        {
            get
            {
                if (this.objectRenderer != null)
                {
                    return this.objectRenderer;
                }
                if (this.referencedGO != null)
                {
                    return this.referencedGO.GetComponent<Renderer>();
                }
                return null;
            }
        }

        public int selectedMesh
        {
            get
            {
                return this.selected;
            }
            set
            {
                if (value == 0)
                {
                    this.selected = 0;
                }
                if ((value > 0) && (value < this.meshes.Count))
                {
                    this.selected = value;
                }
                else if (value < 0)
                {
                    this.selected = 0;
                }
                else if (value >= this.meshes.Count)
                {
                    Mesh item = (Mesh) UnityEngine.Object.Instantiate(this.original);
                    item.name = this.original.name;
                    this.meshes.Add(item);
                    this.selected = this.meshes.Count - 1;
                }
                this.referencedMesh = this.meshes[this.selected];
            }
        }

        public Transform transform
        {
            get
            {
                if (this.referencedFilter != null)
                {
                    return this.referencedFilter.transform;
                }
                return null;
            }
        }
    }
}

