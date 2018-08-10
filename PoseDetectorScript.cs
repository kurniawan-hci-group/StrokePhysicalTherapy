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
	private bool runthough = false;
	private bool armExtention = true;
	private bool lunge = false;
	private bool standUp = false;
	public int test = 0;
	public int step = 0;
	private int nextPose = 0;
	private int x = 0;
	private int y = 0;
	private int h = 0;

	Transform holdShoulder;
	Transform holdElbow;
	Transform holdHand;
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



			if(bPoseMatched == true && nextStep == true && runthough == true){//making sure that the pose has commpletly finished moving before the next pose starts 
				Debug.Log("count");
				step = step + 1;
				runthough = false;
				y = 0;
				h = 0;
				nextPose = nextPose + 1;
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

		//creating the list for the joints that will be focused in each excercise
		if(poseJointsSaver.Count == 0){
			for(int z = 0; z < poseJoints.Count; z++){
				poseJointsSaver.Add(poseJoints[z]);
			}
		}
		if(armExtention == true){//sets the focus on the left shoulder,elbow, and hand of the pose avatar
			poseJoints[0] = poseJointsSaver[4];
			poseJoints[1] = poseJointsSaver[5];
			poseJoints[2] = poseJointsSaver[11];
			for(int c = 3; c < poseJoints.Count; c++){
				poseJoints[c] = poseJointsSaver[13];
			}

		}
		if(lunge == true){
			// poseJointsSaver.Add(HipLeft);
			// //poseJoints[1] = KneeLeft;
			// poseJoints[2] = poseJointsSaver[11];
			// for(int c = 3; c < poseJoints.Count; c++){
			// 	poseJoints[c] = poseJointsSaver[13];
			// }

		}
		if(standUp == true){

		}


		for(int i = 0; i < poseJoints.Count; i++)
		{
			// Debug.Log(poseJoints.Count);
			// Debug.Log(i);
			KinectInterop.JointType joint = poseJoints[i];
			// Debug.Log("joint");
			// Debug.Log(joint);
			// Debug.Log("poseJoints");
			// Debug.Log(poseJoints[i]);
			KinectInterop.JointType nextJoint = kinectManager.GetNextJoint(joint);

			if(nextJoint != joint && (int)nextJoint >= 0 && (int)nextJoint < KinectInterop.Constants.MaxJointCount)
			{
				Transform avatarTransform1 = avatarModel.GetBoneTransform(avatarModel.GetBoneIndexByJoint(joint, isMirrored));
				Transform avatarTransform2 = avatarModel.GetBoneTransform(avatarModel.GetBoneIndexByJoint(nextJoint, isMirrored));

				if(test < 20){ // not going to be needed anymore because we will be using the list of joints I created 
				test = test + 1;
				}

				Transform poseTransform1 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(joint, isMirrored));
				Transform poseTransform2 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(nextJoint, isMirrored));
				// Debug.Log("poseTransform1");
				// Debug.Log(poseTransform1);
				// Debug.Log("poseTransform2");
				// Debug.Log(poseTransform2);
				Debug.Log("Test");
				Debug.Log(test);
				Debug.Log("poseTransform1");
				Debug.Log(poseTransform1);
				//These are the three steps in extend arm
				if(step == 0){//move the pose to first position, elbow bent hand bent
					//Debug.Log("step1");
					armExtention = false;
					if(test == 1){//shoulder
						holdShoulder = poseTransform1;
						y = 0;
						while(y < 1600){
							poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
							y = y + 1;
						}
						y=0;
						while(y < 1500){
							poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
							y = y + 1;
						}
						y = 0;
					}
					if(joint == poseJoints[1]){//elbow
						holdElbow = poseTransform1;
						while(y < 1000){
							poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
							y = y + 1;
						}
						holdElbow = poseTransform1;
					}


					if(joint == poseJoints[2]){//handLeft
						holdHand = poseTransform1;
						while(h < 500){
							poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
							h = h + 1;
						}
					}
					runthough = true;
					nextStep = true;	
				}


				//step two extend the arm
				if(step == 1){
					//Debug.Log("step2");
					if(runthough == false){
						//Debug.Log("step2-1");
						//Debug.Log(poseTransform1);
						if(poseTransform1 == holdElbow ){
							//Debug.Log("step2-2");
							// //Vector3 rotate2 = new Vector3 (-300f,80f,0f);
							// while(x < 651){
								//Debug.Log(x);
								poseTransform1.Rotate(Vector3.right,10 * Time.deltaTime);
								x = x + 1;
							// }
						}
					}
					if(x == 460){
					nextStep = true;
					runthough = true;
					}
				}


				if(step == 2){// extend hand foward
					//Debug.Log("step3");
					if(runthough == false){
						if(poseTransform1 == holdHand){
							// while(y < 800){
								poseTransform1.Rotate(Vector3.right,10 * Time.deltaTime);
								y = y + 1;
							//}
						}
						if(y > 370){
						runthough = true;
						nextStep = true;
						}
					}
				}
				if(nextPose == 3 || nextPose == 4){
					armExtention = false;
					lunge = true;
					if(step == 3){

					}

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

