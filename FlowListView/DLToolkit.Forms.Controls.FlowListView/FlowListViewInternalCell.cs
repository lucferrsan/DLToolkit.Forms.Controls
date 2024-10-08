﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace DLToolkit.Forms.Controls
{
    /// <summary>
    /// Flow list view internal cell.
    /// </summary>
    [Helpers.FlowListView.Preserve(AllMembers = true)]
    public class FlowListViewInternalCell : ViewCell
    {
        readonly WeakReference<MauiFlowListView> _flowListViewRef;
        readonly AbsoluteLayout _rootLayout;
        readonly Grid _rootLayoutAuto;
        readonly bool _useGridAsMainRoot;
        int _desiredColumnCount;
        DataTemplate _flowColumnTemplate;
        FlowColumnExpand _flowColumnExpand;
        IList<DataTemplate> _currentColumnTemplates;

        /// <summary>
        /// Initializes a new instance of the <see cref="DLToolkit.Forms.Controls.FlowListViewInternalCell"/> class.
        /// </summary>
        /// <param name="flowListViewRef">Flow list view reference.</param>
        public FlowListViewInternalCell(WeakReference<MauiFlowListView> flowListViewRef)
        {
            _flowListViewRef = flowListViewRef;
            flowListViewRef.TryGetTarget(out MauiFlowListView flowListView);
            _useGridAsMainRoot = !flowListView.FlowUseAbsoluteLayoutInternally;

            if (!_useGridAsMainRoot)
            {
                _rootLayout = new AbsoluteLayout()
                {
                    Padding = 0d,
                    BackgroundColor = flowListView.FlowRowBackgroundColor,
                };
                View = _rootLayout;
            }
            else
            {
                _rootLayoutAuto = new Grid()
                {
                    RowSpacing = 0d,
                    ColumnSpacing = 0d,
                    Padding = 0d,
                    BackgroundColor = flowListView.FlowRowBackgroundColor,
                };
                View = _rootLayoutAuto;
            }

            _flowColumnTemplate = flowListView.FlowColumnTemplate;
            _desiredColumnCount = flowListView.FlowDesiredColumnCount;
            _flowColumnExpand = flowListView.FlowColumnExpand;

            View.GestureRecognizers.Clear();
            View.GestureRecognizers.Add(new TapGestureRecognizer());
        }

        private IList<DataTemplate> GetDataTemplates(IList container)
        {
            List<DataTemplate> templates = new List<DataTemplate>();

            if (_flowColumnTemplate is FlowTemplateSelector flowTemplateSelector)
            {
                _flowListViewRef.TryGetTarget(out MauiFlowListView flowListView);

                for (int i = 0; i < container.Count; i++)
                {
                    var template = flowTemplateSelector.SelectTemplate(container[i], i, flowListView);
                    templates.Add(template);
                }

                return templates;
            }

            if (_flowColumnTemplate is DataTemplateSelector templateSelector)
            {
                _flowListViewRef.TryGetTarget(out MauiFlowListView flowListView);

                for (int i = 0; i < container.Count; i++)
                {
                    var template = templateSelector.SelectTemplate(container[i], flowListView);
                    templates.Add(template);
                }

                return templates;
            }

            for (int i = 0; i < container.Count; i++)
            {
                templates.Add(_flowColumnTemplate);
            }

            return templates;
        }

        private bool RowLayoutChanged(int containerCount, IList<DataTemplate> templates, int columnCount)
        {
            // Check if desired number of columns is equal to current number of columns
            if (_currentColumnTemplates == null || containerCount != _currentColumnTemplates.Count)
            {
                return true;
            }

            // Check if desired column view types are equal to current columns view types
            for (int i = 0; i < containerCount; i++)
            {
                var currentTemplateType = _currentColumnTemplates[i].GetHashCode();
                var templateType = templates[i].GetHashCode();

                if (currentTemplateType != templateType)
                {
                    return true;
                }
            }

            if (_desiredColumnCount != columnCount)
            {
                return true;
            }

            return false;
        }

        private void SetBindingContextForView(View view, object bindingContext)
        {
            if (view != null && view.BindingContext != bindingContext)
                view.BindingContext = bindingContext;
        }

        void AddViewToLayoutAutoHeightDisabled(View view, int containerCount, int colNumber)
        {
            double desiredColumnWidth = 1d / _desiredColumnCount;
            Rect bounds = Rect.Zero;

            if (_flowColumnExpand != FlowColumnExpand.None && _desiredColumnCount > containerCount)
            {
                int diff = _desiredColumnCount - containerCount;
                bool isLastColumn = colNumber == containerCount - 1;

                switch (_flowColumnExpand)
                {
                    case FlowColumnExpand.First:

                        if (colNumber == 0)
                        {
                            bounds = new Rect(0d, 0d, desiredColumnWidth + (desiredColumnWidth * diff), 1d);
                        }
                        else if (isLastColumn)
                        {
                            bounds = new Rect(1d, 0d, desiredColumnWidth, 1d);
                        }
                        else
                        {
                            bounds = new Rect(desiredColumnWidth * (colNumber + diff) / (1d - desiredColumnWidth), 0d, desiredColumnWidth, 1d);
                        }

                        break;

                    case FlowColumnExpand.Last:

                        if (colNumber == 0)
                        {
                            bounds = new Rect(0d, 0d, desiredColumnWidth + (desiredColumnWidth * diff), 1d);
                        }
                        else if (isLastColumn)
                        {
                            bounds = new Rect(1d, 0d, desiredColumnWidth + (desiredColumnWidth * diff), 1d);
                        }
                        else
                        {
                            bounds = new Rect(desiredColumnWidth * colNumber / (1d - desiredColumnWidth), 0d, desiredColumnWidth, 1d);
                        }

                        break;

                    case FlowColumnExpand.Proportional:

                        double propColumnsWidth = 1d / containerCount;
                        if (colNumber == 0)
                        {
                            bounds = new Rect(0d, 0d, propColumnsWidth, 1d);
                        }
                        else if (isLastColumn)
                        {
                            bounds = new Rect(1d, 0d, propColumnsWidth, 1d);
                        }
                        else
                        {
                            bounds = new Rect(propColumnsWidth * colNumber / (1d - propColumnsWidth), 0d, propColumnsWidth, 1d);
                        }

                        break;

                    case FlowColumnExpand.ProportionalFirst:

                        int propFMod = _desiredColumnCount % containerCount;
                        double propFSize = desiredColumnWidth * Math.Floor((double)_desiredColumnCount / containerCount);
                        double propFSizeFirst = propFSize + desiredColumnWidth * propFMod;

                        if (colNumber == 0)
                        {
                            bounds = new Rect(0d, 0d, propFSizeFirst, 1d);
                        }
                        else if (isLastColumn)
                        {
                            bounds = new Rect(1d, 0d, propFSize, 1d);
                        }
                        else
                        {
                            bounds = new Rect(((propFSize * colNumber) + (propFSizeFirst - propFSize)) / (1d - propFSize), 0d, propFSize, 1d);
                        }

                        break;

                    case FlowColumnExpand.ProportionalLast:

                        int propLMod = _desiredColumnCount % containerCount;
                        double propLSize = desiredColumnWidth * Math.Floor((double)_desiredColumnCount / containerCount);
                        double propLSizeLast = propLSize + desiredColumnWidth * propLMod;

                        if (colNumber == 0)
                        {
                            bounds = new Rect(0d, 0d, propLSize, 1d);
                        }
                        else if (isLastColumn)
                        {
                            bounds = new Rect(1d, 0d, propLSizeLast, 1d);
                        }
                        else
                        {
                            bounds = new Rect((propLSize * colNumber) / (1d - propLSize), 0d, propLSize, 1d);
                        }

                        break;
                }
            }
            else
            {
                if (Math.Abs(1d - desiredColumnWidth) < Epsilon.DoubleValue)
                {
                    bounds = new Rect(1d, 0d, desiredColumnWidth, 1d);
                }
                else
                {
                    bounds = new Rect(desiredColumnWidth * colNumber / (1d - desiredColumnWidth), 0d, desiredColumnWidth, 1d);
                }
            }

            AbsoluteLayout.SetLayoutBounds(view, bounds);
            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
            _rootLayout.Children.Add(view);

            _rootLayout.Children.Add(view);
        }

        void AddViewToLayoutAutoHeightEnabled(View view, int containerCount, int colNumber)
        {
            if (_desiredColumnCount > containerCount)
            {
                int diff = _desiredColumnCount - containerCount;
                bool isLastColumn = colNumber == containerCount - 1;

                switch (_flowColumnExpand)
                {
                    case FlowColumnExpand.None:

                        _rootLayoutAuto.Children.Add(view);

                        break;

                    case FlowColumnExpand.First:

                        if (colNumber == 0)
                        {
                            _rootLayoutAuto.Children.Add(view);
                        }
                        else
                        {
                            _rootLayoutAuto.Children.Add(view);
                        }

                        break;

                    case FlowColumnExpand.Last:

                        if (isLastColumn)
                        {
                            _rootLayoutAuto.Children.Add(view);
                        }
                        else
                        {
                            _rootLayoutAuto.Children.Add(view);
                        }

                        break;

                    case FlowColumnExpand.Proportional:

                        int howManyP = _desiredColumnCount / containerCount - 1;
                        _rootLayoutAuto.Children.Add(view);

                        break;

                    case FlowColumnExpand.ProportionalFirst:

                        int firstSizeAdd = (int)((double)_desiredColumnCount) % containerCount; //1
                        int otherSize = (int)Math.Floor((double)_desiredColumnCount / containerCount); //2

                        if (colNumber == 0)
                            _rootLayoutAuto.Children.Add(view);
                        else
                            _rootLayoutAuto.Children.Add(view);

                        break;

                    case FlowColumnExpand.ProportionalLast:

                        int lastSizeAdd = (int)((double)_desiredColumnCount) % containerCount; //1
                        int otherSize1 = (int)Math.Floor((double)_desiredColumnCount / containerCount); //2

                        if (isLastColumn)
                        {
                            _rootLayoutAuto.Children.Add(view);
                        }
                        else
                        {
                            _rootLayoutAuto.Children.Add(view);
                        }

                        break;
                }
            }
            else
            {
                _rootLayoutAuto.Children.Add(view);
            }
        }

        /// <summary>
        /// Override this method to execute an action when the BindingContext changes.
        /// </summary>
        /// <remarks></remarks>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            UpdateData();

            if (BindingContext is INotifyCollectionChanged container)
            {
                container.CollectionChanged -= Container_CollectionChanged;
                container.CollectionChanged += Container_CollectionChanged;
            }
        }

        private void Container_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            if (!(BindingContext is IList container))
                return;

            var newDesiredColumnCount = 0;

            if (_flowListViewRef.TryGetTarget(out MauiFlowListView flowListView) && flowListView != null)
            {
                _flowColumnTemplate = flowListView.FlowColumnTemplate;
                newDesiredColumnCount = flowListView.FlowDesiredColumnCount;
                _flowColumnExpand = flowListView.FlowColumnExpand;
            }

            var flowGroupColumn = BindingContext as FlowGroupColumn;
            if (flowGroupColumn != null)
            {
                newDesiredColumnCount = flowGroupColumn.ColumnCount;
            }

            // Getting view types from templates
            var containerCount = container.Count;
            IList<DataTemplate> templates = GetDataTemplates(container);

            bool layoutChanged = false;
            if (flowGroupColumn != null && flowGroupColumn.ForceInvalidateColumns)
            {
                layoutChanged = true;
                flowGroupColumn.ForceInvalidateColumns = false;
            }
            else
            {
                layoutChanged = RowLayoutChanged(containerCount, templates, newDesiredColumnCount);
            }

            _desiredColumnCount = newDesiredColumnCount;

            if (!layoutChanged) // REUSE VIEWS
            {
                if (_useGridAsMainRoot)
                {
                    for (int i = 0; i < containerCount; i++)
                    {
                        var view = _rootLayoutAuto.Children
                            .FirstOrDefault(v => _rootLayoutAuto.GetRow(v) == 0 && _rootLayoutAuto.GetColumn(v) == i) as View;

                        if (view != null)
                        {
                            SetBindingContextForView(view, container[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < containerCount; i++)
                    {
                        var view = _rootLayoutAuto.Children
                            .FirstOrDefault(v => _rootLayoutAuto.GetRow(v) == 0 && _rootLayoutAuto.GetColumn(v) == i) as View;

                        if (view != null)
                        {
                            SetBindingContextForView(view, container[i]);
                        }
                    }
                }
            }
            else // RECREATE COLUMNS
            {
                _currentColumnTemplates = new List<DataTemplate>(templates);

                if (_useGridAsMainRoot)
                {
                    if (_rootLayoutAuto.Children.Count > 0)
                        _rootLayoutAuto.Children.Clear();

                    var colDefs = new ColumnDefinitionCollection();
                    for (int i = 0; i < _desiredColumnCount; i++)
                    {
                        colDefs.Add(new ColumnDefinition() { Width = new GridLength(1d, GridUnitType.Star) });
                    }
                    _rootLayoutAuto.ColumnDefinitions = colDefs;

                    for (int i = 0; i < containerCount; i++)
                    {
                        if (!(templates[i].CreateContent() is View view))
                            throw new InvalidCastException("DataTemplate must return a View");

                        AddTapGestureToView(view);

                        SetBindingContextForView(view, container[i]);
                        if (containerCount == 0 || _desiredColumnCount == 0)
                            return;

                        AddViewToLayoutAutoHeightEnabled(view, containerCount, i);
                    }
                }
                else
                {
                    if (_rootLayout.Children.Count > 0)
                        _rootLayout.Children.Clear();

                    for (int i = 0; i < containerCount; i++)
                    {
                        if (!(templates[i].CreateContent() is View view))
                            throw new InvalidCastException("DataTemplate must return a View");

                        AddTapGestureToView(view);

                        SetBindingContextForView(view, container[i]);
                        if (containerCount == 0 || _desiredColumnCount == 0)
                            return;

                        AddViewToLayoutAutoHeightDisabled(view, containerCount, i);
                    }
                }
            }
        }

        void AddTapGestureToView(View view)
        {
            var command = new Command(async (obj) =>
            {
                await ExecuteTapGestureRecognizer(view);
            });

            view.GestureRecognizers.Add(new TapGestureRecognizer() { Command = command });
            view.GestureRecognizers.Add(new ClickGestureRecognizer() { Command = command, Buttons = ButtonsMask.Primary, NumberOfClicksRequired = 1 });
        }

        async Task ExecuteTapGestureRecognizer(View view)
        {
            if (view is IFlowViewCell flowCell)
            {
                flowCell.OnTapped();
            }

            _flowListViewRef.TryGetTarget(out MauiFlowListView flowListView);

            if (flowListView != null)
            {
                int tapBackgroundEffectDelay = flowListView.FlowTappedBackgroundDelay;

                try
                {
                    if (tapBackgroundEffectDelay != 0)
                    {
                        view.BackgroundColor = flowListView.FlowTappedBackgroundColor;
                    }

                    flowListView.FlowPerformTap(view, view.BindingContext);
                }
                finally
                {
                    if (tapBackgroundEffectDelay != 0)
                    {
                        await Task.Delay(tapBackgroundEffectDelay);
                        view.BackgroundColor = flowListView.FlowRowBackgroundColor;
                    }
                }
            }
        }
    }
}

