using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SetSky : MonoBehaviour
{
    #if UNITY_EDITOR
    public Sprite Sky;
    SpriteRenderer skyRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if(Application.isPlaying) Destroy(this);
        skyRenderer = GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        UpdateSky();
    }
    void OnValidate() { UnityEditor.EditorApplication.delayCall = _OnValidate; }
    void _OnValidate()
    {
        if (this == null) return;
        UpdateSky();
    }
    void UpdateSky()
    {
        if(skyRenderer==null)Start();
        
        #if UNITY_EDITOR
        Sprite oldSprite = skyRenderer.sprite;
        #endif

        skyRenderer.sprite = Sky;

        #if UNITY_EDITOR
        if(!Application.isPlaying && oldSprite != Sky)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        #endif
    }
    #endif
}
