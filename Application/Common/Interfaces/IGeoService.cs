using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IGeoService
    {
        /// <summary>
        /// Aдрес в координаты 
        /// </summary>
        Task<Point?> AddressСoordinate(string address);

        /// <summary>
        /// Координаты в адрес
        /// </summary>
        Task<string?> СoordinateAddress(double latitude, double longitude);
    }
}