using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Geo
{
    /// <summary>
    /// Результат поиска Адрес в координаты 
    /// </summary>
    public class ResAddressCoordinate
    {
        /// <summary>
        /// Широта
        /// </summary>
        public string? Lat { get; set; }

        /// <summary>
        /// Долгота
        /// </summary>
        public string? Lon { get; set; }
    }
}