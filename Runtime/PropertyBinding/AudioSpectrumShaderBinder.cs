using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AaronStatic.Audio {
    [ExecuteInEditMode]
    public class AudioSpectrumShaderBinder : MonoBehaviour
    {
        public AudioSpectrum AudioSpectrum;
        public string ShaderPropertyName;
        public Property Property;

        private MeshRenderer m_Renderer;

        private bool m_doneEvent = false;

        private void Update()
        {
            if(!m_doneEvent && AudioSpectrum != null)
            {
                AudioSpectrum.OnUpdate.AddListener(OnUpdate);
                m_doneEvent = true;
            }
        }

        void OnUpdate(AudioData data)
        {
            if(m_Renderer == null)
            {
                m_Renderer = GetComponent<MeshRenderer>();
            }
            if(Property == Property.Bass)
            {
                m_Renderer.material.SetFloat(ShaderPropertyName,data.Bass);
            }
            if (Property == Property.Mid)
            {
                m_Renderer.material.SetFloat(ShaderPropertyName, data.Mid);
            }
            if (Property == Property.Treble)
            {
                m_Renderer.material.SetFloat(ShaderPropertyName, data.Treble);
            }
            if (Property == Property.Sum)
            {
                m_Renderer.material.SetFloat(ShaderPropertyName, data.Sum);
            }
            if (Property == Property.Avg)
            {
                m_Renderer.material.SetFloat(ShaderPropertyName, data.Avg);
            }
            if (Property == Property.Spectrum)
            {
                m_Renderer.material.SetTexture(ShaderPropertyName, data.Spectrum);
            }
            if (Property == Property.SpectrumHistory)
            {
                m_Renderer.material.SetTexture(ShaderPropertyName, data.SpectrumHistory);
            }
        }        
    }
}