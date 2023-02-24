using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Client.Records;
using Terminal.Core.CollectionSpace;
using Terminal.Core.EnumSpace;
using Terminal.Core.ModelSpace;

namespace Terminal.Client.Components
{
  public partial class DealsComponent
  {
    /// <summary>
    /// Table records
    /// </summary>
    protected virtual IList<ActiveOrderRecord> Items { get; set; } = new List<ActiveOrderRecord>();

    /// <summary>
    /// Update table records 
    /// </summary>
    /// <param name="items"></param>
    public virtual Task UpdateItems(IIndexCollection<ITransactionPositionModel> items)
    {
      Items = items.Select(o => new ActiveOrderRecord
      {
        Time = o.Time,
        Name = o.Instrument.Name,
        Side = o.Side ?? OrderSideEnum.None,
        Size = o.Volume ?? 0,
        OpenPrice = o.OpenPrice ?? 0,
        ClosePrice = o.ClosePrice ?? 0,
        Gain = o.GainLoss ?? 0

      }).ToList();

      return Render(() => { });
    }
  }
}
