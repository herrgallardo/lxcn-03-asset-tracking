using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AssetTracker
{
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

    // Abstract base class for all assets
    public abstract class Asset
    {
        public string SerialNumber { get; set; }
        public Price PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }

        // Asset type property to be implemented by derived classes
        public abstract string AssetType { get; }

        // Constructor
        protected Asset(string serialNumber, Price purchasePrice, DateTime purchaseDate, string brand, string model)
        {
            SerialNumber = serialNumber;
            PurchasePrice = purchasePrice;
            PurchaseDate = purchaseDate;
            Brand = brand;
            Model = model;
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
            return $"{AssetType,-12} | {SerialNumber,-18} | {Brand,-12} | {Model,-18} | {PurchaseDate.ToShortDateString(),-15} | {PurchasePrice,-10}";
        }
    }

    // Computer class that inherits from Asset
    public class Computer : Asset
    {
        // Override of the AssetType property from the base class
        public override string AssetType => "Computer";

        // Constructor that passes parameters to the base class constructor
        public Computer(string serialNumber, Price purchasePrice, DateTime purchaseDate, string brand, string model)
            : base(serialNumber, purchasePrice, purchaseDate, brand, model)
        {
        }
    }

    // Phone class that inherits from Asset
    public class Phone : Asset
    {
        // Override of the AssetType property from the base class
        public override string AssetType => "Phone";

        // Constructor that passes parameters to the base class constructor
        public Phone(string serialNumber, Price purchasePrice, DateTime purchaseDate, string brand, string model)
            : base(serialNumber, purchasePrice, purchaseDate, brand, model)
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
                return new Computer(SerialNumber, price, PurchaseDate, Brand, Model);
            }
            else if (Type == "Phone")
            {
                return new Phone(SerialNumber, price, PurchaseDate, Brand, Model);
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

                    if (assetCollection != null && assetCollection.Assets != null)
                    {
                        foreach (var assetXml in assetCollection.Assets)
                        {
                            try
                            {
                                Asset asset = assetXml.ToAsset();
                                AddAsset(asset);
                            }
                            catch
                            {
                                // Silently continue if an asset fails to load
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently handle any errors during loading
            }
        }

        // Display assets sorted by class (computers first, then phones) and then by purchase date
        public void DisplayAssets()
        {
            if (_assets.Count == 0)
            {
                Console.WriteLine("\nNo assets found in inventory.");
                return;
            }

            Console.WriteLine("\nAsset Inventory:");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"{"Type",-12} | {"Serial Number",-18} | {"Brand",-12} | {"Model",-18} | {"Purchase Date",-15} | {"Price",-10}");
            Console.WriteLine(new string('-', 100));

            // Sort assets by type and then by purchase date
            var sortedAssets = _assets
                .OrderBy(a => a.AssetType)
                .ThenBy(a => a.PurchaseDate)
                .ToList();

            string previousType = "";
            int assetsNearEndOfLife = 0;

            foreach (var asset in sortedAssets)
            {
                // Add extra spacing between different asset types
                if (previousType != "" && previousType != asset.AssetType)
                {
                    Console.WriteLine();
                }
                previousType = asset.AssetType;

                // Check if the asset is nearing end of life (less than 3 months away from 3 years)
                TimeSpan timeUntilEndOfLife = asset.TimeUntilEndOfLife();

                // If the asset is less than 3 months away from being 3 years old, display in red
                if (timeUntilEndOfLife.TotalDays < 90 && timeUntilEndOfLife.TotalDays > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(asset);
                    Console.ResetColor();
                    assetsNearEndOfLife++;
                }
                else
                {
                    Console.WriteLine(asset);
                }

                // Add a separator line between items
                Console.WriteLine(new string('-', 100));
            }

            // Display summary information
            Console.WriteLine($"Total assets: {_assets.Count}");
            Console.WriteLine($"Assets nearing end of life (< 3 months): {assetsNearEndOfLife}");
            Console.WriteLine($"Computers: {_assets.Count(a => a.AssetType == "Computer")}");
            Console.WriteLine($"Phones: {_assets.Count(a => a.AssetType == "Phone")}");
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