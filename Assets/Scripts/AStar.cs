using UnityEngine;
using System.Collections.Generic;

public class PathNode
{
    public Rect rect;
    public PathNode parent;
    public float gCost; // Cost from start
    public float hCost; // Estimated cost to end
    public float fCost => gCost + hCost;

    public PathNode(Rect rect)
    {
        this.rect = rect;
        parent = null;
        gCost = 0;
        hCost = 0;
    }

    public Vector2 Position => rect.center;
}

public static class AStar
{
    public static List<Vector2> FindPath(List<Rect> walkableRects, Vector2 start, Vector2 end)
    {
        // Convert rects to nodes, only considering positions on layer 7
        var nodes = new List<PathNode>();
        foreach (var rect in walkableRects)
        {
            // Check multiple points of the rect to ensure it's entirely on layer 7
            Vector2[] points = new Vector2[]
            {
                rect.center,
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMax, rect.yMax)
            };

            bool allPointsValid = true;
            foreach (var point in points)
            {
                RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero, 0f, 1 << 7);
                if (!hit)
                {
                    allPointsValid = false;
                    break;
                }
            }

            if (allPointsValid)
            {
                nodes.Add(new PathNode(rect));
            }
        }

        // Find closest nodes to start and end positions
        PathNode startNode = FindClosestNode(nodes, start);
        PathNode endNode = FindClosestNode(nodes, end);

        if (startNode == null || endNode == null)
            return null;

        var openSet = new List<PathNode> { startNode };
        var closedSet = new HashSet<PathNode>();

        while (openSet.Count > 0)
        {
            PathNode current = GetLowestFCostNode(openSet);

            if (current == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current, nodes))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float newGCost = current.gCost + Vector2.Distance(current.Position, neighbor.Position);

                if (!openSet.Contains(neighbor) || newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = Vector2.Distance(neighbor.Position, endNode.Position);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    private static PathNode FindClosestNode(List<PathNode> nodes, Vector2 position)
    {
        PathNode closest = null;
        float minDistance = float.MaxValue;

        foreach (var node in nodes)
        {
            float distance = Vector2.Distance(node.Position, position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = node;
            }
        }

        return closest;
    }

    private static List<PathNode> GetNeighbors(PathNode node, List<PathNode> allNodes)
    {
        var neighbors = new List<PathNode>();
        float neighborDistance = 1.5f; // Adjust this value based on your tile size

        foreach (var potentialNeighbor in allNodes)
        {
            if (node == potentialNeighbor) continue;

            float distance = Vector2.Distance(node.Position, potentialNeighbor.Position);
            if (distance <= neighborDistance)
            {
                // Check if there's a clear line of sight
                if (CanConnect(node.Position, potentialNeighbor.Position))
                {
                    neighbors.Add(potentialNeighbor);
                }
            }
        }

        return neighbors;
    }

    private static bool CanConnect(Vector2 start, Vector2 end)
    {
        // Check for obstacles on layers 9 and 10
        int obstacleLayerMask = (1 << 9) | (1 << 10);
        RaycastHit2D hit = Physics2D.Linecast(start, end, obstacleLayerMask);
        return !hit;
    }

    private static PathNode GetLowestFCostNode(List<PathNode> nodes)
    {
        PathNode lowest = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fCost < lowest.fCost)
                lowest = nodes[i];
        }
        return lowest;
    }

    private static List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
    {
        var path = new List<Vector2>();
        PathNode current = endNode;

        while (current != startNode)
        {
            path.Add(current.Position);
            current = current.parent;
        }
        path.Add(startNode.Position);
        path.Reverse();
        return path;
    }
}