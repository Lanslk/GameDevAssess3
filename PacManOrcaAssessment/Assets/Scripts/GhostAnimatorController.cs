using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimatorController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] leftSprites;           
    public Sprite[] rightSprites;           
    public Sprite[] upSprites;         
    public Sprite[] downSprites;
    public Sprite[] scaredSprites;
    public Sprite[] recoverSprites;
    public Sprite[] deadSprites;

    private Animator animator;
    private int ghostIndex;

    void Start()
    {
        animator = GetComponent<Animator>();
        ghostIndex = GetGhostIndex();
    }

    void Update()
    {
        UpdateGhostSpriteBasedOnState();
    }

    void UpdateGhostSpriteBasedOnState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("GhostShipRightAnim"))
        {
            spriteRenderer.sprite = rightSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
        else if (stateInfo.IsName("GhostShipLeftAnim"))
        {
            spriteRenderer.sprite = leftSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
        else if (stateInfo.IsName("GhostShipUpAnim"))
        {
            spriteRenderer.sprite = upSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
        else if (stateInfo.IsName("GhostShipDownAnim"))
        {
            spriteRenderer.sprite = downSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
        else if (stateInfo.IsName("GhostShipScaredAnim"))
        {
            spriteRenderer.sprite = scaredSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
        else if (stateInfo.IsName("GhostShipRecoverAnim"))
        {
            spriteRenderer.sprite = recoverSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
        else if (stateInfo.IsName("GhostShipDeadAnim"))
        {
            spriteRenderer.sprite = deadSprites[ghostIndex];
            UpdateSpriteBasedOnExactTime(rightSprites, stateInfo);
        }
    }
    
    void UpdateSpriteBasedOnExactTime(Sprite[] sprites, AnimatorStateInfo stateInfo)
    {
        float currentTime = stateInfo.normalizedTime * stateInfo.length;
        print(currentTime);
        if (currentTime >= 0f && currentTime < 0.3f)
        {
            spriteRenderer.sprite = sprites[0];
        }
        else if (currentTime >= 0.3f)
        {
            spriteRenderer.sprite = sprites[1];
        }
    }
    
    private int GetGhostIndex()
    {
        if (gameObject.name == "Ship-blue")
            return 0;
        else if (gameObject.name == "Ship-green")
            return 1;
        else if (gameObject.name == "Ship-red")
            return 2;
        else
            return 3;
    }
}
