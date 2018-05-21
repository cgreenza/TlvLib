using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Greensoft.TlvLib;
using System.IO;
using System.Collections.Generic;

namespace TlvLibTest
{
    [TestClass]
    public class TlvEncodingTest
    {
        [DataTestMethod]
        // end of stream
        [DataRow(new byte[] { }, null, 0, DisplayName = "(no data)")]
        // 1 byte tag:
        [DataRow(new byte[] { 0x00, 0x01 }, 0x00u, 1, DisplayName = "0000 0000")]
        [DataRow(new byte[] { 0x01, 0x01 }, 0x01u, 1, DisplayName = "0000 0001")]
        [DataRow(new byte[] { 0x80, 0x01 }, 0x80u, 1, DisplayName = "1000 0000")]
        [DataRow(new byte[] { 0xE0, 0x01 }, 0xE0u, 1, DisplayName = "1110 0000")]
        [DataRow(new byte[] { 0x1E, 0x01 }, 0x1Eu, 1, DisplayName = "0001 1110")]
        [DataRow(new byte[] { 0xFE, 0x01 }, 0xFEu, 1, DisplayName = "1111 1110")]
        // 2 byte tag:
        [DataRow(new byte[] { 0x1F, 0x00, 0x01 }, 0x1F00u, 2, DisplayName = "0001 1111 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0x00, 0x01 }, 0xFF00u, 2, DisplayName = "1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x7F, 0x01 }, 0x1F7Fu, 2, DisplayName = "0001 1111 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0x7F, 0x01 }, 0xFF7Fu, 2, DisplayName = "1111 1111 0111 1111")]
        // 3 byte tag:
        [DataRow(new byte[] { 0x1F, 0x80, 0x00, 0x01 }, 0x1F8000u, 3, DisplayName = "0001 1111 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0x80, 0x00, 0x01 }, 0xFF8000u, 3, DisplayName = "1111 1111 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x00, 0x01 }, 0x1FFF00u, 3, DisplayName = "0001 1111 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0x00, 0x01 }, 0xFFFF00u, 3, DisplayName = "1111 1111 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0x7F, 0x01 }, 0x1F807Fu, 3, DisplayName = "0001 1111 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0x80, 0x7F, 0x01 }, 0xFF807Fu, 3, DisplayName = "1111 1111 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x7F, 0x01 }, 0x1FFF7Fu, 3, DisplayName = "0001 1111 1111 1111 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0x7F, 0x01 }, 0xFFFF7Fu, 3, DisplayName = "1111 1111 1111 1111 0111 1111")]
        // 4 byte tag:
        [DataRow(new byte[] { 0x1F, 0x80, 0x80, 0x00, 0x01 }, 0x1F808000u, 4, DisplayName = "0001 1111 1000 0000 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0x80, 0x80, 0x00, 0x01 }, 0xFF808000u, 4, DisplayName = "1111 1111 1000 0000 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x80, 0x00, 0x01 }, 0x1FFF8000u, 4, DisplayName = "0001 1111 1111 1111 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0x80, 0x00, 0x01 }, 0xFFFF8000u, 4, DisplayName = "1111 1111 1111 1111 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0xFF, 0x00, 0x01 }, 0x1F80FF00u, 4, DisplayName = "0001 1111 1000 0000 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0x80, 0xFF, 0x00, 0x01 }, 0xFF80FF00u, 4, DisplayName = "1111 1111 1000 0000 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0xFF, 0x00, 0x01 }, 0x1FFFFF00u, 4, DisplayName = "0001 1111 1111 1111 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0xFF, 0x00, 0x01 }, 0xFFFFFF00u, 4, DisplayName = "1111 1111 1111 1111 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0x80, 0x7F, 0x01 }, 0x1F80807Fu, 4, DisplayName = "0001 1111 1000 0000 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0x80, 0x80, 0x7F, 0x01 }, 0xFF80807Fu, 4, DisplayName = "1111 1111 1000 0000 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x80, 0x7F, 0x01 }, 0x1FFF807Fu, 4, DisplayName = "0001 1111 1111 1111 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0x80, 0x7F, 0x01 }, 0xFFFF807Fu, 4, DisplayName = "1111 1111 1111 1111 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0x80, 0xFF, 0x7F, 0x01 }, 0x1F80FF7Fu, 4, DisplayName = "0001 1111 1000 0000 1111 1111 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0x80, 0xFF, 0x7F, 0x01 }, 0xFF80FF7Fu, 4, DisplayName = "1111 1111 1000 0000 1111 1111 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0xFF, 0x7F, 0x01 }, 0x1FFFFF7Fu, 4, DisplayName = "0001 1111 1111 1111 1111 1111 0111 1111")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F, 0x01 }, 0xFFFFFF7Fu, 4, DisplayName = "1111 1111 1111 1111 1111 1111 0111 1111")]
        public void ReadTagSuccessNonEmv(byte[] input, uint? expectedTag, int expectedLength)
        {
            var ms = new MemoryStream(input);
            var t = TlvEncoding.ReadNextTag(ms, false);
            Assert.AreEqual(expectedTag, t);
            Assert.AreEqual(expectedLength, ms.Position);
        }

        [DataTestMethod]
        // end of stream
        [DataRow(new byte[] { }, null, 0, DisplayName = "(no data)")]
        // 0 byte tag, skip leading 00/FF:
        [DataRow(new byte[] { 0x00 }, null, 1, DisplayName = "0000 0000")]
        [DataRow(new byte[] { 0xFF }, null, 1, DisplayName = "1111 1111")]
        [DataRow(new byte[] { 0x00, 0x00 }, null, 2, DisplayName = "0000 0000 0000 0000")]
        [DataRow(new byte[] { 0xFF, 0xFF }, null, 2, DisplayName = "1111 1111 1111 1111")]
        [DataRow(new byte[] { 0x00, 0xFF }, null, 2, DisplayName = "0000 0000 1111 1111")]
        [DataRow(new byte[] { 0xFF, 0x00 }, null, 2, DisplayName = "1111 1111 0000 0000")]
        // 1 byte tag:
        [DataRow(new byte[] { 0x01, 0x01 }, 0x01u, 1, DisplayName = "0000 0001")]
        [DataRow(new byte[] { 0x80, 0x01 }, 0x80u, 1, DisplayName = "1000 0000")]
        [DataRow(new byte[] { 0x40, 0x01 }, 0x40u, 1, DisplayName = "0100 0000")]
        [DataRow(new byte[] { 0x20, 0x01 }, 0x20u, 1, DisplayName = "0010 0000")]
        [DataRow(new byte[] { 0xFE, 0x01 }, 0xFEu, 1, DisplayName = "1111 1110")]
        // 1 byte tag, skip leading 00:
        [DataRow(new byte[] { 0x00, 0x01, 0x01 }, 0x01u, 2, DisplayName = "0000 0000 0000 0001")]
        [DataRow(new byte[] { 0x00, 0x80, 0x01 }, 0x80u, 2, DisplayName = "0000 0000 1000 0000")]
        [DataRow(new byte[] { 0x00, 0xFE, 0x01 }, 0xFEu, 2, DisplayName = "0000 0000 1111 1110")]
        // 1 byte tag, skip leading 0000:
        [DataRow(new byte[] { 0x00, 0x00, 0x01, 0x01 }, 0x01u, 3, DisplayName = "0000 0000 0000 0000 0000 0001")]
        [DataRow(new byte[] { 0x00, 0x00, 0x80, 0x01 }, 0x80u, 3, DisplayName = "0000 0000 0000 0000 1000 0000")]
        [DataRow(new byte[] { 0x00, 0x00, 0xFE, 0x01 }, 0xFEu, 3, DisplayName = "0000 0000 0000 0000 1111 1110")]
        // 1 byte tag, skip leading FF:
        [DataRow(new byte[] { 0xFF, 0x01, 0x01 }, 0x01u, 2, DisplayName = "1111 1111 0000 0001")]
        [DataRow(new byte[] { 0xFF, 0x80, 0x01 }, 0x80u, 2, DisplayName = "1111 1111 1000 0000")]
        [DataRow(new byte[] { 0xFF, 0xFE, 0x01 }, 0xFEu, 2, DisplayName = "1111 1111 1111 1110")]
        // 1 byte tag, skip leading FFFF:
        [DataRow(new byte[] { 0xFF, 0xFF, 0x01, 0x01 }, 0x01u, 3, DisplayName = "1111 1111 1111 1111 0000 0001")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0x80, 0x01 }, 0x80u, 3, DisplayName = "1111 1111 1111 1111 1000 0000")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0xFE, 0x01 }, 0xFEu, 3, DisplayName = "1111 1111 1111 1111 1111 1110")]
        // 2 byte tag:
        [DataRow(new byte[] { 0x1F, 0x00, 0x01 }, 0x1F00u, 2, DisplayName = "0001 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x7F, 0x01 }, 0x1F7Fu, 2, DisplayName = "0001 1111 0111 1111")]
        // 3 byte tag:
        [DataRow(new byte[] { 0x1F, 0x80, 0x00, 0x01 }, 0x1F8000u, 3, DisplayName = "0001 1111 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x00, 0x01 }, 0x1FFF00u, 3, DisplayName = "0001 1111 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0x7F, 0x01 }, 0x1F807Fu, 3, DisplayName = "0001 1111 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x7F, 0x01 }, 0x1FFF7Fu, 3, DisplayName = "0001 1111 1111 1111 0111 1111")]
        // 4 byte tag:
        [DataRow(new byte[] { 0x1F, 0x80, 0x80, 0x00, 0x01 }, 0x1F808000u, 4, DisplayName = "0001 1111 1000 0000 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x80, 0x00, 0x01 }, 0x1FFF8000u, 4, DisplayName = "0001 1111 1111 1111 1000 0000 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0xFF, 0x00, 0x01 }, 0x1F80FF00u, 4, DisplayName = "0001 1111 1000 0000 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0xFF, 0x00, 0x01 }, 0x1FFFFF00u, 4, DisplayName = "0001 1111 1111 1111 1111 1111 0000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0x80, 0x7F, 0x01 }, 0x1F80807Fu, 4, DisplayName = "0001 1111 1000 0000 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x80, 0x7F, 0x01 }, 0x1FFF807Fu, 4, DisplayName = "0001 1111 1111 1111 1000 0000 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0x80, 0xFF, 0x7F, 0x01 }, 0x1F80FF7Fu, 4, DisplayName = "0001 1111 1000 0000 1111 1111 0111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0xFF, 0x7F, 0x01 }, 0x1FFFFF7Fu, 4, DisplayName = "0001 1111 1111 1111 1111 1111 0111 1111")]
        public void ReadTagSuccessEmv(byte[] input, uint? expectedTag, int expectedLength)
        {
            var ms = new MemoryStream(input);
            var t = TlvEncoding.ReadNextTag(ms, true);
            Assert.AreEqual(expectedTag, t);
            Assert.AreEqual(expectedLength, ms.Position);
        }

        [DataTestMethod]
        [DataRow(new byte[] { 0x1F }, DisplayName = "0001 1111")]
        [DataRow(new byte[] { 0xFF }, DisplayName = "1111 1111")]
        [DataRow(new byte[] { 0x1F, 0x80 }, DisplayName = "0001 1111 1000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF }, DisplayName = "0001 1111 1111 1111")]
        [DataRow(new byte[] { 0x1F, 0x80, 0x80 }, DisplayName = "0001 1111 1000 0000 1000 0000")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0x80 }, DisplayName = "0001 1111 1111 1111 1000 0000")]
        [DataRow(new byte[] { 0x1F, 0x80, 0xFF }, DisplayName = "0001 1111 1000 0000 1111 1111")]
        [DataRow(new byte[] { 0x1F, 0xFF, 0xFF }, DisplayName = "0001 1111 1111 1111 1111 1111")]
        public void ReadTagFailsUnexpectedEnd(byte[] input)
        {
            var ms = new MemoryStream(input);
            Assert.ThrowsException<TlvException>(() => {
                TlvEncoding.ReadNextTag(ms, false);
            }); // last octet is indicating a subsequent octet, but no more data
        }


        [TestMethod]
        public void ReadTagFails5Bytes()
        {
            var ms = new MemoryStream(new byte[] { 0x1F, 0x80, 0x80, 0x80, 0x00 });
            Assert.ThrowsException<TlvException>(() => {
                TlvEncoding.ReadNextTag(ms, false);
            });
        }

        [DataTestMethod]
        // short form (1 byte length):
        [DataRow(new byte[] { 0x00, 0x01 }, 0x00, 1, DisplayName = "0000 0000")]
        [DataRow(new byte[] { 0x7F, 0x01 }, 0x7F, 1, DisplayName = "0111 1111")]
        [DataRow(new byte[] { 0x26, 0x01 }, 0x26, 1, DisplayName = "0010 0110")]
        // long form, indefinite
        [DataRow(new byte[] { 0x80, 0x01 }, null, 1, DisplayName = "1000 0000")]
        // long form
        [DataRow(new byte[] { 0x81, 0x00 }, 0x00, 2, DisplayName = "1000 0001 0000 0000")]
        [DataRow(new byte[] { 0x81, 0x00, 0x01 }, 0x00, 2, DisplayName = "1000 0001 0000 0000")]
        [DataRow(new byte[] { 0x81, 0x01, 0x01 }, 0x01, 2, DisplayName = "1000 0001 0000 0001")]
        [DataRow(new byte[] { 0x81, 0xFF, 0x01 }, 0xFF, 2, DisplayName = "1000 0001 1111 1111")]
        [DataRow(new byte[] { 0x82, 0x00, 0xFF, 0x01 }, 0xFF, 3, DisplayName = "1000 0001 0000 0000 1111 1111")] // unnecessary length byte
        [DataRow(new byte[] { 0x82, 0xFF, 0xFF, 0x01 }, 0xFFFF, 3, DisplayName = "1000 0010 1111 1111 1111 1111")]
        [DataRow(new byte[] { 0x83, 0xFF, 0xFF, 0xFF, 0x01 }, 0xFFFFFF, 4, DisplayName = "1000 0011 1111 1111 1111 1111 1111 1111")]
        [DataRow(new byte[] { 0x84, 0x7F, 0xFF, 0xFF, 0xFF, 0x01 }, 0x7FFFFFFF, 5, DisplayName = "1000 0100 0111 1111 1111 1111 1111 1111 1111 1111")] // max value for positive int
        //[DataRow(new byte[] { 0x84, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 }, 0xFFFFFFFF, 5, DisplayName = "1000 0100 1111 1111 1111 1111 1111 1111 1111 1111")]
        [DataRow(new byte[] { 0x82, 0x00, 0x00, 0x01 }, 0x0000, 3, DisplayName = "1000 0010 0000 0000 0000 0000")]
        [DataRow(new byte[] { 0x83, 0x00, 0x00, 0x00, 0x01 }, 0x000000, 4, DisplayName = "1000 0011 0000 0000 0000 0000 0000 0000")]
        [DataRow(new byte[] { 0x84, 0x00, 0x00, 0x00, 0x00, 0x01 }, 0x00000000, 5, DisplayName = "1000 0100 0000 0000 0000 0000 0000 0000 0000 0000")]
        [DataRow(new byte[] { 0x82, 0xAB, 0xCD, 0x01 }, 0xABCD, 3, DisplayName = "1000 0010 1010 1011 1100 1101")]
        [DataRow(new byte[] { 0x83, 0xAB, 0xCD, 0xEF, 0x01 }, 0xABCDEF, 4, DisplayName = "1000 0011 1010 1011 1100 1101 1110 1111")]
        //[DataRow(new byte[] { 0x84, 0xAB, 0xCD, 0xEF, 0x01, 0x01 }, 0xABCDEF01, 5, DisplayName = "1000 0100 1010 1011 1100 1101 1110 1111 0000 0001")]
        public void ReadLengthSuccess(byte[] input, int? expectedLength, int expectedPosition)
        {
            var ms = new MemoryStream(input);
            var t = TlvEncoding.ReadLength(ms);
            Assert.AreEqual(expectedLength, t);
            Assert.AreEqual(expectedPosition, ms.Position);
        }
        
        [DataTestMethod]
        [DataRow(new byte[] { }, DisplayName = "(no data)")]
        [DataRow(new byte[] { 0x81 }, DisplayName = "1000 0001")]
        [DataRow(new byte[] { 0x82, 0x00 }, DisplayName = "1000 0001 0000 0000")]
        [DataRow(new byte[] { 0x83, 0x00, 0x00 }, DisplayName = "1000 0001 0000 0000 0000 0000")]
        public void ReadLengthFailsUnexpectedEnd(byte[] input)
        {
            var ms = new MemoryStream(input);
            Assert.ThrowsException<TlvException>(() => {
                TlvEncoding.ReadLength(ms);
            });
        }

        [TestMethod]
        public void ReadLengthFails5Bytes()
        {
            var ms = new MemoryStream(new byte[] { 0x85, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.ThrowsException<TlvException>(() => {
                TlvEncoding.ReadLength(ms);
            });
        }

        [DataTestMethod]
        [DataRow(null, new byte[] { 0x80 }, DisplayName = "indefinite")]
        [DataRow(0, new byte[] { 0x00 }, DisplayName = "0")]
        [DataRow(0x7f, new byte[] { 0x7f }, DisplayName = "7F")]
        [DataRow(0x80, new byte[] { 0x81, 0x80 }, DisplayName = "80")]
        [DataRow(0x81, new byte[] { 0x81, 0x81 }, DisplayName = "81")]
        [DataRow(0xff, new byte[] { 0x81, 0xff }, DisplayName = "FF")]
        [DataRow(0x0100, new byte[] { 0x82, 0x01, 0x00 }, DisplayName = "0100")]
        [DataRow(0xFFFF, new byte[] { 0x82, 0xFF, 0xFF }, DisplayName = "FFFF")]
        [DataRow(0xFFFFFF, new byte[] { 0x83, 0xFF, 0xFF, 0xFF }, DisplayName = "FFFFFF")]
        //[DataRow(0xFFFFFFFF, new byte[] { 0x84, 0xFF, 0xFF, 0xFF, 0xFF }, DisplayName = "FFFFFFFF")]
        [DataRow(0x01ABCDEF, new byte[] { 0x84, 0x01, 0xAB, 0xCD, 0xEF }, DisplayName = "01ABCDEF")]
        public void WriteLengthSuccess(int? length, byte[] expectedOutput)
        {
            var ms = new MemoryStream();
            TlvEncoding.WriteLength(ms, length);

            ms.Position = 0;
            var buf = new byte[ms.Length];
            ms.Read(buf, 0, buf.Length);
            AssertByteArrayEqual(expectedOutput, buf);
        }

        [DataTestMethod]
        [DataRow(0x00u, new byte[] { 0x00 }, DisplayName = "0x00: ")]
        [DataRow(0x01u, new byte[] { 0x01 }, DisplayName = "0x01: 0000 0001")]
        [DataRow(0xFEu, new byte[] { 0xFE }, DisplayName = "0xFE: 1111 1110")]
        [DataRow(0xEFu, new byte[] { 0xEF }, DisplayName = "0xEF: 1110 1111")]
        [DataRow(0x1Eu, new byte[] { 0x1E }, DisplayName = "0x1E: 0001 1110")]
        [DataRow(0x0Fu, new byte[] { 0x0F }, DisplayName = "0x0F: 0000 1111")]
        [DataRow(0x1F00u, new byte[] { 0x1F, 0x00 }, DisplayName = "0x1F00: 0001 1111 0000 0000")]
        [DataRow(0x1F7Fu, new byte[] { 0x1F, 0x7F }, DisplayName = "0x1F7F: 0001 1111 0111 1111")]
        [DataRow(0x1FFF7Fu, new byte[] { 0x1F, 0xFF, 0x7F }, DisplayName = "0x1FFF7F: 0001 1111 1111 1111 0111 1111")]
        public void WriteTagSuccess(uint tag, byte[] expectedOutput)
        {
            var ms = new MemoryStream();
            TlvEncoding.WriteTag(ms, tag);

            ms.Position = 0;
            var buf = new byte[ms.Length];
            ms.Read(buf, 0, buf.Length);
            AssertByteArrayEqual(expectedOutput, buf);
        }

        [DataTestMethod]
        // must have following byte i.e. missing data:
        [DataRow(0x1Fu, DisplayName = "0x1F: 0001 1111")]
        [DataRow(0xFFu, DisplayName = "0x1F: 1111 1111")]
        [DataRow(0xFF80u, DisplayName = "0xFF80: 1111 1111 1000 0000")]
        [DataRow(0xFFFFu, DisplayName = "0xFFFF: 1111 1111 1111 1111")]
        // must not have following byte i.e. too much data:
        [DataRow(0x1E00u, DisplayName = "0x1E00: 0001 1110 0000 0000")]
        [DataRow(0x0F00u, DisplayName = "0x0F00: 0000 1111 0000 0000")]
        [DataRow(0xFE00u, DisplayName = "0xFE00: 1111 1110 0000 0000")]
        [DataRow(0x1EFFu, DisplayName = "0x1EFF: 0001 1110 1111 1111")]
        [DataRow(0x0FFFu, DisplayName = "0x0FFF: 0000 1111 1111 1111")]
        [DataRow(0xFEFFu, DisplayName = "0xFEFF: 1111 1110 1111 1111")]
        [DataRow(0xFF0000u, DisplayName = "0xFF0000: 1111 1111 0000 0000 0000 0000")]
        [DataRow(0xFF7F00u, DisplayName = "0xFF7F00: 1111 1111 0111 1111 0000 0000")]
        public void WriteTagInvalid(uint tag)
        {
            var ms = new MemoryStream();
            Assert.ThrowsException<TlvException>(() => {
                TlvEncoding.WriteTag(ms, tag);
            });
        }

        private static void AssertByteArrayEqual(byte[] expected, byte[] actual)
        {
            if (expected == null &&
                actual == null)
                return;

            if (expected == null)
                Assert.IsNull(actual);

            if (expected != null)
                Assert.IsNotNull(actual);

            Assert.AreEqual(BitConverter.ToString(expected), BitConverter.ToString(actual));
        }

        [DataTestMethod]
        [DataRow(new byte[] { 0xe0, 0x00 }, 0xe0u, new byte[] { })]
        [DataRow(new byte[] { 0xe0, 0x00 }, 0xe0u, null)]
        [DataRow(new byte[] { 0xe0, 0x01, 0x00 }, 0xe0u, new byte[] { 0x00 })]
        [DataRow(new byte[] { 0xe0, 0x01, 0xff }, 0xe0u, new byte[] { 0xff })]
        [DataRow(new byte[] { 0xe0, 0x02, 0xff, 0xee }, 0xe0u, new byte[] { 0xff, 0xee })]
        [DataRow(new byte[] { 0x1F, 0x7F, 0x01, 0x12 }, 0x1F7Fu, new byte[] { 0x12 })]
        public void WriteTlv(byte[] expected, uint tag, byte[] tagValue)
        {
            var ms = new MemoryStream();
            TlvEncoding.WriteTlv(ms, tag, tagValue);

            ms.Position = 0;
            var buf = new byte[ms.Length];
            ms.Read(buf, 0, buf.Length);
            AssertByteArrayEqual(expected, buf);
        }

        [TestMethod]
        public void ProcessTlvStreamZeroLengthTag()
        {
            var d = new byte[] { 0xe0, 0x00 };

            var ms = new MemoryStream(d);
            TlvEncoding.ProcessTlvStream(ms, (tag, data) => { });
        }

        private struct Tlv
        {
            public uint Tag;
            public byte[] Value;
        };

        [TestMethod]
        public void ProcessTlvStream()
        {
            var expectedTlv = new[] {
                new Tlv {
                    Tag = 0xe0u,
                    Value = new byte[] { 0x12 },
                },
                new Tlv {
                    Tag = 0xe1u,
                    Value = new byte[] { 0x23, 0x34 },
                },
            };

            var d = new byte[] { 0xe0, 0x01, 0x12, 0xe1, 0x02, 0x23, 0x34 };

            var actualTlv = new List<Tlv>();
            var ms = new MemoryStream(d);
            TlvEncoding.ProcessTlvStream(ms, 
                (tag, data) => {
                    actualTlv.Add(new Tlv {
                        Tag = tag,
                        Value = data,
                    });
                });

            AssertTlvArrayEqual(expectedTlv, actualTlv.ToArray());
        }

        private static void AssertTlvArrayEqual(Tlv[] expected, Tlv[] actual)
        {
            if (expected == null &&
                actual == null)
                return;

            if (expected == null)
                Assert.IsNull(actual);

            if (expected != null)
                Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Length, actual.Length);

            for (int i=0; i<expected.Length; i++)
            {
                var e = expected[i];
                var a = actual[i];
                Assert.AreEqual(e.Tag, a.Tag);
                AssertByteArrayEqual(e.Value, a.Value);
            }
        }

    }
}
