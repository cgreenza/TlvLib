using System;
using System.IO;

namespace Greensoft.TlvLib
{
    // X.690 Basic Encoding Rules (Type/Tag Length Value) i.e. ISO/IEC 8825–1
    // Also EMV 4.2 Book 3, Annex B: Rules for BER-TLV Data Objects
    // https://www.itu.int/rec/dologin_pub.asp?lang=e&id=T-REC-X.690-201508-I!!PDF-E&type=items
    // https://www.itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf

    public static class TlvEncoding
    {
        /// <summary>
        /// Read next TLV tag/identifier from stream
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <param name="removeEmvPadding">true to remove 0x00 and 0xFF padding bytes that may occur before tag according to EMV spec</param>
        /// <returns>tag number or null if end of stream is reached before tag is found</returns>
        public static uint? ReadNextTag(Stream stream, bool removeEmvPadding = false)
        {
            uint tagValue = 0;
            int byteCount = 0;

            while (true)
            {
                if (byteCount >= 4)
                    throw new TlvException(string.Format("Tag is more than 4 bytes long. Partial tag value: 0x{0:x8}", tagValue));

                var thisByte = stream.ReadByte();
                if (thisByte == -1)
                {
                    if (byteCount > 0)
                        throw new TlvException(string.Format("Unexpected end of tag data. Partial tag value: 0x{0:x8}", tagValue));

                    return null; // end of stream, no tag found
                }

                if (removeEmvPadding && byteCount == 0 && (thisByte == 0x00 || thisByte == 0xff)) // todo: check 0xff usage!!!
                    continue; // skip if not already reading tag (EMV allows 0x00 or 0xff padding between TLV entries)

                byteCount++;
                tagValue <<= 8;
                tagValue |= (uint)thisByte;

                if (byteCount == 1 && (thisByte & 0x1f) != 0x1f)
                    return tagValue; // no more data (tag number is 0 to 30 inclusive, so only one octet used)

                if (byteCount != 1 && (thisByte & 0x80) == 0)
                    return tagValue; // no more data (bit 8 is not set for last octet)
            }
        }

        /// <summary>
        /// Read TLV length from stream
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <returns>length or null to indicate indefinite length</returns>
        public static int? ReadLength(Stream stream)
        {
            var readByte = stream.ReadByte();
            if (readByte == -1)
                throw new TlvException("Unexpected end of stream while reading length");

            if ((readByte & 0x80) == 0)
                return (int)readByte; // length is in first byte

            int length = 0;
            var lengthBytes = readByte & 0x7f; // remove 0x80 bit

            if (lengthBytes == 0)
                return null; // indefinite form

            if (lengthBytes > 4)
                throw new TlvException($"Unsupported length: {lengthBytes} bytes");

            for (var i = 0; i < lengthBytes; i++)
            {
                readByte = stream.ReadByte();
                if (readByte == -1)
                    throw new TlvException("Unexpected end of stream while reading length");

                length <<= 8;
                length |= (int)readByte;
            }

            return length;
        }

        /// <summary>
        /// Write TLV tag/identifier to stream
        /// </summary>
        /// <param name="stream">stream to write to</param>
        /// <param name="tag">tag value to write</param>
        public static void WriteTag(Stream stream, uint tag)
        {
            var firstByte = true;
            for (int i = 3; i >= 0; i--) // shift out the bytes
            {
                byte thisByte = (byte)(tag >> (8 * i));

                if (thisByte == 0 && firstByte && i > 0)
                    continue; // don't write leading 00 bytes unless it's the only/last byte

                if (firstByte)
                {
                    if (i == 0) // last byte to write
                    {
                        if ((thisByte & 0x1f) == 0x1f)
                            throw new TlvException("Invalid tag value: first octet indicates subsequent octets, but no subsequent octets found");
                    }
                    else
                    {
                        // more bytes to write
                        if ((thisByte & 0x1f) != 0x1f)
                            throw new TlvException("Invalid tag value: first octet indicates no subsequent octets, but subsequent octets found");
                    }                    
                }
                else
                {
                    if (i == 0) // last byte to write
                    {
                        if ((thisByte & 0x80) == 0x80)
                            throw new TlvException("Invalid tag value: last octet indicates subsequent octets");
                    }
                    else
                    {
                        if ((thisByte & 0x80) != 0x80)
                            throw new TlvException("Invalid tag value: non-last octet indicates no subsequent octets");
                    }
                }

                stream.WriteByte(thisByte);
                firstByte = false;
            }
        }

        /// <summary>
        /// Write TLV length to stream
        /// </summary>
        /// <param name="stream">stream to write to</param>
        /// <param name="length">length to write or null to write indefinite length</param>
        public static void WriteLength(Stream stream, int? length)
        {
            if (length == null)
            {
                stream.WriteByte(0x80); // indefinite form
                return;
            }

            if (length < 0 || length > 0xffffffff)
                throw new TlvException(string.Format("Invalid length value: {0}", length));

            if (length <= 0x7f) // use short form if possible
            {
                stream.WriteByte(checked((byte)length));
                return;
            }

            byte lengthBytes;

            // use minimum number of octets
            if (length <= 0xff)
                lengthBytes = 1;
            else if (length <= 0xffff)
                lengthBytes = 2;
            else if (length <= 0xffffff)
                lengthBytes = 3;
            else if (length <= 0xffffffff)
                lengthBytes = 4;
            else
                throw new TlvException(string.Format("Length value too big: {0}", length));

            stream.WriteByte((byte)(lengthBytes | 0x80));

            // shift out the bytes
            for (var i = lengthBytes - 1; i >= 0; i--)
            {
                var data = (byte)(length >> (8 * i));
                stream.WriteByte(data);
            }
        }

        public static void WriteTlv(Stream stream, uint tag, params byte[] value)
        {
            WriteTag(stream, tag);

            int length = value?.Length ?? 0;
            WriteLength(stream, length);

            if (value == null)
                return;

            stream.Write(value, 0, length);
        }

        public static void ProcessTlvStream(Stream stream, Action<uint, byte[]> processTag, bool removeEmvPadding = false)
        {
            while (true)
            {
                var tag = ReadNextTag(stream, removeEmvPadding);
                if (tag == null)
                    return;

                var length = ReadLength(stream);
                if (length == 0)
                    continue;
                if (length == null)
                    throw new TlvException("Indefinite length not supported");

                var buf = new byte[length.Value];
                var readIndex = 0;
                while (true)
                {
                    var lengthRead = stream.Read(buf, readIndex, length.Value - readIndex);

                    if (lengthRead == 0)
                        throw new TlvException("Unexpected end of stream while reading data");

                    readIndex += lengthRead;

                    if (length == readIndex)
                        break; // all data read
                }

                processTag(tag.Value, buf);
            }
        }
    }
}
