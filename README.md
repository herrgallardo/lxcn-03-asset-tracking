# Asset Tracking Application

## Course Context

- **Program**: Arbetsmarknadsutbildning - IT Påbyggnad/Programmerare
- **Course**: C# .NET Fullstack System Developer
- **Minitask**: Weekly Assignment #3 - Asset Tracking Application

## Learning Objectives

This console application demonstrates advanced C# programming concepts:

- Object-Oriented Programming with inheritance and polymorphism
- Console Application Development with color-coded output
- XML Data Processing
- Currency Conversion
- Date-based Asset Management
- Multi-office inventory tracking

## Overview

A console-based asset management system built to help companies track their electronic assets such as computers and phones across multiple office locations. The application handles different currencies, calculates asset end-of-life timing, and provides visual indicators for assets approaching replacement dates.

## Features

- Track multiple types of assets (Computers, Phones)
- Sort assets by office location and purchase date
- Color-coded alerts for assets nearing end-of-life
- Multi-currency support with automatic conversion
- Load asset data from XML files
- Display comprehensive inventory reports

## Key Components

### Classes

- `Asset`: Abstract base class for all trackable items
- `Computer`, `Phone`: Asset subclasses for specific device types
- `Price`: Handles multi-currency support
- `AssetManager`: Asset collection management and reporting
- `CurrencyConverter`: Handles exchange rate updates and conversions

## Technical Skills Demonstrated

- C# inheritance and polymorphism
- XML serialization/deserialization
- Web service integration (ECB currency rates)
- LINQ queries for data manipulation
- Color-coded console output
- Date calculations and comparisons

## Application Flow

1. Initialize currency converter
2. Load assets from XML file
3. Display sorted assets with:
   - Office location grouping
   - Purchase date sorting within groups
   - Color highlighting for assets approaching end-of-life
4. Display summary statistics

## Requirements

- .NET SDK 9.0 or later
- Internet connection (for currency rate updates)
- XML asset data file

## How to Run

```bash
dotnet build
dotnet run
```

## Implementation Details

### Asset End-of-Life Indicators

- **RED**: Less than 3 months until 3-year end-of-life date
- **YELLOW**: Less than 6 months until 3-year end-of-life date

### Office Locations

- USA (USD)
- Sweden (SEK)
- Germany (EUR)

### Supported Currency Conversion

The application uses real-time currency rates from the European Central Bank with fallback rates:

- USD to EUR conversion
- SEK to EUR conversion
- USD to SEK conversion

## Learning Notes

This application builds on previous console application skills while adding complexity through inheritance, currency management, and more sophisticated data handling. The color-coded visualization provides an intuitive way to identify assets requiring attention.

## Educational Program Details

- **Type**: Arbetsmarknadsutbildning (Labor Market Training)
- **Focus**: IT Påbyggnad/Programmerare (IT Advanced/Programmer)
- **Course**: C# .NET Fullstack System Developer

## Potential Improvements for Future Learning

- Implement user input for adding new assets
- Store assets in a database instead of XML
- Create asset modification functionality
- Add depreciation calculation
- Generate reports exportable to PDF or Excel
- Implement user authentication and permission levels
