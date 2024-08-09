using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DLToolkit.Forms.Controls
{
	/// <summary>
	/// FlowListView grid cell.
	/// </summary>
	[Helpers.FlowListView.Preserve(AllMembers = true)]
    public class FlowGridCell : Grid, IFlowViewCell
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DLToolkit.Forms.Controls.FlowGridCell"/> class.
		/// </summary>
		public FlowGridCell()
		{
		}

		/// <summary>
		/// Raised when cell is tapped.
		/// </summary>
		public virtual void OnTapped()
		{
		}
	}
}

