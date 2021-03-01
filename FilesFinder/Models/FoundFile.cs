using System.IO;

namespace FilesFinder.Models
{
    public class FoundFile
    {
        //public string FileName { get; set; }
        public string Path { get; set; }
        public int CountMatches { get; set; }
        public string Error { get; set; }

        public FoundFile(string pathFile, int countMatches, string error = "")
        {
            Path = pathFile;
            CountMatches = countMatches;
            Error = error;
        }
    }
}