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
using System.Threading.Tasks;

namespace Thrift.Transport
{
	public abstract class TTransport : IDisposable
	{
		public abstract bool IsOpen
		{
			get;
		}

		public bool Peek()
		{
			return IsOpen;
		}

	    public void Open()
	    {
	        OpenAsync().Wait();
	    }
		public abstract Task OpenAsync();

		public abstract void Close();

	    public int Read(byte[] buf, int off, int len)
	    {
	        return ReadAsync(buf, off, len).Result;
	    }
		public abstract Task<int> ReadAsync(byte[] buf, int off, int len);

	    public int ReadAll(byte[] buf, int off, int len)
	    {
	        return ReadAllAsync(buf, off, len).Result;
	    }
		public async Task<int> ReadAllAsync(byte[] buf, int off, int len)
		{
			int got = 0;

		    while (got < len)
			{
				int ret = await ReadAsync(buf, off + got, len - got);
				if (ret <= 0)
				{
					throw new TTransportException(
						TTransportException.ExceptionType.EndOfFile,
						"Cannot read, Remote side has closed");
				}
				got += ret;
			}

			return got;
		}

	    public void Write(byte[] buf)
	    {
	        WriteAsync(buf).Wait();
	    }
		public virtual Task WriteAsync(byte[] buf) 
		{
			return WriteAsync (buf, 0, buf.Length);
		}

	    public void Write(byte[] buf, int off, int len)
	    {
	        WriteAsync(buf, off, len).Wait();
	    }
		public abstract Task WriteAsync(byte[] buf, int off, int len);

		public void Flush()
		{
		    FlushAsync().Wait();
		}

	    public virtual async Task FlushAsync()
	    {
	        
	    }
        
        [Obsolete("Use FlushAsync instead")]
        public virtual IAsyncResult BeginFlush(AsyncCallback callback, object state)
        {
            throw new TTransportException(
                TTransportException.ExceptionType.Unknown,
                "Asynchronous operations are not supported by this transport.");
        }

        [Obsolete("Use FlushAsync instead")]
        public virtual void EndFlush(IAsyncResult asyncResult)
        {
            throw new TTransportException(
                TTransportException.ExceptionType.Unknown,
                "Asynchronous operations are not supported by this transport.");
        }

		#region " IDisposable Support "
		// IDisposable
		protected abstract void Dispose(bool disposing);

		public void Dispose()
		{
			// Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
