﻿using Grpc.Core;
using Grpc.Core.Interceptors;

namespace FirstGrpc.Interceptors {
    public class ServerLoggingInterceptor : Interceptor {
        private readonly ILogger<ServerLoggingInterceptor> _logger;

        public ServerLoggingInterceptor(ILogger<ServerLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<
            TRequest, TResponse>(TRequest request, 
            ServerCallContext context, 
            UnaryServerMethod<TRequest, TResponse> continuation) {
            try {
                //_logger.LogInformation($"Received {request} from {context.Method}");
                //return base.UnaryServerHandler(request, context, continuation);
                _logger.LogInformation("Server interceptor: " + context.Method);                
                return await continuation(request, context);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error thrown by {context.Method}");
                throw;
            }
                
        }

    }
}
