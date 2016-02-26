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
        public void Test01_Running_for_the_first_time()
        {
            var source = @"c:\source";
            var target = @"d:\target";

            var files = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };

            var fileSystem = new FileSystemEmulator(files.Select(x => Path.Combine(source, x)));
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all files from <source> will be under <target>\1\new
            var expected = files.Select(x => Path.Combine(target, "1\\new", x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test02_1_Running_twice_without_changing_source()
        {
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, nothing was changed.
            // We expect that after running backup the second time, the the backup directory will 
            // contain all previous files under directory %root%/1/new
            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };
            var destFiles = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" };
            var files = sourceFiles.Select(x => Path.Combine(source, x)).Union(destFiles.Select(x => Path.Combine(target, x)));

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all old files from <source> will be under <target>\1\new
            var expected = new string[] {
                @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" } // new files under 2/new
            .Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test02_2_Running_twice_by_executing_command_twice()
        {
            var source = @"c:\source";
            var target = @"d:\target";

            var files = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };

            var fileSystem = new FileSystemEmulator(files.Select(x => Path.Combine(source, x)));
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();
            cmd.Execute();

            // Expected that all old files from <source> will be under <target>\1\new
            var expected = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" }
            .Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test03_Running_for_the_Second_Time_Only_New_Files()
        {
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, there were 2 new files: file3.txt, subdir/file22.txt
            // We expect that after running backup the second time, the the backup directory will 
            // contain all previous files under directory %root%/1/new and all new files under %root%/2/new
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
                @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt", // old files under 1/new
                @"2\new\file3.txt", @"2\new\subdir\file22.txt" } // new files under 2/new
            .Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test03_Running_for_the_Second_Time_Only_Deleted_Files()
        {
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, 2 file were deleted: file1.txt, subdir/file11.txt 
            // We expect that after running backup the second time, the the backup directory will 
            // contain all previous files under directory %root%/1/new and all new files under %root%/2/new
            // We also expect a 'deleted' file with the names of the files that were deleted.
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

            // Expected to see %target%\2\deleted.txt file containing the names of deleted files.
            Assert.IsTrue(fileSystem.GetAllFiles(target).Any(x => x == Path.Combine(target, "2", "deleted.txt")));
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
