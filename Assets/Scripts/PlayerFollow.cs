using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [SerializeField] private PlayerController pc;
    [SerializeField] private float yOffset;

    private CinemachineFramingTransposer _cmCam;
    // Start is called before the first frame update
    void Start()
    {
        _cmCam = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        _cmCam.m_TrackedObjectOffset.y = pc.GetIsGrounded ? yOffset : 0;
    }
}
