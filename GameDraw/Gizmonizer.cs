namespace GameDraw
{
    using System;
    using UnityEngine;

    public class Gizmonizer : MonoBehaviour
    {
        private Gizmo gizmo;
        public GameObject gizmoAxis;
        private GameObject gizmoObj;
        public float gizmoSize = 1f;
        private GizmoHandle.GizmoType gizmoType;

        private void OnMouseDown()
        {
            if (this.gizmoObj == null)
            {
                this.resetGizmo();
            }
        }

        private void removeGizmo()
        {
            if (this.gizmoObj != null)
            {
                base.gameObject.layer = 0;
                foreach (Transform transform in base.transform)
                {
                    transform.gameObject.layer = 0;
                }
                UnityEngine.Object.Destroy(this.gizmoObj);
                UnityEngine.Object.Destroy(this.gizmo);
            }
        }

        private void resetGizmo()
        {
            this.removeGizmo();
            base.gameObject.layer = 2;
            foreach (Transform transform in base.transform)
            {
                transform.gameObject.layer = 2;
            }
            this.gizmoObj = (GameObject) UnityEngine.Object.Instantiate(this.gizmoAxis, base.transform.position, base.transform.rotation);
            Transform transform1 = this.gizmoObj.transform;
            transform1.localScale = (Vector3) (transform1.localScale * this.gizmoSize);
            this.gizmo = (Gizmo) this.gizmoObj.GetComponent("Gizmo");
            this.gizmo.setParent(base.transform);
            this.gizmo.setType(this.gizmoType);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.removeGizmo();
            }
            if (this.gizmo != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    this.gizmoType = GizmoHandle.GizmoType.Position;
                    this.gizmo.setType(this.gizmoType);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    this.gizmoType = GizmoHandle.GizmoType.Rotation;
                    this.gizmo.setType(this.gizmoType);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    this.gizmoType = GizmoHandle.GizmoType.Scale;
                    this.gizmo.setType(this.gizmoType);
                }
                if (this.gizmo.needUpdate)
                {
                    this.resetGizmo();
                }
            }
        }
    }
}

