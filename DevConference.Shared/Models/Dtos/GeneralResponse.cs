using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevConference.Shared.Models.Dtos
{
    public class GeneralResponse
    {
        public int StatusCode { get; set; } // 200 - Success, 404 -Not Found
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}
