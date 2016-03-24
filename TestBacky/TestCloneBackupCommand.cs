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

            var cloneSource = @"d:\cloneSource";
            var cloneTarget = @"c:\cloneTarget";

            var fileSystem = new FileSystemEmulator(new[] {
                new EmulatorFile(@"d:\cloneSource\1\new\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\file2"),
                new EmulatorFile(@"d:\cloneSource\1\new\file3"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\file2"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\file3"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\dir1\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\dir1\file2"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\dir2\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir2\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir2\file2"),
                new EmulatorFile(@"d:\cloneSource\2\new\dir3\file1"),
                new EmulatorFile(@"d:\cloneSource\2\new\dir3\file2"),
                new EmulatorFile(@"d:\cloneSource\2\new\dir3\file3"),
            });

            var cmd = new CloneBackupCommand(fileSystem, cloneSource, cloneTarget, 2);
            cmd.Execute();

            // Expected that all files from %cloneSource%\1\new will be under %cloneTarget%
            var expected = new[]
            {
                @"c:\cloneTarget\file1",
                @"c:\cloneTarget\file2",
                @"c:\cloneTarget\file3",
                @"c:\cloneTarget\dir1\file1",
                @"c:\cloneTarget\dir1\file2",
                @"c:\cloneTarget\dir1\file3",
                @"c:\cloneTarget\dir1\dir1\file1",
                @"c:\cloneTarget\dir1\dir1\file2",
                @"c:\cloneTarget\dir1\dir2\file1",
                @"c:\cloneTarget\dir2\file1",
                @"c:\cloneTarget\dir2\file2",
                @"c:\cloneTarget\dir3\file1",
                @"c:\cloneTarget\dir3\file2",
                @"c:\cloneTarget\dir3\file3"
            };
            var actual = fileSystem.EnumerateFiles(cloneTarget);
            TestsUtils.AssertLists<string>(expected, actual);
        }

        [TestMethod]
        public void Clone_02_Clone_previous_backup_that_only_has_new_files_()
        {
            // This test simulates cloning a backup that only has new files.
            // After running the tool, we expect to see all files from the backup copied into
            // the target location with the correct rlative names

            var cloneSource = @"d:\cloneSource";
            var cloneTarget = @"c:\cloneTarget";

            var fileSystem = new FileSystemEmulator(new[] {
                new EmulatorFile(@"d:\cloneSource\1\new\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\file2"),
                new EmulatorFile(@"d:\cloneSource\1\new\file3"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\file2"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\file3"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\dir1\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\dir1\file2"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir1\dir2\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir2\file1"),
                new EmulatorFile(@"d:\cloneSource\1\new\dir2\file2"),
                new EmulatorFile(@"d:\cloneSource\2\new\dir3\file1"),
                new EmulatorFile(@"d:\cloneSource\2\new\dir3\file2"),
                new EmulatorFile(@"d:\cloneSource\2\new\dir3\file3"),
            });

            var cmd = new CloneBackupCommand(fileSystem, cloneSource, cloneTarget, 1);
            cmd.Execute();

            // Expected that all files from %cloneSource%\1\new will be under %cloneTarget%
            var expected = new[]
            {
                @"c:\cloneTarget\file1",
                @"c:\cloneTarget\file2",
                @"c:\cloneTarget\file3",
                @"c:\cloneTarget\dir1\file1",
                @"c:\cloneTarget\dir1\file2",
                @"c:\cloneTarget\dir1\file3",
                @"c:\cloneTarget\dir1\dir1\file1",
                @"c:\cloneTarget\dir1\dir1\file2",
                @"c:\cloneTarget\dir1\dir2\file1",
                @"c:\cloneTarget\dir2\file1",
                @"c:\cloneTarget\dir2\file2"
            };
            var actual = fileSystem.EnumerateFiles(cloneTarget);
            TestsUtils.AssertLists<string>(expected, actual);
        }
    }
}