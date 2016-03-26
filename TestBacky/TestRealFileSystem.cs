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

            // Clear target
            if (Directory.Exists(target)) // This method sometimes return true when the directory doesn't realy exists
                Directory.Delete(target, true);
            Directory.CreateDirectory(target);

            // Clear source
            if (Directory.Exists(source))
                Directory.Delete(source, true);
            Directory.CreateDirectory(source);

            try
            {
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

                // Rename some files
                File.Move(Path.Combine(source, "file5.txt"), Path.Combine(source, "file5_renamed.txt"));
                Directory.CreateDirectory(Path.Combine(source, "subdir"));
                File.Move(Path.Combine(source, "file6.txt"), Path.Combine(source, "subdir", "file6_renamed.txt"));

                // 5
                cmd.Execute();

                // Pretend to rename - use 2 files with same length and last modify but with slight different content
                DateTime now = DateTime.Now;
                File.WriteAllText(Path.Combine(source, "file8.txt"), "hello8");
                File.SetLastWriteTime(Path.Combine(source, "file8.txt"), now);
                // 6
                cmd.Execute();
                File.Delete(Path.Combine(source, "file8.txt"));
                File.WriteAllText(Path.Combine(source, "file8_pretend_rename.txt"), "hello9");
                File.SetLastWriteTime(Path.Combine(source, "file8_pretend_rename.txt"), now);

                // 7
                cmd.Execute();

                // Assert existence of files according to structure
                var actualTargetFiles = Directory.GetFiles(target, "*", SearchOption.AllDirectories);
                var expectedTargetFiles = new[]
                    { "1\\new\\file1.txt", "1\\new\\file2.txt", "1\\new\\file3.txt", "1\\new\\file4.doc",
                  "2\\new\\file5.txt", "2\\new\\file6.txt",
                  "3\\deleted.txt",
                  "4\\modified\\file5.txt", "4\\modified\\file6.txt", "4\\new\\file7.txt",
                  "5\\renamed.txt",
                  "6\\new\\file8.txt",
                  "7\\deleted.txt", "7\\new\\file8_pretend_rename.txt"
            }.Select(x => Path.Combine(target, x));
                TestsUtils.AssertLists(expectedTargetFiles, actualTargetFiles);

                // Assert deleted file are marked correctly
                var expectedDeleted = new[] { "file1.txt", "file2.txt" };
                var actualDeleted = File.ReadAllLines(Path.Combine(target, "3", "deleted.txt"));
                TestsUtils.AssertLists(expectedDeleted, actualDeleted);

                // Assert renamed files are marked correctly
                var renamedFiles = File.ReadAllLines(Path.Combine(target, "5", "renamed.txt"))
                    .Select(JObject.Parse)
                    .Select(x => new
                    {
                        oldName = x.Value<string>("oldName"),
                        newName = x.Value<string>("newName")
                    }).ToArray();

                Assert.AreEqual(2, renamedFiles.Length);
                Assert.AreEqual("file5.txt", renamedFiles[0].oldName);
                Assert.AreEqual("file5_renamed.txt", renamedFiles[0].newName);
                Assert.AreEqual("file6.txt", renamedFiles[1].oldName);
                Assert.AreEqual("subdir\\file6_renamed.txt", renamedFiles[1].newName);
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
        public void RealFileSystem_02_Run_backup_and_test_state_as_the_app_sees()
        {
            var source = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var target = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            // Clear target
            if (Directory.Exists(target)) // This method sometimes return true when the directory doesn't realy exists
                Directory.Delete(target, true);
            Directory.CreateDirectory(target);

            // Clear source
            if (Directory.Exists(source))
                Directory.Delete(source, true);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new FileSystem();
                var cmd = new RunBackupCommand(fs, source, target);

                // Create files in source and run first time
                File.WriteAllText(Path.Combine(source, "file1.txt"), "hello1");
                File.WriteAllText(Path.Combine(source, "file2.txt"), "hello2");
                File.WriteAllText(Path.Combine(source, "file3.txt"), "hello3");
                File.WriteAllText(Path.Combine(source, "file4.doc"), "");

                // 1
                cmd.Execute();
                var state = new TransientState(fs, target).GetLastState();
                TestsUtils.AssertLists(new[] { "file1.txt", "file2.txt", "file3.txt", "file4.doc" }, state.GetFiles().Select(x => x.RelativeName));

                // Add new files
                File.WriteAllText(Path.Combine(source, "file5.txt"), "hello5");
                File.WriteAllText(Path.Combine(source, "file6.txt"), "hello6");

                // 2
                cmd.Execute();
                state = new TransientState(fs, target).GetLastState();
                TestsUtils.AssertLists(new[] { "file1.txt", "file2.txt", "file3.txt", "file4.doc",
                "file5.txt", "file6.txt" }, state.GetFiles().Select(x => x.RelativeName));

                // Delete a few files
                File.Delete(Path.Combine(source, "file1.txt"));
                File.Delete(Path.Combine(source, "file2.txt"));

                // 3
                cmd.Execute();
                state = new TransientState(fs, target).GetLastState();
                TestsUtils.AssertLists(new[] { "file3.txt", "file4.doc", "file5.txt", "file6.txt" }, state.GetFiles().Select(x => x.RelativeName));

                // Modify and add some files
                File.WriteAllText(Path.Combine(source, "file5.txt"), "hello5 - modified");
                File.WriteAllText(Path.Combine(source, "file6.txt"), "hello6 - modified");
                File.WriteAllText(Path.Combine(source, "file7.txt"), "hello7");

                // 4
                cmd.Execute();
                state = new TransientState(fs, target).GetLastState();
                TestsUtils.AssertLists(new[] { "file3.txt", "file4.doc", "file5.txt", "file6.txt", "file7.txt" }, state.GetFiles().Select(x => x.RelativeName));

                // Rename some files
                File.Move(Path.Combine(source, "file5.txt"), Path.Combine(source, "file5_renamed.txt"));
                Directory.CreateDirectory(Path.Combine(source, "subdir"));
                File.Move(Path.Combine(source, "file6.txt"), Path.Combine(source, "subdir", "file6_renamed.txt"));

                // 5
                cmd.Execute();
                state = new TransientState(fs, target).GetLastState();
                TestsUtils.AssertLists(new[] { "file3.txt", "file4.doc", "file5_renamed.txt", "subdir\\file6_renamed.txt", "file7.txt" }, state.GetFiles().Select(x => x.RelativeName));

                // Pretend to rename - use 2 files with same length and last modify but with slight different content
                DateTime now = DateTime.Now;
                File.WriteAllText(Path.Combine(source, "file8.txt"), "hello8");
                File.SetLastWriteTime(Path.Combine(source, "file8.txt"), now);
                // 6
                cmd.Execute();
                File.Delete(Path.Combine(source, "file8.txt"));
                File.WriteAllText(Path.Combine(source, "file8_pretend_rename.txt"), "hello9");
                File.SetLastWriteTime(Path.Combine(source, "file8_pretend_rename.txt"), now);

                // 7
                cmd.Execute();
                state = new TransientState(fs, target).GetLastState();
                TestsUtils.AssertLists(new[] { "file3.txt", "file4.doc", "file5_renamed.txt",
                "subdir\\file6_renamed.txt", "file7.txt", "file8_pretend_rename.txt" }
                , state.GetFiles().Select(x => x.RelativeName));

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
        public void RealFileSystem_03_Run_clone_on_different_stages()
        {
            var cloneSource = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var cloneTarget = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            // Clear target
            if (Directory.Exists(cloneTarget)) // This method sometimes return true when the directory doesn't realy exists
                Directory.Delete(cloneTarget, true);
            Directory.CreateDirectory(cloneTarget);

            // Clear source
            if (Directory.Exists(cloneSource))
                Directory.Delete(cloneSource, true);
            Directory.CreateDirectory(cloneSource);

            try
            {
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
    }
}