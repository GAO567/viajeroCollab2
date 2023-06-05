using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using TMPro;

public class InitSceneLoader : MonoBehaviour
{
    float timeToStartScene = 10.0f;
    %[SerializeField]
    float timeRemaining = 10000.0f;

    [SerializeField]
    int id = 0;

    [SerializeField]
    CollabType collabType = CollabType.FacetoFaceIntersect;

    [SerializeField]
    int avatarId = 0;

    [SerializeField]
    bool isRemote = false;

    [SerializeField]
    TMP_Dropdown userIdDropdown;

    [SerializeField]
    TMP_Dropdown avatarIdDropdown;

    [SerializeField]
    TMP_Dropdown collabTypeDropdown;

    [SerializeField]
    TMP_Dropdown userTypeDropdown;


    bool timerIsRunning = false;

    [SerializeField]
    UnityEngine.Video.VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        timerIsRunning = false;
        collabTypeDropdown.value = (int)collabType;
        userIdDropdown.value = id -1;
        avatarIdDropdown.value = avatarId-1;

        if (isRemote)
            userTypeDropdown.value = 1;
        else
            userTypeDropdown.value = 0;
        //inputFIeld.text = id.ToString();
    }

    public void GoToScene()
    {
        Debug.Log("Time has run out! loading scene");
        GlobalVariables.Set("userId", userIdDropdown.value +1);
        GlobalVariables.Set("collabType", collabTypeDropdown.value);
        GlobalVariables.Set("cameFromVideoScene", 1);
        SceneManager.LoadScene("NetworkScene", LoadSceneMode.Single);
        GlobalVariables.Set("remote", userTypeDropdown.value);

        GlobalVariables.Set("avatarId", avatarIdDropdown.value+1);




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
        print(""+collabTypeDropdown.value);
        if (Input.GetKeyDown(KeyCode.D))
        {
            collabType = CollabType.CoupledView;
            collabTypeDropdown.value = (int)collabType;
        }
        collabType = (CollabType) collabTypeDropdown.value;
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
