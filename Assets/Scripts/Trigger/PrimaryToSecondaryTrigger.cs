using System;
using Dreamteck.Splines;
using UnityEngine;

public enum ChoosenKey
{
    W,
    A,
    D,
    S
}

#region Splines Data Struct

//Informazioni da usare quando dalla secondaria deve confluire alla prima 
[Serializable]
public struct PrimarySplineData
{
    public SplineComputer Spline;
    [Header("Spline Points")]
    [SerializeField] private int _leftPointIndex;
    [SerializeField] private int _commonPointIndex;
    [SerializeField] private int _rightPointIndex;

    public Vector3 LeftPointVector
    {
        get
        {
            var _realSplineIndex = _leftPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    public Vector3 RightPointVector
    {
        get
        {
            var _realSplineIndex = _rightPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    public Vector3 CommonPointVector
    {
        get
        {
            var _realSplineIndex = _commonPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    public Vector3 LeftPointEulerRotation
    {
        get
        {
            return CalculateAngle(Spline.Project(LeftPointVector).forward);
        }
    }
    public Vector3 RightPointEulerRotation
    {
        get
        {
            return CalculateAngle(Spline.Project(LeftPointVector).forward);
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

//Informazioni da usare quando dalla prima deve confluire alla seconda 
[Serializable]
public struct SecondarySplineData
{
    public SplineComputer Spline;
    [Header("Control Point")]
    [SerializeField] private int _controlPointIndex;

    [Header("Final Point")]
    [SerializeField] private int _finalPointIndex;

    public Vector3 ControlPointVector
    {
        get
        {
            var _realSplineIndex = _controlPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    public Vector3 FinalPointVector
    {
        get
        {
            var _realSplineIndex = _finalPointIndex - 1;
            return new Vector3(Spline.GetPoint(_realSplineIndex).position.x, Spline.GetPoint(_realSplineIndex).position.y, Spline.GetPoint(_realSplineIndex).position.z);
        }
    }

    public Vector3 FinalPointEulerRotation
    {
        get
        {
            return CalculateAngle(Spline.Project(FinalPointVector).forward);
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

public class PrimaryToSecondaryTrigger : MonoBehaviour
{
    KeyCode[] KeyCodes = { KeyCode.W, KeyCode.A, KeyCode.D, KeyCode.S };
    [SerializeField] private CustomEvent _triggerUp;
    [SerializeField] private CustomEvent _triggerDown;
    [SerializeField] public PrimarySplineData PrimarySpline;
    [SerializeField] public SecondarySplineData SecondarySpline;
    public static Action<SecondarySplineData, Transform> OnSwitchToSecondarySpline;
    public static Action<PrimarySplineData> OnSwitchToPrimarySpline;
    private bool _isInsideTrigger = false;
    [SerializeField] private ChoosenKey _choosenKey = ChoosenKey.W;
    private Transform _triggerTransform;

    private void HandleSwitchToPrimarySpline(Transform transform)
    {
        OnSwitchToPrimarySpline?.Invoke(PrimarySpline);
    }

    private void HandleTriggerAreaExit()
    {
        _isInsideTrigger = false;
        
        EventMessageManager.DeleteMessage();
    }

    private void HandleTriggerAreaEnter(Transform triggerTransform)
    {
        Debug.Log(triggerTransform.tag);
        EventMessageManager.SendTextMessage(_choosenKey == ChoosenKey.W ? IndicationOnScreenEnum.UP_DIRECTION : IndicationOnScreenEnum.DOWN_DIRECTION);
        _triggerTransform = triggerTransform;
        _isInsideTrigger = true;
    }

    void Update()
    {
        if (_isInsideTrigger && Input.GetKeyDown(KeyCodes[(int)_choosenKey]))
        {
            OnSwitchToSecondarySpline?.Invoke(SecondarySpline, _triggerTransform);
        }
    }

    void OnEnable()
    {
        _triggerUp.EnterTrigger += HandleTriggerAreaEnter;
        _triggerUp.ExitTrigger += HandleTriggerAreaExit;
        _triggerDown.EnterTrigger += HandleSwitchToPrimarySpline;
    }

    void OnDisable()
    {
        _triggerUp.EnterTrigger -= HandleTriggerAreaEnter;
        _triggerUp.ExitTrigger -= HandleTriggerAreaExit;
        _triggerDown.EnterTrigger -= HandleSwitchToPrimarySpline;
    }

}
