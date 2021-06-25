using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rewardPlay : MonoBehaviour {
    public GameObject Player;
    public OVRPlayerController OVRPlayerController;
    public AudioClip RewardClip;
    public AudioSource RewardSource;

    // Use this for initialization
    void Start () {
   
    }

    void Awake()
    {
        OVRPlayerController = Player.GetComponent<OVRPlayerController>();
    }

    // Update is called once per frame
    void Update () {
        if (OVRPlayerController.scored)
        {
            RewardSource.Play();
        }
 
		
	}
}
