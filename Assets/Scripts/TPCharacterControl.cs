﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TPCharacter))]
public class TPCharacterControl : MonoBehaviour {

    private TPCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
    private bool m_Sprint;
    private GameObject m_Target;

    public GameObject EmoteReloadQuad;
    public GameObject EmoteStunnedQuad;
    public GameObject EmoteSprintingQuad;
    public GameObject EmoteTiredQuad;

    private Animation m_Animation;
    public float m_AttackCooldown = 1.0f;
    Vector3 originalCamPos;
    Quaternion originalCamRot;
    private float m_AttackCooldownCount = 0.0f;

    public float SprintCooldown;
    public float SprintDuration;
    public bool isMonster = false;
    public GameObject WaveSpawner;
    public float BaseSpawnInterval = 0.5f;
    public float StunTime = 2f;
    private float elapsedTime = 0;
    private float sprintTime = 0;
    private float sprintCooldownTime = 5.0f;
    private float elapsedStunTime = 0;
    private bool isStunned = false;

    public float m_CameraLockOnSpeed = 5.0f;

    private GameObject currentEmote = null;
    private GameObject currentEmoteQuad = null;

    private GameObject secondaryEmote = null;
    private GameObject secondaryEmoteQuad = null;

    private float multiEmoteTimer = 0.0f;
    private float multiEmoteDelay = 1f;

    int stunEmoteIdx = -1;
    int sprintEmoteIdx = -1;
    int tiredEmoteIdx = -1;
    int reloadEmoteIdx = -1;

    int stunIdx = 0;
    int sprintIdx = 1;
    int tiredIdx = 2;
    int reloadIdx = 3;

    int currentActiveIndex = 0;

    GameObject[] emotes = { null, null, null, null };

    // Use this for initialization
    void Start () {
        m_Character = GetComponentInChildren<TPCharacter>();
        m_Animation = GetComponentInChildren<Animation>();
        m_Cam = Camera.main.transform;
        originalCamPos = m_Cam.localPosition;
        originalCamRot = m_Cam.localRotation;
    }
	
	// Update is called once per frame
	void Update () {
        elapsedTime += Time.deltaTime;
        var speed = GetComponent<Rigidbody>().velocity.magnitude;
        //print(speed);
        if (elapsedTime > BaseSpawnInterval && speed > 0.1)
        {
            var spawnerPos = new Vector3(transform.position.x, 1, transform.position.z);
            var spawner = Instantiate(WaveSpawner, spawnerPos, new Quaternion(0, 0, 0, 0)) as GameObject;

            var behaviour = spawner.GetComponent<SpawnerBehaviour>();
            behaviour.WaveTimeToLive += behaviour.WaveTimeToLive * speed * 0.5f;
            behaviour.WaveExpandRate += behaviour.WaveExpandRate * speed * 0.02f;
            elapsedTime = 0;
        }

        if(!GetComponent<ShooterBehaviour>().HasBullet)
        {
            if (reloadEmoteIdx == -1)
            {
                GameObject emote = (GameObject)Instantiate(EmoteReloadQuad, transform);
                emote.transform.parent = transform;
                emote.transform.localPosition = new Vector3(0, 6.1f, 4.44f);
                emotes[reloadIdx] = emote;
                reloadEmoteIdx = reloadIdx;
            }
            else
            {
                Color c = emotes[reloadEmoteIdx].GetComponent<SpriteRenderer>().color;
                c = new Color(c.r, c.g, c.b,1- (GetComponent<ShooterBehaviour>().elapsedTime / GetComponent<ShooterBehaviour>().ReloadTime));
                emotes[reloadEmoteIdx].GetComponent<SpriteRenderer>().color = c;
            }
        }
        else
        {
            Destroy(emotes[reloadIdx]);
            reloadEmoteIdx = -1;
        }

        if(multiEmoteTimer > multiEmoteDelay)
        {
            int intCounts = 0;
            if (stunEmoteIdx != -1)
                intCounts++;
            if (sprintEmoteIdx != -1)
                intCounts++;
            if (tiredEmoteIdx != -1)
                intCounts++;
            if (reloadEmoteIdx != -1)
                intCounts++;

            print(intCounts);
            if(intCounts > 1)
            {
                currentActiveIndex++;
                if (currentActiveIndex > emotes.Length - 1)
                    currentActiveIndex = 0;
                while (emotes[currentActiveIndex] == null)
                {
                    intCounts = 0;
                    if (stunEmoteIdx != -1)
                        intCounts++;
                    if (sprintEmoteIdx != -1)
                        intCounts++;
                    if (tiredEmoteIdx != -1)
                        intCounts++;
                    if (reloadEmoteIdx != -1)
                        intCounts++;

                    if (intCounts == 0)
                        break;

                    if (emotes[currentActiveIndex] == null)
                        currentActiveIndex++;
                    else
                        break;
                    if (currentActiveIndex > emotes.Length - 1)
                        currentActiveIndex = 0;
                }
                if (currentActiveIndex > emotes.Length - 1)
                    currentActiveIndex = 0;
                for (int i = 0; i < emotes.Length; i++)
                {
                    if (i != currentActiveIndex && emotes[i] != null)
                        emotes[i].SetActive(false);
                    else
                    {
                        if (emotes[i] != null)
                            emotes[i].SetActive(true);
                    }
                }
            }
            multiEmoteTimer = 0;
        }
        else
        {
            multiEmoteTimer += Time.deltaTime;
        }

        if (isStunned)
        {
            if (stunEmoteIdx == -1)
            {
                GameObject emote = (GameObject)Instantiate(EmoteStunnedQuad, transform);
                emote.transform.parent = transform;
                emote.transform.localPosition = new Vector3(0, 6.1f, 4.44f);
                emotes[stunIdx] = emote;
                stunEmoteIdx = stunIdx;
            }
            else
            {
                Color c = emotes[stunEmoteIdx].GetComponent<SpriteRenderer>().color;
                c = new Color(c.r, c.g, c.b, (elapsedStunTime / StunTime));
                emotes[stunEmoteIdx].GetComponent<SpriteRenderer>().color = c;
            }

            elapsedStunTime += Time.deltaTime;

            if (elapsedStunTime > StunTime)
            {
                Destroy(emotes[stunIdx]);
                stunEmoteIdx = -1;
                Destroy(currentEmoteQuad);
                currentEmoteQuad = null;
                currentEmote = null;
                isStunned = false;
                elapsedStunTime = 0;
            }
        }

        if (!m_Jump)
        {
            m_Jump = Input.GetButtonDown("Jump");
        }

        if(m_Sprint)
        {
            sprintTime += Time.deltaTime;

            if(sprintTime > SprintDuration)
            {
                m_Sprint = false;
                sprintTime = 0;
            }
        }
        else
        {
            if (tiredEmoteIdx == -1)
            {
                GameObject emote = (GameObject)Instantiate(EmoteTiredQuad, transform);
                emote.transform.parent = transform;
                emote.transform.localPosition = new Vector3(0, 6.1f, 4.44f);
                emotes[tiredIdx] = emote;
                tiredEmoteIdx = tiredIdx;
            }
            else
            {
                Color c1 = emotes[tiredEmoteIdx].GetComponent<SpriteRenderer>().color;
                c1 = new Color(c1.r, c1.g, c1.b, (SprintCooldown/sprintCooldownTime));
                emotes[tiredEmoteIdx].GetComponent<SpriteRenderer>().color = c1;
            }
            
            sprintCooldownTime += Time.deltaTime;
        }

        if (Input.GetButtonDown("Sprint") && !isMonster && !isStunned && SprintCooldown > sprintCooldownTime)
        {
            if (sprintEmoteIdx == -1)
            {
                GameObject emote = (GameObject)Instantiate(EmoteSprintingQuad, transform);
                emote.transform.parent = transform;
                emote.transform.localPosition = new Vector3(0, 6.1f, 4.44f);
                emotes[sprintIdx] = emote;
                sprintEmoteIdx = sprintIdx;
            }
            Destroy(emotes[tiredIdx]);
            tiredEmoteIdx = -1;
            m_Sprint = true;
            sprintCooldownTime = 0;
            Destroy(currentEmoteQuad);
            currentEmote = null;
            currentEmoteQuad = null;
        }
        else if(Input.GetButtonUp("Sprint") && !isMonster)
        {
            sprintEmoteIdx = -1;
            if (m_Sprint)
                sprintCooldownTime = 0;
            m_Sprint = false;
        }

        if (m_Target)
        {
            if((m_Target.transform.position - transform.position).magnitude>5.0f)
            {
                m_Target = null;
            }
            else
            {
                Vector3 hwPoint = (m_Target.transform.position - transform.position) /2;
                hwPoint.z *= -1;
                hwPoint = RotateX(hwPoint, -40);
                m_Cam.localPosition = Vector3.Slerp(m_Cam.localPosition, originalCamPos + hwPoint, Time.deltaTime * m_CameraLockOnSpeed);
            }
        }
        else
        {
            m_Cam.localPosition = Vector3.Lerp(m_Cam.localPosition, originalCamPos, Time.deltaTime * m_CameraLockOnSpeed);
        }
    }


    Vector3 RotateX(Vector3 v, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float ty = v.y;
        float tz = v.z;
        v.y = (cos * ty) - (sin * tz);
        v.z = (cos * tz) + (sin * ty);

        return v;
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        if(Input.GetButtonDown("Fire3"))
        {

            if (Camera.main != null)
            {
                RaycastHit hitInfo = new RaycastHit();
                if (Input.GetKeyDown(KeyCode.Mouse2))
                {
                    print("Mouse Wheel Click!");

                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
                    {
                        Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                        if (hitInfo.transform.gameObject.tag == "NPC")      //TODO REPLACE WITH TABLE
                        {
                            if(hitInfo.transform.gameObject.Equals(m_Target))
                            {
                                m_Target = null;
                            }
                            else
                            {
                                m_Target = hitInfo.transform.gameObject;
                            }
                        }
                        else
                        {
                            m_Target = null;
                        }
                    }
                    else
                    {
                        m_Target = null;
                        Debug.Log("No hit");
                    }
                    Debug.Log("Mouse is down");
                }
                else if (Input.GetKeyDown(KeyCode.Period))
                {
                    // Cast a sphere wrapping character controller 10 meters forward
                    // to see if it is about to hit anything.
                    RaycastHit[] hList = Physics.SphereCastAll(transform.position, 10, transform.forward, 10);
                    RaycastHit bestTarget = new RaycastHit();
                    float closestDistanceSqr = Mathf.Infinity;
                    Vector3 currentPosition = transform.position;
                    bool npcFound = false;
                    foreach (RaycastHit h in hList)
                    {
                        Vector3 directionToTarget = h.transform.position - currentPosition;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < closestDistanceSqr && h.transform.gameObject.tag.ToString().Equals("NPC"))
                        {
                            npcFound = true;
                            closestDistanceSqr = dSqrToTarget;
                            bestTarget = h;
                        }

                        hitInfo = bestTarget;
                    }

                    if (npcFound && hitInfo.transform.gameObject.tag.ToString().Equals("NPC"))      //TODO REPLACE WITH TABLE
                    {
                        if (hitInfo.transform.gameObject.Equals(m_Target))
                        {
                            m_Target = null;
                        }
                        else
                        {
                            m_Target = hitInfo.transform.gameObject;
                        }
                    }
                    else
                    {
                        print("No targets found");
                        m_Target = null;
                    }
                }
            }
        }

        {
            // read inputs
            float h = Input.GetAxis("Horizontal") * 5f;
            float v = Input.GetAxis("Vertical") * 5f;
            bool crouch = Input.GetKey(KeyCode.C);

            if(isMonster)
            {
                h = Input.GetAxis("Horizontal_Mnst") * 5f;
                v = Input.GetAxis("Vertical_Mnst") * 5f;
            }

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward * -1 + h * Vector3.right * -1;
            }
            //print(m_Move);
            // pass all parameters to the character control script

            if(!isStunned)
            {
                m_Character.Move(m_Move, crouch, m_Jump, m_Sprint, m_Target);
                if (h != 0 || v != 0)
                {
                    if (m_Target != null)
                    {
                        //TODO play cautious animation
                        m_Animation.Play("Walk");
                    }
                    else
                    {
                        m_Animation.Play("Walk");
                    }
                }
                else
                {
                    if (m_Target != null)
                    {
                        //TODO play cautious animation
                        m_Animation.Play("Wait");
                    }
                    else
                    {
                        m_Animation.Play("Wait");
                    }
                }
            }
            
            m_Jump = false;
        }
    }

    IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        yield break;
    }


    void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Scream") && !isMonster)
        {
            isStunned = true;
            elapsedStunTime = 0;
        }
    }
}
