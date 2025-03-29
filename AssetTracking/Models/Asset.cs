using System;

namespace AssetTracker.Models
{
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
}