using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestBacky
{
    [TestClass]
    public class TestStateClass
    {
        [TestMethod]
        public void Test01_Testing_inner_tree_to_extract_files()
        {
            var tree = new FilesAndDirectoriesTree();
            tree.Add(new VirtualFileMock("file1.txt"));
            tree.Add(new VirtualFileMock("file2.txt"));
            tree.Add(new VirtualFileMock("subdir1\\file3.txt"));
            tree.Add(new VirtualFileMock("subdir1\\file4.txt"));
            tree.Add(new VirtualFileMock("subdir2\\file5.txt"));

            var files = tree.GetFirstLevelFiles("").Select(x => x.LogicalName);
            AssertLists(new[] { "file1.txt", "file2.txt" }, files);

            files = tree.GetFirstLevelFiles("subdir1").Select(x => x.LogicalName);
            AssertLists(new[] { "subdir1\\file3.txt", "subdir1\\file4.txt" }, files);

            files = tree.GetFirstLevelFiles("subdir2").Select(x => x.LogicalName);
            AssertLists(new[] { "subdir2\\file5.txt" }, files);
        }

        [TestMethod]
        public void Test02_Testing_inner_tree_to_extract_directories()
        {
            var tree = new FilesAndDirectoriesTree();
            tree.Add(new VirtualFileMock("file1.txt"));
            tree.Add(new VirtualFileMock("file2.txt"));
            tree.Add(new VirtualFileMock("subdir1\\file3.txt"));
            tree.Add(new VirtualFileMock("subdir1\\file4.txt"));
            tree.Add(new VirtualFileMock("subdir2\\subdir2\\file5.txt"));
            tree.Add(new VirtualFileMock("subdir2\\subdir4\\file6.txt"));
            tree.Add(new VirtualFileMock("subdir2\\subdir5\\file7.txt"));

            var files = tree.GetFirstLevelFiles("subdir2", "subdir2").Select(x => x.LogicalName);
            AssertLists(new[] { "subdir2\\subdir2\\file5.txt" }, files);

            var directories = tree.GetFirstLevelDirectories("subdir1");
            AssertLists(new string [0], directories);

            directories = tree.GetFirstLevelDirectories("subdir2");
            AssertLists(new[] { "subdir2", "subdir4", "subdir5" }, directories);
        }



        private static void AssertLists<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var diff = expected.Except(actual);
            Assert.IsFalse(diff.Any(), "missing items");

            diff = actual.Except(expected);
            Assert.IsFalse(diff.Any(), "extra items");
        }
    }


    public class VirtualFileMock: IVirtualFile
    {
        public string Name;

        public VirtualFileMock(string name)
        {
            this.Name = name;
        }

        public string LogicalName
        {
            get
            {
                return Name;
            }
        }

        public string PhysicalPath
        {
            get
            {
                return Name;
            }
        }

        public string[] GetPath()
        {
            var ret = Name.Split('\\');
            return ret;
        }
    }
}
