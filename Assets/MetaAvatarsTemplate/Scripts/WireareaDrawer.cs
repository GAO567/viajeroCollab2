using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireareaDrawer : MonoBehaviour
{
    [SerializeField] Vector3 BoundsSize;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Vector3 stuff;
    [SerializeField] float viewportDepth;

    GameObject topRightGO;
    GameObject topLeftGO;
    GameObject bottomRightGO;
    GameObject bottomLeftGO;

    // Start is called before the first frame update
    void Start()
    {
        if (lineRenderer)
            lineRenderer.positionCount = 8;// (16);

       
    }

    // Update is called once per frame
    void Update()
    {
        Camera _camera = Camera.main;

        
        float sizeX = BoundsSize.x;
        float sizeY = BoundsSize.y;
        float sizeZ = BoundsSize.z;
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
