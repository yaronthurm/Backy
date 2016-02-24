using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;

namespace TestBacky
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var source = @"c:\source";
            var target = @"d:\target";

            var sourceFiles = new string[] { @"file1.txt", @"file2.txt", @"subdir\file11.txt" }.Select(x => Path.Combine(source, x));
            var destFiles = new string[] { @"1\file1.txt", @"1\file3.txt", @"1\subdir\file11.txt", @"1\subdir\file33.txt" }.Select(x => Path.Combine(target, x)); 
            var cmd = new RunBackupCommand(new FileSystemEmulator(sourceFiles.ToList(), destFiles.ToList()), source, target);
            cmd.Execute();
        }
    }
}
