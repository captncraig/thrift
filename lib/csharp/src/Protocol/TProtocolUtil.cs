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

namespace Thrift.Protocol
{
	public static class TProtocolUtil
	{
	    public static void Skip(TProtocol prot, TType type)
	    {
	        SkipAsync(prot, type).Wait();
	    }

		public static async Task SkipAsync(TProtocol prot, TType type)
		{
			switch (type)
			{
				case TType.Bool:
					await prot.ReadBoolAsync();
					break;
				case TType.Byte:
					await prot.ReadByteAsync();
					break;
				case TType.I16:
                    await prot.ReadI16Async();
					break;
				case TType.I32:
                    await prot.ReadI32Async();
					break;
				case TType.I64:
                    await prot.ReadI64Async();
					break;
				case TType.Double:
                    await prot.ReadDoubleAsync();
					break;
				case TType.String:
					// Don't try to decode the string, just skip it.
                    await prot.ReadBinaryAsync();
					break;
				case TType.Struct:
                    await prot.ReadStructBeginAsync();
					while (true)
					{
                        TField field = await prot.ReadFieldBeginAsync();
						if (field.Type == TType.Stop)
						{
							break;
						}
                        await SkipAsync(prot, field.Type);
                        await prot.ReadFieldEndAsync();
					}
                    await prot.ReadStructEndAsync();
					break;
				case TType.Map:
                    TMap map = await prot.ReadMapBeginAsync();
					for (int i = 0; i < map.Count; i++)
					{
                        await SkipAsync(prot, map.KeyType);
                        await SkipAsync(prot, map.ValueType);
					}
                    await prot.ReadMapEndAsync();
					break;
				case TType.Set:
                    TSet set = await prot.ReadSetBeginAsync();
					for (int i = 0; i < set.Count; i++)
					{
                        await SkipAsync(prot, set.ElementType);
					}
                    await prot.ReadSetEndAsync();
					break;
				case TType.List:
                    TList list = await prot.ReadListBeginAsync();
					for (int i = 0; i < list.Count; i++)
					{
                        await SkipAsync(prot, list.ElementType);
					}
                    await prot.ReadListEndAsync();
					break;
			}
		}
	}
}
