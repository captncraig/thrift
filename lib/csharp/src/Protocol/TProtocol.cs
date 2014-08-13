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

        public abstract Task<TMessage> ReadMessageBeginAsync();
        public abstract Task ReadMessageEndAsync();
        public abstract Task<TStruct> ReadStructBeginAsync();
        public abstract Task ReadStructEndAsync();
        public abstract Task<TField> ReadFieldBeginAsync();
        public abstract Task ReadFieldEndAsync();
        public abstract Task<TMap> ReadMapBeginAsync();
        public abstract Task ReadMapEndAsync();
        public abstract Task<TList> ReadListBeginAsync();
        public abstract Task ReadListEndAsync();
        public abstract Task<TSet> ReadSetBeginAsync();
        public abstract Task ReadSetEndAsync();
        public abstract Task<sbyte> ReadByteAsync();
        public abstract Task<bool> ReadBoolAsync();
        public abstract Task<short> ReadI16Async();
        public abstract Task<int> ReadI32Async();
        public abstract Task<long> ReadI64Async();
        public abstract Task<double> ReadDoubleAsync();
        public abstract Task<byte[]> ReadBinaryAsync();
        public virtual async Task<string> ReadStringAsync()
        {
            var buf = await ReadBinaryAsync();
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }

        // Synchronous methods preserved as callthroughs to async methods.

        public void WriteMessageBegin(TMessage message)
        {
            WriteMessageBeginAsync(message).Wait();
        }
        public void WriteMessageEnd()
        {
            WriteMessageEndAsync().Wait();
        }
        public void WriteStructBegin(TStruct struc)
        {
            WriteStructBeginAsync(struc).Wait();
        }
        public void WriteStructEnd()
        {
            WriteStructEndAsync().Wait();
        }
        public void WriteFieldBegin(TField field)
        {
            WriteFieldBeginAsync(field).Wait();
        }
        public void WriteFieldEnd()
        {
            WriteFieldEndAsync().Wait();
        }
        public void WriteFieldStop()
        {
            WriteFieldStopAsync().Wait();
        }
        public void WriteMapBegin(TMap map)
        {
            WriteMapBeginAsync(map).Wait();
        }
        public void WriteMapEnd()
        {
            WriteMapEndAsync().Wait();
        }
        public void WriteListBegin(TList list)
        {
            WriteListBeginAsync(list).Wait();
        }
        public void WriteListEnd()
        {
            WriteListEndAsync().Wait();
        }
        public void WriteSetBegin(TSet set)
        {
            WriteSetBeginAsync(set).Wait();
        }
        public void WriteSetEnd()
        {
            WriteSetEndAsync().Wait();
        }
        public void WriteBool(bool b)
        {
            WriteBoolAsync(b).Wait();
        }
        public void WriteByte(sbyte b)
        {
            WriteByteAsync(b).Wait();
        }
        public void WriteI16(short i16)
        {
            WriteI16Async(i16).Wait();
        }
        public void WriteI32(int i32)
        {
            WriteI32Async(i32).Wait();
        }
        public void WriteI64(long i64)
        {
            WriteI64Async(i64).Wait();
        }
        public void WriteDouble(double d)
        {
            WriteDoubleAsync(d).Wait();
        }
        public void WriteString(string s)
        {
            WriteStringAsync(s).Wait();
        }
        public void WriteBinary(byte[] b)
        {
            WriteBinaryAsync(b).Wait();
        }
        public TMessage ReadMessageBegin()
        {
            return ReadMessageBeginAsync().Result;
        }
        public void ReadMessageEnd()
        {
            ReadMessageEndAsync().Wait();
        }
        public TStruct ReadStructBegin()
        {
            return ReadStructBeginAsync().Result;
        }
        public void ReadStructEnd()
        {
            ReadStructEndAsync().Wait();
        }
        public TField ReadFieldBegin()
        {
            return ReadFieldBeginAsync().Result;
        }
        public void ReadFieldEnd()
        {
            ReadFieldEndAsync().Wait();
        }
        public TMap ReadMapBegin()
        {
            return ReadMapBeginAsync().Result;
        }
        public void ReadMapEnd()
        {
            ReadMapEndAsync().Wait();
        }
        public TList ReadListBegin()
        {
            return ReadListBeginAsync().Result;
        }
        public void ReadListEnd()
        {
            ReadListEndAsync().Wait();
        }
        public TSet ReadSetBegin()
        {
            return ReadSetBeginAsync().Result;
        }
        public void ReadSetEnd()
        {
            ReadSetEndAsync().Wait();
        }
        public bool ReadBool()
        {
            return ReadBoolAsync().Result;
        }
        public sbyte ReadByte()
        {
            return ReadByteAsync().Result;
        }
        public short ReadI16()
        {
            return ReadI16Async().Result;
        }
        public int ReadI32()
        {
            return ReadI32Async().Result;
        }
        public long ReadI64()
        {
            return ReadI64Async().Result;
        }
        public double ReadDouble()
        {
            return ReadDoubleAsync().Result;
        }
        public string ReadString()
        {
            return ReadStringAsync().Result;
        }
        public byte[] ReadBinary()
        {
            return ReadBinaryAsync().Result;
        }

    }
}
