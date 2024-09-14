using EdNexusData.Broker.Domain;
using Ardalis.GuardClauses;
using DnsClient;
using Microsoft.Extensions.Logging;
using EdNexusData.Broker.Service.Models;
using System.Web;
using System.Net.Http.Json;

namespace EdNexusData.Broker.Service.Lookup;

public class DirectoryLookupService
{
    private readonly ILogger<DirectoryLookupService> _logger;
    
    private readonly ILookupClient _lookupClient;
    private readonly HttpClient _httpClient;

    public DirectoryLookupService(ILogger<DirectoryLookupService> logger, 
        ILookupClient lookupClient, 
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _lookupClient = lookupClient;
        _httpClient = httpClientFactory.CreateClient("default");
    }

    public async Task<District> SearchAsync(string searchDomain)
    {
        if (Uri.CheckHostName(searchDomain) == UriHostNameType.Unknown)
        {
            throw new ArgumentException("{0} is not a valid domain", searchDomain);
        }
        
        var txtresult = await ResolveBrokerUrl(searchDomain);

        // Get directory list
        Guard.Against.Null(txtresult.Host, "host", "Unable to get host from broker TXT record.");
        
        _httpClient.BaseAddress = new Uri($"https://{txtresult.Host}");
        var path = "/" + StripPathSlashes(txtresult.Path) + "/api/v1/directory/search?domain=" + HttpUtility.UrlEncode(searchDomain);
        var client = await _httpClient.GetAsync(path);

        var result = await client.Content.ReadFromJsonAsync<District>();
        if (result is not null)
        {
            return result;
        }

        return new District();
    }

    public async Task<BrokerDnsTxtRecord> ResolveBrokerUrl(string searchDomain)
    {
        var txtresult = new BrokerDnsTxtRecord();
        
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

        return txtresult;
    }

    public BrokerDnsTxtRecord ParseBrokerTXTRecord(string txtRecord)
    {
        // v=edubroker1; a=broker.host.org; href=/broker

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
            PublicKey = values.TryGetValue("p", out var p) ? p : null
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

        return text;
    }
}