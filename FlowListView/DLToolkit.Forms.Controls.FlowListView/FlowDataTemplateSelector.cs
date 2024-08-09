using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DLToolkit.Forms.Controls
{
    [Helpers.FlowListView.Preserve(AllMembers = true)]
    public class FlowDataTemplateSelector : DataTemplateSelector
    {
        readonly WeakReference<MauiFlowListView> _flowListViewRef;

        private readonly DataTemplate _defaultTemplate;

        public FlowDataTemplateSelector(WeakReference<MauiFlowListView> flowListViewRef)
        {
            _flowListViewRef = flowListViewRef;
            _defaultTemplate = new DataTemplate(() => new FlowListViewInternalCell(flowListViewRef));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is IFlowLoadingModel)
            {
                if (_flowListViewRef.TryGetTarget(out MauiFlowListView flowListView))
                {
                    return flowListView.FlowLoadingTemplate;
                }
            }
            else if (item is IFlowEmptyModel)
            {
                if (_flowListViewRef.TryGetTarget(out MauiFlowListView flowListView))
                {
                    return flowListView.FlowEmptyTemplate;
                }
            }

            return _defaultTemplate;
        }
    }
}
