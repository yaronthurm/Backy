using BackyLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBacky
{
    public static class TestsUtils
    {
        public static void AssertLists<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var diff = expected.Except(actual);
            Assert.IsFalse(diff.Any(), "missing items");

            diff = actual.Except(expected);
            Assert.IsFalse(diff.Any(), "extra items");
        }

        public static void AssertState(IFileSystem fs, string target, string source, params IEnumerable<EmulatorFile>[] expectedFilesByVersion)
        {
            var stateCalculator = new StateCalculator(fs, target, source, MachineID.One.Value);
            stateCalculator.MaxVersion.ShouldBe(expectedFilesByVersion.Length);
            for (int i = 0; i < expectedFilesByVersion.Length; i++)
            {
                var actualFiles = GetFilesForVersion(fs, target, source, i + 1);
                var expectedFiles = expectedFilesByVersion[i];
                AssertEmulatorFiles(fs, expectedFiles, actualFiles, "index: " + (i+1));
            }
        }

        public static void AssertLastState(IFileSystem fs, string target, string source, IEnumerable<EmulatorFile> expectedFiles, string msg = "")
        {
            var stateCalculator = new StateCalculator(fs, target, source, MachineID.One.Value);
            var actualFiles = GetFilesForVersion(fs, target, source, stateCalculator.MaxVersion);
            AssertEmulatorFiles(fs, expectedFiles, actualFiles, msg);
        }

        public static void AssertEmulatorFiles(IFileSystem fs, IEnumerable<EmulatorFile> expected, IEnumerable<EmulatorFile> actual, string msg)
        {
            // First, assert relative names
            actual.Select(x => x.Name).ShouldBe(expected.Select(x => x.Name), ignoreOrder: true, customMessage: msg);

            // Now check one by one
            foreach (var actualFile in actual)
            {
                msg = "file: " + actualFile.Name + " " + msg;
                var correspondingExpectedFile = expected.First(x => x.Name == actualFile.Name);
                actualFile.LastModified.ShouldBe(correspondingExpectedFile.LastModified, customMessage: msg);
                actualFile.Content.ShouldBe(correspondingExpectedFile.Content, customMessage: msg);
                actualFile.IsShallow.ShouldBe(correspondingExpectedFile.IsShallow, customMessage: msg);
                if (correspondingExpectedFile.PhysicalRelativePath != null)
                    actualFile.PhysicalRelativePath.ShouldBe(correspondingExpectedFile.PhysicalRelativePath, customMessage: msg);
            }
        }
        
        private static IEnumerable<EmulatorFile> GetFilesForVersion(IFileSystem fs, string target, string source, int version)
        {
            var stateCalculator = new StateCalculator(fs, target, source, MachineID.One.Value);
            var state = stateCalculator.GetState(version);
            var actualFiles = state.GetFiles()
                .Select(x =>
                {
                    if (x.IsShallow)
                        return EmulatorFile.FromShallowData(x.RelativeName, x.LastWriteTime);
                    else
                        return EmulatorFile.FromFileName(x.PhysicalPath, x.RelativeName, fs, stateCalculator.Target);
                });
            return actualFiles;
        }               
    }
}
