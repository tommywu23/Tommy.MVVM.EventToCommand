using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Tommy.MVVM.EventToCommand.Behavior {
	public static class EventAsync {
		public static Task<object> FromEvent<T>(
			Action<EventHandler<T>> addEventHandler,
			Action<EventHandler<T>> removeEventHandler,
			Action beginAction = null) {
			return new EventHandlerTaskSource<T>(
				addEventHandler,
				removeEventHandler,
				beginAction).Task;
		}

		public static Task<RoutedEventArgs> FromRoutedEvent(
			Action<RoutedEventHandler> addEventHandler,
			Action<RoutedEventHandler> removeEventHandler,
			Action beginAction = null) {
			return new RoutedEventHandlerTaskSource(
				addEventHandler,
				removeEventHandler,
				beginAction).Task;
		}

		private sealed class EventHandlerTaskSource<TEventArgs> {
			private readonly TaskCompletionSource<object> tcs;
			private readonly Action<EventHandler<TEventArgs>> removeEventHandler;

			public EventHandlerTaskSource(
				Action<EventHandler<TEventArgs>> addEventHandler,
				Action<EventHandler<TEventArgs>> removeEventHandler,
				Action beginAction = null) {
				if (addEventHandler == null) {
					throw new ArgumentNullException("addEventHandler");
				}

				if (removeEventHandler == null) {
					throw new ArgumentNullException("removeEventHandler");
				}

				this.tcs = new TaskCompletionSource<object>();
				this.removeEventHandler = removeEventHandler;
				addEventHandler.Invoke(EventCompleted);

				if (beginAction != null) {
					beginAction.Invoke();
				}
			}

			public Task<object> Task {
				get { return tcs.Task; }
			}

			private void EventCompleted(object sender, TEventArgs args) {
				this.removeEventHandler.Invoke(EventCompleted);
				this.tcs.SetResult(args);
			}
		}

		private sealed class RoutedEventHandlerTaskSource {
			private readonly TaskCompletionSource<RoutedEventArgs> tcs;
			private readonly Action<RoutedEventHandler> removeEventHandler;

			public RoutedEventHandlerTaskSource(
				Action<RoutedEventHandler> addEventHandler,
				Action<RoutedEventHandler> removeEventHandler,
				Action beginAction = null) {
				if (addEventHandler == null) {
					throw new ArgumentNullException("addEventHandler");
				}

				if (removeEventHandler == null) {
					throw new ArgumentNullException("removeEventHandler");
				}

				this.tcs = new TaskCompletionSource<RoutedEventArgs>();
				this.removeEventHandler = removeEventHandler;
				addEventHandler.Invoke(EventCompleted);

				if (beginAction != null) {
					beginAction.Invoke();
				}
			}

			public Task<RoutedEventArgs> Task {
				get { return tcs.Task; }
			}

			private void EventCompleted(object sender, RoutedEventArgs args) {
				this.removeEventHandler.Invoke(EventCompleted);
				this.tcs.SetResult(args);
			}
		}
	}
}
