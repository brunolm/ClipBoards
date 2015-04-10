using ClipBoards.Models;
using ClipBoards.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace ClipBoards.ViewModels
{
    [PropertyChanged.ImplementPropertyChanged]
    public class MainViewModel
    {
        private const string saveFileName = "save.txt";

        public MainViewModel()
        {
            LoadSettings();

            CopyCommand = new RelayCommand<ClipItem>(CopyCommandExecute);

            AddCommand = new RelayCommand(o =>
            {
                SelectedItem = new ClipItem();
                ClipItems.Add(SelectedItem);
            });

            RemoveCommand = new RelayCommand(o =>
            {
                ClipItems.Remove(SelectedItem);
            }, o => SelectedItem != null);

            ToggleAlwaysOnTopCommand = new RelayCommand<Window>(wnd =>
            {
                wnd.Topmost = Settings.Default.AlwaysOnTop;
            });
        }

        public ICommand AddCommand { get; set; }

        public ObservableCollection<ClipItem> ClipItems { get; set; }

        public ICommand CopyCommand { get; set; }

        public ICommand RemoveCommand { get; set; }

        public ClipItem SelectedItem { get; set; }

        public ICommand ToggleAlwaysOnTopCommand { get; set; }

        private void CopyCommandExecute(ClipItem item)
        {
            if (item.Content == null)
                return;

            if (item.IsFile)
            {
                if (File.Exists(item.Content))
                {
                    switch (Path.GetExtension(item.Content).ToLower())
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".gif":
                        case ".bmp":
                            CopyFileImage(item);
                            break;

                        case ".txt":
                        case ".log":
                        case ".cfg":
                        case ".csv":
                        case ".html":
                        case ".xaml":
                        case ".xml":
                        case ".md":
                        case ".tex":
                        case ".sql":
                        case ".ini":
                            CopyFileText(item);
                            break;

                        default:
                            CopyFileDrop(item);
                            break;
                    }

                }
            }
            else
            {
                Clipboard.SetText(item.Content);
            }
        }

        private void CopyFileDrop(ClipItem item)
        {
            var data = Clipboard.GetData(System.Windows.DataFormats.FileDrop);
            Clipboard.SetData(System.Windows.DataFormats.FileDrop, new string[] { item.Content });
        }

        private void CopyFileImage(ClipItem item)
        {
            Bitmap img = Bitmap.FromFile(item.Content) as Bitmap;

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                img.GetHbitmap()
                , IntPtr.Zero
                , System.Windows.Int32Rect.Empty
                , BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

            Clipboard.SetImage(source);

            img.Dispose();
        }

        private void CopyFileText(ClipItem item)
        {
            Clipboard.SetText(File.ReadAllText(item.Content));
        }

        private void LoadSettings()
        {
            ClipItems = new ObservableCollection<ClipItem>();

            if (File.Exists(saveFileName))
            {
                try
                {
                    using (var sr = new StreamReader(saveFileName))
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(List<ClipItem>));
                        var list = xml.Deserialize(sr) as List<ClipItem>;

                        ClipItems = new ObservableCollection<ClipItem>(list);
                    }
                }
                catch { }
            }
        }

        public bool Save()
        {
            try
            {
                using (var sw = new StreamWriter(saveFileName))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(List<ClipItem>));
                    xml.Serialize(sw, ClipItems.ToList());
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
