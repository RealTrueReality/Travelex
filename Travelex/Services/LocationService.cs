using Microsoft.Maui.Devices.Sensors;

namespace Travelex.Services;

public interface ILocationService
{
    Task<bool> CheckAndRequestLocationPermission();
}

public class LocationService : ILocationService
{
    public async Task<bool> CheckAndRequestLocationPermission()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking location permission: {ex}");
            return false;
        }
    }
}
