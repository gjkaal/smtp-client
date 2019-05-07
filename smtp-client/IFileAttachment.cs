using System.IO;

namespace Nice2Experience.Smtp
{
    public interface IFileAttachment
    {
        Stream ContentStream { get; }
        string FileName { get; }
    }
}
