using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Tommy.MVVM.EventToCommand.Behavior;

namespace Tommy.MVVM.EventToCommand
{
    public class EventToCommandBehavior : Behavior<FrameworkElement>
    {
        public bool PassEventArgsToCommand { get; set; }

        public EventToCommandBehavior() {
            PassEventArgsToCommand = true;            
        }

        protected override void OnAttached()
        {
            var evt = AssociatedObject.GetType().GetRuntimeEvent(Event);
            if (evt != null)
            {
                MethodInfo addMethod = evt.AddMethod;
                MethodInfo removeMethod = evt.RemoveMethod;
                ParameterInfo[] addParameters = addMethod.GetParameters();
                Type delegateType = addParameters[0].ParameterType;
                Action<object, object> handler = (s, e) => FireCommand(e);
                MethodInfo handlerInvoke = typeof(Action<object, object>).GetRuntimeMethod("Invoke", new[] { typeof(object), typeof(object) });
                Delegate @delegate = handlerInvoke.CreateDelegate(delegateType, handler);

                Func<object, EventRegistrationToken> add = a => (EventRegistrationToken)addMethod.Invoke(AssociatedObject, new object[] { @delegate });
                Action<EventRegistrationToken> remove = t => removeMethod.Invoke(AssociatedObject, new object[] { t });

                WindowsRuntimeMarshal.AddEventHandler(add, remove, handler);
            }
        }

        private void FireCommand(object e)
        {
            var dataContext = AssociatedObject.DataContext;
            if (dataContext != null)
            {
                var dcType = dataContext.GetType();
                var commandGetter = dcType.GetRuntimeMethod("get_" + Command, new Type[0]);
                if (commandGetter != null)
                {
                    var command = commandGetter.Invoke(dataContext, null) as ICommand;
                    if (command != null && command.CanExecute(CommandParameter))
                    {
                        if (PassEventArgsToCommand && CommandParameter == null)
                        {
                            command.Execute(e);
                        }
                        else
                        {
                            command.Execute(CommandParameter);
                        }
                    }
                }
            }
        }

        #region Event

        /// <summary>
        /// Event Property name
        /// </summary>
        public const string EventPropertyName = "Event";

        public string Event
        {
            get { return (string)GetValue(EventProperty); }
            set { SetValue(EventProperty, value); }
        }

        /// <summary>
        /// Event Property definition
        /// </summary>
        public static readonly DependencyProperty EventProperty = DependencyProperty.Register(
            EventPropertyName,
            typeof(string),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(default(string)));

        #endregion

        #region Command

        /// <summary>
        /// Command Property name
        /// </summary>
        public const string CommandPropertyName = "Command";

        public string Command
        {
            get { return (string)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Command Property definition
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            CommandPropertyName,
            typeof(string),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(default(string)));

        #endregion

        #region CommandParameter

        /// <summary>
        /// CommandParameter Property name
        /// </summary>
        public const string CommandParameterPropertyName = "CommandParameter";

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// CommandParameter Property definition
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            CommandParameterPropertyName,
            typeof(object),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(default(object)));

        #endregion
    }
}
