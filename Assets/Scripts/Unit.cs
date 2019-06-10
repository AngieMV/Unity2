using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class Unit : MonoBehaviour
{
	public bool IsAlive { get; protected set; } = true;
	public int TeamNumber = 0;

	[SerializeField]
	protected float _Health = 100f;

    [SerializeField]
    protected float _DangerThreshold = 35f;

    [SerializeField]
	protected float _AttackDamage = 8f;

	[SerializeField]
    private Laser _LaserPrefab;

    protected Animator _Anim;
	protected Eye[] _Eyes;
	

    protected abstract void UnitAwake();

	protected void Awake()
	{
		_Anim = GetComponent<Animator>();
		_Eyes = GetComponentsInChildren<Eye>();
		foreach (var item in GetComponentsInChildren<Renderer>())
		{
			item.material.color = GameManager.Instance.TeamColors[TeamNumber];
		}
		UnitAwake();
	}

	protected bool CanSee(Vector3 hitPos, Transform other)
	{
		foreach (var eye in _Eyes)
		{
			var startPos = eye.transform.position;
			var dir = hitPos - startPos;
			var ray = new Ray(startPos, dir);
			if(Physics.Raycast(ray, out var hit))
			{
				if(hit.transform == other)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected void ShootLasersFromEyes(Vector3 hitPos, Transform other)
	{
		foreach (var eye in _Eyes)
		{
			Instantiate(_LaserPrefab).Shoot(eye.transform.position, hitPos);
		}

		var otherUnit = other.GetComponent<Unit>();
		if(otherUnit != null && otherUnit.TeamNumber != TeamNumber)
		{
			otherUnit.TakeDamage(_AttackDamage);
		}
	}

    public void TakeDamage(float damage)
    {
		_Health -= damage;
		if(_Health <= 0f)
		{
			_Health = 0f;
			Die();
		}
    }

    protected void Recover(float health) {
        _Health += health;
        Debug.Log("Recovering " + _Health);
    }

    protected bool IsInDanger() {
        Debug.Log("Is in Danger " + _Health);
        return _Health < _DangerThreshold;
    }

	protected virtual void Die()
	{
		IsAlive = false;
		_Anim.SetTrigger("IsDead");
	}
}
