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
    public class TestRunBackupCommand_CrossMode
    {
        [TestMethod]
        public void Backup_01_1_Running_twice_with_different_modes_should_fail()
        {
            // This test simulates running the tool with mode=diff then with mode=current_state.
            // Running the second time should fail

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3") };
            var fs = new FileSystemEmulator(files);
            var cmd1 = new RunBackupCommand(fs, source, target, MachineID.One);
            cmd1.Execute(); // Running once
            
            var cmd2 = new RunBackupCommand2(fs, source, target, MachineID.One);
            try
            {
                cmd2.Execute(); // Running twice
            }
            catch (Exception)
            {
                return;
            }
            Assert.Fail("Should have thrown an exception");
        }

        [TestMethod]
        public void Backup_01_2_Running_twice_with_different_modes_should_fail()
        {
            // This test simulates running the tool with mode=current_state then with mode=diff.
            // Running the second time should fail

            var source = @"c:\source";
            var target = @"d:\target";

            var files = new EmulatorFile[] {
                new EmulatorFile(@"c:\source\file1.txt", content: "1"),
                new EmulatorFile(@"c:\source\file2.txt", content: "2"),
                new EmulatorFile(@"c:\source\subdir\file11.txt", content: "3")};
            var fs = new FileSystemEmulator(files);
            var cmd2 = new RunBackupCommand2(fs, source, target, MachineID.One);
            cmd2.Execute(); // Running once

            var cmd1 = new RunBackupCommand(fs, source, target, MachineID.One);
            try
            {
                cmd1.Execute(); // Running twice
            }
            catch (Exception)
            {
                return;
            }
            Assert.Fail("Should have thrown an exception");
        }

    }
}
