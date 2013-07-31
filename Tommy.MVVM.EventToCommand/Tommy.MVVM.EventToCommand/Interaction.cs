using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace Tommy.MVVM.EventToCommand.Behavior {
	public static class Interaction {
		private static readonly Dictionary<int, WeakReference<FrameworkElement>> _collectionOwners = new Dictionary<int, WeakReference<FrameworkElement>>();

		public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors", typeof(ObservableCollection<Behavior>),
			typeof(Interaction), new PropertyMetadata(DesignMode.DesignModeEnabled ? new ObservableCollection<Behavior>() : null, BehaviorsChanged));

		[EditorBrowsable(EditorBrowsableState.Always)]
		public static ObservableCollection<Behavior> GetBehaviors(DependencyObject obj) {
			var behaviors = obj.GetValue(BehaviorsProperty) as ObservableCollection<Behavior>;
			if (behaviors == null) {
				behaviors = new ObservableCollection<Behavior>();
				SetBehaviors(obj, behaviors);
			}

			return behaviors;
		}

		public static void SetBehaviors(DependencyObject obj, ObservableCollection<Behavior> value) {
			obj.SetValue(BehaviorsProperty, value);
		}

		private static void BehaviorsChanged(object sender, DependencyPropertyChangedEventArgs args) {
			var associatedObject = sender as FrameworkElement;
			if (associatedObject != null) {
				var oldList = args.OldValue as ObservableCollection<Behavior>;
				if (oldList != null) {
					foreach (var behavior in oldList) {
						behavior.AssociatedObject = null;
					}

					_collectionOwners.Remove(oldList.GetHashCode());
					oldList.CollectionChanged -= OnBehaviorsCollectionChanged;
				}

				var newList = args.NewValue as ObservableCollection<Behavior>;
				if (newList != null) {
					foreach (var behavior in newList) {
						behavior.AssociatedObject = sender as FrameworkElement;
					}

					_collectionOwners.Add(newList.GetHashCode(), new WeakReference<FrameworkElement>(associatedObject));
					newList.CollectionChanged += OnBehaviorsCollectionChanged;
				}
			}
		}

		private static void OnBehaviorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			FrameworkElement associatedObject;
			if (!_collectionOwners[sender.GetHashCode()].TryGetTarget(out associatedObject)) {
				return;
			}

			switch (e.Action) {
				case NotifyCollectionChangedAction.Add: {
						foreach (Behavior behavior in e.NewItems) {
							behavior.AssociatedObject = associatedObject;
						}
						break;
					}
				case NotifyCollectionChangedAction.Reset:
				case NotifyCollectionChangedAction.Remove: {
						foreach (Behavior behavior in e.OldItems) {
							behavior.AssociatedObject = null;
						}
						break;
					}
			}
		}
	}
}
