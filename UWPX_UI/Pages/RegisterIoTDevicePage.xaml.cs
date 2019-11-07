﻿using System;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Shared.Classes;
using UWPX_UI.Controls;
using UWPX_UI.Controls.IoT;
using UWPX_UI.Controls.Settings;
using UWPX_UI_Context.Classes;
using UWPX_UI_Context.Classes.DataContext.Pages;
using UWPX_UI_Context.Classes.DataTemplates;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XMPP_API.Classes.XmppUri;
using XMPP_API_IoT.Classes.Bluetooth.Events;

namespace UWPX_UI.Pages
{
    public sealed partial class RegisterIoTDevicePage: Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly RegisterIoTDevicePageContext VIEW_MODEL = new RegisterIoTDevicePageContext();

        private readonly ObservableCollection<SettingsPageButtonDataTemplate> DEVICE_TYPES = new ObservableCollection<SettingsPageButtonDataTemplate>
        {
            new SettingsPageButtonDataTemplate {Glyph = "\uE957", Name = "Standalone", Description = "Standalone devices", NavTarget = null},
            new SettingsPageButtonDataTemplate {Glyph = "\uF22C", Name = "Hub Based", Description = "Devices, that connect to a device hub", NavTarget = null},
        };

        private FrameworkElement LastPopUpElement = null;
        private string curViewState;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public RegisterIoTDevicePage()
        {
            InitializeComponent();
            UpdateViewState(State_0.Name);
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void UpdateViewState(string state)
        {
            if (VisualStateManager.GoToState(this, state, true))
            {
                curViewState = state;
            }
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            titleBar.OnPageNavigatedTo();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            titleBar.OnPageNavigatedFrom();
        }

        private async void WhatIsAnIoTDevice_link_Click(object sender, RoutedEventArgs e)
        {
            await UiUtils.LaunchUriAsync(new Uri(Localisation.GetLocalizedString("RegisterIoTDevicePage_what_is_an_iot_device_url")));
        }

        private async void Cancel_ibtn_Click(IconButtonControl sender, RoutedEventArgs args)
        {
            await qrCodeScanner.StopAsync();
            titleBar.OnGoBackRequested();
        }

        private async void Retry_ibtn_Click(IconButtonControl sender, RoutedEventArgs args)
        {
            if (string.Equals(curViewState, State_1.Name))
            {
                await qrCodeScanner.StopAsync();
            }

            UpdateViewState(State_0.Name);
        }

        private void BtScanner_btsc_DeviceChanged(BluetoothScannerControl sender, BLEDeviceEventArgs args)
        {
            if (!(args.DEVICE is null))
            {
                UpdateViewState(State_3.Name);
            }
        }

        private void Send3_ibtn_Click(IconButtonControl sender, RoutedEventArgs args)
        {

        }

        private void SettingsSelectionControl_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!(DeviceFamilyHelper.IsMouseInteractionMode() && sender is FrameworkElement deviceModeSelection))
            {
                return;
            }

            LastPopUpElement = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(deviceModeSelection) as FrameworkElement) as FrameworkElement;
            Canvas.SetZIndex(LastPopUpElement, 10);
            LastPopUpElement.Scale(scaleX: 1.05f, scaleY: 1.05f, easingType: EasingType.Sine).Start();
        }

        private void SettingsSelectionControl_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (LastPopUpElement is null)
            {
                return;
            }

            Canvas.SetZIndex(LastPopUpElement, 0);
            LastPopUpElement.Scale(easingType: EasingType.Sine).Start();
            LastPopUpElement = null;
        }

        private async void SettingsSelectionLargeControl_Click(SettingsSelectionLargeControl sender, RoutedEventArgs args)
        {
            UpdateViewState(State_1.Name);
            VIEW_MODEL.MODEL.RegisterIoTUriAction = null;
            await qrCodeScanner.StartAsync();

        }

        private async void SettingsSelectionSmallControl_Click(SettingsSelectionSmallControl sender, RoutedEventArgs args)
        {
            UpdateViewState(State_1.Name);
            VIEW_MODEL.MODEL.RegisterIoTUriAction = null;
            await qrCodeScanner.StartAsync();
        }

        private async void QrCodeScanner_NewValidQrCode(QrCodeScannerControl sender, UWPX_UI_Context.Classes.Events.NewQrCodeEventArgs args)
        {
            IUriAction action = UriUtils.parse(new Uri(args.QR_CODE));
            if (action is RegisterIoTUriAction registerIoTUriAction)
            {
                await SharedUtils.CallDispatcherAsync(async () =>
                {
                    await qrCodeScanner.StopAsync();

                    UpdateViewState(State_2.Name);
                    VIEW_MODEL.MODEL.RegisterIoTUriAction = registerIoTUriAction;
                });
            }
        }

        #endregion
    }
}
