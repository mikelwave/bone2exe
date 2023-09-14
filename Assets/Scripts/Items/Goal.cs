using System.Collections;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public float HeightSinSpeed;
    public float HeightSinMultiplier;
    float HeightSinProgression;

    public float RotationSinSpeed;
    public float RotationSinMultiplier;
    float RotationSinProgression;
    Transform child;
    public Sprite[] goalSprites;
    SpriteRenderer goal;
    BoilState goalAnim = new BoilState(new int[]{0,1});
    public GameObject playerGoalSpawn;
    // Start is called before the first frame update
    void Start()
    {
        if(child==null)
        child = transform.GetChild(0);
        goal = child.GetComponent<SpriteRenderer>();
        InvokeRepeating("Animate",0,1f);
        //CancelInvoke("Animate");
    }

    // Update is called once per frame
    void Update()
    {
        HeightSinProgression = Mathf.Repeat(HeightSinProgression+Time.deltaTime*HeightSinSpeed,Mathf.PI*2);
        Vector3 pos = child.localPosition;
        pos.y = Mathf.Sin(HeightSinProgression)*HeightSinMultiplier;
        child.localPosition = pos;

        RotationSinProgression = Mathf.Repeat(RotationSinProgression+Time.deltaTime*RotationSinSpeed,Mathf.PI*2);
        child.localEulerAngles = new Vector3(0,0,Mathf.Sin(RotationSinProgression)*RotationSinMultiplier);
    }
    void Animate()
    {
        goal.sprite = goalSprites[goalAnim.GetIndexAndIncrease()];
    }
    IEnumerator IGoalAnim(Vector3 pos,GameObject o)
    {
        GameMaster.Goal = true;
        print("Goal: "+GameMaster.Goal);
        DataShare.PlaySound("Goal_reach",pos,false);
        PlayerControl.JumpingDustParticleSystem.transform.SetParent(null);
        o.SetActive(false);
        if(goal == null) Start();

        goal.sprite = null;
        Transform t = Instantiate(playerGoalSpawn,pos,Quaternion.identity).transform;
        // Change gun for dark world player type
        if(o.name.Contains("Dark"))
        {
            t.GetChild(0).GetComponent<Animator>().SetTrigger("Dark");
        }
        float progress = 0;
        Vector3 TargetPos = transform.position;
        TargetPos.y-=0.6f;
        Vector3 StartPos = t.position;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*5;
            float mathStep = Mathf.SmoothStep(0.0f, 1.0f, progress);
            t.position = Vector3.Lerp(StartPos,TargetPos,mathStep);
            yield return 0;
        }

        yield return new WaitForSeconds(0.75f);
        GameMaster.ShowResults();
        yield return new WaitForSeconds(0.5f);
        Camera cam = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<Camera>();
        Transition.TransitionEvent(true,cam);
        Time.timeScale = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Player")
        {
            CancelInvoke("Animate");
            print("Goal reached");
            StartCoroutine(IGoalAnim(other.transform.position,other.gameObject));
        }
    }
}
