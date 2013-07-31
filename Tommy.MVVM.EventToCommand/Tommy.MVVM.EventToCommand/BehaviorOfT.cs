using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Tommy.MVVM.EventToCommand.Behavior {
	public abstract class Behavior<T> : Behavior where T : FrameworkElement {
		protected Behavior() {
		}

		public T AssociatedObject {
			get {
				return (T)base.AssociatedObject;
			}
			set {
				base.AssociatedObject = value;
			}
		}
	}
}
