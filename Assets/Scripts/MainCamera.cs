using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float Yaxis;
    public float Xaxis;

    public Transform target;

    private float rotSensitive = 3f;//ī�޶� ȸ�� ����
    private float dis = 2f;//ī�޶�� �÷��̾������ �Ÿ�
    private float RotationMin = -10f;//ī�޶� ȸ������ �ּ�
    private float RotationMax = 80f;//ī�޶� ȸ������ �ִ�
    private float smoothTime = 0.12f;//ī�޶� ȸ���ϴµ� �ɸ��� �ð�
 
    private Vector3 targetRotation;
    private Vector3 currentVel;

    public bool enableMobile = false;

    public FixedTouchField touchField;

    void LateUpdate()
    {
        if (enableMobile)
        {
            Yaxis = Yaxis + touchField.TouchDist.x * rotSensitive;
            Xaxis = Xaxis + touchField.TouchDist.y * rotSensitive;
        }
        else
        {
            Yaxis = Yaxis + Input.GetAxis("Mouse X") * rotSensitive;
            Xaxis = Xaxis - Input.GetAxis("Mouse Y") * rotSensitive;
        }

        //Xaxis�� ���콺�� �Ʒ��� ������(�������� �Է� �޾�����) ���� �������� ī�޶� �Ʒ��� ȸ���Ѵ� 

        Xaxis = Mathf.Clamp(Xaxis, RotationMin, RotationMax);
        //X��ȸ���� �Ѱ�ġ�� �����ʰ� �������ش�.

        targetRotation = Vector3.SmoothDamp(targetRotation, new Vector3(Xaxis, Yaxis), ref currentVel, smoothTime);
        this.transform.eulerAngles = targetRotation;
        //SmoothDamp�� ���� �ε巯�� ī�޶� ȸ��

        transform.position = target.position - transform.forward * dis;
        //ī�޶��� ��ġ�� �÷��̾�� ������ ����ŭ �������ְ� ��� ����ȴ�.
    }
}
