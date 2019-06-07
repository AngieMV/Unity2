using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : Unit
{
    [SerializeField]
    private float _AttackCD = 1f;

    [SerializeField]
    private float _AttackRadius = 5f;

    [SerializeField]
    private LayerMask _UnitsLayerMask;

    private IEnumerator _CurrentState;
    private Outpost _TargetOutpost;
    private Unit _TargetUnit;
    private NavMeshAgent _Agent;

    protected override void UnitAwake()
    {
        _Agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        SetState(State_Idle());
    }


    private void Update()
    {
        if (IsAlive == false) return;

        _Anim.SetFloat("Vertical", _Agent.velocity.magnitude);
    }

    private void SetState(IEnumerator newState)
    {
        if (_CurrentState != null)
        {
            StopCoroutine(_CurrentState);
        }

        _CurrentState = newState;
        StartCoroutine(_CurrentState);
    }

    private IEnumerator State_Idle()
    {
        while (_TargetOutpost == null)
        {
            _TargetOutpost = Outpost.OutpostList.GetRandom();
            yield return null;
        }

        SetState(State_MovingToOutpost());
    }

    private IEnumerator State_MovingToOutpost()
    {
        _Agent.SetDestination(_TargetOutpost.transform.position);
        while (_Agent.remainingDistance > _Agent.stoppingDistance)
        {
            LookForEnemy();
            yield return null;
        }

        SetState(State_CapturingOutpost());
    }

    private IEnumerator State_CapturingOutpost()
    {
        while (_TargetOutpost.CurrentTeam != TeamNumber || _TargetOutpost.CaptureValue < 1f)
        {
            LookForEnemy();
            yield return null;
        }

        _TargetOutpost = null;
        SetState(State_Idle());
    }

    private IEnumerator State_AttackingEnemy()
    {
        _Agent.isStopped = true;
        _Agent.ResetPath();
        var shootTimer = 0f;

        while (_TargetUnit != null && _TargetUnit.IsAlive)
        {
            shootTimer += Time.deltaTime;

            transform.LookAt(_TargetUnit.transform);
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

            if(shootTimer >= _AttackCD)
            {
                shootTimer = 0f;
                ShootLasersFromEyes(_TargetUnit.transform.position + Vector3.up, _TargetUnit.transform);
            }

            yield return null;
        }

        _TargetUnit = null;
        SetState(State_Idle());
    }

    private IEnumerator State_Dead()
    {
        yield return null;
    }


    private void LookForEnemy()
    {
        var aroundMe = Physics.OverlapSphere(transform.position, _AttackRadius, _UnitsLayerMask);
        foreach (var item in aroundMe)
        {
            var otherUnit = item.GetComponent<Unit>();
            if(otherUnit != null && otherUnit.IsAlive && otherUnit.TeamNumber != TeamNumber)
            {
                _TargetUnit = otherUnit;
                SetState(State_AttackingEnemy());
                return;
            }
        }
    }


    override protected void Die()
    {
        base.Die();

        SetState(State_Dead());
        _Agent.isStopped = true;
        _Agent.ResetPath();
        _TargetOutpost = null;
    }
}
