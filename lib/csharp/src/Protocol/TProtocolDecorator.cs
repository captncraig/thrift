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

        public override TMessage ReadMessageBegin()
        {
            return WrappedProtocol.ReadMessageBegin();
        }

        public override void ReadMessageEnd()
        {
            WrappedProtocol.ReadMessageEnd();
        }

        public override TStruct ReadStructBegin()
        {
            return WrappedProtocol.ReadStructBegin();
        }

        public override void ReadStructEnd()
        {
            WrappedProtocol.ReadStructEnd();
        }

        public override TField ReadFieldBegin()
        {
            return WrappedProtocol.ReadFieldBegin();
        }

        public override void ReadFieldEnd()
        {
            WrappedProtocol.ReadFieldEnd();
        }

        public override TMap ReadMapBegin()
        {
            return WrappedProtocol.ReadMapBegin();
        }

        public override void ReadMapEnd()
        {
            WrappedProtocol.ReadMapEnd();
        }

        public override TList ReadListBegin()
        {
            return WrappedProtocol.ReadListBegin();
        }

        public override void ReadListEnd()
        {
            WrappedProtocol.ReadListEnd();
        }

        public override TSet ReadSetBegin()
        {
            return WrappedProtocol.ReadSetBegin();
        }

        public override void ReadSetEnd()
        {
            WrappedProtocol.ReadSetEnd();
        }

        public override bool ReadBool()
        {
            return WrappedProtocol.ReadBool();
        }

        public override sbyte ReadByte()
        {
            return WrappedProtocol.ReadByte();
        }

        public override short ReadI16()
        {
            return WrappedProtocol.ReadI16();
        }

        public override int ReadI32()
        {
            return WrappedProtocol.ReadI32();
        }

        public override long ReadI64()
        {
            return WrappedProtocol.ReadI64();
        }

        public override double ReadDouble()
        {
            return WrappedProtocol.ReadDouble();
        }

        public override String ReadString()
        {
            return WrappedProtocol.ReadString();
        }

        public override byte[] ReadBinary()
        {
            return WrappedProtocol.ReadBinary();
        }
    }

}
