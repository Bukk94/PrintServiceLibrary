# Print service library (PSL)

[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)
![version](https://img.shields.io/badge/version-1.0-blue) 
[![Build Status](https://travis-ci.com/Bukk94/PrintServiceLibrary.svg?token=XTeWt6KEyExzbH1iNFWD&branch=master)](https://travis-ci.com/Bukk94/PrintServiceLibrary)

This library servers as an interface into printer's communication. 
The user can communicate with various connection interfaces using a single API interface.

API can communicate using:
- USB port
- LPT (Parallel port)
- COM (Serial port)
- Network connection (TCP/IP)

PSL was mainly developed for Zebra printers using the ZPL language. 
But basic communication interface should work with any kind of printer and language as long as you follow standard printer's structure and language.

### ZPL dependant methods

These methods are only for printers that support the ZPL language.
- `UploadFontToPrinter`
- `ListPrinterMemory`
- `GetPrinterFreeMemory`
- `PreviewZPLPrint`

# Code Examples

## Printing using the USB port

```csharp
string zplCommands = "^XA^FO100,153^FDTest Text to Print^FS^XZ";
IPrintService printService = new PrintService();

// Sends zplCommands to printer called "my printer" 
// using the USB port. Printer will print 5 copies.
printService.PrintUSB(zplCommands, "my printer", 5);
```

## Printing using TCP/IP

```csharp
string zplCommands = "^XA^FO100,153^FDTest Text to Print^FS^XZ";
IPrintService printService = new PrintService();

// Sends zplCommands to printer on IP 192.168.1.1:9100
printService.PrintNetwork(zplCommands, System.Net.IPAddress.Parse("192.168.1.1"), 9100);
```

## Getting list of local printers
```csharp
IPrintService printService = new PrintService();

// Loads all locally installed printers and return list of their names
printService.LoadInstalledPrinters();
```

# Neodynamic integration

PSL API has [Neodynamic ThermalLabel SDK](https://www.neodynamic.com/products/printing/thermal-label/sdk-vb-net-csharp/download/) 
integration. If you do have Neodynamic license key, you can pass it in 
`PrintService` constructor and call methods like `PreviewZPLPrint`.

These methods further enhance Neodynamic ZPL code generation from raw XML template by locally 
previewing ZPL commands that will get sent to the printer or by local data binding without needing communication with the printer.

You can use these commands regardless of Neodynamic SDK license but beware that the "TRIAL" logo will be embedded in the ZPL commands.

# Lightweight version

There is also a Lightweight version in a separate branch that doesn't contain Neodynamic ThermalLabel SDK.
This removes all necessary libraries (which are quite large) and only provides printer communication.

# Languages

PSL API is multilingual. Supported languages are English and Czech.

### Licensing

PSL API is licensed under the MIT license (more in [LICENSE.MD](https://github.com/Bukk94/PrintServiceLibrary/blob/master/LICENSE)).