using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EndingSequence : MonoBehaviour
{
    [SerializeField] Profile ReplacementProfileLeft;
    [SerializeField] Profile ReplacementProfileRight;
    [SerializeField] TextAsset endingText;
    [Space]
    [Header ("Rouge")]
    [Space]
    BoilAnimation rougeAnim;
    [SerializeField] Vector3 RougeSpawnPosition;
    [SerializeField] Vector3 RougeSpawnPositionDark;
    [SerializeField] Vector3 Rouge2ndSpawnPosition;
    [SerializeField] Sprite[] laughSprites;
    [SerializeField] Sprite[] boredSprites;
    [SerializeField] Sprite[] jumpSprites;
    [SerializeField] GameObject smokePoof;
    [SerializeField] Vector2 playerSpawnPoint = new Vector2(-10,-6);

    PlayerControl player;
    BulletShooter bulletShooter;
    SpriteRenderer blackFade;

    Coroutine seqCor;
    IEnumerator ISequence(byte eventByte)
    {
        ///print("Event: "+eventByte);
        switch(eventByte)
        {
            default: yield return 0; break;

            // Event 1 - Rouge appears
            case 1:
            yield return new WaitForSeconds(0.7f);
            rougeAnim.transform.position = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>().worldMode == GameMaster.WorldMode.LightMode ? RougeSpawnPosition : RougeSpawnPositionDark;
            Vector3 pos = rougeAnim.transform.position;
            Instantiate(smokePoof,pos+(Vector3.up*0.5f),Quaternion.identity);
            DataShare.PlaySound("Appear_puff",pos,false);
            yield return new WaitForSeconds(0.2f);
            rougeAnim.gameObject.SetActive(true);
            DataShare.LoadMusic("Rouge");
            break;

            // Event 1 - Fake ransomware screen appears
            case 2:
            yield return new WaitForSeconds(0.7f);
            DataShare.PlaySound("Ominous",false,0.2f,1f);
            ScreenEffects.FadeScreen(1f,true,Color.white);
            yield return new WaitForSeconds(1f);
            GameObject.Find("DecoGrid").SetActive(false);
            GameObject.FindWithTag("MainMap").SetActive(false);
            GameObject.FindWithTag("SSMap")?.SetActive(false);
            GameObject.Find("Lighting").SetActive(false);
            GameObject.Find("Light 2D").GetComponent<Light2D>().intensity = 1;
            Transform HUD = GameObject.FindWithTag("HUD").transform;
            HUD.GetComponent<HUD>().enabled = false;
            for (int i = 0; i < HUD.childCount-1; i++)
            {
                HUD.GetChild(i).gameObject.SetActive(false);
            }

            // Set player position
            Transform toMove = player.transform;
            player.enabled = false;
            Destroy(toMove.GetComponent<VelocityCap>());
            Destroy(toMove.GetComponent<Rigidbody2D>());
            toMove.position = playerSpawnPoint;
            toMove.localScale = Vector3.one;

            // Set Bloody Marry position
            toMove = GameObject.Find("BloodyMarry").transform;
            Destroy(toMove.GetComponent<VelocityCap>());
            Destroy(toMove.GetComponent<Rigidbody2D>());
            pos = toMove.position;
            pos.x = -playerSpawnPoint.x;
            pos.y = playerSpawnPoint.y-0.5f;
            toMove.position = pos;
            toMove.localScale = Vector3.one;
            toMove.rotation = Quaternion.identity;
            toMove.GetComponent<Animator>().SetInteger("Status",3);

            // Set rouge position
            SineMovementLate rougeSineMovement = rougeAnim.GetComponent<SineMovementLate>();
            rougeSineMovement.enabled = false;
            rougeAnim.transform.position = Rouge2ndSpawnPosition;
            rougeSineMovement.enabled = true;

            yield return new WaitForSeconds(1f);
            DataShare.PlaySound("Surprise_bang",false,0.2f,1f);
            transform.GetChild(1).gameObject.SetActive(true);
            ScreenEffects.FadeScreen(10000,false,Color.white);
            CamControl.ShakeCamera(0.5f,0.25f);
            break;

            // Rouge laugh
            case 3:
            rougeAnim.sprites = laughSprites;
            break;

            // Throw anti-viruses
            case 4:
            Transform rougeTransform = rougeAnim.transform;
            Transform[] antiviruses = new Transform[rougeTransform.childCount];
            for (int i = 0; i < antiviruses.Length; i++)
            {
                antiviruses[i] = rougeTransform.GetChild(i);
            }
            bulletShooter.transform.position = player.transform.position;
            bulletShooter.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            DataShare.LoadMusic("");
            InvokeRepeating("FireAntiVirus",0,bulletShooter.fireRate);
            bulletShooter.SetFireRate(0);
            // 7 Seconds
            float EndTime = 7;
            float progress = 0;
            int toActivateAtOnce = antiviruses.Length/3;
            int stage = 0;
            float stageTime = 0;
            float nextStageTime = EndTime/3.5f;
            while(progress<EndTime)
            {
                progress+=Time.deltaTime;
                stageTime+=Time.deltaTime;

                if(stageTime>=nextStageTime)
                {
                    stageTime = 0;
                    stage++;
                    if(stage == 2)
                    {
                        rougeAnim.ResetAnim(boredSprites);
                    }
                    int max = stage >= 3 ? antiviruses.Length : (toActivateAtOnce*stage);

                    for(int i = 0; i<max;i++)
                    {
                        antiviruses[i].gameObject.SetActive(true);
                    }

                }
                yield return 0;
            }
            CancelInvoke("FireAntiVirus");
            break;

            // Rouge jump off
            case 5:
            yield return new WaitForSeconds(0.5f);
            rougeAnim.ResetAnim(jumpSprites);
            yield return new WaitForSeconds(0.5f);
            rougeAnim.GetComponent<SineMovementLate>().enabled = false;
            rougeAnim.GetComponent<MoveToPointCurve>().Play();
            rougeAnim.GetComponentInChildren<ParticleSystem>().Play();
            DataShare.PlaySound("Whoosh2",rougeAnim.transform.position,false);
            Color c = Color.black;
            c.a = 0;
            blackFade.color = c;
            blackFade.gameObject.SetActive(true);
            progress = 0;
            while(progress < 1)
            {

                progress += Time.deltaTime;
                yield return 0;
                c.a = Mathf.Lerp(0,1,progress);
                blackFade.color = c;
            }
            c.a = 1;
            blackFade.color = c;
            yield return new WaitForSeconds(1f);
            PlayerControl.freezePlayerInput++;
            break;

            //Screen fade end
            case 6: 
            yield return new WaitForSeconds(0.7f);
            ScreenEffects.FadeScreen(1f,true,Color.white);
            yield return new WaitForSeconds(0.7f);
            CutscenePlayer.LoadCutscene("ending","Tierlist");
            break;
        }
    }
    void FireAntiVirus()
    {
        bulletShooter.Shoot();
    }
    void Sequence()
    {
        if(seqCor != null) StopCoroutine(seqCor);
        seqCor = StartCoroutine(ISequence(DialogueSystem.Event));
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
        rougeAnim = transform.GetChild(0).GetComponent<BoilAnimation>();
        bulletShooter = transform.GetChild(2).GetComponent<BulletShooter>();
        blackFade = transform.GetChild(3).GetComponent<SpriteRenderer>();
        player.Flip(1);
        DialogueSystem.SetProfileData(0,ReplacementProfileLeft);
        DialogueSystem.SetProfileData(1,ReplacementProfileRight);
        DialogueSystem.PrepareText(endingText);
        DialogueSystem.triggerEvent = Sequence;

        DialogueSystem.StartConvo(0);
    }
    void OnDestroy()
    {
        DialogueSystem.triggerEvent -= Sequence;
    }
}
