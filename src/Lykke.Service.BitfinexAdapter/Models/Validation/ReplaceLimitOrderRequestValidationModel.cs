using FluentValidation;
using Lykke.Service.BitfinexAdapter.Models.LimitOrders;

namespace Lykke.Service.BitfinexAdapter.Models.Validation
{
    public class ReplaceLimitOrderRequestValidationModel : AbstractValidator<ReplaceLimitOrderRequest>
    {
        public ReplaceLimitOrderRequestValidationModel()
        {
            RuleFor(reg => reg.TradeType).SetValidator(new TradeTypeEnumValidator()).WithMessage(s => $"Unrecognized tradeType {s.TradeType}");
            RuleFor(reg => reg.OrderIdToCancel).GreaterThan(0).WithMessage(s => "OrderId to replace not specified.");
        }
    }
}
