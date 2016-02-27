using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 


[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{	
	// Bool that has the characters (facing the player) actions become mirrored. Default false.
	public bool mirroredMovement = false;
	
	// Bool that determines whether the avatar is allowed to move in vertical direction.
	public bool verticalMovement = false;
	
	// Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
	protected int moveRate = 1;
	
	// Slerp smooth factor
	public float smoothFactor = 5f;
	
	// Whether the offset node must be repositioned to the user's coordinates, as reported by the sensor or not.
	public bool offsetRelativeToSensor = false;

    public Vector3 newOffset = Vector3.zero;

	// The body root node
	protected Transform bodyRoot;
	
	// A required variable if you want to rotate the model in space.
	protected GameObject offsetNode;
	
	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	protected Transform[] bones;
	
	// Rotations of the bones when the Kinect tracking starts.
	protected Quaternion[] initialRotations;
	protected Quaternion[] initialLocalRotations;
	
	// Initial position and rotation of the transform
	protected Vector3 initialPosition;
	protected Quaternion initialRotation;
	
	// Calibration Offset Variables for Character Position.
	protected bool offsetCalibrated = false;
	protected float xOffset, yOffset, zOffset;

	// private instance of the KinectManager
	protected KinectManager kinectManager;


	// transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
	private Transform _transformCache;
	public new Transform transform
	{
		get
		{
			if (!_transformCache) 
				_transformCache = base.transform;
			
			return _transformCache;
		}
	}
	
	public void Awake()
    {	
		// check for double start
		if(bones != null)
			return;

        //Debug.Log("AvatarController on " + gameObject.name);
		
		// inits the bones array
		bones = new Transform[(int)KinectCommon.NuiSkeletonPositionIndex.Count];
		
		// Initial rotations and directions of the bones.
		initialRotations = new Quaternion[bones.Length];
		initialLocalRotations = new Quaternion[bones.Length];

		// Map bones to the points the Kinect tracks
		MapBones();

		// Get initial bone rotations
		GetInitialRotations();
	}
	
	// Update the avatar each frame.
    public void UpdateAvatar(ulong UserID)
    {	
		if(!transform.gameObject.activeInHierarchy) 
			return;

        Debug.Log("UpdateAvatar on " + gameObject.name);

		// Get the KinectManager instance
		if(kinectManager == null)
		{
			kinectManager = KinectManager.Instance;
		}
		
		// move the avatar to its Kinect position
        //Debug.Log("Try moving on " + gameObject.name);
		MoveAvatar(UserID);

        //Debug.Log("Before Bone transform loop on " + gameObject.name + " expected length " + bones.Length);
		for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
            //Debug.Log(boneIndex + " mapping to joint " + bones[boneIndex] + " on " + gameObject.name, bones[boneIndex]);
            //Debug.Log("Try Bone transform loop on " + gameObject.name, bones[boneIndex]);
			if (bones[boneIndex] == null) 
				continue;
			
			if(boneIndex2JointMap.ContainsKey(boneIndex))
			{
				KinectCommon.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
				TransformBone(UserID, joint, boneIndex, !mirroredMovement);
			}
			else if(specIndex2JointMap.ContainsKey(boneIndex))
			{
				// special bones (clavicles)
				List<KinectCommon.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];
				
				if(alJoints.Count >= 2)
				{
					//Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
					//TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
				}
			}
		}
	}
	
	// Set bones to their initial positions and rotations
	public void ResetToInitialPosition()
	{	
		if(bones == null)
			return;
		
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			transform.rotation = Quaternion.identity;
		}
		
		// For each bone that was defined, reset to initial position.
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				bones[i].rotation = initialRotations[i];
			}
		}
		
		if(bodyRoot != null)
		{
			bodyRoot.localPosition = Vector3.zero;
			bodyRoot.localRotation = Quaternion.identity;
		}
		
		// Restore the offset's position and rotation
		if(offsetNode != null)
		{
			offsetNode.transform.position = initialPosition;
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.position = initialPosition;
			transform.rotation = initialRotation;
		}
	}
	
	// Invoked on the successful calibration of a player.
	public void SuccessfulCalibration(ulong userId)
	{
		// reset the models position
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		
		// re-calibrate the position offset
		offsetCalibrated = false;
	}
	
	// Apply the rotations tracked by kinect to the joints.
	protected void TransformBone(ulong userId, KinectCommon.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
        //Debug.Log("Before Transforming bone for " + gameObject.name);
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;

        //Debug.Log("Enter Transforming bone for " + gameObject.name);

        //Debug.Log("Transforming boneIndex " + boneIndex + " mapping to joint " + joint.ToString("g") + " transform ", boneTransform);
		
		int iJoint = (int)joint;
		if(iJoint < 0)
			return;
		
		// Get Kinect joint orientation
		Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
		if(jointRotation == Quaternion.identity)
			return;
		
		// Smoothly transition to the new rotation
		Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
		
		if(smoothFactor != 0f)
        	boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
		else
			boneTransform.rotation = newRotation;
	}
	
	// Apply the rotations tracked by kinect to a special joint
	protected void TransformSpecialBone(ulong userId, KinectCommon.NuiSkeletonPositionIndex joint, KinectCommon.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;
		
		if(!kinectManager.IsJointTracked(userId, (int)joint) || 
		   !kinectManager.IsJointTracked(userId, (int)jointParent))
		{
			return;
		}
		
		Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
		Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;
		
//		if(!flip)
//		{
//			Vector3 mirroredAngles = jointRotation.eulerAngles;
//			mirroredAngles.y = -mirroredAngles.y;
//			mirroredAngles.z = -mirroredAngles.z;
//			
//			jointRotation = Quaternion.Euler(mirroredAngles);
//		}
		
		if(jointRotation != Quaternion.identity)
		{
			// Smoothly transition to the new rotation
			Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
			
			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}
		
	}
	
	// Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
	// Only pulls positional, not rotational.
	protected void MoveAvatar(ulong UserID)
	{
        //Debug.Log("Enter MoveAvatar for userId " + UserID);
		if(bodyRoot == null || kinectManager == null)
			return;
        //Debug.Log("bodyRoot and kinectManager ensured");
		if(!kinectManager.IsJointTracked(UserID, (int)KinectCommon.NuiSkeletonPositionIndex.SpineBase))
			return;
		
        // Get the position of the body and store it.
		Vector3 trans = kinectManager.GetUserPosition(UserID);
        //Debug.Log("GetUserPosition returned " + trans);
		
		// If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
		if (!offsetCalibrated)
		{
            Debug.Log("Calibrating user position");
			offsetCalibrated = true;
			
			xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			yOffset = trans.y * moveRate;
			zOffset = -trans.z * moveRate;
			
			if(offsetRelativeToSensor)
			{
				Vector3 cameraPos = Camera.main.transform.position;
				
				float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
				Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
				Vector3 offsetPos = cameraPos + relativePos;
				
				if(offsetNode != null)
				{
					offsetNode.transform.position = offsetPos;
				}
				else
				{
					transform.position = offsetPos;
				}
			}
		}
	
		// Smoothly transition to the new position
		Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

        targetPos += newOffset;

		if(smoothFactor != 0f)
			bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
		else
			bodyRoot.localPosition = targetPos;
	}
	
	// If the bones to be mapped have been declared, map that bone to the model.
	protected virtual void MapBones()
	{
		// make OffsetNode as a parent of model transform.
		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
		offsetNode.transform.position = transform.position;
		offsetNode.transform.rotation = transform.rotation;
		offsetNode.transform.parent = transform.parent;
		
		transform.parent = offsetNode.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		
		// take model transform as body root
		bodyRoot = transform;
		
		// get bone transforms from the animator component
		var animatorComponent = GetComponent<Animator>();
		
		for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!boneIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;

			bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
		}
	}
	
	// Capture the initial rotations of the bones
	protected void GetInitialRotations()
	{
		// save the initial rotation
		if(offsetNode != null)
		{
			initialPosition = offsetNode.transform.position;
			initialRotation = offsetNode.transform.rotation;
			
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			initialPosition = transform.position;
			initialRotation = transform.rotation;
			
			transform.rotation = Quaternion.identity;
		}
		
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
				initialLocalRotations[i] = bones[i].localRotation;
			}
		}
		
		// Restore the initial rotation
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.rotation = initialRotation;
		}
	}
	
	// Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
	protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
		// Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		
		//If an offset node is specified, combine the transform with its
		//orientation to essentially make the skeleton relative to the node
		if (offsetNode != null)
		{
			// Grab the total rotation by adding the Euler and offset's Euler.
			Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
			// Grab our new rotation.
			newRotation = Quaternion.Euler(totalRotation);
		}
		
		return newRotation;
	}
	
	// Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
		float xPos;
		float yPos;
		float zPos;
		
		// If movement is mirrored, reverse it.
		if(!mirroredMovement)
			xPos = jointPosition.x * moveRate - xOffset;
		else
			xPos = -jointPosition.x * moveRate - xOffset;
		
		yPos = jointPosition.y * moveRate - yOffset;
		zPos = -jointPosition.z * moveRate - zOffset;
		
		// If we are tracking vertical movement, update the y. Otherwise leave it alone.
		Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);
		
		return avatarJointPos;
	}
	
	// dictionaries to speed up bones' processing
	// the author of the terrific idea for kinect-joints to mecanim-bones mapping
	// along with its initial implementation, including following dictionary is
	// Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
	private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
    {
        {0, HumanBodyBones.Hips},
        {1, HumanBodyBones.Spine},
        {2, HumanBodyBones.Neck},
        {3, HumanBodyBones.Head},

        {4, HumanBodyBones.LeftShoulder},
        {5, HumanBodyBones.LeftUpperArm},
        {6, HumanBodyBones.LeftLowerArm},
        {7, HumanBodyBones.LeftHand},

        {8, HumanBodyBones.RightShoulder},
        {9, HumanBodyBones.RightUpperArm},
        {10, HumanBodyBones.RightLowerArm},
        {11, HumanBodyBones.RightHand},

        {12, HumanBodyBones.LeftUpperLeg},
        {13, HumanBodyBones.LeftLowerLeg},
        {14, HumanBodyBones.LeftFoot},
        {15, HumanBodyBones.LeftToes},

        {16, HumanBodyBones.RightUpperLeg},
        {17, HumanBodyBones.RightLowerLeg},
        {18, HumanBodyBones.RightFoot},
        {19, HumanBodyBones.RightToes},

        {20, HumanBodyBones.Chest},

        {21, HumanBodyBones.LeftMiddleDistal},
        {22, HumanBodyBones.LeftThumbProximal},

        {23, HumanBodyBones.RightMiddleDistal},
        {24, HumanBodyBones.RightThumbProximal},
	};
	
	protected readonly Dictionary<int, KinectCommon.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectCommon.NuiSkeletonPositionIndex>
	{
        {0, KinectCommon.NuiSkeletonPositionIndex.SpineBase},
        {1, KinectCommon.NuiSkeletonPositionIndex.SpineMid},
        {2, KinectCommon.NuiSkeletonPositionIndex.Neck},
        {3, KinectCommon.NuiSkeletonPositionIndex.Head},

        {4, KinectCommon.NuiSkeletonPositionIndex.ShoulderLeft},
        {5, KinectCommon.NuiSkeletonPositionIndex.ElbowLeft},
        {6, KinectCommon.NuiSkeletonPositionIndex.WristLeft},
        {7, KinectCommon.NuiSkeletonPositionIndex.HandLeft},

        {8, KinectCommon.NuiSkeletonPositionIndex.ShoulderRight},
        {9, KinectCommon.NuiSkeletonPositionIndex.ElbowRight},
        {10, KinectCommon.NuiSkeletonPositionIndex.WristRight},
        {11, KinectCommon.NuiSkeletonPositionIndex.HandRight},

        {12, KinectCommon.NuiSkeletonPositionIndex.HipLeft},
        {13, KinectCommon.NuiSkeletonPositionIndex.KneeLeft},
        {14, KinectCommon.NuiSkeletonPositionIndex.AnkleLeft},
        {15, KinectCommon.NuiSkeletonPositionIndex.FootLeft},

        {16, KinectCommon.NuiSkeletonPositionIndex.HipRight},
        {17, KinectCommon.NuiSkeletonPositionIndex.KneeRight},
        {18, KinectCommon.NuiSkeletonPositionIndex.AnkleRight},
        {19, KinectCommon.NuiSkeletonPositionIndex.FootRight},

        {20, KinectCommon.NuiSkeletonPositionIndex.SpineShoulder},

        {21, KinectCommon.NuiSkeletonPositionIndex.HandTipLeft},
        {22, KinectCommon.NuiSkeletonPositionIndex.ThumbLeft},

        {23, KinectCommon.NuiSkeletonPositionIndex.HandTipRight},
        {24, KinectCommon.NuiSkeletonPositionIndex.ThumbRight},
	};
	
	protected readonly Dictionary<int, List<KinectCommon.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectCommon.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectCommon.NuiSkeletonPositionIndex> {KinectCommon.NuiSkeletonPositionIndex.ShoulderLeft, KinectCommon.NuiSkeletonPositionIndex.SpineShoulder} },
		{9, new List<KinectCommon.NuiSkeletonPositionIndex> {KinectCommon.NuiSkeletonPositionIndex.ShoulderRight, KinectCommon.NuiSkeletonPositionIndex.SpineShoulder} },
	};
	
	protected readonly Dictionary<int, KinectCommon.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectCommon.NuiSkeletonPositionIndex>
	{
        {0, KinectCommon.NuiSkeletonPositionIndex.SpineBase},
        {1, KinectCommon.NuiSkeletonPositionIndex.SpineMid},
        {2, KinectCommon.NuiSkeletonPositionIndex.Neck},
        {3, KinectCommon.NuiSkeletonPositionIndex.Head},

        {4, KinectCommon.NuiSkeletonPositionIndex.ShoulderRight},
        {5, KinectCommon.NuiSkeletonPositionIndex.ElbowRight},
        {6, KinectCommon.NuiSkeletonPositionIndex.WristRight},
        {7, KinectCommon.NuiSkeletonPositionIndex.HandRight},

        {8, KinectCommon.NuiSkeletonPositionIndex.ShoulderLeft},
        {9, KinectCommon.NuiSkeletonPositionIndex.ElbowLeft},
        {10, KinectCommon.NuiSkeletonPositionIndex.WristLeft},
        {11, KinectCommon.NuiSkeletonPositionIndex.HandLeft},

        {12, KinectCommon.NuiSkeletonPositionIndex.HipRight},
        {13, KinectCommon.NuiSkeletonPositionIndex.KneeRight},
        {14, KinectCommon.NuiSkeletonPositionIndex.AnkleRight},
        {15, KinectCommon.NuiSkeletonPositionIndex.FootRight},

        {16, KinectCommon.NuiSkeletonPositionIndex.HipLeft},
        {17, KinectCommon.NuiSkeletonPositionIndex.KneeLeft},
        {18, KinectCommon.NuiSkeletonPositionIndex.AnkleLeft},
        {19, KinectCommon.NuiSkeletonPositionIndex.FootLeft},

        {20, KinectCommon.NuiSkeletonPositionIndex.SpineShoulder},

        {21, KinectCommon.NuiSkeletonPositionIndex.HandTipRight},
        {22, KinectCommon.NuiSkeletonPositionIndex.ThumbRight},

        {23, KinectCommon.NuiSkeletonPositionIndex.HandTipLeft},
        {24, KinectCommon.NuiSkeletonPositionIndex.ThumbLeft},
	};
	
	protected readonly Dictionary<int, List<KinectCommon.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectCommon.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectCommon.NuiSkeletonPositionIndex> {KinectCommon.NuiSkeletonPositionIndex.ShoulderRight, KinectCommon.NuiSkeletonPositionIndex.SpineShoulder} },
		{9, new List<KinectCommon.NuiSkeletonPositionIndex> {KinectCommon.NuiSkeletonPositionIndex.ShoulderLeft, KinectCommon.NuiSkeletonPositionIndex.SpineShoulder} },
	};
	
}

