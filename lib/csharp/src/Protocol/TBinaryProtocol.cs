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
 *
 * Contains some contributions under the Thrift Software License.
 * Please see doc/old-thrift-license.txt in the Thrift distribution for
 * details.
 */

using System;
using System.Text;
using System.Threading.Tasks;
using Thrift.Transport;

namespace Thrift.Protocol
{
	public class TBinaryProtocol : TProtocol
	{
		protected const uint VERSION_MASK = 0xffff0000;
		protected const uint VERSION_1 = 0x80010000;

		protected bool strictRead_ = false;
		protected bool strictWrite_ = true;

		#region BinaryProtocol Factory
		 /**
		  * Factory
		  */
		  public class Factory : TProtocolFactory {

			  protected bool strictRead_ = false;
			  protected bool strictWrite_ = true;

			  public Factory()
				  :this(false, true)
			  {
			  }

			  public Factory(bool strictRead, bool strictWrite)
			  {
				  strictRead_ = strictRead;
				  strictWrite_ = strictWrite;
			  }

			public TProtocol GetProtocol(TTransport trans) {
			  return new TBinaryProtocol(trans, strictRead_, strictWrite_);
			}
		  }

		#endregion

		public TBinaryProtocol(TTransport trans)
			: this(trans, false, true)
		{
		}

		public TBinaryProtocol(TTransport trans, bool strictRead, bool strictWrite)
			:base(trans)
		{
			strictRead_ = strictRead;
			strictWrite_ = strictWrite;
		}

		#region Write Methods

		public override async Task WriteMessageBeginAsync(TMessage message)
		{
			if (strictWrite_)
			{
				uint version = VERSION_1 | (uint)(message.Type);
				await WriteI32Async((int)version);
				await WriteStringAsync(message.Name);
				await WriteI32Async(message.SeqID);
			}
			else
			{
				await WriteStringAsync(message.Name);
				await WriteByteAsync((sbyte)message.Type);
                await WriteI32Async(message.SeqID);
			}
		}

		public override Task WriteMessageEndAsync()
		{
		    return NoopTask;
		}

        public override Task WriteStructBeginAsync(TStruct struc)
		{
            return NoopTask;
		}

        public override Task WriteStructEndAsync()
		{
            return NoopTask;
		}

        public override async Task WriteFieldBeginAsync(TField field)
		{
			await WriteByteAsync((sbyte)field.Type);
			await WriteI16Async(field.ID);
		}

        public override Task WriteFieldEndAsync()
		{
            return NoopTask;
		}

        public override Task WriteFieldStopAsync()
		{
			return WriteByteAsync((sbyte)TType.Stop);
		}

        public override async Task WriteMapBeginAsync(TMap map)
		{
			await WriteByteAsync((sbyte)map.KeyType);
			await WriteByteAsync((sbyte)map.ValueType);
			await WriteI32Async(map.Count);
		}

        public override Task WriteMapEndAsync()
		{
            return NoopTask;
		}

        public override async Task WriteListBeginAsync(TList list)
		{
			await WriteByteAsync((sbyte)list.ElementType);
			await WriteI32Async(list.Count);
		}

        public override Task WriteListEndAsync()
		{
            return NoopTask;
		}

        public override async Task WriteSetBeginAsync(TSet set)
		{
			await WriteByteAsync((sbyte)set.ElementType);
			await WriteI32Async(set.Count);
		}

        public override Task WriteSetEndAsync()
		{
            return NoopTask;
		}

        public override Task WriteBoolAsync(bool b)
		{
			return WriteByteAsync(b ? (sbyte)1 : (sbyte)0);
		}

		private byte[] bout = new byte[1];
        public override async Task WriteByteAsync(sbyte b)
		{
			bout[0] = (byte)b;
			trans.Write(bout, 0, 1);
		}

		private byte[] i16out = new byte[2];
        public override async Task WriteI16Async(short s)
		{
			i16out[0] = (byte)(0xff & (s >> 8));
			i16out[1] = (byte)(0xff & s);
			trans.Write(i16out, 0, 2);
		}

		private byte[] i32out = new byte[4];
        public override async Task WriteI32Async(int i32)
		{
			i32out[0] = (byte)(0xff & (i32 >> 24));
			i32out[1] = (byte)(0xff & (i32 >> 16));
			i32out[2] = (byte)(0xff & (i32 >> 8));
			i32out[3] = (byte)(0xff & i32);
			trans.Write(i32out, 0, 4);
		}

		private byte[] i64out = new byte[8];
        public override async Task WriteI64Async(long i64)
		{
			i64out[0] = (byte)(0xff & (i64 >> 56));
			i64out[1] = (byte)(0xff & (i64 >> 48));
			i64out[2] = (byte)(0xff & (i64 >> 40));
			i64out[3] = (byte)(0xff & (i64 >> 32));
			i64out[4] = (byte)(0xff & (i64 >> 24));
			i64out[5] = (byte)(0xff & (i64 >> 16));
			i64out[6] = (byte)(0xff & (i64 >> 8));
			i64out[7] = (byte)(0xff & i64);
			trans.Write(i64out, 0, 8);
		}

        public override Task WriteDoubleAsync(double d)
		{
#if !SILVERLIGHT
			return WriteI64Async(BitConverter.DoubleToInt64Bits(d));
#else
            var bytes = BitConverter.GetBytes(d);
            WriteI64(BitConverter.ToInt64(bytes, 0));
#endif
		}

        public override async Task WriteBinaryAsync(byte[] b)
		{
			await WriteI32Async(b.Length);
			trans.Write(b, 0, b.Length);
		}

		#endregion

		#region ReadMethods

		public override async Task<TMessage> ReadMessageBeginAsync()
		{
			TMessage message = new TMessage();
            int size = await ReadI32Async();
			if (size < 0)
			{
				uint version = (uint)size & VERSION_MASK;
				if (version != VERSION_1)
				{
					throw new TProtocolException(TProtocolException.BAD_VERSION, "Bad version in ReadMessageBegin: " + version);
				}
				message.Type = (TMessageType)(size & 0x000000ff);
                message.Name = await ReadStringAsync();
                message.SeqID = await ReadI32Async();
			}
			else
			{
				if (strictRead_)
				{
					throw new TProtocolException(TProtocolException.BAD_VERSION, "Missing version in readMessageBegin, old client?");
				}
				message.Name = ReadStringBody(size);
                message.Type = (TMessageType)await ReadByteAsync();
                message.SeqID = await ReadI32Async();
			}
			return message;
		}

        public override async Task ReadMessageEndAsync()
		{
		}

        public override async Task<TStruct> ReadStructBeginAsync()
		{
			return new TStruct();
		}

        public override async Task ReadStructEndAsync()
		{
		}

        public override async Task<TField> ReadFieldBeginAsync()
		{
			TField field = new TField();
            field.Type = (TType)await ReadByteAsync();

			if (field.Type != TType.Stop)
			{
                field.ID = await ReadI16Async();
			}

			return field;
		}

        public override async Task ReadFieldEndAsync()
		{
		}

        public override async Task<TMap> ReadMapBeginAsync()
		{
			TMap map = new TMap();
			map.KeyType = (TType)await ReadByteAsync();
            map.ValueType = (TType)await ReadByteAsync();
            map.Count = await ReadI32Async();

			return map;
		}

        public override async Task ReadMapEndAsync()
		{
		}

        public override async Task<TList> ReadListBeginAsync()
		{
			TList list = new TList();
            list.ElementType = (TType)await ReadByteAsync();
            list.Count = await ReadI32Async();

			return list;
		}

        public override async Task ReadListEndAsync()
		{
		}

        public override async Task<TSet> ReadSetBeginAsync()
		{
			TSet set = new TSet();
            set.ElementType = (TType)await ReadByteAsync();
            set.Count = await ReadI32Async();

			return set;
		}

        public override async Task ReadSetEndAsync()
		{
		}

        public override async Task<bool> ReadBoolAsync()
		{
            return await ReadByteAsync() == 1;
		}

		private byte[] bin = new byte[1];
        public override async Task<sbyte> ReadByteAsync()
		{
			ReadAll(bin, 0, 1);
			return (sbyte)bin[0];
		}

		private byte[] i16in = new byte[2];
        public override async Task<short> ReadI16Async()
		{
			ReadAll(i16in, 0, 2);
			return (short)(((i16in[0] & 0xff) << 8) | ((i16in[1] & 0xff)));
		}

		private byte[] i32in = new byte[4];
        public override async Task<int> ReadI32Async()
		{
			ReadAll(i32in, 0, 4);
			return (int)(((i32in[0] & 0xff) << 24) | ((i32in[1] & 0xff) << 16) | ((i32in[2] & 0xff) << 8) | ((i32in[3] & 0xff)));
		}

#pragma warning disable 675

        private byte[] i64in = new byte[8];
        public override async Task<long> ReadI64Async()
		{
			ReadAll(i64in, 0, 8);
            unchecked {
              return (long)(
                  ((long)(i64in[0] & 0xff) << 56) |
                  ((long)(i64in[1] & 0xff) << 48) |
                  ((long)(i64in[2] & 0xff) << 40) |
                  ((long)(i64in[3] & 0xff) << 32) |
                  ((long)(i64in[4] & 0xff) << 24) |
                  ((long)(i64in[5] & 0xff) << 16) |
                  ((long)(i64in[6] & 0xff) << 8) |
                  ((long)(i64in[7] & 0xff)));
            }
        }

#pragma warning restore 675

        public override async Task<double> ReadDoubleAsync()
		{
#if !SILVERLIGHT
            return BitConverter.Int64BitsToDouble(await ReadI64Async());
#else
            var value = ReadI64();
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToDouble(bytes, 0);
#endif
		}

        public override async Task<byte[]> ReadBinaryAsync()
		{
            int size = await ReadI32Async();
			byte[] buf = new byte[size];
			trans.ReadAll(buf, 0, size);
			return buf;
		}
		private  string ReadStringBody(int size)
		{
			byte[] buf = new byte[size];
			trans.ReadAll(buf, 0, size);
			return Encoding.UTF8.GetString(buf, 0, buf.Length);
		}

		private int ReadAll(byte[] buf, int off, int len)
		{
			return trans.ReadAll(buf, off, len);
		}

		#endregion
	}
}
