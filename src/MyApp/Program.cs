using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();
            new SrcStack(app, Constants.APP_NAME, new StackProps());
            app.Synth();
        }
    }
}
