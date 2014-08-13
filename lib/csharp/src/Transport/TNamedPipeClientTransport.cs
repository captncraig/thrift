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

using System.IO.Pipes;
using System.Threading.Tasks;

namespace Thrift.Transport
{
	public class TNamedPipeClientTransport : TTransport
	{
		private NamedPipeClientStream client;
		private string ServerName;
		private string PipeName;

		public TNamedPipeClientTransport(string pipe)
		{
			ServerName = ".";
			PipeName = pipe;
		}

		public TNamedPipeClientTransport(string server, string pipe)
		{
			ServerName = (server != "") ? server : ".";
			PipeName = pipe;
		}

		public override bool IsOpen
		{
			get { return client != null && client.IsConnected; }
		}

		public override Task OpenAsync()
		{
			if (IsOpen)
			{
				throw new TTransportException(TTransportException.ExceptionType.AlreadyOpen);
			}
			client = new NamedPipeClientStream(ServerName, PipeName, PipeDirection.InOut, PipeOptions.None);
            client.Connect();
		    return NoopTask; 
		}

		public override void Close()
		{
			if (client != null)
			{
				client.Close();
				client = null;
			}
		}

        public override Task<int> ReadAsync(byte[] buf, int off, int len)
		{
			if (client == null)
			{
				throw new TTransportException(TTransportException.ExceptionType.NotOpen);
			}

			return client.ReadAsync(buf, off, len);
		}

        public override Task WriteAsync(byte[] buf, int off, int len)
		{
			if (client == null)
			{
				throw new TTransportException(TTransportException.ExceptionType.NotOpen);
			}

			return client.WriteAsync(buf, off, len);
		}

		protected override void Dispose(bool disposing)
		{
			client.Dispose();
		}
	}
}