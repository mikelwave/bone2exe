// Modified script from Unity documentation
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleFlow : MonoBehaviour
{
    ParticleSystem m_System;
    ParticleSystem.Particle[] m_Particles;
    List<Vector4> customData = new List<Vector4>();
    [SerializeField] string soundName = "";
    [SerializeField] Vector2 pitchRange = Vector2.one;

    private void LateUpdate()
    {
        InitializeIfNeeded();
        if(Time.timeScale==0) return;

        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        int numParticlesAlive = m_System.GetParticles(m_Particles);
        m_System.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        Vector2 pos = transform.position;

        // Go through only living particles
        for (int i = 0; i < numParticlesAlive; i++)
        {
            // Assign a value of 1 so the particle is marked to not play a sound again
            if(customData[i].x == 0.0f)
            {
                customData[i] = new Vector4(1, 0, 0, 0);
                if(soundName!= "")
                {
                    DataShare.PlaySound(soundName,m_Particles[i].position,false,1,Random.Range(pitchRange.x,pitchRange.y));
                }
                ///print("ID "+i+" position:  "+m_Particles[i].position);
                Debug.DrawLine(pos,m_Particles[i].position,Color.blue);
            }
        }

        // Apply the particle changes to the Particle System

        m_System.SetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
    }

    void InitializeIfNeeded()
    {
        if (m_System == null)
            m_System = GetComponent<ParticleSystem>();

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
    }
}