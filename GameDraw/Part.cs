namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    [Serializable]
    public class Part
    {
        public List<Part> Child;
        public Part PARENT;
        public string PART_DESCRIPTION;
        public string PART_NAME;
        public List<GameDraw.State> PART_STATES;
        public List<string> Tags;
        public int[] VERTEX_INDEXES;

        public Part(int[] VERTEX_INDEXES)
        {
            this.PART_NAME = "";
            this.PART_DESCRIPTION = "";
            this.Child = new List<Part>();
            this.VERTEX_INDEXES = VERTEX_INDEXES;
            this.PART_STATES = new List<GameDraw.State>();
            this.PARENT = null;
            this.Tags = new List<string>();
        }

        public Part(int[] VERTEX_INDEXES, string PART_NAME, string PART_DESCRIPTION)
        {
            this.PART_NAME = PART_NAME;
            this.PART_DESCRIPTION = PART_DESCRIPTION;
            this.Child = new List<Part>();
            this.VERTEX_INDEXES = VERTEX_INDEXES;
            this.PART_STATES = new List<GameDraw.State>();
            this.PARENT = null;
            this.Tags = new List<string>();
        }

        public Part(int[] VERTEX_INDEXES, string PART_NAME, string PART_DESCRIPTION, params string[] Tag)
        {
            this.PART_NAME = PART_NAME;
            this.PART_DESCRIPTION = PART_DESCRIPTION;
            this.Child = new List<Part>();
            this.VERTEX_INDEXES = VERTEX_INDEXES;
            this.PART_STATES = new List<GameDraw.State>();
            this.PARENT = null;
            this.Tags = new List<string>();
            foreach (string str in Tag)
            {
                this.Tags.Add(str);
            }
        }

        public void AddPart(string Name, int[] VERTEX_INDEXES)
        {
            this.AddPart(Name, "", VERTEX_INDEXES);
        }

        public void AddPart(string Name, string Description, int[] VERTRX_INDEXES)
        {
            Part item = new Part(VERTRX_INDEXES, Name, Description);
            item.PARENT = this;
            this.Child.Add(item);
        }

        public void AddPart(string Name, string Description, int[] VERTEX_INDEXES, params string[] Tag)
        {
            Part item = new Part(VERTEX_INDEXES, Name, Description, Tag);
            item.PARENT = this;
            this.Child.Add(item);
            foreach (string str in Tag)
            {
                this.Tags.Add(str);
            }
        }

        public void AddState(string name, string Description, List<Vector3> verts)
        {
            GameDraw.State item = new GameDraw.State(verts);
            item.Name = name;
            item.Description = Description;
            if (this.PART_STATES == null)
            {
                this.PART_STATES = new List<GameDraw.State>();
            }
            this.PART_STATES.Add(item);
        }

        public void AddState(string name, string Description, List<Vector3> verts, params string[] Tag)
        {
            GameDraw.State item = new GameDraw.State(verts, Tag);
            item.Name = name;
            item.Description = Description;
            if (this.PART_STATES == null)
            {
                this.PART_STATES = new List<GameDraw.State>();
            }
            this.PART_STATES.Add(item);
        }

        private Part getRoot(Part part)
        {
            if (part.PARENT != null)
            {
                return this.getRoot(part.PARENT);
            }
            return part;
        }

        public void RemovePart(Part part)
        {
            this.Child.Remove(part);
        }

        public void RemovePartAt(int index)
        {
            this.Child.RemoveAt(index);
        }

        public Part Root()
        {
            return this.getRoot(this);
        }

        public Part Parent
        {
            get
            {
                return this.Parent;
            }
        }

        public string PartDescription
        {
            get
            {
                return this.PART_DESCRIPTION;
            }
            set
            {
                this.PART_DESCRIPTION = value;
            }
        }

        public string PartName
        {
            get
            {
                return this.PART_NAME;
            }
            set
            {
                this.PART_NAME = value;
            }
        }

        public int[] VertexIndexes
        {
            get
            {
                return this.VERTEX_INDEXES;
            }
        }
    }
}

