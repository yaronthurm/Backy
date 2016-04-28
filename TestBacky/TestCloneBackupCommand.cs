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
    public class TestCloneBackupCommand
    {
        [TestMethod]
        public void Clone_01_Clone_last_backup_that_only_has_new_files()
        {
            // This test simulates cloning a backup that only has new files.
            // After running the tool, we expect to see all files from the backup copied into
            // the target location with the correct rlative names

            var backupSource = @"c:\source";
            var backupTarget = @"d:\target";
            var cloneSource = new CloneSource { OriginalSourcePath = backupSource, BackupPath = backupTarget };
            var cloneTarget = @"d:\cloneTarget";

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
            var cmd = new RunBackupCommand(fs, backupSource, backupTarget, MachineID.One);
            cmd.Execute();

            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\dir3\file1"),
                new EmulatorFile(@"c:\source\dir3\file2"),
                new EmulatorFile(@"c:\source\dir3\file3") });

            cmd.Execute();


            var cloneCmd = new CloneBackupCommand(fs, cloneSource, cloneTarget, 2);
            cloneCmd.Execute();

            var expected = new[]
            {
                new EmulatorFile(@"file1"),
                new EmulatorFile(@"file2"),
                new EmulatorFile(@"file3"),
                new EmulatorFile(@"dir1\file1"),
                new EmulatorFile(@"dir1\file2"),
                new EmulatorFile(@"dir1\file3"),
                new EmulatorFile(@"dir1\dir1\file1"),
                new EmulatorFile(@"dir1\dir1\file2"),
                new EmulatorFile(@"dir1\dir2\file1"),
                new EmulatorFile(@"dir2\file1"),
                new EmulatorFile(@"dir2\file2"),
                new EmulatorFile(@"dir3\file1"),
                new EmulatorFile(@"dir3\file2"),
                new EmulatorFile(@"dir3\file3") };

            var actual = fs.EnumerateFiles(cloneTarget)
                .Select(x => EmulatorFile.FromlFileName(x, x.Replace(cloneTarget + "\\", ""), fs));
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }

        [TestMethod]
        public void Clone_02_Clone_previous_backup_that_only_has_new_files_()
        {
            // This test simulates cloning a previoes backup that only has new files.
            // After running the tool, we expect to see all files from the backup copied into
            // the target location with the correct rlative names

            var backupSource = @"c:\source";
            var backupTarget = @"d:\target";
            var cloneSource = new CloneSource { OriginalSourcePath = backupSource, BackupPath = backupTarget };
            var cloneTarget = @"d:\cloneTarget";

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
            var cmd = new RunBackupCommand(fs, backupSource, backupTarget, MachineID.One);
            cmd.Execute();

            fs.AddFiles(new[] {
                new EmulatorFile(@"c:\source\dir3\file1"),
                new EmulatorFile(@"c:\source\dir3\file2"),
                new EmulatorFile(@"c:\source\dir3\file3") });

            cmd.Execute();


            var cloneCmd = new CloneBackupCommand(fs, cloneSource, cloneTarget, 1);
            cloneCmd.Execute();

            var expected = new[]
            {
                new EmulatorFile(@"file1"),
                new EmulatorFile(@"file2"),
                new EmulatorFile(@"file3"),
                new EmulatorFile(@"dir1\file1"),
                new EmulatorFile(@"dir1\file2"),
                new EmulatorFile(@"dir1\file3"),
                new EmulatorFile(@"dir1\dir1\file1"),
                new EmulatorFile(@"dir1\dir1\file2"),
                new EmulatorFile(@"dir1\dir2\file1"),
                new EmulatorFile(@"dir2\file1"),
                new EmulatorFile(@"dir2\file2")};

            var actual = fs.EnumerateFiles(cloneTarget)
                .Select(x => EmulatorFile.FromlFileName(x, x.Replace(cloneTarget + "\\", ""), fs));
            TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
        }
    }
}