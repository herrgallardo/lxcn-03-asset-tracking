using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AssetTracker.Models.XmlClasses
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
}