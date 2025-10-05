using BepInEx.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace owd.EasyQuests
{
    public class ConfCts<T>
    {
        protected ConfigEntry<T> Cfg;
        T Get() { return Cfg.Value; }
        public void Set(T v) { Cfg.Value = v; }
        protected CancellationTokenSource Cts = new CancellationTokenSource();

        virtual public async Task Queue()
        {
            Cts?.Cancel();
            Cts = new CancellationTokenSource();
            var token = Cts.Token;

            try
            {
                PluginLogger.LogWarning("Queue attempt");

                while (HeroController.instance == null && !token.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);
                }

                if (token.IsCancellationRequested)
                    return;

                Func();
            }
            catch (TaskCanceledException)
            {
                PluginLogger.LogWarning("Func task canceled due to a newer request.");
            }
            catch (Exception ex)
            {
                PluginLogger.LogError($"Unexpected error in Queue: {ex}");
            }
        }

        virtual protected void Func() { }
        virtual protected void Bind_(ConfigFile configFile) {}

        public void Init(ConfigFile configFile)
        {
            Bind_(configFile);
            Cfg.SettingChanged += (_, __) => _ = Queue();
            _ = Queue();
        }
    }
}