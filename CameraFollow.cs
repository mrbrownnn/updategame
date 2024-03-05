using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float followspeed = 0.1f;
    // khai bao toc do di chuyen thanh ham co the thay doi
    [SerializeField] private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, PlayerControler.Instance.transform.position + offset, followspeed);
        //function thay doi vi tri dua theo nhan vat khi di chuyen
    }
}
