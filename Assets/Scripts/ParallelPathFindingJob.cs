using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pathfinding.Jobs
{
    // Bật tăng tốc độ biên dịch Job bằng cách thêm
    [BurstCompile]
    // IJobParallelFor = mỗi entity/path request sẽ được xử lý song song ở các core khác nhau.
    public struct ParallelPathfindingJob : IJobParallelFor
    {
        // NativeArray<NodeData> để lưu toàn bộ bản đồ dạng đọc-only (bảo vệ thread-safe).

        // NativeArray<PathRequest> để chứa danh sách yêu cầu(start → end) của các đơn vị.

        // NativeArray<int> để chứa kết quả trả về (node trước đích).

        //Các job chỉ được đọc dữ liệu bản đồ và yêu cầu.

        // Tuyệt đối không sửa đổi dữ liệu trong lúc chạy song song → tránh Race Condition.
        
        [ReadOnly] public NativeArray<NodeData> nodes;
        [ReadOnly] public NativeArray<PathRequest> requests;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> results; // ID node gần end nhất (demo)

        public void Execute(int index)
        {
            var request = requests[index];
            int start = request.startNodeId;
            int end = request.endNodeId;

            NativeArray<float> distances = new NativeArray<float>(nodes.Length, Allocator.Temp);
            NativeArray<int> previous = new NativeArray<int>(nodes.Length, Allocator.Temp);
            NativeList<int> unvisited = new NativeList<int>(Allocator.Temp);

            for (int i = 0; i < nodes.Length; i++)
            {
                distances[i] = float.MaxValue;
                previous[i] = -1;
                unvisited.Add(i);
            }

            distances[start] = 0;

            while (unvisited.Length > 0)
            {
                int current = -1;
                float min = float.MaxValue;

                for (int i = 0; i < unvisited.Length; i++)
                {
                    int id = unvisited[i];
                    if (distances[id] < min)
                    {
                        min = distances[id];
                        current = id;
                    }
                }

                if (current == -1) break;

                for (int i = 0; i < unvisited.Length; i++)
                {
                    if (unvisited[i] == current)
                    {
                        unvisited.RemoveAtSwapBack(i);
                        break;
                    }
                }

                if (current == end) break;

                var node = nodes[current];
                for (int i = 0; i < node.neighbors.Length; i++)
                {
                    int neighborId = node.neighbors[i];
                    var neighbor = nodes[neighborId];
                    float dist = math.distance(node.worldPosition, neighbor.worldPosition);
                    float alt = distances[current] + dist;

                    if (alt < distances[neighborId])
                    {
                        distances[neighborId] = alt;
                        previous[neighborId] = current;
                    }
                }
            }

            results[index] = previous[end];

            distances.Dispose();
            previous.Dispose();
            unvisited.Dispose();
        }
    }

    public struct PathRequest
    {
        public int startNodeId;
        public int endNodeId;
    }

    public struct NodeData
    {
        public float3 worldPosition;
        public FixedList32Bytes<int> neighbors;
    }
}
