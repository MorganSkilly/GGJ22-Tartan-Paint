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
        public MeshRenderer goodMesh, badMesh;

        public GameObject particleCaster;

        public override void OnStartLocalPlayer()
        {
            goodMesh.gameObject.SetActive(false);
            badMesh.gameObject.SetActive(false);
            Camera.main.transform.SetParent(playerHead.transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            rigidbody = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (!isLocalPlayer)
            { 
                return; 
            }

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
            if (!isLocalPlayer)
            {            
                return;
            }

            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            rigidbody.MovePosition(transform.position + move * Time.deltaTime * speed);

            if (Input.GetMouseButton(0))
                CmdShootRay();

            /*RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.gameObject.GetComponent<PlayerScript>().isActiveAndEnabled)
                        hit.transform.gameObject.GetComponent<PlayerScript>().CmdUpdatePlayerHealth(health - 10);
                }
            }
            else
            {
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            }*/
        }

        [Command]
        void CmdShootRay()
        {
            RpcFireWeapon();
        }

        [ClientRpc]
        void RpcFireWeapon()
        {
            RaycastHit hit;

            Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * 1000, Color.red);

            particleCaster.GetComponent<ParticleSystem>().Play();

            if (Physics.Raycast(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * 1000, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.tag == "Player" && hit.transform.gameObject != gameObject)
                {
                    //if (hit.transform.gameObject.GetComponent<PlayerScript>().isActiveAndEnabled)
                    //    hit.transform.gameObject.GetComponent<PlayerScript>().health -= 10;

                    Debug.Log(gameObject.name + " - HIT - " + hit.transform.gameObject.name);

                    hit.transform.position = Vector3.zero;
                    //Destroy(hit.transform.gameObject);
                    //hit.transform.gameObject.GetComponent<HealthTracker>().health -= 10;
                }
            }
        }
    }
}