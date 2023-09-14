using UnityEngine;

public abstract class ChangedElement
{
    protected Material normalMaterial;
    protected SuperBulletVariant main;
    public abstract void Write(bool set);
    public abstract void Read(SuperBulletVariant main);
}

class BulletElement : ChangedElement
{
    Gradient normalGradient;
    SpriteRenderer spriteRenderer;
    TrailRenderer trailRenderer;
    public override void Write(bool set)
    {
        if(set)
        {
            spriteRenderer.material = main.superMaterial;
            trailRenderer.colorGradient = main.superGradient;
        }
        else 
        {
            spriteRenderer.material = normalMaterial;
            trailRenderer.colorGradient = normalGradient;
        }
    }
    public override void Read(SuperBulletVariant main)
    {
        this.main = main;
        spriteRenderer = main.transform.GetComponent<SpriteRenderer>();
        trailRenderer  = main.transform.GetComponent<TrailRenderer>();

        normalMaterial = spriteRenderer.material;
        normalGradient = trailRenderer.colorGradient;

    }
}
class ImpactElement : ChangedElement
{
    ParticleSystemRenderer[] impactParticleSystem = new ParticleSystemRenderer[2];

    public override void Read(SuperBulletVariant main)
    {
        this.main = main;
        impactParticleSystem[0] = main.transform.GetComponent<ParticleSystemRenderer>();
        impactParticleSystem[1] = main.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
        normalMaterial = impactParticleSystem[0].material;
    }

    public override void Write(bool set)
    {
        if(set)
        {
            impactParticleSystem[0].material = main.superMaterial;
            impactParticleSystem[1].material = main.superMaterial;
        }
        else 
        {
            impactParticleSystem[0].material = normalMaterial;
            impactParticleSystem[1].material = normalMaterial;
        }
    }
}
public class SuperBulletVariant : MonoBehaviour
{
    public Material superMaterial;
    public Gradient superGradient;

    ChangedElement changedElement;
    bool loadedValues = false;
    bool changed = false;

    delegate void ToggleCallback(bool set = false);
    ToggleCallback toggleCallback;

    void OnEnable()
    {
        if(!loadedValues)
        {
            // Load correct element type
            changedElement = GetComponent<SpriteRenderer>() == null ? new ImpactElement() : new BulletElement();

            changedElement.Read(this);
            toggleCallback = changedElement.Write;

            loadedValues = true;
        }
        if(GameMaster.superModeTime == 0) return;

        changed = true;
        toggleCallback?.Invoke(true);
    }

    void OnDisable()
    {
        if(!changed) return;

        changed = false;
        toggleCallback?.Invoke(false);
    }
}
