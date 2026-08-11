using UnityEngine;

namespace A1.Meta
{
    public static class AnimatorExtensions
    {
        public static float PlayAndGetLength(this Animator animator, string stateName)
        {
            if (animator.isActiveAndEnabled == false)
            {
                return 0f;
            }

            animator.Play(stateName);

            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == stateName)
                {
                    return clip.length;
                }
            }

            return 0f;
        }

        public static float PlayAndGetLength(this Animator animator, string stateName, int layer, float nomarlizedTime)
        {
            if (animator.isActiveAndEnabled == false)
            {
                return 0f;
            }

            animator.Play(stateName, layer, nomarlizedTime);

            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == stateName)
                {
                    return clip.length;
                }
            }

            return 0f;
        }

        public static bool IsAnimationFinished(this Animator animator, string stateName)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1f && !animator.IsInTransition(0);
        }

        public static float GetAnimationLength(this Animator animator, string stateName, string speedParamName = null)
        {
            var clip = GetAnimationClipInfo(animator, stateName);
            if (clip == null) return 0f;

            float speed = 1f;
            if (speedParamName != null)
            {
                float rawSpeed = animator.GetFloat(speedParamName);
                speed = Mathf.Abs(rawSpeed);
            }
            if (speed <= 0f) speed = 1f;

            float result = clip.length / speed;
            return result;
        }


        public static AnimationClip GetAnimationClipInfo(this Animator animator, string animationName)
        {
            AnimationClip result = null;

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == animationName)
                {
                    result = clip;
                    break;
                }
            }
            return result;
        }
    }
}
