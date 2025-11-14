using RazorLight;
using System.Text.Json;

namespace MyPortal.Application.Utilities;

public class TemplateRenderer
{
    private readonly RazorLightEngine _engine;
    
    public TemplateRenderer()
    {
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(TemplateRenderer))
            .UseMemoryCachingProvider()
            .Build();
    }
    
    public async Task<string> RenderAsync(string templateContent, object model)
    {
        try
        {
            // Generate a unique key for this template
            var templateKey = Guid.NewGuid().ToString();
            
            // Compile and render the template
            var result = await _engine.CompileRenderStringAsync(templateKey, templateContent, model);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error rendering template: {ex.Message}", ex);
        }
    }
    
    public static dynamic BuildOfferEmailModel(
        string offersJson,
        string note,
        string firstname,
        string networkLogo,
        string networkUrl,
        string networkName,
        string networkSignupUrl,
        string address)
    {
        // Parse offers JSON
        var offersList = JsonSerializer.Deserialize<List<dynamic>>(offersJson) ?? new List<dynamic>();
        
        return new
        {
            offers = offersList,
            note = note,
            firstname = firstname,
            networklogo = networkLogo,
            networkurl = networkUrl,
            networkname = networkName,
            networksignupurl = networkSignupUrl,
            address = address
        };
    }
}
