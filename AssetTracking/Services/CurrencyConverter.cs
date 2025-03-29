using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AssetTracker.Models.XmlClasses;

namespace AssetTracker.Services
{
    // Class to handle currency conversion
    static class CurrencyConverter
    {
        static private string xmlUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
        static Envelope? envelope = null; // Initialize as null, don't call Update() here

        // Check if we have valid exchange rates
        static public bool HasValidRates()
        {
            return envelope?.Cube?.Cube1?.Cube != null &&
                   envelope.Cube.Cube1.Cube.Count > 0 &&
                   envelope.Cube.Cube1.Cube.Any(c => c.currency == "USD") &&
                   envelope.Cube.Cube1.Cube.Any(c => c.currency == "SEK");
        }

        // Method to convert between any two currencies via EUR
        static public bool ConvertTo(decimal value, string fromCurrency, string toCurrency, out decimal result)
        {
            result = -1;

            // Initialize envelope if it's null
            if (envelope == null)
            {
                envelope = Update();
            }

            // If input and output currencies are the same
            if (fromCurrency == toCurrency)
            {
                result = value;
                return true;
            }

            // Convert from source currency to EUR first
            decimal valueInEur = value;

            // If source is not EUR, convert to EUR
            if (fromCurrency != "EUR")
            {
                bool foundFromRate = false;
                foreach (var cube in envelope.Cube.Cube1.Cube)
                {
                    if (cube.currency == fromCurrency)
                    {
                        valueInEur = value / cube.rate;
                        foundFromRate = true;
                        break;
                    }
                }

                if (!foundFromRate)
                    return false;
            }

            // If target is EUR, we're done
            if (toCurrency == "EUR")
            {
                result = valueInEur;
                return true;
            }

            // Convert from EUR to target currency
            foreach (var cube in envelope.Cube.Cube1.Cube)
            {
                if (cube.currency == toCurrency)
                {
                    result = valueInEur * cube.rate;
                    return true;
                }
            }

            return false;
        }

        static public Envelope Update(bool suppressErrors = false)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Envelope));

                // Use XmlReaderSettings to handle DTD (Document Type Definition) issues
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreWhitespace = true
                };

                XmlReader xmlReader = XmlReader.Create(xmlUrl, settings);

                using (xmlReader)
                {
                    var deserializedEnvelope = serializer.Deserialize(xmlReader) as Envelope;
                    if (deserializedEnvelope == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize the XML into an Envelope object.");
                    }
                    envelope = deserializedEnvelope;
                }

                return envelope;
            }
            catch (Exception ex)
            {
                if (!suppressErrors)
                {
                    Console.WriteLine($"Error updating currency rates: {ex.Message}");
                }

                // Create a basic envelope with fallback rates if the update fails
                var fallbackEnvelope = new Envelope
                {
                    Cube = new Cube2
                    {
                        Cube1 = new Cube1
                        {
                            time = DateTime.Now.ToString("yyyy-MM-dd"),
                            Cube = new List<Cube>
                            {
                                new Cube { currency = "USD", rate = 1.1m },
                                new Cube { currency = "SEK", rate = 10.5m }
                            }
                        }
                    }
                };

                envelope = fallbackEnvelope;
                return fallbackEnvelope;
            }
        }
    }
}