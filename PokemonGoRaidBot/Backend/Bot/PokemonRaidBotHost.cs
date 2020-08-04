using Botje.Core;
using Botje.Core.Loggers;
using Botje.Core.Utils;
using Botje.DB;
using Botje.Messaging;
using Botje.Messaging.PrivateConversation;
using Botje.Messaging.Telegram;
using Microsoft.Extensions.DependencyInjection;
using NGettext;
using Ninject;
using RaidBot.Backend.Bot.PokemonRaidBot;
using RaidBot.Backend.Bot.PokemonRaidBot.LocationAPI;
using RaidBot.Backend.Bot.PokemonRaidBot.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace RaidBot.Backend.Bot
{
    public class PokemonRaidBotHost : IPokemonRaidBotHost
    {
        private string _id = "8d91f20d-c832-43c1-8fe3-0fcbd37da3b6";

        private ISettingsManager _settings;

        public string ID => $"{_id}";

        public string Name => _settings?.Name;

        public PokemonRaidBotHost(ISettingsManager settings)
        {
            _settings = settings;
        }

        private bool _isRunning = false;
        public bool IsRunning => _isRunning;

        public IKernel Kernel { get; private set; }

        public IServiceCollection ServiceCollection { get; internal set; }

        private CancellationTokenSource source;

        public void Start()
        {
            if (_isRunning) throw new System.InvalidOperationException("Already running");
            _isRunning = true;

            TimeUtils.Initialize(_settings.Timezones);
            Kernel = new StandardKernel();
            Kernel.Bind<ILoggerFactory>().To<ConsoleLoggerFactory>();
            Kernel.Bind<ISettingsManager>().ToConstant(_settings);
            ICatalog catalog = new Catalog("raidbot", "i18n", new CultureInfo(string.IsNullOrEmpty(_settings.Language) ? "en-US" : _settings.Language));
            Kernel.Bind<ICatalog>().ToConstant(catalog);
            Kernel.Bind<ITimeService>().To<TimeService>();
            var database = Kernel.Get<Database>();
            database.Setup(_settings.DataFolder);
            Kernel.Bind<IDatabase>().ToConstant(database);
            Kernel.Bind<IPrivateConversationManager>().To<PrivateConversationManager>().InSingletonScope();

            var serviceProvider = ServiceCollection.BuildServiceProvider();
            Kernel.Bind<IServiceProvider>().ToConstant(serviceProvider);



            // Google location API
            var googleLocationAPIService = Kernel.Get<GoogleAddressService>();
            googleLocationAPIService.SetApiKey(_settings.GoogleLocationAPIKey);
            Kernel.Bind<ILocationToAddressService>().ToConstant(googleLocationAPIService);

            // Set up the messaging client
            source = new CancellationTokenSource();
            TelegramClient client = Kernel.Get<ThrottlingTelegramClient>();
            client.Setup(_settings.BotKey, source.Token);
            Kernel.Bind<IMessagingClient>().ToConstant(client);

            // Set up the components
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.RaidCreationWizard>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.RaidEditor>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.RaidEventHandler>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.CreateRaidsFromPogoAfo>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.SummarizeActiveRaids>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.UpdatePublishedRaidsInChannels>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.Modules.UpdatePublishedRaidsInPrimaryChannel>().InSingletonScope();

            Kernel.Bind<IBotModule>().To<PokemonRaidBot.ChatCommands.WhoAmI>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.ChatCommands.WhereAmI>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.ChatCommands.RaidStatistics>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.ChatCommands.Alias>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.ChatCommands.Level>().InSingletonScope();
            Kernel.Bind<IBotModule>().To<PokemonRaidBot.ChatCommands.Register>().InSingletonScope();

            var modules = Kernel.GetAll<IBotModule>().ToList();

            // Start the system
            modules.ForEach(m => m.Startup());
            client.Start();
        }

        public void Stop()
        {
            if (!_isRunning) throw new System.InvalidOperationException("Not running");
            source.Cancel();

            var modules = Kernel.GetAll<IBotModule>().ToList();
            modules.ForEach(m => m.Shutdown());

            Kernel.Dispose();
            Kernel = null;

            _isRunning = false;
        }
    }
}
