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
using System.Threading.Tasks;

namespace Thrift.Transport
{ 
  public class TFramedTransport : TTransport, IDisposable
	{
		protected TTransport transport = null;
		protected MemoryStream writeBuffer;
		protected MemoryStream readBuffer = null;

		private const int header_size = 4;
		private static byte[] header_dummy = new byte[header_size]; // used as header placeholder while initilizing new write buffer

		public class Factory : TTransportFactory
		{
			public override TTransport GetTransport(TTransport trans)
			{
				return new TFramedTransport(trans);
			}
		}

		protected TFramedTransport()
		{
			InitWriteBuffer();
		}

		public TFramedTransport(TTransport transport) : this()
		{
			this.transport = transport;
		}

		public override Task OpenAsync()
		{
			return transport.OpenAsync();
		}

		public override bool IsOpen
		{
			get
			{
				return transport.IsOpen;
			}
		}

		public override void Close()
		{
			transport.Close();
		}

        public override async Task<int> ReadAsync(byte[] buf, int off, int len)
		{
			if (readBuffer != null)
			{
				int got = await readBuffer.ReadAsync(buf, off, len);
				if (got > 0)
				{
					return got;
				}
			}

			// Read another frame of data
			await ReadFrameAsync();

			return await readBuffer.ReadAsync(buf, off, len);
		}

		private async Task ReadFrameAsync()
		{
			byte[] i32rd = new byte[header_size];
			await transport.ReadAllAsync(i32rd, 0, header_size);
			int size = DecodeFrameSize(i32rd);

			byte[] buff = new byte[size];
			await transport.ReadAllAsync(buff, 0, size);
			readBuffer = new MemoryStream(buff);
		}

        public override Task WriteAsync(byte[] buf, int off, int len)
		{
			return writeBuffer.WriteAsync(buf, off, len);
		}

        public override async Task FlushAsync()
		{
			byte[] buf = writeBuffer.GetBuffer();
			int len = (int)writeBuffer.Length;
			int data_len = len - header_size;
			if ( data_len < 0 )
				throw new System.InvalidOperationException (); // logic error actually

			InitWriteBuffer();

			// Inject message header into the reserved buffer space
			EncodeFrameSize(data_len,ref buf);

			// Send the entire message at once
			await transport.WriteAsync(buf, 0, len);

			await transport.FlushAsync();
		}

		private void InitWriteBuffer ()
		{
			// Create new buffer instance
			writeBuffer = new MemoryStream(1024);

			// Reserve space for message header to be put right before sending it out
			writeBuffer.Write ( header_dummy, 0, header_size );
		}
		
		private static void EncodeFrameSize(int frameSize, ref byte[] buf) 
		{
			buf[0] = (byte)(0xff & (frameSize >> 24));
			buf[1] = (byte)(0xff & (frameSize >> 16));
			buf[2] = (byte)(0xff & (frameSize >> 8));
			buf[3] = (byte)(0xff & (frameSize));
		}
		
		private static int DecodeFrameSize(byte[] buf)
		{
			return 
				((buf[0] & 0xff) << 24) |
				((buf[1] & 0xff) << 16) |
				((buf[2] & 0xff) <<  8) |
				((buf[3] & 0xff));
		}


		#region " IDisposable Support "
		private bool _IsDisposed;
		
		// IDisposable
		protected override void Dispose(bool disposing)
		{
			if (!_IsDisposed)
			{
				if (disposing)
				{
					if (readBuffer != null)
						readBuffer.Dispose();
				}
			}
			_IsDisposed = true;
		}
		#endregion
	}
}
