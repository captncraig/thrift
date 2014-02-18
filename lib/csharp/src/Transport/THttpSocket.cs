using System.Net;

namespace Thrift.Transport
{
    public class THttpSocket : TServerTransport
    {
        private readonly HttpListener _listener;

        public THttpSocket(int port)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:"+port);
        }

        public override void Listen()
        {
            _listener.Start();
        }

        public override void Close()
        {
            _listener.Stop();
        }

        protected override TTransport AcceptImpl()
        {
            var context = _listener.GetContext();
            return new TStreamTransport(context.Request.InputStream, context.Response.OutputStream);
        }
    }
}