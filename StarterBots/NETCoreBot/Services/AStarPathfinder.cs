using System;
using System.Collections.Generic;
using System.Linq;
using NETCoreBot.Enums;
using NETCoreBot.Models;

namespace NETCoreBot.Services;

public class AStarPathfinder
{
    private class Node
    {
        public int X { get; }
        public int Y { get; }
        public int GCost { get; set; } // Distance from start
        public int HCost { get; set; } // Estimated distance to goal
        public int FCost => GCost + HCost;
        public Node? Parent { get; set; }

        public Node(int x, int y, Node? parent = null, int gCost = 0, int hCost = 0)
        {
            X = x;
            Y = y;
            Parent = parent;
            GCost = gCost;
            HCost = hCost;
        }

        public override bool Equals(object? obj)
        {
            return obj is Node other && X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    public static List<(int X, int Y)>? FindPath(
        GameState state,
        int startX,
        int startY,
        int targetX,
        int targetY
    )
    {
        var startNode = new Node(
            startX,
            startY,
            null,
            gCost: 0,
            hCost: ManhattanDistance(startX, startY, targetX, targetY)
        );
        var endNode = new Node(targetX, targetY);

        var openSet = new List<Node> { startNode };
        var closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            // Pick node with lowest FCost
            var current = openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();

            if (current.X == endNode.X && current.Y == endNode.Y)
                return ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbours(state, current, targetX, targetY))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                var openNode = openSet.FirstOrDefault(n => n.X == neighbor.X && n.Y == neighbor.Y);
                if (openNode == null)
                {
                    openSet.Add(neighbor);
                }
                else if (neighbor.GCost < openNode.GCost)
                {
                    // Better path
                    openNode.GCost = neighbor.GCost;
                    openNode.Parent = current;
                }
            }
        }

        // No path found
        return null;
    }

    private static IEnumerable<Node> GetNeighbours(
        GameState state,
        Node node,
        int targetX,
        int targetY
    )
    {
        var directions = new (int dx, int dy)[]
        {
            (0, -1), // Up
            (0, 1), // Down
            (-1, 0), // Left
            (1, 0), // Right
        };

        foreach (var (dx, dy) in directions)
        {
            int nx = node.X + dx;
            int ny = node.Y + dy;

            var cell = state.Cells.FirstOrDefault(c => c.X == nx && c.Y == ny);
            if (cell == null || cell.Content == CellContent.Wall)
                continue;

            yield return new Node(
                nx,
                ny,
                parent: node,
                gCost: node.GCost + 1,
                hCost: ManhattanDistance(nx, ny, targetX, targetY)
            );
        }
    }

    private static List<(int X, int Y)> ReconstructPath(Node node)
    {
        var path = new List<(int X, int Y)>();

        while (node != null)
        {
            path.Add((node.X, node.Y));
            node = node.Parent;
        }

        path.Reverse();
        return path;
    }

    private static int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }
}
