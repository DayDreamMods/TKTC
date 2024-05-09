using CessilCellsCeaChells.CeaChore;
using TKTC;

[assembly: RequiresEnumInsertion(typeof(ShopItemCategory), Requires.ShopItemCategory_TKTC_Name)]

namespace TKTC;

public static class Requires {
    public const string ShopItemCategory_TKTC_Name = "TeamKillTotalConversion";
}