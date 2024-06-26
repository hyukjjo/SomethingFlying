using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField]
    private LaunchTarget _bear;

    [SerializeField]
    private float _launchForce = 0f; // 발사하는 힘의 크기
    [SerializeField]
    private float _launchAngle = 0f; // 발사하는 각도

    private Vector3 _bearOriginPos;

    private void Awake()
    {
        _bearOriginPos = _bear.transform.position;
    }

    public void SetLaunchAngleValue(float angle)
    {
        _launchAngle = angle;
    }

    public void SetLaunchForceValue(float force)
    {
        _launchForce = force;
    }

    public void LaunchBear()
    {
        if (_bear != null)
        {
            // 각도를 라디안으로 변환
            float angleInRadians = _launchAngle * Mathf.Deg2Rad;

            // 방향 벡터 계산
            Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)).normalized;

            // _bear의 Rigidbody 컴포넌트 가져오기
            Rigidbody2D bearRigidbody = _bear.GetComponent<Rigidbody2D>();

            if (bearRigidbody != null)
            {
                // 현재 속도를 초기화하고 새로운 힘 적용
                bearRigidbody.velocity = Vector2.zero;
                bearRigidbody.AddForce(direction * _launchForce * _launchForce, ForceMode2D.Impulse);
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
        _bear.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        _bear.transform.position = _bearOriginPos;
        _bear.transform.rotation = Quaternion.identity;
        _launchAngle = 0f;
        _launchForce = 0f;
    }
}