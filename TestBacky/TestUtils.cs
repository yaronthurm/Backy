﻿using BackyLogic;
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
                var state = stateCalculator.GetState(i + 1);
                var actualFiles = state.GetFiles().Select(x => EmulatorFile.FromlFileName(x.PhysicalPath, x.RelativeName, fs));
                var expectedFiles = expectedFilesByVersion[i];
                AssertEmulatorFiles(fs, expectedFiles, actualFiles, "index: " + (i+1));
            }

            {
                var latestState = stateCalculator.GetLastState();
                var actualFiles = latestState.GetFiles().Select(x => EmulatorFile.FromlFileName(x.PhysicalPath, x.RelativeName, fs));
                var expectedFiles = expectedFilesByVersion.Last();
                AssertEmulatorFiles(fs, expectedFiles, actualFiles, "index: last");
            }
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
            }
        }
    }
}
