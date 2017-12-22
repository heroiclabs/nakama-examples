﻿/**
 * Copyright 2017 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Nakama;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework;

namespace Showreel
{	
	public class Authentication : MonoBehaviour
	{
		private bool _initializedFakeData = false;
		
		private void Start()
		{
			NakamaManager.AfterConnected += (sender, evt) =>
			{
				if (!_initializedFakeData)
				{
					FakeData.init();
					_initializedFakeData = true;
				}
				
				SceneManager.LoadScene("SelectionMenuScene");
			};
			
			NakamaManager.AfterDisconnected += (sender, evt) =>
			{
				SceneManager.LoadScene("AuthenticationScene");
			};	
		}

		// Invoked by the UI 
		public void PlayAsGuest()
		{
			INAuthenticateMessage authMessage = BuildDeviceAuthenticateMessage();
			NakamaManager.Instance.Connect(authMessage);
		}

		public void LinkViaFacebook()
		{
			
		}
		
		private static NAuthenticateMessage BuildDeviceAuthenticateMessage()
		{
			var id = PlayerPrefs.GetString("nk.deviceid");
			if (string.IsNullOrEmpty(id))
			{
				id = SystemInfo.deviceUniqueIdentifier;
				PlayerPrefs.SetString("nk.deviceid", id);
			}
			Debug.LogFormat("Device Id: '{0}'.", id);
			return NAuthenticateMessage.Device(id);
		}
	}
}
