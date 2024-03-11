using System.Runtime.InteropServices;
using Nest;
using SearchApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();


app.MapGet("/search", async (string? city, int? rating) =>
{
    var host = "https://96cb61893fd34951b5018633338ab10a.europe-west3.gcp.cloud.es.io:443";
    var userName = "elastic";
    var password = "Pc5ci6F4AY0251c55ZIbqKi7";
    var indexName = "event";

    var connSettings = new ConnectionSettings(new Uri(host));
    connSettings.BasicAuthentication(userName, password);
    connSettings.DefaultIndex(indexName);
    connSettings.DefaultMappingFor<Hotel>(m => m.IdProperty(p => p.Id));

    var client = new ElasticClient(connSettings);

    if (rating is null)
    {
        rating = 1;
    }

    // Match
    // Prefix
    // Range
    // Fuzzy Match
    ISearchResponse<Hotel> result;

    if (city is null)
    {
        result = await client.SearchAsync<Hotel>(s => s.Query(q =>
            q.MatchAll() &&
            q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
            ));
    }
    else
    {
        result = await client.SearchAsync<Hotel>(s =>
            s.Query(q => q.Prefix(p => p.Field(f => f.CityName).Value(city).CaseInsensitive())
                         &&
                         q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
            )
        );
    }

    return result.Hits.Select(x => x.Source).ToList();
});

app.Run();