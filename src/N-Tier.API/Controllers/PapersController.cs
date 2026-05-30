using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace N_Tier.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Academic Papers Service")]
public class PapersController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PapersController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Fetch academic metadata from OpenAlex API
    /// </summary>
    [HttpGet("fetch-openalex")]
    public async Task<IActionResult> FetchFromOpenAlex([FromQuery] string keyword)
    {
        var client = _httpClientFactory.CreateClient();
        string url = $"https://api.openalex.org/works?search={Uri.EscapeDataString(keyword)}&mailto=phuocse@fpt.edu.vn";
        client.DefaultRequestHeaders.Add("User-Agent", "N-Tier-Academic-Trend-Analysis-App");

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, $"Error fetching data from OpenAlex. Details: {content}");
        }

        return Content(content, "application/json");
    }

    /// <summary>
    /// Fetch academic metadata from Semantic Scholar API
    /// </summary>
    [HttpGet("fetch-semantic-scholar")]
    public async Task<IActionResult> FetchFromSemanticScholar([FromQuery] string keyword)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "N-Tier-Academic-Trend-Analysis-App");
        // Semantic scholar API yêu cầu 'fields' trong tham số để lấy nhiều thông tin hơn là id và tiêu đề
        string url = $"https://api.semanticscholar.org/graph/v1/paper/search?query={Uri.EscapeDataString(keyword)}&limit=10&fields=title,authors,abstract,venue";
        
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, $"Error fetching data from Semantic Scholar. Details: {content}");
        }

        return Content(content, "application/json");
    }

    /// <summary>
    /// Fetch academic metadata from Crossref API
    /// </summary>
    [HttpGet("fetch-crossref")]
    public async Task<IActionResult> FetchFromCrossref([FromQuery] string keyword)
    {
        var client = _httpClientFactory.CreateClient();
        string url = $"https://api.crossref.org/works?query={Uri.EscapeDataString(keyword)}&select=title,author,abstract,publisher";
        client.DefaultRequestHeaders.Add("User-Agent", "N-Tier-Academic-Trend-Analysis-App (mailto:phuocse@fpt.edu.vn)");

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, $"Error fetching data from Crossref. Details: {content}");
        }

        return Content(content, "application/json");
    }
}
