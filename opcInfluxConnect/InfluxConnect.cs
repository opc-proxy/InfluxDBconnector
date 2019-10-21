using System;
using OpcProxyCore;
using Newtonsoft.Json.Linq;

using InfluxDB.Collector;
using OpcProxyClient;

using NLog;

namespace OpcInfluxConnect{
    class InfluxImpl : IOPCconnect {
        serviceManager _serv;
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public void setServiceManager(serviceManager serv){
            _serv = serv;
        }
        public void OnNotification(object sub, MonItemNotificationArgs items){

            foreach(var itm in items.values){
                Metrics.Collector.Measure(items.name,itm.Value );
                
                /* --- This does not work ---- FIXME! 

                Metrics.Collector.Write(
                    items.name, 
                    new Dictionary<string, object> { { "value", itm.Value } },
                    null,
                    DateTime.Now
                );
                */

                logger.Debug("Write to influxDB value for {0} to {1} at {2}", items.name, itm.Value, itm.SourceTimestamp);
            }
        }

        public void init(JObject config){
            
            try{
                Metrics.Collector = new CollectorConfiguration()
                    //.Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
                    .Batch.AtInterval(TimeSpan.FromSeconds(5))
                    .WriteTo.InfluxDB("http://localhost:8086", "opcProxyData")
                    .CreateCollector();
                logger.Debug("OpcInfluxConnect initialized");
            }
            catch(Exception e){
                logger.Error(e, "Problems in connecting to Influx DB");
            }
        }
        
    }

}