namespace Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions
{
    public enum ApiErrorCode
    {
        Unknown = 101,
        InternalServerError,
        InputParamValidationError,
        OrderNotFound,
        VolumeTooSmall,
        NotEnoughBalance,
        IncorrectPrice,
        IncorrectOrderType,
        IncorrectTradetype,
        IncorrectInstrument,
        IncorrectAmount,
        Unauthorized,
        RateLimit
    }
}
