using System;
using Xunit;
using OpcInfluxConnect;
using OpcProxyCore;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class UnitTest1
    {
        serviceManager sm;
        InfluxConnect influx;
        public UnitTest1()
        {
            var json = JObject.Parse(@"
                {
                    'opcServerURL':'opc.tcp://localhost:4840/freeopcua/server/',
                    'loggerConfig' :{
                        'loglevel' :'debug'
                    },

                    'nodesLoader' : {
                        'targetIdentifier' : 'browseName',
                        'whiteList':['MyVariable']
                    },
                    'influx':{
                        'organizationName' : 'dsi',
                        'bucketName' : 'peppo4' 
                    }
                }
            ");

            Environment.SetEnvironmentVariable("OPC_INFLUX_TOKEN","6_slTQpvMu67fhqOxBIlS9WTJGkadjHvaYhgvls95POrtVA9m48yZuKNyr5NidnA8P9Fd4hRB6ZoCqLZqXd5GQ==");
            //sm = new serviceManager(new string[] {""});
            sm = new serviceManager(json);
            influx = new InfluxConnect();
            sm.addConnector(influx);
            Task.Run( () => sm.run() );
            Console.WriteLine("Warming up...");
            Thread.Sleep(1000);
        }
        [Fact]
        public void Test1()
        {
            Thread.Sleep(5000);
        }
    }
}
