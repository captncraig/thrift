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
using System.Collections.Generic;

namespace Thrift.Protocol
{

    /**
     * TProtocolDecorator forwards all requests to an enclosed TProtocol instance, 
     * providing a way to author concise concrete decorator subclasses.  While it has 
     * no abstract methods, it is marked abstract as a reminder that by itself, 
     * it does not modify the behaviour of the enclosed TProtocol.
     *
     * See p.175 of Design Patterns (by Gamma et al.)
     * See TMultiplexedProtocol
     */
    public abstract class TProtocolDecorator : TProtocol
    {
        private TProtocol WrappedProtocol;

        /**
         * Encloses the specified protocol.
         * @param protocol All operations will be forward to this protocol.  Must be non-null.
         */
        public TProtocolDecorator(TProtocol protocol)
            : base(protocol.Transport)
        {

            WrappedProtocol = protocol;
        }

        public override Task WriteMessageBeginAsync(TMessage tMessage)
        {
            return WrappedProtocol.WriteMessageBeginAsync(tMessage);
        }

        public override Task WriteMessageEndAsync()
        {
            return WrappedProtocol.WriteMessageEndAsync();
        }

        public override Task WriteStructBeginAsync(TStruct tStruct)
        {
            return WrappedProtocol.WriteStructBeginAsync(tStruct);
        }

        public override Task WriteStructEndAsync()
        {
            return WrappedProtocol.WriteStructEndAsync();
        }

        public override Task WriteFieldBeginAsync(TField tField)
        {
            return WrappedProtocol.WriteFieldBeginAsync(tField);
        }

        public override Task WriteFieldEndAsync()
        {
            return WrappedProtocol.WriteFieldEndAsync();
        }

        public override Task WriteFieldStopAsync()
        {
            return WrappedProtocol.WriteFieldStopAsync();
        }

        public override Task WriteMapBeginAsync(TMap tMap)
        {
            return WrappedProtocol.WriteMapBeginAsync(tMap);
        }

        public override Task WriteMapEndAsync()
        {
            return WrappedProtocol.WriteMapEndAsync();
        }

        public override Task WriteListBeginAsync(TList tList)
        {
            return WrappedProtocol.WriteListBeginAsync(tList);
        }

        public override Task WriteListEndAsync()
        {
            return WrappedProtocol.WriteListEndAsync();
        }

        public override Task WriteSetBeginAsync(TSet tSet)
        {
            return WrappedProtocol.WriteSetBeginAsync(tSet);
        }

        public override Task WriteSetEndAsync()
        {
            return WrappedProtocol.WriteSetEndAsync();
        }

        public override Task WriteBoolAsync(bool b)
        {
            return WrappedProtocol.WriteBoolAsync(b);
        }

        public override Task WriteByteAsync(sbyte b)
        {
            return WrappedProtocol.WriteByteAsync(b);
        }

        public override Task WriteI16Async(short i)
        {
            return WrappedProtocol.WriteI16Async(i);
        }

        public override Task WriteI32Async(int i)
        {
            return WrappedProtocol.WriteI32Async(i);
        }

        public override Task WriteI64Async(long l)
        {
            return WrappedProtocol.WriteI64Async(l);
        }

        public override Task WriteDoubleAsync(double v)
        {
            return WrappedProtocol.WriteDoubleAsync(v);
        }

        public override Task WriteStringAsync(String s)
        {
            return WrappedProtocol.WriteStringAsync(s);
        }

        public override Task WriteBinaryAsync(byte[] bytes)
        {
            return WrappedProtocol.WriteBinaryAsync(bytes);
        }

        public override Task<TMessage> ReadMessageBeginAsync()
        {
            return WrappedProtocol.ReadMessageBeginAsync();
        }

        public override Task ReadMessageEndAsync()
        {
            return WrappedProtocol.ReadMessageEndAsync();
        }

        public override Task<TStruct> ReadStructBeginAsync()
        {
            return WrappedProtocol.ReadStructBeginAsync();
        }

        public override Task ReadStructEndAsync()
        {
            return WrappedProtocol.ReadStructEndAsync();
        }

        public override Task<TField> ReadFieldBeginAsync()
        {
            return WrappedProtocol.ReadFieldBeginAsync();
        }

        public override Task ReadFieldEndAsync()
        {
            return WrappedProtocol.ReadFieldEndAsync();
        }

        public override Task<TMap> ReadMapBeginAsync()
        {
            return WrappedProtocol.ReadMapBeginAsync();
        }

        public override Task ReadMapEndAsync()
        {
            return WrappedProtocol.ReadMapEndAsync();
        }

        public override Task<TList> ReadListBeginAsync()
        {
            return WrappedProtocol.ReadListBeginAsync();
        }

        public override Task ReadListEndAsync()
        {
            return WrappedProtocol.ReadListEndAsync();
        }

        public override Task<TSet> ReadSetBeginAsync()
        {
            return WrappedProtocol.ReadSetBeginAsync();
        }

        public override Task ReadSetEndAsync()
        {
            return WrappedProtocol.ReadSetEndAsync();
        }

        public override Task<bool> ReadBoolAsync()
        {
            return WrappedProtocol.ReadBoolAsync();
        }

        public override Task<sbyte> ReadByteAsync()
        {
            return WrappedProtocol.ReadByteAsync();
        }

        public override Task<short> ReadI16Async()
        {
            return WrappedProtocol.ReadI16Async();
        }

        public override Task<int> ReadI32Async()
        {
            return WrappedProtocol.ReadI32Async();
        }

        public override Task<long> ReadI64Async()
        {
            return WrappedProtocol.ReadI64Async();
        }

        public override Task<double> ReadDoubleAsync()
        {
            return WrappedProtocol.ReadDoubleAsync();
        }

        public override Task<String> ReadStringAsync()
        {
            return WrappedProtocol.ReadStringAsync();
        }

        public override Task<byte[]> ReadBinaryAsync()
        {
            return WrappedProtocol.ReadBinaryAsync();
        }
    }

}
