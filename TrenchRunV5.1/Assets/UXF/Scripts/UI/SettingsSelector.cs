﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UXF{
	public class SettingsSelector : MonoBehaviour {

		public DropDownController ddController;
		public Session session;
		public PopupController popupController;
		public ExperimentStartupController startupController;
		List<string> settingsNames;
		public string settingsFolder;
		Dictionary<string, object> settingsDict;
		[HideInInspector]
		public string experimentName;

		void Start ()
		{
			settingsFolder = Application.streamingAssetsPath;
			TryGetSettingsList();
		}
		

		void TryGetSettingsList()
		{
			settingsNames = Directory.GetFiles(settingsFolder, startupController.settingsSearchPattern)
                                .Select(f => Path.GetFileName(f))
                                .ToList();

            if (settingsNames.Count > 0)
            {
                ddController.SetItems(settingsNames);
                LoadCurrentSettingsDict();
            }
            else
            {
                Popup settingsError = new Popup();
                settingsError.messageType = MessageType.Error;
                settingsError.message = string.Format(
					"No settings files found at {0} that match pattern {1}. Please create at least one json-encoded file containing your settings.",
					settingsFolder,
					startupController.settingsSearchPattern);
                settingsError.onOK = new System.Action(() => {TryGetSettingsList();});
                popupController.DisplayPopup(settingsError);
            }

		}

		public void LoadCurrentSettingsDict()
		{
			string fname = ddController.GetContents().ToString();
			ReadSettingsDict(fname);
            experimentName = Path.GetFileNameWithoutExtension(fname);
        }

        void ReadSettingsDict(string settingsName)
        {
			string settingsPath = Path.Combine(settingsFolder, settingsName);
            session.ReadSettingsFile(settingsPath, new System.Action<Dictionary<string, object>>((dict) => HandleSettingsDict(dict)));
        }

		void HandleSettingsDict(Dictionary<string, object> dict)
		{
			settingsDict = dict;
		}

		public Settings GetSettings()
		{
			return new Settings(settingsDict);
		}

	}
}