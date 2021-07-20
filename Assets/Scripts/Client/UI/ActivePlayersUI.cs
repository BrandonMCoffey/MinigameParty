using System.Collections.Generic;
using Assets.Scripts.Server.Net;
using UnityEngine;

namespace Assets.Scripts.Client.UI {
    public class ActivePlayersUI : MonoBehaviour {
        [SerializeField] private ActivePlayerIdentity _playerIdentityPrefab = null;

        private Dictionary<ulong, ActivePlayerIdentity> _clientIdentityMap;

        public void AddPlayer(PlayerData playerData, string playerType)
        {
            ActivePlayerIdentity identity = Instantiate(_playerIdentityPrefab);
            _clientIdentityMap.Add(playerData.ClientId, identity);
            identity.PlayerNumber.text = "Player " + _clientIdentityMap.Count;
            identity.PlayerName.text = playerData.PlayerName;
            identity.PlayerType.text = playerType;
        }

        public void RemovePlayer()
        {
        }
    }
}