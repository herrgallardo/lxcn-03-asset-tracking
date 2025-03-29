using System;
using AssetTracker.Services;

namespace AssetTracker.Models
{
    // Price class to handle currency and value
    public class Price
    {
        public decimal Value { get; set; }
        public Currency Currency { get; set; }

        public Price(decimal value, Currency currency)
        {
            Value = value;
            Currency = currency;
        }

        // Convert to USD using CurrencyConverter
        public decimal ToUSD()
        {
            if (Currency == Currency.USD)
                return Value;

            decimal convertedValue;
            string fromCurrencyStr = Currency.ToString();

            if (CurrencyConverter.ConvertTo(Value, fromCurrencyStr, "USD", out convertedValue))
                return convertedValue;

            // Fallback to approximate rates if conversion fails
            decimal fallbackRate = Currency switch
            {
                Currency.EUR => 1.1m, // Approximate EUR to USD rate
                Currency.SEK => 0.095m, // Approximate SEK to USD rate
                _ => 1.0m
            };

            return Value * fallbackRate;
        }

        public override string ToString()
        {
            string currencySymbol = Currency switch
            {
                Currency.USD => "$",
                Currency.EUR => "€",
                Currency.SEK => "kr",
                _ => ""
            };

            return $"{currencySymbol}{Value:N2}";
        }
    }
}