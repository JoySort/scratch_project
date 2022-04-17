using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Network;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;


namespace CMDTest.UPDUpperRunner;

public class UDPTransportPerformanceTest
{
    private Logger? logger;
    private readonly int testCycle = 3 * 1;
    private string? porjectJsonString;
    private Project project;
    private UdpClient udpClient;

    public UDPTransportPerformanceTest()
    {
        LogManager.LoadConfiguration("../../../../UpperRunner/nlog.config");
        ConfigUtil.setConfigFolder("../../../../LibUnitTest/config");
        logger = LogManager.GetCurrentClassLogger();
    }
    
   
    public void Main()
    {
        ConsolidateWorker.getInstance().OnResult+=((sender, args) => SortingWorker.getInstance().processBulk(args.Results));
        SortingWorker.getInstance().OnResult+=((sender, args) => LBWorker.getInstance().processBulk(args.Results));
        LBWorker.getInstance().OnResult+=((sender, args) => EmitWorker.getInstance().processBulk(args.Results));
        EmitWorker.getInstance().OnResult+=((sender, args) => LowerMachineWorker.getInstance().processBulk(args.Results));
        LowerMachineWorker.init();
        
        try{
            string project_file_path = @"../../../../LibUnitTest/fixtures/project_apple_rec_start.json";
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,project_file_path);
            porjectJsonString = File.ReadAllText(path);
            ProjectParser parser = new ProjectParser(porjectJsonString);
            project = parser.getProject();
        }catch(Exception e){
         logger.Error(e.Message);   
        }

        logger.Info("APPLE Test begin");
        ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.start);
    }

  
    public void udpToReciveData()
    {
        var bindAddress = IPAddress.Any;
        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(bindAddress, 5113));
        var from = new IPEndPoint(0, 0);
       // Task.Run(() =>
       // {
            while (true)
            {
                var recvBuffer = udpClient.Receive(ref from);

                var msg = Encoding.UTF8.GetString(recvBuffer);
                var results = JsonConvert.DeserializeObject<List<RecResult>>(msg);

                var fromIP = from.Address.ToString();
                var fromPort = int.Parse(from.Port.ToString());

                var start_time = DateTime.Now.Millisecond;
                appleConsolidationTest(results.ToArray());
                
                var responseMsg = new Dictionary<string, long>();
                responseMsg["count"]=results.Count;
                responseMsg["timeTook"]= DateTime.Now.Millisecond-start_time;
                
                sendResponds(fromIP, fromPort, responseMsg);
           }
       // });
    }
    
    
    public void appleConsolidationTest(RecResult[] recResults )
    {   
        ConsolidateWorker worker = ConsolidateWorker.getInstance();
        
        worker.processBulk(new List<RecResult>(recResults));

       // ProjectEventDispatcher.getInstance().dispatchProjectStatusStartEvent(project,ProjectState.stop);
    }
    
    private void sendResponds(string fromIP, int fromPort, Dictionary<string,long> respondDiscoverMsg)
    {
       
        var msg = JsonConvert.SerializeObject(respondDiscoverMsg);
        var data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length, fromIP, fromPort);
    }
}