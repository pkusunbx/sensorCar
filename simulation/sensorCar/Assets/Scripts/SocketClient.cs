﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car {

	// internal enum CarMode {
	// 	Manual,
	// 	Net
	// }

	[RequireComponent(typeof (CarController))]
	public class SocketClient : MonoBehaviour {

		private CarController carController;
		private SocketIOComponent socket;
		private CarSensor carSensor;
		
		private bool doRecord = false;

		Dictionary<string, string> data;


		public void Start()  {

			carController = GetComponent<CarController>();
			socket = GetComponent<SocketIOComponent>();
			carSensor = GetComponent<CarSensor>();

			socket.On("open", onOpen);
			socket.On("error", onError);
			socket.On("close", onClose);
			socket.On("steer", onSteer);		
		}

		public void Update() {

			// TODO mode change stuff

			if (doRecord) {
				doRecord = false;

				// collect data
				data = new Dictionary<string, string>();

				for (int i = 0; i < carSensor.distances.Count; i++) {

					if (float.IsInfinity(carSensor.distances[i])) {
						doRecord = true;
						return;

					} else {
						data["s" + i.ToString()] = carSensor.distances[i].ToString();
					}
				}

				Debug.Log(new JSONObject(data));

				socket.Emit("evaluate", new JSONObject(data));
			}

		}

		public void onOpen(SocketIOEvent obj) {
			Debug.Log("[SocketIO] Open received: " + obj.name + " " + obj.data);

			doRecord = true;
		}
		
		public void onError(SocketIOEvent obj) {
			Debug.Log("[SocketIO] Error received: " + obj.name + " " + obj.data);

			doRecord = false;
		}
		
		public void onClose(SocketIOEvent obj) {	
			Debug.Log("[SocketIO] Close received: " + obj.name + " " + obj.data);

			doRecord = false;
		}

		public void onSteer(SocketIOEvent obj) {
			Debug.Log("[SocketIO] Steer received: " + obj.name + " " + obj.data);

			// Retrieve json and converte to 
			JSONObject jsonData = obj.data;

			string jsonString = jsonData.GetField("steering_angle").str;

			float steering = float.Parse(jsonString);

			Debug.Log("Steering Angle: " + steering);

			float v = CrossPlatformInputManager.GetAxis("Vertical");

			carController.netMove(steering, v, v, 0.0f);
			// carController.netMove(25, 0.1f, 0.0f, 0.0f);

			doRecord = true;
		}
	}
}
