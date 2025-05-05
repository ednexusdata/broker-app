using System.Reflection;
using EdNexusData.Broker.Common.Attributes;
using EdNexusData.Broker.Common.Authentication;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Common.Mappings;
using EdNexusData.Broker.Common.Payloads;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core;

public class ConnectorLoader
{
    public List<Type> Connectors { get; private set; } = new List<Type>();
    public List<Type> Authenticators { get; private set; } = new List<Type>();
    public List<Type> Payloads { get; private set; } = new List<Type>();
    public List<Type> PayloadJobs { get; private set; } = new List<Type>();
    public List<Type> PayloadContentActions { get; private set; } = new List<Type>();

    public Dictionary<string, Type> Transformers { get; private set; } = new Dictionary<string, Type>();

    public Dictionary<Type, Type> Importers { get; private set; } = new Dictionary<Type, Type>();

    public Dictionary<string, Assembly> Assemblies { get; private set; } = new Dictionary<string, Assembly>();

    public Dictionary<string, string> ConnectorIndex { get; private set; } = new Dictionary<string, string>();
    public Dictionary<string, string> ConfigurationIndex { get; private set; } = new Dictionary<string, string>();
    public Dictionary<string, string> AuthenticationIndex { get; private set; } = new Dictionary<string, string>();
    

    public bool IsLoaded { get; set; } = false;

    private ILogger<ConnectorLoader> _logger;

    public ConnectorLoader()
    {
        _logger = LoggerFactory.Create(config =>
        {
            config.AddConsole();
        }).CreateLogger<ConnectorLoader>();
        
        if (IsLoaded == false)
        {
            LoadConnectorAssemblies();
            LoadConfigurations();
            LoadPayloads();
            LoadPayloadJobs();
            LoadPayloadContentActions();
            LoadTransfomers();
            LoadImporters();
            LoadAuthenticators();
        }
    }

    public Type? GetConnector(string connectorType)
    {
        return Connectors.Where(x => x.GetInterface(nameof(IConnector)) is not null && x.FullName == connectorType).FirstOrDefault();
    }

    public Type? GetAuthenticator(string authenticationType)
    {
        return Authenticators.Where(x => x.GetInterface(nameof(IAuthenticationProvider)) is not null && x.FullName == authenticationType).FirstOrDefault();
    }


    public List<Type>? GetConfigurations(Assembly assembly)
    {
        return assembly.GetExportedTypes().Where(x => x.GetInterface(nameof(IConfiguration)) is not null).ToList();
    }

    public List<Type>? GetPayloads(Assembly assembly)
    {
        return assembly.GetExportedTypes().Where(x => x.GetInterface(nameof(Payload)) is not null && x.IsAbstract == false).ToList();
    }

    public List<Type>? GetPayloadJobs()
    {
        return PayloadJobs;
    }

    public List<Type>? GetPayloadContentActions()
    {
        return PayloadContentActions;
    }

    private void LoadPayloads()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.IsSubclassOf(typeof(Payload)));

        foreach(var type in types)
        {
            if (type.IsAbstract == false && type.Assembly.GetName().Name == "EdNexusData.Broker.Common") 
            {
                Payloads.Add(type);
                
                _logger.LogInformation($"Payload loaded: {type.FullName} from {type.AssemblyQualifiedName}");
            }
        }
    }

    private bool VerifyOrCreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation("Directory created: " + path);
        }
        
        return true;
    }

    private void LoadConnectorAssemblies()
    {
        IsLoaded = true;

        var path = $"{AppDomain.CurrentDomain.BaseDirectory}connectors";

        VerifyOrCreateDirectory(path);
        var connectorAssemblyPaths = Directory.GetDirectories(path);
        if (connectorAssemblyPaths.Length == 0)
        {
            _logger.LogInformation($"No connectors loaded from paths: {connectorAssemblyPaths}");
            return;
        }

        foreach(var assemblyPath in connectorAssemblyPaths)
        {
            if (string.IsNullOrEmpty(assemblyPath)) return;

            var connectorAssemblyFiles = Directory.GetFiles(assemblyPath);

            foreach(var assemblyFilePath in connectorAssemblyFiles)
            {
                if (string.IsNullOrEmpty(assemblyFilePath)) return;

                var fileInfo = new FileInfo(assemblyFilePath);
                if (fileInfo.Extension == ".dll")
                {
                    // if (AppDomain.CurrentDomain.GetAssemblies().Where(x => String.Equals(x.Location, assemblyFilePath, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                    // {
                    //     Assembly.LoadFrom(assemblyFilePath);
                    // }
                    
                    try
                    {
                        Assembly.LoadFrom(assemblyFilePath);
                    }
                    catch (FileLoadException)
                    {

                    }
                }
            }
        }

        var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.GetInterface(nameof(IConnector)) is not null);

        if (types is not null) {
            foreach(var type in types)
            {
                if (type.GetInterface(nameof(IConnector)) != null)
                {
                    Connectors.Add(type);
                    Assemblies.Add(type.Assembly.GetName().Name!, type.Assembly);
                    ConnectorIndex.Add(type.FullName!, type.AssemblyQualifiedName!);
                    
                    _logger.LogInformation($"Connector loaded: {type.FullName} from {type.AssemblyQualifiedName}");
                }
            }
        }
        
    }

    private void LoadConfigurations()
    {
        foreach(var connector in Connectors)
        {
            var configurations = connector.Assembly.GetExportedTypes().Where(p => p.GetInterface(nameof(IConfiguration)) is not null);
            if (configurations.Count() > 0)
            {
                foreach(var config in configurations)
                {
                    ConfigurationIndex.Add(config.FullName!, connector.AssemblyQualifiedName!);
                }
            }
        }
    }

    private void LoadAuthenticators()
    {
        foreach(var connector in Connectors)
        {
            var authenticators = connector.Assembly.GetExportedTypes().Where(p => p.GetInterface(nameof(IAuthenticationProvider)) is not null);
            if (authenticators.Count() > 0)
            {
                foreach(var config in authenticators)
                {
                    Authenticators.Add(config);
                }
            }
        }
    }

    private void LoadPayloadJobs()
    {
        foreach(var connector in Connectors)
        {
            var payloadJobs = connector.Assembly.GetExportedTypes().Where(p => p.IsAssignableTo(typeof(PayloadJob)) && p.IsAbstract == false);
            if (payloadJobs.Count() > 0)
            {
                foreach(var payloadJob in payloadJobs)
                {
                    PayloadJobs.Add(payloadJob);
                
                    _logger.LogInformation($"IPayloadJob loaded: {payloadJob.FullName} from {payloadJob.AssemblyQualifiedName}");
                }
            }
        }
    }

    private void LoadPayloadContentActions()
    {
        foreach(var connector in Connectors)
        {
            var payloadContentActions = connector.Assembly.GetExportedTypes().Where(p => p.IsAssignableTo(typeof(PayloadContentActionJob)) && p.IsAbstract == false);
            if (payloadContentActions.Count() > 0)
            {
                foreach(var payloadContentAction in payloadContentActions)
                {
                    PayloadContentActions.Add(payloadContentAction);
                
                    _logger.LogInformation($"IPayloadContentAction loaded: {payloadContentAction.FullName} from {payloadContentAction.AssemblyQualifiedName}");
                }
            }
        }
    }

    private void LoadTransfomers()
    {
        foreach(var connector in Connectors)
        {
            var transformers = connector.Assembly.GetExportedTypes().Where(p => p.GetInterface("ITransformer`2") is not null && p.IsAbstract == false);
            if (transformers.Count() > 0)
            {
                foreach(var transformer in transformers)
                {
                    var mapsFroms = transformer.GetCustomAttributes(false).Where(x => x.GetType() == typeof(MapsFromAttribute)).ToList();
                    foreach(var mapsFrom in mapsFroms)
                    {
                        var convMapsFrom = (MapsFromAttribute)mapsFrom;
                        Transformers.Add($"{transformer.Assembly.GetName().Name}::{convMapsFrom.Schema}::{convMapsFrom.SchemaVersion}", transformer);

                        _logger.LogInformation($"Transformer loaded: {transformer.Assembly.GetName().Name}::{convMapsFrom.Schema}::{convMapsFrom.SchemaVersion} from {transformer.AssemblyQualifiedName}");
                    }
                    
                }
            }
        }
    }

    private void LoadImporters()
    {
        foreach(var connector in Connectors)
        {
            var importers = connector.Assembly.GetExportedTypes().Where(p => p.GetInterface("IImporter") is not null && p.IsAbstract == false);
            if (importers.Count() > 0)
            {
                foreach(var importer in importers)
                {
                    var imports = importer.GetCustomAttributes(false).Where(x => x.GetType() == typeof(ImportsAttribute)).ToList();
                    foreach(var import in imports)
                    {
                        var convImport = (ImportsAttribute)import;

                        foreach(var convImportType in convImport.Types)
                        {
                            Importers.Add(convImportType, importer);
                            
                            _logger.LogInformation($"Importer loaded: {convImportType.FullName} from {importer.AssemblyQualifiedName}");
                        }
                        
                    }
                    
                }
            }
        }
    }
}