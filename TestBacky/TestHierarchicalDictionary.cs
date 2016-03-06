using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackyLogic;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestBacky
{
    [TestClass]
    public class TestHierarchicalDictionaryClass
    {
        [TestMethod]
        public void Test01_Testing_inner_tree_to_extract_items()
        {
            var tree = new HierarchicalDictionary<int, string>();
            tree.Add("item1", new[] { 1, 2, 3 });
            tree.Add("item2", new[] { 1, 2, 4 });
            tree.Add("item3", new[] { 2, 3, 4 });
            tree.Add("item4", new[] { 2, 2, 4 });
            tree.Add("item5", new[] { 1, 2, 3, 4 });

            var allItems = tree.GetAllItems();
            AssertLists(new[] { "item1", "item2", "item3", "item4", "item5" }, allItems);

            var itemsFrom1 = tree.GetFirstLevelItems(new[] { 1 });
            AssertLists(new string[0], itemsFrom1);

            var itemsFrom1_2 = tree.GetFirstLevelItems(new[] { 1, 2 });
            AssertLists(new []{ "item1", "item2" }, itemsFrom1_2);

            var containersFrom1 = tree.GetFirstLevelContainers(new[] { 1 });
            AssertLists(new[] { 2 }, containersFrom1);

            var containersFrom1_2 = tree.GetFirstLevelContainers(new[] { 1, 2 });
            AssertLists(new[] { 3 }, containersFrom1_2);

            var containersFrom2 = tree.GetFirstLevelContainers(new[] { 2 });
            AssertLists(new[] { 3, 2 }, containersFrom2);
        }

        [TestMethod]
        public void Test02_Testing_Descendants()
        {
            var tree = new HierarchicalDictionary<int, string>();
            tree.Add("item1", new[] { 1, 2, 3 });
            tree.Add("item2", new[] { 1, 2, 4 });
            tree.Add("item3", new[] { 2, 3, 4 });
            tree.Add("item4", new[] { 2, 2, 4 });
            tree.Add("item5", new[] { 1, 2, 3, 4 });

            var descendants1 = tree.GetAllDescendantsItems(new[] { 1 });
            AssertLists(new[] { "item1", "item2", "item5" }, descendants1);

            var descendants2 = tree.GetAllDescendantsItems(new[] { 2 });
            AssertLists(new[] { "item3", "item4" }, descendants2);

            var descendants2_3 = tree.GetAllDescendantsItems(new[] { 2, 3 });
            AssertLists(new[] { "item3" }, descendants2_3);
        }





        private static void AssertLists<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var diff = expected.Except(actual);
            Assert.IsFalse(diff.Any(), "missing items: " + string.Join(",", diff));

            diff = actual.Except(expected);
            Assert.IsFalse(diff.Any(), "extra items: " + string.Join(",", diff));
        }
    }
}

