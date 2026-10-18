using System.Net;
using System.Net.Http.Json;
using Tobot.Client;
using Tobot.Shared;
using Tobot.Shared.Models;

namespace Tobot.Tests;

/// <summary>
/// Tests for <see cref="TobotApiClient"/> using a fake HTTP handler.
/// </summary>
public class TobotApiClientTests
{
	/// <summary>
	/// Fake handler returning a canned response and recording the request.
	/// </summary>
	/// <param name="responder">Creates the response for a request.</param>
	private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
	{
		/// <summary>
		/// The last request received.
		/// </summary>
		public HttpRequestMessage? LastRequest { get; private set; }

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			LastRequest = request;
			return Task.FromResult(responder(request));
		}
	}

	/// <summary>
	/// Creates a client over the given handler.
	/// </summary>
	/// <param name="handler">The fake handler.</param>
	/// <returns>A client.</returns>
	private static TobotApiClient CreateClient(FakeHandler handler) =>
		new(new HttpClient(handler) { BaseAddress = new Uri("http://tobot.test") });

	/// <summary>
	/// A 200 response is deserialized.
	/// </summary>
	[Fact]
	public async Task GetDistanceAsync_ReturnsReading()
	{
		var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new DistanceReading(42.5)) });
		var reading = await CreateClient(handler).GetDistanceAsync(3);

		Assert.Equal(42.5, reading?.DistanceCm);
		Assert.Equal($"{TobotRoutes.Distance}?samples=3", handler.LastRequest!.RequestUri!.PathAndQuery);
	}

	/// <summary>
	/// A 503 means the sensor could not be read.
	/// </summary>
	[Fact]
	public async Task GetDistanceAsync_Returns_Null_On503()
	{
		var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
		Assert.Null(await CreateClient(handler).GetDistanceAsync());
	}

	/// <summary>
	/// Drive posts to the drive route and reports success.
	/// </summary>
	[Fact]
	public async Task DriveAsync_PostsCommand()
	{
		var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
		bool ok = await CreateClient(handler).DriveAsync(new DriveCommand(10, -10));

		Assert.True(ok);
		Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
		Assert.Equal(TobotRoutes.Drive, handler.LastRequest.RequestUri!.AbsolutePath);
	}
}
