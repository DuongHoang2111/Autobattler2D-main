using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Pathfinding.Jobs;

public class PathManager : MonoBehaviour
{
    public Graph graph;
    public int unitCount = 10;

    void Start()
    {
        var nodeData = graph.ExportAsNodeData(Allocator.TempJob);
        var requests = new NativeArray<PathRequest>(unitCount, Allocator.TempJob);
        var results = new NativeArray<int>(unitCount, Allocator.TempJob);

        for (int i = 0; i < unitCount; i++)
        {
            requests[i] = new PathRequest
            {
                startNodeId = UnityEngine.Random.Range(0, nodeData.Length),
                endNodeId = UnityEngine.Random.Range(0, nodeData.Length)
            };
        }

        var job = new ParallelPathfindingJob
        {
            nodes = nodeData,
            requests = requests,
            results = results
        };
        // unitCount: số lượng đơn vị cần tìm đường.

        // 1: batch size = 1 request xử lý mỗi lần → Unity tự chia đều cho các core.

        // Nếu số lượng entity cực lớn(vài ngàn), bạn có thể chỉnh batchSize > 1 để tối ưu thêm.
        JobHandle handle = job.Schedule(unitCount, 1);
        handle.Complete();

        for (int i = 0; i < unitCount; i++)
        {
            Debug.Log($"Unit {i} → previous before goal: {results[i]}");
        }

        nodeData.Dispose();
        requests.Dispose();
        results.Dispose();
    }
}
