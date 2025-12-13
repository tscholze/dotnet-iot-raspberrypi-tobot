using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Tobot.Web.Hubs;

namespace Tobot.Web.Components.Pages
{
    /// <summary>
    /// A simple remote control component that exposes basic robot actions via buttons
    /// and an optional query parameter (<c>action</c>) for triggering actions by URL.
    /// </summary>
    public partial class Remote : IAsyncDisposable
    {
        /// <summary>
        /// The SignalR hub connection used to invoke server-side robot control methods.
        /// </summary>
        private HubConnection? _hubConnection;

        // NavigationManager is injected via Razor markup (@inject NavigationManager Navigation)
        // and generated into this partial class; no duplicate property needed here.

        /// <summary>
        /// Optional query parameter used to trigger an action on navigation.
        /// Accepted values: <c>forward</c>, <c>backward</c>, <c>stop</c>, <c>left</c>, <c>right</c>, <c>lighton</c>/<c>light-on</c>, <c>lightoff</c>/<c>light-off</c>.
        /// </summary>
        [SupplyParameterFromQuery(Name = "action")]
        public string? Action { get; set; }

        /// <summary>
        /// Initializes the SignalR connection and executes the query-parameter action if present.
        /// </summary>
        /// <returns>A task that completes when initialization and any triggered action finishes.</returns>
        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/tobothub"))
                .WithAutomaticReconnect()
                .Build();

            await _hubConnection.StartAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(Action))
            {
                await ExecuteAction(Action).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes the query-parameter action on parameter changes (e.g., when navigating with a new action).
        /// </summary>
        /// <returns>A task that completes when the action invocation finishes.</returns>
        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Action))
            {
                await ExecuteAction(Action).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a specific action by name.
        /// </summary>
        /// <param name="action">Action keyword to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecuteAction(string action)
        {
            switch (action.ToLowerInvariant())
            {
                case "forward":
                    await Forward().ConfigureAwait(false);
                    break;
                case "backward":
                    await Backward().ConfigureAwait(false);
                    break;
                case "stop":
                    await Stop().ConfigureAwait(false);
                    break;
                case "left":
                    await Left().ConfigureAwait(false);
                    break;
                case "right":
                    await Right().ConfigureAwait(false);
                    break;
                case "lighton":
                case "light-on":
                    await LightOn().ConfigureAwait(false);
                    break;
                case "lightoff":
                case "light-off":
                    await LightOff().ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Drives both motors forward at a fixed speed.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocation.</returns>
        private async Task Forward()
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorForward), 1, 50.0).ConfigureAwait(false);
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorForward), 2, 50.0).ConfigureAwait(false);
        }

        /// <summary>
        /// Drives both motors backward at a fixed speed.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocation.</returns>
        private async Task Backward()
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorBackward), 1, 50.0).ConfigureAwait(false);
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorBackward), 2, 50.0).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops both motors.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocation.</returns>
        private async Task Stop()
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync(nameof(TobotHub.StopAllMotors)).ConfigureAwait(false);
        }

        /// <summary>
        /// Turns left by reversing motor 1 and driving motor 2 forward.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocation.</returns>
        private async Task Left()
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorBackward), 1, 50.0).ConfigureAwait(false);
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorForward), 2, 50.0).ConfigureAwait(false);
        }

        /// <summary>
        /// Turns right by driving motor 1 forward and reversing motor 2.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocation.</returns>
        private async Task Right()
        {
            if (_hubConnection is null) return;
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorForward), 1, 50.0).ConfigureAwait(false);
            await _hubConnection.InvokeAsync(nameof(TobotHub.MoveMotorBackward), 2, 50.0).ConfigureAwait(false);
        }

        /// <summary>
        /// Turns all LEDs on by invoking the hub method for each LED.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocations.</returns>
        private async Task LightOn()
        {
            if (_hubConnection is null) return;
            for (int i = 1; i <= 4; i++)
            {
                await _hubConnection.InvokeAsync(nameof(TobotHub.TurnLedOn), i).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Turns all LEDs off by invoking the hub method for each LED.
        /// </summary>
        /// <returns>A task representing the asynchronous hub invocations.</returns>
        private async Task LightOff()
        {
            if (_hubConnection is null) return;
            for (int i = 1; i <= 4; i++)
            {
                await _hubConnection.InvokeAsync(nameof(TobotHub.TurnLedOff), i).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Disposes the hub connection when the component is torn down.
        /// </summary>
        /// <returns>A task representing the asynchronous disposal operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
