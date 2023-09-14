using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class TailsDollLightning : MonoBehaviour
{
    [SerializeField] VolumeProfile secondLightningEffect;
    SpriteRenderer spriteRenderer;
    IEnumerator ILightningTime()
    {
        Volume v = GetComponent<Volume>();
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.enabled = true;
        v.profile = secondLightningEffect;
        yield return new WaitForSeconds(0.05f);
        Destroy(v);
        DataShare.PlaySound("Lightning",false,0.1f,1f);
        yield return new WaitForSeconds(0.50f);
        GetComponent<Animator>().SetTrigger("End");
    }
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        transform.SetParent(null);
        StartCoroutine(ILightningTime());
    }
}
