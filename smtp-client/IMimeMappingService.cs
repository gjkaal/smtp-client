namespace Nice2Experience.Smtp
{
    public interface IMimeMappingService
    {
        string GetMimeType(string fileName);
        void AddMapping(string extension, string mimeType);
    }
}
