using Basics;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Grpc.Core.Metadata;

Console.WriteLine("Hello, World!");

//var factory = new StaticResolverFactory(add => new[] {
//new BalancerAddress("localhost", 5057),
//new BalancerAddress("localhost", 5058),
//});

//var services = new ServiceCollection();
//services.AddSingleton<ResolverFactory>(factory);

//var channel = GrpcChannel.ForAddress("static://localhost", new GrpcChannelOptions()
//{ 
//    Credentials = ChannelCredentials.Insecure,
//    ServiceConfig = new ServiceConfig {
//        LoadBalancingConfigs = {new RoundRobinConfig() }
//    },
//    ServiceProvider = services.BuildServiceProvider(),
//});

var retryPolicy = new MethodConfig { // use retry policy
    Names = {MethodName.Default },
    RetryPolicy = new RetryPolicy {
        MaxAttempts = 5,
        InitialBackoff = TimeSpan.FromSeconds(0.5),
        MaxBackoff = TimeSpan.FromSeconds(0.5),
        BackoffMultiplier = 1,
        RetryableStatusCodes = { StatusCode.Internal }
    }
};
var hedging = new MethodConfig { // use hedging policy to send multiple requests in parallel
    Names = { MethodName.Default },
    HedgingPolicy = new HedgingPolicy {
        MaxAttempts = 5,
        HedgingDelay = TimeSpan.FromSeconds(0.5),
        NonFatalStatusCodes = { StatusCode.Internal }
    }
};

var options = new GrpcChannelOptions {
    ServiceConfig = new ServiceConfig {
        MethodConfigs = { retryPolicy }
    }
};
using var channel = GrpcChannel.ForAddress("https://localhost:7078", options);

//create health check client
var healthClient = new Health.HealthClient(channel);
var healthResult = await healthClient.CheckAsync(new HealthCheckRequest());
Console.WriteLine($"{healthResult.Status}");

var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);
Unary(client);
//await ClientStreamingAsync(client);
//ServerStreaming(client);
//BiDirectionalStreaming(client);
Console.ReadLine();

void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client) {
    var metadata = new Metadata { { "grpc-accept-encoding", "gzip" } };
    var request = new Request{ Content = "Hello you!" };
    var response = client.Unary(request, headers: metadata);
    //var response = client.Unary(request, deadline: DateTime.UtcNow.AddMilliseconds(3));
    Console.WriteLine($"{response.Message}");
}

async Task ClientStreamingAsync(FirstServiceDefinition.FirstServiceDefinitionClient client) {
    var requestStream = client.ClientStream();
    for (int i = 0; i < 10; i++) {
        await requestStream.RequestStream.WriteAsync(new Request { Content = i.ToString() });
    }
    await requestStream.RequestStream.CompleteAsync();
    var response = await requestStream;
    Console.WriteLine($"{response.Message}");
}

async void ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client) {
    var cancellationTokenSource = new CancellationTokenSource();

    var metadata = new Metadata();
    metadata.Add(new Entry("my-first-key", "my-first-value"));
    metadata.Add(new Entry("my-second-key", "my-second-value"));

    try {        
        var request = new Request { Content = "Hello you!" };
        using var response = client.ServerStream(request, headers: metadata);
        // request cancellation after 3 seconds
        cancellationTokenSource.CancelAfter(3000);

        await foreach (var item in response.ResponseStream.ReadAllAsync(cancellationTokenSource.Token)) {
            Console.WriteLine($"{item.Message}");
            if (item.Message == "2") {
                cancellationTokenSource.Cancel();
            }
        }

        //var trailers = response.GetTrailers();
        //foreach (var trailer in trailers) {
        //    Console.WriteLine($"{trailer.Key} : {trailer.Value}");
        //}

    } catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) /*GPRC StatusCodes*/ {
        Console.WriteLine(ex.StatusCode);
    } catch(RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied) {
        
    }
}

async void BiDirectionalStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client) {
    var requestStream = client.BiDirectionalStream();
    for (int i = 0; i < 10; i++) {
        await requestStream.RequestStream.WriteAsync(new Request { Content = i.ToString() });
    }
   while (await requestStream.ResponseStream.MoveNext()) {
       var response = requestStream.ResponseStream.Current;
       Console.WriteLine($"{response.Message}");
   }
   await requestStream.RequestStream.CompleteAsync();
    
}