/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus Utilities SDK License Version 1.31 (the "License"); you may not use
the Utilities SDK except in compliance with the License, which is provided at the time of installation
or download, or which otherwise accompanies this software in either electronic or hard copy form.
You may obtain a copy of the License at https://developer.oculus.com/licenses/utilities-1.31

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
	/// <summary>
	/// The rate acceleration during movement.
	/// </summary>
	public float Acceleration = 0.1f;
    // initialise for no movement before trail start
    public float Acceleration_factor = 0f;
	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	public float Damping = 0.3f;
    
    // press to pause timer 
    public bool pauseTimer = false;

	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.
	/// </summary>
	public float BackAndSideDampen = 0.5f;

	/// <summary>
	/// The force applied to the character when jumping.
	/// </summary>
	public float JumpForce = 0.3f;

	/// <summary>
	/// The rate of rotation when using a gamepad.
	/// </summary>
	public float RotationAmount = 1.5f;

	/// <summary>
	/// The rate of rotation when using the keyboard.
	/// </summary>
	public float RotationRatchet = 45.0f;

	/// <summary>
	/// The player will rotate in fixed steps if Snap Rotation is enabled.
	/// </summary>
	[Tooltip("The player will rotate in fixed steps if Snap Rotation is enabled.")]
	public bool SnapRotation = true;

	/// <summary>
	/// How many fixed speeds to use with linear movement? 0=linear control
	/// </summary>
	[Tooltip("How many fixed speeds to use with linear movement? 0=linear control")]
	public int FixedSpeedSteps;

	/// <summary>
	/// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
	/// </summary>
	public bool HmdResetsY = true;

	/// <summary>
	/// If true, tracking data from a child OVRCameraRig will update the direction of movement.
	/// </summary>
	public bool HmdRotatesY = true;

	/// <summary>
	/// Modifies the strength of gravity.
	/// </summary>
	public float GravityModifier = 0.379f;

	/// <summary>
	/// If true, each OVRPlayerController will use the player's physical height.
	/// </summary>
	public bool useProfileData = true;

	/// <summary>
	/// The CameraHeight is the actual height of the HMD and can be used to adjust the height of the character controller, which will affect the
	/// ability of the character to move into areas with a low ceiling.
	/// </summary>
	[NonSerialized]
	public float CameraHeight;

	/// <summary>
	/// This event is raised after the character controller is moved. This is used by the OVRAvatarLocomotion script to keep the avatar transform synchronized
	/// with the OVRPlayerController.
	/// </summary>
	public event Action<Transform> TransformUpdated;

	/// <summary>
	/// This bool is set to true whenever the player controller has been teleported. It is reset after every frame. Some systems, such as
	/// CharacterCameraConstraint, test this boolean in order to disable logic that moves the character controller immediately
	/// following the teleport.
	/// </summary>
	[NonSerialized] // This doesn't need to be visible in the inspector.
	public bool Teleported;

	/// <summary>
	/// This event is raised immediately after the camera transform has been updated, but before movement is updated.
	/// </summary>
	public event Action CameraUpdated;

	/// <summary>
	/// This event is raised right before the character controller is actually moved in order to provide other systems the opportunity to
	/// move the character controller in response to things other than user input, such as movement of the HMD. See CharacterCameraConstraint.cs
	/// for an example of this.
	/// </summary>
	public event Action PreCharacterMove;

	/// <summary>
	/// When true, user input will be applied to linear movement. Set this to false whenever the player controller needs to ignore input for
	/// linear movement.
	/// </summary>
	public bool EnableLinearMovement = true;

	/// <summary>
	/// When true, user input will be applied to rotation. Set this to false whenever the player controller needs to ignore input for rotation.
	/// </summary>
	public bool EnableRotation = true;

	public CharacterController Controller = null;
	protected OVRCameraRig CameraRig = null;

	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private OVRPose? InitialPose;
	public float InitialYRotation { get; private set; }
	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool  SkipMouseRotation = true; // It is rare to want to use mouse movement in VR, so ignore the mouse by default.
	private bool  HaltUpdateMovement = false;
	private bool prevHatLeft = false;
	private bool prevHatRight = false;
	private float SimulationRate = 60f;
	private float buttonRotation = 0f;
	private bool ReadyToSnapTurn; // Set to true when a snap turn has occurred, code requires one frame of centered thumbstick to enable another snap turn.

    private Vector3 originalPos = Vector3.zero;
    public bool TrialOver = false;

    public int frame_counter = 0;
    public GameObject RewardZone;
    public Renderer RewardZoneRend;
    public GameObject Track;
    public Renderer TrackRend;
    public GameObject EndBlackBox;
    public Renderer EndBlackBoxRend;
    public GameObject CueZone;
    public Renderer CueZoneRend;
    public GameObject BlackBoxEnd;
    public Renderer BlackBoxEndRend;
    public AudioClip RewardClip;
    public AudioSource source;
    public float TrackLength;
    public float IntegrationDistance;
    public float ogIntegrationDistance;

    public GameObject beaconed_cue1;
    public GameObject beaconed_cue2;
    public GameObject beaconed_cue3;
    public GameObject non_beaconed_cue1;
    public GameObject non_beaconed_cue2;
    public GameObject non_beaconed_cue3;
    

    public float Reward_location = 0.0f;

    // for disabling when they aren't used

    public bool scored = false;
    public int score;

    public int actual_score;
    public bool actually_scored = false;

    public bool canScore = true;
    public TextMesh ScoreText;
    public TextMesh LevelText;
    public TextMesh PercentCorrect;
    public float RewardBoundsXmins;
    public float RewardBoundsXmaxs;
    public float CueBoundsXmins;
    public float CueBoundsXmaxs;
    public float TrackStart;
    public float TrackEnd;
    public float TrackRendEnd;
    public bool moveCue;
    public bool variableCueContrast;
    public bool variableCueSight;
    public bool cueVisibility = true;
    public bool Timer_on = false;
    public float time_left;
    public float stop_time_stamp;
    public float LevelText_time_stamp;
    public float CueSight = 1.0f;
    public bool shaping;
    public string move_mech;
    private int movecounter=0;
    public int frames_for_tap;
    public float extratap_influence;
    public string last_action_button = "None";
    public int n_scorable_stops;
    public int n_scorable_stops_left;
    public bool currently_stopped = false;
    public bool practiceOver = false;
    public float button_tap_factor = 1;
    public int button_counter = 0;
    public List<int> button_tap_frames = new List<int>();
    public int n_trials_per_track_length;
    public GameObject hintcube;
    public string sensory_mod;
    public string sensory_dens;
    public int n_tracks;

    public int p_score = 0;
    public int nb_score = 0;
    public int b_score = 0;

    public string track_length_difficulty = "short";

    public string TrialType;
    public string task;
    public int Trial_number = 0;
    
    public float TeleportTo;
    public float TeleportFrom;

    public float gain_std;
    public float gain_range;
    public bool show_correct;
    public float correct_proportion;
    public float probe_criteria;

    Vector3 moveDirection = Vector3.zero;

    void Start()
	{
        Cursor.visible = true;
        // Add eye-depth as a camera offset from the player controller
        var p = CameraRig.transform.localPosition;
		p.z = OVRManager.profile.eyeDepth;
		CameraRig.transform.localPosition = p;
        score = 0;
    }

	void Awake()
	{
        source = GetComponent<AudioSource>();
		Controller = gameObject.GetComponent<CharacterController>();

		if(Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

		// We use OVRCameraRig to set rotations to cameras,
		// and to be influenced by rotation
		OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

		if(CameraRigs.Length == 0)
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		else if (CameraRigs.Length > 1)
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		else
			CameraRig = CameraRigs[0];

		InitialYRotation = transform.rotation.eulerAngles.y;
        originalPos = Controller.transform.localPosition;

        RewardBoundsXmins = RewardZoneRend.GetComponent<Renderer>().bounds.min.x;
        RewardBoundsXmaxs = RewardZoneRend.GetComponent<Renderer>().bounds.max.x;

        CueBoundsXmins = CueZoneRend.GetComponent<Renderer>().bounds.min.x;
        CueBoundsXmaxs = CueZoneRend.GetComponent<Renderer>().bounds.max.x;

        TrackStart = TrackRend.GetComponent<Renderer>().bounds.min.x;
        //TrackEnd = TrackRend.GetComponent<Renderer>().bounds.max.x;
        TrackEnd = BlackBoxEndRend.GetComponent<Renderer>().bounds.min.x;
        //Debug.Log("RewardBoundsXmins is " + RewardBoundsXmins);
        //Debug.Log("RewardBoundsXmaxs is " + RewardBoundsXmaxs);
        //Debug.Log("moveCue is " + moveCue);

        TrackLength = TrackEnd - TrackStart;

        IntegrationDistance = CueBoundsXmaxs - RewardBoundsXmins;
        ogIntegrationDistance = IntegrationDistance;
        Debug.Log("IntegrationDistance is " + IntegrationDistance);

        TeleportFrom = TrackEnd + 100f;
        TeleportTo = Controller.transform.localPosition.x;

    }

    void OnEnable()
	{
		OVRManager.display.RecenteredPose += ResetOrientation;

		if (CameraRig != null)
		{
			CameraRig.UpdatedAnchors += UpdateTransform;
		}
	}

	void OnDisable()
	{
		OVRManager.display.RecenteredPose -= ResetOrientation;

		if (CameraRig != null)
		{
			CameraRig.UpdatedAnchors -= UpdateTransform;
		}
	}

	void Update() // called every frame
	{
        Scoring();
        IsTrialOver();
        // update time_left by subtracting time of processing frame
        if (Timer_on && pauseTimer==false) {
            time_left -= Time.deltaTime;
        }
	}

	protected virtual void UpdateController()
	{
		if (useProfileData)
		{
			if (InitialPose == null)
			{
				// Save the initial pose so it can be recovered if useProfileData
				// is turned off later.
				InitialPose = new OVRPose()
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}

			var p = CameraRig.transform.localPosition;
			if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
			{
				p.y = OVRManager.profile.eyeHeight - (0.5f * Controller.height) + Controller.center.y;
			}
			else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
			{
				p.y = - (0.5f * Controller.height) + Controller.center.y;
			}
			CameraRig.transform.localPosition = p;
		}
		else if (InitialPose != null)
		{
			// Return to the initial pose if useProfileData was turned off at runtime
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}

		CameraHeight = CameraRig.centerEyeAnchor.localPosition.y;

		if (CameraUpdated != null)
		{
			CameraUpdated();
		}

		UpdateMovement();

		moveDirection = Vector3.zero;

		float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

		// Gravity
		if (Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

		moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;


		if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
		{
			// Offset correction for uneven ground
			float bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}

		if (PreCharacterMove != null)
		{
			PreCharacterMove();
			Teleported = false;
		}

		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

		// Move contoller
		Controller.Move(moveDirection);
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);



    }

    public void updateHintCubeColor()
    {
        var cubeRenderer = hintcube.GetComponent<Renderer>();

        if (TrialType == "beaconed")
        {
            cubeRenderer.material.SetColor("_Color", Color.green);
            Debug.Log("green");
        } else
        {
            cubeRenderer.material.SetColor("_Color", Color.black);
            Debug.Log("black");
        }
    }



    public virtual void UpdateMovement()
	{
		if (HaltUpdateMovement)
			return;

		if (EnableLinearMovement)
		{
            bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
			bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
			bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            frame_counter += 1;
            if (move_mech == "button_tap")
            {
                if (button_tap_frames.Contains(frame_counter - frames_for_tap))
                {
                    button_counter -= 1;
                    button_tap_factor = 1 + (extratap_influence * button_counter);
                    button_tap_frames.Remove(frame_counter - frames_for_tap);

                }
                else if (!button_tap_frames.Any())
                {
                    moveForward = false;
                }
                else
                {
                    moveForward = true;
                }

                if (OVRInput.GetUp(OVRInput.RawButton.X) || OVRInput.GetUp(OVRInput.RawButton.A))
                {
                    button_counter += 1;
                    moveForward = true;

                    button_tap_frames.Add(frame_counter);
                    button_tap_factor = 1 + (extratap_influence * button_counter);
                }

            } else if (move_mech == "button_tap_coordinated")
            {

                if (button_tap_frames.Contains(frame_counter - frames_for_tap))
                {
                    button_counter -= 1;
                    button_tap_factor = 1 + (extratap_influence * button_counter);
                    button_tap_frames.Remove(frame_counter - frames_for_tap);

                }
                else if (!button_tap_frames.Any())
                {
                    moveForward = false;
                }
                else
                {
                    moveForward = true;
                }

                if (OVRInput.GetUp(OVRInput.RawButton.X) && (last_action_button == "None" || last_action_button == "A"))
                {
                    button_counter += 1;
                    moveForward = true;
                    last_action_button = "X";
                    button_tap_frames.Add(frame_counter);
                    button_tap_factor = 1 + (extratap_influence * button_counter);

                }
                else if (OVRInput.GetUp(OVRInput.RawButton.A) && (last_action_button == "None" || last_action_button == "X"))
                {
                    button_counter += 1;
                    moveForward = true;
                    last_action_button = "A";
                    button_tap_frames.Add(frame_counter);
                    button_tap_factor = 1 + (0.2f * button_counter);
                }
            }

			bool dpad_move = false;

			if (OVRInput.Get(OVRInput.Button.DpadUp))
			{
				moveForward = true;
				dpad_move = true;

			}

			if (OVRInput.Get(OVRInput.Button.DpadDown))
			{
				moveBack = true;
				dpad_move = true;
			}

			MoveScale = 1.0f;

			if ((moveForward && moveLeft) || (moveForward && moveRight) ||
				(moveBack && moveLeft) || (moveBack && moveRight))
				MoveScale = 0.70710678f;

			// No positional movement if we are in the air
			//if (!Controller.isGrounded)
			//	MoveScale = 0.0f;

			MoveScale *= SimulationRate * Time.deltaTime;

			// Compute this for key movement
			float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier * button_tap_factor;

			// Run!
			//if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            //    moveInfluence *= 2.0f;

			Quaternion ort = transform.rotation;
			Vector3 ortEuler = ort.eulerAngles;
			ortEuler.z = ortEuler.x = 0f;
			ort = Quaternion.Euler(ortEuler);

			if (moveForward)
			    MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
			if (moveBack)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
			if (moveLeft)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
			if (moveRight)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);


			moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
            //if (move_mech != "button_tap" && move_mech != "button_tap_coordinated")
            //{
            //    moveInfluence *= 1.0f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            //}
			
#endif

			Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            if (move_mech == "button_tap" || move_mech == "button_tap_coordinated")
            {
                primaryAxis.x = 0.0f;
                primaryAxis.y = 0.0f;
            }

			// If speed quantization is enabled, adjust the input to the number of fixed speed steps.
			if (FixedSpeedSteps > 0)
			{
				primaryAxis.y = Mathf.Round(primaryAxis.y * FixedSpeedSteps) / FixedSpeedSteps;
				primaryAxis.x = Mathf.Round(primaryAxis.x * FixedSpeedSteps) / FixedSpeedSteps;
			}

			if (primaryAxis.y > 0.0f)
				MoveThrottle += ort * (primaryAxis.y * transform.lossyScale.z * moveInfluence * Vector3.forward);

			if (primaryAxis.y < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.y) * transform.lossyScale.z * moveInfluence *
									   BackAndSideDampen * Vector3.back);

			if (primaryAxis.x < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.x) * transform.lossyScale.x * moveInfluence *
									   BackAndSideDampen * Vector3.left);

			if (primaryAxis.x > 0.0f)
				MoveThrottle += ort * (primaryAxis.x * transform.lossyScale.x * moveInfluence * BackAndSideDampen *
									   Vector3.right);
		}

		if (EnableRotation)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

			bool curHatLeft = OVRInput.Get(OVRInput.Button.PrimaryShoulder);

			if (curHatLeft && !prevHatLeft)
				euler.y -= RotationRatchet;

			prevHatLeft = curHatLeft;

			bool curHatRight = OVRInput.Get(OVRInput.Button.SecondaryShoulder);

			if (curHatRight && !prevHatRight)
				euler.y += RotationRatchet;

			prevHatRight = curHatRight;

			euler.y += buttonRotation;
			buttonRotation = 0f;


#if !UNITY_ANDROID || UNITY_EDITOR
			if (!SkipMouseRotation)
				euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
#endif

			if (SnapRotation)
			{

				//if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft))
				//{
					//if (ReadyToSnapTurn)
					//{
						//euler.y -= RotationRatchet;
						//ReadyToSnapTurn = false;
					//}
				//}
				//else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight))
				//{
					//if (ReadyToSnapTurn)
					//{
						//euler.y += RotationRatchet;
						//ReadyToSnapTurn = false;
					//}
				//}
				//else
				//{
					//ReadyToSnapTurn = true;
				//}
			//}
			//else
			//{
				//Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				//euler.y += secondaryAxis.x * rotateInfluence;
			}

			transform.rotation = Quaternion.Euler(euler);
		}
	}


	/// <summary>
	/// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
	/// </summary>
	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform root = CameraRig.trackingSpace;
		Transform centerEye = CameraRig.centerEyeAnchor;

		if (HmdRotatesY && !Teleported)
		{
			Vector3 prevPos = root.position;
			Quaternion prevRot = root.rotation;

			transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

			root.position = prevPos;
			root.rotation = prevRot;
		}

		UpdateController();
		if (TransformUpdated != null)
		{
			TransformUpdated(root);
		}
	}

	/// <summary>
	/// Jump! Must be enabled manually.
	/// </summary>
	public bool Jump()
	{
		if (!Controller.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

		return true;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	/// <summary>
	/// Gets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">Allow mouse rotation.</param>
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	/// <summary>
	/// Sets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	/// <summary>
	/// Resets the player look rotation when the device orientation is reset.
	/// </summary>
	public void ResetOrientation()
	{
		if (HmdResetsY && !HmdRotatesY)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			euler.y = InitialYRotation;
			transform.rotation = Quaternion.Euler(euler);
		}
	}

    public void ResetTrial()
    {
        TrialOver = false;
        RewardBoundsXmins = RewardZoneRend.GetComponent<Renderer>().bounds.min.x;
        RewardBoundsXmaxs = RewardZoneRend.GetComponent<Renderer>().bounds.max.x;
        TrackEnd = BlackBoxEndRend.GetComponent<Renderer>().bounds.min.x;

        CueBoundsXmins = CueZoneRend.GetComponent<Renderer>().bounds.min.x;
        CueBoundsXmaxs = CueZoneRend.GetComponent<Renderer>().bounds.max.x;

    }

    public void IsTrialOver()
    {
        if (Controller.transform.localPosition.x > TeleportFrom)
            // TrackEnd relates to current position of trial end(entrance to black box)
        {
            TrialOver = true;
            
        }
    }

    public void SetMoveCue(bool moveCue_bool)
    {
        moveCue = moveCue_bool;
    }

    public void SetTask(string task_string)
    {
        task = task_string;
        if (task == "blackbox2goal")
        {
            cueVisibility = false;
            moveCue = false;
            SetObjectRenderer(beaconed_cue1, cueVisibility);
            SetObjectRenderer(beaconed_cue2, cueVisibility);
            SetObjectRenderer(beaconed_cue3, cueVisibility);
            SetObjectRenderer(non_beaconed_cue1, cueVisibility);
            SetObjectRenderer(non_beaconed_cue2, cueVisibility);
            SetObjectRenderer(non_beaconed_cue3, cueVisibility);

            // TODO refactor this, very messy

        } else if (task == "cue2goal") {
            // do nothing this is default task
        }
    }

    public void SetVariableCueContrast(bool variableCueContrast_bool)
    {
        variableCueContrast = variableCueContrast_bool;
    }

    public void SetVariableCueSight(bool variableCueSight_bool)
        // not fully implemented yet
    {
        variableCueSight = variableCueSight_bool;
    }

    public void SetTrackDifficulty(string track_length_difficulty_string)
    {
        track_length_difficulty = track_length_difficulty_string;
    }

    public void EnableMovement()
    {
        Acceleration = Acceleration_factor;
        Timer_on = true;
    }

    public void DisableMovement()
    {
        Acceleration = 0f;
    }

    public void SetSessionTime(float Session_duration)
    {
        time_left = Session_duration*60; // multiply by 60 as deltatime is in seconds
        stop_time_stamp = time_left;
    }

    public void ResetScored()
    {
        scored = false;
        actually_scored = false;
        Reward_location = 0.0f;
        b_score = 0;
        nb_score = 0;
        p_score = 0;
        n_scorable_stops_left = n_scorable_stops;
    }

    public void Scoring()
    {
        if (Controller.transform.localPosition.x > RewardBoundsXmins && Controller.transform.localPosition.x < RewardBoundsXmaxs && 
            scored == false && moveDirection.x < 0.001 && moveDirection.x > -0.001 && actually_scored == false && n_scorable_stops_left>0)
        {
            actual_score++;
            actually_scored = true;

            Reward_location = Controller.transform.localPosition.x; // save location of reward

            if (TrialType == "beaconed")
            {
                b_score++;
            }
            else if (TrialType == "non_beaconed")
            {
                nb_score++;
            }
            else if (TrialType == "probe")
            {
                p_score++;
            }
        }

        if (Controller.transform.localPosition.x > RewardBoundsXmins && Controller.transform.localPosition.x < RewardBoundsXmaxs && 
            scored == false && moveDirection.x < 0.001 && moveDirection.x > -0.001 && canScore && n_scorable_stops_left > 0)
        {
            score++;
            ScoreText.text = "Score: " + actual_score.ToString();
            source.Play();
            scored = true;

        }

        if (moveDirection.x < 0.001 && moveDirection.x > -0.001)
        {
            if (currently_stopped == false)
            {
                n_scorable_stops_left -= 1;
                currently_stopped = true;
                stop_time_stamp = time_left;
            }

        }
        else if ((stop_time_stamp - time_left) > 0.1)
        {
            currently_stopped = false;
        }

        // reset n_scorable stops if next level text is still displayded and turn off next level text
        if ((LevelText_time_stamp - time_left) > 2) {
            LevelText.color = new Color(0, 0, 0, 0); // invisible
        }

    }

    public string TrialTypeSettings(string TrialType_string)
    {
        Trial_number += 1;

        // set trial type for other functions
        TrialType = TrialType_string;

        //override this trialtype if the imcremental method is used.
        if (track_length_difficulty == "incremental_nb" || track_length_difficulty == "random_length_nb" || track_length_difficulty == "random_incremental_nb")
        {
            if (Trial_number % 2 == 1)
            {
                TrialType = "beaconed";
            } else
            {
                TrialType = "non_beaconed";
            }
        }

        if (track_length_difficulty == "incremental_p" || track_length_difficulty == "random_length_p" || track_length_difficulty == "random_incremental_p")
        {
            if (Trial_number % 2 == 1)
            {
                TrialType = "beaconed";
            }
            else
            {
                TrialType = "probe";
            }
        }


        if (TrialType == "beaconed")
        {
            Controller.transform.localPosition = new Vector3(originalPos.x, Controller.transform.localPosition.y, originalPos.z+30f);
            canScore = true;
        }
        else if (TrialType == "non_beaconed")
        {
            Controller.transform.localPosition = new Vector3(originalPos.x, Controller.transform.localPosition.y, originalPos.z);
            canScore = true;
        }
        else if (TrialType == "probe")
        {
            Controller.transform.localPosition = new Vector3(originalPos.x, Controller.transform.localPosition.y, originalPos.z);
            canScore = false;
        }
        else { Debug.Log("passed wrong string to TrialTypeSettings in TrialLoop class"); }

        return TrialType;
    }

    public void updateBounds()
    { 
        RewardBoundsXmins = RewardZoneRend.GetComponent<Renderer>().bounds.min.x;
        RewardBoundsXmaxs = RewardZoneRend.GetComponent<Renderer>().bounds.max.x;

        CueBoundsXmins = CueZoneRend.GetComponent<Renderer>().bounds.min.x;
        CueBoundsXmaxs = CueZoneRend.GetComponent<Renderer>().bounds.max.x;

        TrackStart = TrackRend.GetComponent<Renderer>().bounds.min.x;
        TrackRendEnd = TrackRend.GetComponent<Renderer>().bounds.max.x;
        TrackEnd = BlackBoxEndRend.GetComponent<Renderer>().bounds.min.x;

        TrackLength = TrackEnd - TrackStart;
        IntegrationDistance = CueBoundsXmaxs - RewardBoundsXmins;

        TeleportFrom = TrackEnd + 100f;
        TeleportTo = Controller.transform.localPosition.x;

    }

    public void ResetScore()
    {
        score = 0;
        actual_score = 0;
        ScoreText.text = "Score: " + actual_score.ToString();
        correct_proportion = 0;
        Trial_number = 0;
        //PercentCorrect.text = "% Correct: " + (correct_proportion*100f).ToString();

    }

    public void SetObjectRenderer(GameObject gameobject, bool visible)
    {
        gameobject.GetComponent<Renderer>().enabled = visible;
    }

    public void SetShaping(bool shaping_bool)
    {
        shaping = shaping_bool;
    }

    public void SetMoveMech(string move_mech_string)
    {
        move_mech = move_mech_string;
        if (move_mech=="analogue" || move_mech=="button_tap" || move_mech == "button_tap_synchronised")
        {
            Debug.Log("move_mech" + move_mech + "selected");
        } else
        {
            Debug.Log("incorrect move_mech entered:" + move_mech + ", currently only analogue, button_tap and button_tap_synchronised is supported");
        }
    }

    public void SetRandomAcceleration()
    {
        Debug.Log("gainstd is "+ gain_std);
        Debug.Log("gainrange is " + gain_range);

        if (gain_std > 0)
        {
            Debug.Log("doing that");
            Acceleration = generateNormal(Acceleration_factor, gain_std);
            if (Acceleration <= 0)
            {
                SetRandomAcceleration();
            }
            //Debug.Log("acceleration is set to" + Acceleration);
        } else if (gain_range > 0)
        {
            Acceleration = UnityEngine.Random.Range(Acceleration_factor-(gain_range/2), Acceleration_factor+(gain_range / 2));
            Debug.Log("doing this");
        }
        Debug.Log("doing nothing");

    }

    public float generateNormal(float mu, float sigma) {
        float x1 = UnityEngine.Random.Range(0.0f, 1.0f);
        float x2 = UnityEngine.Random.Range(0.0f, 1.0f);

        return mu + (sigma*(UnityEngine.Mathf.Sqrt((-2f * UnityEngine.Mathf.Log(x1)))*UnityEngine.Mathf.Cos((2 * UnityEngine.Mathf.PI) * x2)));

    }

    public void SetGainRange(float gain_range_float)
    {
        gain_range = gain_range_float;
    }

    public void SetGainStd(float gain_std_float)
    {
        gain_std = gain_std_float;
    }

    public void SetNScorableStops(int n_scorable_stops_int)
    {
        n_scorable_stops = n_scorable_stops_int;
        n_scorable_stops_left = n_scorable_stops_int;
    }

    public void SetShowCorrect(bool show_correct_bool)
    {
        show_correct = show_correct_bool;
        if (show_correct)
        {
            PercentCorrect.color = new Color(0, 255, 0, 1);
        }
        else
        {
            PercentCorrect.color = new Color(0, 255, 0, 0);
        }
    }

    public void setPercentCorrectColor(float probe_criteria, float correct_proportion)
        // this can be alters quickly to have a different colour for below or over criteria
    {
        if (show_correct)
        {
            PercentCorrect.color = new Color(0, 255, 0);


            //if (correct_proportion >= probe_criteria)
            //{
            //    PercentCorrect.color = new Color(0, 255, 0);
            //} else
            //{
            //PercentCorrect.color = new Color(255, 0, 0);
            //}
            

        } else
        {
            PercentCorrect.color = new Color(0, 0, 0, 0);
        }  
    }

    public void setNextLevelText()
    {
        if (LevelText.text == "New Level")
        {
            LevelText.color = new Color(255, 255, 255, 1); // white
            LevelText_time_stamp = time_left;
        } else if (LevelText.text == "UNSEEN TEXT")
        {
            LevelText.color = new Color(0, 0, 0, 0); // invisible
        }
    }


    public void setProbeCriteria(float probe_criteria_float)
    {
        probe_criteria = probe_criteria_float;
    }

    public void SetNTrialsPerTrackLength(int trials_per_track_length_int)
    {
        n_trials_per_track_length = trials_per_track_length_int;
    }
    public void UpdateVisibleScoreMeasures(float correct_proportion)
    {
        PercentCorrect.text = "✓ " + (Mathf.RoundToInt(correct_proportion *100f)).ToString() + "%";
        ScoreText.text = "Score: " + actual_score.ToString();    // this is also called elsewhere and isn't needed here, I just added it so all the score updates are in the same place   
        //setPercentCorrectColor(probe_criteria, correct_proportion);
    }

    public void SetCorrect_proportion(float correct_proportion_float)
    {
        correct_proportion = correct_proportion_float;
    }

    public void SetSensoryMod(string sensory_mod_string)
    {
        sensory_mod = sensory_mod_string;
        Debug.LogWarning("sensory_mod in OVRPlayerController is " + sensory_mod);
    }

    public void SetSensoryDens(string sensory_dens_string)
    {
        sensory_dens = sensory_dens_string;
        Debug.LogWarning("sensory_dens in OVRPlayerController is " + sensory_dens);
    }

    public void SetNTracks(int n_tracks_int)
    {
        n_tracks = n_tracks_int;
    }


}
