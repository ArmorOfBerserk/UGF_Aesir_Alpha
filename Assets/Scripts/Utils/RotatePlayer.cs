using Dreamteck.Splines;
using UnityEngine;

public class RotatePlayer : MonoBehaviour
{

    SplineProjector projector;
    public bool forwardDirection = true;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        projector = GetComponentInChildren<SplineProjector>();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        var remappedForward = forwardDirection ? new Vector3(-1.0f, 0.0f, 0.0f) : new Vector3(1.0f, 0.0f, 0.0f);
        var remappedUp = new Vector3(0.0f, 1.0f, 0.0f);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        var forward = projector.result.forward;
        // Calcola la rotazione in base alla tangente
        /* Quaternion targetRotation = Quaternion.LookRotation(projector.result.forward); */
        var rotation = Quaternion.LookRotation(forward, remappedUp) * axisRemapRotation;
        // Applica la rotazione al personaggio
        transform.rotation = new Quaternion(transform.rotation.x, rotation.y, transform.rotation.z, rotation.w);

    }

}
