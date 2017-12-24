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
using Framework;
using Nakama;

namespace Showreel
{
    // This is only needed for Demo purposes.
    // You can ignore this class entirely.
    public static class FakeData
    {
        private static bool _initializedFakeData = false;
        private static readonly Action<INError> _errorHandler = err =>
        {
            Logger.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
        };
        private static readonly NClient _client = new NClient.Builder(NakamaManager.ServerKey)
            .Host(NakamaManager.HostIp)
            .Port(NakamaManager.Port)
            .SSL(NakamaManager.UseSsl)
            .Build();

        private static INSession user1Session, user2Session, user3Session;
        
        // This initializes the server with fake data so that views are not empty when loaded.
        // This assumes that the current user is connected to the system.
        public static void init()
        {
            if (_initializedFakeData)
            {
                return;
            }
            
            _client.Register(buildAuthenticationMessage(), session =>
            {
                user1Session = session;
                _client.Connect(user1Session, c => {
                    setupUser1();
                    _client.Disconnect();
                    
                    _client.Register(buildAuthenticationMessage(), session2 =>
                    {
                        user2Session = session2;
                        _client.Connect(user2Session, c2 => {
                            setupUser2();
                            _client.Disconnect();
                            
                            _client.Register(buildAuthenticationMessage(), session3 =>
                            {
                                user3Session = session3;
                                _client.Connect(user3Session, c3 => {
                                    setupUser3();
                                    _client.Disconnect();
                
                                    setupMainUser();
                                    _initializedFakeData = true;
                                });
                            }, _errorHandler);
                        });   
                    }, _errorHandler);
                });
            }, _errorHandler);
        }

        private static void setupUser1()
        {
            _client.Send(NFriendAddMessage.ById(NakamaManager.Instance.Session.Id), b => { }, _errorHandler);
        }
        
        private static void setupUser2()
        {
            var builder = new NGroupCreateMessage.Builder("House Party");
            builder.Description("House warming party in a few days");
            builder.Lang("en");
            _client.Send(builder.Build(), b => { }, _errorHandler); // don't care about the response.
        }
        
        private static void setupUser3()
        {
            _client.Send(NFriendAddMessage.ById(NakamaManager.Instance.Session.Id), b => { }, _errorHandler);
        }
        
        private static void setupMainUser()
        {
            // let's add two users as friends
            NakamaManager.Instance.AddFriend(NFriendAddMessage.ById(user1Session.Id));
            NakamaManager.Instance.AddFriend(NFriendAddMessage.ById(user2Session.Id));
            
            var builder = new NGroupCreateMessage.Builder("School friends");
            builder.Description("Weekend getaway");
            builder.Lang("en");
            _client.Send(builder.Build(), b => { }, _errorHandler);
            
            builder = new NGroupCreateMessage.Builder("Thanksgiving");
            builder.Description("Turkey eating at my house!");
            builder.Lang("en");
            _client.Send(builder.Build(), b => { }, _errorHandler);
        }

        private static INAuthenticateMessage buildAuthenticationMessage()
        {
            return NAuthenticateMessage.Custom(Guid.NewGuid().ToString());
        }
    }
}