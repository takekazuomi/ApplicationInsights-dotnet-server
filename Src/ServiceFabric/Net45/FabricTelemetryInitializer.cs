using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Microsoft.ApplicationInsights.ServiceFabric
{
    public class FabricTelemetryInitializer : ITelemetryInitializer
    {
        private const string ServiceContextKeyName = "AI.SF.ServiceContext";

        private ServiceContext serviceContext;

        private ServiceContext ApplicableServiceContext
        {
            get
            {
                if (this.serviceContext != null)
                {
                    return this.serviceContext;
                }
                else
                {
                    return CallContext.LogicalGetData(ServiceContextKeyName) as ServiceContext;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FabricTelemetryInitializer(ServiceContext context)
        {
            this.serviceContext = context;
        }

        public FabricTelemetryInitializer()
        {}

        public static void SetServiceCallContext(ServiceContext context)
        {
            // The call initializes TelemetryConfiguration that will create and Intialize modules
            TelemetryConfiguration configuration = TelemetryConfiguration.Active;

            CallContext.LogicalSetData(ServiceContextKeyName, context);
        }

        public void Initialize(ITelemetry telemetry)
        {
            try
            {
                if (this.ApplicableServiceContext != null)
                {
                    telemetry.Context.Properties.Add("Cx.SF.ServiceTypeName", this.ApplicableServiceContext.ServiceTypeName);
                    telemetry.Context.Properties.Add("Cx.SF.PartitionId", this.ApplicableServiceContext.PartitionId.ToString());
                    telemetry.Context.Properties.Add("Cx.SF.ApplicationName", this.ApplicableServiceContext.CodePackageActivationContext.ApplicationName);
                    telemetry.Context.Properties.Add("Cx.SF.ApplicationTypeName", this.ApplicableServiceContext.CodePackageActivationContext.ApplicationTypeName);
                    telemetry.Context.Properties.Add("Cx.SF.NodeName", this.ApplicableServiceContext.NodeContext.NodeName);

                    if (this.ApplicableServiceContext is StatelessServiceContext)
                    {
                        telemetry.Context.Properties.Add("Cx.SF.InstanceId", this.ApplicableServiceContext.ReplicaOrInstanceId.ToString());
                    }

                    if (this.ApplicableServiceContext is StatefulServiceContext)
                    {
                        telemetry.Context.Properties.Add("Cx.SF.ReplicaId", this.ApplicableServiceContext.ReplicaOrInstanceId.ToString());
                    }

                    if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
                    {
                        telemetry.Context.Cloud.RoleName = this.ApplicableServiceContext.ServiceName.ToString();
                    }
                    if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleInstance))
                    {
                        telemetry.Context.Cloud.RoleInstance = this.ApplicableServiceContext.ReplicaOrInstanceId.ToString();
                    }
                }
            }
            catch
            {
                // Something went wrong trying to set these extra properties. We shouldn't fail though.
            }
        }
    }
}

