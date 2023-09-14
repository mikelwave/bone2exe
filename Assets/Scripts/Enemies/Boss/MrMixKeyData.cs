using UnityEngine;

public class MrMixKeyData : MonoBehaviour
{
    public MrMixKeys main;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void ButtonPress()
    {
        anim.SetTrigger("Press");
        main.KeyPress(transform.name[0],transform.position);
    }
}
