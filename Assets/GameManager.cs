using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class GameManager : MonoBehaviour
{
    public bool rightHand = false; //false is left hand, true is right hand for the experiment
    public GameObject ball;        // the pink ball in the experiment; set with inspector because it is a public variable

    private bool reset = false; 
    private int trial = 0;
    private float r = 0.4f;
    private float theta;
    private double error;
    private UnityEngine.Random rnd = new UnityEngine.Random();  //Allows for random numbers to be generated
    private Vector3 corVec = new Vector3(0.1f, 0.1f, 0.1f);     //Units should default to meters, f means float
    private GameObject OVRCameraRig; //Object which contains VR headset and hands
    private GameObject LeftFab;      //Left hand gameobject
    private GameObject RightFab;     //Right hand gameobject
    private GameObject DisplayText;  //Parent gameobject containing all on-screen text (below)

    //************************
    private GameObject Degree;
    private GameObject Error;
    private GameObject Trial;
    private GameObject BallPos;
    private GameObject HandPos;
    private GameObject Pinch;
    //************************

    public static GameManager Instance; //A copy of the GameManager so other scripts can use variables in here (not currently being used)

    [SerializeField] private OVRSkeleton skeleton; //I do not know what SerializeField does, but you need it for OVRSkeleton to work
    private OVRHand hand; //Will become the functioning hand in the game, depending on "rightHand"
    private Transform tip; //Will track the index finger tip on whatever hand becomes active

    private int[,] expOrder = new int[25, 2];     //This defines the experiment order, the 2 might be for a different less efficient randomization paradigm
    private float[,,] data = new float[5, 5, 2];   //This would store the collected data from the experiment
    public string path;
    public string output;
    public int emailSent;


    //Start is called when the game is launched
    void Start()

    {

        CreateText(); //calls the function to create the text file once the experiment starts

        if (Instance == null)
        {
            Instance = this;    //Stores a copy of itself in Instance
        }

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                expOrder[i * 5 + j, 0] = i;
                expOrder[i * 5 + j, 1] = j;
            }
        }

        Shuffle();    //Randomizes the condition order

        LeftFab = LeftHand.Instance.gameObject;
        RightFab = RightHand.Instance.gameObject;

        //Using GetChild on a GameObject will return the closest GameObject from the top, like an array
        DisplayText = GameObject.Find("DisplayText");
        Degree = DisplayText.transform.GetChild(0).gameObject;     
        Error = DisplayText.transform.GetChild(1).gameObject;
        Trial = DisplayText.transform.GetChild(2).gameObject;
        HandPos = DisplayText.transform.GetChild(3).gameObject;
        BallPos = DisplayText.transform.GetChild(4).gameObject;
        Pinch = DisplayText.transform.GetChild(5).gameObject;

        //GetComponent is different for every type of component you're working with
        Degree.GetComponent<TextMesh>().text = "deg: n/a";
        Error.GetComponent<TextMesh>().text = "err: n/a";
        Trial.GetComponent<TextMesh>().text = "tri: 1";

        if (rightHand == false) 
        {

            skeleton = LeftFab.GetComponent<OVRSkeleton>(); //Getting the skeleton data
            hand = LeftFab.GetComponent<OVRHand>();         //Getting the hand prefab in the OVR camera rig
            RightFab.gameObject.SetActive(false);      //SetActive is how you uncheck the GameObjects inside the script

        }

        else
        {

            skeleton = RightFab.GetComponent<OVRSkeleton>();
            hand = RightFab.GetComponent<OVRHand>();
            LeftFab.gameObject.SetActive(false);

        }

        theta = ((expOrder[trial, 0] + 1) * 20 + 30) * Mathf.PI / 180f; //Sets the angle
        Instantiate(ball, new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta)), Quaternion.identity);
        Degree.GetComponent<TextMesh>().text = "deg: " + ((expOrder[trial, 0] + 1) * 20 + 30).ToString();

        tip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;  //Gets access to the fingertip position
        //8 might be able to replace "(int)OVRSkeleton.BoneId.Hand_IndexTip"

        //hand.GetComponent<Renderer>().enabled = false;   makes hand invisible 

    }

    // Update is called once per frame
    void Update()

    {
        //posiiton of the ball, no y
        BallPos.GetComponent<TextMesh>().text = System.Math.Round(Ball.Instance.gameObject.transform.position.x, 2).ToString() + ","
                                              + System.Math.Round(Ball.Instance.gameObject.transform.position.z, 2).ToString() + ",";

        //Position of fingertip, no y
        HandPos.GetComponent<TextMesh>().text = System.Math.Round(tip.transform.position.x, 2).ToString() + ","
                                              + System.Math.Round(tip.transform.position.z, 2).ToString() + ",";

        Pinch.GetComponent<TextMesh>().text = hand.GetFingerIsPinching(OVRHand.HandFinger.Index).ToString(); //How to get hand pinching bool


        if ((tip.transform.position).magnitude >= r && (reset == false) && trial < 25)
        {

            //computes absolute value of angle from the ball to the fingertip
            error = Vector3.Angle(Ball.Instance.gameObject.transform.position, tip.transform.position);  
            error = System.Math.Round(error, 3);

            reset = true;

            //By making the size of the ball 0, it is effectively made invisible
            Ball.Instance.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);


            trial++;
            Error.GetComponent<TextMesh>().text = "err: " + error.ToString();
            Trial.GetComponent<TextMesh>().text = "tri: " + (trial + 1).ToString();

            {

                //Content of the previously created text file
                string content = "Error: " + error + "\r";
                //Add some text to it by appending a new line after each trial
                File.AppendAllText(path, content);

            }

        }
        // run this script once at the 25th trials and the email has not been sent yet (this prevents an email from being sent during every frame of the 25th trial)
        if (trial == 25 && emailSent == 0)
        {
            //must enable SMTP on the specified gmail account https://www.youtube.com/watch?v=D-NYmDWiFjU

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("elisenstein@gmail.com");
            //enter SMTP enabled gmail address above to send FROM
            mail.To.Add("elisenstein@gmail.com");
            //enter SMTP enabled gmail address above to send TO
            mail.Subject = "Test Smtp Mail";
            mail.IsBodyHtml = true; //to make message body as html  
            mail.Body = "Data";
            System.Net.Mail.Attachment attachment;
            attachment = new System.Net.Mail.Attachment(path); //********
            mail.Attachments.Add(attachment);

            // you can use others too.
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("elisenstein@gmail.com", "password") as ICredentialsByHost;
            //enter SMTP enabled gmail account above NetworkCredential("emailaddress@gmail.com", "password for email address")
            smtpServer.EnableSsl = true;
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
            smtpServer.Send(mail);
            emailSent = 1;
        }

        if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index) == true && reset == true && trial < 25)
        {
            reset = false;
            theta = ((expOrder[trial, 0] + 1) * 20 + 30) * Mathf.PI / 180f;
            Ball.Instance.gameObject.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
            Ball.Instance.gameObject.transform.position = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
            Degree.GetComponent<TextMesh>().text = "deg: " + ((expOrder[trial, 0] + 1) * 20 + 30).ToString();

        }

        if (trial >= 25)
        {
            Ball.Instance.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
        }

    }

    void Shuffle()     //Fisher-yates shuffle
    {
        int rand; ;
        int tempOne;
        int tempTwo;

        for (int i = 0; i < expOrder.GetLength(0); i++)
        {
            rand = UnityEngine.Random.Range(0, expOrder.GetLength(0) - 1);

            tempOne = expOrder[rand, 0];
            tempTwo = expOrder[rand, 1];

            expOrder[rand, 0] = expOrder[i, 0];
            expOrder[rand, 1] = expOrder[i, 1];

            expOrder[i, 0] = tempOne;
            expOrder[i, 1] = tempTwo;

        }

    }

    //Create a text file that contains the errors for each trial
    void CreateText()
    {
        //Path of the file to save to Oculus Quest internal files
        path = "./sdcard/Android/data/com.TadinLab.AutismDemo/Data/" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
        output = "";
        emailSent = 0;

        //Create file if it doesn't exist
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "List of Errors by Trial \n\n");
        }

    }

}