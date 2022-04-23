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
    public class TestRunBackupCommand2_FailScenarios
    {
        [TestMethod]
        public void Backup_01_1_Running_for_the_first_time_Fail_on_all_files_copy()
        {
            // This test simulates running the tool for the first time and failing to copy all files.
            // The next time a backup is being run should resolve all the issues and bring the system to a
            // valid state (as if the failure never happened)

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n")
            };
            var fs = new FileSystemEmulator(files);

            // force excpetion during copy
            fs.OnBeforeCopy = (sourceName, destName) =>            
                throw new Exception($"Failed copying {sourceName} to {destName}");
        
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd.Execute();
            cmd.Failures.Count.ShouldBe(3);

            // Allow to copy without errors
            fs.OnBeforeCopy = (sourceName, destName) => { };
            cmd.Execute();

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", content: "1"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n") };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected,actual, "");
        }

        [TestMethod]
        public void Backup_01_2_Running_for_the_first_time_Fail_on_a_single_file()
        {
            // This test simulates running the tool for the first time and failing to copy a specific file.
            // The next time a backup is being run should resolve all the issues and bring the system to a
            // valid state - a new version is added and the state is correct

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n")
            };
            var fs = new FileSystemEmulator(files);

            // force excpetion during copy
            fs.OnBeforeCopy = (sourceName, destName) =>
            {
                if (sourceName == @"c:\source\file2.txt")
                    throw new Exception($"Failed copying {sourceName} to {destName}");
            };

            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd.Execute();
            cmd.Failures.Count.ShouldBe(1);

            // Remove exceptions during copy and run again
            fs.OnBeforeCopy = (sourceName, destName) => { };
            cmd.Execute();

            // Expected that all files that were copied correctly in the first time will show up under version 1,
            // while the file that failed to copy will show up under version 2
            var expected = new[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", content: "1"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nsubdir\\file11.txt\r\n"),
                new EmulatorFile(@"d:\target\guid1\History\2\new.txt", content: "file2.txt\r\n"),
            };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void Backup_01_3_Running_for_the_first_time_Fail_writing_list_of_new_files()
        {
            // This test simulates running the tool for the first time and failing to write the list of new files.
            // The next time a backup is being run should resolve all the issues and bring the system to a
            // valid state (as if the failure never happened)

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n")
            };
            var fs = new FileSystemEmulator(files);

            // force excpetion during copy
            fs.OnBeforeAppendOrWriteLines = (file, lines) =>
                throw new Exception($"Failed appending to {file}");

            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);
            try
            {
                cmd.Execute();
                Assert.Fail("Should have thrown and exception");
            }
            catch { }

            // Remove exceptions during copy and run again
            fs.OnBeforeAppendOrWriteLines = (file, lines) => { };
            cmd.Execute();

            // Expected that all files will show up under version 1
            var expected = new[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", content: "1"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
            };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void Backup_02_1_Running_for_the_second_time_Only_deleted_files_Failing_on_all_files_copy()
        {
            // This test simulates running the tool for the second time and failing to copy all deleted files.
            // The next time a backup is being run should resolve all the issues and bring the system to a
            // valid state (as if the failure never happened)

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
            };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.DeleteFile(@"c:\source\file1.txt");
            fs.DeleteFile(@"c:\source\subdir\file11.txt");

            // force excpetion during copy
            fs.OnBeforeCopy = (sourceName, destName) =>
                throw new Exception($"Failed copying {sourceName} to {destName}");

            // Second run
            cmd.Execute();
            cmd.Failures.Count.ShouldBe(2);

            // Allow to copy without errors
            fs.OnBeforeCopy = (sourceName, destName) => { };
            cmd.Execute();

            // Expected to see 2 versions
            var expected = new[] {
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
                new EmulatorFile(@"d:\target\guid1\History\2\deleted\file1.txt", content: "1"),
                new EmulatorFile(@"d:\target\guid1\History\2\deleted\subdir\file11.txt", content: "11"),
            };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void Backup_02_2_Running_for_the_second_time_Only_deleted_files_Failing_on_all_files_delete()
        {
            // This test simulates running the tool for the second time and failing to delete all deleted files.
            // The next time a backup is being run should resolve all the issues and bring the system to a
            // valid state (as if the failure never happened)

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
            };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.DeleteFile(@"c:\source\file1.txt");
            fs.DeleteFile(@"c:\source\subdir\file11.txt");

            // force excpetion during delete
            fs.OnBeforeDelete = filename =>
            {
                if (filename.Contains("file1.txt") || filename.Contains("file11.txt"))
                    throw new Exception($"Failed to delete file {filename}");
            };
                

            // Second run
            cmd.Execute();
            cmd.Failures.Count.ShouldBe(2);

            // Allow to run without errors
            fs.OnBeforeDelete = filename => { };
            cmd.Execute();

            // Expected to see 2 versions
            var expected = new[] {
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
                new EmulatorFile(@"d:\target\guid1\History\2\deleted\file1.txt", content: "1"),
                new EmulatorFile(@"d:\target\guid1\History\2\deleted\subdir\file11.txt", content: "11"),
            };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void Backup_02_3_Running_for_the_second_time_Only_deleted_files_Failing_on_mix_phases()
        {
            // This test simulates running the tool for the second time and failing to delete/copy files.
            // The next time a backup is being run should resolve all the issues and bring the system to a
            // valid state (as if the failure never happened)

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\file3.txt", content: "3"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
            };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.DeleteFile(@"c:\source\file1.txt");
            fs.DeleteFile(@"c:\source\file3.txt");
            fs.DeleteFile(@"c:\source\subdir\file11.txt");

            // force excpetion during copy/delete
            fs.OnBeforeCopy = (sourceName, destName) =>
            {
                if (sourceName.Contains("file1.txt"))
                    throw new Exception($"Failed copying {sourceName} to {destName}");
            };
            fs.OnBeforeDelete = filename =>
            {
                if (filename.Contains("file3.txt"))
                    throw new Exception($"Failed to delete file {filename}");
            };


            // Second run
            cmd.Execute();
            cmd.Failures.Count.ShouldBe(2);

            // Allow to run without errors
            fs.OnBeforeCopy = (sourceName, destName) => { };
            fs.OnBeforeDelete = filename => { };
            cmd.Execute();

            // Expected to see 3 versions
            var expected = new[] {
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", content: "2"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nfile3.txt\r\nsubdir\\file11.txt\r\n"),
                new EmulatorFile(@"d:\target\guid1\History\2\deleted\file3.txt", content: "3"),
                new EmulatorFile(@"d:\target\guid1\History\2\deleted\subdir\file11.txt", content: "11"),
                new EmulatorFile(@"d:\target\guid1\History\3\deleted\file1.txt", content: "1"),                
            };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void Backup_03_1_Running_for_the_second_time_Only_modified_files_Failing_on_mix_phases ()
        {
            // This test simulates running the tool for the second time after some files were modified
            // while having errors to copy away the old version.

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
            };
            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            // First run
            cmd.Execute();

            fs.UpdateFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "1_");
            fs.UpdateFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "2_");

            // Second run
            // force excpetion during copy/delete
            fs.OnBeforeCopy = (sourceName, destName) =>
            {
                if (sourceName.Contains("file1.txt") && destName.Contains("History"))
                    throw new Exception($"Failed copying {sourceName} to {destName}");
                if (sourceName.Contains("file2.txt") && destName.Contains("CurrentState"))
                    throw new Exception($"Failed copying {sourceName} to {destName}");
            };
            cmd.Execute();
            cmd.Failures.Count.ShouldBe(2);

            // Allow to run without errors
            fs.OnBeforeCopy = (sourceName, destName) => { };
            cmd.Execute();

            // Expected to see 2 versions
            var expected = new[] {
                new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "1_"),
                new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "2_"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
                new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),

                new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", new DateTime(2015, 1, 1), "1_"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", new DateTime(2015, 1, 1), "2_"),
                new EmulatorFile(@"d:\target\guid1\CurrentState\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
                new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
                new EmulatorFile(@"d:\target\guid1\History\2\modified\file1.txt", new DateTime(2010, 1, 1), "1"),
                new EmulatorFile(@"d:\target\guid1\History\2\modified\file2.txt", new DateTime(2010, 1, 1), "2"),
            };
            var actual = fs.ListAllFiles();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        //[TestMethod]
        //public void Backup_03_5_Running_for_the_second_time_Modified_and_new_files()
        //{
        //    // This test simulates running the tool for the second time after some files were modified and some were added.
        //    // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
        //    // In the second run, file1.txt and file2.txt are modified and file3.txt, subdir3/file44.txt were added

        //    var source = @"c:\source";
        //    var target = @"d:\target";

        //    var files = new EmulatorFile[] {
        //        new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
        //        new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
        //        new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
        //    };
        //    var fs = new FileSystemEmulator(files);
        //    var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

        //    // First run
        //    cmd.Execute();

        //    fs.UpdateFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "1_");
        //    fs.UpdateFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "2_");
        //    fs.AddFiles(new[] {
        //        new EmulatorFile(@"c:\source\file3.txt", content: "new file 3"),
        //        new EmulatorFile(@"c:\source\subdir3\file4.txt", content: "new file 4") });

        //    // Second run
        //    cmd.Execute();

        //    // Expected to see 2 versions
        //    var expected = new[] {
        //        new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "1_"),
        //        new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "2_"),
        //        new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
        //        new EmulatorFile(@"c:\source\file3.txt", content: "new file 3"),
        //        new EmulatorFile(@"c:\source\subdir3\file4.txt", content: "new file 4"),
        //        new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),

        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", new DateTime(2015, 1, 1), "1_"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", new DateTime(2015, 1, 1), "2_"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file3.txt", content: "new file 3"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\subdir3\file4.txt", content: "new file 4"),

        //        new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\new.txt", content: "file3.txt\r\nsubdir3\\file4.txt\r\n"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\modified\file1.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\modified\file2.txt", new DateTime(2010, 1, 1), "2"),
        //    };
        //    var actual = fs.ListAllFiles();
        //    TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        //}

        //[TestMethod]
        //public void Backup_03_6_Running_for_the_second_time_Modified_new_and_deleted_files()
        //{
        //    // This test simulates running the tool for the second time after some files were modified, some were added
        //    // and some were deleted.
        //    // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
        //    // In the second run, file1.txt and file2.txt are modified, file3.txt and subdir3/file4.txt were added
        //    // and subdir\file11.txt was deleted.

        //    var source = @"c:\source";
        //    var target = @"d:\target";

        //    var files = new EmulatorFile[] {
        //        new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
        //        new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
        //        new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
        //    };
        //    var fs = new FileSystemEmulator(files);
        //    var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

        //    // First run
        //    cmd.Execute();

        //    fs.UpdateFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "1_");
        //    fs.UpdateFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "2_");
        //    fs.AddFiles(new[] {
        //        new EmulatorFile(@"c:\source\file3.txt", content: "new file 3"),
        //        new EmulatorFile(@"c:\source\subdir3\file4.txt", content: "new file 4") });
        //    fs.DeleteFile(@"c:\source\subdir\file11.txt");

        //    // Second run
        //    cmd.Execute();

        //    // Expected to see 2 versions
        //    var expected = new[] {
        //        new EmulatorFile(@"c:\source\file1.txt", new DateTime(2015, 1, 1), "1_"),
        //        new EmulatorFile(@"c:\source\file2.txt", new DateTime(2015, 1, 1), "2_"),
        //        new EmulatorFile(@"c:\source\file3.txt", content: "new file 3"),
        //        new EmulatorFile(@"c:\source\subdir3\file4.txt", content: "new file 4"),
        //        new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),

        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file1.txt", new DateTime(2015, 1, 1), "1_"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", new DateTime(2015, 1, 1), "2_"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file3.txt", content: "new file 3"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\subdir3\file4.txt", content: "new file 4"),

        //        new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\new.txt", content: "file3.txt\r\nsubdir3\\file4.txt\r\n"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\modified\file1.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\modified\file2.txt", new DateTime(2010, 1, 1), "2"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\deleted\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
        //    };
        //    var actual = fs.ListAllFiles();
        //    TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        //}

        //[TestMethod]
        //public void Backup_03_7_Running_for_the_second_time_Only_renamed_files()
        //{
        //    // This test simulates running the tool for the second time after some files were renamed
        //    // In the first run there were 3 files in the source: file1.txt, file2.txt, subdir/file11.txt
        //    // In the second run, file1.txt and subdir are renamed

        //    var source = @"c:\source";
        //    var target = @"d:\target";

        //    var files = new EmulatorFile[] {
        //        new EmulatorFile(@"c:\source\file1.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
        //        new EmulatorFile(@"c:\source\subdir\file11.txt", new DateTime(2010, 1, 1), "11"),
        //        new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),
        //    };
        //    var fs = new FileSystemEmulator(files);
        //    var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

        //    // First run
        //    cmd.Execute();

        //    fs.RenameFile(@"c:\source\file1.txt", @"c:\source\file1_renamed.txt");
        //    fs.RenameDirectory(@"c:\source\subdir", @"c:\source\subdir_renamed");

        //    // Second run
        //    cmd.Execute();

        //    // Expected to see 2 versions
        //    var expected = new[] {
        //        new EmulatorFile(@"c:\source\file1_renamed.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"c:\source\file2.txt", new DateTime(2010, 1, 1), "2"),
        //        new EmulatorFile(@"c:\source\subdir_renamed\file11.txt", new DateTime(2010, 1, 1), "11"),
        //        new EmulatorFile(@"d:\target\guid1\backy.ini", content: "c:\\source\r\nguid1\r\n1\r\n"),

        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file1_renamed.txt", new DateTime(2010, 1, 1), "1"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\file2.txt", new DateTime(2010, 1, 1), "2"),
        //        new EmulatorFile(@"d:\target\guid1\CurrentState\subdir_renamed\file11.txt", new DateTime(2010, 1, 1), "11"),

        //        new EmulatorFile(@"d:\target\guid1\History\1\new.txt", content: "file1.txt\r\nfile2.txt\r\nsubdir\\file11.txt\r\n"),
        //        new EmulatorFile(@"d:\target\guid1\History\2\renamed.txt", content: "{\"oldName\":\"file1.txt\",\"newName\":\"file1_renamed.txt\"}\r\n{\"oldName\":\"subdir\\\\file11.txt\",\"newName\":\"subdir_renamed\\\\file11.txt\"}\r\n"),
        //    };
        //    var actual = fs.ListAllFiles();
        //    TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        //}
    }
}
