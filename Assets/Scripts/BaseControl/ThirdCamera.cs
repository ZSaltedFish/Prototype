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
        private float _curDistance;

        public void Start()
        {
            _chacatorRotateAngle = Vector3.zero;
            SrcCamera = GetComponentInChildren<Camera>();
            _curDistance = -SrcCamera.transform.localPosition.z;
        }
        public void Update()
        {
            Move();
            RotateAngle();
        }

        public void LateUpdate()
        {
            CameraDistance();
            UpdateCamera();
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
            _curDistance += value * CameraSpeed * Time.deltaTime;
            SrcCamera.transform.localPosition = Vector3.forward * _curDistance;
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

            Ray ray = new Ray(LockUpObject.transform.position, -SrcCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, -_curDistance, 1 << 8))
            {
                if (hit.collider.gameObject != SrcCamera.gameObject)
                {
                    float dist = Vector3.Distance(hit.point, LockUpObject.transform.position) - 0.05f;
                    Vector3 p = new Vector3(0, 0, -dist);
                    SrcCamera.transform.localPosition = p;
                    SrcCamera.transform.LookAt(LockUpObject.transform.position);
                }
            }
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
                if (Input.GetButton("Jump"))
                {
                    _ySpeed = Mathf.Max(_ySpeed, 0);
                }
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
