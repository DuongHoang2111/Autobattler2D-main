using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    public HealthBar barPrefab;
    public SpriteRenderer spriteRender;
    public Animator animator;

    public int baseDamage = 1;
    public int baseHealth = 3;
    [Range(1, 5)]
    public int range = 1;
    public float attackSpeed = 100f; // Attacks per second
    public float movementSpeed = 100f; // Attacks per second

    protected Team myTeam;
    protected BaseEntity currentTarget = null;
    protected Node currentNode;

    public Node CurrentNode => currentNode;

    protected bool HasEnemy => currentTarget != null;
    protected bool IsInRange => currentTarget != null && Vector3.Distance(this.transform.position, currentTarget.transform.position) <= range;
    protected bool moving;
    protected Node destination;
    protected HealthBar healthbar;

    protected bool dead = false;
    protected bool canAttack = true;
    protected float waitBetweenAttack;

    public void Setup(Team team, Node currentNode)
    {
        myTeam = team;
        if (myTeam == Team.Team2)
        {
            spriteRender.flipX = true;
        }

        this.currentNode = currentNode;
        transform.position = currentNode.worldPosition;
        currentNode.SetOccupied(true);

        healthbar = Instantiate(barPrefab, this.transform);
        healthbar.Setup(this.transform, baseHealth);
    }

    protected void Start()
    {
        GameManager.Instance.OnRoundStart += OnRoundStart;
        GameManager.Instance.OnRoundEnd += OnRoundEnd;
        GameManager.Instance.OnUnitDied += OnUnitDied;
    }

    protected virtual void OnRoundStart() { }
    protected virtual void OnRoundEnd() { }
    protected virtual void OnUnitDied(BaseEntity diedUnity) { }

    protected void FindTarget()
    {
        var allEnemies = GameManager.Instance.GetEntitiesAgainst(myTeam);

        // Lấy vị trí hiện tại ở main thread
        Vector3 currentPosition = transform.position;

        // Lấy trước danh sách vị trí của các kẻ địch ở main thread
        var enemyPositions = allEnemies.Select(e => new {
            Enemy = e,
            Position = e.transform.position
        }).ToList();

        // Chạy song song (không dùng trực tiếp transform trong parallel)
        var closestEnemyData = enemyPositions
            .AsParallel()
            .Select(data => new
            {
                Enemy = data.Enemy,
                Distance = Vector3.Distance(currentPosition, data.Position)
            })
            .OrderBy(data => data.Distance)
            .FirstOrDefault();

        currentTarget = closestEnemyData?.Enemy;
    }

    protected bool MoveTowards(Node nextNode)
    {
        Vector3 direction = (nextNode.worldPosition - this.transform.position);
        if (direction.sqrMagnitude <= 0.005f)
        {
            transform.position = nextNode.worldPosition;
            animator.SetBool("walking", false);
            return true;
        }
        animator.SetBool("walking", true);

        this.transform.position += direction.normalized * movementSpeed * Time.deltaTime;
        return false;
    }

    protected void GetInRange()
    {
        if (currentTarget == null)
            return;

        if (!moving)
        {
            destination = null;
            List<Node> candidates = GridManager.Instance.GetNodesCloseTo(currentTarget.CurrentNode);
            candidates = candidates.OrderBy(x => Vector3.Distance(x.worldPosition, this.transform.position)).ToList();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!candidates[i].IsOccupied)
                {
                    destination = candidates[i];
                    break;
                }
            }
            if (destination == null)
                return;

            // Tìm đường đi từ currentNode đến destination
            var path = GridManager.Instance.GetPath(currentNode, destination);
            if (path == null || path.Count < 2)
                return;

            // Di chuyển đến node tiếp theo trong path
            destination = path[1]; // Đi tới node đầu tiên sau currentNode trong đường đi ngắn nhất
            destination.SetOccupied(true);
        }

        moving = !MoveTowards(destination);
        if (!moving)
        {
            // Giải phóng node trước đó
            currentNode.SetOccupied(false);
            SetCurrentNode(destination);
        }
    }

    public void SetCurrentNode(Node node)
    {
        currentNode = node;
    }

    public void TakeDamage(int amount)
    {
        baseHealth -= amount;
        healthbar.UpdateBar(baseHealth);

        if (baseHealth <= 0 && !dead)
        {
            dead = true;
            currentNode.SetOccupied(false);
            GameManager.Instance.UnitDead(this);
        }
    }

    protected virtual void Attack()
    {
        if (!canAttack)
            return;

        animator.SetTrigger("attack");

        waitBetweenAttack = 1 / attackSpeed;
        StartCoroutine(WaitCoroutine());
    }

    IEnumerator WaitCoroutine()
    {
        canAttack = false;
        yield return null;
        animator.ResetTrigger("attack");
        yield return new WaitForSeconds(waitBetweenAttack);
        canAttack = true;
    }
}
