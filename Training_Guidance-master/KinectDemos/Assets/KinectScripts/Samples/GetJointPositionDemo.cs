using UnityEngine;
using System.Collections;
using System.IO;

public class GetJointPositionDemo : MonoBehaviour 
{
    [Tooltip("The Kinect joint we want to track.")]
    public KinectInterop.JointType joint = KinectInterop.JointType.HandRight;

	[Tooltip("Current joint position in Kinect coordinates (meters).")]
	public Vector3 jointPosition;

	[Tooltip("Path to the CSV file, we want to save the joint data to.")]
	public string saveFilePath = "E:\\TempFiles\\";
	
	[Tooltip("How many seconds to save data to the CSV file, or 0 to save non-stop.")]
	public float secondsToSave = 0f;

    // Wenchuan
    private bool flagRaiseHand;
    private int count;
    private bool start;
    private bool isSaving;
    AvatarController avatar;
    private int FrameRate;

	// start time of data saving to csv file
	private float saveStartTime = -1f;

	void Start()
	{
        avatar = GetComponent<AvatarController>();
        FrameRate = avatar.NormalFrameRate;  // according to user setting
        if (avatar.isPTRecorder)
        {
            saveFilePath = saveFilePath + "MotionData_PT.txt";
        }
        else if (avatar.isUserLearning)
        { 
            saveFilePath = saveFilePath + "MotionData_user.txt";
        }

        if (avatar.isPTRecorder || avatar.isUserLearning)
        {
            isSaving = true;
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
        }
        else
        {
            isSaving = false;
        }

        flagRaiseHand = false;
        start = false;
        count = 0;
	}

	void Update () 
	{
        if (!isSaving)
            return;

		// check the start time
		if(saveStartTime < 0f)
		{
			saveStartTime = Time.time;
		}

		// get the joint position
		KinectManager manager = KinectManager.Instance;

		if(manager && manager.IsInitialized())
		{
			if(manager.IsUserDetected())
			{
				long userId = manager.GetPrimaryUserID();
                
                if (start)  // write motion data
                {
                    for (int k = 0; k < 25; k++)
                    {
						if (manager.IsJointTracked(userId, k))  // else ????????????????
                        {
                            // output the joint position for easy tracking
                            Vector3 jointPos = manager.GetJointPosition(userId, k);
                            jointPosition = jointPos;

                            if ((secondsToSave == 0f) || ((Time.time - saveStartTime) <= secondsToSave))
                            {
                                using (StreamWriter writer = File.AppendText(saveFilePath))
                                { 
                                    string sLine = jointPos.x.ToString() + " " + jointPos.y.ToString() + " " + jointPos.z.ToString();   // Wenchuan
                                    writer.WriteLine(sLine);
                                }
                            }
                        }
                    }
                }
                else  // check whether start saving data
                {
                    KinectInterop.JointType Hand = KinectInterop.JointType.HandRight;
                    KinectInterop.JointType Head = KinectInterop.JointType.Head;
                    if (manager.IsJointTracked(userId, (int)Hand) && manager.IsJointTracked(userId, (int)Head))
                    {
                        Vector3 HandPos = manager.GetJointPosition(userId, (int)Hand);
                        Vector3 HeadPos = manager.GetJointPosition(userId, (int)Head);
                        if (HandPos.y > HeadPos.y)  // raise the right hand
                        {
                            flagRaiseHand = true;
                        }
                    }
                    if (flagRaiseHand && !start)
                    {
                        count++;
                        if (count == FrameRate * 3)  // start after 3 second
                            start = true;
                    }
                }
			}
		}
	}
}
