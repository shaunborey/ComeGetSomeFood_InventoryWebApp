using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGSFEntityLib
{
    public interface IOrderableItem
    {
        int ItemID { get; }
        string SKU { get; set; }
        string Brand { get; set; }
        string Description { get; set; }
        int CurrentQty { get; set; }
        int OnOrderQty { get; set; }
        int OrderThreshold { get; set; }
        double UnitCost { get; set; }
        bool IsSelected { get; set; }

        bool UpdateCurrentQty(int qtyChange);
    }
}
