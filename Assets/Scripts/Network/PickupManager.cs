using System;
using System.Collections;
using FishNet;
using FishNet.Managing;
using Objects;
using UI;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace Network
{
    public class PickupManager : MonoBehaviour
    {
        [SerializeField] private GameObject healthPickupPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float respawnDelay = 10f;

        private void OnEnable()
        {
            ConnectionUI.HostStarted += OnHostStarted;
        }

        private void OnDisable()
        {
            ConnectionUI.HostStarted -= OnHostStarted;
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsServer)
            {
                SpawnAll();
            }
        }

        public void OnPickedUp(Vector3 position)
        {
            StartCoroutine(Respawn(position));
        }

        private IEnumerator Respawn(Vector3 pos)
        {
            yield return new WaitForSeconds(respawnDelay);
            SpawnPickup(pos);
        }

        private void SpawnPickup(Vector3 pos)
        {
            if (!healthPickupPrefab)
            {
                Debug.LogError("Pickup prefab not assigned!");
                return;
            }

            var go = Instantiate(healthPickupPrefab, pos, Quaternion.identity);

            go.GetComponent<FirstAid>().Init(this);
            var networkObject = go.GetComponent<NetworkObject>();
            InstanceFinder.ServerManager.Spawn(networkObject);
        }

        private void SpawnAll()
        {
            foreach (var point in spawnPoints)
            {
                SpawnPickup(point.position);
                Debug.Log("SpawnAll called, points: " + spawnPoints.Length);
            }
        }
    }
}