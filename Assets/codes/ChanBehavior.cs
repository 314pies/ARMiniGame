using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChanBehavior : MonoBehaviour
{
    public Image CreateBoxButton;
    public Image JumpButton;
    // Use this for initialization
    void Start()
    {

    }
    /// <summary>
    /// When performing some "cute" action, set to true
    /// </summary>
    public bool CuteAction = false;
    public Animator anim;
    public float JumpSpeed = 200;
    public float RunSpeed = 2.0f;
    public float RotateSpeed = 8.0f;
    public float StopDistance = 0.2f;
    public Rigidbody CharacteRigidBody;

    GameObject CreatedBox = null;
    GameObject DeleteBox = null;
    /// <summary>
    /// Last time clicked the box
    /// </summary>
    private float LastClieckedTime;

    Vector2 ClickedScreenPos;
    Vector3 RunTargetPos;
    // Update is called once per frame
    void Update()
    {
        if (CuteAction) return;

        #region Get Input

        if (IsScreenPressed())
        {
            Debug.Log("Clicked detected");
            if (IsInRect(JumpButton.rectTransform, ClickedScreenPos))
            {
                CharacteRigidBody.velocity = new Vector3(CharacteRigidBody.velocity.x, JumpSpeed, CharacteRigidBody.velocity.z);
            }
            else if (IsInRect(CreateBoxButton.rectTransform, ClickedScreenPos))
            {
                Debug.Log("Creating box");
                CreatedBox = Instantiate(BoxPrefab);
            }
            else
            {
                //Select target pos
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(ClickedScreenPos);
                if (Physics.Raycast(ray, out hit, 100000.0f))
                {
                    if (hit.collider.tag == "Player" && Grounded() && anim.GetInteger("state") == 0)
                    {
                        Debug.Log("Performing cute action");
                        StartCoroutine(PerformCuteAction());
                    }
                    else if (hit.collider.tag == "Box")
                    {
                        //If double clicked this box, delete it
                        if (DeleteBox == hit.collider.gameObject && Time.time < LastClieckedTime + 1.0f)
                        {
                            Destroy(DeleteBox);
                            DeleteBox = null;
                        }
                        else
                        {
                            DeleteBox = hit.collider.gameObject;                            
                        }
                        LastClieckedTime = Time.time;
                    }
                    else//Update target pos
                    {
                        RunTargetPos = hit.point;
                        RunTargetPos.y = 0;
                        Debug.Log(hit.point);
                    }

                }
            }
        }
        #endregion

        //A box is created
        if (CreatedBox != null)
        {
            SetScreenInputPoint();
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(ClickedScreenPos);
            if (Physics.Raycast(ray, out hit, 100000.0f))
            {
                if (hit.collider.tag != "Player")
                {
                    CreatedBox.transform.position = hit.point;
                }
            }
            if (IsScreenReleased())
            {
                Transform _boxTransform = CreatedBox.transform;
                //Change the box layer
                for (int i = 0; i < _boxTransform.childCount; i++)
                {
                    _boxTransform.GetChild(i).gameObject.layer = 0;
                }

                CreatedBox = null;

            }
        }


        if (Grounded())//Running
        {
            #region Run to target position

            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), RunTargetPos) > StopDistance)
            {
                Vector3 dir = (RunTargetPos - transform.position).normalized * RunSpeed;
                dir.y = CharacteRigidBody.velocity.y;
                CharacteRigidBody.velocity = dir;

                Quaternion targetRotation = Quaternion.LookRotation(
                   new Vector3(RunTargetPos.x, transform.position.y, RunTargetPos.z)
                   - transform.position);

                // Smoothly rotate towards the target point.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
                anim.SetInteger("state", 1);
            }
            else
            {
                CharacteRigidBody.velocity = new Vector3(0, CharacteRigidBody.velocity.y, 0);
                anim.SetInteger("state", 0);
            }
            #endregion
        }
        else//jumping
        {
            anim.SetInteger("state", 2);
        }
    }

    const float Limit = 0.001f;
    bool Grounded()
    {
        return (CharacteRigidBody.velocity.y < Limit && CharacteRigidBody.velocity.y > -Limit);
    }

    bool IsScreenPressed()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            ClickedScreenPos = Input.mousePosition;
            return true;
        }
#endif
#if UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ClickedScreenPos = Input.GetTouch(0).position;
            return true;
        }
#endif
        return false;
    }

    //Invoked to update ClickedScreenPos
    bool SetScreenInputPoint()
    {
#if UNITY_EDITOR
        ClickedScreenPos = Input.mousePosition;
        return true;
#endif
#if UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount > 0)
        {
            ClickedScreenPos = Input.GetTouch(0).position;
            return true;
        }
        return false;
#endif
    }

    bool IsScreenReleased()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            return true;
        }
#endif
#if UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
        {
            return true;
        }
#endif
        return false;
    }

    private bool IsInRect(RectTransform rect, Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint);
    }

    public GameObject BoxPrefab;

    public float CuteActiontime = 3.0f;
    public int[] CuteActionIndexs = { 3, 4, 5, 6 };
    /// <summary>
    /// To ensute that the random value wont repeat
    /// </summary>
    List<int> CuteActionIndexsRange = new List<int>();

    IEnumerator PerformCuteAction()
    {
        CuteAction = true;
        print("CC" + CuteActionIndexsRange.Count);

        //Refill
        if (CuteActionIndexsRange.Count == 0)
            foreach (int _index in CuteActionIndexs)
                CuteActionIndexsRange.Add(_index);

        int _id = Random.Range(0, CuteActionIndexsRange.Count);
        yield return new WaitForSeconds(0.1f);

        anim.SetInteger("state", CuteActionIndexsRange[_id]);
        Debug.Log(CuteActionIndexs[_id]);
        CuteActionIndexsRange.RemoveAt(_id);
        yield return new WaitForSeconds(CuteActiontime);
        CuteAction = false;
    }
}
