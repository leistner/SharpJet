﻿// <copyright file="ConnectTests.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// SharpJet, a library to communicate with Jet IPC.
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

namespace Hbm.Devices.Jet
{
    using System.Collections;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class ConnectTests
    {
        private bool connectCallbackCalled;
        private bool connectCompleted;

        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(Behaviour.ConnectionSuccess).Returns(true);
                yield return new TestCaseData(Behaviour.ConnectionFail).Returns(false);
            }
        }

        [SetUp]
        public void Setup()
        {
            this.connectCallbackCalled = false;
        }

        [Test, TestCaseSource(typeof(ConnectTests), "TestCases")]
        public bool ConnectTest(Behaviour behaviour)
        {
            var connection = new TestJetConnection(behaviour);
            var peer = new JetPeer(connection);
            peer.Connect(this.OnConnect, 1);
            Assert.AreEqual(this.connectCallbackCalled, true);
            return this.connectCompleted;
        }

        [Test]
        public void RemoveStatesWhenDisconnect()
        {
            const string state = "theState";

            var connection = new TestJetConnection(Behaviour.ConnectionSuccess);
            var peer = new JetPeer(connection);
            peer.Connect(this.OnConnect, 1);
            JValue stateValue = new JValue(12);
            peer.AddState(state, stateValue, this.OnSet, this.OnResponse, 3000);
            peer.Disconnect();
            Assert.AreEqual(0, peer.NumberOfRegisteredStateCallbacks());
            string removeJson = connection.messages[1];
            JToken json = JToken.Parse(removeJson);
            JToken method = json["method"];
            Assert.AreEqual("remove", method.ToString());
            JToken parameters = json["params"];
            JToken path = parameters["path"];
            Assert.AreEqual(state, path.ToString());
        }

        private JToken OnSet(string arg1, JToken arg2)
        {
            return null;
        }

        private void OnResponse(bool arg1, JToken arg2)
        {
        }

        private void OnConnect(bool completed)
        {
            this.connectCallbackCalled = true;
            this.connectCompleted = completed;
        }
    }
}
