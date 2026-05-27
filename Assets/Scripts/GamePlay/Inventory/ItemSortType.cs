namespace GamePlay.Inventory
{
    public enum ItemSortType
    {
        Name, //默认拾取顺序(按名称)
        Quantity, //按数量
        Rarity, //按稀有度
        Type, //按类型
    }

    public enum SortOrder
    {
        Ascending, //升序
        Descending //降序
    }
}