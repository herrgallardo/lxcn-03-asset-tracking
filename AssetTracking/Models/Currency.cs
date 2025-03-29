using System;

namespace AssetTracker.Models
{
    // Enum for asset currency types
    public enum Currency
    {
        USD,  // US Dollar
        EUR,  // Euro
        SEK   // Swedish Krona
    }

    // Class from LiveCurrency.cs for currency objects
    public class CurrencyObj
    {
        public string CurrencyCode { get; set; }
        public double ExchangeRateFromEUR { get; set; }

        public CurrencyObj(string currencyCode, double rate)
        {
            CurrencyCode = currencyCode;
            ExchangeRateFromEUR = rate;
        }
    }
}