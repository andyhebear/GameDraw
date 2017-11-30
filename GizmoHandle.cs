using GameDraw;
using System;
using UnityEngine;

public class GizmoHandle : MonoBehaviour
{
    public GizmoAxis axis;
    public Camera cam;
    public GizmoControl control = GizmoControl.Both;
    public Material highligh;
    public RuntimeManager manager;
    private bool mouseDown;
    public float moveSensitivity = 2f;
    public bool needUpdate;
    public Material original;
    private Transform otherTrans;
    public GameObject positionEnd;
    public GameObject rotationEnd;
    public float rotationSensitivity = 64f;
    public GameObject scaleEnd;
    public GizmoType type;
    public float val;

    private void Awake()
    {
        this.otherTrans = base.transform.parent;
        this.original = base.GetComponent<Renderer>().sharedMaterial;
    }

    private void OnMouseDown()
    {
        this.mouseDown = true;
    }

    private void OnMouseDrag()
    {
        Vector3 localScale;
        float num = 0f;
        if (!this.mouseDown)
        {
            return;
        }
        switch (this.control)
        {
            case GizmoControl.Horizontal:
                num = Input.GetAxis("Mouse X") * Time.deltaTime;
                break;

            case GizmoControl.Vertical:
                num = Input.GetAxis("Mouse Y") * Time.deltaTime;
                break;

            case GizmoControl.Both:
                num = (Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y")) * Time.deltaTime;
                break;
        }
        switch (this.type)
        {
            case GizmoType.Position:
                num *= this.moveSensitivity;
                switch (this.axis)
                {
                    case GizmoAxis.X:
                        this.otherTrans.Translate((Vector3) (Vector3.right * num));
                        break;

                    case GizmoAxis.Y:
                        this.otherTrans.Translate((Vector3) (Vector3.up * num));
                        break;

                    case GizmoAxis.Z:
                        this.otherTrans.Translate((Vector3) (Vector3.forward * num));
                        break;
                }
                goto Label_01E3;

            case GizmoType.Rotation:
                num *= this.rotationSensitivity;
                switch (this.axis)
                {
                    case GizmoAxis.X:
                        this.otherTrans.Rotate((Vector3) (Vector3.right * num));
                        break;

                    case GizmoAxis.Y:
                        this.otherTrans.Rotate((Vector3) (Vector3.up * num));
                        break;

                    case GizmoAxis.Z:
                        this.otherTrans.Rotate((Vector3) (Vector3.forward * num));
                        break;
                }
                goto Label_01E3;

            case GizmoType.Scale:
                num *= this.moveSensitivity;
                localScale = this.otherTrans.localScale;
                switch (this.axis)
                {
                    case GizmoAxis.X:
                        localScale.x += num;
                        goto Label_0169;

                    case GizmoAxis.Y:
                        localScale.y += num;
                        goto Label_0169;

                    case GizmoAxis.Z:
                        localScale.z += num;
                        goto Label_0169;
                }
                break;

            default:
                goto Label_01E3;
        }
    Label_0169:
        this.otherTrans.localScale = localScale;
    Label_01E3:
        this.val = num;
    }

    private void OnMouseEnter()
    {
        this.positionEnd.GetComponent<Renderer>().sharedMaterial = this.highligh;
        this.rotationEnd.GetComponent<Renderer>().sharedMaterial = this.highligh;
        this.scaleEnd.GetComponent<Renderer>().sharedMaterial = this.highligh;
    }

    private void OnMouseExit()
    {
        this.positionEnd.GetComponent<Renderer>().sharedMaterial = this.original;
        this.rotationEnd.GetComponent<Renderer>().sharedMaterial = this.original;
        this.scaleEnd.GetComponent<Renderer>().sharedMaterial = this.original;
    }

    private void OnMouseUp()
    {
        this.mouseDown = false;
        this.needUpdate = true;
    }

    public void setParent(Transform other)
    {
        this.otherTrans = other;
    }

    public void setType(GizmoType type)
    {
        this.type = type;
        this.positionEnd.active = type == GizmoType.Position;
        this.rotationEnd.active = type == GizmoType.Rotation;
        this.scaleEnd.active = type == GizmoType.Scale;
    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(this.cam.ScreenPointToRay(Input.mousePosition), out hit, float.PositiveInfinity, 8))
        {
            Debug.Log("Hit Handle");
        }
    }

    public enum GizmoAxis
    {
        X,
        Y,
        Z
    }

    public enum GizmoControl
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum GizmoType
    {
        Position,
        Rotation,
        Scale
    }
}

