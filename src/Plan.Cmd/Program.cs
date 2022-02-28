using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using RulesEngine;
using RulesEngine.Models;

namespace Plan.Cmd
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var wf = new Workflow
            {
                WorkflowName = "wf1",
                Rules = new Rule[]
                 {
                     new Rule
                     {
                         RuleName="wf1-r1",
                         Expression="input1.A > 100"
                     },
                     new Rule
                     {
                         
                     }
                 }
            };
            var ser = new ServiceCollection();
            var prov = ser.BuildServiceProvider();
            var lf = LoggerFactory.Create(x => x.AddConsole());

            var logger = lf.CreateLogger("Rules");
            var eng = new RulesEngine.RulesEngine(new Workflow[] {wf},logger);
            var res = eng.ExecuteAllRulesAsync("wf1", new Data { A = 123 }).GetAwaiter().GetResult();
            foreach (var item in res)
            {
                logger.LogInformation("{0} \tis \t{1}",item.Rule.RuleName,item.IsSuccess);
            }
        }
    }
    class Data
    {
        public int A { get; set; }
    }
}