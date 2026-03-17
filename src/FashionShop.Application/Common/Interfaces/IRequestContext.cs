using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.Interfaces
{

    public interface IRequestContext
    {
        Guid? UserId { get; set; }
        string SessionId { get; set; }
        bool IsAuthenticated { get; }
    }
    
}
