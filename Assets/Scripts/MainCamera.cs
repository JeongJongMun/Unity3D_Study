using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float Yaxis;
    public float Xaxis;

    public Transform target;

    public float rotSensitive = 10f;//ī�޶� ȸ�� ����
    public float RotationMin = -10f;//ī�޶� ȸ������ �ּ�
    public float RotationMax = 80f;//ī�޶� ȸ������ �ִ�
    public float smoothTime = 0.12f;//ī�޶� ȸ���ϴµ� �ɸ��� �ð�
 
    private Vector3 targetRotation;
    private Vector3 currentVel;

    public bool enableMobile = false;

    public FixedTouchField touchField;

    public float offsetX;
    public float offsetY;
    public float offsetZ;
    void LateUpdate()
    {
        if (enableMobile)
        {
            Yaxis = Yaxis + touchField.TouchDist.x * rotSensitive;
            Xaxis = Xaxis - touchField.TouchDist.y * rotSensitive;
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
        transform.eulerAngles = targetRotation;
        //SmoothDamp�� ���� �ε巯�� ī�޶� ȸ��

        Vector3 FixPedPos = new Vector3(target.transform.position.x + offsetX,
                                        target.transform.position.y + offsetY,
                                        target.transform.position.z + offsetZ);
        transform.position = FixPedPos;
        //transform.position = target.position - transform.forward * dis;
        //ī�޶��� ��ġ�� �÷��̾�� ������ ����ŭ �������ְ� ��� ����ȴ�.
    }
}
