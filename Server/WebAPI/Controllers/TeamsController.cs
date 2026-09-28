using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using Application.RabbitMQRequests;
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
  /// Поставщик сообщений в RabbitMQ.
  /// </summary>
  private readonly RabbitMQClient rabbitMqClient;
  
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
  /// Запустить пересчёт метрик для команды: публикует сообщение в очередь RabbitMQ.
  /// </summary>
  /// <param name="id">Идентификатор команды.</param>
  /// <returns>Ok, если сообщение отправлено.</returns>
  [HttpPost("{id:int}/recalculate")]
  public async Task<IActionResult> Recalculate(int id)
  {
    var team = await this.teamService.GetTeamById(id);
    if (team == null)
      return this.NotFound();

    await this.rabbitMqClient.SendMessage(new GenerateTeamReportRequest
    {
      TeamId = id,
      DaysAgo = 0
    });
    return this.Ok();
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="mapper">Маппер.</param>
  /// <param name="teamService">Сервис команд.</param>
  /// <param name="rabbitMqClient">Поставщик сообщений в RabbitMQ.</param>
  public TeamsController(IMapper mapper, TeamService teamService, RabbitMQClient rabbitMqClient)
  {
    this.mapper = mapper;
    this.teamService = teamService;
    this.rabbitMqClient = rabbitMqClient;
  }
}