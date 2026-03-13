using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace Infrastructure.Geo
{
    public class GeoService : IGeoService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<GeoService> logger;
        private const string nomUrl = "https://nominatim.openstreetmap.org/";

        public GeoService(HttpClient httpClient, ILogger<GeoService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;

            // требование Nominatim
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SocialNavigator/1.0");
            httpClient.Timeout = TimeSpan.FromSeconds(10); // Таймаут для предотвращения зависаний
        }

        public async Task<Point?> AddressСoordinate(string address)
        {
            try
            {
                var url = $"{nomUrl}search?q={Uri.EscapeDataString(address)}&format=json&limit=1&addressdetails=1&layer=address"; // q= - закодированный адрес, limit=1 - только 1 рез, layer=address - только адреса

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<ResAddressCoordinate>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // Десериализация JSON в список

                if (results != null && results.Count > 0)
                {
                    var result = results[0];
                    if (result.Lat != null && result.Lon != null)
                    {
                        return new Point(
                            double.Parse(result.Lon, System.Globalization.CultureInfo.InvariantCulture),
                            double.Parse(result.Lat, System.Globalization.CultureInfo.InvariantCulture))
                        { SRID = 4326 };
                    }
                }

                logger.LogWarning("Не найден адрес {Address}", address);
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError("Ошибка при трансформации адреса в координаты: {Address}", address);
                return null;
            }
        }

        public async Task<string?> СoordinateAddress(double latitude, double longitude)
        {
            try
            {
                var url = $"{nomUrl}reverse?lat={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&format=json&addressdetails=1&zoom=18&layer=address";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResCoordinateAddress>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Address != null)
                {                  
                    return FormatAddress(result.Address);
                }

                return result?.DisplayName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при трансформации координат в адрес: {Latitude}, {Longitude}", latitude, longitude);
                return null;
            }           
        }

        private string? FormatAddress(AddressDetails address)
        {
            if (address == null) return null;

            var parts = new List<string>();

            // Город/населенный пункт
            if (!string.IsNullOrEmpty(address.City))
                parts.Add(address.City);
            else if (!string.IsNullOrEmpty(address.Town))
                parts.Add(address.Town);
            else if (!string.IsNullOrEmpty(address.Village))
                parts.Add(address.Village);
            else if (!string.IsNullOrEmpty(address.Hamlet))
                parts.Add(address.Hamlet);
            else if (!string.IsNullOrEmpty(address.Suburb))
                parts.Add(address.Suburb);

            // Улица
            if (!string.IsNullOrEmpty(address.Road))
                parts.Add(address.Road);
            else if (!string.IsNullOrEmpty(address.Pedestrian))
                parts.Add(address.Pedestrian);
            else if (!string.IsNullOrEmpty(address.Street))
                parts.Add(address.Street);

            // Номер дома 
            if (!string.IsNullOrEmpty(address.HouseNumber))
            {
                parts.Add(address.HouseNumber);
            }
            else if (!string.IsNullOrEmpty(address.HouseName))
            {
                parts.Add(address.HouseName);
            }

            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }
    }
}