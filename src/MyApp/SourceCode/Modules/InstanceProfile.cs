using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System.Collections.Generic;

namespace MyApp.Modules
{
    public sealed class InstanceProfile: Construct
    {
        Construct scope = null;
        string id = "";

        public Role Role {get;set;}

        public CfnInstanceProfile CfnInstanceProfile {get;set;}
        public  InstanceProfile(Construct scope, string id, EcsInstanceRole batchInstanceRole): base(scope, id)
        {
           this.scope = scope;
           this.id = id;

            this.CfnInstanceProfile = new CfnInstanceProfile(this, id, new CfnInstanceProfileProps{
                InstanceProfileName = batchInstanceRole.Role.RoleName,
                Roles = new string[]{
                    batchInstanceRole.Role.RoleName
                }
            });
        }
    }
}