namespace Sitecore.Support.EventHandlers
{
  using System;
  using System.Linq;

  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Publishing;
  using Sitecore.SecurityModel;
  using Sitecore.XA.Foundation.Theming;

  [UsedImplicitly]
  public class AssetContentRefresher : XA.Foundation.Theming.EventHandlers.AssetContentRefresher
  {
    [UsedImplicitly]
    public new void OnPublishEnd(object sender, EventArgs args)
    {
      // new OnPublishEnd calling base OnPublishEnd because Sitecore cannot see inherited methods
      // so explicitly called base.OnPublishEnd(...) which will call overridden RemovePublishedOptimizedItems
      base.OnPublishEnd(sender, args);
    }

    protected override void RemovePublishedOptimizedItems(Publisher publisher)
    {
      var items = GetOptimizedItems(publisher);
      if (items == null)
      {
        return;
      }

      using (new LongRunningOperationWatcher(1000, $"Deleting {items.Length} optimized SXA items"))
      foreach (var optimizedItem in items)
      {
        optimizedItem?.Delete();
      }
    }

    private static Item[] GetOptimizedItems(Publisher publisher)
    {
      var targetDatabase = publisher.Options.TargetDatabase;
      var rootItem = publisher.Options.RootItem;
      if (rootItem == null)
      {
        return null;
      }

      var item = targetDatabase.GetItem(rootItem.ID);
      if (item == null)
      {
        return null;
      }

      var items = GetOptimizedItems(item);

      // since GetOptimizedItems returns optimized items from entire database, results need additional filtering
      using (new LongRunningOperationWatcher(1000, $"Skipping optimized SXA items outside of published root item (total number: {items.Length})"))
      {
        var filteredItems = items
          .Where(x => x.Axes.IsDescendantOf(item))
          .ToArray();

        if (filteredItems.Length <= 0)
        {
          filteredItems = items
            .Where(x => x.Axes.IsDescendantOf(item.Parent))
            .ToArray();
        }

        return filteredItems;
      }
    }

    private static Item[] GetOptimizedItems(Item item)
    {
      using (new SecurityDisabler())
      using (new LongRunningOperationWatcher(1000, $"Querying optimized SXA items"))
      return DoGetOptimizedItems(item);
    }

    private static Item[] DoGetOptimizedItems(Item item)
    {
      var optimized = "optimized";
      var optimizedMin = "optimized-min";
      var query = $"fast://*[@@templateid='{Templates.OptimizedFile.ID}' and (@@name='{optimized}' or @@name='{optimizedMin}')]";
      var items = item.Database.SelectItems(query);

      return items;
    }
  }
}