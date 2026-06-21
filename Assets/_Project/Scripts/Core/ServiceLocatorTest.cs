using UnityEngine;

public class ServiceLocatorTest : MonoBehaviour, IService
{
    private void Start()
    {
        Debug.Log("ServiceLocatorTest starting...");
        
        // Register this service
        ServiceLocator.Register<ServiceLocatorTest>(this);
        
        // Retrieve the service
        var retrievedService = ServiceLocator.Get<ServiceLocatorTest>();
        if (retrievedService != null)
        {
            Debug.Log("Successfully retrieved ServiceLocatorTest via ServiceLocator!");
        }
        
        // Test unregistration
        ServiceLocator.Unregister<ServiceLocatorTest>();
        
        // Verify it's unregistered
        if (!ServiceLocator.HasService<ServiceLocatorTest>())
        {
            Debug.Log("ServiceLocatorTest successfully unregistered.");
        }
    }
}