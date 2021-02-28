using System;
using UnityEngine;

namespace Control
{
    public class ThirdCamera : MonoBehaviour
    {
        public Camera SrcCamera;
        public GameObject LockUpObject;
        public float AxisSpeed = 120f;
        public float Speed = 5f;
        public float CameraSpeed = 3f;
        public float RotateSpeed = 60f;
        public float JumpSpeed = 8f;
        public float Gravity = -9.8f;

        private Vector3 _chacatorRotateAngle;

        public void Start()
        {
            _chacatorRotateAngle = Vector3.zero;
            SrcCamera = GetComponentInChildren<Camera>();
        }
        public void Update()
        {
            UpdateCamera();
            Move();
            RotateAngle();
            CameraDistance();
        }

        private void RotateAngle()
        {
            if (_chacatorRotateAngle.magnitude > 0.001f)
            {
                Quaternion curQua = Quaternion.LookRotation(_chacatorRotateAngle, Vector3.up);
                Quaternion newQua = Quaternion.Lerp(LockUpObject.transform.rotation, curQua, Time.deltaTime * RotateSpeed);
                LockUpObject.transform.rotation = newQua;
                Vector3 vp = LockUpObject.transform.eulerAngles;
                vp.x = vp.z = 0;
                LockUpObject.transform.eulerAngles = vp;
            }
        }

        private void CameraDistance()
        {
            float value = Input.GetAxis("Mouse ScrollWheel");
            SrcCamera.transform.localPosition += Vector3.forward * value * CameraSpeed * Time.deltaTime;
        }

        private void UpdateCamera()
        {
            transform.position = LockUpObject.transform.position;

            float deltaSpeed = AxisSpeed * Time.deltaTime;
            float xAxis = Input.GetAxis("Mouse X") * deltaSpeed;
            float yAxis = Input.GetAxis("Mouse Y") * deltaSpeed;

            Vector3 euler = transform.eulerAngles;

            euler.x -= yAxis;
            euler.y += xAxis;

            euler.x = XAdjust(euler.x);
            euler.z = 0;

            transform.eulerAngles = euler;
        }

        private float _ySpeed = 0;
        private void Move()
        {
            CharacterController controller = LockUpObject.GetComponent<CharacterController>();
            Vector3 dire = Vector3.zero;
            Vector3 forward = transform.forward;
            forward.y = 0;
            dire += forward.normalized * Input.GetAxisRaw("Vertical");
            Vector3 right = transform.right;
            dire += right * Input.GetAxisRaw("Horizontal");

            Vector3 normailzedDire = dire.normalized;
            _chacatorRotateAngle = normailzedDire;
            Vector3 speedDire = normailzedDire * Time.deltaTime * Speed;

            if (controller.isGrounded)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    _ySpeed = JumpSpeed;
                }
            }
            else
            {
                _ySpeed += Gravity * Time.deltaTime;
            }

            speedDire.y = _ySpeed * Time.deltaTime;
            controller.Move(speedDire);
        }

        private float XAdjust(float x)
        {
            if (x > 270f)
            {
                return Mathf.Clamp(x, 280, 360);
            }
            else
            {
                return Mathf.Clamp(x, -80, 90);
            }
        }
    }
}
