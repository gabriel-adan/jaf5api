using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WebApi.Models;
using Logic.Contracts;

namespace WebApi.Controllers
{
    public class CampController : ApiBaseController
    {
        private readonly ICampLogic campLogic;
        private readonly string imagesPath;
        private readonly int spatialReference;

        public CampController(ILogger<ApiBaseController> logger, ICampLogic campLogic, IConfiguration configuration) : base (logger)
        {
            this.campLogic = campLogic;
            imagesPath = configuration.GetValue<string>("CampImagesRoute");
            spatialReference = configuration.GetValue<int>("SpatialReference");
        }

        [HttpPost("FindInBufferZone")]
        [Authorize(Roles = "Player")]
        public IActionResult FindInBufferZone(BufferForm form)
        {
            try
            {
                var camps = campLogic.ListByBufferZone(form.Longitude, form.Latitude, form.Radius, spatialReference, imagesPath);
                return Ok(camps);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "FindInBufferZone method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }
    }
}
