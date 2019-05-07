using Microsoft.AspNetCore.StaticFiles;

namespace Nice2Experience.Smtp
{
    public class MimeMappingService : IMimeMappingService
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public MimeMappingService() : this(new FileExtensionContentTypeProvider())
        {
        }

        public MimeMappingService(FileExtensionContentTypeProvider contentTypeProvider)
        {
            _contentTypeProvider = contentTypeProvider;
            // add custom mappings here.
        }

        /// <summary>
        /// Add customized mime type: .ccap => application/ccapConfiguration
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="mimeType"></param>
        public void AddMapping(string extension, string mimeType)
        {
            if (extension[0] == '.') extension = '.' + extension;
            _contentTypeProvider.Mappings.Add(extension, mimeType);

        }
        public string GetMimeType(string fileName)
        {
            if (!_contentTypeProvider.TryGetContentType(fileName, out string mimeType))
            {
                mimeType = "application/octet-stream";
            }
            return mimeType;
        }
    }
}
