using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// add the UXF namespace
using UXF;
using UXFExamples;

public class SetCueGoalLength : MonoBehaviour
{
    public float resetMin = 0;
    public float resetMax = 0;
    public Vector3 cue_location;
    private float temp;
    public GameObject Player;
    public OVRPlayerController OVRPlayerController;
    public TextMesh LevelText;
    private string track_length_difficulty;
    private float originalIntegrationDistance;
    private int trial_count = 0;
    public float track_length_factor;
    public int random_track_length_factor;
    private float og_x;
    public List<int> track_length_factors = new List<int>();
    // Use this for initialization

    void Start()
    {
        track_length_factor = 0;
        cue_location = transform.localPosition;
        og_x = cue_location.x;
        //Debug.Log("transform.localPosition is " + transform.localPosition);
    }

    public int DrawTrackLengthFactor()
    {
        bool drawn = false;
        while (drawn == false)
        {
            random_track_length_factor = UnityEngine.Random.Range(1, (OVRPlayerController.n_tracks)+1);
            if (!(track_length_factors.Contains(random_track_length_factor)))
            {
                track_length_factors.Add(random_track_length_factor);
                drawn = true;
            }
        }
        return random_track_length_factor;
    }

    public void SetLength()
    {
        trial_count = OVRPlayerController.Trial_number;
        if (trial_count == 0) { track_length_factor = 0; }

        // set length to 1.5x short track for medium and 1.5x medium for long track
        originalIntegrationDistance = OVRPlayerController.ogIntegrationDistance;
        track_length_difficulty = OVRPlayerController.track_length_difficulty;
        //Debug.Log("track_length_difficulty in SetCueGoalLength script is " + track_length_difficulty);
        if (track_length_difficulty == "short")
        {
            // no change if short
            transform.localPosition = new Vector3(cue_location.x, cue_location.y, cue_location.z);
            cue_location = transform.localPosition;
        }
        else if (track_length_difficulty == "medium")
        {
            // 1.5x short track
            temp = ((originalIntegrationDistance * 1.5f) - originalIntegrationDistance);
            transform.localPosition = new Vector3(temp, cue_location.y, cue_location.z);
            cue_location = transform.localPosition;
        }
        else if (track_length_difficulty == "long")
        {
            // 1.5^2*short track
            temp = ((originalIntegrationDistance * 1.5f * 1.5f) - originalIntegrationDistance); ;
            transform.localPosition = new Vector3(temp, cue_location.y, cue_location.z);
            cue_location = transform.localPosition;
        }
        else if (track_length_difficulty == "incremental_nb" || track_length_difficulty == "incremental_p")
        {
            if (trial_count % OVRPlayerController.n_trials_per_track_length == 0 && OVRPlayerController.practiceOver)
            {
                LevelText.text = "New Level";
                temp = cue_location.x + ((originalIntegrationDistance * (float)Math.Pow(1.5, track_length_factor+1)) - (originalIntegrationDistance * (float)Math.Pow(1.5, track_length_factor)));
                transform.localPosition = new Vector3(temp, cue_location.y, cue_location.z);
                cue_location = transform.localPosition;
                track_length_factor = track_length_factor + 1;
            } else if (OVRPlayerController.practiceOver == false) {
                LevelText.text = "  Practice";
            } else {
            LevelText.text = "UNSEEN TEXT";
            }

        }
        else if (track_length_difficulty == "random_length_nb" || track_length_difficulty == "random_length_p")
        {
            if (trial_count % 2 == 0 && trial_count > 0)
            {
                // 7.5 used as this is approx 1.5^5 eg, the 6th 
                float rand_tmp = UnityEngine.Random.Range(0.66f, 5f);
                Debug.Log("rand_tmp in SetCueGoalLength script is " + rand_tmp);
                temp = og_x + ((originalIntegrationDistance * rand_tmp) - originalIntegrationDistance);
                transform.localPosition = new Vector3(temp, cue_location.y, cue_location.z);
                cue_location = transform.localPosition;
            }
        }
        else if (track_length_difficulty == "random_incremental_nb" || track_length_difficulty == "random_incremental_p")
        {
            if (trial_count % OVRPlayerController.n_trials_per_track_length == 0 && OVRPlayerController.practiceOver)
            {
                LevelText.text = "New Level";
                int random_track_length_factor = DrawTrackLengthFactor();
                Debug.Log("random_track_length_factor in SetCueGoalLength script is " + random_track_length_factor);
                temp = og_x + ((originalIntegrationDistance * (float)Math.Pow(1.5, random_track_length_factor)) - originalIntegrationDistance);
                transform.localPosition = new Vector3(temp, cue_location.y, cue_location.z);
                cue_location = transform.localPosition;
            } else if (OVRPlayerController.practiceOver == false) {
                LevelText.text = "  Practice";
            } else {
                LevelText.text = "UNSEEN TEXT";
            }
        }


        //Debug.Log("newset in SetCueGoalLength script is " + cue_location);

            //Debug.Log("cue_location.x is  " + cue_location.x );
            //Debug.Log("cue_location.x-80f is  " + (cue_location.x - 80f));
            //Debug.Log("transform.localPosition is " + transform.localPosition);
    }
}
