using UnityEngine;

public class Enermyscripts : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected float speed;



    [Header("Mana Enemy")]

    [SerializeField] protected float manahad;


    protected float damage;

    protected float recoilTimer;

    protected Rigidbody2D rb;

    [SerializeField] protected PlayerControler player;


    // Start is called before the first frame update
    protected virtual void Start()
    {

    }
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerControler>();
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        if (health <= 0)
        {
       
            Destroy(gameObject);
         

        }
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }
    protected void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && ! PlayerControler.Instance.playerState.invincible)
        {
            Attack();
            PlayerControler.Instance.HitSlowTime(0, 5, 0.5f);
            //default thoi gian slowdown khi dinh attack
        }
    }
    protected virtual void Attack()
    {
        PlayerControler.Instance.TakeDamage(damage);
    }
}