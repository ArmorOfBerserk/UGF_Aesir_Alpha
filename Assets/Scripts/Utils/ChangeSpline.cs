using System;
using Dreamteck.Splines;
using UnityEngine;

public class ChangeSpline : MonoBehaviour
{
    public static Action<SplineComputer> onChangeSpline;
    private SplineComputer spline;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        spline = GetComponent<SplineComputer>();
    }

    public void test(){
        onChangeSpline?.Invoke(spline);
    }
}
