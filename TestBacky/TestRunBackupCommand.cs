using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.AccessControl;

namespace TestBacky
{
    [TestClass]
    public class TestRunBackupCommand
    {
        [TestMethod]
        public void Backup_01_Running_for_the_first_time()
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
            var actual = fileSystem.EnumerateFiles(target);
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Backup_02_1_Running_for_the_second_time_No_change_in_source()
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
            var actual = fileSystem.EnumerateFiles(target);
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Backup_02_2_Running_twice_by_executing_command_twice()
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
            var actual = fileSystem.EnumerateFiles(target);
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Backup_03_1_Running_for_the_second_time_Only_new_files()
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
            var actual = fileSystem.EnumerateFiles(target);
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Backup_03_2_Running_for_the_second_time_Only_deleted_files()
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
            var actual = fileSystem.EnumerateFiles(target);
            TestsUtils.AssertLists<string>(expected, actual);

            // Expected to see "file1.txt" and "subdir\file11.txt" in the deleted file
            TestsUtils.AssertLists<string>(new[] { "file1.txt", "subdir\\file11.txt" }, fileSystem.ReadLines(Path.Combine(target, "2", "deleted.txt")));
        }

        [TestMethod]
        public void Backup_03_3_Running_for_the_second_time_Both_new_and_deleted_files()
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

            var fileSystem = new FileSystemEmulator(files.Select(x => new EmulatorFile(x) { Lines = new List<string> { x } }));
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
            var actual = fileSystem.EnumerateFiles(target);
            TestsUtils.AssertLists<string>(expected, actual);

            // Expected to see "file1.txt" and "subdir\file11.txt" in the deleted file
            TestsUtils.AssertLists<string>(new[] { "file1.txt", "subdir\\file11.txt" }, fileSystem.ReadLines(Path.Combine(target, "2", "deleted.txt")));
        }

        [TestMethod]
        public void Backup_03_4_Running_for_the_second_time_Only_modified_files()
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
            var actual = fileSystem.EnumerateFiles(@"d:\target");
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Backup_03_5_Running_for_the_second_time_Modified_and_new_files()
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
            var actual = fileSystem.EnumerateFiles(@"d:\target");
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Backup_03_6_Running_for_the_second_time_Modified_new_and_deleted_files()
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
                new EmulatorFile(@"c:\source\file3.txt", new DateTime(2010, 1, 1)) { Lines = new List<string> { "123" } }, // new file
                new EmulatorFile(@"c:\source\subdir2\file44.txt", new DateTime(2010, 1, 1)) { Lines = new List<string> { "123" } }, // new file

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
            var actual = fileSystem.EnumerateFiles(@"d:\target");
            TestsUtils.AssertLists<string>(expected, actual);

            // Expected to see deleted file
            TestsUtils.AssertLists<string>(new[] { "subdir\\file11.txt" }, fileSystem.ReadLines(Path.Combine(@"d:\target", "2", "deleted.txt")));
        }

        [TestMethod]
        public void Backup_03_7_Running_for_the_second_time_Only_renamed_files()
        {
            // This test simulates running the tool for the second time after some files were renamed
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and subdir are renamed
            // We expect that all original files will be found under %target%\1\new
            // all renames should show up in the 'renamed.txt' file under %target%\2\

            var file1 = new EmulatorFile(@"d:\target\1\new\file1.txt", new DateTime(2015, 1, 1)) { Lines = new List<string> { "file1" } };
            var file1Renamed = new EmulatorFile(@"c:\source\file1_renamed.txt", new DateTime(2015, 1, 1)) { Lines = new List<string> { "file1" } };
            
            var file2 = new EmulatorFile(@"d:\target\1\new\file2.txt", new DateTime(2015, 1, 1)) { Lines = new List<string> { "file2" } };
            var file2NotRenamed = new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1)) { Lines = new List<string> { "file2" } };

            var file11 = new EmulatorFile(@"d:\target\1\new\subdir\file11.txt", new DateTime(2015, 1, 1)) { Lines = new List<string> { "file11" } };
            var file11Renamed = new EmulatorFile(@"c:\source\subdir_renamed\file11.txt", new DateTime(2015, 1, 1)) { Lines = new List<string> { "file11" } };

            var files = new EmulatorFile[] {
                // Source
                file1Renamed, file2NotRenamed, file11Renamed,

                // Target
                file1, file2, file11
            };

            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, @"c:\source", @"d:\target");
            cmd.Execute();

            // Expected that all old files from %source% will remain under %target%\1\new
            // And renamed files will be marked under %target%\2\renamed.txt
            var expected = new string[] {
                @"d:\target\1\new\file1.txt",
                @"d:\target\1\new\file2.txt",
                @"d:\target\1\new\subdir\file11.txt",

                @"d:\target\2\renamed.txt",
            };
            var actual = fileSystem.EnumerateFiles(@"d:\target");
            TestsUtils.AssertLists<string>(expected, actual);

            // Expected to see renamed file
            var renamedFiles = fileSystem.ReadLines(Path.Combine(@"d:\target", "2", "renamed.txt"))
                .Select(JObject.Parse)
                .Select(x => new
                {
                    oldName = x.Value<string>("oldName"),
                    newName = x.Value<string>("newName")
                }).ToArray();

            Assert.AreEqual(2, renamedFiles.Length);
            Assert.AreEqual("file1.txt", renamedFiles[0].oldName);
            Assert.AreEqual("file1_renamed.txt", renamedFiles[0].newName);
            Assert.AreEqual("subdir\\file11.txt", renamedFiles[1].oldName);
            Assert.AreEqual("subdir_renamed\\file11.txt", renamedFiles[1].newName);
        }

        [TestMethod]
        public void Backup_03_7_Running_twice_after_modifiying_files()
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
            var actual = fileSystem.EnumerateFiles(@"d:\target");
            TestsUtils.AssertLists<string>(expected, actual);
        }
    }
}
