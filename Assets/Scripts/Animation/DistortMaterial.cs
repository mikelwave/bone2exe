using UnityEngine;

public class DistortMaterial : MonoBehaviour
{
    [SerializeField] Material material;
    const float distortionSpeed = 0.3f;
    void ChangeShader()
    {
        material.SetFloat("_DistortionSpeed",DataShare.movingBGs ? distortionSpeed : 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        DataShare.onMovingBGsSwitch+=ChangeShader;
        ChangeShader();
    }
    void OnDisable()
    {
        DataShare.onMovingBGsSwitch-=ChangeShader;
    }
    void OnDestroy()
    {
        DataShare.onMovingBGsSwitch-=ChangeShader;
    }
}
