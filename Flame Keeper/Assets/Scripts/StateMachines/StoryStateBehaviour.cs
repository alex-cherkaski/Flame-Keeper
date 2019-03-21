using UnityEngine;

public class StoryStateBehaviour : StateMachineBehaviour
{
    [HideInInspector]
    public MenuLevelConfig mainMenuConfig;

    // This will be called when the animator first transitions to this state.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    // This will be called once the animator has transitioned out of the state.
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (mainMenuConfig != null)
        {
            mainMenuConfig.OnStoryFinished();
        }
    }

    // This will be called every frame whilst in the state.
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateIK(animator, stateInfo, layerIndex);
    }
}
