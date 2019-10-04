using System;
using UnityEngine;
using UnityEngine.VFX.Utility;
using UnityEngine.VFX;

namespace AaronStatic.VFX.Utility
{
    [AddComponentMenu("VFX/Property Binders/Audio Spectrum History Binder")]
    [VFXBinder("Audio/Audio Spectrum + History to AttributeMap")]
    class VFXAudioSpectrumHistoryBinder : VFXBinderBase
    {
        public enum AudioSourceMode
        {
            AudioSource,
            AudioListener
        }

        public string CountProperty { get { return (string)m_CountProperty; } set { m_CountProperty = value; } }
        [VFXPropertyBinding("System.UInt32"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_CountParameter")]
        protected ExposedProperty m_CountProperty = "Count";

        public string HistoryCountProperty { get { return (string)m_HistoryCountProperty; } set { m_HistoryCountProperty = value; } }
        [VFXPropertyBinding("System.UInt32"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_CountParameter")]
        protected ExposedProperty m_HistoryCountProperty = "Size";

        public string TextureProperty { get { return (string)m_TextureProperty; } set { m_TextureProperty = value; } }
        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_TextureParameter")]
        protected ExposedProperty m_TextureProperty = "SpectrumTexture";

        public string HistoryProperty { get { return (string)m_HistoryProperty; } set { m_HistoryProperty = value; } }
        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_TextureParameter")]
        protected ExposedProperty m_HistoryProperty = "HistoryTexture";

        public FFTWindow FFTWindow = FFTWindow.BlackmanHarris;
        public uint Samples = 64;
        public uint History = 64;
        public AudioSourceMode Mode = AudioSourceMode.AudioSource;
        public AudioSource AudioSource = null;

        private Texture2D m_Texture;
        private Texture2D m_History;
        private float[] m_AudioCache;
        private Color[] m_ColorCache;
        private Color[] m_HistoryCache;

        public override bool IsValid(VisualEffect component)
        {
            bool mode = (Mode == AudioSourceMode.AudioSource ? AudioSource != null : true);
            bool texture = component.HasTexture(TextureProperty);
            bool historytex = component.HasTexture(HistoryProperty);
            bool count = component.HasUInt(CountProperty);
            bool historyc = component.HasUInt(HistoryCountProperty);

            return mode && texture && count && historytex && historyc;
        }

        void UpdateTexture()
        {
            if (m_Texture == null || m_Texture.width != Samples || m_Texture.height != 1)
            {
                m_Texture = new Texture2D((int)Samples, 1, TextureFormat.RFloat, false);
                m_AudioCache = new float[Samples];
                m_ColorCache = new Color[Samples];
            }

            if (m_History == null || m_History.width != Samples || m_History.height != History)
            {
                m_History = new Texture2D((int)Samples, (int)History, TextureFormat.RFloat, false);
                m_HistoryCache = new Color[Samples * History];
            }

            if (Mode == AudioSourceMode.AudioListener)
                AudioListener.GetSpectrumData(m_AudioCache, 0, FFTWindow);
            else if (Mode == AudioSourceMode.AudioSource)
                AudioSource.GetSpectrumData(m_AudioCache, 0, FFTWindow);
            else throw new NotImplementedException();

            for (int i = 0; i < Samples; i++)
            {
                m_ColorCache[i] = new Color(m_AudioCache[i], 0, 0, 0);
            }

            //shift history by n Samples left
            var destination = new Color[m_HistoryCache.Length];
            Array.Copy(m_HistoryCache, Samples, destination, 0, m_HistoryCache.Length - Samples);
            m_HistoryCache = destination;
            //append to history
            Array.Copy(m_ColorCache, 0, m_HistoryCache, m_HistoryCache.Length - Samples, Samples);
            
            m_Texture.SetPixels(m_ColorCache);
            m_Texture.name = "AudioSpectrum" + Samples;
            m_Texture.Apply();

            m_History.SetPixels(m_HistoryCache);
            m_History.name = "AudioSpectrumHistory" + Samples;
            m_History.Apply();
        }

        public override void UpdateBinding(VisualEffect component)
        {
            UpdateTexture();
            component.SetTexture(TextureProperty, m_Texture);
            component.SetTexture(HistoryProperty, m_History);
            component.SetUInt(CountProperty, Samples);
            component.SetUInt(HistoryCountProperty, History);
        }

        public override string ToString()
        {
            return string.Format("Audio Spectrum : '{0} samples' -> {1}", m_CountProperty, (Mode == AudioSourceMode.AudioSource ? "AudioSource" : "AudioListener"));
        }
    }
}