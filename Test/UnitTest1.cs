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
                        'organizationName' : 'jo',
                        'bucketName' : 'prova' 
                    }
                }
            ");

            Environment.SetEnvironmentVariable("OPC_INFLUXDB_TOKEN","CORCd-2ILX_O9_ZybBKkVCgizxaRy_dI3B7uVIMYbmdy32XlHDXUK_D-zhws0lrVIV6GxNTSU_7ASJVYqWLdVg==");
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
