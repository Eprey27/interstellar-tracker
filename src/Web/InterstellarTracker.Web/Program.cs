using Microsoft.Extensions.DependencyInjection;
using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.Web;

// Build services
var services = new ServiceCollection();

// Register Application and Infrastructure layers
services.AddApplication();
services.AddInfrastructure();

var serviceProvider = services.BuildServiceProvider();

// Get repository from DI
var repository = serviceProvider.GetRequiredService<InterstellarTracker.Application.Common.Interfaces.ICelestialBodyRepository>();

// Create and run window
var window = new Window(repository);
window.Run();
