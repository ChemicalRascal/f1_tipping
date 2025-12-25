using NuGet.Packaging;

namespace F1Tipping.Common;

public static class ConfigExtensions
{
    extension(IConfigurationBuilder config)
    {
        public IConfigurationBuilder AddDetectedCommandLine(string[] args)
        {
            var argProviders = System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface
                    && t.IsAssignableTo(typeof(IDefineCliArgs)));

            if (argProviders.Any())
            {
                var collectedArgs = new Dictionary<string, string>();
                collectedArgs.AddRange(argProviders.SelectMany(t =>
                {
                    var iMap = t.GetInterfaceMap(typeof(IDefineCliArgs));
                    var index = iMap.InterfaceMethods.IndexOf(
                        iMap.InterfaceMethods.First(m => m.Name == "get_Switches"));
                    if (index == -1)
                    {
                        return [];
                    }

                    var args = iMap.TargetMethods[index].Invoke(null, null);
                    if (args is null)
                    {
                        return [];
                    }

                    return (IEnumerable<KeyValuePair<string, string>>)args;
                }));

                config.AddCommandLine(args, collectedArgs);
            }

            return config;
        }
    }
}

public interface IDefineCliArgs
{
    static abstract IEnumerable<KeyValuePair<string, string>> Switches { get; }
}