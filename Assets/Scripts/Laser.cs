using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _LifeSpan = 0.05f;

    public void Shoot(Vector3 startPos, Vector3 endPos)
    {
        var line = GetComponent<LineRenderer>();
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
        Destroy(gameObject, _LifeSpan);
    }

}
