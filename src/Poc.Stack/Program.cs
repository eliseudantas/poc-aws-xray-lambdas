using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poc.CDK
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new PocStack(app, "XrayPocStack", new StackProps { 
                Description = "This is the stack created to analyze how Xray works with Lambdas Function, APIs and SQS."
            });
            app.Synth();
        }
    }
}
