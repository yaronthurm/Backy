using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.AccessControl;
using Shouldly;

namespace TestBacky
{
    [TestClass]
    public class TestRunBackupCommand
    {
        [TestMethod]
        public void Backup_01_Running_for_the_first_time()
        {
            // This test simulates running the tool for the first time.
            // After running the tool, we expect to see all files from the source

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3") };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, source, target);
            cmd.Execute();

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"file1.txt", content: "1"),
                new EmulatorFile(@"file2.txt", content: "2"),
                new EmulatorFile(@"subdir\file11.txt", content: "3") };
            TestsUtils.AssertState(fs, target, expected);
        }

        [TestMethod]
        public void Backup_02_1_Running_for_the_second_time_No_change_in_source()
        {
            // This test simulates running the tool for the second time without any change in the source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, nothing was changed.
            // We expect that after running backup the second time, the backup directory will 
            // be the same as after running it for the first time

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3") };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, source, target);
            cmd.Execute(); // Running once

            cmd = new RunBackupCommand(fs, source, target);
            cmd.Execute(); // Running twice

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"file1.txt", content: "1"),
                new EmulatorFile(@"file2.txt", content: "2"),
                new EmulatorFile(@"subdir\file11.txt", content: "3") };
            TestsUtils.AssertState(fs, target, expected);
        }

        [TestMethod]
        public void Backup_02_2_Running_twice_by_executing_command_twice()
        {
            // This test simulates running the tool for the second time without any change in the source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, nothing was changed.
            // We expect that after running backup the second time, the backup directory will 
            // be the same as after running it for the first time

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt"),
                new EmulatorFile(@"c:\source\file2.txt"),
                new EmulatorFile(@"c:\source\subdir\file11.txt") };
            var fileSystem = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fileSystem, source, target);
            cmd.Execute(); // Running once
            cmd.Execute(); // Running twice

            // Expected that all files will show up under version 1
            var expected = new[] { "file1.txt", "file2.txt", "subdir\\file11.txt" };
            var stateCalculator = new StateCalculator(fileSystem, target);
            stateCalculator.MaxVersion.ShouldBe(1);
            var latestState = stateCalculator.GetLastState();
            latestState.GetFiles().Select(x => x.RelativeName).ShouldBe(expected, true);
        }

        [TestMethod]
        public void Backup_03_1_Running_for_the_second_time_Only_new_files()
        {
            // This test simulates running the tool for the second time with new files added to the source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, there were 2 new files: file3.txt, subdir/file22.txt
            // We expect that after running backup the second time, the the backup directory will 
            // contain 2 version, the latest with all files and the first one with only the files
            // from the first run

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt"),
                new EmulatorFile(@"c:\source\file2.txt"),
                new EmulatorFile(@"c:\source\subdir\file11.txt") };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, source, target);

            // First run
            cmd.Execute();

            var newFiles = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file3.txt"),
                new EmulatorFile(@"c:\source\subdir\file22.txt") };
            fs.AddFiles(newFiles);
            
            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt"),
                new EmulatorFile(@"file2.txt"),
                new EmulatorFile(@"subdir\file11.txt") };
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file1.txt"),
                new EmulatorFile(@"file2.txt"),
                new EmulatorFile(@"subdir\file11.txt"),
                new EmulatorFile(@"file3.txt"),
                new EmulatorFile(@"subdir\file22.txt")
            };

            TestsUtils.AssertState(fs, target, expectedVersion1, expectedVersion2);
        }

        [TestMethod]
        public void Backup_03_2_Running_for_the_second_time_Only_deleted_files()
        {
            // This test simulates running the tool for the second time after some files were deleted from source.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir\file11.txt
            // In the second run, 2 files were deleted: file1.txt, subdir\file11.txt
            // We expect to see 2 versions, the first with all original files, the second
            // with only the remaining files

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt"),
                new EmulatorFile(@"c:\source\file2.txt"),
                new EmulatorFile(@"c:\source\subdir\file11.txt") };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, source, target);

            // First run
            cmd.Execute();

            fs.DeleteFile(@"c:\source\file1.txt");
            fs.DeleteFile(@"c:\source\subdir\file11.txt");

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt"),
                new EmulatorFile(@"file2.txt"),
                new EmulatorFile(@"subdir\file11.txt") };
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file2.txt")};
            TestsUtils.AssertState(fs, target, expectedVersion1, expectedVersion2);
        }

        [TestMethod]
        public void Backup_03_3_Running_for_the_second_time_Both_new_and_deleted_files()
        {
            // This test simulates running the tool for the second time after some files were deleted from source and some were added.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, 2 file were deleted: file1.txt, subdir\file11.txt
            // and 3 files were added: file3.txt, file4.txt, subdir2\file111.txt
            // We expect 2 versions to represent each state

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt"),
                new EmulatorFile(@"c:\source\file2.txt"),
                new EmulatorFile(@"c:\source\subdir\file11.txt") };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, source, target);

            // First run
            cmd.Execute();

            fs.DeleteFile(@"c:\source\file1.txt");
            fs.DeleteFile(@"c:\source\subdir\file11.txt");
            fs.AddFiles(new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file3.txt"),
                new EmulatorFile(@"c:\source\file4.txt"),
                new EmulatorFile(@"c:\source\subdir2\file111.txt") });

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt"),
                new EmulatorFile(@"file2.txt"),
                new EmulatorFile(@"subdir\file11.txt") };
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file2.txt"),
                new EmulatorFile(@"file3.txt"),
                new EmulatorFile(@"file4.txt"),
                new EmulatorFile(@"subdir2\file111.txt"),
            };
            TestsUtils.AssertState(fs, target, expectedVersion1, expectedVersion2);
        }

        [TestMethod]
        public void Backup_03_4_Running_for_the_second_time_Only_modified_files()
        {
            // This test simulates running the tool for the second time after some files were modified.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1)),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1)),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1))};
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, source, target);

            // First run
            cmd.Execute();

            fs.UpdateLastModified(@"c:\source\file1.txt", new DateTime(2015, 1, 1));
            fs.UpdateLastModified(@"c:\source\file2.txt", new DateTime(2015, 1, 1));

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2010, 1, 1)),
                new EmulatorFile(@"file2.txt", new DateTime(2010, 1, 1)),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1))};
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2015, 1, 1)),
                new EmulatorFile(@"file2.txt", new DateTime(2015, 1, 1)),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1))};

            TestsUtils.AssertState(fs, target, expectedVersion1, expectedVersion2);
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
                new EmulatorFile(@"c:\source\file3.txt", new DateTime(2010, 1, 1), "123"), // new file
                new EmulatorFile(@"c:\source\subdir2\file44.txt", new DateTime(2010, 1, 1), "123"), // new file

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

            var file1 = new EmulatorFile(@"d:\target\1\new\file1.txt", new DateTime(2015, 1, 1), "file1");
            var file1Renamed = new EmulatorFile(@"c:\source\file1_renamed.txt", new DateTime(2015, 1, 1), "file1");
            
            var file2 = new EmulatorFile(@"d:\target\1\new\file2.txt", new DateTime(2015, 1, 1), "file2");
            var file2NotRenamed = new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "file2");

            var file11 = new EmulatorFile(@"d:\target\1\new\subdir\file11.txt", new DateTime(2015, 1, 1), "file11");
            var file11Renamed = new EmulatorFile(@"c:\source\subdir_renamed\file11.txt", new DateTime(2015, 1, 1), "file11");

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
