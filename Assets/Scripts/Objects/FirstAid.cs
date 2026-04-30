using Network;
using FishNet.Object;
using UnityEngine;

namespace Objects
{
    public class FirstAid : NetworkBehaviour
    {
        [SerializeField] private int healAmount = 40;

        private PickupManager _manager;
        private Vector3 _spawnPosition;

        public void Init(PickupManager manager)
        {
            _manager = manager;
            _spawnPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            var player = other.GetComponent<PlayerNetwork>();
            if (player == null) return;

            // Мёртвый не подбирает
            if (!player.IsAlive.Value) return;

            // Не лечить при полном HP
            if (player.HP.Value >= 100) return;

            player.HP.Value = Mathf.Min(100, player.HP.Value + healAmount);

            _manager.OnPickedUp(_spawnPosition);
            Despawn(DespawnType.Destroy);
        }
    }
}