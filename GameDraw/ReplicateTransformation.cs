namespace GameDraw
{
    using System;
    using UnityEngine;

    public class ReplicateTransformation : MonoBehaviour
    {
        public Transform target;

        private void Update()
        {
            if (this.target != null)
            {
                base.transform.position = this.target.transform.position;
                base.transform.rotation = this.target.transform.rotation;
                base.transform.localScale = this.target.transform.lossyScale;
            }
        }
    }
}

