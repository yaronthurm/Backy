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
    public class TestBAckupTheBackupCommand
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
    }
}