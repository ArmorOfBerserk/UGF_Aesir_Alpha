using System.Collections;
using UnityEngine;

public class DissolveWall : Triggerable
{
    MeshRenderer meshRenderer;
    bool isVisible = true;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Trigger()
    {
        if (isVisible)
        {
            isVisible = false;
            meshRenderer.enabled = false;
        } 
        else
        {
            isVisible = true;
            meshRenderer.enabled = true;
        }
    }    
}

