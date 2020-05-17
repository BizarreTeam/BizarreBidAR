using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    public int value;
    private int distance;
    // Start is called before the first frame update
    void Start()
    {
        distance = 50;
    }

    // Update is called once per frame
    void Update()
    {
        //RotateOrgan
        transform.Rotate(new Vector3(0, 1, 0), 20 * Time.deltaTime);
    }

    public bool PickInput()
    {
        bool ret = false;
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            //create a ray cast and set it to the mouses cursor position in game
            Ray ray;
            if (Input.GetMouseButtonDown(0))
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            else
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                //draw invisible ray cast/vector
                Debug.DrawLine(ray.origin, hit.point);
                if (hit.collider.gameObject == this.gameObject)
                    ret = true;
            }
        }
        return ret;
    }
}
