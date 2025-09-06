using DevConference.Shared.Models.Dtos;
using DevConferenceAPI.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevConferenceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferencesController : ControllerBase
    {
        private readonly ConferencesManager _manager;

        public ConferencesController(ConferencesManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        [Route("GetAllConferences")]
       public async Task<IActionResult> GetAllConferences()
        {
           var response =  await _manager.GetAllConferences();
            return StatusCode(response.StatusCode, response.Data);
        }

        [HttpPost]
        [Route("CreateConference")]
        public async Task<IActionResult> CreateConference([FromBody] ConferencesDto model)
        {
            var response = await _manager.CreateConference(model);
            return StatusCode(response.StatusCode, response);
        }

    }
}
