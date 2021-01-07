using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    float fireDelay;

    public GameObject[] weapons; // 무기1,2,3
    public bool[] hasWeapons; // 무기1,2,3 획득했는지 
    public GameObject[] grenades;
    public int hasGrenades;
    public Camera followCamera;
    public GameObject grenade; //수류탄 프리팹

    Animator animator;
    Rigidbody rigidbody;
    GameObject nearObject;
    Weapon equipWeapon; // 현재 손에 들고있는 weapon
    MeshRenderer[] meshs; //피격효과를 만들기위함 캐릭터가 많은MeshRenderer을 가지고있어서 배열형태 

    int equipWeaponIndex = -1;

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool rDown;
    bool gDown; // 수류탄 투척

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isFireReady = true;
    bool isBoarder;
    bool isReload=false;
    bool isDamage; //플레이어 쳐맞는거 방지


    Vector3 moveVec;
    Vector3 dodgeVec;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();// 모두가져오기위해 GetComponents에0 s붙음
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        Move();
        Jump();
        Dodge();
        Interation();
        Swap();

        Turn();
        Attack();
        Reload();
        Grenade();//수류탄 투척
    
    }
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk"); // 뛰는 키 
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        fDown = Input.GetButton("Fire1");
        rDown = Input.GetButtonDown("Reload");
        gDown = Input.GetButtonDown("Fire2");
    }
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge) // 닷지중에 
        {
            moveVec = dodgeVec;
        }
        if (isSwap || !isFireReady || isReload) // 스왑 or 총을쏠때 or 장전중일떄 moveVec = 0
        {
            moveVec = Vector3.zero;
        }

        if(!isBoarder) //벽에 충돌하면 앞으로 못가게 하기위함
            transform.position += moveVec * speed * (wDown ? 1f : 0.3f) * Time.deltaTime; 

        animator.SetBool("isWalk", moveVec != Vector3.zero); // moveVec가 제로벡터가 아니면 -> 걷고있으면 isWalk = true
        animator.SetBool("isRun", wDown && moveVec != Vector3.zero); // moveVec 가 제로벡터가 아니고 wDown키가 눌려있으면 isRun = true

        transform.LookAt(transform.position + moveVec); // 가는방향쪽 보기
    }
    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap) // 점프키 and 움직이지 않고 and 점프중이 아니고 and 닷지중이 아닐떄 and  스왑중이 아닐떄
        {
            rigidbody.AddForce(Vector3.up * 15, ForceMode.Impulse); // ForceMode.Impulse = 순간적인 힘
            animator.SetBool("isJump", true); // 
            animator.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Dodge()
    {
        if (jDown && moveVec !=Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            dodgeVec = moveVec; // 가던방향을 dodgeVec에 저장
            speed *= 2;
            animator.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut",0.4f); // 0.4초 딜레이후에 DodgeOut실행
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) // 가지고있지 않는다 or 이미 손에 들고있을때 
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isDodge && !isJump)
        {
            if(equipWeapon != null) // 현재 가지고있는 weapon 비활성화
            {
                equipWeapon.gameObject.SetActive(false);   
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>(); // 
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }

    void Attack()
    {
        if (equipWeapon == null) // 장착한 weapon이 없을떄 return
        {
            return;
        }
        fireDelay += Time.deltaTime; // fireDelay 
        isFireReady = equipWeapon.rate < fireDelay; // fireDelay(마지막으로 attack하고 경과시간)가 equipWeapon.rate(공격속도)보다 클때 공격가능

        if(fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }
    void Reload()
    {
        if (equipWeapon == null || equipWeapon.type == Weapon.Type.Melee || ammo ==0)
        {
            return;
        }
        if(rDown && !isJump &&!isDodge && !isSwap && isFireReady)
        {
            animator.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2f);

        }
    }
    void ReloadOut() // 수정해야할듯??? 총알 남아있을 시에 장전 오류??
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo; // 넣을 탄 개수
        equipWeapon.curAmmo = equipWeapon.maxAmmo;
        ammo -= reAmmo;
        isReload = false;

    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);

        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // 마우스 포인트로 Ray쏘기
        RaycastHit rayHit;
        if (fDown || gDown)
        {
            if (Physics.Raycast(ray, out rayHit, 100))// ray에 닿은 값 rayHit에 저장
                // Raycast : ray를 쏘아서 닿는 오브젝트 감지하는 함수
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // 높은곳을 클릭하면 높은곳으로 바라봄 -> 땅만 보게 y=0
                transform.LookAt(transform.position + nextVec);
            }
        }
    }
    void Grenade()
    {
        if(hasGrenades == 0)
        {
            return;
        }
        if(gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))// ray에 닿은 값 rayHit에 저장
                                                      // Raycast : ray를 쏘아서 닿는 오브젝트 감지하는 함수
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 2; // 높게 던지기 위함
                //transform.LookAt(transform.position + nextVec);

                GameObject instantGrenade = Instantiate(grenade, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }
    void FreezeRotation()
    {
        rigidbody.angularVelocity = Vector3.zero; //회전속도를 0으로 만듬 -> 외부충격으로 회전 방지
    }
    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);//시작위치 쏘는방향 색
        isBoarder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
            //쏘는곳 방향 길이 
            // LayerMask가 wall이면 true로 바뀜
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag =="Floor")
        {
            animator.SetBool("isJump", false);
            isJump = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxHasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;

                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet") // Enemy한테 피격시
        {
            if (!isDamage) // 맞고있지 않을 때 -> 무한으로 피격 방지
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                
                if(other.GetComponent<Rigidbody>() != null)
                    Destroy(other.gameObject);

                StartCoroutine(OnDamage());
            }
        }
    }
    IEnumerator OnDamage()
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs) // 캐릭터가 많은 renderer를 가지고있어서 
        {
            mesh.material.color = Color.yellow;
        }
        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

    }
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
        //Debug.Log(nearObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}
