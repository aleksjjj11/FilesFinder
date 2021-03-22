using System.Linq;
using FilesFinder.ViewModels;
using NUnit.Framework;

namespace TestOfLogic
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("C:\\Users\\DNS\\YandexDisk\\WorkUnik\\TRPO\\Test", "*.*", "doc1.txt; *.c")]
        public void GetFilesTest(string currentDirectory, string findMask, string excludeMask)
        {
            var viewmodel = new MainViewModel
            {
                CurrentDirectory = currentDirectory,
                FileMask = findMask,
                ExcludeMask = excludeMask,
            };
            var res = viewmodel.GetFilesEnumerable().ToArray();
            Assert.True(true);
        }
    }
}