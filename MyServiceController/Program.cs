using MyServiceController.Container;

namespace MyServiceController
{

    public interface IService
    {
        void Serve();
    }

    public class ServiceA : IService
    {
        public void Serve()
        {
            Console.WriteLine("Serving A");
        }
    }

    public class ServiceB : IService
    {
        private readonly IService _service;

        public ServiceB(IService service)
        {
            _service = service;
        }
        
        public void Serve()
        {
            Console.WriteLine("Serving B");
            _service.Serve();
        }
    }
    
    public class Program
    {
        static void Main(string[] args)
        {
            var container = new SimpleIoCContainer();
            
            // 注冊服務
            container.AddSingleton<IService, ServiceA>();
            container.AddTransient<ServiceB, ServiceB>();
            
            // 解釋服務
            var serviceA = container.Resolve<IService>();
            serviceA.Serve();

            var serviceB = container.Resolve<ServiceB>();
            serviceB.Serve();
            
            var anotherServiceA = container.Resolve<IService>();
            Console.WriteLine(ReferenceEquals(serviceA, anotherServiceA)?"Same instance":"Different instance");
        }
    }
}