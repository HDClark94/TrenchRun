using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audiotory_flow : MonoBehaviour {
    public GameObject original_audioflow;
    public GameObject original_flowcube;
    public OVRPlayerController OVRPlayerController;
    public int n_sounds_per_wall = 4; //current hard coded 408 is a rough approximation for the same number used in the visual game
    public GameObject wall;
    private int n_audio;
    private int n_walls = 3;
    private int which_wall;
    private GameObject[] AudioClones;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {	
	}

    public void InstantiateAudioFlow()
    {
        if (OVRPlayerController.sensory_mod == "auditory")
        {
            // make flow cube invisible
            original_flowcube.GetComponent<Renderer>().enabled = false;
            original_audioflow.GetComponent<AudioSource>().mute = false;

            if (OVRPlayerController.sensory_dens == "high")
            { n_sounds_per_wall = 60; }

            // activating 20% cue sparsity (80% decrease in density of cues available)
            else if (OVRPlayerController.sensory_dens == "low")
            { n_sounds_per_wall = 12; }
            
            
        } else if (OVRPlayerController.sensory_mod == "visual")
        {
            // make flow cube visible
            original_flowcube.GetComponent<Renderer>().enabled = true;
            original_audioflow.GetComponent<AudioSource>().mute = true;

            if (OVRPlayerController.sensory_dens == "high")
            { n_sounds_per_wall = 400; }

            // activating 20% cue sparsity (80% decrease in density of cues available)
            else if (OVRPlayerController.sensory_dens == "low")
            { n_sounds_per_wall = 80; }
        }

        n_audio = (int) (n_sounds_per_wall * n_walls * ((OVRPlayerController.TrackEnd-OVRPlayerController.TrackStart+100) /
                (OVRPlayerController.TrackRendEnd - OVRPlayerController.TrackStart)));
        Debug.Log("n_sounds_per_wall is " + n_sounds_per_wall);
        Debug.Log("tmp is " + ((OVRPlayerController.TrackEnd - OVRPlayerController.TrackStart) /
            (OVRPlayerController.TrackRendEnd - OVRPlayerController.TrackStart)));
          
        Debug.Log("OVRPlayerController.TrackEnd is " + (OVRPlayerController.TrackEnd));
        Debug.Log("OVRPlayerController.TrackStart is " + (OVRPlayerController.TrackStart));
        AudioClones = new GameObject[n_audio];

        for (int i = 0; i < n_audio; i++)
        {
            which_wall = Random.Range(0, n_walls);
            if (which_wall == 0)
            {
                AudioClones[i] = Instantiate(original_audioflow, new Vector3(Random.Range(OVRPlayerController.TrackStart-100, OVRPlayerController.TrackEnd),
                    Random.Range(OVRPlayerController.transform.position.y - 8f, OVRPlayerController.transform.position.y + 2f),
                    OVRPlayerController.transform.position.z+7f), original_audioflow.transform.rotation);
            } else if (which_wall == 1)
            {
                AudioClones[i] = Instantiate(original_audioflow, new Vector3(Random.Range(OVRPlayerController.TrackStart-100, OVRPlayerController.TrackEnd),
                    Random.Range(OVRPlayerController.transform.position.y - 8f, OVRPlayerController.transform.position.y + 2f),
                    OVRPlayerController.transform.position.z-7f), original_audioflow.transform.rotation);
            } else if (which_wall == 2)
            {
                AudioClones[i] = Instantiate(original_audioflow, new Vector3(Random.Range(OVRPlayerController.TrackStart-100, OVRPlayerController.TrackEnd),
                OVRPlayerController.transform.position.y - 8f,
                Random.Range(OVRPlayerController.transform.position.z-7f, OVRPlayerController.transform.position.z+7f)), original_audioflow.transform.rotation);
            }        
        }
    }


    public void DestroyAudioFlowClones()
    {   
        //if (OVRPlayerController.sensory_mod == "auditory")
        //Debug.Log("destroying clones at end of trial");
        foreach (GameObject AudioClone in AudioClones)
        {
            Destroy(AudioClone);
        }
    }
}
