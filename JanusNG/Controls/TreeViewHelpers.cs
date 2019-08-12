using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CodeJam;

namespace Rsdn.JanusNG.Controls
{
	public static class TreeViewHelpers
	{
		public static void SelectItem(this TreeView treeView, params object[] itemPath)
		{
			Code.NotNull(treeView, nameof(treeView));
			Code.NotNull(itemPath, nameof(itemPath));

			ItemsControl container = treeView;
			TreeViewItem tvi = null;
			foreach (var node in itemPath)
			{
				if (container.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
					container.UpdateLayout();
				tvi = (TreeViewItem) container.ItemContainerGenerator.ContainerFromItem(node);
				if (tvi == null)
				{
					MessageBox.Show($"Oops! {container.ItemContainerGenerator.Status}");
					return;
				}
				tvi.IsExpanded = true;
				container = tvi;
			}

			if (tvi != null)
			{
				tvi.IsSelected = true;
				tvi.BringIntoView();
			}
		}
	}
}