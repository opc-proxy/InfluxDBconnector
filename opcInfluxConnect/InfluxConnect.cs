using System;
using System.Threading;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using Newtonsoft.Json.Linq;
using NLog;
using Opc.Ua;
using OpcProxyClient;
using OpcProxyCore;

namespace OpcInfluxConnect {
    public class InfluxConnect : IOPCconnect {
        serviceManager _serv;
        public static Logger logger = LogManager.GetCurrentClassLogger ();

        public InfluxDBClient client = null;
        public WriteApi writeApi;
        public InfluxConfigWrapper _conf;

        public void setServiceManager (serviceManager serv) {
            _serv = serv;
        }
        public void OnNotification (object sub, MonItemNotificationArgs items) {

            foreach (var itm in items.values) {
                if (DataValue.IsBad (itm)) continue;
                dynamic value = itm.Value;
                var point = PointData.Measurement(_conf.opcSystemName)
                    .Tag ("Name", items.name)
                    .Field ("value", value)
                    .Timestamp (itm.SourceTimestamp.ToUniversalTime (), WritePrecision.Ms);

                writeApi.WritePoint(point);

                logger.Debug("Write to influxDB value for {0} val {1}", items.name, itm.Value.ToString());
            }
        }
        public async void init (JObject config, CancellationTokenSource cts) {

            try {
                _conf = config.ToObject<InfluxConfigWrapper>();
                string Url = "http://"+_conf.influx.host + ":" + _conf.influx.port.ToString();
                string token = Environment.GetEnvironmentVariable("OPC_INFLUXDB_TOKEN") ?? "";

                var client_opt = new InfluxDBClientOptions.Builder()
                    .Url(Url)
                    .AuthenticateToken(token.ToCharArray())
                    .Bucket(_conf.influx.bucketName)
                    .Org(_conf.influx.organizationName)
                    .ReadWriteTimeOut(TimeSpan.FromMilliseconds(_conf.influx.timeoutMs))
                    .TimeOut(TimeSpan.FromMilliseconds(_conf.influx.timeoutMs))
                    .LogLevel(InfluxDB.Client.Core.LogLevel.None)
                    .Build();

                client = InfluxDBClientFactory.Create( client_opt );
                var health = await client.HealthAsync ();
                if (health.Status != HealthCheck.StatusEnum.Pass) throw new Exception ("Connection failed with host: " + Url);

                // query to test connection... is the only way I could find
                // Note: the following lines will throw in case
                var flux = $"from(bucket:\"{_conf.influx.bucketName}\") |> range(start: 0) |> limit(n: 1)";
                var fluxTables = await client.GetQueryApi().QueryAsync(flux);
                
                var write_opt = new WriteOptions.Builder()
                    .BatchSize(_conf.influx.batchSize)
                    .FlushInterval(_conf.influx.flushIntervalMs)
                    .RetryInterval(_conf.influx.retryIntervalMs)
                    .Build();
                writeApi = client.GetWriteApi(write_opt);

                logger.Info ("InfluxDB connection succesfully initialized");

            } catch (Exception e) {
                cts.Cancel ();
                logger.Fatal ("Problems in initializing connection to InfluxDB. Quiting...");
                logger.Error (e.Message);
            }
        }

        public void clean () {
            writeApi?.Flush();
            writeApi?.Dispose ();
            client?.Dispose ();
            logger.Info ("Influx DB connection closed");
        }

    }

    public class InfluxConfigWrapper{
        public InfluxConfig influx {get;set;}
        public string opcSystemName {get;set;}
        public InfluxConfigWrapper()
        {
            influx = new InfluxConfig();
            opcSystemName = "OPC";
        }
    }
    public class InfluxConfig {
        public string organizationName {get;set;}
        public string bucketName {get;set;}
        public string host {get;set;}
        public int port {get;set;}
        public double timeoutMs {get;set;}
        public int batchSize {get; set;}
        public int flushIntervalMs {get;set;}
        public int retryIntervalMs {get; set;}
        public InfluxConfig(){

            organizationName = "none";
            bucketName = "";
            host = "localhost";
            port = 9999;
            timeoutMs = 10000;
            batchSize = 1000;
            flushIntervalMs = 2000;
            retryIntervalMs = 1000;
        }
    }

}