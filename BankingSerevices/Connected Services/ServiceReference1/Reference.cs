﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BankingSerevices.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.myInterface")]
    public interface myInterface {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/myInterface/PiValue", ReplyAction="http://tempuri.org/myInterface/PiValueResponse")]
        double PiValue();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/myInterface/PiValue", ReplyAction="http://tempuri.org/myInterface/PiValueResponse")]
        System.Threading.Tasks.Task<double> PiValueAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/myInterface/absValue", ReplyAction="http://tempuri.org/myInterface/absValueResponse")]
        int absValue(int x);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/myInterface/absValue", ReplyAction="http://tempuri.org/myInterface/absValueResponse")]
        System.Threading.Tasks.Task<int> absValueAsync(int x);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface myInterfaceChannel : BankingSerevices.ServiceReference1.myInterface, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class myInterfaceClient : System.ServiceModel.ClientBase<BankingSerevices.ServiceReference1.myInterface>, BankingSerevices.ServiceReference1.myInterface {
        
        public myInterfaceClient() {
        }
        
        public myInterfaceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public myInterfaceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public myInterfaceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public myInterfaceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public double PiValue() {
            return base.Channel.PiValue();
        }
        
        public System.Threading.Tasks.Task<double> PiValueAsync() {
            return base.Channel.PiValueAsync();
        }
        
        public int absValue(int x) {
            return base.Channel.absValue(x);
        }
        
        public System.Threading.Tasks.Task<int> absValueAsync(int x) {
            return base.Channel.absValueAsync(x);
        }
    }
}