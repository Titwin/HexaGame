using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegKinematic : MonoBehaviour
{
    [Header("Kinematic chain parameters")]
    public Transform kneeTransform;
    public Transform footTransform;

    public ConfigurableJoint ankleJoint;
    public HingeJoint kneeJoint;
    
    
    [Header("Gradient decent parameters")]
    public float gain = 2.0f;
    public int maxIteration = 200;
    public float bound = 10.0f;
    public float step = 5.0f;
    public float stopError = 0.1f;
    public float stopIteration = 0.01f;

    [Header("IK results (debug)")]
    public float angle1 = 0.0f;
    public float angle2 = 0.0f;
    public float angle3 = 0.0f;

    [Header("Starting position")]
    public Vector3 startPosition;

    private Matrix4x4 ankle;
    private Matrix4x4 knee;
    private Matrix4x4 foot;

    private Vector3 offset1;
    private Vector3 offset2;


    void Start ()
    {
        ankle = Matrix4x4.Translate(transform.localPosition);
        knee = Matrix4x4.Translate(kneeTransform.localPosition);
        foot = Matrix4x4.Translate(footTransform.localPosition);

        offset1 = transform.localEulerAngles;
        offset2 = kneeTransform.localEulerAngles;

        startPosition = getPosition();
    }

    public void IKsetPosition(Vector3 target)
    {
        Vector3 gradient;
        float pa1, pa2, pa3;
        Vector3 position = _getPosition(angle1, angle2, angle3);
        Vector3 error = target - position;

        for (int i = 0; i < maxIteration; i++)
        {
            // angle 1 
            gradient = _getPosition(angle1 + step, angle2, angle3) - position;
            pa1 = gain * Vector3.Dot(gradient, error);
            if (pa1 > bound) pa1 = bound;
            else if (pa1 < -bound) pa1 = -bound;

            // angle 2
            gradient = _getPosition(angle1, angle2 + step, angle3) - position;
            pa2 = gain * Vector3.Dot(gradient, error);
            if (pa2 > bound) pa2 = bound;
            else if (pa2 < -bound) pa2 = -bound;

            // angle 3
            gradient = _getPosition(angle1, angle2, angle3 + step) - position;
            pa3 = gain * Vector3.Dot(gradient, error);
            if (pa3 > bound) pa3 = bound;
            else if (pa3 < -bound) pa3 = -bound;

            // iterate
            angle1 += pa1; angle2 += pa2; angle3 += pa3;
            if (Mathf.Abs(pa1) < stopIteration && Mathf.Abs(pa2) < stopIteration && Mathf.Abs(pa3) < stopIteration)
                break;

            position = _getPosition(angle1, angle2, angle3);
            error = target - position;

            if (error.magnitude < stopError)
                break;
        }

        transform.localEulerAngles = offset1 + new Vector3(0, angle1, angle2);
        kneeTransform.localEulerAngles = offset2 + new Vector3(0, 0, angle3);
    }

    Vector3 _getPosition(float a, float b, float c)
    {
        Matrix4x4 M = ankle * Matrix4x4.Rotate(Quaternion.Euler(offset1 + new Vector3(0, a, b))) * knee * Matrix4x4.Rotate(Quaternion.Euler(offset2 + new Vector3(0, 0, c))) * foot;
        return M.GetColumn(3);
    }
    public Vector3 getPosition()
    {
        return _getPosition(angle1, angle2, angle3);
    }
    public float getLimitDistance(Vector3 direction, float stepLength)
    {
        direction.y = 0;
        Vector3.Normalize(direction);
        Vector3 P = getPosition() - startPosition;
        float R = 0.5f * stepLength;

        float a = Mathf.Abs(P.x * direction.z - direction.x * P.z);

        if (a > R) return -1;
        else if (P.magnitude > R && Vector3.Dot(P, direction) > 0) return -1;
        else return R * Mathf.Cos(Mathf.Asin(a / R)) - Vector3.Dot(P, direction);
    }
}
