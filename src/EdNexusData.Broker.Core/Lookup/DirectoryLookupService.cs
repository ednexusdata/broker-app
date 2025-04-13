using DnsClient;
using EdNexusData.Broker.Core.Models;
using System.Web;
using System.Net.Http.Json;
using Ardalis.Result;

namespace EdNexusData.Broker.Core.Lookup;

public class DirectoryLookupService
{
    private readonly ILookupClient _lookupClient;
    private readonly Environment environment;
    private readonly HttpClient _httpClient;

    public DirectoryLookupService(
        ILookupClient lookupClient, 
        IHttpClientFactory httpClientFactory,
        Environment environment)
    {
        _lookupClient = lookupClient;
        this.environment = environment;
        _httpClient = httpClientFactory.CreateClient("default");
    }

    public async Task<District> SearchAsync(string searchDomain)
    {
        if (Uri.CheckHostName(searchDomain) == UriHostNameType.Unknown)
        {
            throw new ArgumentException("{0} is not a valid domain", searchDomain);
        }
        
        var txtresult = await ResolveBroker(searchDomain);
        _ = txtresult.Host ?? throw new NullReferenceException("Unable to get host from broker TXT record.");

        _httpClient.BaseAddress = new Uri($"https://{txtresult.Host}");
        var path = StripPathSlashes(txtresult.Path) + "/api/v1/directory/search?domain=" + HttpUtility.UrlEncode(searchDomain);
        var client = await _httpClient.GetAsync(path);

        if (client.IsSuccessStatusCode)
        {
            var result = await client.Content.ReadFromJsonAsync<District>();
            if (result is not null)
            {
                return result;
            }
        }
        else
        {
            throw new ArgumentException(await client.Content.ReadAsStringAsync());
        }
        
        return new District();
    }

    public async Task<BrokerDnsTxtRecord> ResolveBroker(string searchDomain)
    {
        var txtresult = new BrokerDnsTxtRecord();
        
        if (environment.IsNonProductionToLocalEnvironment())
        {
            var uri = environment.Addresses.FirstOrDefault(x => x.Scheme == "https");

            _ = uri ?? throw new ArgumentException("Unable to parse address to URI.");
            
            txtresult.Host = (uri.Host == "[::]") ? "localhost" : uri.Host;

            if (uri.Port != 443)
            {
                txtresult.Host = $"{txtresult.Host}:{uri.Port}";
            }

            txtresult.Version = "edubroker1";
            txtresult.Environment = environment.EnvironmentName.ToLower();
        }
        else if (environment.IsNonProductionEnvironment())
        {
            var dnsresult = await _lookupClient.QueryAsync(searchDomain, QueryType.TXT);

            var txtRecords = dnsresult.Answers.TxtRecords();
            
            if (txtRecords.Count() > 0)
            {
                var brokerTXTRecord = txtRecords.Where(x => x.Text.Contains("v=edubroker") && x.Text.Contains($"env={environment.EnvironmentName.ToLower()}"))?.FirstOrDefault();

                if (brokerTXTRecord is not null)
                {
                    txtresult = ParseBrokerTXTRecord(brokerTXTRecord.Text.First());
                }
            }
        }
        else
        {
            var dnsresult = await _lookupClient.QueryAsync(searchDomain, QueryType.TXT);

            var txtRecords = dnsresult.Answers.TxtRecords();
            
            if (txtRecords.Count() > 0)
            {
                var brokerTXTRecord = txtRecords.Where(x => x.Text.First().Contains("v=edubroker"))?.FirstOrDefault();

                if (brokerTXTRecord is not null)
                {
                    txtresult = ParseBrokerTXTRecord(brokerTXTRecord.Text.First());
                }
            }
        }

        return txtresult;
    }

    public BrokerDnsTxtRecord ParseBrokerTXTRecord(string txtRecord)
    {
        // v=edubroker1; a=broker.host.org; href=/broker; env=train

        string[] parts = txtRecord.Trim().Split(";");
        var values = new Dictionary<string, string>();

        foreach(var part in parts)
        {
            string[] val = part.Trim().Split("=");
            values.Add(val[0].Trim(), val[1].Trim());
        }

        return new BrokerDnsTxtRecord()
        {
            Version = values.TryGetValue("v", out var v) ? v : null,
            Host = values.TryGetValue("a", out var a) ? a : null,
            Path = values.TryGetValue("href", out var href) ? href : null,
            KeyAlgorithim = values.TryGetValue("k", out var k) ? k : null,
            PublicKey = values.TryGetValue("p", out var p) ? p : null,
            Environment = values.TryGetValue("env", out var env) ? env : null
        };
    }

    public string StripPathSlashes(string? input)
    {
        if (input is null)
        {
            return "";
        }

        var text = input;

        // Remove begining slash, if there is one
        if (input.Substring(0,1) == "/")
        {
            text = text.Substring(1, text.Length - 1);
        }

        // Remove ending slash, if there is one
        if (input.Substring(input.Length -1, -1) == "/")
        {
            text = text.Substring(input.Length -1, -1);
        }

        return "/" + text;
    }
}