using Microsoft.AspNetCore.Mvc;

namespace FileHostingApi.Controllers; 

[ApiController]
[Route("files")]
public class FilesController : Controller {
    
    [HttpGet("{fileId}")]
    public async Task<IActionResult> Get(string fileId, [FromQuery] string? name = null) {
        if (Program.StorageService == null) throw new Exception();
        name ??= fileId;
        Stream? file = await Program.StorageService.GetFile(fileId);
        if (file == null) {
            return NotFound();
        }
        
        // Check user agent
        string? userAgent = Request.Headers["User-Agent"].ToString();
        Console.WriteLine("User-Agent: " + userAgent);
        
        // Check IP
        string? ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine("IP: " + ip);
        
        // URLencode name
        name = Uri.EscapeDataString(name);

        bool isWhitelistedIpPrefix = Program.WhitelistedIPPrefixs.Any(prefix => ip?.StartsWith(prefix.ToString()) ?? false);
        if (!Program.WhitelistedUserAgents.Contains(userAgent) && !isWhitelistedIpPrefix) {
            string path = Path.Combine(Program.Config!["local_store_path"], "bad.png");
            return File(System.IO.File.Open(path, FileMode.Open), "application/octet-stream");
        }
        
        Response.Headers.ContentDisposition = $"attachment; filename=\"{name}\"";
        return File(file, "application/octet-stream");
    }
    
}