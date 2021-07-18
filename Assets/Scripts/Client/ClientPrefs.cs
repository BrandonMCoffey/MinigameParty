using System;
using UnityEngine;

namespace Assets.Scripts.Client {
    public class ClientPrefs : MonoBehaviour {
        public static string GetClientGuid()
        {
            if (PlayerPrefs.HasKey("client_guid")) {
                return PlayerPrefs.GetString("client_guid");
            }

            string guidString = Guid.NewGuid().ToString();

            PlayerPrefs.SetString("client_guid", guidString);
            return guidString;
        }

        public static string GetClientName()
        {
            return PlayerPrefs.GetString("ClientName", "");
        }

        public static void SetClientName(string name)
        {
            if (name == string.Empty) return;
            PlayerPrefs.SetString("ClientName", name);
        }

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat("MasterVolume", 1);
        }

        public static void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        }

        public static void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
    }
}