using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField]
    private LaunchObject _launchObject;

    [SerializeField]
    private float _launchForce = 0f; // 발사하는 힘의 크기
    [SerializeField]
    private float _launchAngle = 0f; // 발사하는 각도

    private Vector3 _bearOriginPos;

    private void Awake()
    {
        _bearOriginPos = _launchObject.transform.position;
    }

    public Transform GetLaunchObject()
    {
        return _launchObject.transform;
    }

    public void SetLaunchAngleValue(float angle)
    {
        _launchAngle = angle;
    }

    public void SetLaunchForceValue(float force)
    {
        _launchForce = force;
    }

    public void Launch()
    {
        if (_launchObject != null)
        {
            // 각도를 라디안으로 변환
            float angleInRadians = _launchAngle * Mathf.Deg2Rad;

            // 방향 벡터 계산
            Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)).normalized;

            // _bear의 Rigidbody 컴포넌트 가져오기
            Rigidbody2D rigidbody = _launchObject.GetComponent<Rigidbody2D>();

            if (rigidbody != null)
            {
                // 현재 속도를 초기화하고 새로운 힘 적용
                rigidbody.isKinematic = false;
                rigidbody.velocity = Vector2.zero;
                rigidbody.AddForce(direction * _launchForce * _launchForce, ForceMode2D.Impulse);
            }
            else
            {
                Debug.LogError("Rigidbody2D component is missing on the ShootingTarget.");
            }
        }
        else
        {
            Debug.LogError("ShootingTarget (_bear) is not assigned.");
        }
    }

    public void ResetLauncher()
    {
        _launchObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        _launchObject.transform.position = _bearOriginPos;
        _launchObject.transform.rotation = Quaternion.identity;
        _launchAngle = 0f;
        _launchForce = 0f;
    }
}