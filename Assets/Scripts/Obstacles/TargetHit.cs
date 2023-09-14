using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;

public class TargetHit : MonoBehaviour
{
    #region main
    [SerializeField] bool oneTimeUse = true;
    [SerializeField] bool usesKeyBlocks = true;
    SpriteRenderer spriteRenderer;
    public bool Activated = false;
    public string hitSound = "Target_hit";
    IEnumerator IPressCooldown()
    {
        Activated = true;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        Activated = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        string s = other.tag.ToLower();
        if(!Activated && s.Contains("playerbullet"))
        {
            if(usesKeyBlocks) GameMaster.CheckKeyGenerate();
            other.transform.parent.GetComponent<BulletMovement>().DisableBullet(true,spriteRenderer,true,false,false);
            Activated = oneTimeUse;
            if(!oneTimeUse) StartCoroutine(IPressCooldown());
            else GetComponent<ObjectEnabler>().canDespawn = false;
            DataShare.PlaySound(hitSound,transform.position,false);
            OnHitEvent?.Invoke();
        }
    }
    #endregion
    [Serializable]
    public class MainEvent : UnityEvent {};

    [SerializeField]
    MainEvent hitEvent = new MainEvent();
    public MainEvent OnHitEvent { get { return hitEvent; } set { hitEvent = value; }}
}
