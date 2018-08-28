using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PoseDetectorScript : MonoBehaviour 
{
	[Tooltip("User avatar model, who needs to reach the target pose.")]
	public PoseModelHelper avatarModel;

	[Tooltip("Model in pose that need to be reached by the user.")]
	public PoseModelHelper poseModel;

	[Tooltip("List of joints to compare.")]
	public List<KinectInterop.JointType> poseJoints = new List<KinectInterop.JointType>();
	public List<KinectInterop.JointType> poseJointsSaver = new List<KinectInterop.JointType>();//this holds the list to be refered back to as the other list is cleared and changed to focus on specific joints

	[Tooltip("Threshold, above which we consider the pose is matched.")]
	public float matchThreshold = 0.7f;

	[Tooltip("GUI-Text to display information messages.")]
	public UnityEngine.UI.Text infoText;

	// match percent (between 0 and 1)
	private float fMatchPercent = 0f;
	// whether the pose is matched or not
	private bool bPoseMatched = false;
	public bool nextStep = false;
	private bool armExtention = true;
	private bool lunge = false;
	private bool standUp = false;
	private int step = 0;
	private int nextPose = 0;





	/// <summary>
	/// Gets the pose match percent.
	/// </summary>
	/// <returns>The match percent (value between 0 and 1).</returns>
	public float GetMatchPercent()
	{
		return fMatchPercent;
	}


	/// <summary>
	/// Determines whether the target pose is matched or not.
	/// </summary>
	/// <returns><c>true</c> if the target pose is matched; otherwise, <c>false</c>.</returns>
	public bool IsPoseMatched()
	{
		return bPoseMatched;
	}


	void Update () 
	{
		KinectManager kinectManager = KinectManager.Instance;
		AvatarController avatarCtrl = avatarModel ? avatarModel.gameObject.GetComponent<AvatarController>() : null;

		if(kinectManager != null && kinectManager.IsInitialized() && 
		   avatarModel != null && avatarCtrl && kinectManager.IsUserTracked(avatarCtrl.playerId))
		{
			// get mirrored state
			bool isMirrored = avatarCtrl.mirroredMovement;
			
			// get the difference
			string sDiffDetails = string.Empty;
			fMatchPercent = 1f - GetPoseDifference(isMirrored, true, ref sDiffDetails);
			bPoseMatched = (fMatchPercent >= matchThreshold);



			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			////making sure that the pose has finished moving, and reached the pose matech percentage before the next pose starts 
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			if(bPoseMatched == true && nextStep == true){
				step = step + 1;
				nextStep = false;
			}


			string sPoseMessage = string.Format("Pose match: {0:F0}% {1}", fMatchPercent * 100f, 
			                                    (bPoseMatched ? "- Matched" : ""));
			if(infoText != null)
			{
				infoText.text = sPoseMessage + "\n\n" + sDiffDetails;
			}
		}
		else
		{
			// no user found
			if(infoText != null)
			{
				infoText.text = "Try to match the pose on the left.";
			}
		}
	}


	// gets angle or percent difference in pose
	public float GetPoseDifference(bool isMirrored, bool bPercentDiff, ref string sDiffDetails)
	{
		float fAngleDiff = 0f;
		float fMaxDiff = 0f;
		sDiffDetails = string.Empty;

		KinectManager kinectManager = KinectManager.Instance;
		if(!kinectManager || !avatarModel || !poseModel || poseJoints.Count == 0)
		{
			return 0f;
		}

		// copy model rotation
		Quaternion poseSavedRotation = poseModel.GetBoneTransform(0).rotation;
		poseModel.GetBoneTransform(0).rotation = avatarModel.GetBoneTransform(0).rotation;

		StringBuilder sbDetails = new StringBuilder();
		sbDetails.Append("Joint differences:").AppendLine();

		//creating a secondary list to save the original position for joints to be refrenced back to 
		if(poseJointsSaver.Count == 0){
			for(int z = 0; z < poseJoints.Count; z++){
				poseJointsSaver.Add(poseJoints[z]);
			}
		}

		/////////////////////////////////////////////////
		//the joints that will be tacked for each exercise
		//////////////////////////////////////////////////

		if(armExtention == true){//sets the focus on the left shoulder,elbow, and hand of the pose avatar, the paticipants right arm 
			poseJoints[0] = poseJointsSaver[4];//shoulder right 
			poseJoints[1] = poseJointsSaver[5];//elbow right 
			poseJoints[2] = poseJointsSaver[11];//wrist right 
			for(int c = 3; c < poseJoints.Count; c++){//fills up the rest of the spots, needed otherwise the last joint, wrist right, will not be tracked
				poseJoints[c] = poseJointsSaver[13];
			}

		}
		if(standUp == true){
			poseJoints[0] = poseJointsSaver[16];//hip right
			poseJoints[1] = poseJointsSaver[17];//knee right
			poseJoints[2] = poseJointsSaver[18];//ankle right
			poseJoints[3] = poseJointsSaver[19];//hip left
			poseJoints[4] = poseJointsSaver[20];//knee left
			poseJoints[5] = poseJointsSaver[21];//ankle left
			poseJoints[6] = poseJointsSaver[4];//shoulder right
			poseJoints[7] = poseJointsSaver[5];//elbow right
			poseJoints[8] = poseJointsSaver[3];//shoulder left
			poseJoints[9] = poseJointsSaver[6];//elbow left 
			for(int c = 10; c < poseJoints.Count; c++){//fills up the rest of the spots, needed otherwise the last joint, elbow left, will not be tracked
				poseJoints[c] = poseJointsSaver[13];
			}

		}
		if(lunge == true){
			poseJoints[0] = poseJointsSaver[16];//hip right
			poseJoints[1] = poseJointsSaver[17];//knee right
			poseJoints[2] = poseJointsSaver[18];//ankle right
			poseJoints[3] = poseJointsSaver[22];//foot right 
			poseJoints[4] = poseJointsSaver[19];//hip left
			poseJoints[5] = poseJointsSaver[20];//knee left
			poseJoints[6] = poseJointsSaver[21];//ankle left
			poseJoints[7] = poseJointsSaver[23];//foot left
			for(int c = 8; c < poseJoints.Count; c++){//fills up the rest of the spots, needed otherwise the last joint, foot left, will not be tracked
				poseJoints[c] = poseJointsSaver[13];
			}
		}


		for(int i = 0; i < poseJoints.Count; i++)
		{
;
			KinectInterop.JointType joint = poseJoints[i];

			KinectInterop.JointType nextJoint = kinectManager.GetNextJoint(joint);

			if(nextJoint != joint && (int)nextJoint >= 0 && (int)nextJoint < KinectInterop.Constants.MaxJointCount)
			{
				Transform avatarTransform1 = avatarModel.GetBoneTransform(avatarModel.GetBoneIndexByJoint(joint, isMirrored));
				Transform avatarTransform2 = avatarModel.GetBoneTransform(avatarModel.GetBoneIndexByJoint(nextJoint, isMirrored));


				Transform poseTransform1 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(joint, isMirrored));
				Transform poseTransform2 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(nextJoint, isMirrored));

				//These are the three steps in extend arm
				if(step == 0){//move the pose to first position, elbow bent hand bent
					

					//Place the code to make avatar start in position 1 elbow bent, wrist bent

					nextStep = true;// this is set to true after the pose has been reached to make sure the avater is not skipping ahead before the position has been reached.
				}


				//step two extend the arm
				if(step == 1){
					

					//code to move avatar to the next position, elbow straight but wrist bent 


					nextStep = true;
				}


				if(step == 2){// extend wrist foward
		


					//Place code to move avatar to final position of this exercise, wrist straight elbow straight


				nextStep = true;
	
				}

				//next exercise, sitting to standing 
				if(step == 3){
				armExtention = false;//stops focusing on just the right arm joints 
				standUp = true;//changes the joints that are being focused on
						
				
				//Place the code to move to seated position 



				nextStep = true;	

				}



				if(step == 4){//move elbows to 90 degree angles and move feet backwards 
						
				

				//place code to move avatar to elbows bent at 90 degrees holding onto the arm rests


				nextStep = true;

				}


				
				if(step == 5){//move pose to stand up 
						
				
				//place code to move after to standing position, final position for this exercise



				nextStep = true;
				}

				//next exercise, lunge
				if(step == 6){
					armExtention = false;//makes sure the joints for this exercise are not being tracked
					standUp = false;//makes sure the joints for this exercise are not being tracked
					lunge = true;//changes the joints being focused on to the legs and hips and off the arms 
						

						//place code to move leg forward and bend back leg for lunge position 

				}
				if(avatarTransform1 != null && avatarTransform2 != null && poseTransform1 != null && poseTransform2 != null)
				{
					Vector3 vAvatarBone = (avatarTransform2.position - avatarTransform1.position).normalized;
					Vector3 vPoseBone = (poseTransform2.position - poseTransform1.position).normalized;

					float fDiff = Vector3.Angle(vPoseBone, vAvatarBone);
					if(fDiff > 90f) fDiff = 90f;

					fAngleDiff += fDiff;
					fMaxDiff += 90f;  // we assume the max diff could be 90 degrees

					sbDetails.AppendFormat("{0} - {1:F0} deg.", joint, fDiff).AppendLine();
				}
				else
				{
					sbDetails.AppendFormat("{0} - n/a", joint).AppendLine();
				}
			}
		}

		poseModel.GetBoneTransform(0).rotation = poseSavedRotation;

		// calculate percent diff
		float fPercentDiff = 0f;
		if(bPercentDiff && fMaxDiff > 0f)
		{
			fPercentDiff = fAngleDiff / fMaxDiff;
		}

		// details info
		sbDetails.AppendLine();
		sbDetails.AppendFormat("Sum-Diff: - {0:F0} deg out of {1:F0} deg", fAngleDiff, fMaxDiff).AppendLine();
		sbDetails.AppendFormat("Percent-Diff: {0:F0}%", fPercentDiff * 100).AppendLine();
		sDiffDetails = sbDetails.ToString();
		
		return (bPercentDiff ? fPercentDiff : fAngleDiff);
	}

}

