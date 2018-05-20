# TlvLib
C# .NET library for reading and writing TLV (Tag Length Value) encoded data.

## Standards
This library follows the encoding rules defined in the following standards:
* [X.690 ASN.1 encoding rules](https://www.itu.int/rec/dologin_pub.asp?lang=e&id=T-REC-X.690-201508-I!!PDF-E&type=items) (ISO/IEC 8825–1):
  * Basic Encoding Rules (BER)
  * Canonical Encoding Rules (CER)
  * Distinguished Encoding Rules (DER)
* EMV 4.2 Book 3, Annex B: Rules for BER-TLV Data Objects

## Examples in C#

To read all TLV encoded data from a stream/byte array:
```c#
TlvEncoding.ProcessTlvStream(stream, 
    (tag, data) => {
        Console.WriteLine($"Tag:{tag} Data:{BitConverter.ToString(data)}");
    });
```

To write a tag-length-value entry to a stream:
```c#
WriteTlv(stream, tag, tagData);
```

Low level read access:
```c#
var tag = TlvEncoding.ReadNextTag(stream);
var length = TlvEncoding.ReadLength(stream);
```

Low level write access:
```c#
TlvEncoding.WriteTag(stream, tag);
TlvEncoding.WriteLength(stream, length);
```

## NuGet Package
Available from NuGet:
https://www.nuget.org/packages/TlvLib
