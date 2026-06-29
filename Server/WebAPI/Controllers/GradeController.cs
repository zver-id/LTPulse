using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GradeController(IRepository repository, ILogger<GradeController> logger, IMapper mapper)
  : GenericController(repository, mapper)
{
  /// <summary>
  /// Логгер.
  /// </summary>
  private readonly ILogger<GradeController> logger = logger;

  [HttpGet]
  public async Task<ActionResult<Response<GradeDTO>>> GetGrades(int teamId, DateTime date, bool? onlyUnresearched, bool? onlyNegative)
  {
    var gradeService = new GradeService(this.Repository);
    var team = await this.Repository.GetById<Team>(teamId);
    List<Grade> grades;
    if (onlyNegative == true)
    {
      grades = await gradeService.GetNegativeGrades(team, onlyUnresearched == true);
    }
    else
    {
      grades = await gradeService.GetAllGrades(team, date, date - TimeSpan.FromDays(1));
    }
    return this.Ok(new Response<GradeDTO>
    {
      Data = this.Mapper.Map<List<GradeDTO>>(grades),
      Schema = typeof(GradeDTO).GetProperties()
        .ToDictionary(p => p.Name,
          p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name)
    });
  }
}