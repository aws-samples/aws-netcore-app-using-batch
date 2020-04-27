using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System.Collections.Generic;

namespace MyApp.Modules
{
    public sealed class BatchServiceRole: Construct
    {
        Construct scope = null;
        string id = "";

        public Role Role {get;set;}
        public BatchServiceRole(Construct scope, string id): base(scope, id)
        {
           this.scope = scope;
           this.id = id;

            this.Role =  GetRole(
                this, id+"-role",
                Utilities.ServiceBuilder.GetBatchServiceRoleManagedPolicyARNs(),
                new string[]{
                    Constants.BATCH_ROLE_SERVICE
                }, 
                Constants.BATCH_SERVICE_ROLE_NAME
            );
        }

        public Role GetRole(Construct scope, string roleId, 
                string[] ManagedPolicyArns, 
                string[] PrincipalServices,
                string PolicyName){


            var roleProps =  new RoleProps{
                Path = "/",
                AssumedBy = new ServicePrincipal(PrincipalServices[0])
            };

            if(PrincipalServices.Length > 0){
                List<PrincipalBase> principalBases = new List<PrincipalBase>();
                foreach(string service in PrincipalServices){
                    PrincipalBase principalBase = new ServicePrincipal(service);
                    principalBases.Add(principalBase);
                }
                var compositePrincipal = new CompositePrincipal(principalBases.ToArray());
                roleProps =  new RoleProps{
                    Path = "/",
                    AssumedBy = compositePrincipal
                };
            }

            var iamRole = new Role(scope, roleId, roleProps);

            foreach(string arn in ManagedPolicyArns){
                iamRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName(arn));
            }
                 
            return iamRole;
        }
    }
}