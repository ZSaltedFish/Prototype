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
            SrcAnim = GetComponentInParent<Animator>();
        }

        public void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                SrcAnim.SetTrigger("Attack");
            }

            if (Input.GetButtonDown("Fire2"))
            {
                SrcAnim.SetTrigger("HaverAttack");
            }
        }
        public void OnAttackBegin()
        {
            _attackBegin = true;
        }

        public void OnAttackEnd()
        {
            _attackBegin = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (_attackBegin)
            {
                if (Effect != null)
                {
                    Effect.transform.position = other.ClosestPoint(transform.position);
                    Effect.Play();
                }
            }

        }
    }
}
