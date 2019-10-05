using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace AaronStatic.Audio
{
    public enum Property
    {
        Bass,
        Mid,
        Treble,
        Sum,
        Avg,
        Spectrum,
        SpectrumHistory
    }

    public struct AudioData
    {
        public float Bass;
        public float Mid;
        public float Treble;
        public float Sum;
        public float Avg;
        public Texture2D Spectrum;
        public Texture2D SpectrumHistory;
    }

    [Serializable]
    public class UpdateEvent : UnityEvent<AudioData>
    {
    }

    [ExecuteInEditMode]
    public class AudioSpectrum : MonoBehaviour
    {
        public AudioSource AudioSource = null;
        public FFTWindow FFTWindow = FFTWindow.BlackmanHarris;
        public uint Samples = 64;
        public uint History = 64;
        public uint Frequency = 60;

        public UpdateEvent OnUpdate = new UpdateEvent();

        private float _nextUpdate = 0;

        private float[] m_lastSpectrum;
        private Texture2D m_Texture;
        private Texture2D m_History;

        private Color[] m_ColorCache;
        private Color[] m_HistoryCache;

        private int m_bassCutoff;
        private int m_midCutoff;
        
        void Update()
        {
            if (AudioSource == null) return;

            _nextUpdate -= Time.deltaTime;
            if(_nextUpdate <= 0)
            {
                _nextUpdate = 1 / Frequency;

                GetSpectrum();
                UpdateTextures();
                OnUpdate.Invoke(GetData());
            }
        }        

        private void GetSpectrum()
        {
            if(m_lastSpectrum == null || m_lastSpectrum.Length != Samples)
            {
                m_lastSpectrum = new float[Samples];
                m_bassCutoff = (int)Math.Floor((float)Samples * 0.25f);
                m_midCutoff = (int)Math.Floor((float)Samples * 0.75f);
            }
            AudioSource.GetSpectrumData(m_lastSpectrum, 0, FFTWindow);
        }

        private void UpdateTextures()
        {
            if (m_Texture == null || m_Texture.width != Samples || m_Texture.height != 1)
            {
                m_Texture = new Texture2D((int)Samples, 1, TextureFormat.RFloat, false);
            }

            if (m_History == null || m_History.width != Samples || m_History.height != History)
            {
                m_History = new Texture2D((int)Samples, (int)History, TextureFormat.RFloat, false);
            }

            if(m_ColorCache == null || m_ColorCache.Length != Samples)
            {
                m_ColorCache = new Color[Samples];                
            }

            if (m_HistoryCache == null || m_HistoryCache.Length != Samples * History)
            {
                m_HistoryCache = new Color[Samples * History];
            }

            for (int i = 0; i < Samples; i++)
            {
                m_ColorCache[i] = new Color(m_lastSpectrum[i], 0, 0, 0);
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

        private AudioData GetData()
        {
            AudioData data = new AudioData();

            float val = 0;
            float total = 0;

            //Bass
            for (int i = 0; i < m_bassCutoff; i++)
            {
                val += m_lastSpectrum[i];
            }
            data.Bass = val / m_bassCutoff;
            total += val;

            //Mid
            val = 0;
            for (int i = m_bassCutoff; i < m_midCutoff; i++)
            {
                val += m_lastSpectrum[i];
            }
            data.Mid = val / (m_midCutoff - m_bassCutoff);
            total += val;

            //Treble
            val = 0;
            for (int i = m_midCutoff; i < Samples; i++)
            {
                val += m_lastSpectrum[i];
            }
            data.Treble = val / (Samples - m_midCutoff);
            total += val;

            data.Sum = total;
            data.Avg = total / Samples;
            data.Spectrum = m_Texture;
            data.SpectrumHistory = m_History;

            return data;
        }
    }
}