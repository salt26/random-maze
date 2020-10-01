using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwatMovement : MonoBehaviour
{

    public float turnSpeed = 5f;
    public List<AudioClip> footsteps;

    [HideInInspector]
    public Camera mainCamera;

    int m_AnimationState;
    Animator m_Animator;
    Vector3 m_Movement;
    Rigidbody m_Rigidbody;
    float footstepTiming = 0f;

    //Transform lookAt;
    Transform follow;
    float vertical = 0f;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        follow = GameController.gc.virtualCamera.Follow;

        /*
        GameObject g = new GameObject("LookAt");
        lookAt = g.transform;
        lookAt.SetParent(this.transform);
        lookAt.position = follow.position + new Vector3(0f, 0f, 2f);
        GameController.gc.virtualCamera.LookAt = lookAt;
        */
    }

    private void Update()
    {
        vertical = MobileJoystick.instance.moveDirection.y;

        /*
        if (!Mathf.Approximately(MobileJoystick.instance.moveDirection.x, 0f))
        {
            lookAt.position = new Vector3(follow.position.x, lookAt.localPosition.y, follow.position.z);
            lookAt.localPosition += new Vector3(
                MobileJoystick.instance.moveDirection.x * 0.5f,
                0f,
                2f);
                //Mathf.Sqrt(Mathf.Max(1f - Mathf.Pow(MobileJoystick.instance.moveDirection.x, 2), 0f)) + 1f);
        }
        else
        {
            lookAt.position = new Vector3(follow.position.x, lookAt.localPosition.y, follow.position.z);
            lookAt.localPosition += new Vector3(0f, 0f, 2f);
        }
        */
    }

    void FixedUpdate()
    {
        /*
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        */
        float horizontal = 0f;

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isMoving = hasHorizontalInput || hasVerticalInput;

        int horizontalState = !hasHorizontalInput ? 1 : (horizontal > 0 ? 2 : 0);
        int verticalState = !hasVerticalInput ? 1 : (vertical > 0 ? 2 : 0);
        m_AnimationState = verticalState * 3 + horizontalState;
        // bool isSprinting = (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)) && m_AnimationState == 7;
        bool isSprinting = Vector2.SqrMagnitude(MobileJoystick.instance.moveDirection) >= 0.99f && 
            MobileJoystick.instance.moveDirection.y > 0f && m_AnimationState == 7;

        if (isSprinting)
            MobileJoystick.instance.SetCircleColor(new Color(1f, 0.06666667f, 0f));
        else if (isMoving)
            MobileJoystick.instance.SetCircleColor(new Color(1f, 0.8666667f, 0f));
        else
            MobileJoystick.instance.SetCircleColor(new Color(1f, 1f, 1f));

        //if (isMoving && isSprinting) GameController.gc.SetShiftPressed();

        //m_Movement.Set(horizontal, 0f, vertical);
        //m_Movement.Normalize();

        GameController.gc.virtualCamera.GetComponent<Transform>().Rotate(
            new Vector3(0f, 1f, 0f) * 4f
            * Mathf.Pow(MobileJoystick.instance.moveDirection.x, 2) * Mathf.Sign(MobileJoystick.instance.moveDirection.x));

        float angle = Mathf.Deg2Rad * mainCamera.transform.eulerAngles.y;
        m_Movement = Vector3.RotateTowards(transform.forward, 
            new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)), turnSpeed * Time.fixedDeltaTime, 0f);

        float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, mainCamera.transform.eulerAngles.y);
        bool needTurnLeft = deltaAngle < -30;
        bool needTurnRight = deltaAngle > 30;

        if (isMoving)
        {
            if (isSprinting) footstepTiming += Time.fixedDeltaTime / 0.38f;
            else footstepTiming += Time.fixedDeltaTime / 0.49f;
        }

        if (footstepTiming > 1f)
        {
            footstepTiming = 0f;
            int index = UnityEngine.Random.Range(0, footsteps.Count - 1);
            GetComponent<AudioSource>().PlayOneShot(footsteps[index]);
            footsteps.Add(footsteps[index]);
            footsteps.RemoveAt(index);
        }

        m_Animator.SetInteger("AnimationState", m_AnimationState);
        m_Animator.SetBool("IsSprinting", isSprinting);
        m_Animator.SetBool("NeedTurnLeft", needTurnLeft);
        m_Animator.SetBool("NeedTurnRight", needTurnRight);
    }

    void OnAnimatorMove()
    {
        m_Rigidbody.MovePosition(m_Animator.rootPosition);
        if (m_AnimationState == 4)
        {
            m_Rigidbody.MoveRotation(m_Animator.rootRotation);
        }
        else
        {
            m_Rigidbody.MoveRotation(Quaternion.LookRotation(m_Movement));
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        m_Animator.SetLookAtWeight(.7f);
        m_Animator.SetLookAtPosition(transform.position + mainCamera.transform.forward * 1000f);
        //m_Animator.SetIKRotation(AvatarIKGoal.Left, Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0));
    }
}
