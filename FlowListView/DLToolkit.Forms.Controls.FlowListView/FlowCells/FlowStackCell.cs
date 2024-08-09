using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DLToolkit.Forms.Controls
{
	/// <summary>
	/// FlowListView stack cell.
	/// </summary>
	[Helpers.FlowListView.Preserve(AllMembers = true)]
    public class FlowStackCell : StackLayout, IFlowViewCell
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DLToolkit.Forms.Controls.FlowStackCell"/> class.
		/// </summary>
		public FlowStackCell()
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

