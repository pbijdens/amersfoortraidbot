using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaidBot.Utils
{
    public static class AngularIntegrationUtils
    {
        public static string ExtractStyleIncludesForCurrentLanguage(IHostingEnvironment env, PathString path)
        {
            string contents = ReadIndexFileForCurrentLanguage(env, path);

            var relevantMatches = Regex.Matches(contents, "<link\\s+href=\"([^\"]*)\"\\s+rel=\"stylesheet\"\\s*/>");
            StringBuilder result = new StringBuilder();

            foreach (Match match in relevantMatches)
            {
                var grp1 = match.Groups[1];
                result.AppendLine(match.Value);
            }

            return $"{result}";
        }

        public static string ExtractScriptIncludesForCurrentLanguage(IHostingEnvironment env, PathString path)
        {
            string contents = ReadIndexFileForCurrentLanguage(env, path);

            var relevantMatches = Regex.Matches(contents, "<script type=\"text/javascript\" src=\"([^\"]*)\"></script>");
            StringBuilder result = new StringBuilder();

            foreach (Match match in relevantMatches)
            {
                var grp1 = match.Groups[1];
                result.AppendLine(match.Value);
            }

            return $"{result}";
        }

        private static string ReadIndexFileForCurrentLanguage(IHostingEnvironment env, PathString path)
        {
            string language = path.ToString().TrimStart('/').Split('/').FirstOrDefault();
            string languageFromURL = $"{language}";
            string filePath = Path.Combine(env.WebRootPath, "dist", languageFromURL, "index.html");
            if (!File.Exists(filePath))
            {
                languageFromURL = "nl-NL";
                filePath = Path.Combine(env.WebRootPath, "dist", languageFromURL, "index.html");
            }
            string contents = File.ReadAllText(filePath);
            return contents;
        }
    }
}
