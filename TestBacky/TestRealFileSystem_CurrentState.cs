using BackyLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TestBacky
{
    [TestClass]
    public class TestRealFileSystem_CurrentState
    {
        [TestMethod]
        public void RealFileSystem__CurrentState_01_Run_backup_and_test_files_on_disk()
        {
            var randDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var source = Path.Combine(randDir, "source");
            var target = Path.Combine(randDir, "target");

            Directory.CreateDirectory(target);
            Directory.CreateDirectory(source);

            try
            {
                var fs = new OSFileSystem();
                SimulateRunningBackups(source, target, fs);                
            }
            finally
            {
                Directory.Delete(randDir, true);
            }
        }

        private void SimulateRunningBackups(string source, string target, IFileSystem fs)
        {
            var cmd = new RunBackupCommand2(fs, source, target, MachineID.One);

            var now = new DateTime(2010, 1, 1);

            // Create files in source and run first time
            WriteFile(source, "file1.txt", "hello1", now);
            WriteFile(source, "file2.txt", "hello2", now);
            WriteFile(source, "file3.txt", "hello3", now);
            WriteFile(source, "file4.doc", "", now);            

            // 1
            cmd.Execute();            
            target = fs.GetTopLevelDirectories(target).First();
            AssertDirectory(fs, Path.Combine(target, "History", "1"), 
                new AssertedFile[] { 
                    new AssertedFile { Name = "new.txt", Content = "file1.txt\r\nfile2.txt\r\nfile3.txt\r\nfile4.doc" } });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] { 
                    new AssertedFile { Name = "file1.txt", Content = "hello1", Date = now },
                    new AssertedFile { Name = "file2.txt", Content = "hello2", Date = now },
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                });

            // Add new files
            WriteFile(source, "file5.txt", "hello5", now);
            WriteFile(source, "file6.txt", "hello6", now);

            // 2
            cmd.Execute();
            AssertDirectory(fs, Path.Combine(target, "History", "2"),
                new AssertedFile[] {
                    new AssertedFile { Name = "new.txt", Content = "file5.txt\r\nfile6.txt" } });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] {
                    new AssertedFile { Name = "file1.txt", Content = "hello1", Date = now },
                    new AssertedFile { Name = "file2.txt", Content = "hello2", Date = now },
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                    new AssertedFile { Name = "file5.txt", Content = "hello5", Date = now },
                    new AssertedFile { Name = "file6.txt", Content = "hello6", Date = now },
                });

            
            // Delete a few files
            File.Delete(Path.Combine(source, "file1.txt"));
            File.Delete(Path.Combine(source, "file2.txt"));

            // 3
            cmd.Execute();
            AssertDirectory(fs, Path.Combine(target, "History", "3"),
                new AssertedFile[] {
                    new AssertedFile { Name = "deleted\\file1.txt", Content = "hello1", Date = now },
                    new AssertedFile { Name = "deleted\\file2.txt", Content = "hello2", Date = now },
                });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] {
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                    new AssertedFile { Name = "file5.txt", Content = "hello5", Date = now },
                    new AssertedFile { Name = "file6.txt", Content = "hello6", Date = now },
                });
                        
            // Modify and add some files
            var now1 = new DateTime(2010, 1, 2);
            WriteFile(source, "file5.txt", "hello5 - modified", now1);
            WriteFile(source, "file6.txt", "hello6 - modified", now1);
            WriteFile(source, "file7.txt", "hello7", now1);

            // 4
            cmd.Execute();
            AssertDirectory(fs, Path.Combine(target, "History", "4"),
                new AssertedFile[] {
                    new AssertedFile { Name = "new.txt", Content = "file7.txt" },
                    new AssertedFile { Name = "modified\\file5.txt", Content = "hello5", Date = now },
                    new AssertedFile { Name = "modified\\file6.txt", Content = "hello6", Date = now },
                });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] {
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                    new AssertedFile { Name = "file5.txt", Content = "hello5 - modified", Date = now1 },
                    new AssertedFile { Name = "file6.txt", Content = "hello6 - modified", Date = now1 },
                    new AssertedFile { Name = "file7.txt", Content = "hello7", Date = now1 },
                });                       
            
            // Rename some files
            File.Move(Path.Combine(source, "file5.txt"), Path.Combine(source, "file5_renamed.txt"));
            Directory.CreateDirectory(Path.Combine(source, "subdir"));
            File.Move(Path.Combine(source, "file6.txt"), Path.Combine(source, "subdir", "file6_renamed.txt"));

            // 5
            cmd.Execute();
            AssertDirectory(fs, Path.Combine(target, "History", "5"),
                new AssertedFile[] {
                    new AssertedFile { Name = "renamed.txt", Content = "{\"oldName\":\"file5.txt\",\"newName\":\"file5_renamed.txt\"}\r\n{\"oldName\":\"file6.txt\",\"newName\":\"subdir\\\\file6_renamed.txt\"}", }
                });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] {
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                    new AssertedFile { Name = "file5_renamed.txt", Content = "hello5 - modified", Date = now1 },
                    new AssertedFile { Name = "subdir\\file6_renamed.txt", Content = "hello6 - modified", Date = now1 },
                    new AssertedFile { Name = "file7.txt", Content = "hello7", Date = now1 },
                });
            
            // Pretend to rename (2 steps) - use 2 files with same length and last modify but with slight different content
            var now2 = new DateTime(2010, 1, 3);
            WriteFile(source, "file8.txt", "hello8", now2);
            // 6
            cmd.Execute();
            AssertDirectory(fs, Path.Combine(target, "History", "6"),
                new AssertedFile[] {
                    new AssertedFile { Name = "new.txt", Content = "file8.txt" },
                });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] {
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                    new AssertedFile { Name = "file5_renamed.txt", Content = "hello5 - modified", Date = now1 },
                    new AssertedFile { Name = "subdir\\file6_renamed.txt", Content = "hello6 - modified", Date = now1 },
                    new AssertedFile { Name = "file7.txt", Content = "hello7", Date = now1 },
                    new AssertedFile { Name = "file8.txt", Content = "hello8", Date = now2 },
                });
            
            File.Delete(Path.Combine(source, "file8.txt"));
            WriteFile(source, "file8_pretend_rename.txt", "hello9", now2);

            // 7
            cmd.Execute();
            AssertDirectory(fs, Path.Combine(target, "History", "7"),
                new AssertedFile[] {
                    new AssertedFile { Name = "new.txt", Content = "file8_pretend_rename.txt" },
                    new AssertedFile { Name = "deleted\\file8.txt", Content = "hello8", Date = now2 },
                });
            AssertDirectory(fs, Path.Combine(target, "CurrentState"),
                new AssertedFile[] {
                    new AssertedFile { Name = "file3.txt", Content = "hello3", Date = now },
                    new AssertedFile { Name = "file4.doc", Content = "", Date = now },
                    new AssertedFile { Name = "file5_renamed.txt", Content = "hello5 - modified", Date = now1 },
                    new AssertedFile { Name = "subdir\\file6_renamed.txt", Content = "hello6 - modified", Date = now1 },
                    new AssertedFile { Name = "file7.txt", Content = "hello7", Date = now1 },
                    new AssertedFile { Name = "file8_pretend_rename.txt", Content = "hello9", Date = now2 },
                });
        }


        private static void WriteFile(string dir, string name, string content, DateTime date)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllText(path, content);
            File.SetLastWriteTime(path, date);
        }

        private static void AssertFile(string path, string content, DateTime? updated)
        {

        }

        
        private class AssertedFile
        {
            public string Name { get; set; }
            public string Content { get; set; }
            public DateTime? Date { get; set; }
        } 

        private static void AssertDirectory(IFileSystem fs, string path, IEnumerable<AssertedFile> expectedFiles)
        {
            var actualFilesDic = fs.EnumerateFiles(path)
                .Select(x => new AssertedFile
                {
                    Name = x.Replace(path + "\\", ""),
                    Content = string.Join("\r\n", fs.ReadLines(x)),
                    Date = fs.GetLastWriteTime(x)
                }).ToDictionary(x => x.Name, x => x);
            var expectedFilesDic = expectedFiles.ToDictionary(x => x.Name, x => x);

            foreach (var kvp in expectedFilesDic)
            {
                if (!actualFilesDic.ContainsKey(kvp.Key))
                    Assert.Fail($"Missing file '{kvp.Key}'");
                if (kvp.Value.Content != null && 
                    actualFilesDic[kvp.Key].Content != kvp.Value.Content)
                    Assert.Fail($"Mismatch content for file '{kvp.Key}'.\nExpected: {kvp.Value.Content}\nActual: {actualFilesDic[kvp.Key].Content}");
                if (kvp.Value.Date != null &&
                    actualFilesDic[kvp.Key].Date != kvp.Value.Date)
                    Assert.Fail($"Mismatch date for file '{kvp.Key}'.\nExpected: {kvp.Value.Date}\nActual: {actualFilesDic[kvp.Key].Date}");
            }

            foreach (var kvp in actualFilesDic)
            {
                if (!expectedFilesDic.ContainsKey(kvp.Key))
                    Assert.Fail($"Unexpected file found {kvp.Key}");
            }
        }
    }
}