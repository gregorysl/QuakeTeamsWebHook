using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace QuakeTeamsWebHook
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private  static readonly NewLog GameLog = new NewLog();
        

        static void Main(string[] args)
        {
            logger.Info("Starting QuakeTeamsWebHook");
            var logPath = ConfigurationManager.AppSettings["logPath"];
            MonitorTailOfFile(logPath);
        }

        private static string GetJsonToSend(string scoreJson, string endReason, string mapName) {
            
            TextReader tr = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "VictoryTemplate.json"));
            string myText = tr.ReadToEnd();
            var withScore = myText.Replace("$FACTS",scoreJson).Replace("$ENDREASON", endReason).Replace("$MAPNAME", mapName);
            return withScore;
        }
        private static void SendNotification(string scoreJson, string endReason, string mapName)
        {
            logger.Debug("Getting Json to send");
            var values = GetJsonToSend(scoreJson,endReason, mapName);

            logger.Debug($"Obtained JSON to send {values}");
            var webHookUrl = ConfigurationManager.AppSettings["webHookUrl"];  
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(webHookUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(values);
            }


            logger.Debug("Calling MS Teams Server");
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    logger.Info($"Server response:{result}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "MS Teams Call exception");
            }
        }

        public static void MonitorTailOfFile(string filePath)
        {
            var initialFileSize = new FileInfo(filePath).Length;
            var lastReadLength = initialFileSize - 1024;
            if (lastReadLength < 0) lastReadLength = 0;

            while (true)
            {
                try
                {
                    var fileSize = new FileInfo(filePath).Length;
                    if (fileSize > lastReadLength)
                    {
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            fs.Seek(lastReadLength, SeekOrigin.Begin);
                            var buffer = new byte[1024];

                            while (true)
                            {
                                var bytesRead = fs.Read(buffer, 0, buffer.Length);
                                lastReadLength += bytesRead;

                                if (bytesRead == 0)
                                    break;

                                var text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                var textsplit = text.Split(
                                    new[] { "\r\n", "\r", "\n" },
                                    StringSplitOptions.None);
                                foreach (var line in textsplit)
                                {
                                    GameLog.ProcessTheRows(line);
                                }

                                if (GameLog.Game.Finished)
                                {
                                    logger.Info("Game finished");
                                    var scoreJson = GameLog.Game.ScorecardJson();
                                    var endReason = GameLog.Game.EndReason;
                                    var mapName = GameLog.Game.MapName;
                                    if (!string.IsNullOrEmpty(scoreJson))
                                    {
                                        logger.Info("Running notification task");
                                        Task.Run(() => SendNotification(scoreJson, endReason, mapName));
                                    }
                                    else
                                    {
                                        logger.Info("Omitting MSTEAMS notification as no players were playing");
                                    }
                                    GameLog.Game.Reset();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
