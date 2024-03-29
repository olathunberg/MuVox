﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using TTech.MuVox.Features.Messages;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;
using TTech.MuVox.Helpers;

namespace TTech.MuVox
{
    public class MainWindowModel : ViewModelBase
    {
        private readonly ViewModelLocator viewModelLocator = (ViewModelLocator)Application.Current.Resources["ViewModelLocator"];
        private ViewModelBase? _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel ?? viewModelLocator.Recorder; }
            set
            {
                if (_currentViewModel == value)
                    return;
                Set(() => CurrentViewModel, ref _currentViewModel, value);
            }
        }

        public MainWindowModel()
        {
            Shared.HotKeyManager.RegisterHotKey(System.Windows.Forms.Keys.F3, Shared.KeyModifiers.Alt);
            Shared.HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            CurrentViewModel = viewModelLocator.Recorder;

            Messenger.Default.Register<GotoPageMessage>(
                this, (action) =>
                {
                    if (action.GotoPage == Pages.Recorder)
                        CurrentViewModel = viewModelLocator.Recorder;
                    if (action.GotoPage == Pages.Processor)
                        CurrentViewModel = viewModelLocator.Processor;
                    if (action.GotoPage == Pages.Editor)
                        CurrentViewModel = viewModelLocator.Editor;
                    if (action.GotoPage == Pages.Settings)
                        CurrentViewModel = viewModelLocator.Settings;
                });
        }

        public string TitleText => $"TTech - MuVox {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)} '{Assembly.GetExecutingAssembly().GetLinkerTime().ToShortDateString()}'";

        public bool CanStartRecording => viewModelLocator.Recorder.Recorder.RecordingState != Features.Recorder.RecordingState.Monitoring;

        private RelayCommand<CancelEventArgs>? windowClosingCommand;
        public ICommand WindowClosingCommand
        {
            get
            {
                return windowClosingCommand ?? (windowClosingCommand = new RelayCommand<CancelEventArgs> (
                    args =>
                    {
                        if (viewModelLocator.Recorder.Recorder.RecordingState != Features.Recorder.RecordingState.Monitoring)
                        {
                            args.Cancel = true;
                        }
                    }));
            }
        }

        private RelayCommand? showSettingsCommand;
        public ICommand ShowSettingsCommand
        {
            get
            {
                return showSettingsCommand ?? (showSettingsCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send(new GotoPageMessage(Pages.Settings));
                    },
                    () => true));
            }
        }

        private RelayCommand? setMarkerCommand;
        public ICommand SetMarkerCommand
        {
            get
            {
                return setMarkerCommand ?? (setMarkerCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send(new SetMarkerMessage());
                    },
                    () => true));
            }
        }

        private RelayCommand? startRecordningCommand;
        public ICommand StartRecordningCommand
        {
            get
            {
                return startRecordningCommand ?? (startRecordningCommand = new RelayCommand(
                    async () =>
                    {
                        Messenger.Default.Send(new StartRecordingMessage());
                        await System.Threading.Tasks.Task.Delay(500);
                        RaisePropertyChanged(() => CanStartRecording);
                    },
                    () => true));
            }
        }

        private void HotKeyManager_HotKeyPressed(object sender, Shared.HotKeyEventArgs e)
        {
            Messenger.Default.Send(new SetMarkerMessage());
        }

        public override void Cleanup()
        {
            base.Cleanup();

            viewModelLocator.Recorder.Cleanup();
            viewModelLocator.Processor.Cleanup();
            viewModelLocator.Editor.Cleanup();
        }
    }
}
