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
    public class TestRealFileSystem
    {
        [TestMethod]
        public void RealFileSystem_01_Run_backup_and_test_files_on_disk()
        {
            var source = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var target = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            Directory.CreateDirectory(target);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new FileSystem();
                var expectedStates = SimulateRunningBackups(source, target, fs);
                TestsUtils.AssertState(fs, target, expectedStates);
            }
            finally
            {
                foreach (var dir in Directory.GetDirectories(target))
                    UnmarkDirectoryAsReadOnly(dir);
                Directory.Delete(target, true);
                Directory.Delete(source, true);
            }
        }
        
        [TestMethod]
        public void RealFileSystem_02_Run_clone_on_different_stages()
        {
            var backupSource = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var backupTarget = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var cloneTarget = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var cloneSource = backupTarget;

            Directory.CreateDirectory(backupSource);
            Directory.CreateDirectory(backupTarget);
            Directory.CreateDirectory(cloneTarget);
            

            try
            {
                var fs = new FileSystem();
                var expectedStates = SimulateRunningBackups(backupSource, backupTarget, fs);

                // 1 - new files
                Directory.CreateDirectory(Path.Combine(cloneSource, "1\\new"));
                File.WriteAllText(Path.Combine(cloneSource, "1\\new\\file1.txt"), "hello1");
                File.WriteAllText(Path.Combine(cloneSource, "1\\new\\file2.txt"), "hello2");
                File.WriteAllText(Path.Combine(cloneSource, "1\\new\\file3.txt"), "hello3");
                File.WriteAllText(Path.Combine(cloneSource, "1\\new\\file4.doc"), "");

                // 2 - more new files
                Directory.CreateDirectory(Path.Combine(cloneSource, "2\\new"));
                File.WriteAllText(Path.Combine(cloneSource, "2\\new\\file5.txt"), "hello5");
                File.WriteAllText(Path.Combine(cloneSource, "2\\new\\file6.txt"), "hello6");

                // 3 - deleted files
                Directory.CreateDirectory(Path.Combine(cloneSource, "3"));
                File.WriteAllText(Path.Combine(cloneSource, "3\\deleted.txt"), "file1.txt\nfile2.txt");

                // 4 - Modify and add some files
                Directory.CreateDirectory(Path.Combine(cloneSource, "4\\new"));
                Directory.CreateDirectory(Path.Combine(cloneSource, "4\\modified"));
                File.WriteAllText(Path.Combine(cloneSource, "4\\modified\\file5.txt"), "hello5 - modified");
                File.WriteAllText(Path.Combine(cloneSource, "4\\modified\\file6.txt"), "hello6 - modified");
                File.WriteAllText(Path.Combine(cloneSource, "4\\new\\file7.txt"), "hello7");

                // 5 - Rename some files
                Directory.CreateDirectory(Path.Combine(cloneSource, "5"));
                File.WriteAllText(Path.Combine(cloneSource, "5\\renamed.txt"),
                    "{\"oldName\":\"file5.txt\",\"newName\": \"file5_renamed.txt\"}" + "\n" +
                    "{\"oldName\":\"file6.txt\",\"newName\": \"subdir\\\\file6_renamed.txt\"}");

                // Clone version 1
                {
                    var cmd = new CloneBackupCommand(new FileSystem(), cloneSource, cloneTarget, 1);
                    cmd.Execute();

                    // Expected to see all files from version 1
                    var expected = new[]
                    {
                        Path.Combine(cloneTarget, "file1.txt"),
                        Path.Combine(cloneTarget, "file2.txt"),
                        Path.Combine(cloneTarget, "file3.txt"),
                        Path.Combine(cloneTarget, "file4.doc"),
                    };
                    TestsUtils.AssertLists(expected, Directory.GetFiles(cloneTarget));
                    Assert.AreEqual("hello1", File.ReadAllText(Path.Combine(cloneTarget, "file1.txt")));
                    Assert.AreEqual("hello2", File.ReadAllText(Path.Combine(cloneTarget, "file2.txt")));
                    Assert.AreEqual("hello3", File.ReadAllText(Path.Combine(cloneTarget, "file3.txt")));
                    Assert.AreEqual("", File.ReadAllText(Path.Combine(cloneTarget, "file4.doc")));
                }

                // Clone version 2
                {
                    Directory.Delete(cloneTarget, true);
                    var cmd = new CloneBackupCommand(new FileSystem(), cloneSource, cloneTarget, 2);
                    cmd.Execute();

                    // Expected to see all files from version 1
                    var expected = new[]
                    {
                        Path.Combine(cloneTarget, "file1.txt"),
                        Path.Combine(cloneTarget, "file2.txt"),
                        Path.Combine(cloneTarget, "file3.txt"),
                        Path.Combine(cloneTarget, "file4.doc"),
                        Path.Combine(cloneTarget, "file5.txt"),
                        Path.Combine(cloneTarget, "file6.txt")
                    };
                    TestsUtils.AssertLists(expected, Directory.GetFiles(cloneTarget));
                    Assert.AreEqual("hello1", File.ReadAllText(Path.Combine(cloneTarget, "file1.txt")));
                    Assert.AreEqual("hello2", File.ReadAllText(Path.Combine(cloneTarget, "file2.txt")));
                    Assert.AreEqual("hello3", File.ReadAllText(Path.Combine(cloneTarget, "file3.txt")));
                    Assert.AreEqual("", File.ReadAllText(Path.Combine(cloneTarget, "file4.doc")));
                    Assert.AreEqual("hello5", File.ReadAllText(Path.Combine(cloneTarget, "file5.txt")));
                    Assert.AreEqual("hello6", File.ReadAllText(Path.Combine(cloneTarget, "file6.txt")));
                }

                // Clone version 3
                {
                    Directory.Delete(cloneTarget, true);
                    var cmd = new CloneBackupCommand(new FileSystem(), cloneSource, cloneTarget, 3);
                    cmd.Execute();

                    // Expected to see all files from version 1
                    var expected = new[]
                    {
                        Path.Combine(cloneTarget, "file3.txt"),
                        Path.Combine(cloneTarget, "file4.doc"),
                        Path.Combine(cloneTarget, "file5.txt"),
                        Path.Combine(cloneTarget, "file6.txt")
                    };
                    TestsUtils.AssertLists(expected, Directory.GetFiles(cloneTarget));
                    Assert.AreEqual("hello3", File.ReadAllText(Path.Combine(cloneTarget, "file3.txt")));
                    Assert.AreEqual("", File.ReadAllText(Path.Combine(cloneTarget, "file4.doc")));
                    Assert.AreEqual("hello5", File.ReadAllText(Path.Combine(cloneTarget, "file5.txt")));
                    Assert.AreEqual("hello6", File.ReadAllText(Path.Combine(cloneTarget, "file6.txt")));
                }

                // Clone version 4
                {
                    Directory.Delete(cloneTarget, true);
                    var cmd = new CloneBackupCommand(new FileSystem(), cloneSource, cloneTarget, 4);
                    cmd.Execute();

                    // Expected to see all files from version 1
                    var expected = new[]
                    {
                        Path.Combine(cloneTarget, "file3.txt"),
                        Path.Combine(cloneTarget, "file4.doc"),
                        Path.Combine(cloneTarget, "file5.txt"),
                        Path.Combine(cloneTarget, "file6.txt"),
                        Path.Combine(cloneTarget, "file7.txt")
                    };
                    TestsUtils.AssertLists(expected, Directory.GetFiles(cloneTarget));
                    Assert.AreEqual("hello3", File.ReadAllText(Path.Combine(cloneTarget, "file3.txt")));
                    Assert.AreEqual("", File.ReadAllText(Path.Combine(cloneTarget, "file4.doc")));
                    Assert.AreEqual("hello5 - modified", File.ReadAllText(Path.Combine(cloneTarget, "file5.txt")));
                    Assert.AreEqual("hello6 - modified", File.ReadAllText(Path.Combine(cloneTarget, "file6.txt")));
                    Assert.AreEqual("hello7", File.ReadAllText(Path.Combine(cloneTarget, "file7.txt")));
                }

                // Clone version 5
                {
                    Directory.Delete(cloneTarget, true);
                    var cmd = new CloneBackupCommand(new FileSystem(), cloneSource, cloneTarget, 5);
                    cmd.Execute();

                    // Expected to see all files from version 1
                    var expected = new[]
                    {
                        Path.Combine(cloneTarget, "file3.txt"),
                        Path.Combine(cloneTarget, "file4.doc"),
                        Path.Combine(cloneTarget, "file5_renamed.txt"),
                        Path.Combine(cloneTarget, "subdir\\file6_renamed.txt"),
                        Path.Combine(cloneTarget, "file7.txt")
                    };
                    TestsUtils.AssertLists(expected, Directory.GetFiles(cloneTarget, "*", SearchOption.AllDirectories));
                    Assert.AreEqual("hello3", File.ReadAllText(Path.Combine(cloneTarget, "file3.txt")));
                    Assert.AreEqual("", File.ReadAllText(Path.Combine(cloneTarget, "file4.doc")));
                    Assert.AreEqual("hello5 - modified", File.ReadAllText(Path.Combine(cloneTarget, "file5_renamed.txt")));
                    Assert.AreEqual("hello6 - modified", File.ReadAllText(Path.Combine(cloneTarget, "subdir\\file6_renamed.txt")));
                    Assert.AreEqual("hello7", File.ReadAllText(Path.Combine(cloneTarget, "file7.txt")));
                }
            }
            finally
            {
                Directory.Delete(cloneTarget, true);
                Directory.Delete(cloneSource, true);
            }
        }




        private IEnumerable<EmulatorFile>[] SimulateRunningBackups(string source, string target, IFileSystem fs)
        {
            var cmd = new RunBackupCommand(fs, source, target);

            var now = new DateTime(2010, 1, 1);

            // Create files in source and run first time
            WriteFile(source, "file1.txt", "hello1", now);
            WriteFile(source, "file2.txt", "hello2", now);
            WriteFile(source, "file3.txt", "hello3", now);
            WriteFile(source, "file4.doc", "", now);

            // 1
            cmd.Execute();
            var expectedVersion1 = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1"),
                    new EmulatorFile(@"file2.txt", now, "hello2"),
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, "")};

            // Add new files
            WriteFile(source, "file5.txt", "hello5", now);
            WriteFile(source, "file6.txt", "hello6", now);

            // 2
            cmd.Execute();
            var expectedVersion2 = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1"),
                    new EmulatorFile(@"file2.txt", now, "hello2"),
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, ""),
                    new EmulatorFile(@"file5.txt", now, "hello5"),
                    new EmulatorFile(@"file6.txt", now, "hello6")};

            // Delete a few files
            File.Delete(Path.Combine(source, "file1.txt"));
            File.Delete(Path.Combine(source, "file2.txt"));

            // 3
            cmd.Execute();
            var expectedVersion3 = new[] {
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, ""),
                    new EmulatorFile(@"file5.txt", now, "hello5"),
                    new EmulatorFile(@"file6.txt", now, "hello6")};

            // Modify and add some files
            var now1 = new DateTime(2010, 1, 2);
            WriteFile(source, "file5.txt", "hello5 - modified", now1);
            WriteFile(source, "file6.txt", "hello6 - modified", now1);
            WriteFile(source, "file7.txt", "hello7", now1);

            // 4
            cmd.Execute();
            var expectedVersion4 = new[] {
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, ""),
                    new EmulatorFile(@"file5.txt", now1, "hello5 - modified"),
                    new EmulatorFile(@"file6.txt", now1, "hello6 - modified"),
                    new EmulatorFile(@"file7.txt", now1, "hello7")
                };

            // Rename some files
            File.Move(Path.Combine(source, "file5.txt"), Path.Combine(source, "file5_renamed.txt"));
            Directory.CreateDirectory(Path.Combine(source, "subdir"));
            File.Move(Path.Combine(source, "file6.txt"), Path.Combine(source, "subdir", "file6_renamed.txt"));

            // 5
            cmd.Execute();
            var expectedVersion5 = new[] {
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, ""),
                    new EmulatorFile(@"file5_renamed.txt", now1, "hello5 - modified"),
                    new EmulatorFile(@"subdir\file6_renamed.txt", now1, "hello6 - modified"),
                    new EmulatorFile(@"file7.txt", now1, "hello7")
                };

            // Pretend to rename - use 2 files with same length and last modify but with slight different content
            var now2 = new DateTime(2010, 1, 3);
            WriteFile(source, "file8.txt", "hello8", now2);
            // 6
            cmd.Execute();
            var expectedVersion6 = new[] {
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, ""),
                    new EmulatorFile(@"file5_renamed.txt", now1, "hello5 - modified"),
                    new EmulatorFile(@"subdir\file6_renamed.txt", now1, "hello6 - modified"),
                    new EmulatorFile(@"file7.txt", now1, "hello7"),
                    new EmulatorFile(@"file8.txt", now2, "hello8")
                };

            File.Delete(Path.Combine(source, "file8.txt"));
            WriteFile(source, "file8_pretend_rename.txt", "hello9", now2);

            // 7
            cmd.Execute();
            var expectedVersion7 = new[] {
                    new EmulatorFile(@"file3.txt", now, "hello3"),
                    new EmulatorFile(@"file4.doc", now, ""),
                    new EmulatorFile(@"file5_renamed.txt", now1, "hello5 - modified"),
                    new EmulatorFile(@"subdir\file6_renamed.txt", now1, "hello6 - modified"),
                    new EmulatorFile(@"file7.txt", now1, "hello7"),
                    new EmulatorFile(@"file8_pretend_rename.txt", now2, "hello9")
                };

            return new[] {expectedVersion1, expectedVersion2, expectedVersion3, expectedVersion4,
                    expectedVersion5, expectedVersion6, expectedVersion7};
        }

        private void UnmarkDirectoryAsReadOnly(string dirName)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.SetAccessRuleProtection(true, false); // Disable inheritance
            dSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    "Everyone",
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        private static void WriteFile(string dir, string name, string content, DateTime date)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllText(path, content);
            File.SetLastWriteTime(path, date);
        }
    }
}