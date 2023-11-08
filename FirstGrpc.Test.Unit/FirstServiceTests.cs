using System;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using FirstGrpc.Services;
using Basics;
using FirstGrpc.Test.Unit.Helpers;
using Grpc.Core;
using FluentAssertions;

namespace MyProject.Tests
{
    public class FirstServiceTests
    {
        private readonly IFirstService sut;

        public FirstServiceTests()
        {
            sut = new FirstService();
        }

        [Fact]
        public async void Unary_WhenCalled_ReturnsResponse()
        {
            // Arrange
            var request = new Request { Content = "Hello" };

            // use private construcotr to create new TestServerCallContext
            var context = new TestServerCallContext(new Metadata(), new System.Threading.CancellationToken());
            

            var expectedResponse = new Response { Message = $"{request.Content} from server" };

            // Act
            var actualResponse = await sut.Unary(request, context);

            // Assert
            actualResponse.Should().BeEquivalentTo(expectedResponse);
        }

    }
}
