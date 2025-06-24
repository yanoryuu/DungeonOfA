using UnityEngine;

public static class AnimatorCommon
{
    public static void SetAnimation(string animationName,Animator _animator)
    {
        _animator.SetBool(animationName, true);
    }

    public static void OutAnimation(string animationName,Animator _animator)
    {
        _animator.SetBool(animationName, false);
    }
    
    public static void SetAction(string actionName,Animator _animator)
    {
        _animator.SetTrigger(actionName);
    }

    public static void ResetAction(string actionName,Animator _animator)
    {
        _animator.ResetTrigger(actionName);
    }
}
