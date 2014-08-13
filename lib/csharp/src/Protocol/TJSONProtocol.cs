/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership. The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thrift.Transport;
using System.Globalization;

namespace Thrift.Protocol
{
	/// <summary>
	/// JSON protocol implementation for thrift.
	///
	/// This is a full-featured protocol supporting Write and Read.
	///
	/// Please see the C++ class header for a detailed description of the
	/// protocol's wire format.
	///
	/// Adapted from the Java version.
	/// </summary>
	public class TJSONProtocol : TProtocol
	{
		/// <summary>
		/// Factory for JSON protocol objects
		/// </summary>
		public class Factory : TProtocolFactory
		{
			public TProtocol GetProtocol(TTransport trans)
			{
				return new TJSONProtocol(trans);
			}
		}

		private static byte[] COMMA = new byte[] { (byte)',' };
		private static byte[] COLON = new byte[] { (byte)':' };
		private static byte[] LBRACE = new byte[] { (byte)'{' };
		private static byte[] RBRACE = new byte[] { (byte)'}' };
		private static byte[] LBRACKET = new byte[] { (byte)'[' };
		private static byte[] RBRACKET = new byte[] { (byte)']' };
		private static byte[] QUOTE = new byte[] { (byte)'"' };
		private static byte[] BACKSLASH = new byte[] { (byte)'\\' };

		private byte[] ESCSEQ = new byte[] { (byte)'\\', (byte)'u', (byte)'0', (byte)'0' };

		private const long VERSION = 1;
		private byte[] JSON_CHAR_TABLE = {
	0,  0,  0,  0,  0,  0,  0,  0,(byte)'b',(byte)'t',(byte)'n',  0,(byte)'f',(byte)'r',  0,  0, 
	0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 
	1,  1,(byte)'"',  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
  };

		private char[] ESCAPE_CHARS = "\"\\/bfnrt".ToCharArray();

		private byte[] ESCAPE_CHAR_VALS = {
	(byte)'"', (byte)'\\', (byte)'/', (byte)'\b', (byte)'\f', (byte)'\n', (byte)'\r', (byte)'\t',
  };

		private const int DEF_STRING_SIZE = 16;

		private static byte[] NAME_BOOL = new byte[] { (byte)'t', (byte)'f' };
		private static byte[] NAME_BYTE = new byte[] { (byte)'i', (byte)'8' };
		private static byte[] NAME_I16 = new byte[] { (byte)'i', (byte)'1', (byte)'6' };
		private static byte[] NAME_I32 = new byte[] { (byte)'i', (byte)'3', (byte)'2' };
		private static byte[] NAME_I64 = new byte[] { (byte)'i', (byte)'6', (byte)'4' };
		private static byte[] NAME_DOUBLE = new byte[] { (byte)'d', (byte)'b', (byte)'l' };
		private static byte[] NAME_STRUCT = new byte[] { (byte)'r', (byte)'e', (byte)'c' };
		private static byte[] NAME_STRING = new byte[] { (byte)'s', (byte)'t', (byte)'r' };
		private static byte[] NAME_MAP = new byte[] { (byte)'m', (byte)'a', (byte)'p' };
		private static byte[] NAME_LIST = new byte[] { (byte)'l', (byte)'s', (byte)'t' };
		private static byte[] NAME_SET = new byte[] { (byte)'s', (byte)'e', (byte)'t' };

		private static byte[] GetTypeNameForTypeID(TType typeID)
		{
			switch (typeID)
			{
				case TType.Bool:
					return NAME_BOOL;
				case TType.Byte:
					return NAME_BYTE;
				case TType.I16:
					return NAME_I16;
				case TType.I32:
					return NAME_I32;
				case TType.I64:
					return NAME_I64;
				case TType.Double:
					return NAME_DOUBLE;
				case TType.String:
					return NAME_STRING;
				case TType.Struct:
					return NAME_STRUCT;
				case TType.Map:
					return NAME_MAP;
				case TType.Set:
					return NAME_SET;
				case TType.List:
					return NAME_LIST;
				default:
					throw new TProtocolException(TProtocolException.NOT_IMPLEMENTED,
												 "Unrecognized type");
			}
		}

		private static TType GetTypeIDForTypeName(byte[] name)
		{
			TType result = TType.Stop;
			if (name.Length > 1)
			{
				switch (name[0])
				{
					case (byte)'d':
						result = TType.Double;
						break;
					case (byte)'i':
						switch (name[1])
						{
							case (byte)'8':
								result = TType.Byte;
								break;
							case (byte)'1':
								result = TType.I16;
								break;
							case (byte)'3':
								result = TType.I32;
								break;
							case (byte)'6':
								result = TType.I64;
								break;
						}
						break;
					case (byte)'l':
						result = TType.List;
						break;
					case (byte)'m':
						result = TType.Map;
						break;
					case (byte)'r':
						result = TType.Struct;
						break;
					case (byte)'s':
						if (name[1] == (byte)'t')
						{
							result = TType.String;
						}
						else if (name[1] == (byte)'e')
						{
							result = TType.Set;
						}
						break;
					case (byte)'t':
						result = TType.Bool;
						break;
				}
			}
			if (result == TType.Stop)
			{
				throw new TProtocolException(TProtocolException.NOT_IMPLEMENTED,
											 "Unrecognized type");
			}
			return result;
		}

		///<summary>
		/// Base class for tracking JSON contexts that may require
		/// inserting/Reading additional JSON syntax characters
		/// This base context does nothing.
		///</summary>
		protected class JSONBaseContext
		{
			protected TJSONProtocol proto;

			public JSONBaseContext(TJSONProtocol proto)
			{
				this.proto = proto;
			}

			public virtual void Write() { }

            public virtual async Task ReadAsync() { }

			public virtual bool EscapeNumbers() { return false; }
		}

		///<summary>
		/// Context for JSON lists. Will insert/Read commas before each item except
		/// for the first one
		///</summary>
		protected class JSONListContext : JSONBaseContext
		{
			public JSONListContext(TJSONProtocol protocol)
				: base(protocol)
			{

			}
			private bool first = true;

			public override void Write()
			{
				if (first)
				{
					first = false;
				}
				else
				{
					proto.trans.WriteZZZ(COMMA);
				}
			}

			public override async Task ReadAsync()
			{
				if (first)
				{
					first = false;
				}
				else
				{
					await proto.ReadJSONSyntaxCharAsync(COMMA);
				}
			}
		}

		///<summary>
		/// Context for JSON records. Will insert/Read colons before the value portion
		/// of each record pair, and commas before each key except the first. In
		/// addition, will indicate that numbers in the key position need to be
		/// escaped in quotes (since JSON keys must be strings).
		///</summary>
		protected class JSONPairContext : JSONBaseContext
		{
			public JSONPairContext(TJSONProtocol proto)
				: base(proto)
			{

			}

			private bool first = true;
			private bool colon = true;

			public override void Write()
			{
				if (first)
				{
					first = false;
					colon = true;
				}
				else
				{
					proto.trans.WriteZZZ(colon ? COLON : COMMA);
					colon = !colon;
				}
			}

			public override async Task ReadAsync()
			{
				if (first)
				{
					first = false;
					colon = true;
				}
				else
				{
					await proto.ReadJSONSyntaxCharAsync(colon ? COLON : COMMA);
					colon = !colon;
				}
			}

			public override bool EscapeNumbers()
			{
				return colon;
			}
		}

		///<summary>
		/// Holds up to one byte from the transport
		///</summary>
		protected class LookaheadReader
		{
			protected TJSONProtocol proto;

			public LookaheadReader(TJSONProtocol proto)
			{
				this.proto = proto;
			}

			private bool hasData;
			private byte[] data = new byte[1];

			///<summary>
			/// Return and consume the next byte to be Read, either taking it from the
			/// data buffer if present or getting it from the transport otherwise.
			///</summary>
			public async Task<byte> ReadAsync()
			{
				if (hasData)
				{
					hasData = false;
				}
				else
				{
                    await proto.trans.ReadAllAsync(data, 0, 1);
				}
				return data[0];
			}

			///<summary>
			/// Return the next byte to be Read without consuming, filling the data
			/// buffer if it has not been filled alReady.
			///</summary>
			public async Task<byte> PeekAsync()
			{
				if (!hasData)
				{
					await proto.trans.ReadAllAsync(data, 0, 1);
				}
				hasData = true;
				return data[0];
			}
		}

		// Default encoding
		protected Encoding utf8Encoding = UTF8Encoding.UTF8;

		// Stack of nested contexts that we may be in
		protected Stack<JSONBaseContext> contextStack = new Stack<JSONBaseContext>();

		// Current context that we are in
		protected JSONBaseContext context;

		// Reader that manages a 1-byte buffer
		protected LookaheadReader reader;

		///<summary>
		/// Push a new JSON context onto the stack.
		///</summary>
		protected void PushContext(JSONBaseContext c)
		{
			contextStack.Push(context);
			context = c;
		}

		///<summary>
		/// Pop the last JSON context off the stack
		///</summary>
		protected void PopContext()
		{
			context = contextStack.Pop();
		}

		///<summary>
		/// TJSONProtocol Constructor
		///</summary>
		public TJSONProtocol(TTransport trans)
			: base(trans)
		{
			context = new JSONBaseContext(this);
			reader = new LookaheadReader(this);
		}

		// Temporary buffer used by several methods
		private byte[] tempBuffer = new byte[4];

		///<summary>
		/// Read a byte that must match b[0]; otherwise an exception is thrown.
		/// Marked protected to avoid synthetic accessor in JSONListContext.Read
		/// and JSONPairContext.Read
		///</summary>
		protected async Task ReadJSONSyntaxCharAsync(byte[] b)
		{
			byte ch = await reader.ReadAsync();
			if (ch != b[0])
			{
				throw new TProtocolException(TProtocolException.INVALID_DATA,
											 "Unexpected character:" + (char)ch);
			}
		}

		///<summary>
		/// Convert a byte containing a hex char ('0'-'9' or 'a'-'f') into its
		/// corresponding hex value
		///</summary>
		private static byte HexVal(byte ch)
		{
			if ((ch >= '0') && (ch <= '9'))
			{
				return (byte)((char)ch - '0');
			}
			else if ((ch >= 'a') && (ch <= 'f'))
			{
				ch += 10;
				return (byte)((char)ch - 'a');
			}
			else
			{
				throw new TProtocolException(TProtocolException.INVALID_DATA,
											 "Expected hex character");
			}
		}

		///<summary>
		/// Convert a byte containing a hex value to its corresponding hex character
		///</summary>
		private static byte HexChar(byte val)
		{
			val &= 0x0F;
			if (val < 10)
			{
				return (byte)((char)val + '0');
			}
			else
			{
				val -= 10;
				return (byte)((char)val + 'a');
			}
		}

		///<summary>
		/// Write the bytes in array buf as a JSON characters, escaping as needed
		///</summary>
		private void WriteJSONString(byte[] b)
		{
			context.Write();
			trans.WriteZZZ(QUOTE);
			int len = b.Length;
			for (int i = 0; i < len; i++)
			{
				if ((b[i] & 0x00FF) >= 0x30)
				{
					if (b[i] == BACKSLASH[0])
					{
						trans.WriteZZZ(BACKSLASH);
						trans.WriteZZZ(BACKSLASH);
					}
					else
					{
						trans.WriteZZZ(b, i, 1);
					}
				}
				else
				{
					tempBuffer[0] = JSON_CHAR_TABLE[b[i]];
					if (tempBuffer[0] == 1)
					{
						trans.WriteZZZ(b, i, 1);
					}
					else if (tempBuffer[0] > 1)
					{
						trans.WriteZZZ(BACKSLASH);
						trans.WriteZZZ(tempBuffer, 0, 1);
					}
					else
					{
						trans.WriteZZZ(ESCSEQ);
						tempBuffer[0] = HexChar((byte)(b[i] >> 4));
						tempBuffer[1] = HexChar(b[i]);
						trans.WriteZZZ(tempBuffer, 0, 2);
					}
				}
			}
			trans.WriteZZZ(QUOTE);
		}

		///<summary>
		/// Write out number as a JSON value. If the context dictates so, it will be
		/// wrapped in quotes to output as a JSON string.
		///</summary>
		private void WriteJSONInteger(long num)
		{
			context.Write();
			String str = num.ToString();

			bool escapeNum = context.EscapeNumbers();
			if (escapeNum)
				trans.WriteZZZ(QUOTE);

			trans.WriteZZZ(utf8Encoding.GetBytes(str));

			if (escapeNum)
				trans.WriteZZZ(QUOTE);
		}

		///<summary>
		/// Write out a double as a JSON value. If it is NaN or infinity or if the
		/// context dictates escaping, Write out as JSON string.
		///</summary>
		private void WriteJSONDouble(double num)
		{
			context.Write();
			String str = num.ToString(CultureInfo.InvariantCulture);
			bool special = false;

			switch (str[0])
			{
				case 'N': // NaN
				case 'I': // Infinity
					special = true;
					break;
				case '-':
					if (str[1] == 'I')
					{ // -Infinity
						special = true;
					}
					break;
			}

			bool escapeNum = special || context.EscapeNumbers();

			if (escapeNum)
				trans.WriteZZZ(QUOTE);

			trans.WriteZZZ(utf8Encoding.GetBytes(str));

			if (escapeNum)
				trans.WriteZZZ(QUOTE);
		}
		///<summary>
		/// Write out contents of byte array b as a JSON string with base-64 encoded
		/// data
		///</summary>
		private void WriteJSONBase64(byte[] b)
		{
			context.Write();
			trans.WriteZZZ(QUOTE);

			int len = b.Length;
			int off = 0;

			while (len >= 3)
			{
				// Encode 3 bytes at a time
				TBase64Utils.encode(b, off, 3, tempBuffer, 0);
				trans.WriteZZZ(tempBuffer, 0, 4);
				off += 3;
				len -= 3;
			}
			if (len > 0)
			{
				// Encode remainder
				TBase64Utils.encode(b, off, len, tempBuffer, 0);
				trans.WriteZZZ(tempBuffer, 0, len + 1);
			}

			trans.WriteZZZ(QUOTE);
		}

		private void WriteJSONObjectStart()
		{
			context.Write();
			trans.WriteZZZ(LBRACE);
			PushContext(new JSONPairContext(this));
		}

		private void WriteJSONObjectEnd()
		{
			PopContext();
			trans.WriteZZZ(RBRACE);
		}

		private void WriteJSONArrayStart()
		{
			context.Write();
			trans.WriteZZZ(LBRACKET);
			PushContext(new JSONListContext(this));
		}

		private void WriteJSONArrayEnd()
		{
			PopContext();
			trans.WriteZZZ(RBRACKET);
		}

		public override async Task WriteMessageBeginAsync(TMessage message)
		{
			WriteJSONArrayStart();
			WriteJSONInteger(VERSION);

			byte[] b = utf8Encoding.GetBytes(message.Name);
			WriteJSONString(b);

			WriteJSONInteger((long)message.Type);
			WriteJSONInteger(message.SeqID);
		}

        public override async Task WriteMessageEndAsync()
		{
			WriteJSONArrayEnd();
		}

        public override async Task WriteStructBeginAsync(TStruct str)
		{
			WriteJSONObjectStart();
		}

        public override async Task WriteStructEndAsync()
		{
			WriteJSONObjectEnd();
		}

        public override async Task WriteFieldBeginAsync(TField field)
		{
			WriteJSONInteger(field.ID);
			WriteJSONObjectStart();
			WriteJSONString(GetTypeNameForTypeID(field.Type));
		}

        public override async Task WriteFieldEndAsync()
		{
			WriteJSONObjectEnd();
		}

        public override async Task WriteFieldStopAsync() { }

        public override async Task WriteMapBeginAsync(TMap map)
		{
			WriteJSONArrayStart();
			WriteJSONString(GetTypeNameForTypeID(map.KeyType));
			WriteJSONString(GetTypeNameForTypeID(map.ValueType));
			WriteJSONInteger(map.Count);
			WriteJSONObjectStart();
		}

        public override async Task WriteMapEndAsync()
		{
			WriteJSONObjectEnd();
			WriteJSONArrayEnd();
		}

        public override async Task WriteListBeginAsync(TList list)
		{
			WriteJSONArrayStart();
			WriteJSONString(GetTypeNameForTypeID(list.ElementType));
			WriteJSONInteger(list.Count);
		}

        public override async Task WriteListEndAsync()
		{
			WriteJSONArrayEnd();
		}

        public override async Task WriteSetBeginAsync(TSet set)
		{
			WriteJSONArrayStart();
			WriteJSONString(GetTypeNameForTypeID(set.ElementType));
			WriteJSONInteger(set.Count);
		}

        public override async Task WriteSetEndAsync()
		{
			WriteJSONArrayEnd();
		}

        public override async Task WriteBoolAsync(bool b)
		{
			WriteJSONInteger(b ? (long)1 : (long)0);
		}

        public override async Task WriteByteAsync(sbyte b)
		{
			WriteJSONInteger((long)b);
		}

        public override async Task WriteI16Async(short i16)
		{
			WriteJSONInteger((long)i16);
		}

        public override async Task WriteI32Async(int i32)
		{
			WriteJSONInteger((long)i32);
		}

        public override async Task WriteI64Async(long i64)
		{
			WriteJSONInteger(i64);
		}

        public override async Task WriteDoubleAsync(double dub)
		{
			WriteJSONDouble(dub);
		}

		public override async Task WriteStringAsync(String str)
		{
			byte[] b = utf8Encoding.GetBytes(str);
			WriteJSONString(b);
		}

        public override async Task WriteBinaryAsync(byte[] bin)
		{
			WriteJSONBase64(bin);
		}

		/**
		 * Reading methods.
		 */

		///<summary>
		/// Read in a JSON string, unescaping as appropriate.. Skip Reading from the
		/// context if skipContext is true.
		///</summary>
		private async Task<byte[]> ReadJSONStringAsync(bool skipContext)
		{
			MemoryStream buffer = new MemoryStream();


			if (!skipContext)
			{
				await context.ReadAsync();
			}
			await ReadJSONSyntaxCharAsync(QUOTE);
			while (true)
			{
				byte ch = await reader.ReadAsync();
				if (ch == QUOTE[0])
				{
					break;
				}

				// escaped?
				if (ch != ESCSEQ[0])
				{
					buffer.Write(new byte[] { (byte)ch }, 0, 1);
					continue;
				}

				// distinguish between \uXXXX and \?
				ch = await reader.ReadAsync();
				if (ch != ESCSEQ[1])  // control chars like \n
				{
					int off = Array.IndexOf(ESCAPE_CHARS, (char)ch);
					if (off == -1)
					{
						throw new TProtocolException(TProtocolException.INVALID_DATA,
														"Expected control char");
					}
					ch = ESCAPE_CHAR_VALS[off];
					buffer.Write(new byte[] { (byte)ch }, 0, 1);
					continue;
				}


				// it's \uXXXX
				await trans.ReadAllAsync(tempBuffer, 0, 4);
				var wch = (short)((HexVal((byte)tempBuffer[0]) << 12) +
								  (HexVal((byte)tempBuffer[1]) << 8) +
								  (HexVal((byte)tempBuffer[2]) << 4) + 
								   HexVal(tempBuffer[3]));
				var tmp = utf8Encoding.GetBytes(new char[] { (char)wch });
				buffer.Write(tmp, 0, tmp.Length);
			}
			return buffer.ToArray();
		}

		///<summary>
		/// Return true if the given byte could be a valid part of a JSON number.
		///</summary>
		private bool IsJSONNumeric(byte b)
		{
			switch (b)
			{
				case (byte)'+':
				case (byte)'-':
				case (byte)'.':
				case (byte)'0':
				case (byte)'1':
				case (byte)'2':
				case (byte)'3':
				case (byte)'4':
				case (byte)'5':
				case (byte)'6':
				case (byte)'7':
				case (byte)'8':
				case (byte)'9':
				case (byte)'E':
				case (byte)'e':
					return true;
			}
			return false;
		}

		///<summary>
		/// Read in a sequence of characters that are all valid in JSON numbers. Does
		/// not do a complete regex check to validate that this is actually a number.
		////</summary>
		private async Task<String> ReadJSONNumericCharsAsync()
		{
			StringBuilder strbld = new StringBuilder();
			while (true)
			{
				byte ch = await reader.PeekAsync();
				if (!IsJSONNumeric(ch))
				{
					break;
				}
				strbld.Append((char)await reader.ReadAsync());
			}
			return strbld.ToString();
		}

		///<summary>
		/// Read in a JSON number. If the context dictates, Read in enclosing quotes.
		///</summary>
		private async Task<long> ReadJSONIntegerAsync()
		{
			await context.ReadAsync();
			if (context.EscapeNumbers())
			{
				await ReadJSONSyntaxCharAsync(QUOTE);
			}
			String str = await ReadJSONNumericCharsAsync();
			if (context.EscapeNumbers())
			{
				await ReadJSONSyntaxCharAsync(QUOTE);
			}
			try
			{
				return Int64.Parse(str);
			}
			catch (FormatException)
			{
				throw new TProtocolException(TProtocolException.INVALID_DATA,
											 "Bad data encounted in numeric data");
			}
		}

		///<summary>
		/// Read in a JSON double value. Throw if the value is not wrapped in quotes
		/// when expected or if wrapped in quotes when not expected.
		///</summary>
		private async Task<double> ReadJSONDoubleAsync()
		{
			await context.ReadAsync();
			if (await reader.PeekAsync() == QUOTE[0])
			{
				byte[] arr = await ReadJSONStringAsync(true);
				double dub = Double.Parse(utf8Encoding.GetString(arr,0,arr.Length), CultureInfo.InvariantCulture);

				if (!context.EscapeNumbers() && !Double.IsNaN(dub) &&
					!Double.IsInfinity(dub))
				{
					// Throw exception -- we should not be in a string in this case
					throw new TProtocolException(TProtocolException.INVALID_DATA,
												 "Numeric data unexpectedly quoted");
				}
				return dub;
			}
			else
			{
				if (context.EscapeNumbers())
				{
					// This will throw - we should have had a quote if escapeNum == true
					await ReadJSONSyntaxCharAsync(QUOTE);
				}
				try
				{
					return Double.Parse(await ReadJSONNumericCharsAsync(), CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					throw new TProtocolException(TProtocolException.INVALID_DATA,
												 "Bad data encounted in numeric data");
				}
			}
		}

		//<summary>
		/// Read in a JSON string containing base-64 encoded data and decode it.
		///</summary>
		private async Task<byte[]> ReadJSONBase64Async()
		{
			byte[] b = await ReadJSONStringAsync(false);
			int len = b.Length;
			int off = 0;
			int size = 0;
			// reduce len to ignore fill bytes 
			while ((len > 0) && (b[len - 1] == '='))
			{
				--len;
			}
			// read & decode full byte triplets = 4 source bytes
			while (len > 4)
			{
				// Decode 4 bytes at a time
				TBase64Utils.decode(b, off, 4, b, size); // NB: decoded in place
				off += 4;
				len -= 4;
				size += 3;
			}
			// Don't decode if we hit the end or got a single leftover byte (invalid
			// base64 but legal for skip of regular string type)
			if (len > 1)
			{
				// Decode remainder
				TBase64Utils.decode(b, off, len, b, size); // NB: decoded in place
				size += len - 1;
			}
			// Sadly we must copy the byte[] (any way around this?)
			byte[] result = new byte[size];
			Array.Copy(b, 0, result, 0, size);
			return result;
		}

		private async Task ReadJSONObjectStartAsync()
		{
			await context.ReadAsync();
			await ReadJSONSyntaxCharAsync(LBRACE);
			PushContext(new JSONPairContext(this));
		}

        private async Task ReadJSONObjectEndAsync()
		{
			await ReadJSONSyntaxCharAsync(RBRACE);
			PopContext();
		}

        private async Task ReadJSONArrayStartAsync()
		{
			await context.ReadAsync();
			await ReadJSONSyntaxCharAsync(LBRACKET);
			PushContext(new JSONListContext(this));
		}

        private async Task ReadJSONArrayEndAsync()
		{
			await ReadJSONSyntaxCharAsync(RBRACKET);
			PopContext();
		}

        public override async Task<TMessage> ReadMessageBeginAsync()
		{
			TMessage message = new TMessage();
			await ReadJSONArrayStartAsync();
			if (await ReadJSONIntegerAsync() != VERSION)
			{
				throw new TProtocolException(TProtocolException.BAD_VERSION,
											 "Message contained bad version.");
			}

            var buf = await ReadJSONStringAsync(false);
			message.Name = utf8Encoding.GetString(buf,0,buf.Length);
			message.Type = (TMessageType)await ReadJSONIntegerAsync();
            message.SeqID = (int)await ReadJSONIntegerAsync();
			return message;
		}

        public override Task ReadMessageEndAsync()
		{
			return ReadJSONArrayEndAsync();
		}

        public override async Task<TStruct> ReadStructBeginAsync()
		{
			await ReadJSONObjectStartAsync();
			return new TStruct();
		}

        public override Task ReadStructEndAsync()
		{
			return ReadJSONObjectEndAsync();
		}

        public override async Task<TField> ReadFieldBeginAsync()
		{
			TField field = new TField();
			byte ch = await reader.PeekAsync();
			if (ch == RBRACE[0])
			{
				field.Type = TType.Stop;
			}
			else
			{
				field.ID = (short)await ReadJSONIntegerAsync();
				await ReadJSONObjectStartAsync();
				field.Type = GetTypeIDForTypeName(await ReadJSONStringAsync(false));
			}
			return field;
		}

        public override Task ReadFieldEndAsync()
		{
			return ReadJSONObjectEndAsync();
		}

        public override async Task<TMap> ReadMapBeginAsync()
		{
			TMap map = new TMap();
			await ReadJSONArrayStartAsync();
            map.KeyType = GetTypeIDForTypeName(await ReadJSONStringAsync(false));
            map.ValueType = GetTypeIDForTypeName(await ReadJSONStringAsync(false));
            map.Count = (int)await ReadJSONIntegerAsync();
            await ReadJSONObjectStartAsync();
			return map;
		}

        public override async Task ReadMapEndAsync()
		{
            await ReadJSONObjectEndAsync();
            await ReadJSONArrayEndAsync();
		}

        public override async Task<TList> ReadListBeginAsync()
		{
			TList list = new TList();
            await ReadJSONArrayStartAsync();
            list.ElementType = GetTypeIDForTypeName(await ReadJSONStringAsync(false));
            list.Count = (int)await ReadJSONIntegerAsync();
			return list;
		}

        public override Task ReadListEndAsync()
		{
            return ReadJSONArrayEndAsync();
		}

        public override async Task<TSet> ReadSetBeginAsync()
		{
			TSet set = new TSet();
            await ReadJSONArrayStartAsync();
            set.ElementType = GetTypeIDForTypeName(await ReadJSONStringAsync(false));
            set.Count = (int)await ReadJSONIntegerAsync();
			return set;
		}

        public override Task ReadSetEndAsync()
		{
            return ReadJSONArrayEndAsync();
		}

        public override async Task<bool> ReadBoolAsync()
		{
            return (await ReadJSONIntegerAsync() != 0);
		}

        public override async Task<sbyte> ReadByteAsync()
		{
            return (sbyte)await ReadJSONIntegerAsync();
		}

        public override async Task<short> ReadI16Async()
		{
            return (short)await ReadJSONIntegerAsync();
		}

        public override async Task<int> ReadI32Async()
		{
            return (int)await ReadJSONIntegerAsync();
		}

        public override async Task<long> ReadI64Async()
		{
            return (long)await ReadJSONIntegerAsync();
		}

        public override Task<double> ReadDoubleAsync()
		{
            return ReadJSONDoubleAsync();
		}

        public override async Task<String> ReadStringAsync()
		{
            var buf = await ReadJSONStringAsync(false);
			return utf8Encoding.GetString(buf,0,buf.Length);
		}

        public override Task<byte[]> ReadBinaryAsync()
		{
            return ReadJSONBase64Async();
		}

	}
}
