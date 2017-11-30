namespace GameDraw
{
    using System;
    using UnityEngine;

    public class GLHighlight : MonoBehaviour
    {
        public RuntimeManager runtime;

        private void OnPostRender()
        {
            this.runtime.DrawSelection();
            this.runtime.HighlightSelection();
        }

        private void Start()
        {
            this.runtime = base.GetComponent<RuntimeManager>();
        }
    }
}

