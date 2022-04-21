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
    public class TestRunBackupCommand2
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
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n")
            };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd.Execute();

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", content: "1"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\subdir\file11.txt", content: "3"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n") };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected,actual, "");
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
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd.Execute(); // Running once

            cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd.Execute(); // Running twice

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"file1.txt", content: "1"),
                new EmulatorFile(@"file2.txt", content: "2"),
                new EmulatorFile(@"subdir\file11.txt", content: "3") };
            TestsUtils.AssertState(fs, target, source, expected);
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
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3") };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd.Execute(); // Running once
            cmd.Execute(); // Running twice

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"file1.txt", content: "1"),
                new EmulatorFile(@"file2.txt", content: "2"),
                new EmulatorFile(@"subdir\file11.txt", content: "3") };
            TestsUtils.AssertState(fs, target, source, expected);
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
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

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

            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
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
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

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
            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
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
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

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
            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
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
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.UpdateFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "11");
            fs.UpdateFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "22");

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2015, 1, 1), "11"),
                new EmulatorFile(@"file2.txt", new DateTime(2015, 1, 1), "22"),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1), "3")};

            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
        }

        [TestMethod]
        public void Backup_03_5_Running_for_the_second_time_Modified_and_new_files()
        {
            // This test simulates running the tool for the second time after some files were modified and some were added.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified and file3.txt, subdir3/file44.txt were added

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.UpdateFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "11");
            fs.UpdateFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "22");
            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\file3.txt", content: "new file 3"),
                new EmulatorFile(@"c:\source\subdir3\file4.txt", content: "new file 4") });

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2015, 1, 1), "11"),
                new EmulatorFile(@"file2.txt", new DateTime(2015, 1, 1), "22"),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1), "3"),
                new EmulatorFile(@"file3.txt", content: "new file 3"),
                new EmulatorFile(@"subdir3\file4.txt", content: "new file 4")};

            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
        }

        [TestMethod]
        public void Backup_03_6_Running_for_the_second_time_Modified_new_and_deleted_files()
        {
            // This test simulates running the tool for the second time after some files were modified, some were added
            // and some were deleted.
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and file2.txt are modified, file3.txt and subdir3/file4.txt were added
            // and subdir\file11.txt was deleted.

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.UpdateFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "11");
            fs.UpdateFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "22");
            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\file3.txt", content: "new file 3"),
                new EmulatorFile(@"c:\source\subdir3\file4.txt", content: "new file 4") });
            fs.DeleteFile(@"c:\source\subdir\file11.txt");

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2015, 1, 1), "11"),
                new EmulatorFile(@"file2.txt", new DateTime(2015, 1, 1), "22"),
                new EmulatorFile(@"file3.txt", content: "new file 3"),
                new EmulatorFile(@"subdir3\file4.txt", content: "new file 4")};

            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
        }

        [TestMethod]
        public void Backup_03_7_Running_for_the_second_time_Only_renamed_files()
        {
            // This test simulates running the tool for the second time after some files were renamed
            // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
            // In the second run, file1.txt and subdir are renamed

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.RenameFile(@"c:\source\file1.txt", @"c:\source\file1_renamed.txt");
            fs.RenameDirectory(@"c:\source\subdir", @"c:\source\subdir_renamed");

            // Second run
            cmd.Execute();

            // Expected to see 2 versions
            var expectedVersion1 = new[] {
                new EmulatorFile(@"file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"subdir\file11.txt", new DateTime(2010, 1, 1), "3")};
            var expectedVersion2 = new[] {
                new EmulatorFile(@"file1_renamed.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"subdir_renamed\file11.txt", new DateTime(2010, 1, 1), "3")};

            TestsUtils.AssertState(fs, target, source, expectedVersion1, expectedVersion2);
        }
    }
}
