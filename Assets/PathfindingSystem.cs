using System.Collections.Generic;
using UnityEngine;

public interface IPathNode
{
    int x { get; }
    int y { get; }
    int gCost { get; set; }
    int hCost { get; set; }
    int fCost { get; }
    bool canBuild { get; }
    IPathNode cameFromNode { get; set; }
    void CalculateFCost();
}

public class PathfindingSystem<T> where T : class, IPathNode
{
    private Grid<T> grid;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 24;
    private List<T> openList;
    private List<T> closedList;

    public PathfindingSystem(Grid<T> grid)
    {
        this.grid = grid;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, bool isCornerIncluded)
    {
        grid.GetXZ(startWorldPosition, out int startX, out int startZ);
        grid.GetXZ(endWorldPosition, out int endX, out int endZ);

        List<T> path = FindPath(startX, startZ, endX, endZ, isCornerIncluded);
        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (T node in path)
            {
                vectorPath.Add(new Vector3(node.x, 0, node.y) * grid.GetCellSize() +
                               Vector3.one * grid.GetCellSize() * .5f);
            }

            return vectorPath;
        }
    }

    private List<T> FindPath(int startX, int startY, int endX, int endY, bool isCornerIncluded)
    {
        T startNode = grid.GetGridObject(startX, startY);
        T endNode = grid.GetGridObject(endX, endY);

        openList = new List<T>() { startNode };
        closedList = new List<T>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                T node = grid.GetGridObject(x, y);
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            T currentNode = GetLowestFCostNode(openList);
            if (currentNode.Equals(endNode))
            {
                //Final Node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (T neighbourNode in GetNeighbourList(currentNode, isCornerIncluded))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.canBuild)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        Debug.Log("Gidilemez");
        return null;
    }

    public List<T> GetNeighbour(T node, bool isCornerIncluded)
    {
        return GetNeighbourList(node, isCornerIncluded);
    }

    private List<T> GetNeighbourList(T currentNode, bool isCornerIncluded)
    {
        List<T> neighbourList = new List<T>();

        if (currentNode.x - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            if (isCornerIncluded)
            {
                if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                if (currentNode.y + 1 < grid.GetHeight())
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
        }

        if (currentNode.x + 1 < grid.GetWidth())
        {
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            if (isCornerIncluded)
            {
                if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                if (currentNode.y + 1 < grid.GetHeight())
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
        }

        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }

    private T GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<T> CalculatePath(T endNode)
    {
        List<T> path = new List<T>();
        T currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode);
            currentNode = (T)currentNode.cameFromNode;
        }

        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(T a, T b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private T GetLowestFCostNode(List<T> nodeList)
    {
        T lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }

        return lowestFCostNode;
    }
}