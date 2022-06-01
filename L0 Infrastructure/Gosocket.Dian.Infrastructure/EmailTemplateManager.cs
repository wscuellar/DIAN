﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public static class EmailTemplateManager
    {
        private static FileManager fileManager = new FileManager("templates-emails");
        public static string GenerateHtmlBody(string templateName, Dictionary<string, string> replacements,string templateBase = "template.html")
        {
            try
            {
                var emailId = Guid.NewGuid();                
                var templateBaseContent = fileManager.GetText(templateBase);
                var templateContent = fileManager.GetText(templateName + ".html");

                var body = templateContent;

                foreach (var token in replacements)
                {
                    body = body.Replace(token.Key, token.Value);
                }

                body = templateBaseContent.Replace("##CONTENT##", body);
                body = body.Replace("##EMAIL_ID##", emailId.ToString());

                return body;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
