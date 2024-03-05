using UnityEngine;

public class Zombie : Enermyscripts
{
    // Start is called before the first frame update
    void Start()
    {
        rb.gravityScale = 12f;
    }
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!isRecoiling)
        {
            transform.position = Vector2.MoveTowards
                (transform.position, new Vector2(PlayerControler.Instance.transform.position.x, transform.position.y), speed * Time.deltaTime);
        // no problem
        }
    }
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        PlayerControler.Instance.TakeDamage(_damageDone);
     }
}
//su dung tinh ke thua de viet len chuong trinh enermyscripts
//su dung override de viet de len lop virtual