using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
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
        public ObservableCollection<string> FoundFilesCollection { get; }

        public MainViewModel()
        {
            FoundFilesCollection = new ObservableCollection<string>();
            bwFindFiles = new BackgroundWorker();
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
            var files = Directory.GetFiles(CurrentDirectory, FileMask, SearchOption.AllDirectories);

        }, () => string.IsNullOrWhiteSpace(FindFiles) && string.IsNullOrWhiteSpace(FileMask));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}