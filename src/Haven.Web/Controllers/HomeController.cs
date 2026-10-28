using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Haven.Web.Models;

namespace Haven.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult MuaBan()
{
    ViewData["Title"] = "Mua bán bất động sản";
    return View();
}

public IActionResult ChoThue()
{
    return View();
}

public IActionResult DuAn()
{
    return View();
}

}
