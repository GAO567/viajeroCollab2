using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class Countdown : MonoBehaviour
{
    float timeToStartScene = 10.0f;
    [SerializeField]
    float timeRemaining = 10.0f;

    [SerializeField]
    int id = 0;

    [SerializeField]
    CollabType collabType = CollabType.FacetoFaceIntersect;

    bool timerIsRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        timerIsRunning = true;
        
    }

    //https://stackoverflow.com/questions/42393259/load-scene-with-param-variable-unity
    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                Debug.Log("still running!");
            }
            else
            {
                Debug.Log("Time has run out!");

                SceneManager.LoadScene("drawCube", LoadSceneMode.Single);
                //
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }
}
