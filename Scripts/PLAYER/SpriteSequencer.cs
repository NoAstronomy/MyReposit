using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSequencer : MonoBehaviour
{

    public static SpriteSequencer spriteSequencer;
    public Object dashPref;
    private List<GameObject> dash = new List<GameObject>();

    void Start()
    {
        spriteSequencer = this;
    }

    void Update()
    {
        for (int i = 0; i < dash.Count; i++)
        {
            if(dash[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Dash"))
                continue;
            Destroy(dash[i]);
            dash.Remove(dash[i]);
        }
    }

    public static void CreateDashSequence(Vector3 origin, int i, Vector2 currentScale)
    {
        return;
        GameObject localDash = Instantiate(spriteSequencer.dashPref, origin, Quaternion.identity) as GameObject;
        localDash.transform.localScale = currentScale;
        localDash.GetComponent<Animator>().SetFloat("Speed", 2f / Mathf.Pow(i+1, 0.4f));
        spriteSequencer.dash.Add(localDash as GameObject);
    }
}
