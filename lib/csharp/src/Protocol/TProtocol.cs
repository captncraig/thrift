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
	public abstract class TProtocol : IDisposable
	{
		protected TTransport trans;

        //For task based methods that do nothing, this is a convient task to return.
        protected readonly Task NoopTask = Task.FromResult(0);

		protected TProtocol(TTransport trans)
		{
			this.trans = trans;
		}

		public TTransport Transport
		{
			get { return trans; }
		}

        #region " IDisposable Support "
        private bool _IsDisposed;

        // IDisposable
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    if (trans is IDisposable)
                        (trans as IDisposable).Dispose();
                }
            }
            _IsDisposed = true;
        }
        #endregion

        public abstract Task WriteMessageBeginAsync(TMessage message);
        public abstract Task WriteMessageEndAsync();
	    public abstract Task WriteStructBeginAsync(TStruct struc);
        public abstract Task WriteStructEndAsync();
        public abstract Task WriteFieldBeginAsync(TField field);
        public abstract Task WriteFieldEndAsync();
        public abstract Task WriteFieldStopAsync();
        public abstract Task WriteMapBeginAsync(TMap map);
        public abstract Task WriteMapEndAsync();
        public abstract Task WriteListBeginAsync(TList list);
        public abstract Task WriteListEndAsync();
        public abstract Task WriteSetBeginAsync(TSet set);
        public abstract Task WriteSetEndAsync();
        public abstract Task WriteBoolAsync(bool b);
        public abstract Task WriteByteAsync(sbyte b);
        public abstract Task WriteI16Async(short i16);
        public abstract Task WriteI32Async(int i32);
        public abstract Task WriteI64Async(long i64);
        public abstract Task WriteDoubleAsync(double d);
        public virtual Task WriteStringAsync(string s)
        {
            return WriteBinaryAsync(Encoding.UTF8.GetBytes(s));
        }
        public abstract Task WriteBinaryAsync(byte[] b);

        public void WriteMessageBeginZZZ(TMessage message)
        {
            WriteMessageBeginAsync(message).Wait();
        }
        public void WriteMessageEndZZZ()
        {
            WriteMessageEndAsync().Wait();
        }
        public void WriteStructBeginZZZ(TStruct struc)
        {
            WriteStructBeginAsync(struc).Wait();
        }
	    public void WriteStructEndZZZ()
	    {
	        WriteStructEndAsync().Wait();
	    }
        public void WriteFieldBeginZZZ(TField field)
        {
            WriteFieldBeginAsync(field).Wait();
        }
        public void WriteFieldEndZZZ()
        {
            WriteFieldEndAsync().Wait();
        }
        public void WriteFieldStopZZZ()
        {
            WriteFieldStopAsync().Wait();
        }
        public void WriteMapBeginZZZ(TMap map)
        {
            WriteMapBeginAsync(map).Wait();
        }
        public void WriteMapEndZZZ()
        {
            WriteMapEndAsync().Wait();
        }
        public void WriteListBeginZZZ(TList list)
        {
            WriteListBeginAsync(list).Wait();
        }
        public void WriteListEndZZZ()
        {
            WriteListEndAsync().Wait();
        }
        public void WriteSetBeginZZZ(TSet set)
        {
            WriteSetBeginAsync(set).Wait();
        }
        public void WriteSetEndZZZ()
        {
            WriteSetEndAsync().Wait();
        }
        public void WriteBoolZZZ(bool b)
        {
            WriteBoolAsync(b).Wait();
        }
        public void WriteByteZZZ(sbyte b)
        {
            WriteByteAsync(b).Wait();
        }
        public void WriteI16ZZZ(short i16)
        {
            WriteI16Async(i16).Wait();
        }
        public void WriteI32ZZZ(int i32)
        {
            WriteI32Async(i32).Wait();
        }
        public void WriteI64ZZZ(long i64)
        {
            WriteI64Async(i64).Wait();
        }
        public void WriteDoubleZZZ(double d)
        {
            WriteDoubleAsync(d).Wait();
        }
		public void WriteStringZZZ(string s)
		{
		    WriteStringAsync(s).Wait();
		}
	    public void WriteBinaryZZZ(byte[] b)
	    {
	        WriteBinaryAsync(b).Wait();
	    }

		public abstract TMessage ReadMessageBegin();
		public abstract void ReadMessageEnd();
		public abstract TStruct ReadStructBegin();
		public abstract void ReadStructEnd();
		public abstract TField ReadFieldBegin();
		public abstract void ReadFieldEnd();
		public abstract TMap ReadMapBegin();
		public abstract void ReadMapEnd();
		public abstract TList ReadListBegin();
		public abstract void ReadListEnd();
		public abstract TSet ReadSetBegin();
		public abstract void ReadSetEnd();
		public abstract bool ReadBool();
		public abstract sbyte ReadByte();
		public abstract short ReadI16();
		public abstract int ReadI32();
		public abstract long ReadI64();
		public abstract double ReadDouble();
		public virtual string ReadString() {
            var buf = ReadBinary();
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }
		public abstract byte[] ReadBinary();
	}
}
