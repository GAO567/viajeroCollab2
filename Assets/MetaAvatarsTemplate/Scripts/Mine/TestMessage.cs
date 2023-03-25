using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMessage : MonoBehaviour
{

    [SerializeField]
    OVRCameraRig player1;

    [SerializeField]
    GameObject rightHandPlayer1;

    [SerializeField]
    GameObject headPlayer;

    [SerializeField]
    GameObject leftHandPlayer1;

    [SerializeField]
    OVRCameraRig player2;

    [SerializeField]
    GameObject rightHandPlayer2;

    [SerializeField]
    GameObject headPlayer2;

    [SerializeField]
    GameObject leftHandPlayer2;

    [SerializeField]
    int playerNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setPlayerNumber(int number)
    {
        playerNumber = number;
    }

    public void setReference(GameObject obj)
    {
        if(playerNumber == 1)
        {
            player1 = obj.GetComponent<OVRCameraRig>();
            rightHandPlayer1 = player1.rightControllerAnchor.gameObject;
            headPlayer = player1.centerEyeAnchor.gameObject;
            leftHandPlayer1 = player1.leftControllerAnchor.gameObject;
        }
        else if(playerNumber == 2)
        {
            player2 = obj.GetComponent<OVRCameraRig>();
            rightHandPlayer2 = player2.rightControllerAnchor.gameObject;
            headPlayer2 = player2.centerEyeAnchor.gameObject;
            leftHandPlayer2 = player2.leftControllerAnchor.gameObject;
        }
        else
        {
            //do nothing... for now
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
