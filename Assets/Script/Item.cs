using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin,Grenade, Heart,Weapon};  // 열거형 변수선언
    public Type type;
    public int value;

    Rigidbody rigid;
    CapsuleCollider capsuleCollider;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()  // 아이템이 더 멋나보이게 회전
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true; // 외부 물리충격에 움직이지 않는다. 고정시킴 위치를
            capsuleCollider.enabled = false;
        }

    }
}
