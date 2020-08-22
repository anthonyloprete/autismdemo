using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool rightHand = false; //false is left, true is right
    public GameObject ball;

    private bool reset = false;
    private int trial = 0;
    private float r = 0.4f;
    private float theta;
    private double error;
    private float[] initCoords;
    private Random rnd = new Random();
    private Vector3 corVec = new Vector3(0.1f, 0.1f, 0.1f);
    private GameObject OVRCameraRig;
    private GameObject TrackingSpace;
    private GameObject LeftHandAnchor;
    private GameObject RightHandAnchor;
    private GameObject LeftFab;
    private GameObject RightFab;
    private GameObject Hand;
    private GameObject DisplayText;
    private GameObject Degree;
    private GameObject Error;
    private GameObject Trial;
    private GameObject BallPos;
    private GameObject HandPos;
    private GameObject Pinch;
    public static GameManager Instance;

    [SerializeField] private OVRSkeleton skeleton;
    private OVRHand hand;
    private Transform tip;

    private int[,] expOrder = new int[25, 2];
    private float[,,] data = new float[5, 5, 2];


    // Start is called before the first frame update
    void Start()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        //init_coords[0] = CenterEye.Instance.gameObject.transform.position.x;
        //init_coords[1] = CenterEye.Instance.gameObject.transform.position.y;
        //init_coords[2] = CenterEye.Instance.gameObject.transform.position.z;

        //LeftFab.gameObject.GetComponent<OVRMeshRenderer>().enabled = false; makes hand invisible

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                expOrder[i * 5 + j, 0] = i;
                expOrder[i * 5 + j, 1] = j;
            }
        }

        Shuffle();

        OVRCameraRig = GameObject.Find("OVRCameraRig");
        TrackingSpace = OVRCameraRig.transform.GetChild(0).gameObject;
        LeftHandAnchor = TrackingSpace.transform.GetChild(4).gameObject;
        RightHandAnchor = TrackingSpace.transform.GetChild(5).gameObject;
        LeftFab = LeftHandAnchor.transform.GetChild(1).gameObject;
        RightFab = RightHandAnchor.transform.GetChild(1).gameObject;

        DisplayText = GameObject.Find("DisplayText");
        Degree = DisplayText.transform.GetChild(0).gameObject;
        Error = DisplayText.transform.GetChild(1).gameObject;
        Trial = DisplayText.transform.GetChild(2).gameObject;
        HandPos = DisplayText.transform.GetChild(3).gameObject;
        BallPos = DisplayText.transform.GetChild(4).gameObject;
        Pinch = DisplayText.transform.GetChild(5).gameObject;

        Degree.GetComponent<TextMesh>().text = "deg: n/a";
        Error.GetComponent<TextMesh>().text = "err: n/a";
        Trial.GetComponent<TextMesh>().text = "tri: 1";


        if (rightHand == false)
        {

            Hand = LeftHand.Instance.gameObject;
            skeleton = LeftFab.GetComponent<OVRSkeleton>();
            hand = LeftFab.GetComponent<OVRHand>();
            RightFab.gameObject.SetActive(false);


        }

        else
        {

            Hand = RightHand.Instance.gameObject;
            skeleton = RightFab.GetComponent<OVRSkeleton>();
            hand = RightFab.GetComponent<OVRHand>();
            LeftFab.gameObject.SetActive(false);

        }


        theta = Random.Range(110f, 200f) * Mathf.PI / 180f;
        theta = ( (expOrder[trial, 0] + 1) * 20 + 30) * Mathf.PI / 180f;
        Instantiate(ball, new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta)), Quaternion.identity);
        Degree.GetComponent<TextMesh>().text = "deg: " + ((expOrder[trial, 0] + 1) * 20 + 30).ToString();

        //fingerBones = new List<OVRBone>(skeleton.Bones);
        //HandPos.GetComponent<TextMesh>().text = fingerBones.Count.ToString();



        tip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;

    }

    // Update is called once per frame
    void Update()

    {

        BallPos.GetComponent<TextMesh>().text = System.Math.Round(Ball.Instance.gameObject.transform.position.x,2).ToString() + ","
                                              + System.Math.Round(Ball.Instance.gameObject.transform.position.z,2).ToString() + ",";


        HandPos.GetComponent<TextMesh>().text = System.Math.Round(tip.transform.position.x, 2).ToString() + ","
                                              + System.Math.Round(tip.transform.position.z, 2).ToString() + ",";

        Pinch.GetComponent<TextMesh>().text = hand.GetFingerIsPinching(OVRHand.HandFinger.Index).ToString();


        //theta = Random.Range(40f, 160f) * Mathf.PI / 180f;
        //Ball.Instance.gameObject.transform.position = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));

        if ((tip.transform.position).magnitude >= r && (reset == false) && trial < 25)
        {
            //error = Vector3.Distance(Ball.Instance.gameObject.transform.position, tip.transform.position);
            error = Vector3.Angle(Ball.Instance.gameObject.transform.position, tip.transform.position);
            error = System.Math.Round(error, 3);

            reset = true;
            Ball.Instance.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
            trial++;
            Error.GetComponent<TextMesh>().text = "err: " + error.ToString();
            Trial.GetComponent<TextMesh>().text = "tri: " + (trial + 1).ToString();
        }

        if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index) == true && reset == true && trial < 25)
        {
            reset = false;
            theta = ((expOrder[trial, 0] + 1) * 20 + 30) * Mathf.PI / 180f;
            Ball.Instance.gameObject.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
            Ball.Instance.gameObject.transform.position = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
            Degree.GetComponent<TextMesh>().text = "deg: " + ((expOrder[trial, 0] + 1) * 20 + 30).ToString();



        }

        if (trial >= 25) {
            Ball.Instance.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
        }

    }

    public void Shuffle()
    {
        int rand;;
        int tempOne;
        int tempTwo;

        for (int i = 0; i < expOrder.GetLength(0); i++)
        {
            rand = Random.Range(0, expOrder.GetLength(0) - 1);

            tempOne = expOrder[rand, 0];
            tempTwo = expOrder[rand, 1];

            expOrder[rand, 0] = expOrder[i, 0];
            expOrder[rand, 1] = expOrder[i, 1];

            expOrder[i, 0] = tempOne;
            expOrder[i, 1] = tempTwo;

        }
    }
}