using System.Collections.Generic;
using UnityEngine;

public static class HexGridUtils
{
    public static List<Vector2Int> GetNeighbors(Vector2Int currentCell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int x = currentCell.x;
        int y = currentCell.y;

        if (Mathf.Abs(y) % 2 == 0)
        {
            neighbors.Add(new Vector2Int(x - 1, y));
            neighbors.Add(new Vector2Int(x + 1, y));
            neighbors.Add(new Vector2Int(x + 1, y + 1));
            neighbors.Add(new Vector2Int(x + 2, y + 1));
            neighbors.Add(new Vector2Int(x - 1, y - 1));
            neighbors.Add(new Vector2Int(x - 2, y - 1));
        }
        else
        {
            neighbors.Add(new Vector2Int(x - 1, y));
            neighbors.Add(new Vector2Int(x + 1, y));
            neighbors.Add(new Vector2Int(x + 1, y + 1));
            neighbors.Add(new Vector2Int(x + 2, y + 1));
            neighbors.Add(new Vector2Int(x - 1, y - 1));
            neighbors.Add(new Vector2Int(x - 2, y - 1));
        }

        return neighbors;
    }

    public static List<Vector2Int> GetMovementRange(Vector2Int startPos, int moveRange, Dictionary<Vector2Int, Tile> allTiles)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distanceMap = new Dictionary<Vector2Int, int>();

        queue.Enqueue(startPos);
        distanceMap[startPos] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = distanceMap[current];

            if (currentDist > 0)
            {
                reachable.Add(current);
            }

            if (currentDist >= moveRange) continue;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (allTiles.ContainsKey(neighbor))
                {
                    Tile tileScript = allTiles[neighbor];

                    if (tileScript.IsOccupied && neighbor != startPos) continue;

                    if (!distanceMap.ContainsKey(neighbor))
                    {
                        distanceMap[neighbor] = currentDist + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return reachable;
    }
    public static List<Vector2Int> GetTilesInRange(Vector2Int startPos, int range, Dictionary<Vector2Int, Tile> allTiles)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distanceMap = new Dictionary<Vector2Int, int>();

        queue.Enqueue(startPos);
        distanceMap[startPos] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = distanceMap[current];

            reachable.Add(current);
            if (currentDist >= range) continue;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (allTiles.ContainsKey(neighbor) && !distanceMap.ContainsKey(neighbor))
                {
                    distanceMap[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return reachable;
    }
}