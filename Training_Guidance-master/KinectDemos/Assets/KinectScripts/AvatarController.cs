using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 


/// <summary>
/// Avatar controller is the component that transfers the captured user motion to a humanoid model (avatar).
/// </summary>
[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{
    /******************************** Options added by Wenchuan ********************************************************/
    // Fixed position means: no matter where the real user stands, the avatar will appear in "transform.position" which is set by us in the first frame
    [Tooltip("Fixed position.")]
    public bool FixedPosition;

    [Tooltip("Is the right avatar or the left one.")]
    public bool isRight = false;

    [Tooltip("Is recording for a physical therapist or a user.")]
    public bool isPTRecorder = false;

    [Tooltip("Is file controlling the avatar or the Kinect sensor.")]
    public bool isUserLearning = false;

    [Tooltip("Is showing the guidance.")]
    public bool isGuidanceShowing = false;

    [Tooltip("Whether showing visual guidance.")]
    public int VisualGuidance = 0;   // 0: no visual gudiance, 1: visual guidance

    [Tooltip("Whether showing textual guidance.")]
    public int TextualGuidance = 0;  // 0: no textual guidance, 1: simple textual guidance, 2: detailed textual guidance
    
    [Tooltip("Normal frame rate of the game.")]
    public int NormalFrameRate = 30;

    [Tooltip("Slow frame rate when showing guidance.")]
    public int SlowFrameRate = 1;

    [Tooltip("Data path.")]
    public string Path = "E:\\Training_Guidance\\data";

    [Tooltip("Name of the user.")]
    public string User = "no";
    /************************************************************************************************************/

	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Whether the avatar is facing the player or not.")]
	public bool mirroredMovement = false;
	
	[Tooltip("Whether the avatar is allowed to move vertically or not.")]
	public bool verticalMovement = false;
	
	[Tooltip("Rate at which the avatar will move through the scene.")]
	public float moveRate = 1f;
	
	[Tooltip("Smooth factor used for avatar movements and joint rotations.")]
	public float smoothFactor = 10f;
	
	[Tooltip("Game object this transform is relative to (optional).")]
	public GameObject offsetNode;

	[Tooltip("If specified, makes the initial avatar position relative to this camera, to be equal to the player's position relative to the sensor.")]
	public Camera posRelativeToCamera;

    //	[Tooltip("Whether the avatar position overlays the color camera background or not.")]
    //	protected bool avatarPosOverlaysBackground = true;

    /******************************** Variables added by Wenchuan ********************************************************/
    protected StreamReader read;   // read the control data
    protected StreamReader readGuidance; // read guidance information when showing guidace
    private StreamWriter write_motion;
    private StreamWriter write_control;

    protected bool isUsingKinect;
    protected bool Start;
    private string PTControlData;
    private string PTMotionData;
    private string UserControlData;
    private string UserMotionData;
    private string GuidanceFile;
    private bool needWrite;  // whether need to write control and motion data

    // position ("trans") of the avatar in the last frame
    // when the file ends, the avatar will stay in the final position
    private Vector3 position_last;

    // the first "trans" in the PT control file
    private Vector3 trans0;   // the first "trans" in the PT control file

    // the following is used as a trigger to start the training when user is learning
    private bool start0;  // true only when Start = true && it's the first frame
    protected bool flagRaiseHand;
    protected int count; // count number of frames since raising the hand
    private int frames;  // count number of frames since started

    // the following is used for "FixedPosition"
    private Vector3 UserOffset;
    private Vector3 Position0;    // the fixed position set by us

    // the following is used for showing guidance
    private int score;
    private bool isSlowGuidance;

    public GameObject Position1;
    public GameObject RightPosition1;
    public GameObject Arrow1;
    public GameObject Position2;
    public GameObject RightPosition2;
    public GameObject Arrow2;
    public GameObject Position3;
    public GameObject RightPosition3;
    public GameObject Arrow3;
    public GameObject Position4;
    public GameObject RightPosition4;
    public GameObject Arrow4;
    public GameObject Position5;
    public GameObject RightPosition5;
    public GameObject Arrow5;

    public GameObject GuidanceText1;
    public GameObject GuidanceText2;
    public GameObject GuidanceText3;
    public GameObject GuidanceText4;
    public GameObject GuidanceText5;

    private List<int> Type_Right_Leg = new List<int>();
    private List<int> Type_Left_Leg = new List<int>();
    private List<int> Type_Right_Arm = new List<int>();
    private List<int> Type_Left_Arm = new List<int>();
    private List<int> Type_Trunk = new List<int>();

    private List<float> Diff_Right_Leg = new List<float>();
    private List<float> Diff_Left_Leg = new List<float>();
    private List<float> Diff_Right_Arm = new List<float>();
    private List<float> Diff_Left_Arm = new List<float>();
    private List<float> Diff_Trunk = new List<float>();

    public GameObject Instruction;

    private bool Right_Leg_Straight = false;  // for guidance part
    private bool Left_Leg_Straight = false;
    private bool Right_Arm_Straight = false;
    private bool Left_Arm_Straight = false;
    /************************************************************************************************************/

    // userId of the player
    [NonSerialized]
	public Int64 playerId = 0;

	// The body root node
	protected Transform bodyRoot;

	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	protected Transform[] bones;
	
	// Rotations of the bones when the Kinect tracking starts.
	protected Quaternion[] initialRotations;

	// Initial position and rotation of the transform
	protected Vector3 initialPosition;
	protected Quaternion initialRotation;
	protected Vector3 offsetNodePos;
	protected Quaternion offsetNodeRot;
	protected Vector3 bodyRootPosition;
	
	// Calibration Offset Variables for Character Position.
	protected bool offsetCalibrated = false;
	protected float xOffset, yOffset, zOffset;
	//private Quaternion originalRotation;

	// whether the parent transform obeys physics
	protected bool isRigidBody = false;
	
	// private instance of the KinectManager
	protected KinectManager kinectManager;


	/// <summary>
	/// Gets the number of bone transforms (array length).
	/// </summary>
	/// <returns>The number of bone transforms.</returns>
	public int GetBoneTransformCount()
	{
		return bones != null ? bones.Length : 0;
	}

	/// <summary>
	/// Gets the bone transform by index.
	/// </summary>
	/// <returns>The bone transform.</returns>
	/// <param name="index">Index</param>
	public Transform GetBoneTransform(int index)
	{
		if(index >= 0 && index < bones.Length)
		{
			return bones[index];
		}

		return null;
	}

	/// <summary>
	/// Gets the bone index by joint type.
	/// </summary>
	/// <returns>The bone index.</returns>
	/// <param name="joint">Joint type</param>
	/// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
	public int GetBoneIndexByJoint(KinectInterop.JointType joint, bool bMirrored)
	{
		int boneIndex = -1;
		
		if(jointMap2boneIndex.ContainsKey(joint))
		{
			boneIndex = !bMirrored ? jointMap2boneIndex[joint] : mirrorJointMap2boneIndex[joint];
		}

        if (boneIndex == 2)  // Wenchuan: no bone transform for chest
            boneIndex = 3;  
		return boneIndex;
	}
	
	/// <summary>
	/// Gets the special index by two joint types.
	/// </summary>
	/// <returns>The spec index by joint.</returns>
	/// <param name="joint1">Joint 1 type.</param>
	/// <param name="joint2">Joint 2 type.</param>
	/// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
	public int GetSpecIndexByJoint(KinectInterop.JointType joint1, KinectInterop.JointType joint2, bool bMirrored)
	{
		int boneIndex = -1;
		
		if((joint1 == KinectInterop.JointType.ShoulderLeft && joint2 == KinectInterop.JointType.SpineShoulder) ||
		   (joint2 == KinectInterop.JointType.ShoulderLeft && joint1 == KinectInterop.JointType.SpineShoulder))
		{
			return (!bMirrored ? 25 : 26);
		}
		else if((joint1 == KinectInterop.JointType.ShoulderRight && joint2 == KinectInterop.JointType.SpineShoulder) ||
		        (joint2 == KinectInterop.JointType.ShoulderRight && joint1 == KinectInterop.JointType.SpineShoulder))
		{
			return (!bMirrored ? 26 : 25);
		}
		
		return boneIndex;
	}
	
	
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

    // added by Wenchuan: read initial rotations from the file (using the public "read" stream)
    void FileGetInitialRotations()
    {
        // read from file
        initialPosition = String2Vector3(read.ReadLine()); 
        initialRotation = String2Quaternion(read.ReadLine());
        if(isGuidanceShowing)  // don't follow the previous rotations when showing guidance
        {
            if (isRight)
            {
                initialRotation = Quaternion.Euler(0, 180, 0); // face the user
                // or new Quaternion(0, -1, 0, 0);
            }
            else
            {
                float tiltAroundY = 80; // 120;
                initialRotation = Quaternion.Euler(0, tiltAroundY, 0);
            }
        }
        offsetNodePos = String2Vector3(read.ReadLine());
        offsetNodeRot = String2Quaternion(read.ReadLine());
        bodyRootPosition = String2Vector3(read.ReadLine());
        for(int i = 0; i < bones.Length; i++)
        {
            initialRotations[i] = String2Quaternion(read.ReadLine());
        }
        return;
    }

    // added by Wenchuan
    void Preprocessing_PTRecorder()
    {
        Start = false;
        isUsingKinect = true;
        if (!isRight)  // record a PT using the left avatar (not mirrored)
        {
            mirroredMovement = false;
            transform.rotation = Quaternion.identity;  // do not rotate

            write_control = new StreamWriter(PTControlData, false);
            write_motion = new StreamWriter(PTMotionData, false);
            // false: overwrite if existed, create if not existed
            // true: append if existed, create if not existed
        }
        else  // the right avatar is mirrored
        {
            mirroredMovement = true;
            transform.rotation = new Quaternion(0, -1, 0, 0);   // rotate 180 degree (face the user)
        }
        return;
    }

    // added by Wenchuan
    void Preprocessing_UserLearning()
    {
        Start = false;
        isUsingKinect = isRight ? true : false;
        if (isRight)  // right avatar: record the user
        {
            mirroredMovement = false;
            transform.rotation = Quaternion.identity;  // do not rotate

            write_control = new StreamWriter(UserControlData, false);
            write_motion = new StreamWriter(UserMotionData, false);
            // false: overwrite if existed, create if not existed
            // true: append if existed, create if not existed
        }
        else  // left avatar: show the PT movement
        {
            mirroredMovement = false;
            transform.rotation = Quaternion.identity;  // do not rotate
            if (!File.Exists(PTControlData))
            {
                kinectManager.calibrationText.GetComponent<GUIText>().text = "PT Files missing!";
                Debug.Log("PT Files missing!");
                return;
            }
            read = new StreamReader(PTControlData, Encoding.Default);
        }
        return;
    }

    // added by Wenchuan
    void Preprocessing_GuidanceShowing()
    {
        Start = true;
        isUsingKinect = false;

        mirroredMovement = isRight;  // right avatar faces the user
        
        // Changing transform.rotation here only changes rotation in the start scene. When showing guidance, the initial rotation is read
        // from file, so rotation should be revised in FileGetInitialRotations().

        if (!File.Exists(GuidanceFile) || !File.Exists(UserControlData))
        {
            Debug.Log("Files missing!");
            kinectManager.calibrationText.GetComponent<GUIText>().text = "Files missing!";
            return;
        }
        read = new StreamReader(UserControlData, Encoding.Default);
        readGuidance = new StreamReader(GuidanceFile, Encoding.Default);
        score = Convert.ToInt16(readGuidance.ReadLine());

        // read types information in the guidance file, see attached files for explanations
        while (true)
        {
            int type = Convert.ToInt16(readGuidance.ReadLine());
            if (type == 0)  // no more guidance for this frame
                break;
 
            if(type == 5) Right_Leg_Straight = true;
            else if (type == -5) Left_Leg_Straight = true;
            else if (type == 3) Right_Arm_Straight = true;
            else if (type == -3) Left_Arm_Straight = true;
        }
        return;
    }

    // added by Wenchuan
    void WriteInitialControlData()
    {
        string sLine = initialPosition.x.ToString() + " " + initialPosition.y.ToString() + " " + initialPosition.z.ToString();
        write_control.WriteLine(sLine);
        sLine = initialRotation.w.ToString() + " " + initialRotation.x.ToString() + " " + initialRotation.y.ToString() + " " + initialRotation.z.ToString();
        write_control.WriteLine(sLine);
        sLine = offsetNodePos.x.ToString() + " " + offsetNodePos.y.ToString() + " " + offsetNodePos.z.ToString();
        write_control.WriteLine(sLine);
        sLine = offsetNodeRot.w.ToString() + " " + offsetNodeRot.x.ToString() + " " + offsetNodeRot.y.ToString() + " " + offsetNodeRot.z.ToString();
        write_control.WriteLine(sLine);
        sLine = bodyRootPosition.x.ToString() + " " + bodyRootPosition.y.ToString() + " " + bodyRootPosition.z.ToString();
        write_control.WriteLine(sLine);
        // initial rotations
        for (int i = 0; i < bones.Length; i++)
        {
            sLine = initialRotations[i].w.ToString() + " " + initialRotations[i].x.ToString() + " " + initialRotations[i].y.ToString() + " " + initialRotations[i].z.ToString();
            write_control.WriteLine(sLine);
        }
        return;
    }

    public void Awake()
    {
        // check for double start
		if(bones != null)
			return;
		if(!gameObject.activeInHierarchy) 
			return;

        // added by Wenchuan: choose only one option
        if (isUserLearning && isPTRecorder)
            return;
        if (isUserLearning && isGuidanceShowing)
            return;
        if (isPTRecorder && isGuidanceShowing)
            return;

        // Wenchuan: set path
        PTControlData = Path + "\\PT\\ControlData_PT.txt";
        PTMotionData = Path + "\\PT\\MotionData_PT.txt";
        UserControlData = Path + "\\" + User + "\\ControlData_user.txt";
        UserMotionData = Path + "\\" + User + "\\MotionData_user.txt";
        GuidanceFile = Path + "\\" + User + "\\Guidance.txt";

        // Wenchuan: set the frame rate
        QualitySettings.vSyncCount = 0;  // turn off vSync
        Application.targetFrameRate = NormalFrameRate;

        // Wenchuan: set parameters
        start0 = false;
        verticalMovement = true;
        flagRaiseHand = false;
        isSlowGuidance = false;
        if (FixedPosition)
        {
            smoothFactor = 0.0f;
            Position0 = transform.position;
        }
        count = 0;
        frames = -1;

        // Wenchuan: preprocessing 
        needWrite = ((isPTRecorder && !isRight) || (isUserLearning && isRight));
        if (isPTRecorder) Preprocessing_PTRecorder();
        else if(isUserLearning) Preprocessing_UserLearning();
        else Preprocessing_GuidanceShowing();  // isGuidanceShowing

		// Set model's arms to be in T-pose, if needed
		SetModelArmsInTpose();
		
		// inits the bones array
		bones = new Transform[27];
		
		// Initial rotations and directions of the bones.
		initialRotations = new Quaternion[bones.Length];

		// Map bones to the points the Kinect tracks
		MapBones();

        // Wenchuan: Get initial bone rotations 
        if (isUsingKinect)
        {
            GetInitialRotations();  // get data from Kinect
            if (needWrite)
                WriteInitialControlData();
        }
        else   // read initial control data from file
        {
            FileGetInitialRotations();
            // read the initial "trans0" (i.e., the first frame) in the PT control file
            trans0 = FileGetVector3();
        }

		// if parent transform uses physics
		isRigidBody = gameObject.GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Updates the avatar each frame.
	/// </summary>
	/// <param name="UserID">User ID</param>
    public void UpdateAvatar(Int64 UserID)
    {
		if(!gameObject.activeInHierarchy) 
			return;

        // Get the KinectManager instance
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        // added by Wenchuan
        if (isGuidanceShowing)
        {
            if(!isSlowGuidance)
                System.Threading.Thread.Sleep(100);
            else
                System.Threading.Thread.Sleep(1000);
        }

        /****************************** Wenchuan: move the avatar and write control data ************************************/
        MoveAvatar(UserID); 

		for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!bones[boneIndex]) 
				continue;

			if(boneIndex2JointMap.ContainsKey(boneIndex))
			{
				KinectInterop.JointType joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
				TransformBone(UserID, joint, boneIndex, !mirroredMovement);   /*************** write control data here if started **********/
			}
			else if(specIndex2JointMap.ContainsKey(boneIndex))
			{
				// special bones (clavicles)
				List<KinectInterop.JointType> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];

				if(alJoints.Count >= 2)
				{
					//Debug.Log(alJoints[0].ToString());
					Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement); /*************** write control data here if started **********/
				}
			}
		}

        /*********************************** Wenchuan: write motion data if needed ***************************************************/
        if(needWrite && Start)
        {
            long userId = kinectManager.GetPrimaryUserID();
            for (int k = 0; k < 25; k++)
            {
                Vector3 jointPos = kinectManager.GetJointPosition(userId, k);
                string sLine = jointPos.x.ToString() + " " + jointPos.y.ToString() + " " + jointPos.z.ToString();
                write_motion.WriteLine(sLine);
            }
        }

        /****************************** Wenchuan: judge whether the user raises his hand to start ******************************/
        if (!Start)
        {
            if(isUserLearning && isRight)
            {
                if (count == 0)
                {
                    kinectManager.calibrationText.GetComponent<GUIText>().text = "Raise your right hand when you are ready";
                    kinectManager.calibrationText.fontSize = 25;
                    kinectManager.calibrationText.color = Color.white;
                }
                else
                    kinectManager.calibrationText.GetComponent<GUIText>().text = " ";  // add some intervals
            }

            if (!flagRaiseHand)
            {
                flagRaiseHand = RaiseHand();  // check whether the user raises the hand
            }
            if (flagRaiseHand && !Start)
            {
                count++;
                if (count == NormalFrameRate * 2)  // start after 2 second
                {
                    Start = true;
                }
            }
        }
        else  // show the frame number after started
        {
            frames++;
            if ((isPTRecorder || isUserLearning) && isRight)
            {
                kinectManager.calibrationText.GetComponent<GUIText>().text = "Please follow the left avatar"; // frames.ToString();
                kinectManager.calibrationText.fontSize = 25;
                kinectManager.calibrationText.color = Color.white;
            }
        }

        /***************************************** Wenchuan: showing guidance *********************************************/
        if (isGuidanceShowing)
        {
            int g_guidance = 0;  // represent the guidance order, g_guidance <= 5 (showing up to 5 different kinds of guidance for each frame)
            clearGuidance();   // clear guidance for the previous frame 

            // read guidance from file
            while (true)
            {
                int type = Convert.ToInt16(readGuidance.ReadLine());  // type and diff
                if (type == 0)  // no more guidance for this frame
                    break;
                float diff = (float)Convert.ToDouble(readGuidance.ReadLine());  // diff is patient's error, diff = PT - user 

                // show textual guidance
                if (TextualGuidance != 0 && isRight)
                {
                    Vector3 guidance_position = Vector3.zero;
                    Vector3 offset = Vector3.zero;
                    String guidance = TextualGuidancePosition(type, diff, ref guidance_position, ref offset, TextualGuidance); // mt is the relative position of the text message
                    ShowTextualGuidance(guidance, guidance_position, offset, g_guidance);
                    g_guidance++;
                }

                // store guidance in different body parts, used for visual guidance
                storeGuidance(type, diff);
            }

            // show visual guidance
            string GeneralInstruction = "Your score: " + score.ToString() + "; ";
            if (VisualGuidance == 1)
            {
                // draw the user (wrong) body vector in red first
                DrawOriginalBody(Type_Right_Arm, Type_Left_Arm, Type_Right_Leg, Type_Left_Leg, Type_Trunk);

                // draw right position (in green) and arrow (in yellow)
                DrawRightPosition(Type_Right_Arm, Type_Left_Arm, Type_Right_Leg, Type_Left_Leg, Type_Trunk, Diff_Right_Arm, Diff_Left_Arm, Diff_Right_Leg, Diff_Left_Leg, Diff_Trunk);

                // general instructions
                GeneralInstruction = GeneralInstruction + "Your position: red; Corrected position: green\n";
            }

            // show general instructions in the corner
            kinectManager.calibrationText.GetComponent<GUIText>().text = GeneralInstruction;
            kinectManager.calibrationText.fontSize = 25;
            kinectManager.calibrationText.color = Color.white;

            // adjust the frame rate
            if (Type_Right_Arm.Count + Type_Left_Arm.Count + Type_Right_Leg.Count + Type_Left_Leg.Count + Type_Trunk.Count == 0)  // no guidance
            {
                Application.targetFrameRate = NormalFrameRate;
                isSlowGuidance = false;
            }
            else
            {
                Application.targetFrameRate = SlowFrameRate;
                isSlowGuidance = true;
            }
        }
	}

    // Wenchuan: whether the user raises his hand in this frame
    bool RaiseHand()
    {
        KinectInterop.JointType Hand = KinectInterop.JointType.HandRight;
        KinectInterop.JointType Head = KinectInterop.JointType.Head;
        long userId = kinectManager.GetPrimaryUserID();
        if (kinectManager.IsJointTracked(userId, (int)Hand) && kinectManager.IsJointTracked(userId, (int)Head))
        {
            Vector3 HandPos = kinectManager.GetJointPosition(userId, (int)Hand);
            Vector3 HeadPos = kinectManager.GetJointPosition(userId, (int)Head);
            return (HandPos.y > HeadPos.y);
        }
        return false;
    }

    // Wenchuan: clear the guidance of the previous frame
    void clearGuidance()
    {
        kinectManager.calibrationText.GetComponent<GUIText>().text = " ";
        Plot(0, Vector3.zero, Vector3.zero, Vector3.zero, 0.0f);
        Plot(1, Vector3.zero, Vector3.zero, Vector3.zero, 0.0f);
        Plot(2, Vector3.zero, Vector3.zero, Vector3.zero, 0.0f);
        Plot(3, Vector3.zero, Vector3.zero, Vector3.zero, 0.0f);
        Plot(4, Vector3.zero, Vector3.zero, Vector3.zero, 0.0f);

        ShowTextualGuidance("", Vector3.zero, Vector3.zero, 0);
        ShowTextualGuidance("", Vector3.zero, Vector3.zero, 1);
        ShowTextualGuidance("", Vector3.zero, Vector3.zero, 2);
        ShowTextualGuidance("", Vector3.zero, Vector3.zero, 3);
        ShowTextualGuidance("", Vector3.zero, Vector3.zero, 4);

        Type_Right_Leg.Clear();
        Type_Left_Leg.Clear();
        Type_Right_Arm.Clear();
        Type_Left_Arm.Clear();
        Type_Trunk.Clear();

        Diff_Right_Leg.Clear();
        Diff_Left_Leg.Clear();
        Diff_Right_Arm.Clear();
        Diff_Left_Arm.Clear();
        Diff_Trunk.Clear();

        return;
    }

    // Wenchuan: store guidance in different body parts, used for visual guidance
    void storeGuidance(int type, float diff)
    {
        // insert in differnt body part list
        if (type == 1 || type == 2 || type == 3 || type == 8 || type == 9)
        {
            Type_Right_Arm.Add(type);
            Diff_Right_Arm.Add(diff);
        }
        else if (type == -1 || type == -2 || type == -3 || type == -8 || type == -9)
        {
            Type_Left_Arm.Add(type);
            Diff_Left_Arm.Add(diff);
        }
        else if (type == 4 || type == 5 || type == 6 || type == 7)
        {
            Type_Right_Leg.Add(type);
            Diff_Right_Leg.Add(diff);
        }
        else if (type == -4 || type == -5 || type == -6 || type == -7)
        {
            Type_Left_Leg.Add(type);
            Diff_Left_Leg.Add(diff);
        }
        else if (type == 11)
        {
            Type_Trunk.Add(type);
            Diff_Trunk.Add(diff);
        }
        return;
    }

    // Wenchuan: move the message by offset
    void ShowTextualGuidance(string guidance, Vector3 position, Vector3 offset, int g)
    {
        ShowGuidance text;
        if (g == 0)
        {
            text = GuidanceText1.GetComponent<ShowGuidance>();
        }
        else if(g == 1)
        {
            text = GuidanceText2.GetComponent<ShowGuidance>();
        }
        else if (g == 2)
        {
            text = GuidanceText3.GetComponent<ShowGuidance>();
        }
        else if (g == 3)
        {
            text = GuidanceText4.GetComponent<ShowGuidance>();
        }
        else  // g == 4
        {
            text = GuidanceText5.GetComponent<ShowGuidance>();
        }

        text.position = position;
        text.guidance = guidance;
        text.offset = offset;
        return;
    }

    // Wenchuan: user original body position
    void DrawOriginalBody(List<int> Type_Right_Arm, List<int> Type_Left_Arm, List<int> Type_Right_Leg, List<int> Type_Left_Leg, List<int> Type_Trunk)
    {
        int g = 0;
        if (Type_Right_Arm.Count > 0)
        {
            Vector3 start = GetBonePosition(8);
            Vector3 middle = GetBonePosition(9);
            Vector3 endpoint = GetBonePosition(10);
            PlotBone(g, start, middle, 0, 0.05f); g++;
            PlotBone(g, middle, endpoint, 0, 0.05f); g++;
        }
        if (Type_Left_Arm.Count > 0)
        {
            Vector3 start = GetBonePosition(4);
            Vector3 middle = GetBonePosition(5);
            Vector3 endpoint = GetBonePosition(6);
            PlotBone(g, start, middle, 0, 0.05f); g++;
            PlotBone(g, middle, endpoint, 0, 0.05f); g++;
        }
        if (Type_Right_Leg.Count > 0)
        {
            Vector3 start = GetBonePosition(16);
            Vector3 middle = GetBonePosition(17);
            Vector3 endpoint = GetBonePosition(18);
            PlotBone(g, start, middle, 0, 0.05f);  g++;
            PlotBone(g, middle, endpoint, 0, 0.05f); g++;
        }
        if (Type_Left_Leg.Count > 0)
        {
            Vector3 start = GetBonePosition(12);
            Vector3 middle = GetBonePosition(13);
            Vector3 endpoint = GetBonePosition(14);
            PlotBone(g, start, middle, 0, 0.05f); g++;
            PlotBone(g, middle, endpoint, 0, 0.05f); g++;
        }
        if (Type_Trunk.Count > 0)
        {
            Vector3 start = GetBonePosition(0);
            Vector3 endpoint = GetBonePosition(20);
            PlotBone(g, start, endpoint, 0, 0.1f); g++;
        }
        return;
    }

    // Wenchuan: return the position of a bone
    Vector3 GetBonePosition(int Index)  // joint index
    {
        int BoneIndex = GetBoneIndexByJoint(jointIndex2JointMap[Index], mirroredMovement);
        Vector3 position = GetBoneTransform(BoneIndex).position;
        return position;
    }

    // Wenchuan: plot a bone
    void PlotBone(int g, Vector3 start, Vector3 endpoint, int color, float BoneWidth)  // color = 0 (original, red), color = 1 (green)
    {
        DrawCylinder line;
        if (g == 0)
        {
            line = (color == 0) ? Position1.GetComponent<DrawCylinder>() : RightPosition1.GetComponent<DrawCylinder>();
        }
        else if (g == 1)
        {
            line = (color == 0) ? Position2.GetComponent<DrawCylinder>() : RightPosition2.GetComponent<DrawCylinder>();
        }
        else if (g == 2)
        {
            line = (color == 0) ? Position3.GetComponent<DrawCylinder>() : RightPosition3.GetComponent<DrawCylinder>();
        }
        else if (g == 3)
        {
            line = (color == 0) ? Position4.GetComponent<DrawCylinder>() : RightPosition4.GetComponent<DrawCylinder>();
        }
        else  // g == 4
        {
            line = (color == 0) ? Position5.GetComponent<DrawCylinder>() : RightPosition5.GetComponent<DrawCylinder>();
        }

        line.start = start;
        line.endpoint = endpoint;
        line.width = BoneWidth;

        return;
    }

    // Wenchuan: draw the correct position of a bone
    void DrawRightPosition(List<int> Type_Right_Arm, List<int> Type_Left_Arm, List<int> Type_Right_Leg, List<int> Type_Left_Leg, List<int> Type_Trunk, List<float> Diff_Right_Arm, List<float> Diff_Left_Arm, List<float> Diff_Right_Leg, List<float> Diff_Left_Leg, List<float> Diff_Trunk)
    {
        int g = 0;
		int g_arrow = 0;
        if (Type_Right_Arm.Count > 0)
        {
            Vector3 start = GetBonePosition(8);  // original position
            Vector3 middle = GetBonePosition(9);
            Vector3 endpoint = GetBonePosition(10);

			Vector3 middle_last = middle;
			Vector3 endpoint_last = endpoint;

            AdjustPosition(Type_Right_Arm, Diff_Right_Arm, ref start, ref middle, ref endpoint, Right_Arm_Straight);

            PlotBone(g, start, middle, 1, 0.05f); g++;
            PlotBone(g, middle, endpoint, 1, 0.05f); g++;

			if (middle_last != middle) 
			{
				PlotArrow (middle_last, middle, g_arrow); g_arrow++;
			}
			if (endpoint_last != endpoint) 
			{
				PlotArrow (endpoint_last, endpoint, g_arrow); g_arrow++;
			}
  
        }
        if (Type_Left_Arm.Count > 0)
        {
            Vector3 start = GetBonePosition(4);  // original position
            Vector3 middle = GetBonePosition(5);
            Vector3 endpoint = GetBonePosition(6);

			Vector3 middle_last = middle;
			Vector3 endpoint_last = endpoint;

            AdjustPosition(Type_Left_Arm, Diff_Left_Arm, ref start, ref middle, ref endpoint, Left_Arm_Straight);

            PlotBone(g, start, middle, 1, 0.05f); g++;
            PlotBone(g, middle, endpoint, 1, 0.05f); g++;

			if (middle_last != middle) 
			{
				PlotArrow (middle_last, middle, g_arrow); g_arrow++;
			}
			if (endpoint_last != endpoint) 
			{
				PlotArrow (endpoint_last, endpoint, g_arrow); g_arrow++;
			}
        }
        if (Type_Right_Leg.Count > 0)
        {
            Vector3 start = GetBonePosition(16);  // original position
            Vector3 middle = GetBonePosition(17);
            Vector3 endpoint = GetBonePosition(18);

			Vector3 middle_last = middle;
			Vector3 endpoint_last = endpoint;

            AdjustPosition(Type_Right_Leg, Diff_Right_Leg, ref start, ref middle, ref endpoint, Right_Leg_Straight);
			
            PlotBone(g, start, middle, 1, 0.05f); g++;
            PlotBone(g, middle, endpoint, 1, 0.05f); g++;

			if (middle_last != middle) 
			{
				PlotArrow (middle_last, middle, g_arrow); g_arrow++;
			}
			if (endpoint_last != endpoint) 
			{
				PlotArrow (endpoint_last, endpoint, g_arrow); g_arrow++;
			}

        }
        if (Type_Left_Leg.Count > 0)
        {
            Vector3 start = GetBonePosition(12);  // original position
            Vector3 middle = GetBonePosition(13);
            Vector3 endpoint = GetBonePosition(14);

			Vector3 middle_last = middle;
			Vector3 endpoint_last = endpoint;

            AdjustPosition(Type_Left_Leg, Diff_Left_Leg, ref start, ref middle, ref endpoint, Left_Leg_Straight);

            PlotBone(g, start, middle, 1, 0.05f); g++;
            PlotBone(g, middle, endpoint, 1, 0.05f); g++;

			if (middle_last != middle) 
			{
				PlotArrow (middle_last, middle, g_arrow); g_arrow++;
			}
			if (endpoint_last != endpoint) 
			{
				PlotArrow (endpoint_last, endpoint, g_arrow); g_arrow++;
			}
        }
        if (Type_Trunk.Count > 0)
        {
            Vector3 start = GetBonePosition(0);  // original position
            Vector3 middle = Vector3.zero;
            Vector3 endpoint = GetBonePosition(20);

			Vector3 endpoint_last = endpoint;

            AdjustPosition(Type_Trunk, Diff_Trunk, ref start, ref middle, ref endpoint, false);
            PlotBone(g, start, endpoint, 1, 0.1f); g++;

			if (endpoint_last != endpoint) 
			{
				PlotArrow (endpoint_last, endpoint, g_arrow); g_arrow++;
			}
        }
        return;
    }

    // Wenchuan
    void AdjustPosition(List<int> types, List<float> diffs, ref Vector3 start, ref Vector3 middle, ref Vector3 endpoint, bool flag_straight)
    {
        for (int k = 0; k < types.Count; k++)
        {
            int type = types[k];
            float diff = diffs[k];
            int m = TypeMode(type); // m = 0 (start & end), m = 1 (start & middle), m = 2(middle & end), m = 3 (straight)
            if(m == 3)
            {
                continue;
            }
            int t = 0;
            Vector3 direction = GetDirection(type, ref t);

            // update start, middle, endpoint in each loop
            ComputeRightPosition(type, diff, direction, t, m, ref start, ref middle, ref endpoint);
        }

        if(flag_straight == true)
        {
            middle = 0.5f * (start + endpoint);
        }
        return;
    }

    // Wenchuan
	void PlotArrow (Vector3 start, Vector3 endpoint, int g)
	{
		DrawArrow arrow;
		if(g == 0)
		{
			arrow = Arrow1.GetComponent<DrawArrow>();  
		}
		else if(g == 1)
		{
			arrow = Arrow2.GetComponent<DrawArrow>();
		}
		else if (g == 2)
		{
			arrow = Arrow3.GetComponent<DrawArrow>(); 
		}
		else if (g == 3)
		{
			arrow = Arrow4.GetComponent<DrawArrow>(); 
		}
		else  // g == 4
		{ 
			arrow = Arrow5.GetComponent<DrawArrow>();
		}

		arrow.start = start;
		arrow.endpoint = endpoint;
        
		return;
	}

    // Wenchuan
    int TypeMode(int type)  // return  m = 0 (start & end), m = 1 (start & middle), m = 2 (middle & end), m = 3 (straight)
    {
        if (Math.Abs(type) == 1 || Math.Abs(type) == 6 || Math.Abs(type) == 7 || Math.Abs(type) == 8 || Math.Abs(type) == 9 || Math.Abs(type) == 11)
        {
            return 0;
        }
        else if(Math.Abs(type) == 2 || Math.Abs(type) == 4)
        {
            return 2;
        }
        else if (Math.Abs(type) == 3 || Math.Abs(type) == 5)
        {
            return 3;
        }
        else
        {
            return 1;
        }
    }
    
    // Wenchuan
    void ComputeRightPosition(int type, float diff, Vector3 direction, int t, int m, ref Vector3 a, ref Vector3 b, ref Vector3 c) // compute the right position
    {
        Vector3 start;
        Vector3 endpoint;
        if(m == 0)  // start, end
        {
            start = a;
            endpoint = c;
        }
        else if(m == 1)  // start, middle
        {
            start = a;
            endpoint = b;
        }
        else   // m == 2, middle, end, cannot be 3
        {
            start = b;
            endpoint = c;
        }
    
        Vector3 thisbone = endpoint - start;

        // rotate the bone according to "direction"
        Vector3 newbone;
        if (t == 0) // "direction" is direction
        {
            Vector3 axis = Vector3.Cross(thisbone, direction);
            newbone = Rotation(thisbone, axis, diff);
        }
        else  // type == 7, -7, 8, -8
            // "direction" is axis, rotate the projection on the ground
        {
            // this bone(thisbone) -> projection(a1) -> rotate projection (b1) -> new bone
            Vector3 a1 = new Vector3(thisbone.x, 0f, thisbone.z);
            Vector3 b1 = Rotation(a1, direction, diff);
            newbone = new Vector3(b1.x, thisbone.y, b1.z);
        }

        // reverse the y-axis and z-axis of the change
        if (mirroredMovement)  
        {
            Vector3 change = newbone - thisbone;
            Vector3 MirroredChange = new Vector3(-change.x, change.y, -change.z);
            newbone = thisbone + MirroredChange;
        }

        Vector3 endpoint1 = newbone + start;

        if (m == 0)  // start, end
        {
            c = endpoint1;
        }
        else if (m == 1)  // start, middle
        {
            b = endpoint1;
        }
        else   // m == 2, middle, end, cannot be 3
        {
            c = endpoint1;
        }

        return;
    }

    // Wenchuan
    void Type2Index(int type, ref int startIndex, ref int endIndex)
    {
        if (type == 1)   // RightShoulderAngle
        {
            startIndex = 8;
            endIndex = 9;
        }
        else if (type == 2 || type == 3)  // RightElbowAngle
        {
            startIndex = 9;
            endIndex = 10;
        }
        else if (type == 4 || type == 5) // RightKneeAngle
        {
            startIndex = 17;
            endIndex = 18;

        }
        else if (type == 6) //RightLegAngle (height)
        {
            startIndex = 16;
            endIndex = 17;
        }
        else if (type == 7)  //RightLegDirection
        {
            startIndex = 16;
            endIndex = 18;
        }
        else if (type == 8 || type == 9) //RightArmDirection
        {
            startIndex = 8;
            endIndex = 10;
        }
        //left
        else if (type == -1)
        {
            startIndex = 4;
            endIndex = 5;
        }
        else if (type == -2 || type == -3)
        {
            startIndex = 5;
            endIndex = 6;
        }
        else if (type == -4 || type == -5)
        {
            startIndex = 13;
            endIndex = 14;
        }
        else if (type == -6) //LeftLegAngle
        {
            startIndex = 12;
            endIndex = 13;
        }
        else if (type == -7)  //LeftLegDirection
        {
            startIndex = 12;
            endIndex = 14;
        }
        else if (type == -8 || type == -9) //LeftArmDirection
        {
            startIndex = 4; 
            endIndex = 6;
        }
        
        // middle
        else if(type == 10)
        {

        }
        else if (type == 11)  // trunk angle
        {
            startIndex = 0;
            endIndex = 20;
        }
        return;
    }
    
    // Wenchuan
    Vector3 GetDirection(int type, ref int t) // t == 0: return direction, t == 1: return axis
    {
        if (type == 1)  // RightShoulderAngle
        {
            t = 0;
            return new Vector3(0, 1, 0);
        }
        else if (type == 2 || type == 3)  // RightElbowAngle
        {
            t = 0;
            return GetBoneVector(8, 9);
        }
        else if (type == 4 || type == 5) // RightKneeAngle
        {
            t = 0;
            return GetBoneVector(16, 17);
        }
        else if (type == 6) //RightLegAngle
        {
            t = 0;
            return new Vector3(0, 1, 0);
        }
        else if (type == 7)  //RightLegDirection
        {
            t = 1;
            return new Vector3(0, 1, 0);
        }
        else if (type == 8 || type == 9) //RightArmDirection
        {
            t = 1;
            return new Vector3(0, 1, 0);
        }
        //left
        else if (type == -1)
        {
            t = 0;
            return new Vector3(0, 1, 0);
        }
        else if (type == -2 || type == -3)
        {
            t = 0;
            return GetBoneVector(4, 5);
        }
        else if (type == -4 || type == -5)
        {
            t = 0;
            return GetBoneVector(12, 13);
        }
        else if (type == -6) //LeftLegAngle
        {
            t = 0;
            return new Vector3(0, 1, 0);
        }
        else if (type == -7)  //LeftLegDirection
        {
            t = 1;
            return new Vector3(0, -1, 0);
        }
        else if (type == -8 || type == -9) //LeftArmDirection
        {
            t = 1;
            return new Vector3(0, -1, 0); 
        }
        // middle
        else if (type == 11)  // trunk angle
        {
            t = 0;
            return new Vector3(0, 1, 0); 
        }

        t = 0;
        return Vector3.zero;
    }

    // Wenchuan
    Vector3 GetBoneVector(int startIndex, int endIndex)  // joint index
    {
        int startBoneIndex = GetBoneIndexByJoint(jointIndex2JointMap[startIndex], mirroredMovement);
        int endBoneIndex = GetBoneIndexByJoint(jointIndex2JointMap[endIndex], mirroredMovement);
        Vector3 start = GetBoneTransform(startBoneIndex).position;
        Vector3 endpoint = GetBoneTransform(endBoneIndex).position;
        Vector3 bone = endpoint - start;
        return bone;
    }

    // Wenchuan
    Vector3 Rotation(Vector3 a, Vector3 axis, float angle)  // rotate a around axis for an angle
    {
        // Unity is left-hand
        angle = -angle;

        Vector3 norm = axis.normalized;

        float x = norm.x;
        float y = norm.y;
        float z = norm.z;

        float cos = Mathf.Cos(angle * Mathf.PI / 180);
        float sin = Mathf.Sin(angle * Mathf.PI / 180);

        float x2 = (float)Math.Pow(x, 2);
        float y2 = (float)Math.Pow(y, 2);
        float z2 = (float)Math.Pow(z, 2);

        Vector3 RotationMatrix1 = new Vector3(x2 + (1 - x2) * cos, x * y * (1 - cos) - z * sin, x * z * (1 - cos) + y * sin);
        Vector3 RotationMatrix2 = new Vector3(y * x * (1 - cos) + z * sin, y2 + (1 - y2) * cos, y * z * (1 - cos) - x * sin);
        Vector3 RotationMatrix3 = new Vector3(z * x * (1 - cos) - y * sin, z * y * (1 - cos) + x * sin, z2 + (1 - z2) * cos);

        float x1 = Vector3.Dot(RotationMatrix1, a);
        float y1 = Vector3.Dot(RotationMatrix2, a);
        float z1 = Vector3.Dot(RotationMatrix3, a);

        Vector3 b = new Vector3(x1, y1, z1);
        return b;
    }

    // Wenchuan
    float BoneCheck(int start, int endpoint)  // return the width of line
    {
        if(start == 3 && (endpoint == 7 || endpoint == 11))
        {
            return 0.0f;  // not exist
        }

        if(start == 0 && endpoint == 20)  // trunk
        {
            return 0.2f;
        }

        return 0.03f;  // normal width
    }

    // Wenchuan
    void Plot(int g, Vector3 start, Vector3 endpoint, Vector3 endpoint1, float BoneWidth)
    {
        DrawCylinder line;
        DrawCylinder lineRight;
        DrawArrow arrow;
        if(g == 0)
        {
            line = Position1.GetComponent<DrawCylinder>(); 
            lineRight = RightPosition1.GetComponent<DrawCylinder>(); 
            arrow = Arrow1.GetComponent<DrawArrow>();  
        }
        else if(g == 1)
        {
            line = Position2.GetComponent<DrawCylinder>(); 
            lineRight = RightPosition2.GetComponent<DrawCylinder>(); 
            arrow = Arrow2.GetComponent<DrawArrow>();
        }
        else if (g == 2)
        {
            line = Position3.GetComponent<DrawCylinder>(); 
            lineRight = RightPosition3.GetComponent<DrawCylinder>(); 
            arrow = Arrow3.GetComponent<DrawArrow>(); 
        }
        else if (g == 3)
        {
            line = Position4.GetComponent<DrawCylinder>(); 
            lineRight = RightPosition4.GetComponent<DrawCylinder>(); 
            arrow = Arrow4.GetComponent<DrawArrow>(); 
        }
        else  // g == 4
        {
            line = Position5.GetComponent<DrawCylinder>(); 
            lineRight = RightPosition5.GetComponent<DrawCylinder>(); 
            arrow = Arrow5.GetComponent<DrawArrow>();
        }

        // line: the user's body position
        line.start = start;
        line.endpoint = endpoint;
        line.width = BoneWidth;

        // lineRight: the right position
        lineRight.start = start;
        lineRight.endpoint = endpoint1;
        lineRight.width = BoneWidth;

        // draw an arrow pointing from the user position to the right position
        arrow.start = endpoint;
        arrow.endpoint = endpoint1;

        return;
    }

    // Wenchuan
    String TextualGuidancePosition(int type, float diff, ref Vector3 position, ref Vector3 m, int t)  // t = 1: simple, t = 2: detailed
    // return the guidance message
    // position: the bone position
    // m: offset about where to put the message
    {
        float up = 0.15f;
        float right = 0.3f;
        float down = -0.15f;
        float left = -1.5f;

        string message;

        if (type == 1)  // RightShoulderAngle
        {
            position = GetBonePosition(10);
            m = new Vector3(right, up, 0);  // move right up
            if(diff < 0)  // diff = PT - user < 0 < threshold
            {
                message = "Bring your right arm lower";
            }
            else
            {
                message = "Bring your right arm higher";
            }

            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;
        }
        else if (type == 2)  // RightElbowAngle (general)
        {
            position = GetBonePosition(9);
            Vector3 arm = GetBonePosition(9) - GetBonePosition(8);
            if (arm.y > 0)  // raise the arm
            {
                m = new Vector3(right, down, 0);
            }
            else
            {
                m = new Vector3(right, up, 0);
            }

            if (diff > 0)
            {
                message = "Extend your right elbow";
            }
            else
            {
                message = "Decrease your right elbow angle";
            }
            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;

        }
        else if (type == 3)  // elbow should keep straight, diff > 0
        {
            position = GetBonePosition(9);
            Vector3 arm = GetBonePosition(9) - GetBonePosition(8);
            if (arm.y > 0)  // raise the arm
            {
                m = new Vector3(right, down, 0);
            }
            else
            {
                m = new Vector3(right, up, 0);
            }

            return "Keep your right elbow straight";
        }
        else if (type == 4) // RightKneeAngle (general)
        {
            position = GetBonePosition(17);
            m = new Vector3(right, up, 0);

            if (diff > 0)
            {
                message = "Extend your right knee";
            }
            else
            {
                message = "Decrease your right knee angle";
            }
            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;
        }
        else if (type == 5)  // RightKneeAngle, ideal (PT): 180
        {
            position = GetBonePosition(17);
            m = new Vector3(right, up, 0);  // move right
            return "Keep your right knee straight";
        }
        else if (type == 6) //RightLegAngle (height)
        {
            position = GetBonePosition(18);
            m = new Vector3(right, up, 0);  // move right up

            if (diff < 0)
            {
                message = "Bring your right leg lower";
            }
            else
            {
                message = "Bring your right leg higher";
            }

            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;
        }
        else if (type == 7)  //RightLegDirection (front)
        {
            position = GetBonePosition(18);
            m = new Vector3(right, down, 0);  // move right down
            if (t == 1)
            {
                return "Keep your right leg in front of you";
            }
            else // detailed
            {
                if (diff < 0)
                    message = "Bring your right leg more right by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                else
                    message = "Bring your right leg more left by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }
        else if (type == 8) //RightArmDirection (front)
        {
            position = GetBonePosition(10);
            m = new Vector3(right, down, 0);  // move right down

            if (t == 1)
            {
                return "Keep your right arm in front of you";
            }
            else // detailed
            {
                if (diff < 0)
                    message = "Bring your right arm more right by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                else
                    message = "Bring your right arm more left by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }
        else if (type == 9) // RightArmDirection (side)
        {
            position = GetBonePosition(10);
            m = new Vector3(right, down, 0);  // move right down

            if (t == 1)
            {
                return "Keep right arm aligned with your trunk";
            }
            else // detailed
            {
                if (diff < 0)
                    message = "Bring your right arm more backward by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                else
                    message = "Bring your right arm more forward by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }

        //left
        else if (type == -1)  // LeftShouderAngle
        {
            position = GetBonePosition(6);
            m = new Vector3(left, up, 0);  // move left up

            if (diff < 0)  // diff = PT - user < 0 < threshold
            {
                message = "Bring your left arm lower";
            }
            else
            {
                message = "Bring your left arm higher";
            }

            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;
        }
        else if (type == -2)   // RightElbowAngle (general)
        {
            position = GetBonePosition(5);
            Vector3 arm = GetBonePosition(5) - GetBonePosition(4);
            if (arm.y > 0)  // raise the arm
            {
                m = new Vector3(left, down, 0);
            }
            else
            {
                m = new Vector3(left, up, 0);
            }

            if (diff > 0)
            {
                message = "Extend your left elbow";
            }
            else
            {
                message = "Decrease your left elbow angle";
            }
            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;

        }
        else if (type == -3)   // elbow straight
        {
            position = GetBonePosition(5);
            Vector3 arm = GetBonePosition(5) - GetBonePosition(4);
            if (arm.y > 0)  // raise the arm
            {
                m = new Vector3(left, down, 0);
            }
            else
            {
                m = new Vector3(left, up, 0);
            }
            return "Keep your left elbow straight";
        }
        else if (type == -4)   // LeftKneeAngle (general)
        {
            position = GetBonePosition(13);
            m = new Vector3(left, up, 0);

            if (diff > 0)
            {
                message = "Extend your left knee";
            }
            else
            {
                message = "Decrease your left knee angle";
            }
            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;
        }
        else if (type == -5)  // left knee straight
        {
            position = GetBonePosition(13);
            m = new Vector3(left, up, 0);
            return "Keep your left knee straight";
        }
        else if (type == -6) //LeftLegAngle (height)
        {
            position = GetBonePosition(14);
            m = new Vector3(left, up, 0);  // move left up

            if (diff < 0)
            {
                message = "Bring your left leg lower";
            }
            else
            {
                message = "Bring your left leg higher";
            }

            if (t == 2) // detailed
            {
                message = message + " by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
            }
            return message;
        }
        else if (type == -7)  //LeftLegDirection (front)
        {
            position = GetBonePosition(14);
            m = new Vector3(left, down, 0);  // move left down

            if (t == 1)
            {
                return "Keep your left leg in front of you";
            }
            else // detailed
            {
                if (diff < 0)
                    message = "Bring your left leg more left by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                else
                    message = "Bring your left leg more right by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }
        else if (type == -8) //LeftArmDirection (front)
        {
            position = GetBonePosition(6);
            m = new Vector3(left, down, 0);  // move left down

            if (t == 1)
            {
                return "Keep your left arm in front of you";
            }
            else // detailed
            {
                if (diff < 0)
                    message = "Bring your left arm more left by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                else
                    message = "Bring your left arm more right by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }
        else if (type == -9) // LeftArmDirection (side)
        {
            position = GetBonePosition(6);
            m = new Vector3(left, down, 0);  // move left down

            if (t == 1)
            {
                return "Keep left arm aligned with your trunk";
            }
            else // detailed
            {
                if (diff < 0)
                    message = "Bring your left arm more backward by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                else
                    message = "Bring your left arm more forward by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }

        // middle
        else if (type == 11)  // trunk angle
        {
            position = GetBonePosition(0);
            m = new Vector3(left, 0, 0);  // move left

            if(t == 1)
            {
                return "Keep your trunk upright";
            }
            else  // detailed
            {
                message = "Bring your trunk more upright by " + Mathf.Abs(Mathf.Round(diff)).ToString() + " degrees";
                return message;
            }
        }

        return "";
    }

    /// <summary>
	/// Resets bones to their initial positions and rotations.
	/// </summary>
	public void ResetToInitialPosition()
	{
		playerId = 0;

		if(bones == null)
			return;
		
		// For each bone that was defined, reset to initial position.
		transform.rotation = Quaternion.identity;

		for(int pass = 0; pass < 2; pass++)  // 2 passes because clavicles are at the end
		{
			for(int i = 0; i < bones.Length; i++)
			{
				if(bones[i] != null)
				{
					bones[i].rotation = initialRotations[i];
				}
			}
		}

//		if(bodyRoot != null)
//		{
//			bodyRoot.localPosition = Vector3.zero;
//			bodyRoot.localRotation = Quaternion.identity;
//		}

		// Restore the offset's position and rotation
		if(offsetNode != null)
		{
			offsetNode.transform.position = offsetNodePos;
			offsetNode.transform.rotation = offsetNodeRot;
		}

		transform.position = initialPosition;
		transform.rotation = initialRotation;
    }
	
	/// <summary>
	/// Invoked on the successful calibration of the player.
	/// </summary>
	/// <param name="userId">User identifier.</param>
	public void SuccessfulCalibration(Int64 userId)
	{
		playerId = userId;

		// reset the models position
		if(offsetNode != null)
		{
			offsetNode.transform.position = offsetNodePos;
			offsetNode.transform.rotation = offsetNodeRot;
		}

		transform.position = initialPosition;
		transform.rotation = initialRotation;

		// re-calibrate the position offset
		offsetCalibrated = false;
	}

    // Wenchuan: convert string to quaternion
    Quaternion String2Quaternion(string line)
    {
        double[] doubles = Array.ConvertAll<String, double>(line.Split(' '), Double.Parse);
        if (doubles.GetLength(0) != 4)
            return Quaternion.identity;

        Quaternion orientation = new Quaternion((float)doubles[1], (float)doubles[2], (float)doubles[3], (float)doubles[0]);
        // IMPORTANT: cannot update "orientation.w" "orientation.x" "orientation.y" "orientation.z" seprately!!!

        return orientation;
    }

    // Wenchuan: read Quaternion from a file (using the public "read" stream)
    public Quaternion FileGetQuaternion()
    {
        String line;
        line = read.ReadLine();
        if(line != null)
        {
            return String2Quaternion(line);
        }

        return Quaternion.identity;
    }

	// Apply the rotations tracked by kinect to the joints.
	protected void TransformBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)  // flip = !mirroredMovement
    {
        Transform boneTransform = bones[boneIndex];
		if(boneTransform == null)
			return;

        if (isUsingKinect && kinectManager == null)   // Wenchuan
            return;
		
		int iJoint = (int)joint;
		if(iJoint < 0)
			return;
        if (isUsingKinect && !kinectManager.IsJointTracked(userId, iJoint))
            return;


        Quaternion newRotation;
        /*********************************** Wenchuan: get data from Kinect *****************************************/
        if(isUsingKinect)
        {
            /************************************* apply to avatar ************************/
            // if mirrored, jointRotation is the flipped rotation of the mirrored joint
            Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
            if(jointRotation == Quaternion.identity)
				return;
            newRotation = Kinect2AvatarRot(jointRotation, boneIndex);  // apply to the avatar (no need to change boneIndex and boneTransform)

            //************************************* recording data ***********************/
            // what is recorded here is the nonflipped data, following the boneIndex order (not joint order)
            Quaternion jointRotation_writer = kinectManager.GetJointOrientation(userId, (int)boneIndex2JointMap[boneIndex], true);  // for writing
            if(Start && needWrite)
            {
                string sLine = jointRotation_writer.w.ToString() + " " + jointRotation_writer.x.ToString() + " " + jointRotation_writer.y.ToString() + " " + jointRotation_writer.z.ToString();
                write_control.WriteLine(sLine);
            }
        }
        /*********************************** Wenchuan: get data from file *****************************************/
        else 
        {
            Quaternion jointRotation = (Start == true) ? FileGetQuaternion() : Quaternion.identity;
            
            // apply to the avatar
            if (mirroredMovement)
            {
                // apply the flipped rotation to the mirrored joint
                newRotation = Kinect2AvatarRot(Normal2MirroredRotation(jointRotation), MirroredBoneIndex[boneIndex]);
                boneTransform = bones[MirroredBoneIndex[boneIndex]];  // boneTransform stores which bone to rotate
            }
            else
            {
                newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
            }
        }
        /************************************************************************************************/

		if(smoothFactor != 0f)
        	boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
		else
			boneTransform.rotation = newRotation;
	}

	// Apply the rotations tracked by kinect to a special joint
	protected void TransformSpecialBone(Int64 userId, KinectInterop.JointType joint, KinectInterop.JointType jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null)
			return;
		if(isUsingKinect && kinectManager == null)
            return;

        if (isUsingKinect)
        {
            if (!kinectManager.IsJointTracked(userId, (int)joint) ||
               !kinectManager.IsJointTracked(userId, (int)jointParent))
            {
                return;
            }
        }

        Quaternion jointRotation;
        /*********************************** Wenchuan: get data from Kinect *****************************************/
        if(isUsingKinect)
        {
            /************************************* apply to avatar (part 1) ************************/
            Vector3 jointDir = kinectManager.GetJointDirection(userId, (int)joint, false, true);
            // Wenchuan: here the jointDir is not related to flip. It is processed later
            jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

            //************************************* recording data ***********************/
            // what is recorded here is the nonflipped data, following the boneIndex order (not joint order)
            Vector3 jointDir_writer = kinectManager.GetJointDirection(userId, (int)specIndex2JointMap[boneIndex][0], false, true);  // for writing
            Quaternion jointRotation_writer = jointDir_writer != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir_writer) : Quaternion.identity;
            if(Start && needWrite)
            {
                string sLine = jointRotation_writer.w.ToString() + " " + jointRotation_writer.x.ToString() + " " + jointRotation_writer.y.ToString() + " " + jointRotation_writer.z.ToString();
                write_control.WriteLine(sLine);
            }
        }
        /*********************************** Wenchuan: get data from file *****************************************/
        else
        {
            jointRotation = (Start == true) ? FileGetQuaternion() : Quaternion.identity;
        }

		if(!flip)  // if mirrored
		{
			Vector3 mirroredAngles = jointRotation.eulerAngles;
			mirroredAngles.y = -mirroredAngles.y;
			mirroredAngles.z = -mirroredAngles.z;

			jointRotation = Quaternion.Euler(mirroredAngles);
		}
		
		if(jointRotation != Quaternion.identity)
		{
            /************************************* Wenchuan: apply to avatar (part 2) ************************/
			// Smoothly transition to the new rotation
            Quaternion newRotation;
            if (isUsingKinect || !mirroredMovement)
            {
                newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
            }
            else  // !isUsingKinect && mirroredMovement
            {
                newRotation = Kinect2AvatarRot(jointRotation, MirroredBoneIndex[boneIndex]);  // jointRotation has been flipped above
                boneTransform = bones[MirroredBoneIndex[boneIndex]];
            }

			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}
	}

    // Wenchuan
    Quaternion Normal2MirroredRotation(Quaternion NormalRotation)
    {
        Vector3 mirroredAngles = NormalRotation.eulerAngles;
        mirroredAngles.y = -mirroredAngles.y;
        mirroredAngles.z = -mirroredAngles.z;

        return Quaternion.Euler(mirroredAngles);
    }

    // Wenchuan
    Vector3 String2Vector3(string line)
    {
        double[] doubles = Array.ConvertAll<String, double>(line.Split(' '), Double.Parse);
        if (doubles.GetLength(0) != 3)
        {
            return Vector3.zero;
        }
        Vector3 vector;
        vector.x = (float)doubles[0];
        vector.y = (float)doubles[1];
        vector.z = (float)doubles[2];

        return vector;
    }

    // Wenchuan
    public Vector3 FileGetVector3()
    {
        String line;
        line = read.ReadLine();
        if (line != null)
        {
            return String2Vector3(line);
        }

        return Vector3.zero;
    }

	// Move the avatar - get the tracked position of the real user and apply it to avatar.
	protected void MoveAvatar(Int64 UserID)
	{
		if((moveRate == 0f))
		{
			return;
		}
        if (isUsingKinect)
        {
            if (!kinectManager || !kinectManager.IsJointTracked(UserID, (int)KinectInterop.JointType.SpineBase))
                return;
        }
		
		// Wenchuan: get the real user position "trans"
        Vector3 trans;
        if (isUsingKinect)
        {
            trans = kinectManager.GetUserPosition(UserID);
        }
        else  // read from file
        {
            if (!Start)  // not start training yet
            {
                trans = trans0;
            }
            else
            {
                if (start0 == false)  // come here in the first time, because "trans" in the first frame has already been read and stored in "trans0"
                {
                    start0 = true; // DO NOT READ "TRANS" AGAIN!!!
                    trans = trans0;
                }
                else
                {
                    trans = FileGetVector3();  // return (0,0,0) when the file ends
                }
            }
        }

        // Wenchuan: Write position in control file
        if(Start && needWrite)
        {
            string sLine = trans.x.ToString() + " " + trans.y.ToString() + " " + trans.z.ToString();
            write_control.WriteLine(sLine);
        }

        // when the file ends, use the previous "trans"
        if (trans != Vector3.zero)  // file not ended yet
        {
            position_last = trans;
        }
        else    // end of the guidance, use the position of the previous frame
        {
            trans = position_last;
        }

        //tune position of the two avatars (just change the position on the screen, the file records the original data)
        float center = (float)1.5;  // new center for two avatars
        if (isGuidanceShowing)
        {
            trans.x = isRight ? (trans.x + (float)0.5) : (trans.x - (float)2);
        }
        else
        {
            trans.x = isRight ? (trans.x + center) : (trans.x - center);
        }

        // Wenchuan: show instruction message
        if(isPTRecorder)
        {
            Instruction.transform.position = isRight ? new Vector3(1.5f, 2.8f, 0f) : new Vector3(-2.9f, 2.8f, 0f);
            Instruction.GetComponent<TextMesh>().text = isRight ? "Your mirrored avatar" : "Your unmirrored avatar";
        }
        if(isUserLearning)
        {
            Instruction.transform.position = isRight ? new Vector3(2f, 2.8f, 0f) : new Vector3(-2.2f, 2.8f, 0f);
            Instruction.GetComponent<TextMesh>().text = isRight ? "Your avatar" : "PT's avatar";
        }
        if(isGuidanceShowing)
        {
            Instruction.transform.position = isRight ? new Vector3(0.3f, 2.8f, 0f) : new Vector3(-3.5f, 2.8f, 0f);
            Instruction.GetComponent<TextMesh>().text = isRight ? "Your mirrored movement" : "Side view";
        }


//		if(posRelativeToCamera && avatarPosOverlaysBackground)
//		{
//			// gets the user's spine-base position, matching the color-camera background
//			Rect backgroundRect = posRelativeToCamera.pixelRect;
//			PortraitBackground portraitBack = PortraitBackground.Instance;
//			
//			if(portraitBack && portraitBack.enabled)
//			{
//				backgroundRect = portraitBack.GetBackgroundRect();
//			}
//
//			trans = kinectManager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineBase, posRelativeToCamera, backgroundRect);
//		}

		if(!offsetCalibrated)  // Wenchuan: the first frame, get newBodyRootPos for the avatar
		{
			//offsetCalibrated = true;  // flag 1
			
			xOffset = trans.x;  // !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			yOffset = trans.y;  // trans.y * moveRate;
			zOffset = !mirroredMovement ? -trans.z : trans.z;  // -trans.z * moveRate;

            if (posRelativeToCamera)
            {
                if (bodyRoot == null)
                    Debug.Log("null ");

                Vector3 cameraPos = posRelativeToCamera.transform.position;
                // cameraPos: position of the camera in 3D virtual world, it's set by us as (0.0, 1.0, -4.0)

                Vector3 bodyRootPos = bodyRoot != null ? bodyRoot.position : transform.position;
                // bodyRootPos : position of the avatar in 3D virtual world. Use bodyRoot if bodyRoot != null, otherwise use transform (i.e., (2, 0, 0) set by us)

                Vector3 hipCenterPos = bodyRoot != null ? bodyRoot.position : bones[0].position;
                // hipCenterPos: central position of the real user in the real world. Use bodyRoot if bodyRoot != null, otherwise use bones[0] (i.e., hip joint)
                
                float yRelToAvatar = 0f;  // vertical distance between the avatar and the camera in 3D virtual world
				if(verticalMovement)
				{
					yRelToAvatar = (trans.y - cameraPos.y) - (hipCenterPos - bodyRootPos).magnitude;  // "trans" is the position of the real user 
				}
				else
				{
					yRelToAvatar = bodyRootPos.y - cameraPos.y; 
				}

                Vector3 relativePos = new Vector3(trans.x, yRelToAvatar, trans.z);  // avatar's position relative to the camera
				Vector3 newBodyRootPos = cameraPos + relativePos;  // avatar's position relative to the origin

                // Wenchuan: the feet are on the floor in the first frame
                newBodyRootPos.y = 0;  // with this sentence, yRelToAvatar is useless

//				if(offsetNode != null) 
//				{
//					newBodyRootPos += offsetNode.transform.position;
//				}

				if(bodyRoot != null)
				{
					bodyRoot.position = newBodyRootPos;
				}
				else
				{
					transform.position = newBodyRootPos;
				}

				bodyRootPosition = newBodyRootPos;
			}
        }

		// transition to the new position
		Vector3 targetPos = bodyRootPosition + Kinect2AvatarPos(trans, verticalMovement);
        // Wenchuan: "bodyRootPos" is the avatar's position in the first frame
        // "trans" is the current user position. Kinect2AvatarPos() returns the relative position compared with "xOffset/yOffset/zOffset"
        // targetPos is the target position of the avatar on this frame

        if (isRigidBody && !verticalMovement)
		{
			// workaround for obeying the physics (e.g. gravity falling)
			targetPos.y = bodyRoot != null ? bodyRoot.position.y : transform.position.y;
		}

        // Wenchuan: tune the avatar position according to user setting
        if (!offsetCalibrated)  // calculate UserOffset
        {
            offsetCalibrated = true;  // Be sure to retore flag 1 if this sentence is deleted!!!!!!!!!!!!
            UserOffset = Position0 - targetPos;  // targetPos here is targetPos0 (first frame)
        }
        targetPos = FixedPosition ? targetPos + UserOffset : targetPos;  // Wenchuan: be sure to add the offset before the following "Lerp"



        if (bodyRoot != null)
		{
			bodyRoot.position = smoothFactor != 0f ? 
				Vector3.Lerp(bodyRoot.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
		}
		else
		{
			transform.position = smoothFactor != 0f ? 
				Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
        }

    }

    // Set model's arms to be in T-pose
    protected void SetModelArmsInTpose()
	{
		Vector3 vTposeLeftDir = transform.TransformDirection(Vector3.left);
		Vector3 vTposeRightDir = transform.TransformDirection(Vector3.right);
		Animator animator = GetComponent<Animator>();
		
		Transform transLeftUarm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
		Transform transLeftLarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
		Transform transLeftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
		
		if(transLeftUarm != null && transLeftLarm != null)
		{
			Vector3 vUarmLeftDir = transLeftLarm.position - transLeftUarm.position;
			float fUarmLeftAngle = Vector3.Angle(vUarmLeftDir, vTposeLeftDir);
			
			if(Mathf.Abs(fUarmLeftAngle) >= 5f)
			{
				Quaternion vFixRotation = Quaternion.FromToRotation(vUarmLeftDir, vTposeLeftDir);
				transLeftUarm.rotation = vFixRotation * transLeftUarm.rotation;
			}
			
			if(transLeftHand != null)
			{
				Vector3 vLarmLeftDir = transLeftHand.position - transLeftLarm.position;
				float fLarmLeftAngle = Vector3.Angle(vLarmLeftDir, vTposeLeftDir);
				
				if(Mathf.Abs(fLarmLeftAngle) >= 5f)
				{
					Quaternion vFixRotation = Quaternion.FromToRotation(vLarmLeftDir, vTposeLeftDir);
					transLeftLarm.rotation = vFixRotation * transLeftLarm.rotation;
				}
			}
		}
		
		Transform transRightUarm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		Transform transRightLarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
		Transform transRightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
		
		if(transRightUarm != null && transRightLarm != null)
		{
			Vector3 vUarmRightDir = transRightLarm.position - transRightUarm.position;
			float fUarmRightAngle = Vector3.Angle(vUarmRightDir, vTposeRightDir);
			
			if(Mathf.Abs(fUarmRightAngle) >= 5f)
			{
				Quaternion vFixRotation = Quaternion.FromToRotation(vUarmRightDir, vTposeRightDir);
				transRightUarm.rotation = vFixRotation * transRightUarm.rotation;
			}
			
			if(transRightHand != null)
			{
				Vector3 vLarmRightDir = transRightHand.position - transRightLarm.position;
				float fLarmRightAngle = Vector3.Angle(vLarmRightDir, vTposeRightDir);
				
				if(Mathf.Abs(fLarmRightAngle) >= 5f)
				{
					Quaternion vFixRotation = Quaternion.FromToRotation(vLarmRightDir, vTposeRightDir);
					transRightLarm.rotation = vFixRotation * transRightLarm.rotation;
				}
			}
		}
		
	}
	
	// If the bones to be mapped have been declared, map that bone to the model.
	protected virtual void MapBones()
	{
//		// make OffsetNode as a parent of model transform.
//		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
//		offsetNode.transform.position = transform.position;
//		offsetNode.transform.rotation = transform.rotation;
//		offsetNode.transform.parent = transform.parent;
		
//		// take model transform as body root
//		transform.parent = offsetNode.transform;
//		transform.localPosition = Vector3.zero;
//		transform.localRotation = Quaternion.identity;
		
		//bodyRoot = transform;

		// get bone transforms from the animator component
		Animator animatorComponent = GetComponent<Animator>();
				
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
			offsetNodePos = offsetNode.transform.position;
			offsetNodeRot = offsetNode.transform.rotation;
		}

		initialPosition = transform.position;
		initialRotation = transform.rotation;

//		if(offsetNode != null)
//		{
//			initialRotation = Quaternion.Inverse(offsetNodeRot) * initialRotation;
//		}

		transform.rotation = Quaternion.identity;

		// save the body root initial position
		if(bodyRoot != null)
		{
			bodyRootPosition = bodyRoot.position;
		}
		else
		{
			bodyRootPosition = transform.position;
		}

		if(offsetNode != null)
		{
			bodyRootPosition = bodyRootPosition - offsetNodePos;
		}
		
		// save the initial bone rotations
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation;
			}
		}

		// Restore the initial rotation
		transform.rotation = initialRotation;
	}
	
	// Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
	protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
        if(boneIndex == 3 || boneIndex == 4)  // Wenchuan: keep the neck and head fixed
        {
            jointRotation = Quaternion.identity;
        }

		Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		//newRotation = initialRotation * newRotation;

		if(offsetNode != null)
		{
			newRotation = offsetNode.transform.rotation * newRotation;
		}
		else
		{
			newRotation = initialRotation * newRotation;
		}
		
		return newRotation;
	}
	
	// Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
		float xPos;

//		if(!mirroredMovement)
			xPos = (jointPosition.x - xOffset) * moveRate;
//		else
//			xPos = (-jointPosition.x - xOffset) * moveRate;
		
		float yPos = (jointPosition.y - yOffset) * moveRate;
		//float zPos = (-jointPosition.z - zOffset) * moveRate;
		float zPos = !mirroredMovement ? (-jointPosition.z - zOffset) * moveRate : (jointPosition.z - zOffset) * moveRate;
		
		Vector3 newPosition = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

		if(offsetNode != null)
		{
			newPosition += offsetNode.transform.position;
		}
		
		return newPosition;
	}

//	protected void OnCollisionEnter(Collision col)
//	{
//		Debug.Log("Collision entered");
//	}
//
//	protected void OnCollisionExit(Collision col)
//	{
//		Debug.Log("Collision exited");
//	}
	
	// dictionaries to speed up bones' processing
	// the author of the terrific idea for kinect-joints to mecanim-bones mapping
	// along with its initial implementation, including following dictionary is
	// Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
	private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
        { 0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
//        {2, HumanBodyBones.Chest},
		{3, HumanBodyBones.Neck},
//		{4, HumanBodyBones.Head},
		
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
//		{8, HumanBodyBones.LeftIndexProximal},
//		{9, HumanBodyBones.LeftIndexIntermediate},
//		{10, HumanBodyBones.LeftThumbProximal},
		
		{11, HumanBodyBones.RightUpperArm},
		{12, HumanBodyBones.RightLowerArm},
		{13, HumanBodyBones.RightHand},
//		{14, HumanBodyBones.RightIndexProximal},
//		{15, HumanBodyBones.RightIndexIntermediate},
//		{16, HumanBodyBones.RightThumbProximal},
		
		{17, HumanBodyBones.LeftUpperLeg},
		{18, HumanBodyBones.LeftLowerLeg},
		{19, HumanBodyBones.LeftFoot},
//		{20, HumanBodyBones.LeftToes},
		
		{21, HumanBodyBones.RightUpperLeg},
		{22, HumanBodyBones.RightLowerLeg},
		{23, HumanBodyBones.RightFoot},
//		{24, HumanBodyBones.RightToes},
		
		{25, HumanBodyBones.LeftShoulder},
		{26, HumanBodyBones.RightShoulder},
         
	};
	
	protected readonly Dictionary<int, KinectInterop.JointType> boneIndex2JointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},
		
		{5, KinectInterop.JointType.ShoulderLeft},
		{6, KinectInterop.JointType.ElbowLeft},
		{7, KinectInterop.JointType.WristLeft},
		{8, KinectInterop.JointType.HandLeft},
		
		{9, KinectInterop.JointType.HandTipLeft},
		{10, KinectInterop.JointType.ThumbLeft},
		
		{11, KinectInterop.JointType.ShoulderRight},
		{12, KinectInterop.JointType.ElbowRight},
		{13, KinectInterop.JointType.WristRight},
		{14, KinectInterop.JointType.HandRight},
		
		{15, KinectInterop.JointType.HandTipRight},
		{16, KinectInterop.JointType.ThumbRight},
		
		{17, KinectInterop.JointType.HipLeft},
		{18, KinectInterop.JointType.KneeLeft},
		{19, KinectInterop.JointType.AnkleLeft},
		{20, KinectInterop.JointType.FootLeft},
		
		{21, KinectInterop.JointType.HipRight},
		{22, KinectInterop.JointType.KneeRight},
		{23, KinectInterop.JointType.AnkleRight},
		{24, KinectInterop.JointType.FootRight},
	};
	
	protected readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2JointMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
	};

    protected readonly Dictionary<int, int> MirroredBoneIndex = new Dictionary<int, int>
    {
        {0, 0},
		{1, 1},
		{2, 2},
		{3, 3},
		{4, 4},
		
		{5, 11}, 
		{6, 12},
		{7, 13},
		{8, 14},
		
		{9, 15}, 
		{10, 16}, 
		
		{11, 5}, 
		{12, 6}, 
		{13, 7},
		{14, 8},
		
		{15, 9},
		{16, 10},
		
		{17, 21},
		{18, 22},
		{19, 23},
		{20, 24},
		
		{21, 17},
		{22, 18},
		{23, 19},
		{24, 20},

        {25, 26},
        {26, 25},
    };
	
	protected readonly Dictionary<int, KinectInterop.JointType> boneIndex2MirrorJointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},
		
		{5, KinectInterop.JointType.ShoulderRight},
		{6, KinectInterop.JointType.ElbowRight},
		{7, KinectInterop.JointType.WristRight},
		{8, KinectInterop.JointType.HandRight},
		
		{9, KinectInterop.JointType.HandTipRight},
		{10, KinectInterop.JointType.ThumbRight},
		
		{11, KinectInterop.JointType.ShoulderLeft},
		{12, KinectInterop.JointType.ElbowLeft},
		{13, KinectInterop.JointType.WristLeft},
		{14, KinectInterop.JointType.HandLeft},
		
		{15, KinectInterop.JointType.HandTipLeft},
		{16, KinectInterop.JointType.ThumbLeft},
		
		{17, KinectInterop.JointType.HipRight},
		{18, KinectInterop.JointType.KneeRight},
		{19, KinectInterop.JointType.AnkleRight},
		{20, KinectInterop.JointType.FootRight},
		
		{21, KinectInterop.JointType.HipLeft},
		{22, KinectInterop.JointType.KneeLeft},
		{23, KinectInterop.JointType.AnkleLeft},
		{24, KinectInterop.JointType.FootLeft},
	};
	
	protected readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
	};
	
	
	protected readonly Dictionary<KinectInterop.JointType, int> jointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
	{
		{KinectInterop.JointType.SpineBase, 0},
		{KinectInterop.JointType.SpineMid, 1},
		{KinectInterop.JointType.SpineShoulder, 2},
		{KinectInterop.JointType.Neck, 3},
		{KinectInterop.JointType.Head, 4},
		
		{KinectInterop.JointType.ShoulderLeft, 5},
		{KinectInterop.JointType.ElbowLeft, 6},
		{KinectInterop.JointType.WristLeft, 7},
		{KinectInterop.JointType.HandLeft, 8},
		
		{KinectInterop.JointType.HandTipLeft, 9},
		{KinectInterop.JointType.ThumbLeft, 10},
		
		{KinectInterop.JointType.ShoulderRight, 11},
		{KinectInterop.JointType.ElbowRight, 12},
		{KinectInterop.JointType.WristRight, 13},
		{KinectInterop.JointType.HandRight, 14},
		
		{KinectInterop.JointType.HandTipRight, 15},
		{KinectInterop.JointType.ThumbRight, 16},
		
		{KinectInterop.JointType.HipLeft, 17},
		{KinectInterop.JointType.KneeLeft, 18},
		{KinectInterop.JointType.AnkleLeft, 19},
		{KinectInterop.JointType.FootLeft, 20},
		
		{KinectInterop.JointType.HipRight, 21},
		{KinectInterop.JointType.KneeRight, 22},
		{KinectInterop.JointType.AnkleRight, 23},
		{KinectInterop.JointType.FootRight, 24},
	};
	
	protected readonly Dictionary<KinectInterop.JointType, int> mirrorJointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
	{
		{KinectInterop.JointType.SpineBase, 0},
		{KinectInterop.JointType.SpineMid, 1},
		{KinectInterop.JointType.SpineShoulder, 2},
		{KinectInterop.JointType.Neck, 3},
		{KinectInterop.JointType.Head, 4},
		
		{KinectInterop.JointType.ShoulderRight, 5},
		{KinectInterop.JointType.ElbowRight, 6},
		{KinectInterop.JointType.WristRight, 7},
		{KinectInterop.JointType.HandRight, 8},
		
		{KinectInterop.JointType.HandTipRight, 9},
		{KinectInterop.JointType.ThumbRight, 10},
		
		{KinectInterop.JointType.ShoulderLeft, 11},
		{KinectInterop.JointType.ElbowLeft, 12},
		{KinectInterop.JointType.WristLeft, 13},
		{KinectInterop.JointType.HandLeft, 14},
		
		{KinectInterop.JointType.HandTipLeft, 15},
		{KinectInterop.JointType.ThumbLeft, 16},
		
		{KinectInterop.JointType.HipRight, 17},
		{KinectInterop.JointType.KneeRight, 18},
		{KinectInterop.JointType.AnkleRight, 19},
		{KinectInterop.JointType.FootRight, 20},
		
		{KinectInterop.JointType.HipLeft, 21},
		{KinectInterop.JointType.KneeLeft, 22},
		{KinectInterop.JointType.AnkleLeft, 23},
		{KinectInterop.JointType.FootLeft, 24},
	};

    // Wenchuan
    protected readonly Dictionary<int, KinectInterop.JointType> jointIndex2JointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},

		{2, KinectInterop.JointType.Neck},
		{3, KinectInterop.JointType.Head},
		
		{4, KinectInterop.JointType.ShoulderLeft},
		{5, KinectInterop.JointType.ElbowLeft},
		{6, KinectInterop.JointType.WristLeft},
		{7, KinectInterop.JointType.HandLeft},
		
		{8, KinectInterop.JointType.ShoulderRight},
		{9, KinectInterop.JointType.ElbowRight},
		{10, KinectInterop.JointType.WristRight},
		{11, KinectInterop.JointType.HandRight},
		
		{12, KinectInterop.JointType.HipLeft},
		{13, KinectInterop.JointType.KneeLeft},
		{14, KinectInterop.JointType.AnkleLeft},
		{15, KinectInterop.JointType.FootLeft},
		
		{16, KinectInterop.JointType.HipRight},
		{17, KinectInterop.JointType.KneeRight},
		{18, KinectInterop.JointType.AnkleRight},
		{19, KinectInterop.JointType.FootRight},

        {20, KinectInterop.JointType.SpineShoulder},
		{21, KinectInterop.JointType.HandTipLeft},
		{22, KinectInterop.JointType.ThumbLeft},
		{23, KinectInterop.JointType.HandTipRight},
        {24, KinectInterop.JointType.ThumbRight},
	};
	
}

