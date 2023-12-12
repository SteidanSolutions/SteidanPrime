using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SteidanPrime.Models;
using SteidanPrime.Services.Common;
using SteidanPrime.Services.Gambling;
using SteidanPrime.Services.Markov;
using SteidanPrime.Services.Saveboard;

namespace SteidanPrime;

public class DiscordBotClient : IDiscordBotClient
{
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IInteractionHandler _interactionHandler;
        private readonly Settings _settings;
        private readonly IMarkovService _markovService;
        private readonly ISaveboardService _saveboardService;
        private readonly IGamblingService _gamblingService;
        private readonly ILogger<DiscordBotClient> _logger;


        public DiscordBotClient(DiscordSocketClient client,
            InteractionService interactionService,
            IInteractionHandler interactionHandler,
            IMarkovService markovService,
            ISaveboardService saveboardService,
            IGamblingService gamblingService,
            ILogger<DiscordBotClient> _logger,
            IOptions<Settings> settings)
        {
            _settings = settings.Value;
            _client = client;
            _interactionService = interactionService;
            _interactionHandler = interactionHandler;
            _markovService = markovService;
            _saveboardService = saveboardService;
            _gamblingService = gamblingService;
            this._logger = _logger;

            _client.Ready += ClientReady;
            
            if (!Directory.Exists("Resources"))
                Directory.CreateDirectory("Resources");

            if (!Directory.Exists("Resources/Dictionaries"))
                Directory.CreateDirectory("Resources/Dictionaries");
        }
        
        public async Task RunAsync()
        {
            _logger.LogInformation("Started running.");

            if (IsSettingsEmpty())
            {
                const string exceptionMessage = "Settings token or prefix missing.";
                
                _logger.LogCritical(exceptionMessage);
                throw new ArgumentNullException("Settings.Token/Settings.Prefix", exceptionMessage);
            }

            await _client.LoginAsync(TokenType.Bot, _settings.Token);
            await _client.StartAsync();

            // Initialize auto-save timer
            var timer = new Timer
            {
                Interval = 10 * 60 * 1000,
                AutoReset = true    
            };

            timer.Elapsed += AutoSave;
            timer.Enabled = true;

            // Main loop
            while (true)
            {
            }
        }

        private async Task ClientReady()
        {
            _saveboardService.DeserializeSaveboard();
            _markovService.DeserializeDict();
            _markovService.FixMissingDictionaries();
            _gamblingService.DeserializePlayers();
            await _client.SetGameAsync(
                $"with {_markovService.GetTotalWords()} words for Markov chains | !help");

            try
            {
                await _interactionHandler.InitializeAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
            
            await _interactionService.RegisterCommandsGloballyAsync(true);
            //await _commands.RegisterCommandsToGuildAsync(149871097452429312, true);
            //await _commands.RegisterCommandsToGuildAsync(102375147515699200, true);
            //await _commands.RegisterCommandsToGuildAsync(1035712010836516925, true);

            _client.Log += LogAsync;
            _interactionService.Log += LogAsync;
        }

        private async void AutoSave(object source, ElapsedEventArgs e)
        {
            _markovService.SerializeDict();
            _saveboardService.SerializeSaveboard();
            _gamblingService.SerializePlayers();
            await _client.SetGameAsync(
                $"with {_markovService.GetTotalWords()} words for Markov chains");
            
            _logger.LogInformation("Dictionaries auto-saved.");
        }

        private bool IsSettingsEmpty()
        {
            return string.IsNullOrEmpty(_settings.Token) || string.IsNullOrEmpty(_settings.CommandPrefix);
        }
        
        private async Task LogAsync(LogMessage message)
        {
            await Task.Run(() =>
            {
                switch (message.Severity)
                {
                    case LogSeverity.Critical:
                        _logger.LogCritical(message.Message);
                        break;
                    case LogSeverity.Error:
                        _logger.LogError(message.Message);
                        break;
                    case LogSeverity.Warning:
                        _logger.LogWarning(message.Message);
                        break;
                    case LogSeverity.Info:
                        _logger.LogInformation(message.Message);
                        break;
                }
            });

            //await _client.GetGuild(1035712010836516925)
            //    .GetTextChannel(1047962360251416596)
            //    .SendMessageAsync($"{DateTime.Now,-19} [{message.Severity}] {message.Source}: {message.Message} {message.Exception}");
            //break;
        }
}