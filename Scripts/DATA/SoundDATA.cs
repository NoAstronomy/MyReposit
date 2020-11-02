using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundDATA
{
    public class SoundManager
    {
        public container container;
        public Sound sound;
        public SoundManager(GameObject me)
        {
            container = new container(this, me);
            sound = new Sound(this);
        }


    }

    public class container
    {
        private SoundManager link;
        public UnitHost unitHost;

        public List<GameObject> activeSound;
        //public GameObject current
        //public bool isStopSound;
        public container(SoundManager soundManager, GameObject me)
        {
            link = soundManager;
            unitHost = me.GetComponent<UnitHost>();
            activeSound = new List<GameObject>();
        }
    }

    public class Sound
    {
        private SoundManager link;

        public Sound(SoundManager soundManager)
        {
            link = soundManager;
        }
      //  public List<GameObject> sound;
       // public List<AudioStats> audioClip;
       
        public void CreateSound(AudioClip clip, string name)
        {
            GameObject g = new GameObject(name);
            g.AddComponent<AudioSource>();
            g.GetComponent<AudioSource>().clip = clip;
            link.container.activeSound.Add(g);
            int count = link.container.activeSound.Count;
            Object.Destroy(link.container.activeSound[count - 1], link.container.activeSound[count - 1].GetComponent<AudioSource>().clip.length);
           // g.GetComponent<AudioSource>().Play();
            //sound.Add(g);

        }

        public void DestroySound(string name)
        {
           // if(GameObject.FindObjectOfType<AudioSource>().name == name)
            //    Debug.Log("Destroy");
            //Object.Destroy(name);
        }


    }

}
