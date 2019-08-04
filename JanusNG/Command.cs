using System;
using System.Windows.Input;
using CodeJam;
using JetBrains.Annotations;

namespace Rsdn.JanusNG
{
	public class Command : ICommand
	{
		[CanBeNull] private readonly Func<object, bool> _canExecute;

		[NotNull] private readonly Action<object> _execute;

		public Command(Action<object> execute, Func<object, bool> canExecute = null)
		{
			Code.NotNull(execute, nameof(execute));
			_execute = execute;
			_canExecute = canExecute;
		}

		public Command(Action execute, Func<bool> canExecute = null) :
			this(
				p => execute(),
				canExecute == null ? (Func<object, bool>) null : p => canExecute())
		{
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public event EventHandler CanExecuteChanged;

		public void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}