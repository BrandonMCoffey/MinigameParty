using System.Collections.Generic;
using Assets.Scripts.Server.Net;
using UnityEngine;

namespace Assets.Scripts.Client.UI
{
    public class ActivePlayersUI : MonoBehaviour
    {
        [SerializeField] private ActivePlayerIdentity _playerIdentityPrefab = null;

        private Dictionary<ulong, GameObject> _clientObjectMap;

        public void AddPlayer(PlayerData playerData)
        {
            GameObject identity = Instantiate(_playerIdentityPrefab.gameObject);
        }

        public void RemovePlayer()
        {

        }
    }
}