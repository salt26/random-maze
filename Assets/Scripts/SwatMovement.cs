using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwatMovement : MonoBehaviour
{

    public float turnSpeed = 5f;

    [HideInInspector]
    public Camera mainCamera;

    int m_AnimationState;
    Animator m_Animator;
    Vector3 m_Movement;
    Rigidbody m_Rigidbody;
    float footstepTiming = 0f;

    //Transform lookAt;
    //Transform follow;
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR
    float vertical = 0f;
#endif

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR
    private void Update()
    {
        vertical = MobileJoystick.instance.moveDirection.y;
    }
#endif

    void FixedUpdate()
    {
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
#else
        float horizontal = 0f;
#endif

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isMoving = hasHorizontalInput || hasVerticalInput;

        int horizontalState = !hasHorizontalInput ? 1 : (horizontal > 0 ? 2 : 0);
        int verticalState = !hasVerticalInput ? 1 : (vertical > 0 ? 2 : 0);
        m_AnimationState = verticalState * 3 + horizontalState;

#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
        bool isSprinting = (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)) && m_AnimationState == 7;
        
        if (isMoving && isSprinting) GameController.gc.SetShiftPressed();
#else
        bool isSprinting = MobileButton.instance.HasPressed && 
            vertical > 0f && m_AnimationState == 7;

        if (isMoving)
            MobileJoystick.instance.SetCircleColor(new Color(1f, 0.8666667f, 0f));
        else
            MobileJoystick.instance.SetCircleColor(new Color(1f, 1f, 1f));

        if (isSprinting)
            MobileButton.instance.SetCircleColor(new Color(1f, 0.06666667f, 0f));
        else
            MobileButton.instance.SetCircleColor(new Color(1f, 1f, 1f));

        GameController.gc.virtualCamera.GetComponent<Transform>().Rotate(
            new Vector3(0f, 1f, 0f) * 2.2f
            * Mathf.Pow(MobileJoystick.instance.moveDirection.x, 2) * Mathf.Sign(MobileJoystick.instance.moveDirection.x));
#endif

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
            int index = UnityEngine.Random.Range(0, GameController.gc.footsteps.Count - 1);
            GetComponent<AudioSource>().PlayOneShot(GameController.gc.footsteps[index]);
            GameController.gc.footsteps.Add(GameController.gc.footsteps[index]);
            GameController.gc.footsteps.RemoveAt(index);
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
