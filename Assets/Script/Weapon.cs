using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Start is called before the first frame update
    public enum Type { Melee, Range}
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;
    public GameObject bullet;
    public Transform CasebulletPos;
    public GameObject Casebullet;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
            
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StopCoroutine("Shot");
            StartCoroutine("Shot");

        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Swing()
    {
        // 메인함수가 실행되고 그 안에 서브루틴 순서대로 실행되는 것과 달리 코루틴은 메인함수가 실행됨과 동시에 실행
        // yield return null; // 1프레임 대기
        yield return new WaitForSeconds(0.1f); // 0.1초 딜레이
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }
    IEnumerator Shot()
    {
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
        yield return null;

        GameObject intantCaseBullet = Instantiate(Casebullet, CasebulletPos.position, CasebulletPos.rotation);
        Rigidbody CasebulletRigid = intantCaseBullet.GetComponent<Rigidbody>();
        Vector3 caseVec = CasebulletPos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        CasebulletRigid.AddForce(caseVec, ForceMode.Impulse);
        CasebulletRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}
