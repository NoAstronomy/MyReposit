using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioStats", menuName = "Create Audio Stats")]
public class AudioStats : ScriptableObject
{
    [Header("Audio Clip")]
    public AudioClip clip;
}
