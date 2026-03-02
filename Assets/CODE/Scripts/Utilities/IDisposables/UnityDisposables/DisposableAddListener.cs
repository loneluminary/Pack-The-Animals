using UnityEngine.Events;

namespace Utilities.IDisposable
{
	public class DisposableAddListener : System.IDisposable
	{
		private readonly UnityEvent _eventHandler;
		private readonly UnityAction _eventAction;

		public DisposableAddListener(UnityEvent eventHandler, UnityAction eventAction)
		{
			_eventHandler = eventHandler;
			_eventAction = eventAction;
			_eventHandler.AddListener(_eventAction);
		}

		public DisposableAddListener(UnityEvent eventHandler, System.Action eventAction)
		{
			_eventHandler = eventHandler;
			_eventAction = new UnityAction(eventAction);
			_eventHandler.AddListener(_eventAction);
		}

		public void Dispose()
		{
			_eventHandler.RemoveListener(_eventAction);
		}
	}
}