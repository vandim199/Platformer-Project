using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCam : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CenterCam();
        }
    }

    public void CenterCam()
    {
        Camera.main.transform.position = gameObject.transform.position;
        Camera.main.transform.position += new Vector3(0, 0, -10);
    }
}
