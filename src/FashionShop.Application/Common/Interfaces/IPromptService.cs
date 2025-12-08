using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IPromptService 
    {
        public Task<string> GetPromptAsync(string key);
    }
}
