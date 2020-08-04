using Ninject;

namespace RaidBot.Backend.Bot
{
    public interface IPokemonRaidBotHost
    {
        string ID { get; }
        IKernel Kernel { get; }
        string Name { get; }
        bool IsRunning { get; }
        void Start();
        void Stop();
    }
}
