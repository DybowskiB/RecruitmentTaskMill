using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Backend.Interfaces;
using Backend.Services;
using Backend.Settings;

namespace BackendTests;

/// <summary>
/// Note: Only a single representative test is included due to limited time.
/// </summary>
public class DataRequestServiceTests
{
    [Test]
    public async Task CreateOrGetRequestAsync_When_ActiveRequestAlreadyExistsForClient_Should_ReturnExistingRequestId()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var requestQueueMock = new Mock<IRequestQueue>();

        var options = Options.Create(new DataGenerationOptions
        {
            DelaySeconds = 60,
            CacheMinutes = 5
        });

        var service = new DataRequestService(memoryCache, requestQueueMock.Object, options);

        var clientId = "client-123";
        var cancellationToken = CancellationToken.None;

        // Act
        var firstRequestId = await service.CreateOrGetRequestAsync(clientId, cancellationToken);
        var secondRequestId = await service.CreateOrGetRequestAsync(clientId, cancellationToken);

        // Assert
        Assert.That(secondRequestId, Is.EqualTo(firstRequestId));

        requestQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}