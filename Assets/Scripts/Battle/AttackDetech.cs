using UnityEngine;

namespace Control
{
    public class AttackDetech : MonoBehaviour
    {
        public Animator SrcAnim;
        public Collider Weapon;
        public ParticleSystem Effect;
        private bool _attackBegin = false;

        public void Start()
        {
            SrcAnim = GetComponent<Animator>();
        }

        public void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                SrcAnim.SetTrigger("Attack");
            }
        }
        public void OnAttackBegin()
        {
            Debug.Log($"开始攻击");
            Weapon.enabled = true;
            _attackBegin = true;
        }

        public void OnAttackEnd()
        {
            Debug.Log($"结束攻击");
            Weapon.enabled = false;
            _attackBegin = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (_attackBegin)
            {
                Debug.Log($"造成伤害{other.gameObject.name}");
                if (Effect != null)
                {
                    Effect.transform.position = other.ClosestPoint(transform.position);
                    Effect.Play();
                }
            }
            else
            {
                Debug.Log($"碰撞到:{other.gameObject.name}");
            }

        }
    }
}
