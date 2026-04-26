using System;
using System.Linq;
using System.Threading.Tasks;
using Application.ReportGeneration;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class FileReportController(IRepository repository, IMapper mapper) : GenericController(repository, mapper)
{
  [HttpGet]
  public async Task<ActionResult> GenerateReport(int teamId)
  {
    var reportWriter = new ReportGenerator(this.Repository);
    var team = await this.Repository.GetFirstAsync<Team>(t => t.Id == teamId);
    try
    {
      var reportBytes = await reportWriter.GenerateForTeam(team);
      //System.IO.File.WriteAllBytes(@"C:\Temp\Report.csv", reportBytes);
      return this.File(reportBytes, 
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        "report.xlsx");
    }
    catch (Exception e)
    {
      return this.Problem();
    }
  }
}