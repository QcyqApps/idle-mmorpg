namespace IdleMmo.Domain.Enums;

public enum AuditAction
{
    Unknown = 0,
    AccountCreated = 1,
    LoginSuccess = 2,
    LoginFailure = 3,
    RefreshTokenRotated = 4,
    RefreshTokenReuseDetected = 5,
    BattleStarted = 100,
    BattleFinalized = 101,
    BattleReplayMismatch = 102,
    RareDrop = 200,
    RefiningSuccess = 201,
    RefiningFailure = 202,
    PvpWin = 300,
    CurrencyAdjustedManually = 400,
    AccountLinked = 500,
}
