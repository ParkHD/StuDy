using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mashobj;
    public GameObject effectobj;
    public Rigidbody rigid;

    void Start()
    {
        StartCoroutine(Explosion());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        mashobj.SetActive(false);
        effectobj.SetActive(true);

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15,
            Vector3.up, 0f, LayerMask.GetMask("Enemy"));
        //반구체모양의 레이케스트(위치, 반지름, 쏘는방향(상관없음), 길이는0 )

        foreach(RaycastHit hitobj in rayHits)//rayHits안에 있는 데이터를 RaycastHit형식으로 하나씩 가져온다
        {
            hitobj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
        Destroy(gameObject, 5);
    }
}
