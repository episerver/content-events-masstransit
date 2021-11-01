# Optimizely.CMS.MassTransit.Events

## Introduction

This provider allows for remote events processed.  By default this provider uses RabbitMQ as the default transport provider.  If you would like to use a different transport prvoider you will need to install the relevent nuget package and configure the transport in the last parameter of the AddMassTransitEventProvider extension method.  Please see the following link for more details https://masstransit-project.com/usage/transports/

## Configuration

To configure the mass transit event prvovider through configuration, you have a couple options.  
You can set the connection string OptimizelyMassTransitEvents or you can configure the connection string.



```
{
    "ConnectionStrings": {
        "EPiServerDB": "Server=.;Database=databse.cms;User Id=user;Password=password;MultipleActiveResultSets=True",
        "OptimizelyMassTransitEvents": "amqp://guest:guest@localhost:5672"
    }
}
```

OR

`The Exchnage Name and Queue Name are optional and will be set the defaults which are show here.`

```
{
    "EPiServer" : {
        "MassTransitEventProvider" : {
            "ConnectionString": "amqp://guest:guest@localhost:5672",
            "ExchangeName" : "optimizely.fanout.siteevents",
            "QueueuName" : "optimizely.queues.siteevents"
        }
    }
}
```

Next you will need to configure the event provider in the startup file.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddCms()
        .AddMassTransitEventProvider(null, x => x.AddRabbitMqTransport())
        .AddCmsAspNetIdentity<ApplicationUser>();
}
```
