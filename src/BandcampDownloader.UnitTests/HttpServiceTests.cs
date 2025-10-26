using BandcampDownloader.Net;
using BandcampDownloader.Settings;
using Moq;

namespace BandcampDownloader.UnitTests;

public sealed class HttpServiceTests
{
    private HttpService _sut;
    private Mock<ISettingsService> _settingsService;
    private Mock<IHttpClientFactory> _httpClientFactory;

    [SetUp]
    public void Setup()
    {
        _settingsService = new Mock<ISettingsService>();
        _httpClientFactory = new Mock<IHttpClientFactory>();

        _sut = new HttpService(_settingsService.Object, _httpClientFactory.Object);
    }

    [Test]
    public void CreateHttpClient_Returns_HttpClientWithUserAgentHeader()
    {
        // Arrange
        _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        // Act
        var httpClient = _sut.CreateHttpClient();

        // Assert
        Assert.That(httpClient.DefaultRequestHeaders.UserAgent, Is.Not.Null);
    }
}
