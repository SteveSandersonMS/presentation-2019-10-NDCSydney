using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Grpc.Core;

namespace BlazorMart.Client
{
    // This is a quickly hacked-together proof of concept of a CallInvoker that speaks gRPC-Web
    // It only works in the specific scenarios used for this demo. It should not be expected to
    // work in other cases (for example, it doesn't understand error responses).
    // It's probably not very difficult to clean this up and support the whole of gRPC-Web, but
    // I don't need to do that.

    public class GrpcWebCallInvoker : CallInvoker
    {
        private HttpClient httpClient;

        public GrpcWebCallInvoker(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            throw new System.NotImplementedException();
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            throw new System.NotImplementedException();
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            throw new System.NotImplementedException();
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = new GrpcWebCall<TRequest, TResponse>(httpClient, method, options, request);
            var response = call.GetResponseAsync();

            return new AsyncUnaryCall<TResponse>(
                responseAsync: response,
                responseHeadersAsync: null,
                getStatusFunc: null,
                getTrailersFunc: null,
                disposeAction: null); ;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = AsyncUnaryCall(method, host, options, request);
            return call.ResponseAsync.GetAwaiter().GetResult();
        }

        private class GrpcWebCall<TRequest, TResponse>
        {
            private readonly HttpClient httpClient;
            private readonly Method<TRequest, TResponse> method;
            private readonly CallOptions options;
            private readonly TRequest request;
            private Task<HttpResponseMessage> responseTask;

            public GrpcWebCall(HttpClient httpClient, Method<TRequest, TResponse> method, CallOptions options, TRequest request)
            {
                this.httpClient = httpClient;
                this.method = method;
                this.options = options;
                this.request = request;
            }

            public async Task<TResponse> GetResponseAsync()
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, method.FullName);
                httpRequest.Headers.TryAddWithoutValidation("Accept", "application/grpc-web");

                var serializer = method.RequestMarshaller.ContextualSerializer;
                var serializationContext = new DefaultSerializationContext();
                serializer(request, serializationContext);

                var payloadWithHeader = new byte[serializationContext.Payload.Length + 5];
                serializationContext.Payload.CopyTo(payloadWithHeader, 5);

                // Bytes 1-4 (zero-indexed) should be the payload length
                WriteInt32BigEndian(serializationContext.Payload.Length, payloadWithHeader, 1);

                //httpRequest.Content = new StringContent(Convert.ToBase64String(payloadWithHeader));
                httpRequest.Content = new ByteArrayContent(payloadWithHeader);
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/grpc-web");
                responseTask = httpClient.SendAsync(httpRequest);
                var response = await responseTask;
                var responseBody = await response.Content.ReadAsByteArrayAsync();
                if (responseBody == null || responseBody.Length == 0)
                {
                    return default;
                }


                var responseReader = new BinaryReader(new MemoryStream(responseBody));
                responseReader.ReadByte(); // Ignore

                var part1LengthBytes = responseReader.ReadBytes(4);
                Array.Reverse(part1LengthBytes);
                var part1Length = BitConverter.ToInt32(part1LengthBytes, 0);

                var part1Bytes = responseReader.ReadBytes(part1Length);
                var deserializationContext = new DefaultDeserializationContext();
                deserializationContext.SetPayload(part1Bytes);
                var deserializer = method.ResponseMarshaller.ContextualDeserializer;
                var result = deserializer(deserializationContext);
                return result;
            }

            private static void WriteInt32BigEndian(int value, byte[] buffer, int offset)
            {
                buffer[offset++] = (byte)(value >> 24);
                buffer[offset++] = (byte)(value >> 16);
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset] = (byte)value;
            }

            private class DefaultSerializationContext : SerializationContext
            {
                public byte[] Payload { get; set; }

                public override void Complete(byte[] payload)
                {
                    Payload = payload;
                }
            }

            private class DefaultDeserializationContext : DeserializationContext
            {
                private byte[] _payload;

                public void SetPayload(byte[] payload)
                {
                    _payload = payload;
                }

                public override byte[] PayloadAsNewBuffer()
                {
                    Debug.Assert(_payload != null, "Payload must be set.");
                    return _payload;
                }

                public override ReadOnlySequence<byte> PayloadAsReadOnlySequence()
                {
                    Debug.Assert(_payload != null, "Payload must be set.");
                    return new ReadOnlySequence<byte>(_payload);
                }

                public override int PayloadLength => _payload?.Length ?? 0;
            }
        }
    }
}
