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
  public async Task<ActionResult<List<MetricDTO>>> Get(int teamId, int dayCount)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Team not found"); 
    List<Metric> metrics = await this.metricsService.GetMetrics(team, dayCount);
    List<MetricDTO> response = mapper.Map<List<MetricDTO>>(metrics);
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