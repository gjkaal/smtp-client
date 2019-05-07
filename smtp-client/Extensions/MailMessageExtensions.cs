using System.Net.Mail;
using System.IO;

namespace Nice2Experience.Smtp.Extensions
{
    public static class MailMessageExtensions
    {
        public static void AttachFiles(this MailMessage mailMessage, IFileAttachment[] files, IMimeMappingService mimeMapping)
        {
            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    var mimeType = mimeMapping.GetMimeType(file.FileName);

                    var len = (int)file.ContentStream.Length;
                    var byteReader = new BinaryReader(file.ContentStream);
                    var allData = byteReader.ReadBytes(len);
                    var ms = new MemoryStream(allData);
                    var attachment = new Attachment(ms, file.FileName, mimeType);
                    mailMessage.Attachments.Add(attachment);
                }
            }
        }
    }
}
