using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System;
using TMPro;

public class BasicEnemy : HumanoidEnemy
{
    protected Player _player;
    private NavMeshAgent _navMeshAgent;
    private NoiseListener _noiseListener;

    private EnemySpawnerManager _enemySpawnerManager;

    [Header("Ranges")]
    [SerializeField]
    protected float _attackRange = 10.0f;

    [SerializeField]
    protected float _aggroRange = 40.0f;

    protected EnemyStates _states;

    private int _forwardSpeedHash = Animator.StringToHash("ForwardSpeed");
    private int _rightSpeedHash = Animator.StringToHash("RightSpeed");

    [SerializeField]
    private EnemySkills _enemySkills;

    [SerializeField]
    [Tooltip("The amount of force applied to the enemy required to knock it down")]
    private float _forceResistance = 20f;

    private EnemyLineOfSight _lineOfSight;

    private EnemyDashable _enemyDashable;

    public Action OnDamageTaken;

    private EnemyCommunicator _enemyCommunicator;

    private Vector3 _spawnerPosition;

    public override void Awake()
    {
        base.Awake();
        _lineOfSight = GetComponent<EnemyLineOfSight>();
        _noiseListener = GetComponent<NoiseListener>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyDashable = GetComponent<EnemyDashable>();
        _enemyCommunicator = GetComponent<EnemyCommunicator>();
        SetEnemySkills(_enemySkills);
    }

    public void SetEnemySkills(EnemySkills enemySkills)
    {
        _enemySkills = enemySkills;
        _navMeshAgent.speed = _enemySkills.Speed;
        _status.MaxHealth = _enemySkills.Health;
        _status.Health = _enemySkills.Health;
        _states = new EnemyStates(
            chaseState: (EnemyChaseState)
                Activator.CreateInstance(_enemySkills.ChaseStrategy, new object[] { this }),
            attackState: (EnemyAttackState)
                Activator.CreateInstance(_enemySkills.AttackStrategy, new object[] { this }),
            idleState: (EnemyIdleState)
                Activator.CreateInstance(_enemySkills.IdleStrategy, new object[] { this })
        );
    }

    public override void Start()
    {
        base.Start();
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        ChangeState(_states.IdleState);
    }

    public override void Die(int force, Vector3 hitPoint, Vector3 hitDirection)
    {
        base.Die(force, hitPoint, hitDirection);
        ChangeState(new EnemyDeadState(this));
        _enemySpawnerManager?.EnemyDied(this);
        GameManager.Instance.EnemyDied(this);
    }

    public override void Update()
    {
        base.Update();
        _state.Update();
    }

    public override void TakeDamage(
        GameObject damager,
        int damage,
        float force,
        Vector3 hitPoint,
        Vector3 hitDirection,
        Element element
    )
    {
        base.TakeDamage(damager, damage, force, hitPoint, hitDirection, element);

        OnDamageTaken?.Invoke();
        if (force >= _forceResistance && !(_state is EnemyKnockdownState))
        {
            Knockdown(force, hitPoint, hitDirection);
        }
    }

    public void Knockdown(float force, Vector3 hitPoint, Vector3 hitDirection)
    {
        ChangeState(new EnemyKnockdownState(this, force, hitPoint, hitDirection, _state));
    }

    public override void ChangeState(GenericState state)
    {
        base.ChangeState(state);
    }

    public void SetEnemySpawnerManager(EnemySpawnerManager enemySpawnerManager)
    {
        _enemySpawnerManager = enemySpawnerManager;
        _spawnerPosition = _enemySpawnerManager.transform.position;
        StartCoroutine(DontLeaveSpawner());
    }

    private IEnumerator DontLeaveSpawner()
    {
        while (true)
        {
            float distance = Vector3.Distance(_spawnerPosition, transform.position);

            if (distance > _enemySpawnerManager.SpawnRadius)
            {
                // Get random position in camp
                Vector3 randomPosition = new Vector3(
                    UnityEngine.Random.Range(-5.0f, 5.0f),
                    0.0f,
                    UnityEngine.Random.Range(-5.0f, 5.0f)
                );
                randomPosition += _spawnerPosition;
                NavMeshHit hit;
                NavMesh.SamplePosition(
                    randomPosition,
                    out hit,
                    5.0f,
                    UnityEngine.AI.NavMesh.AllAreas
                );
                ChangeState(new EnemyMoveToState(this, randomPosition));
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    //getters and setters
    public Player Player
    {
        get { return _player; }
    }
    public NavMeshAgent NavMeshAgent
    {
        get { return _navMeshAgent; }
    }

    public float AttackRange
    {
        get { return _attackRange; }
    }
    public float AggroRange
    {
        get { return _aggroRange; }
    }

    public int ForwardSpeedHash
    {
        get { return _forwardSpeedHash; }
    }

    public int RightSpeedHash
    {
        get { return _rightSpeedHash; }
    }

    public EnemyStates States
    {
        get { return _states; }
    }

    public EnemySkills EnemySkills
    {
        get { return _enemySkills; }
        set { _enemySkills = value; }
    }

    public EnemyLineOfSight LineOfSight
    {
        get { return _lineOfSight; }
    }

    public NoiseListener NoiseListener
    {
        get { return _noiseListener; }
    }

    public EnemyDashable EnemyDashable
    {
        get { return _enemyDashable; }
    }

    public EnemyCommunicator EnemyCommunicator
    {
        get { return _enemyCommunicator; }
    }
}
