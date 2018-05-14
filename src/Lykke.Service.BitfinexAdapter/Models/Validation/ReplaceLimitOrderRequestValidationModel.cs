using FluentValidation;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitfinexAdapter.Models.Validation
{
    public class ReplaceLimitOrderRequestValidationModel : AbstractValidator<ReplaceLimitOrderRequest>
    {
        public ReplaceLimitOrderRequestValidationModel()
        {
            RuleFor(reg => reg.TradeType).SetValidator(new TradeTypeEnumValidator()).WithMessage(s => $"Unrecognized tradeType {s.TradeType}");
        }
    }
}
