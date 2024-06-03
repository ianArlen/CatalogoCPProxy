using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using CatalogoCPProxy.Helpers; // Importa el helper
using CatalogoCPProxy.Responses; // Asegúrate de importar el namespace correcto

namespace CatalogoCPProxy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly Regex _cpRegex = new Regex(@"^\d{5}$"); // Regex para validar que CP sea numérico de 5 dígitos

        public ProxyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("CatalogoCP")]
        public async Task<IActionResult> GetCatalogoCP([FromQuery] string CP)
        {
            if (!_cpRegex.IsMatch(CP))
            {
                var errorResponse = new Response
                {
                    StatusCode = 400,
                    Message = "El parámetro CP debe ser una cadena numérica de 5 dígitos."
                };
                return BadRequest(errorResponse);
            }

            try
            {
                var response = await _httpClient.GetAsync($"https://thona-api-desarrollo.azurewebsites.net/api/CatalogoCP?CP={CP}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = new Response
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = response.ReasonPhrase
                    };
                    return StatusCode(errorResponse.StatusCode, errorResponse);
                }

                var xmlString = await response.Content.ReadAsStringAsync();
                var xml = XElement.Parse(xmlString);

                var json = JsonConvert.SerializeXNode(xml);
                var lowerCaseJson = JsonHelper.ConvertJsonKeysToLowercase(JToken.Parse(json));

                // Verifica si el JSON contiene datos válidos
                var data = lowerCaseJson["data"];
                if (data["colonias"] == null || data["colonias"]["colonia"] == null || !data["colonias"]["colonia"].HasValues)
                {
                    var errorResponse = new Response
                    {
                        StatusCode = 404,
                        Message = "No se encontraron colonias para el código postal proporcionado."
                    };
                    return NotFound(errorResponse);
                }

                return Content(lowerCaseJson.ToString(Formatting.None), "application/json");
            }
            catch (Exception ex)
            {
                var errorResponse = new Response
                {
                    StatusCode = 500,
                    Message = "Internal Server Error: " + ex.Message
                };
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }
    }
}
