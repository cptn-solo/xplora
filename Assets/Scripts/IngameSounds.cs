using Assets.Scripts.Services.App;
using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts
{
    public class IngameSounds : MonoBehaviour
    {
        [Inject] private readonly AudioPlaybackService audioService;

        [SerializeField] private AudioClip[] sounds;        

        private AudioSource audioSource;

        private readonly Dictionary<string, AudioClip> soundsDict = new();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            foreach (var clip in sounds)
                soundsDict.Add(clip.name, clip);
        }
        private void Start()
        {
            audioService.AttachSounds(this);
        }
        public void SoundEventHandler(SFX sfx) =>
            audioSource.PlayOneShot(soundsDict[sfx.FileName]);
    }
}