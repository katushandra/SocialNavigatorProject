using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Geo
{
    /// <summary>
    ///  Шаблона оформления адреса
    /// </summary>
    public class AddressDetails
    {
        /// <summary>
        /// Город
        /// </summary>
        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>
        /// Поселок
        /// </summary>
        [JsonPropertyName("town")]
        public string? Town { get; set; }

        /// <summary>
        /// Деревня
        /// </summary>
        [JsonPropertyName("village")]
        public string? Village { get; set; }

        /// <summary>
        /// Хутор
        /// </summary>
        [JsonPropertyName("hamlet")]
        public string? Hamlet { get; set; }

        /// <summary>
        /// Пригород или район города
        /// </summary>
        [JsonPropertyName("suburb")]
        public string? Suburb { get; set; }

        /// <summary>
        /// Улица 
        /// </summary>
        [JsonPropertyName("road")]
        public string? Road { get; set; }

        /// <summary>
        /// Улица (альтернативное название)
        /// </summary>
        [JsonPropertyName("pedestrian")]
        public string? Pedestrian { get; set; }

        /// <summary>
        /// Улица (альтернативное название)
        /// </summary>
        [JsonPropertyName("street")]
        public string? Street { get; set; }

        /// <summary>
        /// Номер дома
        /// </summary>
        [JsonPropertyName("house_number")]
        public string? HouseNumber { get; set; }

        /// <summary>
        /// Название дома/организации в доме
        /// </summary>
        [JsonPropertyName("house_name")]
        public string? HouseName { get; set; }
    }
}