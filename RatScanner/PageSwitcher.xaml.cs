﻿using RatScanner.View;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace RatScanner
{
	/// <summary>
	/// Interaction logic for PageSwitcher.xaml
	/// </summary>
	public partial class PageSwitcher : Window
	{
		private const int WindowWidth = 280;
		private const int WindowHeight = 390;

		private NotifyIcon _notifyIcon;

		private static PageSwitcher _instance;
		public static PageSwitcher Instance => _instance ??= new PageSwitcher();

		public PageSwitcher()
		{
			try
			{
				_instance = this;

				InitializeComponent();
				ResetWindowSize();
				Navigate(new MainMenu());
				AddTrayIcon();

				Topmost = RatConfig.AlwaysOnTop;
			}
			catch (Exception e)
			{
				Logger.LogError(e.Message, e);
			}
		}

		internal void ResetWindowSize()
		{
			SizeToContent = SizeToContent.Manual;
			Width = WindowWidth;
			Height = WindowHeight;
		}

		internal void Navigate(UserControl nextPage)
		{
			ContentControl.Content = nextPage;
		}

		internal void Navigate(UserControl nextPage, object state)
		{
			ContentControl.Content = nextPage;

			if (nextPage is ISwitchable s) s.UtilizeState(state);
			else
			{
				throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name);
			}
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (RatConfig.MinimizeToTray && WindowState == WindowState.Minimized) Hide();

			base.OnStateChanged(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();

			base.OnClosed(e);

			Application.Current.Shutdown();
		}

		private void AddTrayIcon()
		{
			_notifyIcon = new NotifyIcon
			{
				Text = "Show",
				Visible = true,
				Icon = Properties.Resources.RatLogoSmall,
			};

			_notifyIcon.Click += delegate
			{
				Show();
				WindowState = WindowState.Normal;
			};
		}

		private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left) DragMove();
		}

		private void OnTitleBarMinimize(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void OnTitleBarMinimal(object sender, RoutedEventArgs e)
		{
			CollapseTitleBar();
			SizeToContent = SizeToContent.WidthAndHeight;
			SetBackgroundOpacity(RatConfig.MinimalUi.Opacity / 100d);
			Navigate(new MinimalMenu());
		}

		private void OnTitleBarClose(object sender, RoutedEventArgs e)
		{
			Close();
		}

		internal void CollapseTitleBar()
		{
			TitleBar.Visibility = Visibility.Collapsed;
		}

		internal void ShowTitleBar()
		{
			TitleBar.Visibility = Visibility.Visible;
		}

		internal void SetBackgroundOpacity(double opacity)
		{
			opacity = Math.Clamp(opacity, 0, 1);
			Background.Opacity = opacity;
		}
	}
}
