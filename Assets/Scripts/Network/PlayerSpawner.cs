using UnityEngine;

namespace Network
{
    public class PlayerSpawner : MonoBehaviour
    {
        public static PlayerSpawner Instance;

        [SerializeField] private Transform[] spawnPoints;

        private int _lastIndex = -1;

        private void Awake()
        {
            Instance = this;
        }

        public Vector3 GetSpawnPosition()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points set!");
                return Vector3.zero;
            }

            int index;

            do
            {
                index = Random.Range(0, spawnPoints.Length);
            }
            while (spawnPoints.Length > 1 && index == _lastIndex);

            _lastIndex = index;

            Debug.Log("Spawn index: " + index);

            return spawnPoints[index].position;
        }
    }
}
