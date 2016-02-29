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

        [TestMethod, Ignore]
        public void Test00_1_Running_on_real_file_system()
        {
            var source = @"D:\FolderForBackyTesting\Source";
            var target = @"D:\FolderForBackyTesting\Target";

            // Clear target
            if (Directory.Exists(target)) // This method sometimes return true when the directory doesn't realy exists
                Directory.Delete(target, true);
            Directory.CreateDirectory(target);

            // Clear source
            if (Directory.Exists(source))
                Directory.Delete(source, true);
            Directory.CreateDirectory(source);


            var cmd = new RunBackupCommand(new FileSystem(), source, target);

            // Create files in source and run first time
            File.WriteAllText(Path.Combine(source, "file1.txt"), "hello1");
            File.WriteAllText(Path.Combine(source, "file2.txt"), "hello2");
            File.WriteAllText(Path.Combine(source, "file3.txt"), "hello3");
            File.WriteAllText(Path.Combine(source, "file4.doc"), "");

            // 1
            cmd.Execute();

            // Add new files
            File.WriteAllText(Path.Combine(source, "file5.txt"), "hello5");
            File.WriteAllText(Path.Combine(source, "file6.txt"), "hello6");

            // 2
            cmd.Execute();

            // Delete a few files
            File.Delete(Path.Combine(source, "file1.txt"));
            File.Delete(Path.Combine(source, "file2.txt"));

            // 3
            cmd.Execute();

            // Modify and add some files
            File.WriteAllText(Path.Combine(source, "file5.txt"), "hello5 - modified");
            File.WriteAllText(Path.Combine(source, "file6.txt"), "hello6 - modified");
            File.WriteAllText(Path.Combine(source, "file7.txt"), "hello7");

            // 4
            cmd.Execute();

            // Assertion
            var actualTargetFiles = Directory.GetFiles(target, "*", SearchOption.AllDirectories);
            var expectedTargetFiles = new[]
                { "1\\new\\file1.txt", "1\\new\\file2.txt", "1\\new\\file3.txt", "1\\new\\file4.doc",
                  "2\\new\\file5.txt", "2\\new\\file6.txt",
                  "3\\deleted.txt",
                  "4\\modified\\file5.txt", "4\\modified\\file6.txt", "4\\new\\file7.txt"
            }.Select(x => Path.Combine(target, x));
            AssertLists(expectedTargetFiles, actualTargetFiles);

            var expectedDeleted = new[] { "file1.txt", "file2.txt" };
            var actualDeleted = File.ReadAllLines(Path.Combine(target, "3", "deleted.txt"));
            AssertLists(expectedDeleted, actualDeleted);

            Directory.Delete(target, true);
            Directory.Delete(source, true);
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

            var emulatorFiles = files.Select(x => Path.Combine(source, x)).Select(x => new EmulatorFile(x));
            var fileSystem = new FileSystemEmulator(emulatorFiles);
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

            var fileSystem = new FileSystemEmulator(files.Select(x => new EmulatorFile(x)));
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

            var fileSystem = new FileSystemEmulator(files.Select(x => Path.Combine(source, x)).Select(x => new EmulatorFile(x)));
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

            var fileSystem = new FileSystemEmulator(files.Select(x => new EmulatorFile(x)));
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

            var fileSystem = new FileSystemEmulator(files.Select(x => new EmulatorFile(x)));
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

        [TestMethod]
        public void Test03_3_Running_for_the_second_time_Both_new_and_deleted_files()
        {
            // This test simulates running the tool for the second time after some files were deleted from source and some were added.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, 2 file were deleted: file1.txt, subdir\file11.txt
            // and 3 files were added: file3.txt, file4.txt, subdir2\file111.txt
            // We expect that all original files will still be found under %target%\1\new
            // That a 'deleted.txt' file with the names of the files that were deleted will be found
            // under %target%\2\deleted.txt
            // And that all new files will be under %target%\2\new

            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file2.txt", @"file3.txt", @"file4.txt", @"subdir2\file111.txt" };
            var destFiles = new string[] { @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt" };
            var files = sourceFiles.Select(x => Path.Combine(source, x)).Union(destFiles.Select(x => Path.Combine(target, x)));

            var fileSystem = new FileSystemEmulator(files.Select(x => new EmulatorFile(x)));
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute();

            // Expected that all old files from %source% will remain under %target%\1\new
            // As well as 'deleted.txt' under %target%\2
            // And all new files will be under %target%\2\new
            var expected = new string[] {
                @"1\new\file1.txt", @"1\new\file2.txt", @"1\new\subdir\file11.txt", // old files
                @"2\deleted.txt", // for holding the names of deletd files
                @"2\new\file3.txt", @"2\new\file4.txt", @"2\new\subdir2\file111.txt"
                }.Select(x => Path.Combine(target, x));
            var actual = fileSystem.GetAllFiles(target);
            AssertLists<string>(expected, actual);

            // Expected to see "file1.txt" and "subdir\file11.txt" in the deleted file
            AssertLists<string>(new[] { "file1.txt", "subdir\\file11.txt" }, fileSystem.ReadLines(Path.Combine(target, "2", "deleted.txt")));
        }

        [TestMethod]
        public void Test03_4_Running_for_the_second_time_Only_modified_files()
        {
            // This test simulates running the tool for the second time after some files were modified.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified
            // We expect that all original files will be found under %target%\1\new
            // and that the modified files will be found under %target%\2\modified

            var files = new EmulatorFile[] {
                // Source
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1)),

                // Target
                new EmulatorFile(@"d:\target\1\new\file1.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\file2.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\subdir\file11.txt", new DateTime(2010, 1, 1)), // same as source
            };

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, @"c:\source", @"d:\target");
            cmd.Execute();

            // Expected that all old files from %source% will remain under %target%\1\new
            // And modified files will be under %target%\2\modified
            var expected = new string[] {
                @"d:\target\1\new\file1.txt",
                @"d:\target\1\new\file2.txt",
                @"d:\target\1\new\subdir\file11.txt",

                @"d:\target\2\modified\file1.txt",
                @"d:\target\2\modified\file2.txt"
            };
            var actual = fileSystem.GetAllFiles(@"d:\target");
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test03_5_Running_for_the_second_time_Modified_and_new_files()
        {
            // This test simulates running the tool for the second time after some files were modified and some were added.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified and file3.txt, subdir3/file44.txt were added
            // We expect that all original files will be found under %target%\1\new
            // all modified files will be found under %target%\2\modified
            // and all new files will be found under %target%\2\new

            var files = new EmulatorFile[] {
                // Source
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1)), // no change
                new EmulatorFile(@"c:\source\file3.txt", new DateTime(2010, 1, 1)), // new file
                new EmulatorFile(@"c:\source\subdir2\file44.txt", new DateTime(2010, 1, 1)), // new file

                // Target
                new EmulatorFile(@"d:\target\1\new\file1.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\file2.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\subdir\file11.txt", new DateTime(2010, 1, 1)), // same as source
            };

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, @"c:\source", @"d:\target");
            cmd.Execute();

            // Expected that all old files from %source% will remain under %target%\1\new
            // And modified files will be under %target%\2\modified
            var expected = new string[] {
                @"d:\target\1\new\file1.txt",
                @"d:\target\1\new\file2.txt",
                @"d:\target\1\new\subdir\file11.txt",

                @"d:\target\2\modified\file1.txt",
                @"d:\target\2\modified\file2.txt",

                @"d:\target\2\new\file3.txt",
                @"d:\target\2\new\subdir2\file44.txt"
            };
            var actual = fileSystem.GetAllFiles(@"d:\target");
            AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Test03_6_Running_for_the_second_time_Modified_new_and_deleted_files()
        {
            // This test simulates running the tool for the second time after some files were modified, some were added
            // and some were deleted.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified, file3.txt and subdir3/file44.txt were added
            // and subdir\file11.txt was deleted.
            // We expect that all original files will be found under %target%\1\new
            // all modified files will be found under %target%\2\modified
            // all new files will be found under %target%\2\new
            // and a 'deleted.txt' file will be found containing the name of the deleted file

            var files = new EmulatorFile[] {
                // Source
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                // new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1)), // deleted file
                new EmulatorFile(@"c:\source\file3.txt", new DateTime(2010, 1, 1)), // new file
                new EmulatorFile(@"c:\source\subdir2\file44.txt", new DateTime(2010, 1, 1)), // new file

                // Target
                new EmulatorFile(@"d:\target\1\new\file1.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\file2.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\subdir\file11.txt", new DateTime(2010, 1, 1)), // same as source
            };

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, @"c:\source", @"d:\target");
            cmd.Execute();

            // Expected that all old files from %source% will remain under %target%\1\new
            // And modified files will be under %target%\2\modified
            var expected = new string[] {
                @"d:\target\1\new\file1.txt",
                @"d:\target\1\new\file2.txt",
                @"d:\target\1\new\subdir\file11.txt",

                @"d:\target\2\modified\file1.txt",
                @"d:\target\2\modified\file2.txt",

                @"d:\target\2\new\file3.txt",
                @"d:\target\2\new\subdir2\file44.txt",

                @"d:\target\2\deleted.txt",

            };
            var actual = fileSystem.GetAllFiles(@"d:\target");
            AssertLists<string>(expected, actual);

            // Expected to see deleted file
            AssertLists<string>(new[] { "subdir\\file11.txt" }, fileSystem.ReadLines(Path.Combine(@"d:\target", "2", "deleted.txt")));
        }

        [TestMethod]
        public void Test03_7_Running_twice_after_modifiying_files()
        {
            // This test simulates running the tool for the second time after some files were modified.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified
            // We expect that all original files will be found under %target%\1\new
            // and that the modified files will be found under %target%\2\modified

            var files = new EmulatorFile[] {
                // Source
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1)), // modified file - from 2015
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1)),

                // Target
                new EmulatorFile(@"d:\target\1\new\file1.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\file2.txt", new DateTime(2010, 1, 1)), // old file - from 2010
                new EmulatorFile(@"d:\target\1\new\subdir\file11.txt", new DateTime(2010, 1, 1)), // same as source
            };

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, @"c:\source", @"d:\target");
            cmd.Execute(); // Run once
            cmd.Execute(); // Run twice

            // Expected that all old files from %source% will remain under %target%\1\new
            // And modified files will be under %target%\2\modified
            var expected = new string[] {
                @"d:\target\1\new\file1.txt",
                @"d:\target\1\new\file2.txt",
                @"d:\target\1\new\subdir\file11.txt",

                @"d:\target\2\modified\file1.txt",
                @"d:\target\2\modified\file2.txt"
            };
            var actual = fileSystem.GetAllFiles(@"d:\target");
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
