using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class IngameSounds : MonoBehaviour
    {
        [SerializeField] private AudioClip[] sounds;        

        private AudioSource audioSource;

        private readonly Dictionary<string, AudioClip> soundsDict = new();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            foreach (var clip in sounds)
                soundsDict.Add(clip.name, clip);
        }
        public void SoundEventHandler(SFX sfx) =>
            audioSource.PlayOneShot(soundsDict[sfx.FileName], sfx.VolumeScale);
    }
}