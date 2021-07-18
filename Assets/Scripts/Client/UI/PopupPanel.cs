using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Client.UI {
    public class PopupPanel : MonoBehaviour {
        [SerializeField] private GameObject _reconnectingImage = null;
        [SerializeField] private Text _titleText = null;
        [SerializeField] private Text _mainText = null;
        [SerializeField] private InputField _ipInputField = null;
        [SerializeField] private InputField _portInputField = null;
        [SerializeField] private GameObject _nameObject = null;
        [SerializeField] private InputField _nameInputField = null;
        [SerializeField] private Button _confirmationButton = null;
        [SerializeField] private Text _confirmationText = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private RandomNames _randomNames = null;

        private Action<string, int, string> _confirmFunction;
        private Action _confirmNotifyFunction;

        public void SetupEnterGameDisplay(string titleText, string mainText, string confirmationText, Action<string, int, string> confirmCallback, string ip, int port)
        {
            ResetState();

            if (_titleText != null) _titleText.text = titleText;
            if (_mainText != null) _mainText.text = mainText;

            if (_ipInputField != null) {
                _ipInputField.gameObject.SetActive(true);
                _ipInputField.text = ip;
            }
            if (_portInputField != null) {
                _portInputField.gameObject.SetActive(true);
                _portInputField.text = port.ToString();
            }

            if (_nameObject != null) _nameObject.SetActive(true);
            if (_nameInputField != null) {
                _nameInputField.gameObject.SetActive(true);
                _nameInputField.text = ClientPrefs.GetClientName();
            }

            if (_confirmationText != null) _confirmationText.text = confirmationText;
            if (_confirmationButton != null) {
                _confirmationButton.onClick.AddListener(OnConfirmClick);
                _confirmationButton.gameObject.SetActive(true);
            }

            if (_cancelButton != null) _cancelButton.gameObject.SetActive(true);

            _confirmFunction = confirmCallback;

            gameObject.SetActive(true);
        }

        private void OnConfirmClick()
        {
            int.TryParse(_portInputField.text, out var portNum);
            if (portNum <= 0) portNum = MainMenuUI.DefaultPort;
            string playerName = _nameInputField.text;
            ClientPrefs.SetClientName(playerName);
            if (playerName == string.Empty && _randomNames != null) playerName = _randomNames.GenerateName();
            _confirmFunction.Invoke(_ipInputField.text, portNum, playerName);
        }

        public void ResetState()
        {
            if (_titleText != null) _titleText.text = string.Empty;
            if (_mainText != null) _mainText.text = string.Empty;
            if (_ipInputField != null) _ipInputField.text = string.Empty;
            if (_portInputField != null) _portInputField.text = string.Empty;
            if (_confirmationText != null) _confirmationText.text = string.Empty;
            if (_reconnectingImage != null) _reconnectingImage.SetActive(false);
            if (_confirmationButton != null) _confirmationButton.gameObject.SetActive(false);
            if (_confirmationButton != null) _confirmationButton.onClick.RemoveListener(OnConfirmClick);
            if (_cancelButton != null) _cancelButton.gameObject.SetActive(false);
            if (_nameObject != null) _nameObject.SetActive(false);
            if (_nameInputField != null) _nameInputField.gameObject.SetActive(false);
            _confirmFunction = null;
            _confirmNotifyFunction = null;
        }

        public void SetupNotifierDisplay(string titleText, string mainText, bool displayImage, string confirmationText, Action confirmAction = null)
        {
            ResetState();

            if (_titleText != null) _titleText.text = titleText;
            if (_mainText != null) _mainText.text = mainText;

            if (_reconnectingImage != null) _reconnectingImage.SetActive(displayImage);

            if (confirmationText != string.Empty) {
                if (_confirmationText != null) _confirmationText.text = confirmationText;
                if (_confirmationButton != null) _confirmationButton.gameObject.SetActive(true);
                if (_confirmationButton != null && confirmAction != null) _confirmationButton.onClick.AddListener(OnConfirmNotifyClick);
                _confirmNotifyFunction = confirmAction;
            } else {
                // TODO Allow user to close "Connecting" Menu
                // if (_cancelButton != null) _cancelButton.gameObject.SetActive(true);
            }

            if (_ipInputField != null) _ipInputField.gameObject.SetActive(false);
            if (_portInputField != null) _portInputField.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        private void OnConfirmNotifyClick()
        {
            _confirmNotifyFunction?.Invoke();
        }
    }
}