using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Geo
{
    /// <summary>
    /// Результат поиска Координаты в адрес
    /// </summary>
    public class ResCoordinateAddress
    {
        /// <summary>
        /// Название места
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Детальная информация об адресе
        /// </summary>
        public AddressDetails? Address { get; set; }
    }
}