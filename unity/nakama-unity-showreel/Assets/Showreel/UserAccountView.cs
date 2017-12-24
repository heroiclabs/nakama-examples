/**
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

using Framework;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace Showreel
{
	public class UserAccountView : MonoBehaviour
	{
		public Text SelfInfoText;
		
		private void Start()
		{
			SelfInfoText = GameObject.Find("SelfInfoText").GetComponent<Text>();
			
			NakamaManager.Instance.SelfFetch(NSelfFetchMessage.Default());
		}

		private void Update()
		{
			var self = StateManager.Instance.SelfInfo;
			if (self == null)
			{
				return;
			}

			var selfText = string.Format(@"
Id: {0}\n
Handle: {1}\n
Fullname: {2}\n
Device ID: {3}
			", self.Id, self.Handle, self.Fullname, self.DeviceIds[0]);
			SelfInfoText.text = selfText;
		}
	}
}