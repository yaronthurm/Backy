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
    public class UnitTest1
    {
        [TestMethod, Ignore]
        public void Test00_Playing_around()
        {
            var files1 = Directory.GetFiles(@"D:\DataFromExternalDrive", "*.*", SearchOption.AllDirectories);
            var files2 = Directory.GetFiles(@"D:\DataFromExternalDrive", "*.*", SearchOption.AllDirectories);
            var files3 = Directory.GetFiles(@"D:\DataFromExternalDrive", "*.*", SearchOption.AllDirectories);
            var files4 = Directory.GetFiles(@"D:\DataFromExternalDrive", "*.*", SearchOption.AllDirectories);
        }

        [TestMethod]
        public void Test01_Running_for_the_first_time()
        {
            // This test simulate running the tool for the first time.
            // We only have files under the source directory
            // After running the tool, we expect to see all files from the source copied into
            // the target location under %target%/1/new

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };

            var fileSystem = new FileSystemEmulator(files.Select(x => Path.Combine(source, x)));
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all files from %source% will be under %target%\1\new
            var expected = files.Select(x => Path.Combine(target, "1\\new", x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test02_1_Running_for_the_second_time_No_change_in_source()
        {
            // This test simulate running the tool for the second time without any change in the source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, nothing was changed.
            // We expect that after running backup the second time, the backup directory will 
            // contain all previous files under directory %target%/1/new

            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };
            var destFiles = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" };
            var files = sourceFiles.Select(x => Path.Combine(source, x)).Union(destFiles.Select(x => Path.Combine(target, x)));

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all old files from %source% will be under %target%\1\new
            var expected = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" }.Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test02_2_Running_twice_by_executing_command_twice()
        {
            // This test simulate running the tool for the first time and right after running it again
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, nothing was changed.
            // We expect that after running backup the second time, the backup directory will 
            // contain all previous files under directory %target%/1/new
            var source = @"c:\source";
            var target = @"d:\target";

            var files = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" };

            var fileSystem = new FileSystemEmulator(files.Select(x => Path.Combine(source, x)));
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();
            cmd.Execute();

            // Expected that all old files from %source% will be under %target%\1\new
            var expected = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" }.Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test03_1_Running_for_the_second_time_Only_new_files()
        {
            // This test simulate running the tool for the second time with new files added to the source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, there were 2 new files: file3.txt, subdir/file22.txt
            // We expect that after running backup the second time, the the backup directory will 
            // contain all previous files under %target%/1/new and all new files under %target%/2/new

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
        public void Test03_2_Running_for_the_second_time_Only_deleted_files()
        {
            // This test simulates running the tool for the second time after some files were deleted from source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, 2 file were deleted: file1.txt, subdir\file11.txt
            // We expect that all original files will still be found under %target%\1\new
            // and that a 'deleted.txt' file with the names of the files that were deleted will be found
            // under %target%\2\deleted.txt

            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file2.txt" };
            var destFiles = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" };
            var files = sourceFiles.Select(x => Path.Combine(source, x)).Union(destFiles.Select(x => Path.Combine(target, x)));

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all old files from %source% will remain under %target%\1\new
            // As well as 'deleted.txt' under %target%\2
            var expected = new string[] {
                @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt", // old files
                @"2\deleted.txt" // for holding the names of deletd files
                }.Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);

            // Expected to see "file1.txt" and "subdir\file11.txt" in the deleted file
            AssertLists<string>(new[] { "file1.txt", "subdir\\file11.txt" }, fileSystem.ReadLines(Path.Combine(target, "2", "deleted.txt")));
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
