using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : Unit
{
    [SerializeField]
    private float _AttackCD = 1f;

    /// <summary>
    /// Recovering rate (cooldown).
    /// </summary>
    [SerializeField]
    private float _RecoverCD = 0.75f;

    [SerializeField]
    private float _AttackRadius = 5f;

    /// <summary>
    /// Defines a radius where the <see cref="AIController"/> will stop attacking the <see cref="_TargetUnit"/>.
    /// </summary>
    [SerializeField]
    private float _ScapeRadius = 10f;

    [SerializeField]
    private LayerMask _UnitsLayerMask;

    private IEnumerator _CurrentState;
    private Outpost _TargetOutpost;
    private Unit _TargetUnit;
    private NavMeshAgent _Agent;

    [SerializeField]
    private GameObject _Human;

    [SerializeField]
    private GameObject _Wolf;

    /// <summary>
    /// Set a reference to the <see cref="AIController"/> to move in the scene when in <see cref="State_Danger"/>.
    /// </summary>
    [SerializeField]
    private Transform _RecoverPosition;

    protected override void UnitAwake()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _Anim = _Human.GetComponent<Animator>();
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
        TransformInWolf(false);
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

    /// <summary>
    /// When the <see cref="AIController"/> captures a outpost, it celebrates dancing.
    /// </summary>
    /// <returns></returns>
    private IEnumerator State_CapturingOutpost()
    {
        while (_TargetOutpost.CurrentTeam != TeamNumber || _TargetOutpost.CaptureValue < 1f)
        {
            LookForEnemy();
            yield return null;
        }
        _Anim.SetTrigger("Celebrate");
        _TargetOutpost = null;
        SetState(State_Idle());
    }

    /// <summary>
    /// Defines a new state. If the <see cref="AIController"/> is in danger. It will move to the health flag, while recovering itself.
    /// </summary>
    /// <returns><see cref="IEnumerator"/></returns>
    private IEnumerator State_Danger()
    {
        TransformInWolf(true);

        float recoverTimer = 0f;
        _Agent.SetDestination(_RecoverPosition.transform.position);

        while (_Agent.pathPending || _Agent.remainingDistance > _Agent.stoppingDistance)
        {
            recoverTimer += Time.deltaTime;
            transform.LookAt(_RecoverPosition.transform.position);

            if (recoverTimer >= _RecoverCD)
            {
                recoverTimer = 0f;
                Recover(5);
            }
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

        while (_TargetUnit != null && _TargetUnit.IsAlive && IsInRange() && !IsInDanger())
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

        if (IsInDanger())
        {
            SetState(State_Danger());
        }
        else {
            _TargetUnit = null;
            SetState(State_Idle());
        }
    }

    /// <summary>
    /// Determine when <see cref="_TargetUnit"/> is in range <see cref="_ScapeRadius"/> to continue attacking it.
    /// </summary>
    /// <returns>true if enemy is in range.</returns>
    private bool IsInRange()
    {
        float distance = Vector3.Distance(this.gameObject.transform.position, _TargetUnit.transform.position);
        return  distance < _ScapeRadius;
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

    /// <summary>
    /// Shapeshift this <see cref="AIController"/>.
    /// </summary>
    /// <param name="isTransformingInWolf">true if converting it into a wolf, false to converting it into a human.</param>
    private void TransformInWolf(bool isTransformingInWolf)
    {
        _Wolf.SetActive(isTransformingInWolf);
        _Human.SetActive(!isTransformingInWolf);
    }
}
