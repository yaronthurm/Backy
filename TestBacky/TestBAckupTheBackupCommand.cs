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
    public class TestBackupTheBackupCommand
    {
        [TestMethod]
        public void BackupTheBackup_01_Running_on_a_single_source_for_the_first_time()
        {
            // This test simulates backing up the backup when there is only one source the copy

            var backupSource = @"c:\source";
            var backupTarget = @"d:\target";
            var backupOfTheBackupTarget = @"e:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1"),
                new EmulatorFile(@"c:\source\file2"),
                new EmulatorFile(@"c:\source\file3"),
                new EmulatorFile(@"c:\source\dir1\file1"),
                new EmulatorFile(@"c:\source\dir1\file2"),
                new EmulatorFile(@"c:\source\dir1\file3"),
                new EmulatorFile(@"c:\source\dir1\dir1\file1"),
                new EmulatorFile(@"c:\source\dir1\dir1\file2"),
                new EmulatorFile(@"c:\source\dir1\dir2\file1"),
                new EmulatorFile(@"c:\source\dir2\file1"),
                new EmulatorFile(@"c:\source\dir2\file2") };

            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, backupSource, backupTarget);
            cmd.Execute();

            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\dir3\file1"),
                new EmulatorFile(@"c:\source\dir3\file2"),
                new EmulatorFile(@"c:\source\dir3\file3") });

            cmd.Execute();


            var btbCommand = new BackupTheBackupCommand(fs, backupTarget, backupOfTheBackupTarget);
            btbCommand.Execute();

            var expected = fs.EnumerateFiles(backupTarget).Select(x => EmulatorFile.FromlFileName(x, x.Replace(backupTarget + "\\", ""), fs)).ToArray();
            var actual = fs.EnumerateFiles(backupOfTheBackupTarget).Select(x => EmulatorFile.FromlFileName(x, x.Replace(backupOfTheBackupTarget + "\\", ""), fs)).ToArray();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void BackupTheBackup_02_Running_on_a_single_source_for_the_second_time()
        {
            // This test simulates backing up the backup when there is only one source the copy
            // We will run the command once, do some additional backups and run it again.

            var backupSource = @"c:\source";
            var backupTarget = @"d:\target";
            var backupOfTheBackupTarget = @"e:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1"),
                new EmulatorFile(@"c:\source\file2"),
                new EmulatorFile(@"c:\source\file3"),
                new EmulatorFile(@"c:\source\dir1\file1"),
                new EmulatorFile(@"c:\source\dir1\file2"),
                new EmulatorFile(@"c:\source\dir1\file3"),
                new EmulatorFile(@"c:\source\dir1\dir1\file1"),
                new EmulatorFile(@"c:\source\dir1\dir1\file2"),
                new EmulatorFile(@"c:\source\dir1\dir2\file1"),
                new EmulatorFile(@"c:\source\dir2\file1"),
                new EmulatorFile(@"c:\source\dir2\file2") };

            var fs = new FileSystemEmulator(files);
            var cmd = new RunBackupCommand(fs, backupSource, backupTarget);
            cmd.Execute();

            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\dir3\file1"),
                new EmulatorFile(@"c:\source\dir3\file2"),
                new EmulatorFile(@"c:\source\dir3\file3") });

            cmd.Execute();

            // First time of backing up the backup
            var btbCommand = new BackupTheBackupCommand(fs, backupTarget, backupOfTheBackupTarget);
            btbCommand.Execute();

            // Do some regular backups
            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\dir4\file1"),
                new EmulatorFile(@"c:\source\dir4\file2"),
                new EmulatorFile(@"c:\source\dir4\file3") });
            cmd.Execute();

            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\dir5\file1"),
                new EmulatorFile(@"c:\source\dir5\file2"),
                new EmulatorFile(@"c:\source\dir5\file3") });
            cmd.Execute();


            // Run the backup of the backup again
            btbCommand.Execute();


            var expected = fs.EnumerateFiles(backupTarget).Select(x => EmulatorFile.FromlFileName(x, x.Replace(backupTarget + "\\", ""), fs)).ToArray();
            var actual = fs.EnumerateFiles(backupOfTheBackupTarget).Select(x => EmulatorFile.FromlFileName(x, x.Replace(backupOfTheBackupTarget + "\\", ""), fs)).ToArray();
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }
    }
}