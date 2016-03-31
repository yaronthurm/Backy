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


        public static void AssertStateByRelativeFileName(IFileSystem fs, string target, params IEnumerable<string>[] expectedFilesByVersion)
        {
            var stateCalculator = new StateCalculator(fs, target);
            stateCalculator.MaxVersion.ShouldBe(expectedFilesByVersion.Length);
            for (int i = 0; i < expectedFilesByVersion.Length; i++)
            {
                var state = stateCalculator.GetState(i + 1);
                state.GetFiles().Select(x => x.RelativeName).ShouldBe(expectedFilesByVersion[i], ignoreOrder: true);
            }

            var latestState = stateCalculator.GetLastState();
            latestState.GetFiles().Select(x => x.RelativeName).ShouldBe(expectedFilesByVersion.Last(), ignoreOrder: true);
        }

        public static void AssertStateFull(IFileSystem fs, string target, params IEnumerable<EmulatorFile>[] expectedFilesByVersion)
        {
            var stateCalculator = new StateCalculator(fs, target);
            stateCalculator.MaxVersion.ShouldBe(expectedFilesByVersion.Length);
            for (int i = 0; i < expectedFilesByVersion.Length; i++)
            {
                var state = stateCalculator.GetState(i + 1);
                var actualFiles = state.GetFiles().Select(x => new EmulatorFile(x.RelativeName, x.LastWriteTime));
                var expectedFiles = expectedFilesByVersion[i];
                AssertEmulatorFiles(fs, expectedFiles, actualFiles);
            }

            {
                var latestState = stateCalculator.GetLastState();
                var actualFiles = latestState.GetFiles().Select(x => new EmulatorFile(x.RelativeName, x.LastWriteTime)); 
                var expectedFiles = expectedFilesByVersion.Last();
                AssertEmulatorFiles(fs, expectedFiles, actualFiles);
            }
        }

        public static void AssertEmulatorFiles(IFileSystem fs, IEnumerable<EmulatorFile> expected, IEnumerable<EmulatorFile> actual)
        {
            // First, assert relative names
            actual.Select(x => x.Name).ShouldBe(expected.Select(x => x.Name), ignoreOrder: true);

            // Now check one by one
            foreach (var actualFile in actual)
            {
                var correspondingExpectedFile = expected.First(x => x.Name == actualFile.Name);
                actualFile.LastModified.ShouldBe(correspondingExpectedFile.LastModified);
                actualFile.Content.ShouldBe(correspondingExpectedFile.Content);
            }
        }
    }
}
