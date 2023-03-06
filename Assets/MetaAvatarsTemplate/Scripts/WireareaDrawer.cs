using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireareaDrawer : MonoBehaviour
{
    [SerializeField] Vector3 BoundsSize;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Vector3 stuff;
    [SerializeField] float viewportDepth;
    [SerializeField] GameObject objToCollide;

    GameObject topRightGO;
    GameObject topLeftGO;
    GameObject bottomRightGO;
    GameObject bottomLeftGO;

    

    TaskManager taskManager;
    private float timeWhenCollisionStarted;

    public Vector3 BoundsSize1 { get => BoundsSize; set => BoundsSize = value; }
    float startTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (lineRenderer)
            lineRenderer.positionCount = 8;// (16);
        taskManager = GameObject.Find("TaskManager").GetComponent<TaskManager>();
        
    }

    public void checkCollision()
    {
        if (objToCollide)
        {
            Vector3 headPosP2Local = this.transform.InverseTransformPoint(objToCollide.transform.position);
            headPosP2Local = new Vector3(Mathf.Abs(headPosP2Local.x), Mathf.Abs(headPosP2Local.y), Mathf.Abs(headPosP2Local.z));

            if (headPosP2Local.x > BoundsSize.x / 2.0f || headPosP2Local.y > BoundsSize.y / 2.0f || headPosP2Local.z > BoundsSize.z / 2.0f)
            {
                print("im here mf");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        timeWhenCollisionStarted = Time.realtimeSinceStartup;

        startTime = Time.realtimeSinceStartup;

        if (taskManager )
        {
            //taskManager.incrementBoundaryViolations();
            //taskManager.collisionStarted(this.Id, collider.gameObject.name, timeWhenCollisionStarted);

            // tTask.incrementCollisions(this.Id,collider.gameObject.name);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //print("stay");
    }

    private void OnTriggerExit(Collider other)
    {
        if(taskManager)
            taskManager.incrementTimeOutsideBounds(Time.realtimeSinceStartup - startTime);
    }

    // Update is called once per frame
    void Update()
    {
        Camera _camera = Camera.main;
        float sizeX = BoundsSize1.x;
        float sizeY = BoundsSize1.y;
        float sizeZ = BoundsSize1.z;
        if (lineRenderer)
        {
            print("entrou aqui = " + sizeX + "," + sizeY + "," + sizeZ + ")");
            //bottom Cube
            lineRenderer.positionCount = 18;
            lineRenderer.SetPosition(0, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(1, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(2, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(3, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(4, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));

            lineRenderer.SetPosition(5, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(6, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(7, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));

            lineRenderer.SetPosition(8, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(9, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(10, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));

            lineRenderer.SetPosition(11, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(12, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(13, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(14, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(15, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));

            lineRenderer.SetPosition(16, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(17, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            
            /*
            arrayStuff[0] = new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f);
            lineRenderer.positionCount = 23;
            //bottom cube
            lineRenderer.SetPosition(0, transform.TransformPoint( new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(1, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(2, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(3, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(4, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            //uo Cube
            lineRenderer.SetPosition(5, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(6, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(7, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(8, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(9, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));


            /*lineRenderer.SetPosition(4, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(5, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(6, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(7, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            

            //front Cube

            
            //right CUbe
            lineRenderer.SetPosition(10, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(11, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(12, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(13, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));

            //left Cube
            lineRenderer.SetPosition(14, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(15, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(16, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f)));
            lineRenderer.SetPosition(17, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));

            //rightCube


            //backCube 
            lineRenderer.SetPosition(18, transform.TransformPoint(new Vector3(-sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(19, transform.TransformPoint(new Vector3(-sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(20, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(21, transform.TransformPoint(new Vector3(sizeX / 2.0f, sizeY / 2.0f, -sizeZ / 2.0f)));
            lineRenderer.SetPosition(22, transform.TransformPoint(new Vector3(sizeX / 2.0f, -sizeY / 2.0f, -sizeZ / 2.0f)));

    */

        }
    }
}
