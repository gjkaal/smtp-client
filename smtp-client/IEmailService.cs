using System.IO;
using System.Threading.Tasks;

namespace Nice2Experience.Smtp
{

    public interface IEmailService
    {
        Task<(string message, bool success)> SendMailContents(
            string toMailAddress,
            string toName,
            string subject,
            string htmlMessageContent,
            string style,
            params IFileAttachment[] files);

        Task<(string message, bool success)> SendMailContents(
            string fromName,
            string toMailAddress,
            string toName,
            string subject,
            string htmlMessageContent,
            string style,
            params IFileAttachment[] files);

        Task<(string message, bool success)> SendMailContents(
            string fromMailAddress,
            string fromName,
            string toMailAddress,
            string toName,
            string subject,
            string htmlMessageContent,
            string style,
            params IFileAttachment[] files);
    }
}
