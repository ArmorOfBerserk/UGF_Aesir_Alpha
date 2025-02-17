using UnityEngine;
using UnityEditor;

[ExecuteInEditMode] // Permette di aggiornare la curva in tempo reale nell'editor
public class EditableBezierCurve : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 controlPoint;
    public Vector3 endPoint;
    public int curveResolution = 20;

    public void DEBUG_CURVE(Vector3[] v)
    {
        startPoint = v[0];
        controlPoint = v[1];
        endPoint = v[2];
    }

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = curveResolution + 1;
    }

    private void Update()
    {
        DrawBezierCurve();
    }

    private void DrawBezierCurve()
    {
        Vector3[] points = new Vector3[curveResolution + 1];

        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            points[i] = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 P0, Vector3 P1, Vector3 P2)
    {
        float u = 1f - t;
        return (u * u) * P0 + (2f * u * t) * P1 + (t * t) * P2;
    }

    // Metodo per aggiornare i punti via codice
    public void SetPoints(Vector3 start, Vector3 control, Vector3 spline, Vector3 end)
    {
        startPoint = start;
        controlPoint = control;
        endPoint = spline;
        DrawBezierCurve();
    }

    // Disegna dei cerchi per ogni punto di controllo nella Scene View
    private void OnDrawGizmos()
    {
        // Dimensione dei cerchi
        float sphereRadius = 0.1f;

        // Disegna startPoint in rosso
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPoint, sphereRadius);

        // Disegna controlPoint in verde
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(controlPoint, sphereRadius);

        // Disegna endPoint in blu
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(endPoint, sphereRadius);
    }
}
