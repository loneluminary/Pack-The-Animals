using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Extensions
{
        public static class ScrollRectExtensions
        {
                public static int Search(this ScrollRect scroll, string objectName)
                {
                        if (!scroll || !scroll.content || string.IsNullOrWhiteSpace(objectName)) throw new ArgumentNullException(nameof(scroll), "ScrollRect or content is null, or objectName is invalid.");
                
                        // Optimize for performance by caching and grouping in a single pass
                        var items = scroll.content.GetComponentsInChildren<Transform>().Where(item => item != scroll.content).ToArray();
                        if (items.IsNullOrEmpty()) return 0;
                
                        var search = new List<Transform>();
                        var unSearch = new List<Transform>();

                        foreach (var item in items)
                        {
                                if (string.Equals(item.name, objectName, StringComparison.OrdinalIgnoreCase)) search.Add(item);
                                else unSearch.Add(item);
                        }

                        unSearch.ForEach(item => item.gameObject.SetActive(false));
                
                        // Return the count of matching items
                        return search.Count;
                }

                public static int ResetSearch(this ScrollRect scroll)
                {
                        if (!scroll || !scroll.content) throw new ArgumentNullException(nameof(scroll), "ScrollRect or content is null.");

                        var items = scroll.content.GetComponentsInChildren<Transform>(true).Where(item => item != scroll.content).ToArray();
                        if (items.IsNullOrEmpty()) return 0;

                        // Iterate through all items and ensure they are set to active.
                        items.ForEach(item => item.gameObject.SetActive(false));
            
                        // Return the count of re-enabled items
                        return items.Length;
                }
        }
}