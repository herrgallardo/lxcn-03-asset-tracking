using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AssetTracker.Models.XmlClasses
{
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
                currency = Models.Currency.USD; // Default to USD if parsing fails
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
}