using System;
using System.IO;
using System.Linq;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MailKit.Security;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MailKit;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace MailSaver.ViewModels
{
    public class EmailSaver
    {
        //Флаг для POP3, определяющий удаление писем
        bool deleteAfterDownload = false;
        //Объект конфигурационного файла
        IConfigurationRoot сonfiguration;
        string saveDirectory;
        
        //Конструктор класса с настройкой конфигурации
        public EmailSaver() {
            сonfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var pathConfiguration = сonfiguration.GetSection("SavePath").Get<PathConfiguration>();
            saveDirectory = pathConfiguration.Path;
        }
        //Метод для выбора протокола
        public async Task<string> Save(int prot)
        {
            if (prot == 1)
                return SaveByPop3();
            else if (prot == 2)
                return SaveByImap();
            else
                return "Unexpected error";
        }
        private string SaveByImap()
        {
            var emailSettings = сonfiguration.GetSection("EmailSettingsIMAP").Get<EmailSettings>();
            // Если путь не является полным, комбинируем его с текущим рабочим каталогом
            if (!Path.IsPathRooted(saveDirectory))
            {
                saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), saveDirectory);
            }

            // Создаем директорию, если она не существует
            Directory.CreateDirectory(saveDirectory);

            using (var client = new ImapClient())
            {
                client.Connect(emailSettings.Server, emailSettings.Port, SecureSocketOptions.SslOnConnect);
                client.Authenticate(emailSettings.Email, emailSettings.Password);

                // Выбираем папку "Входящие"
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);


                var query = SearchQuery.All.And(SearchQuery.FromContains(emailSettings.SenderMask));
                if (!string.IsNullOrEmpty(emailSettings.TopicMask))
                {
                    query = query.And(SearchQuery.SubjectContains(emailSettings.TopicMask));
                }

                if (!string.IsNullOrEmpty(emailSettings.MessageMask))
                {
                    query = query.And(SearchQuery.BodyContains(emailSettings.MessageMask));
                }

                var uids = inbox.Search(query);
                string mess = "";

                foreach (var uid in uids)
                {
                    var message = inbox.GetMessage(uid);
                    string? messageFilePath;
                    if (!string.IsNullOrEmpty(emailSettings.SenderMask) && !string.IsNullOrEmpty(emailSettings.TopicMask))
                    {
                        messageFilePath = Path.Combine(saveDirectory, $"to('{emailSettings.Email}')from('{emailSettings.SenderMask}')subject('{emailSettings.TopicMask}')");
                        Directory.CreateDirectory(messageFilePath);
                        messageFilePath = Path.Combine(messageFilePath, $"{uid}.eml");
                    }
                    else if (!string.IsNullOrEmpty(emailSettings.SenderMask))
                    {
                        messageFilePath = Path.Combine(saveDirectory, $"to('{emailSettings.Email}')from('{emailSettings.SenderMask}')");
                        Directory.CreateDirectory(messageFilePath);
                        messageFilePath = Path.Combine(messageFilePath, $"{uid}.eml");
                    }
                    else
                    {
                        messageFilePath = Path.Combine(saveDirectory, $"to('{emailSettings.Email}')");
                        Directory.CreateDirectory(messageFilePath);
                        messageFilePath = Path.Combine(messageFilePath, $"{uid}.eml");
                    }

                    using (var fileStream = new FileStream(messageFilePath, FileMode.Create))
                    {
                        message.WriteTo(fileStream);
                    }

                    Console.WriteLine($"Saved: {messageFilePath}");
                    mess += $"Saved: {messageFilePath}\n";
                }
                if (mess == "")
                    mess = "Not saved";
                client.Disconnect(true);
                return mess;
            }
        }
        private string SaveByPop3()
        {

            // Загружаем конфигурацию
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var emailSettings = configuration.GetSection("EmailSettingsPOP").Get<EmailSettings>();
            Directory.CreateDirectory(saveDirectory);

            using (var client = new Pop3Client())
            {
                client.Connect(emailSettings.Server, emailSettings.Port, SecureSocketOptions.SslOnConnect);
                client.Authenticate(emailSettings.Email, emailSettings.Password);

                // Получаем количество сообщений на сервере
                var messageCount = client.Count;
                string mess = "";
                for (int i = 0; i < messageCount; i++)
                {
                    var message = client.GetMessage(i);

                    // Получаем дату письма и проверяем, попадает ли она в указанный период
                    if (string.IsNullOrEmpty(emailSettings.SenderMask) || message.From.Mailboxes.Any(m => m.Address.Contains(emailSettings.SenderMask)) &&
                    (!string.IsNullOrEmpty(emailSettings.TopicMask)
                    ? message.Subject.Contains(emailSettings.TopicMask)
                    : false
                     ) &&
                    (!string.IsNullOrEmpty(emailSettings.MessageMask) && !string.IsNullOrEmpty(message.TextBody))
                    ? message.TextBody.ToString().Contains(emailSettings.MessageMask)
                    : false
                    )
                    {
                        string? messageFilePath;
                        
                        if (!string.IsNullOrEmpty(emailSettings.SenderMask) && !string.IsNullOrEmpty(emailSettings.TopicMask)) {
                            messageFilePath = Path.Combine(saveDirectory, $"to('{emailSettings.Email}')from('{emailSettings.SenderMask}')subject('{emailSettings.TopicMask}')");
                            Directory.CreateDirectory(messageFilePath);
                            messageFilePath = Path.Combine(messageFilePath, $"{i + 1}.eml");
                        }
                        else if (!string.IsNullOrEmpty(emailSettings.SenderMask))
                        {
                            messageFilePath = Path.Combine(saveDirectory, $"to('{emailSettings.Email}')from('{emailSettings.SenderMask}')");
                            Directory.CreateDirectory(messageFilePath);
                            messageFilePath = Path.Combine(messageFilePath, $"{i + 1}.eml");
                        }
                        else
                        {
                            messageFilePath = Path.Combine(saveDirectory, $"to('{emailSettings.Email}')");
                            Directory.CreateDirectory(messageFilePath);
                            messageFilePath = Path.Combine(messageFilePath, $"{i + 1}.eml");
                        }

                        using (var fileStream = new FileStream(messageFilePath, FileMode.Create))
                        {
                            message.WriteTo(fileStream);
                        }

                        Console.WriteLine($"Saved: {messageFilePath}");
                        mess += $"Saved: {messageFilePath}\n";
                    }
                    // Удаляем сообщение с сервера, если deleteAfterDownload равно true
                    if (deleteAfterDownload)
                    {
                        client.DeleteMessage(i);
                    }
                }
                if (mess == "")
                    mess = "Not saved";
                client.Disconnect(true);
                return mess;
            }
        }
    }
}
