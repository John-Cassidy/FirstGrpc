using Grpc.Core;
using Grpc.Core.Interceptors;

namespace MVCClient.Interceptors {
    public class ClientLoggerInterceptor : Interceptor {

        private readonly ILogger<ClientLoggerInterceptor> logger;
        public ClientLoggerInterceptor(ILoggerFactory loggerFactory)
            => logger = loggerFactory.CreateLogger<ClientLoggerInterceptor>();

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation) {
            try {
                logger.LogInformation($"Sending {request} to {context.Method.FullName}");
                var response = continuation(request, context);
                logger.LogInformation($"Received {response} from {context.Method.FullName}");
                return response;
            } catch (RpcException e) {
                logger.LogError(e, $"Error calling {context.Method.FullName}");
                throw;
            }
            return base.BlockingUnaryCall(request, context, continuation);
        }

    }
}
