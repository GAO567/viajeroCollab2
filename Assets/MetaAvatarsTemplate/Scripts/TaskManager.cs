using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{

    [SerializeField] GameObject rootObject;
    [SerializeField] GameObject userHead;
    public float angleBetween;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        calculateAngle();
    }

    void calculateAngle()
    {
        if (rootObject && userHead)
        {
            angleBetween = Vector3.Angle(rootObject.transform.forward, userHead.transform.forward);
            Debug.DrawRay(rootObject.transform.position, rootObject.transform.forward, Color.blue);
            Debug.DrawRay(userHead.transform.position, userHead.transform.forward, Color.cyan); 
        }
    }

   
}
