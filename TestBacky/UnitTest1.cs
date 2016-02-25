using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;

namespace TestBacky
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test01_Running_for_the_First_Time()
        {
            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };

            var fileSystem = new FileSystemEmulator(sourceFiles.Select(x => Path.Combine(source, x)));
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all files from <source> will be under <target>\1\new
            var expected = sourceFiles.Select(x => Path.Combine(target, "1\\new", x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }


        [TestMethod]
        public void Test02_Running_for_the_Second_Time_Only_New_Files()
        {
            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt", @"file3.txt", @"subdir\file22.txt" };
            var destFiles = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" };
            var files = sourceFiles.Select(x => Path.Combine(source, x)).Union(destFiles.Select(x => Path.Combine(target, x)));

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all old files from <source> will be under <target>\1\new
            // Expected that all new files from <source> will be under <target>\2\new
            var expected = new string[] {
                @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt",
                @"2\new\file3.txt", @"2\new\subdir\file22.txt" }.Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }



        private static void AssertLists<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var diff = expected.Except(actual);
            Assert.IsFalse(diff.Any(), "missing items");

            diff = actual.Except(expected);
            Assert.IsFalse(diff.Any(), "extra items");
        }
    }
}
