using SpaceEngineersVR.Plugin;
using System;
using VRage.Game;
using VRage.Game.Components;
#pragma warning disable CS0649

namespace SpaceEngineersVR.Patches
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	class SimulationUpdater : MySessionComponentBase
	{
		public static Action UpdateBeforeSim;
		public static Action UpdateAfterSim;
		public static Action UpdateSim;
		public static Action OnWorldUnload;
		public static Action BeforeWorldStart;

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			Logger.Debug("Custom simulation updater initializing.");
		}

		~SimulationUpdater()
		{
			Logger.Debug("Destructing custom simulation updater.");
		}

		public override void BeforeStart()
		{
			BeforeWorldStart?.Invoke();
		}

		public override void UpdateBeforeSimulation()
		{
			UpdateBeforeSim?.Invoke();
		}

		public override void Simulate()
		{
			UpdateSim?.Invoke();
		}

		public override void UpdateAfterSimulation()
		{
			UpdateAfterSim?.Invoke();
		}

		protected override void UnloadData()
		{
			Logger.Debug("Custom simulation updater UnloadData called.");
			OnWorldUnload?.Invoke();
		}
	}
}
