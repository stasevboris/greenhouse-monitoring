// GreenhouseMonitoring/Services/IHomeService.cs
using GreenhouseMonitoring.ViewModels;

namespace GreenhouseMonitoring.Services;

public interface IHomeService
{
    HomeViewModel GetHomeViewModel(); 
}