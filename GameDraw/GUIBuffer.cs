namespace GameDraw
{
    using System;
    using UnityEngine;

    [Serializable]
    public class GUIBuffer
    {
        public string[] createSelection = new string[] { "Primitives", "Create City", "Utilities" };
        public int createSelectionNum;
        public bool CreateStatesFromMesh;
        public Vector2 editScroll = Vector3.zero;
        public string[] editSelection = new string[] { "Mesh Editing", "Customizer" };
        public int editSelectionNum;
        public Vector2 generalScroll = Vector3.zero;
        public Mesh ImportedMesh;
        public int mainSelectionNum;
        public int materialID;
        public Texture2D primitiveIcon;
        public string primitiveName;
        public Texture2D SelectionBrushTexture;
        public bool SelectionByBrush;
        public int selectionNum;
        public float softSelectionAmount = 0.1f;
        public float SoftSelectionBrushRadius = 0.3f;
        public bool softSelectionOnChange;
        public bool subdivideSelection;
        public bool useFreeform;
        public bool useMaterialID;
        public bool useSoftSelection;
        public float weldThreshold;
    }
}

