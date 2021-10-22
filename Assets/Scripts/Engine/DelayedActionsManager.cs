using System;
using System.Collections.Generic;
using UnityEngine;

public static class DelayedActionsManager
{
	static List<DelayedAction> delayedActions = new List<DelayedAction>();
	static List<DelayedAction> toAdd = new List<DelayedAction>();
	static string debugTag => "<b>[DelayedActionsManager] : </b>";

	public static void SceduleAction(Action callback, float delay)
	{
		toAdd.Add(new DelayedAction(callback, delay));
	}

	// called at every frame
	public static void Update(float delta)
	{
		List<DelayedAction> doneActions = new List<DelayedAction>();

		foreach (DelayedAction delayedAction in delayedActions)
		{
			if(delayedAction.Update(delta))
				doneActions.Add(delayedAction);
		}

		doneActions.ForEach(item => delayedActions.Remove(item));
		toAdd.ForEach(item => delayedActions.Add(item));

		toAdd.Clear();
	}

	class DelayedAction
	{
		public Action callback;
		public float delay;

		float timer;

		public DelayedAction(Action callback, float delay)
		{
			this.callback = callback;
			this.delay = delay;

			timer = delay;
		}

		// called at every frame
		public bool Update(float delta)
		{
			timer -= delta;

			if(timer <= 0)
			{
				callback.Invoke();
				return true;
			}

			return false;
		}
	}
}