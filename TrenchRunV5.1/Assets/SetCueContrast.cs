using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCueContrast : MonoBehaviour {
    public Material cue;
    public Renderer rend;
    public GameObject Player;
    public OVRPlayerController OVRPlayerController;
    private bool variableCueContrast;
    private bool cue_visiblility;
    public float alphaLevel = 1.0f;
    // Use this for initialization

    void Start()
    {
        rend.GetComponent<Renderer>();
        rend.enabled = true;
        rend.material = cue;
    }

    public void SetContrast()
    {
        variableCueContrast = OVRPlayerController.variableCueContrast;
        cue_visiblility = OVRPlayerController.cueVisibility;

        if (cue_visiblility)
        {
            //Debug.Log("variableCueContrast in SetCueContrast script is " + variableCueContrast);
            if (variableCueContrast)
            {
                alphaLevel = Random.Range(0.0f, 1.0f);
                cue.color = new Color(1, 1, 1, alphaLevel);
            }
            else
            {
                cue.color = new Color(1, 1, 1, alphaLevel);
            }

        } else
        {
            cue.color = new Color(1, 1, 1, 0f);
            // this disables visibility of the cue
        }
        

            //Debug.Log("alphaLevel in SetContrast script is " + alphaLevel);
    }
}
