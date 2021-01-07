using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
    public enum Type { A, B, C};
    public Type enemyType;

    public int maxHealth; 
    public int curHealth; // 현재 체력
    public Transform target;
    public BoxCollider meleeArea; // 공격범위
    public GameObject bullet; //원거리미니언 총알

    public bool isChase;
    public bool isAttack;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;



    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;// 메테리얼 접근
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }
    void Start()
    {

    }

    void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();

    }
    // Update is called once per frame
    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position); // 목표설정
            nav.isStopped = !isChase;
        }

    }
    void OnTriggerEnter(Collider other) // 피격 시
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec, false));
        }
    }
    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.color = Color.white;
        }
        else // die
        {
            mat.color = Color.gray;
            gameObject.layer = 15;

            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

            if (isGrenade) // Grenade피격시 구분
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }

            Destroy(gameObject, 3);
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; //회전속도를 0으로 만듬 -> 외부충격으로 회전 방지
        }

    }
    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Targeting()
    {
        float targetRadius = 0;
        float targetRange = 0; //감지 

        switch (enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
            case Type.B:
                targetRadius = 1f;
                targetRange = 12f;
                break;
            case Type.C:
                targetRadius = 0.5f; //두꼐(얇을수록 정확도 상승)
                targetRange = 25f; //길이
                break;
        }

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius,
            transform.forward,
            //나아가는 방향(레이캐스트 쏘는방향)
            targetRange, LayerMask.GetMask("Player"));
        //범위

        if (rayHits.Length > 0 && !isAttack)//공격 조건 : 범위에 플레이어가 잡힐때, 공격중이 아닐때
        {
            StartCoroutine(Attack());
        }
    }
    IEnumerator Attack()
    {
        isChase = false;     //공격전 추격 정지
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:

                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true; // 공격범위 활성화
                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;
                yield return new WaitForSeconds(1f);
                break;
            case Type.B:

                yield return new WaitForSeconds(0.2f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true; // 공격범위 활성화
                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position+Vector3.up, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity=transform.forward*20;
                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;     
        isAttack = false;
        anim.SetBool("isAttack", false);

    }
}
