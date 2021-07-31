using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Logic.Contracts;
using WebApi.Models;

namespace WebApi.Controllers
{
    public class TurnController : ApiBaseController
    {
        private readonly ITurnLogic turnLogic;

        public TurnController(ILogger<ApiBaseController> logger, ITurnLogic turnLogic) : base (logger)
        {
            this.turnLogic = turnLogic;
        }

        [HttpPost("Request")]
        [Authorize(Roles = "Player")]
        public IActionResult _Request(RequestTurnForm form)
        {
            try
            {
                var turnResult = turnLogic.Request(form.Date, form.HourId, form.TeamId, form.PerfilId);
                return Ok(turnResult);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "_Request method");
                return NotFound(new { Message = "Ocurrió un error al solicitar turno" });
            }
        }

        [HttpPost("Reserve")]
        [Authorize(Roles = "Owner, Employee")]
        public IActionResult Reserve(TurnReserveForm form)
        {
            try
            {
                var turnResult = turnLogic.Reserve(form.Date, form.HourId, form.Name);
                return Ok(turnResult);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Reserve method");
                return NotFound(new { Message = "Ocurrió un error al reservar turno" });
            }
        }

        [HttpPost("Confirm")]
        [Authorize(Roles = "Owner, Employee")]
        public IActionResult Confirm(ConfirmTurnForm form)
        {
            try
            {
                turnLogic.Confirm(form.Id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Confirm method");
                return NotFound(new { Message = "Ocurrió un error al confirmar el turno" });
            }
        }

        [HttpPost("CreateTeamTurn")]
        [Authorize(Roles = "Player")]
        public IActionResult Create(CreateTurnForm form)
        {
            try
            {
                var turnResult = turnLogic.CreateTeamTurn(form.Date, form.HourId, form.Name, form.IsPrivate, form.PerfilId);
                return Ok(turnResult);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Create method");
                return NotFound(new { Message = "Ocurrió un error al crear el equipo" });
            }
        }

        [HttpPost("FindInBufferZone")]
        [Authorize(Roles = "Player")]
        public IActionResult FindInBufferZone(BufferForm form)
        {
            try
            {
                var turns = turnLogic.ListByBufferZone(form.Longitude, form.Latitude, form.Radius);
                return Ok(turns);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "FindInBufferZone method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }

        [HttpGet("GetList/{id}/{fromDate}/{toDate}")]
        [Authorize(Roles = "Owner, Employee")]
        public IActionResult GetList(int id, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var hourTurns = turnLogic.List(id, fromDate, toDate);
                return Ok(hourTurns);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetList method");
                return NotFound(new { Message = "Ocurrió un error al listar los turnos" });
            }
        }

        [HttpGet("Reserve/{campId}/{hourId}/{date}")]
        [Authorize(Roles = "Owner, Employee")]
        public IActionResult Reserve(int campId, int hourId, DateTime date)
        {
            try
            {
                var turns = turnLogic.GetReserveList(date, hourId, campId);
                return Ok(turns);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Reserve method");
                return NotFound(new { Message = "Ocurrió un error" });
            }
        }

        [HttpGet("Request/{campId}/{hourId}/{date}")]
        [Authorize(Roles = "Owner, Employee")]
        public IActionResult _Request(int campId, int hourId, DateTime date)
        {
            try
            {
                var turns = turnLogic.GetRequestList(date, hourId, campId);
                return Ok(turns);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "_Request method");
                return NotFound(new { Message = "Ocurrió un error" });
            }
        }
    }
}
