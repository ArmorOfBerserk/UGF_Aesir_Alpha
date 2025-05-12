using UnityEngine;

public class CameraMoverTrigger : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.gameObject.GetComponent<CameraMoverController>();

        if (controller != null)
            controller.BeginMove();
        else return;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) // facoltativo 
    {
        // ulteriori trigger da far partire a fine script 
    }
}
