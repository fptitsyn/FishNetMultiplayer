using System.Collections;
using FishNet;
using Objects;
using FishNet.Object;
using UnityEngine;

namespace Network
{
    public class PickupManager : MonoBehaviour
    {
        [SerializeField] private GameObject healthPickupPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float respawnDelay = 10f;

        private void Start()
        {
            if (!InstanceFinder.IsServer) return;
            SpawnAll();
        }

        public void OnPickedUp(Vector3 pos)
        {
            StartCoroutine(Respawn(pos));
        }

        private IEnumerator Respawn(Vector3 pos)
        {
            yield return new WaitForSeconds(respawnDelay);
            Spawn(pos);
        }

        private void Spawn(Vector3 pos)
        {
            var go = Instantiate(healthPickupPrefab, pos, Quaternion.identity);
            go.GetComponent<FirstAid>().Init(this);
            InstanceFinder.ServerManager.Spawn(go);
        }

        private void SpawnAll()
        {
            foreach (var p in spawnPoints)
                Spawn(p.position);
        }
    }
}