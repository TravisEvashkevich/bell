﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioTool.Core;
using AudioTool.Data;
using Microsoft.Practices.ServiceLocation;

namespace AudioTool.ViewModel
{
    public class SearchFilterVM : MainViewModel
    {

        private string _searchText;

        private bool _excludeApproved;
        public bool ExcludeApproved
        {
            get { return _excludeApproved; }
            set
            {
                Set(ref _excludeApproved, value);
                ExecuteSearchCommand(null);
            }
        }

        public SearchFilterVM()
        {
        }

        //The string entered in the searchbox
        public string SearchText { get { return _searchText; } set { Set(ref _searchText, value); } }

        public SmartCommand<object> SearchCommand { get; private set; }

        public void ExecuteSearchCommand(object o)
        {
            var instance = ServiceLocator.Current.GetInstance<MainWindowVM>();
            if (instance.Documents == null)
                return;
            if(instance.Documents.Count == 0) return;

            var start = instance.Documents[0];
            ApplyCriteria(SearchText, new Stack<NodeWithName>(), start);
            //instance.Documents[0].IsExpanded = true;
        }

        private bool IsCriteriaMatched(string criteria, NodeWithName check)
        {
            if (!ExcludeApproved)
                return String.IsNullOrEmpty(criteria) || check.Name.ToLower().Contains(criteria.ToLower());

            if (check.Approved)
                return false;

            return String.IsNullOrEmpty(criteria) || check.Name.ToLower().Contains(criteria.ToLower());
        }

        public void ApplyCriteria(string criteria, Stack<NodeWithName> ancestors, NodeWithName startPoint)
        {

            if (IsCriteriaMatched(criteria, startPoint))
            {
                startPoint.IsVisible = true;
                foreach (var ancestor in ancestors)
                {
                    ancestor.IsVisible = true;
                    ancestor.Expanded = !String.IsNullOrEmpty(criteria);
                }
            }
            else
                startPoint.IsVisible = false;
            
            ancestors.Push(startPoint);
            foreach (var child in startPoint.Children)
                if (child != null && child.GetType() != typeof(Sound))
                    ApplyCriteria(criteria, ancestors, child as NodeWithName);

            ancestors.Pop();
        }

        protected override void InitializeCommands()
        {
            SearchCommand = new SmartCommand<object>(ExecuteSearchCommand);
        }

    }
}
