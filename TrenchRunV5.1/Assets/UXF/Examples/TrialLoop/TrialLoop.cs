using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
// add the UXF namespace
using UXF;

namespace UXFExamples
{

    public class TrialLoop : MonoBehaviour
    {
        public float Probe_probability;
        public float Non_beaconed_probability;
        public float Session_duration;
        public string track_length_difficulty;
        private string TrialType;
        public bool isPractice;
        public GameObject Player;
        public Material cue;
        public Renderer rend;
        public int TrialCount;
        public int TrialCounter;
        public OVRPlayerController OVRPlayerController;
        public audiotory_flow audiotory_Flow;
        public AudioSource rz_left_audio;
        public AudioSource rz_right_audio;
        public GameObject wall;
        public bool moveCue;
        public bool variableCueContrast;
        public bool variableCueSight;
        public bool withinTime;
        public string task;
        public float correct_proportion = 0;
        public float probe_criteria;
        public float gain_std;
        public float gain_range;
        public bool show_correct;
        public string move_mech;
        public int n_scorable_stops;
        public int n_trials_per_track_length;
        public int n_tracks;
        public string sensory_mod;
        public string sensory_dens;
        public GameObject reward_zone_left_wall;
        public GameObject reward_zone_right_wall;
        public GameObject reward_zone_floor;

        // Use this for initialization
        void Awake()
        {
            OVRPlayerController = Player.GetComponent<OVRPlayerController>();
            rend.GetComponent<Renderer>();
            rend.enabled = true;
            rend.material = cue;
            withinTime = true;

    }

        void Start()
        {
            //Debug.Log("trial status is " + OVRPlayerController.TrialOver);
            OVRPlayerController.SetSessionTime(Session_duration);
            TrialType = "beaconed";
            TrialCount = 0;
            TrialCounter = 0;
        }

        UXF.Session session;

        public void GenerateExperiment(Session experimentSession)
        {
            // save reference to session
            session = experimentSession;
            // This function can be called using the Session inspector OnSessionBegin() event, or otherwise

            // retrieve the n_practice_trials setting, which was loaded from our .json file
            int numPracticeTrials = Convert.ToInt32(session.settings["n_practice_trials"]);

            // retrieve condition for type of task (cue to goal)
            task = Convert.ToString(session.settings["task"]);
            OVRPlayerController.SetTask(task);

            // retrieve probe criteria, this is the proportion of correct trials that needs to be passed to be able to get a probe trial
            probe_criteria = (float)Convert.ToDecimal(session.settings["probe_criteria"]);
            //Debug.Log("probe_criteria in start is " + probe_criteria);


            OVRPlayerController.setProbeCriteria(probe_criteria);

            // retrieve condition to move the cue or keep it in a constant location
            moveCue = Convert.ToBoolean(session.settings["move_cue"]);
            OVRPlayerController.SetMoveCue(moveCue);

            // retrieve condition to setTimer
            Session_duration = Convert.ToInt16(session.settings["Session_duration"]);
            OVRPlayerController.SetSessionTime(Session_duration);

            // retrieve track length difficulty
            track_length_difficulty = Convert.ToString(session.settings["track_length_difficulty"]);
            OVRPlayerController.SetTrackDifficulty(track_length_difficulty);

            // retrieve probabilities for nonbeaconed and probe trials
            Probe_probability = (float)Convert.ToDecimal(session.settings["Probe_probability"]);
            Non_beaconed_probability = (float)Convert.ToDecimal(session.settings["Non_beaconed_probability"]);

            // retrieve condition to enable variable cue contrast
            variableCueContrast = Convert.ToBoolean(session.settings["variable_cue_contrast"]);
            OVRPlayerController.SetVariableCueContrast(variableCueContrast);

            // retrieve condition to enable variable cue sight
            variableCueSight = Convert.ToBoolean(session.settings["variable_cue_sight"]);
            OVRPlayerController.SetVariableCueSight(variableCueSight);

            // retrieve gain standard deviation, if 0 then gain is unaltered trial-by-trial
            gain_std = (float)Convert.ToDecimal(session.settings["gain_std"]);
            OVRPlayerController.SetGainStd(gain_std);

            // retrieve gain range, if 0 then gain is unaltered trial-by-trial
            gain_range = (float)Convert.ToDecimal(session.settings["gain_range"]);
            Debug.LogWarning("gain_range in trial loop is " + gain_range);
            OVRPlayerController.SetGainRange(gain_range);

            // retrieve condition to indicate movement mechanics
            move_mech = Convert.ToString(session.settings["movement_mechanism"]);
            OVRPlayerController.SetMoveMech(move_mech);

            // retrieve condition for the number of scorable stops
            n_scorable_stops = Convert.ToInt32(session.settings["n_scorable_stops"]);
            OVRPlayerController.SetNScorableStops(n_scorable_stops);

            // retrieve show_correct this is a flag for showing the task progress as well as well as the score
            show_correct = Convert.ToBoolean(session.settings["show_correct"]);
            OVRPlayerController.SetShowCorrect(show_correct);

            // retrieve number of trials per track_length if incremental track lengths are used 
            n_trials_per_track_length = Convert.ToInt32(session.settings["n_trials_per_track_length"]);
            OVRPlayerController.SetNTrialsPerTrackLength(n_trials_per_track_length);

            // retrieve number of different track_lengths to use
            n_tracks = Convert.ToInt32(session.settings["n_tracks"]);
            OVRPlayerController.SetNTracks(n_tracks);

            // create block 1
            Block practiceBlock = session.CreateBlock(numPracticeTrials);
            practiceBlock.settings["practice"] = true;

            // retrieve the n_main_trials setting, which was loaded from our .json file into our session settings
            int numMainTrials = Convert.ToInt32(session.settings["n_main_trials"]);
            // create block 2
            numMainTrials = (n_tracks * n_trials_per_track_length);
            Block mainBlock = session.CreateBlock(numMainTrials); // block 2

            // retrieve condition to indicate primary sensory modality of the task
            sensory_mod = Convert.ToString(session.settings["sensory_modality"]);
            Debug.LogWarning("sensory_mod in trial loop is " + OVRPlayerController.sensory_mod);
            OVRPlayerController.SetSensoryMod(sensory_mod);
            setRZVolume(); // turn off reward zone audio for purely visual task
            setRZrender();

            // retrieve condition to indicate primary sensory modality of the task
            sensory_dens = Convert.ToString(session.settings["sensory_density"]);
            Debug.LogWarning("sensory_dens in trial loop is " + OVRPlayerController.sensory_dens);
            OVRPlayerController.SetSensoryDens(sensory_dens);

        }


        public void StartLoop()
        {
            // called from OnSessionBegin, hence starting the trial loop when the session starts
            StartCoroutine(Loop());
        }



        IEnumerator Loop()
        {
            //Debug.Log("OVRPlayerController.time_left is " + OVRPlayerController.time_left + "when being entered to OVRplayer");
            foreach (Trial trial in session.trials)
            {
                trial.Begin();
                TrialCount++;
                TrialCounter++;

                UpdateCorrectProportion();
                OVRPlayerController.UpdateVisibleScoreMeasures(correct_proportion);

                TrialType = ChooseTrialType(Probe_probability, Non_beaconed_probability, TrialCount);
                TrialType = OVRPlayerController.TrialTypeSettings(TrialType);
                OVRPlayerController.updateHintCubeColor();
                OVRPlayerController.SetMoveCue(moveCue);
                OVRPlayerController.SetRandomAcceleration(); // this function does nothing if it isn't needed
                audiotory_Flow.InstantiateAudioFlow();
                yield return new WaitUntil(IsTrialOver);
                RecordResults(trial);
                audiotory_Flow.DestroyAudioFlowClones();
                if (Convert.ToInt32(session.settings["n_practice_trials"]) == TrialCounter) { OVRPlayerController.ResetScore(); TrialCount = 0; OVRPlayerController.practiceOver = true; }
                trial.End();
                if(OVRPlayerController.time_left < 0)
                {
                    Debug.Log("breaking");
                    Debug.Log("Final total score: " + OVRPlayerController.actual_score);
                    Debug.Log("Final total rewards: " + OVRPlayerController.score);
                    Debug.Log("Final total beaconed score: " + OVRPlayerController.b_score);
                    Debug.Log("Final total non beaconed score: " + OVRPlayerController.nb_score);
                    Debug.Log("Final total probe score: " + OVRPlayerController.p_score);
                    break;
                }
            }

            // debugger shows score for money calculation
            Debug.Log("Player scored " + OVRPlayerController.score + " points"); 
            session.End();
        }

        void RecordResults(Trial trial)
        {
            trial.result["Trial type"] = TrialType;
            trial.result["Trial Scored"] = OVRPlayerController.actually_scored;
            trial.result["Cummulative Reward"] = OVRPlayerController.score;
            trial.result["Cummulative Score"] = OVRPlayerController.actual_score;
            trial.result["Cue Boundary Min"] = OVRPlayerController.CueBoundsXmins;
            trial.result["Cue Boundary Max"] = OVRPlayerController.CueBoundsXmaxs;
            trial.result["Transparency"] = cue.color.a;
            trial.result["Reward Boundary Min"] = OVRPlayerController.RewardBoundsXmins;
            trial.result["Reward Boundary Max"] = OVRPlayerController.RewardBoundsXmaxs;
            trial.result["Track Start"] = OVRPlayerController.TrackStart;
            trial.result["Track End"] = OVRPlayerController.TrackEnd;
            trial.result["Cue Sight"] = OVRPlayerController.CueSight;
            trial.result["Cummulative Beaconed Score"] = OVRPlayerController.b_score;
            trial.result["Cummulative Non Beaconed Score"] = OVRPlayerController.nb_score;
            trial.result["Cummulative Probe Score"] = OVRPlayerController.p_score;
            trial.result["Rewarded Location"] = OVRPlayerController.Reward_location;
            trial.result["Teleport from"] = OVRPlayerController.TeleportFrom;
            trial.result["Teleport to"] = OVRPlayerController.TeleportTo;
            trial.result["Acceleration"] = OVRPlayerController.Acceleration;

        }

        public bool IsTrialOver()
        {
            if (OVRPlayerController.TrialOver) { return true; }
            else { return false; }
        }

        public void UpdateCorrectProportion()
        {
            if (TrialCount == 1)
            {
                correct_proportion = 0f;
            }
            else
            {
                correct_proportion = (float)OVRPlayerController.actual_score / (float)(TrialCount - 1);
            }
            OVRPlayerController.SetCorrect_proportion(correct_proportion);
        }

        public void setRZrender()
        {
            if (sensory_mod == "auditory")
            {
                Debug.Log("This function isnt doing anything at the moment!");
                //OVRPlayerController.SetObjectRenderer(reward_zone_left_wall, false);
                //OVRPlayerController.SetObjectRenderer(reward_zone_right_wall, false);
                //OVRPlayerController.SetObjectRenderer(reward_zone_floor, false);
            }
        }

        public void setRZVolume()
        {
            if (sensory_mod == "visual")
            {
                rz_left_audio.volume = 0f;
                rz_right_audio.volume = 0f;
            }
        }

        public string ChooseTrialType(float Probe_probability, float Non_beaconed_probability, int TrialCount)
        {
            if (TrialCount == 1)
            {
                // this is just for the first trial
                 return "beaconed";
            }

            if (Probe_probability + Non_beaconed_probability > 1) { Debug.Log("Non-compatible probabilities entered!"); }

            float temp = UnityEngine.Random.value;
            string TrialType = null;

            if (temp <= Probe_probability)
            {
                if (correct_proportion>probe_criteria && TrialCount>10)
                {
                    TrialType = "probe";
                } else
                {
                    // non beaconed is given if correct proportion is not enough
                    TrialType = "non_beaconed";
                }
                
            }
            else if (temp>= 1- Non_beaconed_probability)
            {
                TrialType = "non_beaconed";
            }
            else {
                TrialType = "beaconed";
            }

            return TrialType;
        }
    }

}