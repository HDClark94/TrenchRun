using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomiseBitmap : MonoBehaviour {
    public Material[] bitmaps;
    public Renderer rend;
    public OVRPlayerController OVRPlayerController;

    // Use this for initialization
    void Start () {
        rend.GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = bitmaps[0];
	}
	
	// Update is called once per frame
	public void SetBitmap() {

        if (OVRPlayerController.sensory_mod == "auditory")
        {
            // initialise wall with black color without optic flow, this is the last image in bitmaps []
            rend.sharedMaterial = bitmaps[38];
            Debug.LogWarning("Setting black wall");
        } else if (OVRPlayerController.sensory_mod == "visual")
        {
            // initialise wall with black color without optic flow, this is the last image in bitmaps []
            rend.sharedMaterial = bitmaps[38];
            Debug.LogWarning("Setting black wall");

            // currently not using bitmaps, reenable with code below

            // initialise wall with randomly selected bitmap image
            //rend.sharedMaterial = bitmaps[Random.Range(0, 37)];
            //Debug.LogWarning("Setting random bitmap");
        } else
        {
            Debug.LogWarning("Oops, the only sensory modalities available are auditory or visual");
            Debug.LogWarning("sensory_mod is " + OVRPlayerController.sensory_mod);
        }
    }
}
