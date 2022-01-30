using Mirror;
using System;
using UnityEngine;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        public TextMesh playerNameText;
        public GameObject floatingInfo;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnTeamChanged))]
        public bool playerTeam;

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }

        void OnTeamChanged(bool _Old, bool _New)
        {
            if (playerTeam)
            {
                playerNameText.color = Color.red;
                goodModel.SetActive(false);
                badModel.SetActive(true);
            }
            else
            {
                playerNameText.color = Color.blue;
                goodModel.SetActive(true);
                badModel.SetActive(false);
            }
        }

        public AudioClip spellNoise;

        Rigidbody rigidbody;
        public float mouseSensitivity = 100f;
        public float speed = 10f;
        public float jumpForce = 10f;
        public Transform playerHead;
        float xRot = 0f, yRot = 0f;
        public MeshRenderer goodMesh, badMesh;
        public GameObject goodModel, badModel;

        public GameObject particleCaster;

        public bool team;

        public override void OnStartLocalPlayer()
        {
            goodMesh.gameObject.SetActive(false);
            badMesh.gameObject.SetActive(false);
            Camera.main.transform.SetParent(playerHead.transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            rigidbody = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
            floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            string name = Environment.UserName;

            bool team = UnityEngine.Random.Range(0, 2) == 1;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            int good = 0, bad = 0;

            foreach (GameObject player in players)
            {
                if (player.GetComponent<PlayerScript>().playerTeam)
                    good++;
                else
                    bad++;
            }

            if (good > bad)
                team = false;
            else
                team = true;

            CmdSetupPlayer(name, team);
        }

        [Command]
        public void CmdSetupPlayer(string _name, bool _team)
        {
            // player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerTeam = _team;

            if (_team)
            {
                goodModel.SetActive(false);
                badModel.SetActive(true);
            }
            else
            {
                goodModel.SetActive(true);
                badModel.SetActive(false);
            }
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                floatingInfo.transform.LookAt(Camera.main.transform);
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

            RaycastHit hit;

            if (Input.GetKeyDown("space"))
                if (Physics.Raycast(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.down) * 1000, out hit, 1f))
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

            if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d"))
                GetComponent<AudioSource>().volume = 0.2f;
            else
                GetComponent<AudioSource>().volume = 0f;

            rigidbody.MovePosition(transform.position + move * Time.deltaTime * speed);

            if (Input.GetMouseButton(0))
                CmdShootRay();
        }

        [Command]
        void CmdShootRay()
        {
            RpcFireWeapon();
        }

        [ClientRpc]
        void RpcFireWeapon()
        {
            if (!particleCaster.GetComponent<ParticleSystem>().isPlaying)
            {
                RaycastHit hit;

                Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * 1000, Color.red, 1f);

                particleCaster.GetComponent<ParticleSystem>().Play();

                AudioSource.PlayClipAtPoint(spellNoise, transform.position, 1);

                if (Physics.Raycast(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * 1000, out hit, 20f))
                {
                    Debug.Log(gameObject.name + " - HIT - " + hit.transform.gameObject.name);

                    if (hit.transform.gameObject.tag == "Player" && hit.transform.gameObject != gameObject)
                    {
                        if (playerTeam != hit.transform.gameObject.GetComponent<PlayerScript>().playerTeam)
                        {
                            GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
                            hit.transform.position = spawners[UnityEngine.Random.Range(0, spawners.Length)].transform.position;
                        }
                    }
                }
            }

        }
    }
}