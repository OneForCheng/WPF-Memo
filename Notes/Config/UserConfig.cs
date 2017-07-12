using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Notes.Config
{
    [Serializable]
    public class UserConfig
    {
        public UserConfig()
        {
            WindowState = new WindowState();
            QuerySetting = new QuerySetting();
        }

        [XmlIgnore]
        public string DataFilePath { get; private set; }

        public WindowState WindowState { get; set; }

        public QuerySetting QuerySetting { get; set; }

        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                if (path == null) path = "Config/UserConfig.xml";
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if (!File.Exists(path)) return;
            }
            DataFilePath = path;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(UserConfig));
                    var config = (UserConfig)xmldes.Deserialize(fs);
                    WindowState = config.WindowState;
                    QuerySetting = config.QuerySetting;
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
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Config/UserConfig.xml");
            }

            var xmlsz = new XmlSerializer(typeof(UserConfig));

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
