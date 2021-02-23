using UnityEngine;

namespace Control
{
    public class PlayerControl : MonoBehaviour
    {
        public float Speed = 1;
        public float RotateSpeed = 1;
        public void Update()
        {
            Vector3 dire = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                dire += forward;
            }

            if (Input.GetKey(KeyCode.A))
            {
                dire -= transform.right;
            }

            if (Input.GetKey(KeyCode.S))
            {
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                dire -= forward;
            }

            if (Input.GetKey(KeyCode.D))
            {
                dire += transform.right;
            }

            if (dire.magnitude > 0.5f)
            {
                dire.Normalize();
            }

            transform.position = transform.position + dire * Speed * Time.deltaTime;

            float deltaSpeed = RotateSpeed * Time.deltaTime;
            float xAxis = Input.GetAxis("Mouse X") * deltaSpeed;
            float yAxis = Input.GetAxis("Mouse Y") * deltaSpeed;
            Vector3 euler = transform.eulerAngles;
            euler.x -= yAxis;
            euler.y += xAxis;
            euler.z = 0;
            transform.eulerAngles = euler;
            CastUpdate();
        }

        public void CastUpdate()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1000, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                transform.position = hit.point + Vector3.up * 0.5f;
            }
        }
    }
}
