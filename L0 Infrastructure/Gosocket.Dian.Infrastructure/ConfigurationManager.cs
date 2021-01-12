using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Azure;

namespace Gosocket.Dian.Infrastructure
{
    public static class ConfigurationManager
    {
        public static string GetValue(string key)
        {
            string value;
            try
            {
                value = CloudConfigurationManager.GetSetting(key);
            }
            catch(Exception ex)
            {
                _ = ex.Message;
                value = System.Configuration.ConfigurationManager.AppSettings[key];
            }

            return value;
        }

        public static Dictionary<string, string> Settings
        {
            get
            {
                if (_settings != null && _settings.Count != 0)
                    return _settings;

                _settings = new Dictionary<string, string>();

                var fileManager = new FileManager();

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(fileManager.GetText("configurations", "Ose.config"));

                var nodes = xmlDoc.SelectNodes("Settings/Setting");
                if (nodes == null)
                    return _settings;

                foreach (XmlNode node in nodes)
                    if (node.Attributes != null && !_settings.ContainsKey(node.Attributes["key"].Value))
                        _settings.Add(node.Attributes["key"].Value, node.Attributes["value"].Value);

                return _settings;
            }
        }
        private static Dictionary<string, string> _settings;

        public static Dictionary<string, string> GetCustomSettings(string configFileName)
        {
            var settings = new Dictionary<string, string>();
            try
            {
                var fileManager = new FileManager();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(fileManager.GetText("configurations", configFileName));

                var nodes = xmlDoc.SelectNodes("Settings/Setting");
                if (nodes == null)
                    return settings;

                foreach (XmlNode node in nodes)
                    if (node.Attributes != null && !settings.ContainsKey(node.Attributes["key"].Value))
                        settings.Add(node.Attributes["key"].Value, node.Attributes["value"].Value);
            }
            catch (Exception)
            {
                return settings;
            }

            return settings;
        }
    }
}
