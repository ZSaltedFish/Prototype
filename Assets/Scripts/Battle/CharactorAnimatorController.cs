using UnityEngine;

namespace Control
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class CharactorAnimatorController : MonoBehaviour
    {
        private Animator _anim;
        private CharacterController _body;

        public float CurSpeed;
        public void Start()
        {
            _anim = GetComponent<Animator>();
            _body = GetComponent<CharacterController>();
        }

        public void Update()
        {
            CurSpeed = _body.velocity.magnitude;
            _anim.SetFloat("Speed", CurSpeed);
        }
    }
}
