using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

/// <summary>
/// Контроллер команд.
/// </summary>
[ApiController]
[Route("/api/[controller]")]
public class TeamsController : ControllerBase
{
  /// <summary>
  /// Маппер.
  /// </summary>
  private readonly IMapper mapper;
  
  /// <summary>
  /// Сервис команд.
  /// </summary>
  private readonly TeamService teamService;
  
  /// <summary>
  /// Получить все команды.
  /// </summary>
  /// <returns>Список команд.</returns>
  [HttpGet]
  public async Task<ActionResult<List<TeamDTO>>> GetAllTeams()
  {
    List<Team> teams = await this.teamService.GetAllTeams();
    List<TeamDTO> response = this.mapper.Map<List<TeamDTO>>(teams);
    return this.Ok(response);
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="mapper">Маппер.</param>
  /// <param name="teamService">Сервис команд.</param>
  public TeamsController(IMapper mapper, TeamService teamService)
  {
    this.mapper = mapper;
    this.teamService = teamService;
  }
}