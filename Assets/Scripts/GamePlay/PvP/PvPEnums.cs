namespace GamePlay.PvP
{
    /// <summary>
    /// PvP生命周期状态
    /// </summary>
    public enum BattleState : byte
    {
        Waiting, // 等待玩家加入
        Loading, // 加载场景中
        Fighting // 战斗中
    }

    /// <summary>
    /// 回合结果
    /// </summary>
    public enum RoundResult : byte
    {
        None,
        Player1Win,
        Player2Win
    }
    
    /// <summary>
    /// 回合状态
    /// </summary>
    public enum RoundSubState : byte
    {
        Countdown,
        Active,
        RoundEnd,
        MatchEnd
    }
}
