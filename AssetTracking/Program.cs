using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AssetTracker
{
    // The following classes are named to match the European Central Bank's XML structure
    // The ECB uses "Envelope" and nested "Cube" elements in their exchange rate API
    // This structure follows their exact XML format which looks like:
    // <Envelope>
    //   <Cube>
    //     <Cube time="2025-03-29">
    //       <Cube currency="USD" rate="1.0812"/>
    //       <Cube currency="SEK" rate="11.235"/>
    //       <!-- more currencies -->
    //     </Cube>
    //   </Cube>
    // </Envelope>

    // Innermost Cube represents a single currency's exchange rate
    public class Cube
    {
        [XmlAttribute("currency")]
        public required string currency { get; set; }

        [XmlAttribute("rate")]
        public decimal rate { get; set; }
    }

    // Middle Cube contains the date and all currency rates
    public class Cube1
    {
        [XmlAttribute("time")]
        public required string time { get; set; }

        [XmlElement("Cube")]
        public required List<Cube> Cube { get; set; }
    }

    // Outer Cube is a container
    public class Cube2
    {
        [XmlElement("Cube")]
        public required Cube1 Cube1 { get; set; }
    }

    // Root element of the ECB exchange rate XML document
    public class Envelope
    {
        [XmlElement("Cube")]
        public required Cube2 Cube { get; set; }
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

    // Enum for asset currency types
    public enum Currency
    {
        USD,  // US Dollar
        EUR,  // Euro
        SEK   // Swedish Krona
    }

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

    // Abstract base class for all assets
    public abstract class Asset
    {
        public string SerialNumber { get; set; }
        public Price PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string OfficeLocation { get; set; }

        // Asset type property to be implemented by derived classes
        public abstract string AssetType { get; }

        // Constructor
        protected Asset(string serialNumber, Price purchasePrice, DateTime purchaseDate, string brand, string model, string officeLocation)
        {
            SerialNumber = serialNumber;
            PurchasePrice = purchasePrice;
            PurchaseDate = purchaseDate;
            Brand = brand;
            Model = model;
            OfficeLocation = officeLocation;
        }

        // Method to check if asset is near end of life (3 years)
        public TimeSpan TimeUntilEndOfLife()
        {
            // End of life is 3 years from purchase date
            DateTime endOfLifeDate = PurchaseDate.AddYears(3);
            return endOfLifeDate - DateTime.Now;
        }

        // ToString method for displaying asset information
        public override string ToString()
        {
            decimal usdValue = PurchasePrice.ToUSD();
            return $"{AssetType,-12} | {SerialNumber,-18} | {Brand,-12} | {Model,-18} | {OfficeLocation,-10} | {PurchaseDate.ToShortDateString(),-15} | {PurchasePrice,-15} | ${usdValue,-13:N2}";
        }
    }

    // Computer class that inherits from Asset
    public class Computer : Asset
    {
        // Override of the AssetType property from the base class
        public override string AssetType => "Computer";

        // Constructor that passes parameters to the base class constructor
        public Computer(string serialNumber, Price purchasePrice, DateTime purchaseDate, string brand, string model, string officeLocation)
            : base(serialNumber, purchasePrice, purchaseDate, brand, model, officeLocation)
        {
        }
    }

    // Phone class that inherits from Asset
    public class Phone : Asset
    {
        // Override of the AssetType property from the base class
        public override string AssetType => "Phone";

        // Constructor that passes parameters to the base class constructor
        public Phone(string serialNumber, Price purchasePrice, DateTime purchaseDate, string brand, string model, string officeLocation)
            : base(serialNumber, purchasePrice, purchaseDate, brand, model, officeLocation)
        {
        }
    }

    // XML serialization classes
    [XmlRoot("Assets")]
    public class AssetCollection
    {
        [XmlElement("Asset")]
        public required List<AssetXml> Assets { get; set; }
    }

    public class AssetXml
    {
        [XmlAttribute("Type")]
        public required string Type { get; set; }

        [XmlElement("SerialNumber")]
        public required string SerialNumber { get; set; }

        [XmlElement("Brand")]
        public required string Brand { get; set; }

        [XmlElement("Model")]
        public required string Model { get; set; }

        [XmlElement("PurchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [XmlElement("Price")]
        public decimal Price { get; set; }

        [XmlElement("Currency")]
        public required string Currency { get; set; }

        [XmlElement("OfficeLocation")]
        public required string OfficeLocation { get; set; }

        // Convert from XML representation to Asset object
        public Asset ToAsset()
        {
            // Parse the currency string to the Currency enum
            Currency currency;
            if (!Enum.TryParse(Currency, out currency))
            {
                currency = AssetTracker.Currency.USD; // Default to USD if parsing fails
            }

            Price price = new Price(Price, currency);

            // Create the appropriate asset type based on the Type property
            if (Type == "Computer")
            {
                return new Computer(SerialNumber, price, PurchaseDate, Brand, Model, OfficeLocation);
            }
            else if (Type == "Phone")
            {
                return new Phone(SerialNumber, price, PurchaseDate, Brand, Model, OfficeLocation);
            }
            else
            {
                throw new ArgumentException($"Unknown asset type: {Type}");
            }
        }
    }

    // AssetManager class to manage the collection of assets
    public class AssetManager
    {
        // List to store all assets
        private List<Asset> _assets = new List<Asset>();

        // Add an asset to the list
        public void AddAsset(Asset asset)
        {
            _assets.Add(asset);
        }

        // Load assets from an XML file
        public void LoadAssetsFromXml(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AssetCollection));

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    AssetCollection? assetCollection = serializer.Deserialize(fs) as AssetCollection;
                    if (assetCollection == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize the asset collection.");
                    }
                    if (assetCollection == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize the asset collection.");
                    }

                    if (assetCollection.Assets != null)
                    {
                        foreach (var assetXml in assetCollection.Assets)
                        {
                            try
                            {
                                Asset asset = assetXml.ToAsset();
                                AddAsset(asset);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading asset: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assets: {ex.Message}");
            }
        }

        // Display assets sorted by office location, then by purchase date
        public void DisplayAssets()
        {
            if (_assets.Count == 0)
            {
                Console.WriteLine("\nNo assets found in inventory.");
                return;
            }

            Console.WriteLine("\nAsset Inventory:");
            Console.WriteLine(new string('-', 130));
            Console.WriteLine($"{"Type",-12} | {"Serial Number",-18} | {"Brand",-12} | {"Model",-18} | {"Office",-10} | {"Purchase Date",-15} | {"Local Price",-15} | {"USD Value",-15}");
            Console.WriteLine(new string('-', 130));

            // Sort assets by office location, then by purchase date
            var sortedAssets = _assets
                .OrderBy(a => a.OfficeLocation)
                .ThenBy(a => a.PurchaseDate)
                .ToList();

            string previousOffice = "";
            int assetsNearEndOfLife3Months = 0;
            int assetsNearEndOfLife6Months = 0;

            foreach (var asset in sortedAssets)
            {
                // Add extra spacing between different office locations
                if (previousOffice != "" && previousOffice != asset.OfficeLocation)
                {
                    Console.WriteLine();
                }
                previousOffice = asset.OfficeLocation;

                // Check if the asset is nearing end of life
                TimeSpan timeUntilEndOfLife = asset.TimeUntilEndOfLife();

                // If the asset is less than 3 months away from being 3 years old, display in red
                if (timeUntilEndOfLife.TotalDays < 90 && timeUntilEndOfLife.TotalDays > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(asset);
                    Console.ResetColor();
                    assetsNearEndOfLife3Months++;
                }
                // If the asset is less than 6 months away from being 3 years old, display in yellow
                else if (timeUntilEndOfLife.TotalDays < 180 && timeUntilEndOfLife.TotalDays > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(asset);
                    Console.ResetColor();
                    assetsNearEndOfLife6Months++;
                }
                else
                {
                    Console.WriteLine(asset);
                }

                // Add a separator line between items
                Console.WriteLine(new string('-', 130));
            }

            // Display summary information
            Console.WriteLine($"Total assets: {_assets.Count}");
            Console.WriteLine($"Assets nearing end of life (< 3 months): {assetsNearEndOfLife3Months}");
            Console.WriteLine($"Assets nearing end of life (3-6 months): {assetsNearEndOfLife6Months}");
            Console.WriteLine($"Computers: {_assets.Count(a => a.AssetType == "Computer")}");
            Console.WriteLine($"Phones: {_assets.Count(a => a.AssetType == "Phone")}");

            // Office statistics
            var officeGroups = _assets.GroupBy(a => a.OfficeLocation)
                                     .Select(g => new { Office = g.Key, Count = g.Count() });

            Console.WriteLine("\nAssets by Office:");
            foreach (var group in officeGroups.OrderBy(g => g.Office))
            {
                Console.WriteLine($"{group.Office}: {group.Count}");
            }
        }
    }

    // Main program class
    class Program
    {
        private const string XML_FILE_PATH = "data/assets.xml";

        static void Main(string[] args)
        {
            Console.WriteLine("Asset Tracking System");
            Console.WriteLine("=====================\n");

            try
            {
                Console.WriteLine("Initializing currency converter...");

                // Initialize currency converter with error suppression
                CurrencyConverter.Update(true);

                // Now check if we have valid rates
                if (CurrencyConverter.HasValidRates())
                {
                    Console.WriteLine("Currency rates updated successfully.");
                }
                else
                {
                    Console.WriteLine("Warning: Using fallback currency rates.");
                    Console.WriteLine("Using approximate conversion rates:");
                    Console.WriteLine("1 EUR = 1.10 USD");
                    Console.WriteLine("1 EUR = 10.50 SEK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not update currency rates: {ex.Message}");
                Console.WriteLine("Will use default conversion rates.");
            }

            // Create an instance of the AssetManager
            AssetManager tracker = new AssetManager();

            // Load assets from the XML file
            tracker.LoadAssetsFromXml(XML_FILE_PATH);

            // Display the assets
            tracker.DisplayAssets();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}