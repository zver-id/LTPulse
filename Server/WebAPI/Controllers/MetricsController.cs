using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MetricsController(
  IMapper mapper,
  MetricsService metricsService,
  TeamService teamService,
  ILogger<MetricsController> logger)
  : ControllerBase
{
  /// <summary>
  /// Маппер.
  /// </summary>
  private readonly IMapper mapper = mapper;
  
  /// <summary>
  /// Сервис команд.
  /// </summary>
  private readonly TeamService teamService = teamService;
  
  /// <summary>
  /// Сервис метрик.
  /// </summary>
  private readonly MetricsService metricsService = metricsService;
  
  /// <summary>
  /// Логгер.
  /// </summary>
  private ILogger logger { get; } = logger;

  [HttpGet]
  public async Task<ActionResult<List<Dictionary<string, object>>>> Get(int teamId, int dayCount)
  {
    this.logger.LogInformation("Get team metrics");
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Team not found");
    DateTime beginDate = DateTime.Now - TimeSpan.FromDays(dayCount);
    List<Dictionary<string, object>> response = await this.metricsService.GetMetrics(
      x => (
      (x.Date > beginDate) &&
      (x.Team.Equals(team))));
    return this.Ok(response);
  }

  [HttpGet("filtered")]
  public async Task<ActionResult<List<Dictionary<string, object>>>> GetFiltered(int teamId, int dayCount,
    string filterSing)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Team not found");
    DateTime beginDate = DateTime.Now - TimeSpan.FromDays(dayCount);
    List<Dictionary<string, object>> response = await this.metricsService.GetMetrics(
      x => (
        (x.Date > beginDate) &&
        (x.Team.Equals(team)) &&
        (x.MetricType.MetricGroup.Name.ToLower().Equals(filterSing.ToLower()))));
    return this.Ok(response);
  }
}