using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEntity : BaseEntity
{
    protected override void OnRoundStart()
    {
        if (GameManager.Instance.IsFighting)
        {
            FindTarget();
        }
    }

    public void Update()
    {
        if (!GameManager.Instance.IsFighting)
            return; // Không chạy logic nếu chưa bắt đầu chiến đấu

        if (!HasEnemy)
        {
            FindTarget();
        }

        if (IsInRange && !moving)
        {
            // Trong tầm tấn công
            if (canAttack)
            {
                Attack();
                currentTarget.TakeDamage(baseDamage);
            }
        }
        else
        {
            GetInRange();
        }
    }
}
