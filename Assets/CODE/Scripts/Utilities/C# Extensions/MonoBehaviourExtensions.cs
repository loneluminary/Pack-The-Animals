using System;
using System.Collections;
using UnityEngine;

namespace Utilities.Extensions
{
	public static class MonoBehaviourExtensions
	{
		public static void DelayedExecution(this MonoBehaviour monoBehaviour, float delay, Action callback)
		{
			monoBehaviour.StartCoroutine(Execute());
			
			IEnumerator Execute()
			{
				yield return new WaitForSeconds(delay);
				callback?.Invoke();
			}
		}

		public static void DelayedExecutionUntilNextFrame(this MonoBehaviour monoBehaviour, Action callback)
		{
			monoBehaviour.StartCoroutine(ExecuteAfterFrame());
			IEnumerator ExecuteAfterFrame()
			{
				yield return null;
				callback?.Invoke();
			}
		}

		public static void DelayedExecutionUntil(this MonoBehaviour monoBehaviour, Func<bool> condition, Action callback, bool expectedResult = true)
		{
			monoBehaviour.StartCoroutine(WaitForCondition());
			
			IEnumerator WaitForCondition()
			{
				yield return new WaitUntil(() => condition() == expectedResult);
				callback?.Invoke();
			}
		}

		public static void RepeatExecutionWhile(this MonoBehaviour behaviour, Func<bool> stopCondition, float interval, Action callback)
		{
			behaviour.StartCoroutine(RepeatWhileCoroutine());
			
			IEnumerator RepeatWhileCoroutine()
			{
				while (!stopCondition())
				{
					yield return new WaitForSeconds(interval);
					callback?.Invoke();
				}
			}
		}

		public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component
		{
			T component = behaviour.GetComponent<T>();
			return component ? component : behaviour.gameObject.AddComponent<T>();
		}

		public static void AddComponentIfMissing<T>(this MonoBehaviour behaviour) where T : Component
		{
			if (!behaviour.GetComponent<T>()) behaviour.gameObject.AddComponent<T>();
		}
	}
}