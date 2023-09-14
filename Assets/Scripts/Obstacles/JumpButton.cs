using System.Collections;
using UnityEngine;

public class JumpButton : MonoBehaviour
{
    bool activated = false;
    PlayerControl playerControl;
    SpriteTrailMain spriteTrailMain;
    Animation anim;
    Coroutine regenerate;
    IEnumerator IRegenerate()
    {
        yield return new WaitForSeconds(2f);
        anim.Play("JumpButton_Regen");
        DataShare.PlaySound("KeyJump_respawn",transform.position,false);
        spriteTrailMain.Activate();
        yield return new WaitForSeconds(0.25f);
        activated = false;
    }
    void Start()
    {
        anim = GetComponent<Animation>();
        spriteTrailMain = transform.GetChild(1).GetComponent<SpriteTrailMain>();
        spriteTrailMain.SetSource(transform.GetChild(0).GetComponent<SpriteRenderer>());
        spriteTrailMain.Activate();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!activated && playerControl == null && other.name.Contains("Player"))
        {
            // Add to jumps if player not already monitored
            playerControl = other.GetComponent<PlayerControl>();
            playerControl.JumpsRemaining++;
            // Assign jump event trigger
            playerControl.jumpEvent += Activate;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        // Subtract from jumps if jump key was not pressed
        if(!activated && other.name.Contains("Player") && playerControl!=null)
        {
            if(playerControl.JumpsRemaining>0)
            playerControl.JumpsRemaining--;

            ClearPlayer();
        }
    }
    void OnEnable()
    {
        if(anim!=null)
        {
            anim.Play();
            spriteTrailMain.Activate();
        }
    }
    void OnDisable()
    {
        if(anim==null) return;
        anim.Stop();
        activated = false;
        ClearPlayer();
        if(regenerate!=null)StopCoroutine(regenerate);
    }
    void Activate()
    {
        anim.Play("JumpButton_Press");
        DataShare.PlaySound("KeyJump_press",transform.position,false);
        activated = true;
        spriteTrailMain.DeActivate();
        ClearPlayer();
        if(regenerate!=null)StopCoroutine(regenerate);
        regenerate = StartCoroutine(IRegenerate());
    }
    // Unassign player
    void ClearPlayer()
    {
        if(playerControl == null) return;
        playerControl.jumpEvent -= Activate;
        playerControl = null;
    }
}
