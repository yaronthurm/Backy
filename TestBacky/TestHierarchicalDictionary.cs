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
        public void HierarchicalDictionary_01_Testing_inner_tree_to_extract_items()
        {
            var tree = new HierarchicalDictionary<int, string>();
            tree.Add("item1", new[] { 1, 2, 3 });
            tree.Add("item2", new[] { 1, 2, 4 });
            tree.Add("item3", new[] { 2, 3, 4 });
            tree.Add("item4", new[] { 2, 2, 4 });
            tree.Add("item5", new[] { 1, 2, 3, 4 });

            var allItems = tree.GetAllItems();
            TestsUtils.AssertLists(new[] { "item1", "item2", "item3", "item4", "item5" }, allItems);

            var itemsFrom1 = tree.GetFirstLevelItems(new[] { 1 });
            TestsUtils.AssertLists(new string[0], itemsFrom1);

            var itemsFrom1_2 = tree.GetFirstLevelItems(new[] { 1, 2 });
            TestsUtils.AssertLists(new []{ "item1", "item2" }, itemsFrom1_2);

            var containersFrom1 = tree.GetFirstLevelContainers(new[] { 1 });
            TestsUtils.AssertLists(new[] { 2 }, containersFrom1);

            var containersFrom1_2 = tree.GetFirstLevelContainers(new[] { 1, 2 });
            TestsUtils.AssertLists(new[] { 3 }, containersFrom1_2);

            var containersFrom2 = tree.GetFirstLevelContainers(new[] { 2 });
            TestsUtils.AssertLists(new[] { 3, 2 }, containersFrom2);
        }

        [TestMethod]
        public void HierarchicalDictionary_02_Testing_Descendants()
        {
            var tree = new HierarchicalDictionary<int, string>();
            tree.Add("item1", new[] { 1, 2, 3 });
            tree.Add("item2", new[] { 1, 2, 4 });
            tree.Add("item3", new[] { 2, 3, 4 });
            tree.Add("item4", new[] { 2, 2, 4 });
            tree.Add("item5", new[] { 1, 2, 3, 4 });

            var descendants1 = tree.GetAllDescendantsItems(new[] { 1 });
            TestsUtils.AssertLists(new[] { "item1", "item2", "item5" }, descendants1);

            var descendants2 = tree.GetAllDescendantsItems(new[] { 2 });
            TestsUtils.AssertLists(new[] { "item3", "item4" }, descendants2);

            var descendants2_3 = tree.GetAllDescendantsItems(new[] { 2, 3 });
            TestsUtils.AssertLists(new[] { "item3" }, descendants2_3);
        }

    }
}

