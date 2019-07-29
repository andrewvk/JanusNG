using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Main
{
	public class MessageNode : INotifyPropertyChanged
	{
		private MessageNode[] _children;
		public MessageInfo Message { get; set; }

		public MessageNode[] Children
		{
			get => _children;
			set
			{
				_children = value;
				OnPropertyChanged(nameof(Children));
			}
		}

		public int Level { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}