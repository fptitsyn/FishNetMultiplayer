using System.Collections;
using Network;
using FishNet.Object;
using UnityEngine;

namespace Combat
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private float _speed = 18f;
        [SerializeField] private int _damage = 20;

        private void Update()
        {
            transform.Translate(Vector3.forward * (_speed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServerInitialized) return;

            if (!other.TryGetComponent(out PlayerNetwork target)) return;
            if (target.OwnerId == OwnerId) return;

            int newHp = Mathf.Max(0, target.HP.Value - _damage);
            target.HP.Value = newHp;

            Despawn(DespawnType.Destroy);
        }
    }
}