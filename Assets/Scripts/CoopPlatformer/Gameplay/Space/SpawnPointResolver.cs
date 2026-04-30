using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    public static class SpawnPointResolver
    {
        private static readonly Vector2[] SpawnPoints =
        {
            new(-6f, 0f),
            new(6f, 0f),
            new(0f, 6f),
            new(0f, -6f)
        };

        public static Vector2 GetSpawnPosition(ulong clientId)
        {
            return SpawnPoints[clientId % (ulong)SpawnPoints.Length];
        }
    }
}