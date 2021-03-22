using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FilesFinder.Annotations;
using FilesFinder.Commands;
using FilesFinder.Models;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FilesFinder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BackgroundWorker _bwFindFiles;
        private ObservableCollection<FoundFile> _foundFilesCollection;

        public ObservableCollection<FoundFile> FoundFilesCollection
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
            FoundFilesCollection = new ObservableCollection<FoundFile>();
            _bwFindFiles = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _bwFindFiles.DoWork += BwFindFilesOnDoWork;
            _bwFindFiles.ProgressChanged += BwFindFilesOnProgressChanged;
            _bwFindFiles.RunWorkerCompleted += BwFindFilesOnRunWorkerCompleted;
        }

        private void BwFindFilesOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            MessageBox.Show("Completed");
        }

        private void BwFindFilesOnProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            FoundFilesCollection.Add(args.UserState as FoundFile);
            ProgressFinding = args.ProgressPercentage;
        }

        private void BwFindFilesOnDoWork(object sender, DoWorkEventArgs e)
        {
            var searchOption = IncludeSubDirections == true
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            bool isReplacing = (bool) e.Argument;

            var files = GetFilesEnumerable().ToList();

            int i = 1;
            File.Delete("log.txt");
            foreach (var file in files)
            {
                if (_bwFindFiles.CancellationPending == true) break;
                double progressValue = (double)i / files.Count * 100;
                string textFile = File.ReadAllText(file);
                var bytesFile = File.ReadAllBytes(file);
                string med = Encoding.Default.GetString(bytesFile);
                int countSubText = CountMatches(textFile, FindText);

                if (countSubText == 0) continue;

                if (isReplacing)
                {
                    textFile = textFile.Replace(FindText, ReplaceText);
                    File.WriteAllText(file, textFile);
                }

                File.AppendAllText("log.txt", $"File: {file}\n{med}\n");


                _bwFindFiles.ReportProgress((int)progressValue, new FoundFile(file, countSubText));
                i++;
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }
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

        private string _replaceText;

        public string ReplaceText
        {
            get => _replaceText;
            set
            {
                _replaceText = value;
                OnPropertyChanged(nameof(ReplaceText));
            }
        }

        //Разделитель для масок ;
        private string _findText;

        public string FindText
        {
            get => _findText;
            set
            {
                _findText = value;
                OnPropertyChanged(nameof(FindText));
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

        public ICommand CancelWorkCommand => new RelayCommand(() =>
        {
            _bwFindFiles.CancelAsync();
        }, () => _bwFindFiles.IsBusy == true);

        public ICommand ReplaceCommand => new RelayCommand(() =>
        {
            FoundFilesCollection.Clear();
            //true - will find with replacing
            _bwFindFiles.RunWorkerAsync(true);
        }, () => string.IsNullOrWhiteSpace(ReplaceText) == false &&
                         string.IsNullOrWhiteSpace(FileMask) == false &&
                         string.IsNullOrWhiteSpace(FindText) == false &&
                         _bwFindFiles.IsBusy == false);

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
            //false - will just find without replacing
            _bwFindFiles.RunWorkerAsync(false);
        }, () => string.IsNullOrWhiteSpace(FileMask) == false &&
                         string.IsNullOrWhiteSpace(FindText) == false &&
                         _bwFindFiles.IsBusy == false);

        private int CountMatches(string str, string subStr)
        {
            int result = 0;
            result = (str.Length - str.Replace(subStr, "").Length) / subStr.Length;
            return result;
        }

        public IEnumerable<string> GetFilesEnumerable()
        {
            var searchOption = IncludeSubDirections == true
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;
            IEnumerable<string> result = null;
            string[] allFoundFiles = new string[0];
            string[] allExcludeFiles = new string[0];

            //Получаем все маски для поиска необходимых файлов
            var findMasks = FileMask.Replace(" ", "").Split(';');
            var excludeMasks = ExcludeMask?.Replace(" ", "").Split(';');
            foreach (var findMask in findMasks)
            {
                allFoundFiles = allFoundFiles.Concat(Directory.GetFiles(CurrentDirectory, findMask, searchOption)).ToArray();
            }

            if (string.IsNullOrWhiteSpace(ExcludeMask) == false)
            {
                foreach (var excludeMask in excludeMasks)
                {
                    allExcludeFiles = allExcludeFiles.Concat(Directory.GetFiles(CurrentDirectory, excludeMask, searchOption)).ToArray();
                }
            }
            
            result = string.IsNullOrWhiteSpace(ExcludeMask) == false
                ? allFoundFiles.Except(allExcludeFiles).AsEnumerable()
                : allFoundFiles.AsEnumerable();

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}