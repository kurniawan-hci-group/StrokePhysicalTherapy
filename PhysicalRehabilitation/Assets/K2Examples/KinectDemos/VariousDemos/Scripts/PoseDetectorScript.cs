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
	private bool runthough1 = false;
	private bool runthough2 = false;
	private bool runthough3 = false;
	private bool runthough4 = false;
	private bool runthough5 = false;
	private bool runthough6 = false;
	private bool runthough7 = false;
	private bool runthough8 = false;
	private bool armExtention = true;//change back to true
	private bool lunge = false;//change back to false
	private bool standUp = false;
	public int test = 0;
	private int step = 6;
	private int nextPose = 6;
	private int x = 0;
	private int y = 0;
	private int y1 = 0;
	private int h = 0;
	private int hL = 0;
	private int hR = 0;
	private int kL = 0;
	private int kR = 0;
	private int s = 0;
	private int eL = 0;
	private int eR = 0;
	private int sL = 0;
	private int sR = 0;



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
				kL = 0;
				kR = 0;
				s = 0;
				hL = 0;
				hR = 0;
				eL = 0;
				eR = 0;
				sL = 0;
				sR = 0;
				nextPose = nextPose + 1;
				nextStep = false;
				runthough = false;
				runthough1 = false;
				runthough2 = false;
				runthough3 = false;
				runthough4 = false;
				runthough5 = false;
				runthough6 = false;
				runthough7 = false;
				runthough8 = false;
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
			for(int c = 10; c < poseJoints.Count; c++){
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
			for(int c = 8; c < poseJoints.Count; c++){
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

				if(test < 20){ // not going to be needed anymore because we will be using the list of joints I created 
				test = test + 1;
				}

				Transform poseTransform1 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(joint, isMirrored));
				Transform poseTransform2 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(nextJoint, isMirrored));

				//These are the three steps in extend arm
				if(step == 0){//move the pose to first position, elbow bent hand bent
					Debug.Log("step1");
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

				//next exercise, sitting to standing 
				if(nextPose == 3 ){//the starting position
					armExtention = false;
					standUp = true;
					if(step == 3){
						if(joint == poseJoints[0] && runthough1 == false){//left hip
							y = 0;
							while(y < 1030){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hL < 770){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								hL = hL + 1;
							}
							y = 0;
							while(y < 1030){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hL == 769){
								runthough1 = true;
							}
						}
						if(joint == poseJoints[3] && runthough2 == false && runthough1 == true){//right hip
							y = 0;
							while(y < 940){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hR < 770){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								hR = hR + 1;
							}
							y = 0;
							while(y < 940){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hR == 769){
								runthough2 = true;
							}
						}
						if(joint == poseJoints[1] && runthough3 == false && runthough1 == true && runthough2 == true){//moving the left knee
							y = 0;
							while(y < 1000){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(kL < 580){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								kL = kL + 1;
							}
							y = 0;
							while(y < 1000){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}

							if(kL == 579){
								runthough3 = true;
								y = 0;
							}
						}
						if(joint == poseJoints[4] && runthough4 == false && runthough1 == true && runthough2 == true){//moving the right knee
							y = 0;
							while(y < 1000){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(kR < 580){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								kR = kR + 1;
							}
							y = 0;
							while(y < 1000){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}
							if(kR == 579){
								runthough4 = true;
								y = 0;
							}
						}
						if(joint == poseJoints[6] && runthough5 == false){//moving the shoulder left
							if( s < 600){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								s = s + 1;
							}
							if(s < 1300 && s > 598){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								s = s + 1;
							}
							if(s == 1299){
								runthough5 = true;
							}
						}
						if(joint == poseJoints[8] && runthough6 == false && runthough5 == true){//moving the shoulder right
							if( s < 1900 && s > 1298){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								s = s + 1;
							}
							if(s < 2700 && s > 1898){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								s = s + 1;
							}
							if(s == 2699){
								runthough6 = true;
								s = 0;
							}
						}
						if(joint == poseJoints[7] && runthough7 == false && runthough5 == true && runthough6 == true){//moving the shoulder right
							if( s < 600 ){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								s = s + 1;
							}

							if(s == 599){
								runthough7 = true;
							}
						}
					if(runthough7 == true){
						runthough = true;
						nextStep = true;	
					}
					}
				}



				if(nextPose == 4 ){//move elbows to 90 degree angles and move feet backwards 
					if(step == 4){
						if(joint == poseJoints[8] && runthough6 == false){//moving the shoulder right
							if( y < 800){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(y < 1500 && y > 798){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								y = y + 1;
							}
							if(y == 1499){
								runthough6 = true;
								y = 0;
							}
						}
						if(joint == poseJoints[6] && runthough5 == false ){//moving the shoulder left
							if( y1 < 800){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y1 = y1 + 1;
							}
							if(y1 < 1300 && y1 > 798){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								y1 = y1 + 1;
							}
							if( y1 == 1299){
								runthough5 = true;
								y1 = 0;
							}
						}
						if(joint == poseJoints[1] && runthough3 == false ){//moving the left knee
							s = 0;
							while(s < 1000){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								s = s + 1;
							}
							if(kL < 180){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								kL = kL + 1;
							}
							s = 0;
							while(s < 1000){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								s = s + 1;
							}

							if(kL == 179){
								runthough3 = true;
								s = 0;
							}
						}
						if(joint == poseJoints[4] && runthough4 == false ){//moving the right knee
							h = 0;
							while(h < 1000){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								h = h + 1;
							}
							if(kR < 180){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								kR = kR + 1;
							}
							h = 0;
							while(h < 1000){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								h = h + 1;
							}
							if(kR == 179){
								runthough4 = true;
								h = 0;
							}
						}
						if( runthough4 == true && runthough3 == true && runthough5 == true && runthough6 == true){
						runthough = true;
						nextStep = true;
						}
					}
				}


				if(nextPose == 5 ){//move pose to stand up  
					if(step == 5){
						if(joint == poseJoints[1] && runthough3 == false ){//moving the left knee
							s = 0;
							while(s < 900){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								s = s + 1;
							}
							if(kL < 800){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								kL = kL + 1;
							}
							s = 0;
							while(s < 900){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								s = s + 1;
							}

							if(kL == 879){
								runthough3 = true;
								s = 0;
							}
						}
						if(joint == poseJoints[4] && runthough4 == false ){//moving the right knee
							h = 0;
							while(h < 900){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								h = h + 1;
							}
							if(kR < 880){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								kR = kR + 1;
							}
							h = 0;
							while(h < 900){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								h = h + 1;
							}
							if(kR == 879){
								runthough4 = true;
								h = 0;
							}
						}
						if(joint == poseJoints[0] && runthough1 == false && runthough4 == true && runthough3 == true){//left hip
							y = 0;
							while(y < 960){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hL < 770){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								hL = hL + 1;
							}
							y = 0;
							while(y < 960){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hL == 769){
								runthough1 = true;
								y = 0;
							}
						}
						if(joint == poseJoints[3] && runthough2 == false && runthough4 == true && runthough3 == true){//right hip
							x = 0;
							while(x < 960){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								x = x + 1;
							}
							if(hR < 770){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								hR = hR + 1;
							}
							x = 0;
							while(x < 960){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								x = x + 1;
							}
							if(hR == 769){
								runthough2 = true;
								x = 0;
								hR = 0;
							}
						}
						if(joint == poseJoints[8] && runthough5 == false ){//right shoulder
							if(sR < 770){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								sR = sR + 1;
							}
							if( sR == 769){
								runthough5 = true;
							}
						}
						if(joint == poseJoints[6] && runthough6 == false){//left shoulder
							if(sL < 770){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								sL = sL + 1;
							}
							if(sL == 769){
								runthough6 =  true;
								sL = 0;
							}
						}
						if(joint == poseJoints[7] && runthough7 == false && runthough5 == true){//right elbow
							if(eR < 1000){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								eR = eR + 1;
							}
							if( eR == 999){
								runthough7 = true;
							}
						}
						if(joint == poseJoints[9] && runthough8 == false && runthough6 == true){//left elbow
							if(eL < 1000){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								eL = eL + 1;
							}
							if(eL == 999){
								runthough8 =  true;
							}
						}
						if( runthough4 == true && runthough3 == true && runthough7 == true && runthough8 == true){
						runthough = true;
						nextStep = true;
						}
					}
				}

				//next exercise, lunge
				if(nextPose == 6 ){//the starting position
					armExtention = false;
					standUp = false;
					lunge = true;
					if(step == 6){
						if(joint == poseJoints[0] && runthough2 == false ){//right hip
							y = 0;
							while(y < 860){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hR < 470){
								poseTransform1.Rotate(Vector3.left,5 * Time.deltaTime);
								hR = hR + 1;
							}
							y = 0;
							while(y < 860){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hR == 369){
								runthough2 = true;
							}
						}
						Debug.Log(joint);
						if(joint == poseJoints[2] && runthough1 == false && runthough2 == true){//ankle right
							y = 0;
							while(y < 660){
								poseTransform1.Rotate(Vector3.up,5 * Time.deltaTime);
								y = y + 1;
							}
							if(s < 370){
								poseTransform1.Rotate(Vector3.right,5 * Time.deltaTime);
								hR = hR + 1;
							}
							y = 0;
							while(y <660){
								poseTransform1.Rotate(Vector3.down,5 * Time.deltaTime);
								y = y + 1;
							}
							if(hR == 769){
								runthough1 = true;
							}
						}
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

