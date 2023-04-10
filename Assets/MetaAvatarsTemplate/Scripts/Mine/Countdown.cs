using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using TMPro;

public class Countdown : MonoBehaviour
{
    float timeToStartScene = 10.0f;
    [SerializeField]
    float timeRemaining = 10.0f;

    [SerializeField]
    int id = 0;

    [SerializeField]
    CollabType collabType = CollabType.FacetoFaceIntersect;

    [SerializeField]
    int avatarId = 0;


    [SerializeField]
    TMP_Dropdown dropdown;

    [SerializeField]
    TMP_Dropdown dropdownRemote;

    [SerializeField] TMP_InputField inputFIeld;

    bool timerIsRunning = false;

    [SerializeField]
    UnityEngine.Video.VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        timerIsRunning = false;
        dropdown.value = (int)collabType;
        inputFIeld.text = id.ToString();
    }

    public void GoToScene()
    {
        Debug.Log("Time has run out!");
        GlobalVariables.Set("userId", int.Parse( inputFIeld.text));
        GlobalVariables.Set("CollabType", dropdown.value);
        GlobalVariables.Set("camefromVideoScene", true);
        SceneManager.LoadScene("NetworkScene", LoadSceneMode.Single);
        GlobalVariables.Set("Remote", dropdownRemote.value);
        
        


        //
        timeRemaining = 0;
        timerIsRunning = false;


    }


    public void PlayVideo()
    {
        videoPlayer.enabled = true;
        timerIsRunning = true;
    }

    //https://stackoverflow.com/questions/42393259/load-scene-with-param-variable-unity
    // Update is called once per frame
    void Update()
    {
        print(""+dropdown.value);
        if (Input.GetKeyDown(KeyCode.D))
        {
            collabType = CollabType.CoupledView;
            dropdown.value = (int)collabType;
        }
        collabType = (CollabType) dropdown.value;
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                Debug.Log("still running!");
            }
            else
            {
                GoToScene();
            }
        }
    }
}
