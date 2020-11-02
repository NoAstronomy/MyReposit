using System.Collections;
using System.Collections.Generic;
using SoundDATA;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public SoundDATA.SoundManager soundManager;
    public ControllDATA.ControllManager controllManager;
    public r_BloodDATA.BloodManager bloodManager;
    public AudioClip move;
    public AudioClip jump;
    public AudioClip onGround;
    public AudioClip crystalize;
    public AudioClip dash;
    public AudioClip roll;
    public AudioClip slide;
    public AudioClip grab;
    public AudioClip inertion;
    public Scarf_Follow_Test scarfHandler;

    void Awake()
    {
    }
    void Start()
    {
        soundManager = new SoundManager(gameObject);
        controllManager = GetComponent<PlayerController>().controllManager;
        bloodManager = GetComponent<PlayerController>().bloodController.bloodManager;


       // if(bloodManager != null)
       bloodManager.OnCrystalize += (args) =>
        {
            soundManager.sound.CreateSound(crystalize, "Crystalize");
            return true;
        };

        controllManager.container.OnGrab += (args) =>
        {
           // Debug.Log(321);
            soundManager.sound.CreateSound(grab, "Grab");
        };

        controllManager.container.OnSlide += (args) =>
        {
            //Debug.Log(321);
            soundManager.sound.CreateSound(slide, "Slide");
        };

        controllManager.container.OnUnSlide += (args) =>
        {
            //Debug.Log(321);
            soundManager.sound.DestroySound("Slide");
        };

        controllManager.container.OnDodge += (args) =>
        {
           if((int)args[0] == 1)
               soundManager.sound.CreateSound(dash, "Dash");

            if ((int)args[0] == 2)
                soundManager.sound.CreateSound(roll, "Roll");
        };

        controllManager.container.OnJump += (args) =>
        {
            soundManager.sound.CreateSound(jump, "Jump");
        };

        controllManager.container.OnLand += (args) =>
        {
            soundManager.sound.CreateSound(onGround, "Land");
        };

        controllManager.container.OnEndDash += (args) =>
        {
            if ((int) args[0] == 1)
                soundManager.sound.CreateSound(inertion, "Inertion");
        };


    }

    public void scarfPosBias(float xbias) {
        //scarfHandler.xBias = xbias;
        //yBias = ybias;

    }

    public void A_Step()
    {
        soundManager.sound.CreateSound(move, "Move");
    }

    public void JumpEvent()
    {
        //soundManager.sound.CreateSound(jump);
    }

    void Update()
    {
       // if (controllManager.container.currentMovement == true)
       // {
           // StartCoroutine(make_moveSound());
      //  }

        
    }
}
