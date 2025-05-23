﻿using AwqatSalaat.Helpers;
using AwqatSalaat.ViewModels;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AwqatSalaat.UI.Views
{
    /// <summary>
    /// Interaction logic for CalendarView.xaml
    /// </summary>
    public partial class CalendarView : UserControl
    {
        public static readonly DependencyProperty InViewDateProperty = DependencyProperty.Register(
            nameof(InViewDate),
            typeof(DateTime),
            typeof(CalendarView),
            new PropertyMetadata(new DateTime(2000, 1, 1)));

        public DateTime InViewDate { get => (DateTime)GetValue(InViewDateProperty); set => SetValue(InViewDateProperty, value); }

        private CalendarViewModel ViewModel => DataContext as CalendarViewModel;

        public CalendarView()
        {
            InitializeComponent();
            ViewModel.Result.PropertyChanged += Result_PropertyChanged;
        }

        private void Result_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalendarResult.HasData))
            {
                UpdateInViewDate(listBox);

                if (listBox.HasItems)
                {
                    listBox.ScrollIntoView(listBox.Items[0]);
                }
            }
        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Log.Debug("[Calendar] Scroll changed");
            var listBox = (ListBox)sender;

            UpdateInViewDate(listBox);
        }

        private void UpdateInViewDate(ListBox listBox)
        {
            Log.Debug("[Calendar] Updating displayed date");

            if (listBox.HasItems)
            {
                Log.Debug($"listbox has {listBox.Items.Count} item(s)");
                DateTime first = ViewModel.Result.PrayerTimes[0].Date;

                foreach (var time in ViewModel.Result.PrayerTimes)
                {
                    var container = listBox.ItemContainerGenerator.ContainerFromItem(time) as ListBoxItem;

                    if (container != null && IsUserVisible(container, listBox))
                    {
                        first = time.Date;
                        break;
                    }
                }

                InViewDate = first;
                Log.Debug($"Determined date: {first:u}");
            }
        }

        private bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            Log.Information("Clicked on Calendar Export");
            var window = new CalendarExportWindow()
            {
                DataContext = new CalendarExportViewModel { CalendarResult = ViewModel.Result },
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }
    }

    internal class RecordContainerStyleSelector : StyleSelector
    {
        public Style Standard { get; set; }
        public Style Today { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is CalendarRecord record)
            {
                return record.Date.Date == TimeStamp.Date ? Today : Standard;
            }

            return base.SelectStyle(item, container);
        }
    }
}
