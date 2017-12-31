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

using System;
using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

namespace Framework
{
	public class NakamaManager : Singleton<NakamaManager>
	{
		internal const string HostIp = "127.0.0.1";
		internal const uint Port = 7350;
		internal const bool UseSsl = false;
		internal const string ServerKey = "defaultkey";
		
		private const int MaxReconnectAttempts = 5;
		
		private readonly Queue<Action> _dispatchQueue = new Queue<Action>();
		private readonly INClient _client;
		private readonly Action<INSession> _sessionHandler;
		
		public static event EventHandler AfterConnected = (sender, evt) => { };
		public static event EventHandler AfterDisconnected = (sender, evt) => { };
		private static readonly Action<INError> ErrorHandler = err =>
		{
			Logger.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
		};

		//TODO(mo): Facebook/Google Login with token expiry
		private INAuthenticateMessage _authenticateMessage;
		
		// Flag to tell us whether the socket was closed intentially or not and whether to attempt reconnect.
		private bool doReconnect = true;
		private uint reconnectCount = 0;

		public INSession Session { get; private set; }
		
		private NakamaManager()
		{
			_client = new NClient.Builder(ServerKey).Host(HostIp).Port(Port).SSL(UseSsl).Build();
	
			_sessionHandler = session =>
			{
				Logger.LogFormat("Session: '{0}'.", session.Token);
				_client.Connect(session, done =>
				{
					Session = session;
					reconnectCount = 0;
					// cache session for quick reconnects
					_dispatchQueue.Enqueue(() =>
					{
						PlayerPrefs.SetString("nk.session", session.Token);
						AfterConnected(this, EventArgs.Empty);
					});
				});
			};

			_client.OnDisconnect = evt =>
			{
				Logger.Log("Disconnected from server.");
				if (doReconnect && reconnectCount < MaxReconnectAttempts)
				{
					reconnectCount++;
					_dispatchQueue.Enqueue(() => { Reconnect(); });
				}
				else
				{
					_dispatchQueue.Clear();
					_dispatchQueue.Enqueue(() => { AfterDisconnected(this, EventArgs.Empty); });
					
				}
			};
			_client.OnError = error => ErrorHandler(error);
		}

		private IEnumerator Reconnect()
		{
			// if it's the first time disconnected, then attempt to reconnect immediately
			// every other time, wait 10,20,30,40,50 seconds each time 
			var reconnectTime = ((reconnectCount-1) + 10) * 60;  
			yield return new WaitForSeconds(reconnectTime);
			_sessionHandler(Session);
		}
		
		// Restore serialised session token from PlayerPrefs
		// If the token doesn't exist or is expired `null` is returned.
		private static INSession RestoreSession()
		{	
			var cachedSession = PlayerPrefs.GetString("nk.session");
			if (string.IsNullOrEmpty(cachedSession))
			{
				Logger.Log("No Session in PlayerPrefs.");
				return null;
			}

			var session = NSession.Restore(cachedSession);
			if (!session.HasExpired(DateTime.UtcNow)) return session;
			Logger.Log("Session expired.");
			return null;
		}
		
		private void Update()
		{
			for (int i = 0, l = _dispatchQueue.Count; i < l; i++)
			{
				_dispatchQueue.Dequeue()();
			}
		}
		
		private void OnApplicationQuit()
		{
			doReconnect = false;
			_client.Disconnect();
		}
		
		private void OnApplicationPause(bool isPaused)
		{
			if (isPaused)
			{
				doReconnect = false;
				_client.Disconnect();
				return;
			}

			// let's re-authenticate (if neccessary) and reconnect to the server.
			if (_authenticateMessage != null)
			{
				doReconnect = true;
				Connect(_authenticateMessage);
			}
		}
		
		// This method connects the client to the server and
		// if neccessary authenticates with the server
		public void Connect(INAuthenticateMessage request)
		{	
			// Check to see if we have a valid session token we can restore
			var session = RestoreSession();
			if (session != null)
			{
				// Session is valid, let's connect.
				_sessionHandler(session);
				return;
			}
			
			// Cache message for later use for reconnecting purposes in case the session had expired
			_authenticateMessage = request;
			
			// Let's login to authenticate and get a valid token
			_client.Login(request, _sessionHandler, err =>
			{
				if (err.Code == ErrorCode.UserNotFound)
				{
					_client.Register(request, _sessionHandler, ErrorHandler);
				}
				else
				{
					ErrorHandler(err);	
				}
			});
		}

		public void SelfFetch(NSelfFetchMessage message)
		{
			_client.Send(message, self =>
			{
				StateManager.Instance.SelfInfo = self;
			}, ErrorHandler);
		}
		
		public void FriendAdd(NFriendAddMessage message, bool refreshList=true)
		{
			_client.Send(message, b =>
			{
				if (refreshList)
				{
					FriendsList(NFriendsListMessage.Default());
				}
			}, ErrorHandler);
		}
		
		public void FriendRemove(NFriendRemoveMessage message)
		{
			_client.Send(message, b =>
			{
				FriendsList(NFriendsListMessage.Default());
			}, ErrorHandler);
		}
		
		public void FriendsList(NFriendsListMessage message)
		{
			_client.Send(message, friends =>
			{
				StateManager.Instance.Friends.Clear();
				for (var i = 0; i < friends.Results.Count; i++) {
					StateManager.Instance.Friends.Add(friends.Results[i]);
				}
			}, ErrorHandler);
		}
	}
}
