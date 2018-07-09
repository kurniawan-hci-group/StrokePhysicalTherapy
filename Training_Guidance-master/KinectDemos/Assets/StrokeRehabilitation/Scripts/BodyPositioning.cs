using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BodyPositioning : MonoBehaviour {

    public UnityEngine.UI.Text shoulderLeftText;
    public UnityEngine.UI.Text wristLeftText;
    public UnityEngine.UI.Text handLeftText;
    public UnityEngine.UI.Text thighLeftText;
    public UnityEngine.UI.Text kneeLeftText;
    public UnityEngine.UI.Text footLeftText;
    public UnityEngine.UI.Text elbowLeftText;

    public UnityEngine.UI.Text shoulderRightText;
    public UnityEngine.UI.Text wristRightText;
    public UnityEngine.UI.Text handRightText;
    public UnityEngine.UI.Text thighRightText;
    public UnityEngine.UI.Text kneeRightText;
    public UnityEngine.UI.Text footRightText;
    public UnityEngine.UI.Text elbowRightText;


    // Use this for initialization
    void Start()
    {
        
    }
	
	// Update is called once per frame
	void Update () {
        // get the joint position
        KinectManager manager = KinectManager.Instance;
        if (manager && manager.IsInitialized())
        {
            if (manager.IsUserDetected())
            {
                long userId = manager.GetPrimaryUserID();
                KinectInterop.JointType shoulderLeft = KinectInterop.JointType.ShoulderLeft;
                KinectInterop.JointType wristLeft = KinectInterop.JointType.WristLeft;
                KinectInterop.JointType handLeft = KinectInterop.JointType.HandLeft;
                KinectInterop.JointType thighLeft = KinectInterop.JointType.HipLeft;
                KinectInterop.JointType kneeLeft = KinectInterop.JointType.KneeLeft;
                KinectInterop.JointType footLeft = KinectInterop.JointType.FootLeft;
                KinectInterop.JointType elbowLeft = KinectInterop.JointType.ElbowLeft;

                KinectInterop.JointType shoulderRight = KinectInterop.JointType.ShoulderRight;
                KinectInterop.JointType wristRight = KinectInterop.JointType.WristRight;
                KinectInterop.JointType handRight = KinectInterop.JointType.HandRight;
                KinectInterop.JointType thighRight = KinectInterop.JointType.HipRight;
                KinectInterop.JointType kneeRight = KinectInterop.JointType.KneeRight;
                KinectInterop.JointType footRight = KinectInterop.JointType.FootRight;
                KinectInterop.JointType elbowRight = KinectInterop.JointType.ElbowRight;


                shoulderLeftText.text = string.Format("{0,-12}{1,8}", "Shoulder:", manager.GetJointPosition(userId, (int)shoulderLeft).ToString());
                wristLeftText.text = string.Format("{0,-12}{1,8}", "Wrist:", manager.GetJointPosition(userId, (int)wristLeft).ToString());
                handLeftText.text = string.Format("{0,-12}{1,8}", "Hand:", manager.GetJointPosition(userId, (int)handLeft).ToString());
                thighLeftText.text = string.Format("{0,-12}{1,8}", "Hip:", manager.GetJointPosition(userId, (int)thighLeft).ToString());
                kneeLeftText.text = string.Format("{0,-12}{1,8}", "Knee:", manager.GetJointPosition(userId, (int)kneeLeft).ToString());
                footLeftText.text = string.Format("{0,-12}{1,8}", "Foot:", manager.GetJointPosition(userId, (int)footLeft).ToString());
                elbowLeftText.text = string.Format("{0,-12}{1,8}", "Elbow:", manager.GetJointPosition(userId, (int)elbowLeft).ToString());

                shoulderRightText.text = string.Format("{0,-12}{1,8}", "Shoulder:", manager.GetJointPosition(userId, (int)shoulderRight).ToString());
                wristRightText.text = string.Format("{0,-12}{1,8}", "Wrist:", manager.GetJointPosition(userId, (int)wristRight).ToString());
                handRightText.text = string.Format("{0,-12}{1,8}", "Hand:", manager.GetJointPosition(userId, (int)handRight).ToString());
                thighRightText.text = string.Format("{0,-12}{1,8}", "Hip:", manager.GetJointPosition(userId, (int)thighRight).ToString());
                kneeRightText.text = string.Format("{0,-12}{1,8}", "Knee:", manager.GetJointPosition(userId, (int)kneeRight).ToString());
                footRightText.text = string.Format("{0,-12}{1,8}", "Foot:", manager.GetJointPosition(userId, (int)footRight).ToString());
                elbowRightText.text = string.Format("{0,-12}{1,8}", "Elbow:", manager.GetJointPosition(userId, (int)elbowRight).ToString());


            }
        }

        
        

    }
}
