namespace Sitecore.Support.EventHandlers
{
  using System;

  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Publishing;
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

      return items;
    }

    private static Item[] GetOptimizedItems(Item item)
    {
      return DoGetOptimizedItems(item);
    }

    private static Item[] DoGetOptimizedItems(Item item)
    {
      var optimized = "optimized";
      var optimizedMin = "optimized-min";
      var query = $".//*[@@templateid='{Templates.OptimizedFile.ID}' and (@@name='{optimized}' or @@name='{optimizedMin}')]";
      var items = item.Axes.SelectItems(query) ?? item.Parent?.Axes.SelectItems(query);

      return items;
    }
  }
}