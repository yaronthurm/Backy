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


        public static void AssertState(IFileSystem fs, string target, params IEnumerable<string>[] expectedFilesByVersion)
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
    }
}
