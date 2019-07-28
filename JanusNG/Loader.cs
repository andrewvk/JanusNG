using System;
using System.Windows;
using System.Windows.Controls;
using CodeJam;

namespace Rsdn.JanusNG
{
	public static class Loader
	{
		public static IDisposable ApplyLoader(this Control control)
		{
			var oldStyle = control.Style;
			control.Style = (Style)Application.Current.Resources["BusyAnimationStyle"];
      return Disposable.Create(() => control.Style = oldStyle);
		}
	}
}