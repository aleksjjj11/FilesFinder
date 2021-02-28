using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FilesFinder.Annotations;
using FilesFinder.Commands;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FilesFinder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BackgroundWorker bwFindFiles;
        private ObservableCollection<string> _foundFilesCollection;

        public ObservableCollection<string> FoundFilesCollection
        {
            get => _foundFilesCollection;
            set
            {
                _foundFilesCollection = value;
                OnPropertyChanged(nameof(FoundFilesCollection));
            }
        }

        public MainViewModel()
        {
            FoundFilesCollection = new ObservableCollection<string>();
            bwFindFiles = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            bwFindFiles.DoWork += (sender, args) =>
            {
                var searchOption = IncludeSubDirections == true
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                var files = Directory.GetFiles(CurrentDirectory, FileMask, searchOption);
                int i = 1;
                foreach (var file in files)
                {
                    if (bwFindFiles.CancellationPending == true) break;
                    double progressValue = (double)i / files.Length * 100;
                    bwFindFiles.ReportProgress((int)progressValue, file);
                    i++;
                    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                }
            };

            bwFindFiles.ProgressChanged += (sender, args) =>
            {
                FoundFilesCollection.Add(args.UserState as string);
                ProgressFinding = args.ProgressPercentage;
            };

            bwFindFiles.RunWorkerCompleted += (sender, args) =>
            {
                MessageBox.Show("Completed");
            };
        }

        private int _progressFinding;

        public int ProgressFinding
        {
            get => _progressFinding;
            set
            {
                _progressFinding = value;
                OnPropertyChanged(nameof(ProgressFinding));
            }
        }

        //Разделитель для масок ;
        private string _findFiles;

        public string FindFiles
        {
            get => _findFiles;
            set
            {
                _findFiles = value;
                OnPropertyChanged(nameof(FindFiles));
            }
        }
        private string _excludeMask;

        public string ExcludeMask
        {
            get => _excludeMask;
            set
            {
                _excludeMask = value;
                OnPropertyChanged(nameof(ExcludeMask));
            }
        }

        private string _fileMask;

        public string FileMask
        {
            get => _fileMask;
            set
            {
                _fileMask = value;
                OnPropertyChanged(nameof(FileMask));
            }
        }

        private string _currentDirectory;
        public string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                _currentDirectory = value;
                OnPropertyChanged(nameof(CurrentDirectory));
            }
        }

        private bool _includeSubDirections;
        public bool IncludeSubDirections
        {
            get => _includeSubDirections;
            set
            {
                _includeSubDirections = value;
                OnPropertyChanged(nameof(IncludeSubDirections));
            }
        }

        public ICommand OpenDirectoryCommand => new RelayCommand(() =>
        {
            //Открываем директорию и сохраняем её
            CommonOpenFileDialog dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false
            }; 

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                CurrentDirectory = dlg.FileName;
            }
        }, () => true);

        public ICommand FindFilesCommand => new RelayCommand(() =>
        {
            FoundFilesCollection.Clear();
            bwFindFiles.RunWorkerAsync();
        }, () => bwFindFiles.IsBusy == false);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}