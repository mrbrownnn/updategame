
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using static UnityEngine.Coroutine;



public class PlayerControler : MonoBehaviour
{
    //define layer for animation
    [Header("Horizontal Movement Settings")]

    private Rigidbody2D rb;
    private Animator anim;
    private float xAxis, yAxis;
    private float gravity;
    [SerializeField] private float Walkspeed = 1;// default setting walkspeed=1

    [Space(7)]


    /// ////////////////////////////////////////////////////////////


    [Header(" Settings Air Jump and Buffer Jump")]

    private int JumpBufferCounter;


    [SerializeField] int JumpBufferFrame;

    [SerializeField] private int AirJumpMax;// khai bao so buoc nhay tren khong toi da co the thuc hien

    private int airJumpcounter = 0; // dem so jump tren khong

    [SerializeField] private float JumpForceZ = 45;
    [Space(7)]


    /////////////////////////////////////////////////////////////////


    [Header("Ground Checking")]

    [SerializeField] private Transform GroundCheck;
    [SerializeField] private float groundcheckY = 0.2f;
    [SerializeField] private float groundcheckX = 0.5f;
    [SerializeField] private LayerMask Whatground;
    [Space(7)]


    /////////////////////////////////////////////////////////////////////

    [Header("Health Settings")]
    [SerializeField] public int health;
    [SerializeField] public int maxHealth;
    [SerializeField] public GameObject bloodspurt;






    /// //////////////////////////////////////////////////////////////


    [Header("Dashing Settings")]

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    private bool canDash = true;
    private bool Dashed = false;
    [SerializeField] public GameObject dashEffect;
    [Space(7)]


    [Header("Attacking Settings")]

    private bool attack = false;
    private double timetbweenattack, timesinceattack;


    [SerializeField] public GameObject AttackEffect;
    [SerializeField] private Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] private Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] public LayerMask attackableLayer;
    [SerializeField] private float damage;
    [SerializeField] public GameObject slashEffect;
    [Space(7)]


    ///////////////////////////////////////////////////////// 
    


    [Header("Recoil Settings")]

    [SerializeField] private float recoilXSteps = 5;
    [SerializeField] private float recoilYSteps = 5;

    [SerializeField] private float recoilspeedX = 100;
    [SerializeField] private float recoilspeedY = 100;

    private float stepsXRecoiled, stepsYRecoiled;
    [Space(7)]


    [Header("Mana settings")]
    [SerializeField] protected int manaMax;
    [SerializeField] protected int manacurrent;
    [SerializeField] protected int manaCount;

    public static PlayerControler Instance;
    public PlayerSecondary playerState;


    bool restoreTime;
    float restoreTimeSpeed;
    //main code




    private void Awake()
    {
        if (gameObject != null && Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        Health = maxHealth;
    }


    void Start()
    {
        playerState = GetComponent<PlayerSecondary>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravity = rb.gravityScale;

    }


    void OnDrawGizmos()
    // xac dinh pham vi tan cong
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

    }


    void Update()
    {
        GetInputs();

        UpdateJump();

        if (playerState.dashing)
        {
            return;
        }
        flip();

        Move();

        Jump();

        StartDash();

        Attack();

        RestoreTimeScale();

    }
    private void FixedUpdate()
    {
        if(playerState.dashing) { return; }

        Recoil();
    }



    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");

        yAxis = Input.GetAxisRaw("Vertical");

        attack = Input.GetButtonDown("Attack");

        //Setting cac nut su dung chuc nang
    }


    void flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y); // Set X-axis to -1 for left-facing
            
            playerState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y); // Set X-axis to 1 for right-facing

            playerState.lookingRight = true;
        }
        // su dung de quay nhan vat khi dung nut di chuyen left right
    }


    void Move()
    {
        rb.velocity = new Vector2(Walkspeed * xAxis, rb.velocity.y);

        flip();
        //lenh move left right
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
        //tao conditional
    }


    private void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !Dashed)
        {
            StartCoroutine(Dash());

            Dashed = true;
        }
        if (Grounded())
        {
            Dashed = false;
        }
        //kiem tra dieu kien de dash 
    }
    private IEnumerator Dash()
    {
        canDash = false;

        playerState.dashing = true;

        if (playerState.dashing)
        {
            anim.SetTrigger("Dashing");
        }
        rb.gravityScale = 0;

        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);

        if (dashEffect != null && Grounded())
        {

            Instantiate(dashEffect, transform);
        }

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = gravity;

        playerState.dashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
        // them chuc nang delay khi dung dash, setting trigger cho animation tao conditional de chuyen hieu ung
    }

    void Attack()
    {
        timesinceattack = (timesinceattack + 0.5) + Time.deltaTime;
        timetbweenattack = 0.2;
        //tao hieu ung delay khi spam attack

        if (attack && timesinceattack >= timetbweenattack)
        {
            timesinceattack = 0;


            anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {

                Hit(SideAttackTransform, SideAttackArea, ref playerState.recoilingX, recoilspeedX);

                Instantiate(slashEffect, SideAttackTransform);

            }

            else if (yAxis > 0)
            {


                Hit(UpAttackTransform, UpAttackArea, ref playerState.recoilingY, recoilspeedY);

                SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
                




            }

            else if (yAxis < 0 && !Grounded())
            {

                Hit(DownAttackTransform, DownAttackArea, ref playerState.recoilingY, recoilspeedY);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);

            }

            // them truc yAxis, neu compare va lua chon vung hit theo vi tri yAxis
            //hold up and down and press Z to attack up and down
        }
    }
    //problem solved !!!
    //problem solved !!!!!!!!!
    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {

        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enermyscripts>() != null)
            {
                objectsToHit[i].GetComponent<Enermyscripts>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
                //update recoil when attack
            }
        }
    }
    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (restoreTime)
            {
                if(Time.timeScale < 1)
                {
                    Time.timeScale += Time.deltaTime * restoreTimeSpeed;
                }
                else
                {
                    Time.timeScale = 1;
                    restoreTime = false;
                }
            }
        }
    }

    public void HitSlowTime( float _newTimescale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimescale;
        if( _delay > 0 )
        {
            StartCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }
    IEnumerator StartTimeAgain (float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }
    public void TakeDamage(float _damage)

    {
        Health -= Mathf.RoundToInt(_damage);

        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        playerState.invincible = true;
        GameObject _bloodspurtParticle = Instantiate(bloodspurt, transform.position, quaternion.identity);
        Destroy(_bloodspurtParticle, 1.5f);
        // them hieu ung khi nhan sat thuong

        anim.SetTrigger("TakeDamage");

     

        yield return new WaitForSeconds(1f);

        playerState.invincible = false;
        
    }
    public int Health
    { get { return health; } 
        set
        {
            if( health != value){

                health = Mathf.Clamp(value, 0, maxHealth);
            }
        }
    }
    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }


    public bool Grounded()

    {
        if (Physics2D.Raycast(GroundCheck.position, Vector2.down, groundcheckY, Whatground)
            || Physics2D.Raycast(GroundCheck.position + new Vector3(groundcheckX, 0, 0), Vector2.down, groundcheckY, Whatground)
            || Physics2D.Raycast(GroundCheck.position + new Vector3(-groundcheckX, 0, 0), Vector2.down, groundcheckY, Whatground))
        {
            return true;
        }
        else
        {
            return false;
        }

        // funncion ktra player co cham dat hay khong dua vao raycast la he oxy duoc dat duoi chan player
        // neu raycast va cham voi layer whatground tra ve true, tuc la cham dat
        // nguoc lai tra ve false
        // vector3 use to check funcion grounded()
    }


     void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);


            playerState.jumping = false;

            // tuy chinh toc do nhay, ngan chan gia toc khi nha nut space
        }
        if (!playerState.jumping)
        {

            if (JumpBufferCounter > 0 && Grounded())
            {

                rb.velocity = new Vector3(rb.velocity.x, JumpForceZ);

                playerState.jumping = true;

            }
            else if (!Grounded() && airJumpcounter < AirJumpMax && Input.GetButton("Jump"))
            {
                playerState.jumping = true;

                airJumpcounter++;

                rb.velocity = new Vector3(rb.velocity.x, JumpForceZ);


                // neu k mat dat, so jump nho hon so jump max va an nut jump, co the nhay lan nua (kiem tra dieu kien de jump)
                // hoan thanh bien airjumpcounter ++
            }
        }
        anim.SetBool("Jumping", !Grounded());
        // tao conditional cho animation jump
    }
    
     void UpdateJump()

    //su dung de goi ham update buoc nhay
    // su dung de tang do chinh xac cho cu nhay, su dung de tranh quan tinh do di chuyen walking
    //ham frame cho phep sai so bang so khung hinh toi da co the sai
    {
        if (Grounded())
        {
            playerState.jumping = false;// neu tren mat dat thi dat bang 0
            airJumpcounter = 0;
        }
        if (Input.GetButtonDown("Jump"))
        {
            JumpBufferCounter = JumpBufferFrame;
        }
        else
        {
            JumpBufferCounter--;
        }
    }


    void Recoil()
    {
        if (playerState.recoilingX)
        {
            if (playerState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilspeedX, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilspeedX, 0);
            }
        }


        if (playerState.recoilingY)
        {
            rb.gravityScale = 0;

            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilspeedY);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilspeedY);
            }
            airJumpcounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }
        if (playerState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (playerState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
        // tao hieu ung day lui nhan vat khi attack vao quai vat
    }


    void StopRecoilX()
    {
        stepsXRecoiled = 0;

        playerState.recoilingX = false;
    }


    void StopRecoilY()
    {
        stepsYRecoiled = 0;

        playerState.recoilingY = false;
    }
    // kiem tra dieu kien nhay de sau do xac dinh so luong frame recoil de stop, tranh hien tuong recoil bay ra ngoai man hinh
  
}



