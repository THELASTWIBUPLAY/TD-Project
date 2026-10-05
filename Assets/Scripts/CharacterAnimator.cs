using System.Linq;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Character character;
    private string charClass;
    private int charStar;
    private Animator animator;
    private float animLength = -1;

    // Stores the resolved active animation state name after checking fallbacks
    public string currentAtkAnimName;

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
        }

        charStar = character.starLevel;

        currentAtkAnimName = ResolveAttackAnimationName();

        animator.CrossFadeInFixedTime(currentAtkAnimName, 0);
        CancelInvoke(nameof(UnconditionalPlayIdle));
        Invoke(nameof(UnconditionalPlayIdle), 0.1f);

        animLength = -1f;

        AnimationClip clip = GetClipByName(currentAtkAnimName);

        if (clip != null)
        {
            animLength = clip.length;
        }
    }

    private string ResolveAttackAnimationName()
    {
        string primaryName = $"atk{charClass}{charStar}";
        if (HasAnimationClip(primaryName))
        {
            return primaryName;
        }

        string classFallbackName = $"atk{charClass}1";
        if (HasAnimationClip(classFallbackName))
        {
            Debug.LogWarning($"Animation '{primaryName}' not found. Falling back to '{classFallbackName}'.");
            return classFallbackName;
        }

        Debug.LogWarning($"Animation '{classFallbackName}' not found. Falling back to 'atkRanger1'.");
        return "atkRanger1";
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
        animator.CrossFadeInFixedTime("Idle", 0);
    }

    public void PlayAnim()
    {
        animator.CrossFadeInFixedTime(currentAtkAnimName, 0);

        CancelInvoke(nameof(PlayIdle));
        Invoke(nameof(PlayIdle), animLength > 0 ? animLength : 0.5f);
    }

    private void PlayIdle()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(currentAtkAnimName))
        {
            return;
        }

        animator.CrossFadeInFixedTime("Idle", 0);
    }
}