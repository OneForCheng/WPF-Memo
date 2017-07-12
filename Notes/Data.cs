using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Notes
{

    [Serializable]
    public class Note : INotifyPropertyChanged
    {
        private int _id;
        private string _content;
        private DateTime _createTime;
        private DateTime _lastModTime;
        private bool _finished;
        private bool _deleted;
        private string _state;

        public Note()
        {
            _content = "";
            _createTime = DateTime.Now;
            _lastModTime = DateTime.Now;
            _state = "未完成";
        }

        public Note(int id, string content) 
        {
            _id = id;
            _content = content;
            _createTime = DateTime.Now;
            _lastModTime = DateTime.Now;
            _state = "未完成";
        }

        public int Id
        {
            get
            {
                return _id;
            }

            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged("Id");
                }

            }
        }

        public string Content
        {
            get
            {
                return _content;
            }

            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged("Content");
                }

            }
        }

        public DateTime CreateTime
        {
            get
            {
                return _createTime;
            }

            set
            {
                if (_createTime != value)
                {
                    _createTime = value;
                    OnPropertyChanged("CreateTime");
                }
            }
        }

        public DateTime LastModTime
        {
            get
            {
                return _lastModTime;
            }

            set
            {
                if (_lastModTime != value)
                {
                    _lastModTime = value;
                    OnPropertyChanged("LastModTime");
                }
            }
        }

        public bool Finished
        {
            get
            {
                return _finished;
            }

            set
            {
                if (_finished != value)
                {
                    _finished = value;
                    if (_deleted)
                    {
                        State = "已删除";
                    }
                    else if (_finished)
                    {
                        State = "已完成";
                    }
                    else
                    {
                        State = "未完成";
                    }
                }

            }
        }

        public bool Deleted
        {
            get
            {
                return _deleted;
            }

            set
            {
                if (_deleted != value)
                {
                    _deleted = value;
                    if (_deleted)
                    {
                        State = "已删除";
                    }
                    else if (_finished)
                    {
                        State = "已完成";
                    }
                    else
                    {
                        State = "未完成";
                    }
                }

            }
        }

        [XmlIgnore]
        public string State
        {
            get
            {
                return _state;
            }

            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }

            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


    }

    [Serializable]
    public class NotesContext
    {
        public NotesContext()
        {
            Notes = new List<Note>();
        }

        public List<Note> Notes { get; set; }

        [XmlIgnore]
        public string DataFilePath { get; private set; }

        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                if (path == null) path = "Datas.xml";
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if (!File.Exists(path)) return;
            }
            DataFilePath = path;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(NotesContext));
                    var one = (NotesContext)xmldes.Deserialize(fs);
                    Notes = one.Notes;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        
        public void SaveChanged()
        {
            var path = DataFilePath;
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Datas.xml");
            }

            var xmlsz = new XmlSerializer(typeof(NotesContext));

            var file = new FileInfo(path);

            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }
            using (var sw = new StreamWriter(file.FullName))
            {
                xmlsz.Serialize(sw, this);
            }
        }
    }
}
