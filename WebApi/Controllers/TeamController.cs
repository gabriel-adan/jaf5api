using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using Logic.Contracts;

namespace WebApi.Controllers
{
    public class TeamController : ApiBaseController
    {
        private readonly ITeamLogic teamLogic;

        public TeamController(ILogger<ApiBaseController> logger, ITeamLogic teamLogic) : base(logger)
        {
            this.teamLogic = teamLogic;
        }

        [HttpPost("RequestJoinToTeam")]
        [Authorize(Roles = "Player")]
        public IActionResult RequestJoinToTeam(PlayerJoinForm playerJoinForm)
        {
            try
            {
                if (teamLogic.RequestJoinToTeam(playerJoinForm.TurnId, playerJoinForm.PerfilId))
                    return Ok("Solicitud de unión enviada");
                else
                    return NotFound(new { Message = "No te pudiste unir al grupo" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "RequestJoinToTeam method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }

        [HttpPost("ResponseJoinToTeam")]
        [Authorize(Roles = "Player")]
        public IActionResult ResponseJoinToTeam(PlayerJoinForm playerJoinForm)
        {
            try
            {
                var turn = teamLogic.ResponseJoinToTeam(playerJoinForm.TurnId, playerJoinForm.PlayerId, playerJoinForm.IsAccepted);
                if (turn != null)
                    return Ok(string.Format("Tu grupo {0} acaba de completarse y tiene el turno confirmado para el día {1} a Hs {2} en {3}", turn.Name, turn.DateTime.ToString("dd/MM/yy"), turn.DateTime.ToString("HH:mm"), turn.Field));
                else
                    return Ok("Nuevo integrante sumado a tu grupo!");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ResponseJoinToTeam method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }
    }
}
