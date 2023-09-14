using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class SecretRingScript : MonoBehaviour
{
    bool Active = false;
    ParticleSystem particles;
    [SerializeField] Vector2Int destination;
    [SerializeField] bool leadsToScene = false;
    [Tooltip("Flip direction to be at when exiting, leave at 0 to not change direction at all.")]
    [Range(-1,1)]
    [SerializeField] short preferredFlipDirection = 0;
    IEnumerator ITeleport(Transform player)
    {
        if(leadsToScene)
        {
            DataShare.LoadMusic("");
        }
        DataShare.PlaySound("SecretRing",transform.position,false);
        player.gameObject.SetActive(false);
        GetComponent<ObjectEnabler>().Spawn(false);
        GetComponent<SpriteRenderer>().enabled = false;
        PlayerControl.freezePlayerInput++;
        particles.Play();
        yield return 0;
        ScreenEffects.FadeScreen(2f,false,Color.white);

        yield return new WaitForSeconds(1f);

        //Flash fade in
        ScreenEffects.FadeScreen(2f,true,Color.white);

        //Flash fade out
        yield return new WaitForSeconds(1f);
        if(leadsToScene)
        {
            if(destination.x>=DataShare.worldAmount*2)
            {
                destination.x -= destination.x % 2; // Make even
                if(GameMaster.ShouldGoToDarkWorld())
                destination.x+=1;
            }
            else GameMaster.WorldModeCheck(false);
            GameMaster.Reset();
            DataShare.LoadLevel(destination.x,destination.y,true);
            yield break;
        }
        player.position = (Vector2)destination;

        if(preferredFlipDirection != 0)
        player.GetComponent<PlayerControl>().Flip((int)preferredFlipDirection);

        player.gameObject.SetActive(true);
        CamControl.self.transform.parent.GetComponent<CameraSetup>().SnapCam();
        CamControl.self.SpecialSetup(CamControl.self.transform.position);
        
        ScreenEffects.FadeScreen(2f,false,Color.white);
        Destroy(gameObject);
        PlayerControl.DecPlayerFreeze();
    }
    void Start()
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying) return;
        #endif
        particles = GetComponent<ParticleSystem>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!Active && other.tag == "Player")
        {
            StartCoroutine(ITeleport(other.transform));
            Active = true;
        }
    }

    #if UNITY_EDITOR
    void Update()
    {
        if(!Application.isPlaying && !leadsToScene)
        {
            Vector2 pos = (Vector2)transform.position;
            Debug.DrawLine(transform.position,(Vector2)destination+(Vector2.one*0.5f),Color.red);
        }
    }
    #endif
}
