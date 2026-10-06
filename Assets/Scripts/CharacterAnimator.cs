using System.Linq;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Character character;
    private string charClass;
    private int charStar;
    private Animator animator;
    private float animLength = -1;

    public string atkAnimName;
    public string idleAnimName;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        InitializeAnimator();
    }

    public void InitializeAnimator()
    {
        if (character == null) character = GetComponentInParent<Character>();

        switch (character.classType)
        {
            case CharacterClassType.Ranger:
                charClass = "Ranger";
                break;
            case CharacterClassType.Melee:
                charClass = "Melee";
                break;
            case CharacterClassType.Mage:
                charClass = "Mage";
                break;
            case CharacterClassType.Tank:
                charClass = "Tank";
                break;
            case CharacterClassType.Support:
                charClass = "Support";
                break;
            default:
                charClass = "Ranger";
                break;
        }

        charStar = character.starLevel;

        atkAnimName = GetAtkAnimName();
        idleAnimName = GetIdleAnimName();

        animator.CrossFadeInFixedTime(atkAnimName, 0);
        CancelInvoke(nameof(UnconditionalPlayIdle));
        Invoke(nameof(UnconditionalPlayIdle), 0.1f);

        animLength = -1f;

        AnimationClip clip = GetClipByName(atkAnimName);

        if (clip != null)
        {
            animLength = clip.length;
        }
    }

    // Finds earliest star level that has an animation for a given class
    private string GetAtkAnimName()
    {
        string atkName = "";

        for (int i = charStar; i >= 1; i--)
        {
            atkName = $"atk{charClass}{i}";
            if (HasAnimationClip(atkName))
            {
                return atkName;
            }

            Debug.LogWarning($"Animation '{atkName}' not found");
        }

        return "atk";
    }

    private string GetIdleAnimName()
    {
        string idleName = $"idle{charClass}{charStar}";
        if (HasAnimationClip(idleName))
        {
            return idleName;
        }

        return "idle";
    }

    private bool HasAnimationClip(string clipName)
    {
        return GetClipByName(clipName) != null;
    }

    private AnimationClip GetClipByName(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return null;

        return animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name == clipName);
    }

    private void UnconditionalPlayIdle()
    {
        animator.CrossFadeInFixedTime(idleAnimName, 0);
    }

    public void PlayAnim()
    {
        animator.CrossFadeInFixedTime(atkAnimName, 0);

        CancelInvoke(nameof(PlayIdle));
        Invoke(nameof(PlayIdle), animLength > 0 ? animLength : 0.5f);
    }

    private void PlayIdle()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(atkAnimName))
        {
            return;
        }

        animator.CrossFadeInFixedTime(idleAnimName, 0);
    }
}