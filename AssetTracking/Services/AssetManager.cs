using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AssetTracker.Models;
using AssetTracker.Models.XmlClasses;

namespace AssetTracker.Services
{
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
}