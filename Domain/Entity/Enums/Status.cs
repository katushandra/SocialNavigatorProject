using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity.Enums
{
    /// <summary>
    /// Тип для статуса объекта
    /// </summary>
    public enum Status
    {
        Pending, 
        Approved,
        Rejected, 
        Archived
    }
}
