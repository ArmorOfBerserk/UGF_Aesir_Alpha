using Dreamteck.Splines;
using UnityEngine;
using System;
using System.Collections;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;
using Unity.VisualScripting;

public enum Direction
{
    Left,
    Right
}

public enum Choice
{
    Upper,
    Lower
}

public class RouteSwitchAutomation : MonoBehaviour
{
    public SplineProjector tracer;
    private Coroutine _movementCoroutine;
    public Vector3 P0, P1, P2;
    public EditableBezierCurve debug_secondarie;
    public Direction actualDirection = Direction.Right;
    private Rigidbody _rb;
    public float speed = 1f;

    private void Awake()
    {
        tracer = GetComponent<SplineProjector>();
        _rb = GetComponent<Rigidbody>();
    }


    private void TransitionPrimaryToPrimaryHandler(PrimaryToPrimarySpline spline1, PrimaryToPrimarySpline spline2, Choice choice)
    {
        if (_movementCoroutine == null)
        {
            // Funziona solo se ci sono due spline diverse
            /* var destionationSpline = tracer.spline == spline1.Spline ? spline2 : spline1; */

            //Se ci si deve muovere su due punti della stessa Spline e per adesso, 
            // solo nei casi in cui i punti della stessa spline siano abbastanza distanti 
            // tra di loro e non siano uguali di distanza

            int actualSplinePointIndex = tracer.spline.PercentToPointIndex(tracer.GetPercent());
            var distanceSpline1 = Math.Abs(actualSplinePointIndex - spline1._upperPointIndex);
            var distanceSpline2 = Math.Abs(actualSplinePointIndex - spline2._upperPointIndex);

            //Cerca di prenderti sempre quello più lontano
            var destionationSpline = distanceSpline1 > distanceSpline2 ? spline1 : spline2;
            Vector3 choicedPoint = choice == Choice.Upper ? destionationSpline.UpperPointVector : destionationSpline.LowerPointVector;
            Vector3 rotation = choice == Choice.Upper ? destionationSpline.EulerRotationUpperPoint : destionationSpline.EulerRotationLowerPoint;
            _movementCoroutine = StartCoroutine(IncrociTransitionSpline(destionationSpline, choicedPoint, rotation));
        }
    }

    private void TransitionToSecondaryHandler(SecondarySplineData splineData, Transform colliderTransform)
    {
        Vector3 localHitPoint = colliderTransform.InverseTransformPoint(transform.position);
        actualDirection = localHitPoint.x < 0 ? Direction.Left : Direction.Right;
        /* actualDirection = transform.position.x < bounds.center.x ? Direction.Left : Direction.Right; */

        if (_movementCoroutine == null)
        {
            _movementCoroutine = StartCoroutine(TransitionSpline(splineData.Spline, splineData.ControlPointVector, splineData.FinalPointVector, splineData.FinalPointEulerRotation));
        }

    }

    private void TransitionToPrimaryHandler(PrimarySplineData splineData)
    {
        if (_movementCoroutine == null)
        {
            Vector3 endPoint = actualDirection == Direction.Left ? splineData.LeftPointVector : splineData.RightPointVector;
            Vector3 rotationEuler = actualDirection == Direction.Left ? splineData.LeftPointEulerRotation : splineData.RightPointEulerRotation;
            _movementCoroutine = StartCoroutine(TransitionSpline(splineData.Spline, splineData.CommonPointVector, endPoint, rotationEuler));
        }

    }

    private Vector3[] CorrectionPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
    {
        /* return new Vector3[] { new Vector3(pointA.x, 0, pointA.z), new Vector3(pointC.x, 0, pointA.z), new Vector3(pointC.x, 0, pointC.z) }; */
        return new Vector3[] { new Vector3(pointA.x, pointA.y, pointA.z), new Vector3(pointB.x, pointA.y, pointB.z), new Vector3(pointC.x, pointA.y, pointC.z), new Vector3(pointD.x, pointA.y, pointD.z) };
    }

    IEnumerator TransitionSpline(SplineComputer nextSpline, Vector3 controlPoint, Vector3 finalPoint, Vector3 rotation)
    {
        float t = 0f;

        Vector3 startPoint = transform.position;

        Vector3[] correctedPoints = CorrectionPoint(startPoint, P1, controlPoint, finalPoint);
        (startPoint, P1, controlPoint, finalPoint) = (correctedPoints[0], correctedPoints[1], correctedPoints[2], correctedPoints[3]);

        // Visualizza la curva per il debug
        debug_secondarie.DEBUG_CURVE(new Vector3[] { startPoint, controlPoint, finalPoint });

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, rotation.y, 0f);

        /*        if (Quaternion.Dot(initialRotation, targetRotation) < 0f)
               {
                   targetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
               }
        */

        tracer.spline = null;
        tracer.enabled = false;

        while (t < 1f)
        {
            t += Time.fixedDeltaTime;
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, finalPoint);
            _rb.MovePosition(new Vector3(position.x, _rb.position.y, position.z));
            _rb.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return new WaitForFixedUpdate();
        }

        tracer.enabled = true;
        tracer.spline = nextSpline;
        _movementCoroutine = null;
    }

    // Calcola il punto della curva quadratica per il parametro t (0 a 1)
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 P0, Vector3 P1, Vector3 P2)
    {
        float u = 1f - t;
        return (u * u) * P0 + (2f * u * t) * P1 + (t * t) * P2;
    }

    private IEnumerator IncrociTransitionSpline(PrimaryToPrimarySpline spline, Vector3 finalPoint, Vector3 rotation)
    {
        float t = 0f;

        Vector3 startPoint = transform.position;
        Vector3 direction = (finalPoint - startPoint).normalized;
        Vector3 midPoint = (startPoint + finalPoint) * 0.5f;

        float sign = Mathf.Sign(Vector3.Cross(transform.forward, tracer.result.forward).y);

        Vector3 rotatedDir = Quaternion.Euler(0, 90f * sign, 0) * direction;

        float offset = Vector3.Distance(startPoint, finalPoint) * 0.2f;

        Vector3 controlPoint = midPoint + rotatedDir * offset;

        Vector3[] correctedPoints = CorrectionPoint(startPoint, P1, controlPoint, finalPoint);
        (startPoint, P1, controlPoint, finalPoint) = (correctedPoints[0], correctedPoints[1], correctedPoints[2], correctedPoints[3]);

        debug_secondarie.DEBUG_CURVE(new Vector3[] { startPoint, controlPoint, finalPoint });

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (Quaternion.Dot(initialRotation, targetRotation) < 0f)
        {
            targetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
        }


        tracer.spline = null;
        tracer.enabled = false;

        while (t < 1f)
        {
            t += Time.fixedDeltaTime;
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, finalPoint);
            _rb.MovePosition(new Vector3(position.x, _rb.position.y, position.z));
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return new WaitForFixedUpdate();
        }

        tracer.enabled = true;
        tracer.spline = spline.Spline;
        _movementCoroutine = null;
    }

    private Vector3[] correction(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        return new Vector3[] { new Vector3(v0.x, v0.y, v0.z), new Vector3(v1.x, v0.y, v1.z), new Vector3(v2.x, v0.y, v2.z) };
    }

    private void RotatePlayer(Vector3 forward)
    {
        var remappedForward = new Vector3(1.0f, 0.0f, 0.0f);
        var remappedUp = new Vector3(0.0f, 1.0f, 0.0f);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));
        _rb.rotation = Quaternion.LookRotation(forward, remappedUp) * axisRemapRotation;
        //Nel caso in cui dia problemi, attivare questa
        /* transform.rotation = new Quaternion(transform.rotation.x, rotation.y, transform.rotation.z, rotation.w); */
    }

    void FixedUpdate()
    {
        if (_movementCoroutine == null)
            RotatePlayer(tracer.result.forward);
    }

    #region Change Route Event

    private void OnEnable()
    {
        PrimaryToSecondaryTrigger.OnSwitchToSecondarySpline += TransitionToSecondaryHandler;
        PrimaryToSecondaryTrigger.OnSwitchToPrimarySpline += TransitionToPrimaryHandler;
        PrimaryToPrimaryTrigger.OnButtonPressedIncrocio += TransitionPrimaryToPrimaryHandler;
    }


    private void OnDisable()
    {
        PrimaryToSecondaryTrigger.OnSwitchToSecondarySpline -= TransitionToSecondaryHandler;
        PrimaryToSecondaryTrigger.OnSwitchToPrimarySpline -= TransitionToPrimaryHandler;
        PrimaryToPrimaryTrigger.OnButtonPressedIncrocio -= TransitionPrimaryToPrimaryHandler;
    }

    #endregion

}