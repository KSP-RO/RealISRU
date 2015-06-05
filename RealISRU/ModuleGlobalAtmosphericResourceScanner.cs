using System;
using KSP.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace RealISRU {
	public class ModuleGlobalAtmosphericResourceScanner : PartModule {
		private Rect resourceWindowPos = new Rect(Screen.width / 10, 20, 250, 100);

		[KSPField(guiName = "Analyzer", guiActive = true)]
		string analyzer;

		[KSPField(guiName = "Show Resources", guiActive = true), UI_Toggle (disabledText = "Inactive", enabledText = "Active")]
        public bool analysisDisplay = false;

        [KSPField]
        public float maxAnalysisAltitude = 5000000f;

        public override void OnStart(StartState state) {
            if(state == StartState.Editor) { return; }
			part.force_activate();
        }

		public void OnGUI() {
			if(FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null && analysisDisplay) {
				resourceWindowPos = GUILayout.Window(this.GetInstanceID(), resourceWindowPos, drawResourceWindow, "Atmospheric Analysis", GUILayout.ExpandHeight(true));
			}
		}

        public override void OnFixedUpdate() {
            if(HighLogic.LoadedSceneIsFlight) {
				analyzer = "";

				// check main body
				CelestialBody body = FlightGlobals.getMainBody();
				if(body.name == "Sun" || !body.atmosphere) {
					analysisDisplay = false;
					analyzer = "No atmosphere available.";
					Fields["analysisDisplay"].guiActive = false;
					return;
				}

 				// check altitude
				if(FlightGlobals.ActiveVessel.altitude > maxAnalysisAltitude) {
					analysisDisplay = false;
					analyzer = "Too far to analyse.";
					Fields["analysisDisplay"].guiActive = false;
					return;
				}

				analyzer = "Ready";	
				Fields["analysisDisplay"].guiActive = true;
            } else {
				analysisDisplay = false;
				analyzer = "Analyzer inactive.";
				Fields["analysisDisplay"].guiActive = false;
			}
        }

		private void drawResourceWindow(int id) {
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Resource", GUILayout.Width(200));
			GUILayout.Label("%", GUILayout.Width(50));
			GUILayout.EndHorizontal();

			// gather resources
			CelestialBody body = FlightGlobals.getMainBody();
			List<ResourceData> planetConfigs = ResourceCache.Instance.PlanetaryResources.Where(r => r.PlanetName == body.bodyName && r.ResourceType == 2).ToList();

			// display
			if(planetConfigs.Count < 1) {
				GUILayout.BeginHorizontal();
				GUILayout.Label("No resources available.", GUILayout.Width(200));
				GUILayout.Label("", GUILayout.Width(50));
				GUILayout.EndHorizontal();
			} else {
				ResourceData rd;
				for(int i = 0; i < planetConfigs.Count; i++) {
					rd = planetConfigs[i];
					GUILayout.BeginHorizontal();
					GUILayout.Label(rd.ResourceName, GUILayout.Width(200));
					GUILayout.Label(rd.Distribution.MaxAbundance.ToString("0.000"), GUILayout.Width(50));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndVertical();
			GUI.DragWindow();
		}
	}
}
