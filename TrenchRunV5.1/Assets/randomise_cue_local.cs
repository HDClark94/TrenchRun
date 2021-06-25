using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// add the UXF namespace
using UXF;
using UXFExamples;

public class randomise_cue_local : MonoBehaviour {
    public float resetMin = 0;
    public float resetMax = 0;
    public Vector3 cue_location;
    public GameObject Player;
    public OVRPlayerController OVRPlayerController;
    private bool moveCue;
    private string task;

    private float track_start;
    private float cue_end;

    // Use this for initialization

    void Start () {
        cue_location = transform.localPosition;
    }


    public void SetCue()
    {
        moveCue = OVRPlayerController.moveCue;
        task = OVRPlayerController.task;

        if (moveCue) {
            transform.localPosition = new Vector3(Random.Range(resetMin, resetMax), cue_location.y, cue_location.z);
            cue_location = transform.localPosition;
            //Debug.Log("transform.localPosition in randomise_cue_local script is " + transform.localPosition);
        }    

        if (task == "blackbox2goal") {
            //Debug.Log("called");
            track_start = OVRPlayerController.TrackStart;
            cue_end = OVRPlayerController.CueBoundsXmaxs;

            transform.localPosition = new Vector3(cue_location.x+(cue_end-track_start), cue_location.y, cue_location.z);
            cue_location = transform.localPosition;
        }

        OVRPlayerController.updateBounds();

    }
}
