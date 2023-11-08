namespace Backend_API.Services
{
    public class EmailTemplateService
    {
        public string LoadTemplate(string templateFilePath)
        {
            // Load the HTML template content from the file
            return File.ReadAllText(templateFilePath);
        }

        public string PopulateTemplate(string templateContent, Dictionary<string, string> data)
        {
            // Replace placeholders with actual data
            foreach (var key in data.Keys)
            {
                templateContent = templateContent.Replace($"{{{key}}}", data[key]);
            }
            return templateContent;
        }
    }
}
