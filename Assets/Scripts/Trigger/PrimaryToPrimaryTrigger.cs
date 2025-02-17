using System;
using Dreamteck.Splines;
using UnityEngine;

#region Splines Data Struct
[Serializable]
public struct PrimaryToPrimarySpline
{
    public SplineComputer Spline;
    [Header("Spline Points")]
    [SerializeField] public int _upperPointIndex;
    [SerializeField] public int _lowerPointIndex;

    public Vector3 EulerRotationUpperPoint
    {
        get
        {
            return CalculateAngle(Spline.Project(UpperPointVector).forward);
        }
    }
    public Vector3 EulerRotationLowerPoint
    {
        get
        {
            return CalculateAngle(Spline.Project(LowerPointVector).forward);
        }
    }

    public Vector3 UpperPointVector
    {
        get
        {
            var _realSplineIndex = _upperPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    public Vector3 LowerPointVector
    {
        get
        {
            var _realSplineIndex = _lowerPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    private Vector3 CalculateAngle(Vector3 forward)
    {
        var remappedForward = new Vector3(1.0f, 0.0f, 0.0f);
        var remappedUp = new Vector3(0.0f, 1.0f, 0.0f);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));
        var rotation = Quaternion.LookRotation(forward, remappedUp) * axisRemapRotation;
        return new Quaternion(0, rotation.y, 0, rotation.w).eulerAngles;
    }
}
#endregion

public class PrimaryToPrimaryTrigger : MonoBehaviour
{
    [SerializeField] PrimaryToPrimarySpline _spline1;
    [SerializeField] PrimaryToPrimarySpline _spline2;
    private bool _isEntered = false;
    public static Action<PrimaryToPrimarySpline, PrimaryToPrimarySpline, Choice> OnButtonPressedIncrocio;

    void OnTriggerEnter(Collider other)
    {
        EventMessageManager.SendTextMessage("Premi W per andare SU\nPremi S per andare giù");
        _isEntered = true;
    }

    void OnTriggerExit(Collider other)
    {
        EventMessageManager.DeleteMessage();
        _isEntered = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && _isEntered)
        {
            OnButtonPressedIncrocio?.Invoke(_spline1, _spline2, Choice.Upper);
        }

        if (Input.GetKeyDown(KeyCode.S) && _isEntered)
        {
            OnButtonPressedIncrocio?.Invoke(_spline1, _spline2, Choice.Lower);
        }
    }
}
