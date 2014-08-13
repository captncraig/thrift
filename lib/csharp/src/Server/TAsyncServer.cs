using System;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace Thrift.Server
{
    public class TAsyncServer : TServer
    {
        public TAsyncServer(TAsyncProcessor processor, TServerTransport serverTransport) : base(processor, serverTransport)
        {
        }

        public TAsyncServer(TAsyncProcessor processor, TServerTransport serverTransport, LogDelegate logDelegate)
            : base(processor, serverTransport, logDelegate)
        {
        }

        public TAsyncServer(TAsyncProcessor processor, TServerTransport serverTransport, TTransportFactory transportFactory)
            : base(processor, serverTransport, transportFactory)
        {
        }

        public TAsyncServer(TAsyncProcessor processor, TServerTransport serverTransport, TTransportFactory transportFactory, TProtocolFactory protocolFactory)
            : base(processor, serverTransport, transportFactory, protocolFactory)
        {
        }

        public TAsyncServer(TAsyncProcessor processor, TServerTransport serverTransport, TTransportFactory inputTransportFactory, TTransportFactory outputTransportFactory, TProtocolFactory inputProtocolFactory, TProtocolFactory outputProtocolFactory, LogDelegate logDelegate)
            : base(processor, serverTransport, inputTransportFactory, outputTransportFactory, inputProtocolFactory, outputProtocolFactory, logDelegate)
        {
        }

        private bool _stop;
        private TAsyncProcessor _asyncProcessor;

        public override void Serve()
        {
            _asyncProcessor = (TAsyncProcessor) processor;
            //Run on thread pool thread to guarantee lack of synchronization context,
            //regardless of hosting environment. This way we never need to ConfigureAwait anywhere in the server.
            Task.Run(() =>
            {
                try
                {
                    serverTransport.Listen();
                }
                catch (TTransportException ttx)
                {
                    logDelegate("Error, could not listen on ServerTransport: " + ttx);
                    return;
                }
                if (serverEventHandler != null)
                    serverEventHandler.preServe();

                while (!_stop)
                {
                    TTransport client = serverTransport.Accept();
                    //Exceptions from this are gone. Should be caught and logged internally
                    HandleConnectionAsync(client);
                }
            });
        }

        private async Task HandleConnectionAsync(TTransport client)
        {
            //yield thread to let server listen again.
            //This connection will be resumed from the thread pool.
            await Task.Yield();

            TTransport inputTransport = null;
            TTransport outputTransport = null;
            TProtocol inputProtocol = null;
            TProtocol outputProtocol = null;
            Object connectionContext = null;
            try
            {
                inputTransport = inputTransportFactory.GetTransport(client);
                outputTransport = outputTransportFactory.GetTransport(client);
                inputProtocol = inputProtocolFactory.GetProtocol(inputTransport);
                outputProtocol = outputProtocolFactory.GetProtocol(outputTransport);

                if (serverEventHandler != null)
                    connectionContext = serverEventHandler.createContext(inputProtocol, outputProtocol);

                while (true)
                {
                    if (serverEventHandler != null)
                        serverEventHandler.processContext(connectionContext, inputTransport);
                    if (!await _asyncProcessor.ProcessAsync(inputProtocol, outputProtocol))
                        break;
                }
            }
            catch (TTransportException)
            {
                //Usually a client disconnect, expected
            }
            catch (Exception x)
            {
                //Unexpected
                logDelegate("Error: " + x);
            }

            //Fire deleteContext server event after client disconnects
            if (serverEventHandler != null)
                serverEventHandler.deleteContext(connectionContext, inputProtocol, outputProtocol);

            //Close transports
            if (inputTransport != null)
                inputTransport.Close();
            if (outputTransport != null)
                outputTransport.Close();
        }

        public override void Stop()
        {
            _stop = true;
            serverTransport.Close();
        }
    }
}