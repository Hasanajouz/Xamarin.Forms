﻿using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 56771, "Multi-item add in INotifyCollectionChanged causes a NSInternalInconsistencyException in bindings on iOS", PlatformAffected.iOS)]
	public class Bugzilla56771 : TestContentPage
	{
		const string Success = "Success";
		const string BtnAdd = "btnAdd";
		OptimizedCollection<string> data = new OptimizedCollection<string>();
		int i = 4;

		private void InitializeData()
		{

			data.Add("Item 1");
			data.Add("Item 2");
			data.Add("Item 3");
			data.Add("Item 4");
			BindingContext = data;
		}

		protected override void Init()
		{
			var label = new Label { Text = "Click the Add 2 button." };
			var button = new Button
			{
				Text = "Add 2",
				AutomationId = BtnAdd,
				Command = new Command(() =>
				{
					try
					{
						data.AddRange($"Item {++i}", $"Item {++i}");
					}
					catch (ArgumentException)
					{
						label.Text = Success;
					}
				})
			};
			var buttonI = new Button
			{
				Text = "Insert odd",
				AutomationId = BtnAdd,
				Command = new Command(() =>
				{
					try
					{
						for (int j = 1; j < data.Count; j += 2)
						{
							data.Insert(j, $"Item {++i} inserted");
						}
					}
					catch (ArgumentException)
					{
						label.Text = Success;
					}
				})
			};
			var button1 = new Button
			{
				Text = "Remove 2",
				Command = new Command(() =>
				{
					if (data.Count > 3)
					{
						data.RemoveRangeAt(2, 2);
					}
				})
			};
			var button2 = new Button
			{
				Text = "Clear",
				Command = new Command(() =>
				{
					data.RemoveRangeAt(0, data.Count);
				})
			};
			var listView = new ListView { };
			listView.SetBinding(ListView.ItemsSourceProperty, ".");

			Content = new StackLayout
			{
				Children = { label, button, buttonI, button1, button2, listView }
			};

			InitializeData();
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla56771Test()
		{
			RunningApp.WaitForElement(q => q.Marked(BtnAdd));
			RunningApp.Tap(q => q.Marked(BtnAdd)); 
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif

		[Preserve(AllMembers = true)]
		public class OptimizedCollection<T> : ObservableCollection<T>
		{
			public OptimizedCollection()
			{
			}

			protected override void ClearItems()
			{
				base.ClearItems();
			}

			public void AddRange(params T[] items)
			{
				InsertRangeAt(this.Count, items);
			}

			public void InsertRangeAt(int startIndex, params T[] items)
			{
				int idx = this.Count;
				for (int i = items.Length - 1; i >= 0; i--)
				{
					base.Items.Insert(startIndex, items[i]);
				}
				if (idx < Count)
				{
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems: items.ToList(), startingIndex: startIndex));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
					OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				}
			}

			public void RemoveRangeAt(int startIndex, int count)
			{
				if (count > 0)
				{
					List<T> removedItems = new List<T>(count);
					for (int i = 0; i < count; i++)
					{
						removedItems.Add(base.Items[startIndex]);
						base.Items.RemoveAt(startIndex);
					}
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems: removedItems, startingIndex: startIndex++));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
					OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				}
			}
		}
	}
}