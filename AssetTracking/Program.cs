using System;
using AssetTracker.Services;

namespace AssetTracker
{
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