using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IIngameMusicEvents
    {
        event EventHandler OnPlayerSpawned;
        event EventHandler OnPlayerKilled;
    }
    
    public class IngameMusic : MonoBehaviour
    {
        //[SerializeField] private Game musicEventsSource;
        
        [SerializeField] private AudioClip mainTheme;
        [SerializeField] private AudioClip sadTheme;
        
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void MusicEventsSource_OnPlayerKilled(object sender, EventArgs e)
        {
            StartCoroutine(ChangeTheme(sadTheme));
        }

        private void MusicEventsSource_OnPlayerSpawned(object sender, EventArgs e)
        {
            StartCoroutine(ChangeTheme(mainTheme));
        }

        private IEnumerator ChangeTheme(AudioClip theme)
        {
            audioSource.Stop();

            while (audioSource.isPlaying)
                yield return null;

            audioSource.clip = theme;
            audioSource.Play();
        }

        private void OnEnable()
        {
            //if (musicEventsSource == null)
            //    return;

            //musicEventsSource.OnPlayerSpawned += MusicEventsSource_OnPlayerSpawned;
            //musicEventsSource.OnPlayerKilled += MusicEventsSource_OnPlayerKilled;
        }

        private void OnDisable()
        {
            //if (musicEventsSource == null)
            //    return;

            //musicEventsSource.OnPlayerSpawned -= MusicEventsSource_OnPlayerSpawned;
            //musicEventsSource.OnPlayerKilled -= MusicEventsSource_OnPlayerKilled;
        }

    }
}