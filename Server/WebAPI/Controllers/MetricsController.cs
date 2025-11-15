using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MetricsController : ControllerBase
{
  /// <summary>
  /// Маппер.
  /// </summary>
  private readonly IMapper mapper;
  
  /// <summary>
  /// Сервис команд.
  /// </summary>
  private readonly TeamService teamService;
  private readonly MetricsService metricsService;

  [HttpGet]
  public async Task<ActionResult<List<Dictionary<string, object>>>> Get(int teamId, int dayCount)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Team not found"); 
    List<Dictionary<string, object>> response = await this.metricsService.GetMetrics(team, dayCount);
    return this.Ok(response);
  }

  public MetricsController(IMapper mapper)
  {
    this.mapper = mapper;
    //TODO зарегистрироать все нормально
    this.metricsService = new MetricsService();
    this.teamService = new TeamService();
  }
}