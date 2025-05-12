using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameManager : Manager<GameManager>
{
    public EntitiesDatabaseSO entitiesDatabase;
    public PerformanceLogger perfLogger;

    public Transform team1Parent;
    public Transform team2Parent;

    public Action OnRoundStart;
    public Action OnRoundEnd;
    public Action<BaseEntity> OnUnitDied;

    List<BaseEntity> team1Entities = new List<BaseEntity>();
    List<BaseEntity> team2Entities = new List<BaseEntity>();

    int unitsPerTeam = 120;

    private bool isFighting = false; // Biến kiểm soát trạng thái chiến đấu

    public bool IsFighting => isFighting; // Getter để các entity truy cập

    public void OnEntityBought(EntitiesDatabaseSO.EntityData entityData)
    {
        BaseEntity newEntity = Instantiate(entityData.prefab, team1Parent);
        newEntity.gameObject.name = entityData.name;
        team1Entities.Add(newEntity);

        newEntity.Setup(Team.Team1, GridManager.Instance.GetFreeNode(Team.Team1));
    }

    public List<BaseEntity> GetEntitiesAgainst(Team against)
    {
        if (against == Team.Team1)
            return team2Entities;
        else
            return team1Entities;
    }

    public void UnitDead(BaseEntity entity)
    {
        team1Entities.Remove(entity);
        team2Entities.Remove(entity);

        OnUnitDied?.Invoke(entity);

        Destroy(entity.gameObject);
    }


    // Hàm để thêm 8 kẻ địch mỗi lần bấm
    public void AddEnemies()
    {
        if (team2Entities.Count >= unitsPerTeam)
        {
            Debug.Log("Team 2 đã đủ " + unitsPerTeam + " đơn vị!");
            return;
        }

        int enemiesToAdd = Mathf.Min(8, unitsPerTeam - team2Entities.Count); // Thêm tối đa 8 hoặc số còn lại
        int addedCount = 0;

        for (int i = 0; i < enemiesToAdd; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, entitiesDatabase.allEntities.Count);
            BaseEntity newEntity = Instantiate(entitiesDatabase.allEntities[randomIndex].prefab, team2Parent);
            team2Entities.Add(newEntity);

            Node freeNode = GridManager.Instance.GetFreeNode(Team.Team2);
            if (freeNode == null)
            {
                Debug.LogError("Không còn node trống cho Team 2! Đã thêm " + addedCount + " kẻ địch.");
                Destroy(newEntity.gameObject);
                team2Entities.Remove(newEntity);
                break;
            }

            newEntity.Setup(Team.Team2, freeNode);
            addedCount++;
        }

        Debug.Log("Đã thêm " + addedCount + " kẻ địch vào Team 2! Tổng cộng: " + team2Entities.Count);
    }

    // Hàm để bắt đầu chiến đấu
    public void StartFight()
    {
        if (team2Entities.Count == 0)
        {
            Debug.Log("Chưa có kẻ địch để chiến đấu!");
            return;
        }

        // Log performance
        perfLogger.isFighting = true;

        isFighting = true; // Bật trạng thái chiến đấu
        OnRoundStart?.Invoke(); // Gọi sự kiện để các entity tìm mục tiêu
        Debug.Log("Bắt đầu vòng chiến đấu!");

        if (team2Entities.Count == 0 || team1Entities.Count == 0)
        {
            Debug.Log("Trận đấu kết thúc");
            // Log performance
            perfLogger.isFighting = false;
            return;
        }
    }

    // Giữ lại DebugFight nếu cần
    public void DebugFight()
    {
        // Gọi AddEnemies để spawn kẻ địch
        AddEnemies();
        // Tùy chọn: gọi StartFight ngay sau khi spawn để giữ hành vi cũ
        StartFight();
    }


}

public enum Team
{
    Team1,
    Team2
}
