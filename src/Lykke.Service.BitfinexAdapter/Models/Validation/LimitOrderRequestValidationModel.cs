using FluentValidation;
using Lykke.Service.BitfinexAdapter.Models.LimitOrders;

namespace Lykke.Service.BitfinexAdapter.Models.Validation
{
    public class LimitOrderRequestValidationModel : AbstractValidator<LimitOrderRequest>
    {
        public LimitOrderRequestValidationModel()
        {
            RuleFor(reg => reg.TradeType).SetValidator(new TradeTypeEnumValidator()).WithMessage(s=>$"Unrecognized tradeType {s.TradeType}");
        }
    }

    
}
