using Mirror;
using UnityEngine;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        Rigidbody rigidbody;
        public float mouseSensitivity = 100f;
        public float speed = 10f;
        public float jumpForce = 10f;
        public Transform playerHead;
        float xRot = 0f, yRot = 0f;

        public override void OnStartLocalPlayer()
        {
            Camera.main.transform.SetParent(playerHead.transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            rigidbody = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (!isLocalPlayer) { return; }

            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

            xRot -= mouseY;
            xRot = Mathf.Clamp(xRot, -90f, 90f);

            yRot -= mouseX;

            playerHead.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
            transform.localRotation = Quaternion.Euler(0f, -yRot, 0f);
            playerHead.Rotate(Vector3.up * mouseX);

            if (Input.GetKeyDown("space"))
                rigidbody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }

        void FixedUpdate()
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            rigidbody.MovePosition(transform.position + move * Time.deltaTime * speed);
        }
    }
}