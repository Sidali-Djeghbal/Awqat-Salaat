using AwqatSalaat.Helpers;
using AwqatSalaat.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using System;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AwqatSalaat.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarPage : Page
    {
        public static readonly DependencyProperty InViewDateProperty = DependencyProperty.Register(
            nameof(InViewDate),
            typeof(DateTime),
            typeof(CalendarView),
            new PropertyMetadata(new DateTime(2000, 1, 1)));

        public DateTime InViewDate { get => (DateTime)GetValue(InViewDateProperty); set => SetValue(InViewDateProperty, value); }

        private CalendarViewModel ViewModel => DataContext as CalendarViewModel;

        public CalendarPage()
        {
            this.InitializeComponent();
            ViewModel.Result.PropertyChanged += Result_PropertyChanged;
            listBox.Loaded += ListBox_Loaded;

            // Workaround for a bug https://github.com/microsoft/microsoft-ui-xaml/issues/4035
            gregorianCombobox.RegisterPropertyChangedCallback(ComboBox.ItemsSourceProperty, OnItemsSourceChanged);
            hijriCombobox.RegisterPropertyChangedCallback(ComboBox.ItemsSourceProperty, OnItemsSourceChanged);
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("Calendar page loaded");
            listBox.Loaded -= ListBox_Loaded;
            listBox.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            Log.Debug("[Calendar] Scroll changed");
            UpdateInViewDate(listBox);
        }

        // Workaround for a bug https://github.com/microsoft/microsoft-ui-xaml/issues/4035
        private static void OnItemsSourceChanged(DependencyObject sender, DependencyProperty dp)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox.ItemsSource is not null)
            {
                comboBox.SelectedValuePath = null;
                comboBox.SelectedValuePath = "Id";
            }
        }

        private void Result_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalendarResult.HasData))
            {
                UpdateInViewDate(listBox);

                if (listBox.Items.Count > 0)
                {
                    listBox.ResetScroll();
                }
            }
        }

        private void UpdateInViewDate(ListBox listBox)
        {
            Log.Debug("[Calendar] Updating displayed date");

            if (listBox.Items.Count > 0)
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

        private static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (element.Visibility == Visibility.Collapsed)
                return false;

            Rect bounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(new Point(bounds.Left, bounds.Top)) || rect.Contains(new Point(bounds.Right, bounds.Bottom));
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Clicked on Calendar Export");
            var vm = new CalendarExportViewModel { CalendarResult = ViewModel.Result };
            var window = new CalendarExportWindow(vm);
            window.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(385, 675));
            window.Activate();
        }
    }

    internal class RecordContainerStyleSelector : StyleSelector
    {
        public Style Standard { get; set; }
        public Style Today { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is CalendarRecord record)
            {
                return record.Date.Date == TimeStamp.Date ? Today : Standard;
            }

            return base.SelectStyleCore(item, container);
        }
    }
}
