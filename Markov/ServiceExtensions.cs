using Microsoft.Extensions.DependencyInjection;

namespace Markov
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMarkovChain<TKey, TValue>(
            this IServiceCollection service,
            IKeyGen<TKey, TValue> keygen,
            IStorage<TKey, TValue> storage,
            ITrainer<TKey, TValue> trainer,
            int order)
        {
            return service
                .AddMarkovKeyGen(keygen)
                .AddMarkovStorage(storage)
                .AddMarkovTrainer(trainer)
                .AddMarkovChain<TKey, TValue>(order);
        }

        public static IServiceCollection AddMarkovChain<TKey, TValue>(
            this IServiceCollection service,
            int order)
        {
            return service
                .AddSingleton(x => new Chain<TKey, TValue>(
                    x.GetService<IKeyGen<TKey, TValue>>(),
                    x.GetService<IStorage<TKey, TValue>>(),
                    x.GetService<ITrainer<TKey, TValue>>(),
                    order));
        }

        public static IServiceCollection AddMarkovKeyGen<TKey, TValue>(
            this IServiceCollection service,
            IKeyGen<TKey, TValue> keygen)
        {
            return service.AddSingleton(keygen);
        }

        public static IServiceCollection AddMarkovStorage<TKey, TValue>(
            this IServiceCollection service,
            IStorage<TKey, TValue> storage)
        {
            return service.AddSingleton(storage);
        }

        public static IServiceCollection AddMarkovTrainer<TKey, TValue>(
            this IServiceCollection service,
            ITrainer<TKey, TValue> trainer)
        {
            return service.AddSingleton(trainer);
        }
    }
}
