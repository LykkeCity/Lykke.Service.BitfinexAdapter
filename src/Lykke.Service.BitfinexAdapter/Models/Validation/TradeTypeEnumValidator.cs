using FluentValidation.Validators;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using System;

namespace Lykke.Service.BitfinexAdapter.Models.Validation
{
    public class TradeTypeEnumValidator : PropertyValidator
    {
        public TradeTypeEnumValidator() : base("Invalid update indicator") { }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (!Enum.TryParse<TradeType>(context.PropertyValue?.ToString() ?? "", true, out var side))
            {
                return false;
            }
            return true;
        }
    }
}
