using Nancy;
using Nancy.TinyIoc;

namespace TestingEnvironment.Orchestrator
{
    public class ConfigBootstrapper : DefaultNancyBootstrapper
    {
        private readonly OrchestratorConfiguration _configuration;

        public ConfigBootstrapper()
        {
        }

        public ConfigBootstrapper(OrchestratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(_configuration);
        }
    }
}
