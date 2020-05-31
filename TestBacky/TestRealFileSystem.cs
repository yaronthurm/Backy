using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.AccessControl;
using System.Diagnostics;

namespace TestBacky
{
    [TestClass]
    public class TestRealFileSystem
    {
        //[TestMethod]
        public void UnmarkAsReadOnly()
        {
            //UpdateCreateTimeIfNeeded();
            //MarkDirectoryAsReadOnly(@"G:\Backy\3f744ba5e8424cfc9da55e2\2");
        }

        [TestMethod]
        public void RealFileSystem_01_Run_backup_and_test_files_on_disk()
        {
            var source = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var target = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            Directory.CreateDirectory(target);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new OSFileSystem();
                var expectedStates = SimulateRunningBackups(source, target, fs);
                TestsUtils.AssertState(fs, target, source, expectedStates);
            }
            finally
            {
                UnmarkDirectoryAsReadOnlyRecursive(target);
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
            var cloneSource = new CloneSource { BackupPath = backupTarget, OriginalSourcePath = backupSource };

            Directory.CreateDirectory(backupSource);
            Directory.CreateDirectory(backupTarget);
            Directory.CreateDirectory(cloneTarget);


            try
            {
                var fs = new OSFileSystem();
                var expectedStates = SimulateRunningBackups(backupSource, backupTarget, fs);

                for (int i = 0; i < expectedStates.Length; i++)
                {
                    Directory.Delete(cloneTarget, true);
                    var cmd = new CloneBackupCommand(fs, cloneSource, cloneTarget, i+1);
                    cmd.Execute();

                    var actual = Directory.GetFiles(cloneTarget, "*", SearchOption.AllDirectories)
                        .Select(x => EmulatorFile.FromFileName(x, x.Replace(cloneTarget + "\\", ""), fs));
                    TestsUtils.AssertEmulatorFiles(fs, expectedStates[i], actual, "clone: " + i);
                }
            }
            finally
            {
                UnmarkDirectoryAsReadOnlyRecursive(backupTarget);
                Directory.Delete(backupSource, true);
                Directory.Delete(backupTarget, true);
                Directory.Delete(cloneTarget, true);
            }
        }

        [TestMethod]
        public void RealFileSystem_03_Run_backup_the_backup()
        {
            var backupSource1 = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var backupSource2 = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var backupSource3 = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var backupTarget = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var backupTheBackupTarget = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            

            Directory.CreateDirectory(backupSource1);
            Directory.CreateDirectory(backupSource2);
            Directory.CreateDirectory(backupSource3);
            Directory.CreateDirectory(backupTarget);
            Directory.CreateDirectory(backupTheBackupTarget);


            try
            {
                var fs = new OSFileSystem();
                SimulateRunningBackups(backupSource1, backupTarget, fs);
                SimulateRunningBackups(backupSource2, backupTarget, fs);

                // Backup the backup
                var btbCommand = new BackupTheBackupCommand(fs, backupTarget, backupTheBackupTarget);
                btbCommand.Execute();

                // Running another backup with a new source and then backup the backup again
                SimulateRunningBackups(backupSource3, backupTarget, fs);
                btbCommand.Execute();

                // Run regular backup on one of the source
                WriteFile(backupSource1, "file1234.txt", "hello1234", DateTime.Now);
                var backupCmd = new RunBackupCommand(fs, backupSource1, backupTarget, MachineID.One);
                backupCmd.Execute();

                btbCommand.Execute();

                var expected = fs.EnumerateFiles(backupTarget).Select(x => EmulatorFile.FromFileName(x, x.Replace(backupTarget + "\\", ""), fs)).ToArray();
                var actual = fs.EnumerateFiles(backupTheBackupTarget).Select(x => EmulatorFile.FromFileName(x, x.Replace(backupTheBackupTarget + "\\", ""), fs)).ToArray();
                TestsUtils.AssertEmulatorFiles(fs, expected, actual, "");
            }
            finally
            {
                UnmarkDirectoryAsReadOnlyRecursive(backupTarget);
                UnmarkDirectoryAsReadOnlyRecursive(backupTheBackupTarget);
                Directory.Delete(backupSource1, true);
                Directory.Delete(backupSource2, true);
                Directory.Delete(backupSource3, true);
                Directory.Delete(backupTarget, true);
                Directory.Delete(backupTheBackupTarget, true);
            }
        }

        [TestMethod]
        public void RealFileSystem_04_Run_backup_on_already_shallowed_backup()
        {
            var source = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var target = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            Directory.CreateDirectory(target);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new OSFileSystem();
                var cmd = new RunBackupCommand(fs, source, target, MachineID.One);

                var now = new DateTime(2010, 1, 1);

                // Create files in source and run first time
                Enumerable.Range(1, 8).ToList().ForEach(x => WriteFile(source, $"file{x}.txt", $"hello{x}", now));
                cmd.Execute();
                var expectedVersion = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now, "hello2", false, @"\1\new\file2.txt"),
                    new EmulatorFile(@"file3.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    new EmulatorFile(@"file5.txt", now, "hello5", false, @"\1\new\file5.txt"),
                    new EmulatorFile(@"file6.txt", now, "hello6", false, @"\1\new\file6.txt"),
                    new EmulatorFile(@"file7.txt", now, "hello7", false, @"\1\new\file7.txt"),
                    new EmulatorFile(@"file8.txt", now, "hello8", false, @"\1\new\file8.txt"),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);

                // Modify 4 files and create 4 new files
                var now2 = new DateTime(2010, 1, 2);
                Enumerable.Range(5, 4).ToList().ForEach(x => WriteFile(source, $"file{x}.txt", $"hello{x} - mod", now2));
                Enumerable.Range(9, 4).ToList().ForEach(x => WriteFile(source, $"file{x}.txt", $"hello{x}", now2));
                cmd.Execute();
                expectedVersion = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now, "hello2", false, @"\1\new\file2.txt"),
                    new EmulatorFile(@"file3.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    new EmulatorFile(@"file5.txt", now2, "hello5 - mod", false, @"\2\modified\file5.txt"),
                    new EmulatorFile(@"file6.txt", now2, "hello6 - mod", false, @"\2\modified\file6.txt"),
                    new EmulatorFile(@"file7.txt", now2, "hello7 - mod", false, @"\2\modified\file7.txt"),
                    new EmulatorFile(@"file8.txt", now2, "hello8 - mod", false, @"\2\modified\file8.txt"),
                    new EmulatorFile(@"file9.txt", now2, "hello9", false, @"\2\new\file9.txt"),
                    new EmulatorFile(@"file10.txt", now2, "hello10", false, @"\2\new\file10.txt"),
                    new EmulatorFile(@"file11.txt", now2, "hello11", false, @"\2\new\file11.txt"),
                    new EmulatorFile(@"file12.txt", now2, "hello12", false, @"\2\new\file12.txt"),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);
                
                // Make ver 2 as shallow
                ShallowFoldersMaker.MakeFolderShallow(fs, target, source, MachineID.One.Value, 2);
                expectedVersion = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now, "hello2", false, @"\1\new\file2.txt"),
                    new EmulatorFile(@"file3.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    new EmulatorFile(@"file5.txt", now2, null, true, null),
                    new EmulatorFile(@"file6.txt", now2, null, true, null),
                    new EmulatorFile(@"file7.txt", now2, null, true, null),
                    new EmulatorFile(@"file8.txt", now2, null, true, null),
                    new EmulatorFile(@"file9.txt", now2, null, true, null),
                    new EmulatorFile(@"file10.txt", now2, null, true, null),
                    new EmulatorFile(@"file11.txt", now2, null, true, null),
                    new EmulatorFile(@"file12.txt", now2, null, true, null),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);

                // Delete/Modify/Rename files from the shallow folder
                var now3 = new DateTime(2010, 1, 3);
                for (int i = 1; i <= 12; i += 4)
                {
                    File.Delete(Path.Combine(source, $"file{i}.txt"));
                    WriteFile(source, $"file{i+1}.txt", $"hello{i+1} - mod2", now3);
                    File.Move(Path.Combine(source, $"file{i+2}.txt"), Path.Combine(source, $"file{i+2}_renamed.txt"));
                }

                cmd.Execute();
                expectedVersion = new[] {
                    //new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now3, "hello2 - mod2", false, @"\3\modified\file2.txt"),
                    new EmulatorFile(@"file3_renamed.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    //new EmulatorFile(@"file5.txt", now2, null, true, null),
                    new EmulatorFile(@"file6.txt", now3, "hello6 - mod2", false, @"\3\modified\file6.txt"),
                    new EmulatorFile(@"file7_renamed.txt", now2, "hello7 - mod", false, @"\3\new\file7_renamed.txt"),
                    new EmulatorFile(@"file8.txt", now2, null, true, null),
                    //new EmulatorFile(@"file9.txt", now2, null, true, null),
                    new EmulatorFile(@"file10.txt", now3, "hello10 - mod2", false, @"\3\modified\file10.txt"),
                    new EmulatorFile(@"file11_renamed.txt", now2, "hello11", false, @"\3\new\file11_renamed.txt"),
                    new EmulatorFile(@"file12.txt", now2, null, true, null),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);
            }
            finally
            {
                UnmarkDirectoryAsReadOnlyRecursive(target);
                Directory.Delete(target, true);
                Directory.Delete(source, true);
            }
        }

        [TestMethod]
        public void RealFileSystem_05_Run_shallow_backup_on_already_finisjed_backup()
        {
            var source = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var target = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            Directory.CreateDirectory(target);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new OSFileSystem();
                var cmd = new RunBackupCommand(fs, source, target, MachineID.One);

                var now = new DateTime(2010, 1, 1);

                // Create files in source and run first time
                Enumerable.Range(1, 8).ToList().ForEach(x => WriteFile(source, $"file{x}.txt", $"hello{x}", now));
                cmd.Execute();
                var expectedVersion = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now, "hello2", false, @"\1\new\file2.txt"),
                    new EmulatorFile(@"file3.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    new EmulatorFile(@"file5.txt", now, "hello5", false, @"\1\new\file5.txt"),
                    new EmulatorFile(@"file6.txt", now, "hello6", false, @"\1\new\file6.txt"),
                    new EmulatorFile(@"file7.txt", now, "hello7", false, @"\1\new\file7.txt"),
                    new EmulatorFile(@"file8.txt", now, "hello8", false, @"\1\new\file8.txt"),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);

                // Modify 4 files and create 4 new files
                var now2 = new DateTime(2010, 1, 2);
                Enumerable.Range(5, 4).ToList().ForEach(x => WriteFile(source, $"file{x}.txt", $"hello{x} - mod", now2));
                Enumerable.Range(9, 4).ToList().ForEach(x => WriteFile(source, $"file{x}.txt", $"hello{x}", now2));
                cmd.Execute();
                expectedVersion = new[] {
                    new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now, "hello2", false, @"\1\new\file2.txt"),
                    new EmulatorFile(@"file3.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    new EmulatorFile(@"file5.txt", now2, "hello5 - mod", false, @"\2\modified\file5.txt"),
                    new EmulatorFile(@"file6.txt", now2, "hello6 - mod", false, @"\2\modified\file6.txt"),
                    new EmulatorFile(@"file7.txt", now2, "hello7 - mod", false, @"\2\modified\file7.txt"),
                    new EmulatorFile(@"file8.txt", now2, "hello8 - mod", false, @"\2\modified\file8.txt"),
                    new EmulatorFile(@"file9.txt", now2, "hello9", false, @"\2\new\file9.txt"),
                    new EmulatorFile(@"file10.txt", now2, "hello10", false, @"\2\new\file10.txt"),
                    new EmulatorFile(@"file11.txt", now2, "hello11", false, @"\2\new\file11.txt"),
                    new EmulatorFile(@"file12.txt", now2, "hello12", false, @"\2\new\file12.txt"),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);

                // Delete/Modify/Rename files from the shallow folder
                var now3 = new DateTime(2010, 1, 3);
                for (int i = 1; i <= 12; i += 4)
                {
                    File.Delete(Path.Combine(source, $"file{i}.txt"));
                    WriteFile(source, $"file{i + 1}.txt", $"hello{i + 1} - mod2", now3);
                    File.Move(Path.Combine(source, $"file{i + 2}.txt"), Path.Combine(source, $"file{i + 2}_renamed.txt"));
                }

                cmd.Execute();
                expectedVersion = new[] {
                    //new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now3, "hello2 - mod2", false, @"\3\modified\file2.txt"),
                    new EmulatorFile(@"file3_renamed.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    //new EmulatorFile(@"file5.txt", now2, "hello5 - mod", false, @"\2\modified\file5.txt"),
                    new EmulatorFile(@"file6.txt", now3, "hello6 - mod2", false, @"\3\modified\file6.txt"),
                    new EmulatorFile(@"file7_renamed.txt", now2, "hello7 - mod", false, @"\2\modified\file7.txt"),
                    new EmulatorFile(@"file8.txt", now2, "hello8 - mod", false, @"\2\modified\file8.txt"),
                    //new EmulatorFile(@"file9.txt", now2, "hello9", false, @"\2\new\file9.txt"),
                    new EmulatorFile(@"file10.txt", now3, "hello10 - mod2", false, @"\3\modified\file10.txt"),
                    new EmulatorFile(@"file11_renamed.txt", now2, "hello11", false, @"\2\new\file11.txt"),
                    new EmulatorFile(@"file12.txt", now2, "hello12", false, @"\2\new\file12.txt"),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);

                // Make ver 2 shallow
                ShallowFoldersMaker.MakeFolderShallow(fs, target, source, MachineID.One.Value, 2);
                expectedVersion = new[] {
                    //new EmulatorFile(@"file1.txt", now, "hello1", false, @"\1\new\file1.txt"),
                    new EmulatorFile(@"file2.txt", now3, "hello2 - mod2", false, @"\3\modified\file2.txt"),
                    new EmulatorFile(@"file3_renamed.txt", now, "hello3", false, @"\1\new\file3.txt"),
                    new EmulatorFile(@"file4.txt", now, "hello4", false, @"\1\new\file4.txt"),
                    //new EmulatorFile(@"file5.txt", now2, "hello5 - mod", false, @"\2\modified\file5.txt"),
                    new EmulatorFile(@"file6.txt", now3, "hello6 - mod2", false, @"\3\modified\file6.txt"),
                    new EmulatorFile(@"file7_renamed.txt", now2, null, true, null),
                    new EmulatorFile(@"file8.txt", now2, null, true, null),
                    //new EmulatorFile(@"file9.txt", now2, "hello9", false, @"\2\new\file9.txt"),
                    new EmulatorFile(@"file10.txt", now3, "hello10 - mod2", false, @"\3\modified\file10.txt"),
                    new EmulatorFile(@"file11_renamed.txt", now2, null, true, null),
                    new EmulatorFile(@"file12.txt", now2, null, true, null),
                };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion);                
            }
            finally
            {
                UnmarkDirectoryAsReadOnlyRecursive(target);
                Directory.Delete(target, true);
                Directory.Delete(source, true);
            }
        }

        [TestMethod]
        public void RealFileSystem_06_Run_backup_on_shallowed_files()
        {
            var source = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var target = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            Directory.CreateDirectory(target);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new OSFileSystem();
                var cmd = new RunBackupCommand(fs, source, target, MachineID.One);
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
                TestsUtils.AssertLastState(fs, target, source, expectedVersion1);

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
                TestsUtils.AssertLastState(fs, target, source, expectedVersion2);

                // Make version 1 a shallow folder
                ShallowFoldersMaker.MakeFolderShallow(fs, target, source, MachineID.One.Value, 1);
                var expectedVersion3 = new[] {
                    new EmulatorFile(@"file1.txt", now, null, true),
                    new EmulatorFile(@"file2.txt", now, null, true),
                    new EmulatorFile(@"file3.txt", now, null, true),
                    new EmulatorFile(@"file4.doc", now, null, true),
                    new EmulatorFile(@"file5.txt", now, "hello5"),
                    new EmulatorFile(@"file6.txt", now, "hello6")};
                TestsUtils.AssertLastState(fs, target, source, expectedVersion3);

                // Modify and add some files
                var now1 = new DateTime(2010, 1, 2);
                WriteFile(source, "file1.txt", "hello1 - modified", now1);
                WriteFile(source, "file6.txt", "hello6 - modified", now1);
                WriteFile(source, "file7.txt", "hello7", now1);

                // 4
                cmd.Execute();
                var expectedVersion4 = new[] {
                    new EmulatorFile(@"file1.txt", now1, "hello1 - modified"),
                    new EmulatorFile(@"file2.txt", now, null, true),
                    new EmulatorFile(@"file3.txt", now, null, true),
                    new EmulatorFile(@"file4.doc", now, null, true),
                    new EmulatorFile(@"file5.txt", now, "hello5"),
                    new EmulatorFile(@"file6.txt", now1, "hello6 - modified"),
                    new EmulatorFile(@"file7.txt", now1, "hello7") };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion4);

                // Rename some files
                File.Move(Path.Combine(source, "file1.txt"), Path.Combine(source, "file1_renamed.txt"));
                Directory.CreateDirectory(Path.Combine(source, "subdir"));
                File.Move(Path.Combine(source, "file2.txt"), Path.Combine(source, "subdir", "file2_renamed.txt"));

                // 5
                cmd.Execute();
                var expectedVersion5 = new[] {
                    new EmulatorFile(@"file1_renamed.txt", now1, "hello1 - modified"),
                    new EmulatorFile(@"subdir\file2_renamed.txt", now, "hello2"),
                    new EmulatorFile(@"file3.txt", now, null, true),
                    new EmulatorFile(@"file4.doc", now, null, true),
                    new EmulatorFile(@"file5.txt", now, "hello5"),
                    new EmulatorFile(@"file6.txt", now1, "hello6 - modified"),
                    new EmulatorFile(@"file7.txt", now1, "hello7") };
                TestsUtils.AssertLastState(fs, target, source, expectedVersion5);
            }
            finally
            {
                UnmarkDirectoryAsReadOnlyRecursive(target);
                Directory.Delete(target, true);
                Directory.Delete(source, true);
            }

            
        }



        private IEnumerable<EmulatorFile>[] SimulateRunningBackups(string source, string target, IFileSystem fs)
        {
            var cmd = new RunBackupCommand(fs, source, target, MachineID.One);

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

        private void UnmarkDirectoryAsReadOnlyRecursive(string dirName)
        {
            foreach (var dir in Directory.GetDirectories(dirName, "*", SearchOption.AllDirectories))
                UnmarkDirectoryAsReadOnly(dir);
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

        private void MarkDirectoryAsReadOnlyRecursive(string dirName)
        {
            foreach (var dir in Directory.GetDirectories(dirName, "*", SearchOption.AllDirectories))
                MarkDirectoryAsReadOnly(dir);            
        }

        private void MarkDirectoryAsReadOnly(string dirName)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.SetAccessRuleProtection(true, false); // Disable inheritance
            dSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    "Everyone",
                    FileSystemRights.ReadAndExecute | FileSystemRights.Traverse,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        private void UpdateCreateTimeIfNeeded()
        {
            var source = "d:\\backy";
            var destination = "g:\\backy";
            foreach (var sourceDirectory in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly))
            {
                foreach (var versionDirectory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.TopDirectoryOnly))
                {
                    var destinationDirectory = Path.Combine(destination, Path.GetFileName(sourceDirectory), Path.GetFileName(versionDirectory));
                    if (!Directory.Exists(destinationDirectory)) continue;

                    var destTime = Directory.GetCreationTime(destinationDirectory);
                    var sourceTime = Directory.GetCreationTime(versionDirectory);
                    if (destTime == sourceTime) continue;
                    Trace.WriteLine(destinationDirectory);
                    UnmarkDirectoryAsReadOnly(destinationDirectory);
                    Directory.SetCreationTime(destinationDirectory, sourceTime);
                    MarkDirectoryAsReadOnly(destinationDirectory);
                }

            }
        }

        private static void WriteFile(string dir, string name, string content, DateTime date)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllText(path, content);
            File.SetLastWriteTime(path, date);
        }
    }
}